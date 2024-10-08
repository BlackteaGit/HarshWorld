﻿using System;
using System.Collections.Generic;
using HarmonyLib;
using CoOpSpRpG;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using WTFModLoader;
using WTFModLoader.Manager;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using System.Data.SQLite;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace HarshWorld
{
	public class HarshWorld_Patches : IWTFMod
	{

		public ModLoadPriority Priority => ModLoadPriority.Low;
		public void Initialize()
		{
			Harmony harmony = new Harmony("blacktea.harshworld");
			harmony.PatchAll();
		}

		[HarmonyPatch(typeof(WorldRev3), "update")]
		public class WorldRev3_update
		{

			[HarmonyPrefix]
			private static void Prefix(WorldRev3 __instance, List<ulong> ___flotillas)
			{
			
				if(Globals.initialized && PLAYER.currentGame != null && PLAYER.currentWorld != null && PLAYER.currentSession != null && PLAYER.avatar != null)
				{
				//HWCONFIG.InterruptionFrequency = 250000;
					if (Globals.Interruptions != null && Globals.Interruptions.Count() > 0 && HWCONFIG.InterruptionFrequency > 0)
					{ 
						if (PLAYER.currentSession != null && PLAYER.currentShip != null && PLAYER.currentShip.boostStage >= 1)
						{

							var signature = PLAYER.currentShip.signitureRadius * HWCONFIG.GlobalDifficulty;
							if (PLAYER.currentShip.ownershipHistory.Contains(3UL) && PLAYER.currentShip.ownershipHistory.Contains(8UL)) //more visibility for the ship supplied by friendly pirates faction
							{
								signature *= 4;
							}
							// check if ambush is already spawned nearby
							Boolean interrupted = false;
							foreach (InterruptionBasic interruptionbasic in Globals.Interruptions)
							{
								if (interruptionbasic.InterruptionId != null && interruptionbasic.Grid == PLAYER.currentShip.grid && Vector2.DistanceSquared(PLAYER.currentShip.position, interruptionbasic.Position) < 15000f * 15000f)
								{
									interrupted = true;
								}
							}

							// spawn random ambush on traveling with travel drive and a ship is within heat signature radius
							if (!interrupted)
							{
								var sessionships = PLAYER.currentSession.allShips.Keys.ToArray();
								for (int i = 0; i < sessionships.Length; i++)
								{
									var shipid = sessionships[i];
									if (PLAYER.currentShip.id != shipid && PLAYER.currentSession.allShips[shipid].cosm != null && PLAYER.currentSession.allShips[shipid].cosm.alive && Vector2.DistanceSquared(PLAYER.currentShip.position, PLAYER.currentSession.allShips[shipid].position) < ((signature + (PLAYER.currentSession.allShips[shipid].scanRange / 1000f)) * PLAYER.currentShip.tempVis * PLAYER.currentSession.allShips[shipid].tempView) * ((signature + (PLAYER.currentSession.allShips[shipid].scanRange / 1000f)) * PLAYER.currentShip.tempVis * PLAYER.currentSession.allShips[shipid].tempView)) //&& PLAYER.currentSession.allShips[shipid].faction != 2UL)
									{
										// random ambush, dependent on reputation
										var spotedfaction = PLAYER.currentSession.allShips[shipid].faction;
										var repmodifier = Globals.getAccumulatedReputation(spotedfaction) * 100;
										var bountymodifier = Globals.globalints[GlobalInt.Bounty] * 10;
										if (!interrupted && Squirrel3RNG.Next(1, Math.Max((int)((250000 + repmodifier - bountymodifier) / HWCONFIG.InterruptionFrequency), 2)) == 1 && PLAYER.currentShip.boostStage > 1 && PLAYER.currentShip.position.X < 89000f && PLAYER.currentShip.position.X > -89000f && PLAYER.currentShip.position.Y < 89000f && PLAYER.currentShip.position.Y > -89000f)
										{
											if((spotedfaction == 3U && PLAYER.currentGame.completedQuests.Contains("phase_1_end")) || spotedfaction == 4U)
											{ 
												HWSPAWNMANAGER.addInterruption(new Interruption(INTERRUPTION_BAG.GetRandomTemplate(spotedfaction), PLAYER.currentShip.position, PLAYER.currentShip.grid));
												interrupted = true;
												string faction = "unknown";
												if (Globals.globalfactions.ContainsKey(spotedfaction))
												{
													faction = Globals.globalfactions[spotedfaction].Item1;
												}
												SCREEN_MANAGER.widgetChat.AddMessage("Interdiction field detected. Your ship has been spotted by a " + faction + " faction ship. Your ship's signature was exposed while using travel drive. Your reputation with this faction is not high enough to avoid conflict.", MessageTarget.Ship);

											}
											else if(PLAYER.currentGame.completedQuests.Contains("phase_1_end"))
											{
												HWSPAWNMANAGER.addInterruption(new Interruption(INTERRUPTION_BAG.GetRandomTemplate(), PLAYER.currentShip.position, PLAYER.currentShip.grid));
												interrupted = true;
												string faction = "unknown";
												if (Globals.globalfactions.ContainsKey(spotedfaction))
												{
													faction = Globals.globalfactions[spotedfaction].Item1;
												}
												SCREEN_MANAGER.widgetChat.AddMessage("Interdiction field detected. Your ship has been spotted by a " + faction + " faction ship. Your ship's signature was exposed while using travel drive. Your reputation with this faction is not high enough to avoid conflict.", MessageTarget.Ship);

											}
										}

										// ambush if player has demanded goods in inventory
										//cache? Those checks are expensive should not run with every update or game will freeze!
										if (!interrupted && Squirrel3RNG.Next(1, Math.Max((int)(200000 / Math.Min(HWCONFIG.InterruptionFrequency, 2000)), 2)) == 1 && PLAYER.currentShip.position.X < 89000f && PLAYER.currentShip.position.X > -89000f && PLAYER.currentShip.position.Y < 89000f && PLAYER.currentShip.position.Y > -89000f)
										{
											var demand = Globals.demand;
											var offer = Globals.offer;
											foreach (var item in demand)
											{
												if (offer.ContainsKey(item.Key) || item.Value > 20)
												{
													if (offer.GetValueSafe(item.Key) / item.Value < 0.2)
													{
														var ship = PLAYER.currentShip;
														int num = 0;
														MicroCosm cosm = PROCESS_REGISTER.getCosm(ship);
														if (cosm.cargoBays != null && cosm.cargoBays.Count > 0)
														{
															for (int j = 0; j < cosm.cargoBays.Count; j++)
															{
																if (cosm.cargoBays[j].storage != null)
																{
																	num += cosm.cargoBays[j].storage.countItemByType(item.Key);
																}
															}
														}
														if (PLAYER.avatar.countItemOfType(item.Key) > 0 || num > 0)
														{
															String Tooltip;
															if (ITEMBAG.defaultTip.ContainsKey(item.Key))
															{
																Tooltip = ITEMBAG.defaultTip[item.Key].tip;
															}
															else
															{
																Tooltip = "cargo";
															}
															List<String> conversations = new List<String>
															{
																"Give me yer " + Tooltip + " or i will hurt ye!",
																"Now i'll take yer " + Tooltip + ".",
																"Ye pitful maggot got some " + Tooltip + " We'll ravish yer booty!",
																"Yer " + Tooltip + " will make me rich."
															};
															HWSPAWNMANAGER.addInterruption(new Interruption(INTERRUPTION_BAG.GetRandomTemplate(4UL), PLAYER.currentShip.position, PLAYER.currentShip.grid, conversations, true));
															interrupted = true;
															string faction = "unknown";
															if (Globals.globalfactions.ContainsKey(4UL))
															{
																faction = Globals.globalfactions[4UL].Item1;
															}
																SCREEN_MANAGER.widgetChat.AddMessage("Interdiction field detected. Your ship has been spotted by a " + faction + " faction ship. Your ship's signature was exposed while using travel drive. Your " + Tooltip + " cargo is being scanned.", MessageTarget.Ship);
															break;
														}
													}
												}
											}
										}
										

										// ambush if player has a stolen ship
										if (!interrupted && Squirrel3RNG.Next(1, Math.Max((int)(150000 / HWCONFIG.InterruptionFrequency), 2)) == 1 && PLAYER.currentShip.position.X < 89000f && PLAYER.currentShip.position.X > -89000f && PLAYER.currentShip.position.Y < 89000f && PLAYER.currentShip.position.Y > -89000f)
										{											
											var ship = PLAYER.currentShip;
											foreach (var owner in ship.ownershipHistory)
											{
												if (owner == 3UL || owner == 4UL)
												{
													if(PLAYER.currentSession.allShips[shipid].ownershipHistory.Contains(owner))
													{
														List<String> conversations = new List<String>
														{
														"Nice ship you have there, asshole.",
														"Did you think you can steal a ship and hide?",
														"Time to pay your bills for this ship.",
														"You ship stealin' piece of crap."
														};
														HWSPAWNMANAGER.addInterruption(new Interruption(INTERRUPTION_BAG.GetRandomTemplate(owner), PLAYER.currentShip.position, PLAYER.currentShip.grid, conversations, false));
														interrupted = true;
														string faction = "unknown";
														if (Globals.globalfactions.ContainsKey(owner))
														{
															faction = Globals.globalfactions[owner].Item1;
														}
														SCREEN_MANAGER.widgetChat.AddMessage("Interdiction field detected. Your ship has been spotted by a " + faction + " faction ship. Your ship's signature was exposed while using travel drive. Your ship's transponder signal resembles this faction transponder signature.", MessageTarget.Ship);
														break;
													}
												}

											}
										}

										//special condition for the ship supplied by friendly pirates faction
										if (!interrupted && Squirrel3RNG.Next(1, Math.Max((int)(100000 / HWCONFIG.InterruptionFrequency), 2)) == 1 && PLAYER.currentShip.position.X < 89000f && PLAYER.currentShip.position.X > -89000f && PLAYER.currentShip.position.Y < 89000f && PLAYER.currentShip.position.Y > -89000f)
										{
											if (PLAYER.currentShip.ownershipHistory.Contains(3UL) && PLAYER.currentShip.ownershipHistory.Contains(8UL))
											{
												if (PLAYER.currentSession.allShips[shipid].ownershipHistory.Contains(3UL) && PLAYER.currentSession.allShips[shipid].faction != 2UL)
												{
													List<String> conversations = new List<String>
														{
														"Nice ship you have there, asshole.",
														"Did you think you can steal a ship and hide?",
														"Time to pay your bills for this ship.",
														"You ship stealin' piece of crap."
														};
													HWSPAWNMANAGER.addInterruption(new Interruption(INTERRUPTION_BAG.GetRandomTemplate(3UL), PLAYER.currentShip.position, PLAYER.currentShip.grid, conversations, false));
													interrupted = true;
													string faction = "unknown";
													if (Globals.globalfactions.ContainsKey(3UL))
													{
														faction = Globals.globalfactions[3UL].Item1;
													}
													SCREEN_MANAGER.widgetChat.AddMessage("Interdiction field detected. Your ship has been spotted by a " + faction + " faction ship. Your ship's signature was exposed while using travel drive. Your ship's transponder signal resembles this faction transponder signature.", MessageTarget.Ship);

												}
											}
										}

									}
								}
							}

							// spawn random ambush on traveling with Higgs drive
							if (!interrupted && Squirrel3RNG.Next(1, Math.Max((int)(80000 / HWCONFIG.InterruptionFrequency), 2)) == 1  && PLAYER.currentShip.boostStage > 2 && PLAYER.currentShip.position.X < 89000f && PLAYER.currentShip.position.X > -89000f && PLAYER.currentShip.position.Y < 89000f && PLAYER.currentShip.position.Y > -89000f && (CHARACTER_DATA.hasResearchExclusion(400712U) || CHARACTER_DATA.hasModule(new Color(145, 135, 24))))
							{
								if(PLAYER.currentGame.completedQuests.Contains("phase_1_end"))
								{
									ulong faction = (ulong)Squirrel3RNG.Next(3,5);
									if(Globals.getAccumulatedReputation(faction) < 1000)
									{ 
										HWSPAWNMANAGER.addInterruption(new Interruption(INTERRUPTION_BAG.GetRandomTemplate(faction), PLAYER.currentShip.position, PLAYER.currentShip.grid));
										interrupted = true;
										string factionname = "unknown";
										if (Globals.globalfactions.ContainsKey(faction))
										{
											factionname = Globals.globalfactions[faction].Item1;
										}
										SCREEN_MANAGER.widgetChat.AddMessage("Interdiction field detected. Your ship has been spotted by a " + factionname + " faction ship. Your ship's signature was exposed while using Higg's drive. Your reputation with this faction is not high enough to avoid conflict.", MessageTarget.Ship);

									}
								}
								else
								{
									if (Globals.getAccumulatedReputation(4UL) < 1000)
									{
										HWSPAWNMANAGER.addInterruption(new Interruption(INTERRUPTION_BAG.GetRandomTemplate(4UL), PLAYER.currentShip.position, PLAYER.currentShip.grid));
										interrupted = true;
										string faction = "unknown";
										if (Globals.globalfactions.ContainsKey(4UL))
										{
											faction = Globals.globalfactions[4UL].Item1;
										}
										SCREEN_MANAGER.widgetChat.AddMessage("Interdiction field detected. Your ship has been spotted by a " + faction + " faction ship. Your ship's signature was exposed while using Higg's drive. Your reputation with this faction is not high enough to avoid conflict.", MessageTarget.Ship);

									}
								}
							}


						}

						if (!Globals.eventflags[GlobalFlag.Sige1EventActive] && Globals.globalints[GlobalInt.Bounty] > 0)
						{
							var bountymodifier = Globals.globalints[GlobalInt.Bounty];
							var repmodifier = 0;
							if(Squirrel3RNG.Next(bountymodifier) > 100)
							{ 
								var factions = Globals.globalfactions.Keys.ToArray();
								for (int i = 0; i < factions.Count(); i++)
								{
									repmodifier += Math.Min(0, Globals.getAccumulatedReputation(factions[i]));
								}
								repmodifier *= 100;
							}
							bountymodifier *= 100;
							// homebase siege1 pirate event
							if ( Squirrel3RNG.Next(1, Math.Max((int)(500000 + repmodifier - bountymodifier / HWCONFIG.InterruptionFrequency), 2)) == 1)
							{
								InterruptionType template = InterruptionType.home_siege_pirate_t1;
								var currentdifficulty = (int)Math.Round(MathHelper.Clamp(((float)CHARACTER_DATA.shipsUnlocked() * HWCONFIG.GlobalDifficulty), 1f, 100f));
								if (currentdifficulty <= 12)
								{
									switch (Squirrel3RNG.Next(3))
									{
										case 0:
											template = InterruptionType.home_siege_pirate_t1;
											break;
										case 1:
											template = InterruptionType.home_siege_pirate_t1;
											break;
										case 2:
											template = InterruptionType.home_siege_pirate_t1;
											break;
										default:
											template = InterruptionType.home_siege_pirate_t1;
											break;
									}
								}
								if (currentdifficulty > 12 && currentdifficulty <= 21)
								{
									switch (Squirrel3RNG.Next(4))
									{
										case 0:
											template = InterruptionType.home_siege_pirate_t2;
											break;
										case 1:
											template = InterruptionType.home_siege_pirate_t2;
											break;
										case 2:
											template = InterruptionType.home_siege_pirate_t2;
											break;
										case 3:
											template = InterruptionType.home_siege_pirate_t2;
											break;
										default:
											template = InterruptionType.home_siege_pirate_t2;
											break;
									}
								}
								if (currentdifficulty > 21)
								{
									switch (Squirrel3RNG.Next(8))
									{
										case 0:
											template = InterruptionType.home_siege_pirate_t25;
											break;
										case 1:
											template = InterruptionType.home_siege_pirate_t25;
											break;
										case 2:
											template = InterruptionType.home_siege_pirate_t25;
											break;
										case 3:
											template = InterruptionType.home_siege_pirate_t25;
											break;
										case 4:
											template = InterruptionType.home_siege_pirate_t25;
											break;
										case 5:
											template = InterruptionType.home_siege_pirate_t25;
											break;
										case 6:
											template = InterruptionType.home_siege_pirate_t25;
											break;
										case 7:
											template = InterruptionType.home_siege_pirate_t25;
											break;
										default:
											template = InterruptionType.home_siege_pirate_t25;
											break;
									}
								}
								var interruption = new Interruption(template, PLAYER.currentGame.position, CONFIG.spHomeGrid);
								interruption.interdictionSpot = PLAYER.currentGame.position;
								HWSPAWNMANAGER.addInterruption(interruption);
								Globals.eventflags[GlobalFlag.Sige1EventActive] = true;
								Globals.eventflags[GlobalFlag.Sige1EventSpawnDialogueActive] = true;
							}
						}
					}
				}
			
			}
		}

		[HarmonyPatch(typeof(BattleSessionSP), "localUpdate")]
		public class BattleSessionSP_localUpdate //main update loop for interruptions
		{

			[HarmonyPrefix]
			private static void Prefix(BattleSessionSP __instance, float elapsed)
			{
				if (Globals.initialized)
				{
					if (Globals.GlobalShipRemoveQueue != null && !Globals.GlobalShipRemoveQueue.IsEmpty)  // removing collected garbage ship ids (already set to despawn by the Interruption.DespawnEnqueqedShipAsync) from active sessions
					{
						List<Tuple<ulong, Point>> list = new List<Tuple<ulong, Point>>();
						while (Globals.GlobalShipRemoveQueue.TryDequeue(out var tuple))
						{
							if (__instance.grid == tuple.Item2)
							{
								if (__instance.allShips.TryGetValue(tuple.Item1, out var ship) && ship.removeThis)
								{
									__instance.allShips.Remove(tuple.Item1);
									//ship.Dispose();
								}
							}
							else
							{
								list.Add(tuple);
							}
						}
						for (int i = 0; i < list.Count(); i++)
						{
							Globals.GlobalShipRemoveQueue.Enqueue(list[i]);
						}
						list.Clear();
					}


					if (Globals.Interruptions != null)
					{
						for (int i = 0; i < Globals.Interruptions.Count(); i++)
						{
							var interruptionbasic = Globals.Interruptions[i];
							if (interruptionbasic.InterruptionId != null && interruptionbasic.Grid == __instance.grid)
							{
								Interruption interruption;
								if (Globals.Interruptionbag.TryGetValue(interruptionbasic.InterruptionId, out interruption))
								{
									interruption.Update(elapsed, __instance);
								}
							}
						}
					}

				}
			}
		}
		
		[HarmonyPatch(typeof(MicroCosm), "updateCrew")] //updatig floaty text of the Crew
		public class MicroCosm__updateCrew
		{

			[HarmonyPostfix]
			private static void Postfix(MicroCosm __instance, float elapsed, ref float ___shutdownTimer)
			{
				bool flag = true;
				checked
				{
					foreach (FloatyText animation in __instance.animations)
					{
						flag = false;
						animation.stepInteriorFloatyText(elapsed, __instance);
						if (!animation.visible)
						{
							__instance.animationTrash.Add(animation);
						}
					}

				}
				if (flag)
				{
					___shutdownTimer += elapsed;
					if (___shutdownTimer >= 5f)
					{
						__instance.alive = false;
					}
				}
				else
				{
					___shutdownTimer = 0f;
				}
			}
		}

		[HarmonyPatch(typeof(NonPlayerShip), "tryBuyBudget")] //counting every purchase attempt of to assets their market value
		public class NonPlayerShip__tryBuyBudget
		{

			[HarmonyPostfix]
			private static void Postfix(bool __result, EconomyNode node, InventoryItemType type)
			{
				if (__result)
				{
					if (!Globals.offer.ContainsKey(type)) //counting all successfull purchases for a trading good.
					{
						Globals.offer.Add(type, 1);
					}
					else
					{
						Globals.offer[type] = Globals.offer.GetValueSafe(type) + 1;
					}
				}
				else
				{
					if (!Globals.demand.ContainsKey(type)) //counting all failed purchases for a trading good.
					{
						Globals.demand.Add(type, 1);
					}
					else
					{
						Globals.demand[type] = Globals.demand.GetValueSafe(type) + 1;
					}
				}
			}
		}


		[HarmonyPatch(typeof(Ship), "updateCargo")]   //disabling the random cargo stealing from undamaged ships by activated scoop
		public class Ship_updateCargo
		{

			[HarmonyPrefix]
			private static Boolean Prefix(Ship __instance, float elapsed, BattleSession session, ref System.Collections.Concurrent.ConcurrentQueue<InventoryItem> ___threadCargo)
			{
				if (!___threadCargo.IsEmpty)
				{
					while (___threadCargo.TryDequeue(out InventoryItem inventoryItem))
					{
						bool flag = inventoryItem != null;
						if (flag)
						{
							CargoPod cargoPod = new CargoPod(inventoryItem, __instance.position);
							CargoPod cargoPod2 = cargoPod;
							cargoPod2.position.X = cargoPod2.position.X + ((float)(Squirrel3RNG.NextDouble() * 100.0) - 50f);
							CargoPod cargoPod3 = cargoPod;
							cargoPod3.position.Y = cargoPod3.position.Y + ((float)(Squirrel3RNG.NextDouble() * 100.0) - 50f);
							session.cargo.Add(cargoPod);
							session.cargoDetection(cargoPod.position, true);
						}
						__instance.scoopMode = false;
					}
				}
				bool flag2 = __instance.scoopMode && __instance.cosm != null && __instance.cosm.alive;
				checked
				{
					if (flag2)
					{
						for (int i = 0; i < session.cargo.Count; i++)
						{
							var cargoPod4 = session.cargo[i];
							float num = Vector2.DistanceSquared(cargoPod4.position, __instance.position);
							if (num < 60f * 60f)
							{
								for (int j = 0; j < cargoPod4.items.Count; j++)
								{
									var inventoryItem2 = cargoPod4.items[j];
									bool flag4 = __instance == PLAYER.currentShip && PLAYER.currentGame != null;
									if (flag4)
									{
										bool flag5 = inventoryItem2.type == InventoryItemType.exotic_matter;
										if (flag5)
										{
											CHARACTER_DATA.credits += unchecked((ulong)(checked(inventoryItem2.refineValue * inventoryItem2.stackSize)));
											__instance.floatyText.Enqueue("+" + SCREEN_MANAGER.formatCreditString(unchecked((ulong)(checked(inventoryItem2.refineValue * inventoryItem2.stackSize)))) + " credits");
										}
										else
										{
											bool flag6 = inventoryItem2.type == InventoryItemType.biomass;
											if (flag6)
											{
												uint num2 = 100U * inventoryItem2.stackSize;
												CHARACTER_DATA.exp += unchecked((ulong)num2);
												PLAYER.currentGame.team.grantExp(__instance,num2);
												__instance.floatyText.Enqueue("+" + SCREEN_MANAGER.formatCreditString(unchecked((ulong)(checked(100U * inventoryItem2.stackSize)))) + " exp");
											}
											else
											{
												bool flag7 = inventoryItem2.type == InventoryItemType.core_node;
												if (flag7)
												{
													CHARACTER_DATA.coreNodes += unchecked((ulong)inventoryItem2.stackSize);
													SCREEN_MANAGER.alerts.Enqueue("Core node gained");
													SCREEN_MANAGER.widgetChat.AddMessage("You gained a code node. You now have " + CHARACTER_DATA.coreNodes.ToString(), MessageTarget.Ship);
												}
												else if (inventoryItem2.type == InventoryItemType.gauntlet_scrap)
												{
													if (PLAYER.currentWorld != null && PLAYER.currentWorld.GetType() == typeof(WorldRev3))
													{
														WorldRev3 worldRev = PLAYER.currentWorld as WorldRev3;
														if (worldRev.gauntlet != null)
														{
															worldRev.gauntlet.scrap += inventoryItem2.stackSize;
															__instance.floatyText.Enqueue("+" + SCREEN_MANAGER.formatCreditString((ulong)inventoryItem2.stackSize) + " scrap");
														}
													}
												}
												else
												{
													__instance.cosm.threadDumpCargo(inventoryItem2);
													__instance.floatyText.Enqueue("+" + inventoryItem2.type.ToString());
												}
											}
										}
									}
									else
									{
										__instance.cosm.threadDumpCargo(inventoryItem2);
									}
								}
								session.cargo.Remove(cargoPod4);
								bool flag8 = session == PLAYER.currentSession;
								if (flag8)
								{
									SCREEN_MANAGER.GameSounds[42].play();
								}
								break;
							}
							else if (num < 250f * 250f)
							{
								float scaleFactor = 250f - (float)Math.Sqrt(num);
								Vector2 value = Vector2.Normalize(__instance.position - cargoPod4.position) * scaleFactor * elapsed;
								cargoPod4.position += value;
							}
						}
						for (int i = 0; i < PLAYER.currentSession.allShips.Values.Count(); i++)
						{
							var ship = PLAYER.currentSession.allShips.Values.ToList()[i];
							bool flag10 = ship.data != null && Vector2.DistanceSquared(ship.position, __instance.position) < 300f * 300f;
							if (flag10)
							{
								bool damaged = true;
								float num = ship.checkMassFast();
								if (num / ship.shipMetric.MaxMass >= 0.9f)
								{
									damaged = false;
								}
								if (damaged)
								{
									InventoryItem inventoryItem3 = ship.data.takeOneItem();
									bool flag11 = inventoryItem3 != null;
									if (flag11)
									{
										bool flag12 = Squirrel3RNG.Next(5) > 0;
										if (flag12)
										{
											CargoPod item = new CargoPod(inventoryItem3, ship.position);
											PLAYER.currentSession.cargo.Add(item);
										}
										break;
									}
								}
							}
						}

					}
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(Ship), "aggro")]
		public class Ship_aggro
		{

			[HarmonyPrefix]
			private static Boolean Prefix(Ship __instance, ulong threat, BattleSession session, ref List<ulong> ___aggroSkip)
			{
				bool flag = threat == __instance.faction || __instance == PLAYER.currentShip || __instance.consoles == null;
				if (!flag)
				{
					bool flag2 = __instance.boostStorage > 0f;
					if (flag2)
					{
						__instance.boostStorage = Math.Max(0f, __instance.boostStorage - __instance.forwardAccel * 2f * 1f);
					}
					bool flag3 = false;
					foreach (CoOpSpRpG.Console console in __instance.consoles.Values)
					{
						bool flag4 = console != null && console.crew != null;
						if (flag4)
						{
							flag3 = true;
							break;
						}
					}
					bool flag5 = flag3;
					if (flag5)
					{
						bool flag6 = true;
						bool flag7 = __instance.checkMassFast() < 3000UL;
						if (flag7)
						{
							flag6 = true;
						}
						else
						{
							bool flag8 = __instance.shipMetric != null;
							if (flag8)
							{
								float num = __instance.checkMassFast();
								bool flag9 = num / __instance.shipMetric.MaxMass >= 0.7f; // making docking ports open requiring more damage
								if (flag9)
								{
									flag6 = false;
								}
							}
						}
						bool flag10 = flag6 && !__instance.dockingActive && __instance != PLAYER.currentShip;
						if (flag10)
						{
							__instance.toggleDocking(true);
						}
					}
					else
					{
						bool flag11 = __instance != PLAYER.currentShip && session.GetType() != typeof(BattleSessionSC);
						if (flag11)
						{
							__instance.toggleDocking(true);
						}
					}
					bool flag12 = ___aggroSkip == null;
					if (flag12)
					{
						___aggroSkip = new List<ulong>();
					}
					if (!___aggroSkip.Contains(threat))
					{
						___aggroSkip.Add(threat);
						bool flag14 = false;
						float num2 = 0f;
						foreach (CoOpSpRpG.Console console2 in __instance.consoles.Values)
						{
							if (console2 != null && console2.crew != null)
							{
								if (console2.crew.faction == __instance.faction && console2.crew.team != null && !console2.crew.team.ignoreAggression)
								{
									bool flag17 = console2.crew.team.threaten(threat);
									if (flag17)
									{
										flag14 = true;
										num2 = console2.crew.team.aggroRadius;
									}
								}
							}
						}
						bool flag18 = flag14;
						if (flag18)
						{
							for (int i = 0; i < session.allShips.Values.Count(); i++)
							{
								var ship = session.allShips.Values.ToList()[i];
								if (ship != __instance && ship.faction == __instance.faction && Vector2.DistanceSquared(ship.position, __instance.position) < num2 * num2)
								{
									ship.aggro(threat, session);
								}
							}
						}
					}
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(BattleSession), "localFireShot", new Type[] { typeof(Bullet) })]
		public class BattleSession_localFireShot
		{

			[HarmonyPrefix]
			private static void Prefix(BattleSession __instance, ref Bullet shot)
			{
				/*
				if (shot.effect == WeaponEffectType.beam_shard_impact) // reducing beam damage on top of the ship even further
				{
					foreach (Ship ship in __instance.allShips.Values)
					{
						bool flag = ship.faction != shot.source && ship.testCollision(shot.position, __instance);
						if (flag)
						{
							shot.damageMod = 0.5f;
							break;
						}
					}
				}
				*/
				for (int i = 0; i < __instance.allShips.Values.Count(); i++) // reducing all damage on top of the ship to 0
				{
					var ship = __instance.allShips.Values.ToList()[i];
					bool flag = ship.faction != shot.sourceFaction && ship.testCollision(shot.position, __instance);
					if (flag)
					{
						shot.damageMod = 0f;
						break;
					}
				}
			}
		}

		[HarmonyPatch(typeof(Monster))]
		[HarmonyPatch(MethodType.Constructor)]
		[HarmonyPatch(new Type[] { typeof(int) })]
		public class Monster_Monster
		{

			[HarmonyPostfix]
			private static void Postfix(Monster __instance, int t) // scale monster stats
			{
				int difficulty;
				if (PLAYER.currentGame != null && PLAYER.currentWorld != null && PLAYER.currentSession != null && PLAYER.avatar != null && PLAYER.currentShip != null)
				{
					//var shipsunlocked = CHARACTER_DATA.shipsUnlocked();
					//var mostexpensivedesign = Globals.DifficultyFromCost(HW_CHARACTER_DATA_Extensions.mostExpensiveDesign());
					//var mostExpensiveBuildableDesign = Globals.DifficultyFromCost(Globals.mostExpensiveBuildableDesign());
					difficulty = (int)Math.Round(MathHelper.Clamp((float)((Math.Max(CHARACTER_DATA.shipsUnlocked(), Globals.mostExpensiveDesigndifficulty * 3) + Globals.mostExpensiveBuildableDesigndifficulty * 3.2) / 2 * HWCONFIG.GlobalDifficulty), 1f, 100f));
					float distance = 0;
					float playerarmorquality = -2;
					float playerweaponquality = -2;

					if (PLAYER.avatar.heldArmor != null)
						playerarmorquality = PLAYER.avatar.heldArmor.Quality;

					if (PLAYER.avatar.heldItem != null && PLAYER.avatar.heldItem.GetType() == typeof(Gun))
						playerweaponquality = (PLAYER.avatar.heldItem as Gun).Quality;

					if (playerweaponquality > 1 && playerarmorquality > 1)
					{
						difficulty = (int)Math.Round((difficulty * (1 + ((Math.Max(1, playerweaponquality) + Math.Max(1, playerarmorquality)) / 2 / 70))));
						difficulty = (int)Math.Round(MathHelper.Clamp(difficulty, 1f, (playerweaponquality + playerarmorquality) / 2 * 1.1f));
					}
					else
					{
						difficulty = (int)Math.Round((difficulty * (1 + (Math.Max(1, playerweaponquality) / 70))));
						difficulty = (int)Math.Round(MathHelper.Clamp(difficulty, 1f, Math.Max(1, playerarmorquality * 1.1f)));
					}

					if (PLAYER.currentSession.grid == CONFIG.spHomeGrid)
					{
						distance = Vector2.Distance(PLAYER.currentShip.position, PLAYER.currentGame.position);
					}
					else
					{
						Vector2 vector2 = new Vector2((float)(CONFIG.spHomeGrid.X - PLAYER.currentShip.grid.X), (float)(CONFIG.spHomeGrid.Y - PLAYER.currentShip.grid.Y)) * 200000f;
						vector2 += PLAYER.currentGame.position;
						distance = Vector2.Distance(PLAYER.currentShip.position, vector2);
					}


					float multiplier = Math.Max(6155714f / Math.Max(distance * (1 + ((Math.Max(1, playerweaponquality) + Math.Max(1, playerarmorquality))/2/50)), 1), 1);
					difficulty = Math.Min(difficulty, difficulty / (int)Math.Max((multiplier / (Math.Max(multiplier, difficulty) / difficulty)), 1));
					difficulty = Math.Min(HWCONFIG.MaxMonsterLevel, difficulty);
					int monsterHealthScaled = 100;
					float monsterSpeedScaled = 100f;
					float monsterDetectRangeScaled = 400f;
					float monsterInterestRangeScaled = 1000f;
					float monsterAttackRangeScaled = 10f;
					int monsterDamageScaled = 1;
					int monsterThicknessScaled = 1;
					int points = difficulty * 4;
					int healthpoints = Squirrel3RNG.Next(difficulty / 4, difficulty);
					monsterHealthScaled += Squirrel3RNG.Next(Math.Max(0, (healthpoints - 4) * 20 * 2), Math.Max(10, Convert.ToInt32(((float)healthpoints - 4) * 22 * 3)));
					difficulty = (points - healthpoints) / 4;
					points = difficulty * 3;
					int speedpoints = Squirrel3RNG.Next(difficulty / 3, difficulty);
					if (Squirrel3RNG.Next(10) == 1)
						monsterSpeedScaled *= 1f + (0.04f * Squirrel3RNG.Next(Math.Max(1, (speedpoints - 5)), Math.Max(2, Convert.ToInt32(((float)speedpoints - 5) * 3))));
					difficulty = (points - speedpoints) / 4;
					points = difficulty * 2;
					int damagepoints = Squirrel3RNG.Next(difficulty / 2, difficulty / 2 * 3);
					if (Squirrel3RNG.Next(5) == 1)
						monsterDamageScaled += Squirrel3RNG.Next(Math.Max(0, (damagepoints - 3) * 3), Math.Max(1, Convert.ToInt32(((float)damagepoints - 3) * 4)));
					points -= damagepoints;
					if (Squirrel3RNG.Next(5) == 1)
						monsterThicknessScaled += Squirrel3RNG.Next(Math.Max(0, (points - 3) * 2), Math.Max(1, Convert.ToInt32(((float)points - 3) * 5)));

					if (MONSTERBAG.monsterHealth.ContainsKey(t))
					{
						__instance.health = Math.Max(MONSTERBAG.monsterHealth[t], monsterHealthScaled);
					}
					else
					{
						__instance.health = monsterHealthScaled;
					}
					__instance.healthMax = __instance.health;
					if (MONSTERBAG.monsterSpeed.ContainsKey(t))
					{
						__instance.speed = Math.Max(MONSTERBAG.monsterSpeed[t], monsterSpeedScaled);
					}
					else
					{
						__instance.speed = monsterSpeedScaled;
					}
					if (MONSTERBAG.monsterDetectRange.ContainsKey(t))
					{
						__instance.detectRange = MONSTERBAG.monsterDetectRange[t];
					}
					else
					{
						__instance.detectRange = monsterDetectRangeScaled;
					}
					if (MONSTERBAG.monsterInterestRange.ContainsKey(t))
					{
						__instance.loseInterestRange = MONSTERBAG.monsterInterestRange[t];
					}
					else
					{
						__instance.loseInterestRange = monsterInterestRangeScaled;
					}
					if (MONSTERBAG.monsterAttackRange.ContainsKey(t))
					{
						__instance.attackRange = MONSTERBAG.monsterAttackRange[t];
					}
					else
					{
						__instance.attackRange = monsterAttackRangeScaled;
					}
					if (MONSTERBAG.monsterFightRange.ContainsKey(t))
					{
						__instance.fightRange = MONSTERBAG.monsterFightRange[t];
					}
					else
					{
						__instance.fightRange = __instance.attackRange - 6f;
					}
					if (MONSTERBAG.monsterDamage.ContainsKey(t))
					{
						__instance.attackDamage = Math.Max(MONSTERBAG.monsterDamage[t], monsterDamageScaled);
					}
					else
					{
						__instance.attackDamage = monsterDamageScaled;
					}
					if (MONSTERBAG.monsterThickness.ContainsKey(t))
					{
						__instance.thickness = Math.Max(MONSTERBAG.monsterThickness[t], monsterThicknessScaled);
					}
					else
					{
						__instance.thickness = monsterThicknessScaled;
					}
					__instance.Setscaled(true);
				}
				
			}
		}
	
		[HarmonyPatch(typeof(Monster), "applyDamage")] // making monsters drop items on death.
		public class Monster_applyDamage
		{
			[HarmonyPostfix]
			private static void Postfix(Monster __instance, MicroCosm cosm)
			{
				if (Globals.initialized && PLAYER.currentGame != null && PLAYER.currentWorld != null && PLAYER.currentSession != null && PLAYER.avatar != null && PLAYER.currentShip != null)
				{
					if (__instance.animState == MonsterState.dying || __instance.dead)
					{
						if (cosm != null && cosm.crew != null)
						{
							/*
							float playerarmorquality = PLAYER.avatar.heldArmor.Quality;
							int monsterHealth = __instance.healthMax;
							float monsterSpeed = __instance.speed;
							int monsterDamage = __instance.attackDamage;
							int monsterThickness = __instance.thickness;
							*/

							InventoryItem Item = null;
							InventoryItem Item2 = null;
							int shipsUnlocked = (int)Math.Round((float)((Math.Max(CHARACTER_DATA.shipsUnlocked(), Globals.mostExpensiveDesigndifficulty * 3) + Globals.mostExpensiveBuildableDesigndifficulty * 3.5) / 2));
							//shipsUnlocked = 30; //debugging


							//scaling loot drop chance on distance to the homebase
							float distance = 0;
							if (PLAYER.currentSession.grid == CONFIG.spHomeGrid)
							{
								distance = Vector2.Distance(PLAYER.currentShip.position, PLAYER.currentGame.position);
							}
							else
							{
								Vector2 vector2 = new Vector2((float)(CONFIG.spHomeGrid.X - PLAYER.currentShip.grid.X), (float)(CONFIG.spHomeGrid.Y - PLAYER.currentShip.grid.Y)) * 200000f;
								vector2 += PLAYER.currentGame.position;
								distance = Vector2.Distance(PLAYER.currentShip.position, vector2);
							}
							float multiplier = Math.Max(6155714f / Math.Max(distance, 1), 1);
							int maxdropchance = 35;
							maxdropchance /= (int)Math.Max((multiplier / (Math.Max(multiplier, maxdropchance) / maxdropchance)), 1);

							int selectItem = Squirrel3RNG.Next(1, 14);
							if (selectItem == 1)
							{
								//weapon
								float gunQuality = Squirrel3RNG.Next(Math.Min(30, shipsUnlocked) - 10, 40);
								bool flag = gunQuality <= 0f;
								gunQuality = MathHelper.Clamp(gunQuality, 10f, (float)shipsUnlocked + 10f);
								gunQuality = MathHelper.Clamp(gunQuality, 10f, 39.9f);
								if (flag)
								{
									Item = new Gun(gunQuality, GunSpawnFlags.force_pistol);
								}
								else
								{
									Item = new Gun(gunQuality, GunSpawnFlags.none);
								}
							}
							if (selectItem == 2)
							{
								//armor
								float armorQuality = Squirrel3RNG.Next(Math.Min(30, shipsUnlocked) - 10, 40);
								armorQuality = MathHelper.Clamp(armorQuality, 10f, (float)shipsUnlocked + 10f);
								armorQuality = MathHelper.Clamp(armorQuality, 10f, 39.9f);
								Item = new CrewArmor(armorQuality, ArmorSpawnFlags.none);

							}

							if (selectItem == 3 && Squirrel3RNG.Next(10) == 1)
							{
								// repair gun
								float repairgunQuality = Squirrel3RNG.Next(Math.Min(30, shipsUnlocked) - 10, 40);
								repairgunQuality = MathHelper.Clamp(repairgunQuality, 1f, (float)shipsUnlocked + 10f);
								repairgunQuality = MathHelper.Clamp(repairgunQuality, 1f, 39.9f) / 5;
								Item = new RepairGun(repairgunQuality);
							}
							if (selectItem == 4 && Squirrel3RNG.Next(10) == 1)
							{
								// fire extinguisher
								float extinguisherQuality = Squirrel3RNG.Next(Math.Min(30, shipsUnlocked) - 10, 40);
								extinguisherQuality = MathHelper.Clamp(extinguisherQuality, 10f, (float)shipsUnlocked + 10f);
								extinguisherQuality = MathHelper.Clamp(extinguisherQuality, 10f, 39.9f) / 2;
								Item = new Extinguisher(extinguisherQuality);
							}

							if (selectItem == 5 && Squirrel3RNG.Next(35) == 1)
							{
								//observant aura 
								Item = new ObservantAura(Math.Max(1f, shipsUnlocked / 5f), Math.Min(Math.Max(1, maxdropchance / 7), 5));
							}

							if (selectItem == 6)
							{
								// artifact
								if (Squirrel3RNG.Next(20) == 1)
									Item = new ArtifactItem((float)(1.0 + Squirrel3RNG.NextDouble() * 24.0));
							}
							if (selectItem == 7 && Squirrel3RNG.Next(25) == 1) //rare crystals
							{
								switch (Squirrel3RNG.Next(10))
								{
									case 0:
										Item = new CrystalGene(CrystalType.gold_bulk);
										break;
									case 1:
										Item = new CrystalGene(CrystalType.gold_facet);
										break;
									case 2:
										Item = new InventoryItem(InventoryItemType.crystal_spore);
										break;
									case 3:
										Item = new CrystalGene(CrystalType.mitraxit_bulk);
										break;
									case 4:
										Item = new CrystalGene(CrystalType.mitraxit_facet);
										break;
									case 5:
										Item = new CrystalGene(CrystalType.rhodium_bulk);
										break;
									case 6:
										Item = new CrystalGene(CrystalType.rhodium_facet);
										break;
									case 7:
										Item = new CrystalGene(CrystalType.titanium_bulk);
										break;
									case 8:
										Item = new CrystalGene(CrystalType.titanium_facet);
										break;
									case 9:
										Item = new CrystalGene(CrystalType.iron_facet);
										break;
									default:
										Item = new CrystalGene(CrystalType.iron_facet);
										break;
								}
							}
							if (selectItem == 8 && Squirrel3RNG.Next(35) == 1)
							{
								//core node
								Item = new InventoryItem(InventoryItemType.core_node);
								Item.stackSize = 1U;
							}
							if (selectItem == 9 && Squirrel3RNG.Next(35) == 1)
							{
								//stealth aura 
								//Item = new DeadeyeAura(Math.Max(1f, shipsUnlocked / 5f), Math.Min(Math.Max(1, maxdropchance / 7), 5));
								Item = new StealthAura(Math.Max(1f, shipsUnlocked / 5f), Math.Min(Math.Max(1, maxdropchance / 7), 5));
							}
							if (selectItem == 10 && Squirrel3RNG.Next(35) == 1)
							{
								//plasma aura 
								Item = new AbsorbShotSpeed(Math.Max(1f, shipsUnlocked / 5f));
							}
							if (selectItem == 11 && Squirrel3RNG.Next(35) == 1)
							{
								//sturdy aura 
								//Item = new SturdyAura(Math.Max(1f, shipsUnlocked / 5f), Math.Min(Math.Max(1, maxdropchance / 7), 5));
								//Item = new StealthAura(Math.Max(1f, shipsUnlocked / 5f), Math.Min(Math.Max(1, maxdropchance / 7), 5));
								//Engineer aura
								Item = new EngineerAura(Math.Max(1f, shipsUnlocked / 5f), Math.Min(Math.Max(1, maxdropchance / 7), 5));
							}
							if (selectItem == 12 && Squirrel3RNG.Next(15) == 1)
							{
								//random tuning kit
								Item = new TuningKit(Math.Min(Math.Max(1f, maxdropchance / 4f), 12f));
							}

							if (selectItem == 13 && Squirrel3RNG.Next(20) == 1)
							{
								//beam aura
								Item = new BeamEnergyDistributer(Math.Min(Math.Max(1, shipsUnlocked / 4), Math.Min(Math.Max(1, maxdropchance / 4), 12)));
							}


							int selectItem2 = Squirrel3RNG.Next(1, 14);
							if (selectItem2 == 1)
							{
								switch (Squirrel3RNG.Next(3))
								{
									case 0:
										// Shirt1
										Item2 = new Shirt(vanityItemType.crew_shirt);
										break;
									case 1:
										// Shirt2
										Item2 = new Shirt(vanityItemType.red_shirt);
										break;
									case 2:
										// Shirt3
										Item2 = new Shirt(vanityItemType.tank_top);
										break;
									default:
										// Shirt1
										Item2 = new Shirt(vanityItemType.crew_shirt);
										break;
								}
							}
							if (selectItem2 == 2)
							{
								// biomass
								Item2 = new InventoryItem(InventoryItemType.biomass);
								Item2.stackSize = 1U;
							}
							if (selectItem2 == 3)
							{
								// biomass
								Item2 = new InventoryItem(InventoryItemType.biomass);
								Item2.stackSize = 1U;
							}
							if (selectItem2 == 4)
							{
								// biomass
								Item2 = new InventoryItem(InventoryItemType.biomass);
								Item2.stackSize = 1U;
							}
							if (selectItem2 == 5)
							{
								// dirt
								Item2 = new InventoryItem(InventoryItemType.dirt);
							}
							if (selectItem2 == 6)
							{
								//crystals

								if (RANDOM.chance(0.1))
								{
									Item2 = new InventoryItem(InventoryItemType.crystal_spore);
								}
								if (RANDOM.chance(0.9))
								{
									switch (Squirrel3RNG.Next(16))
									{
										case 0:
											Item2 = new CrystalGene(CrystalType.collector);
											break;
										case 1:
											Item2 = new CrystalGene(CrystalType.collector);
											break;
										case 2:
											Item2 = new CrystalGene(CrystalType.collector);
											break;
										case 3:
											Item2 = new CrystalGene(CrystalType.collector);
											break;
										case 4:
											{
												Item2 = new CrystalGene(CrystalType.flower);
												break;
											}
										case 5:
											{
												Item2 = new CrystalGene(CrystalType.root);
												break;
											}
										case 6:
											{
												Item2 = new CrystalGene(CrystalType.growth);
												break;
											}
										case 7:
											Item2 = new CrystalGene(CrystalType.lense);
											break;
										default:
											Item2 = new CrystalGene(CrystalType.iron_bulk);
											break;
									}
								}
								else
								{
									switch (Squirrel3RNG.Next(12))
									{
										case 0:
											Item2 = new CrystalBranch(2);
											break;
										case 1:
											Item2 = new CrystalBranch(2);
											break;
										case 2:
											Item2 = new CrystalBranch(3);
											break;
										case 3:
											Item2 = new CrystalGene(CrystalType.resonator);
											break;
										case 4:
											Item2 = new CrystalGene(CrystalType.resonator);
											break;
										case 5:
											Item2 = new CrystalGene(CrystalType.resonator);
											break;
										case 6:
											Item2 = new CrystalGene(CrystalType.shell);
											break;
										case 7:
											Item2 = new CrystalGene(CrystalType.shell);
											break;
										case 8:
											Item2 = new CrystalGene(CrystalType.shell);
											break;
										case 9:
											Item2 = new CrystalGene(CrystalType.battery);
											break;
										case 10:
											Item2 = new CrystalGene(CrystalType.battery);
											break;
										case 11:
											Item2 = new CrystalGene(CrystalType.battery);
											break;
										default:
											Item2 = new CrystalGene(CrystalType.resonator);
											break;
									}
								}
							}
							if (selectItem2 == 7)
							{
								// mining laser
								Item2 = new Digger();
							}
							if (selectItem2 == 8)
							{
								// neutralino shard
								Item2 = new InventoryItem(InventoryItemType.neutralino_shard);
							}
							if (selectItem2 == 9)
							{
								// crystal shard
								Item2 = new InventoryItem(InventoryItemType.crystal_shard);
							}
							if (selectItem2 == 10 && Squirrel3RNG.Next(15) == 1)
							{
								// repair gun
								float repairgunQuality = Squirrel3RNG.Next(Math.Min(30, shipsUnlocked) - 10, 40);
								repairgunQuality = MathHelper.Clamp(repairgunQuality, 1f, (float)shipsUnlocked + 5f);
								repairgunQuality = MathHelper.Clamp(repairgunQuality, 1f, 39.9f) / 5;
								Item2 = new RepairGun(repairgunQuality);
							}
							if (selectItem2 == 11 && Squirrel3RNG.Next(15) == 1)
							{
								// fire extinguisher
								float extinguisherQuality = Squirrel3RNG.Next(Math.Min(30, shipsUnlocked) - 10, 40);
								extinguisherQuality = MathHelper.Clamp(extinguisherQuality, 10f, (float)shipsUnlocked + 5f);
								extinguisherQuality = MathHelper.Clamp(extinguisherQuality, 10f, 39.9f) / 2;
								Item2 = new Extinguisher(extinguisherQuality);
							}
							if (selectItem2 == 12 && Squirrel3RNG.Next(10) == 1)
							{
								//auras
								int rnd = Squirrel3RNG.Next(1, 9);
								if (rnd == 1)
									Item2 = new BulletEnhancer(checked(1 + Squirrel3RNG.Next(2))); //wussBulletAura
								if (rnd == 2)
									Item2 = new EmergencyDisplacementOrb((float)(Squirrel3RNG.NextDouble() * 6.0)); //wussDodgeAura
								if (rnd == 3)
									Item2 = new EmergencyDisplacementOrb((float)(2.0 + Squirrel3RNG.NextDouble() * 6.0));//medDodgeAura
								if (rnd == 4)
									Item2 = new NanoAura(Squirrel3RNG.Next(6), checked(Squirrel3RNG.Next(7) + 6)); //higherQualityNanoAura
								if (rnd == 5)
									Item2 = new NanoAura(Squirrel3RNG.Next(6), checked(Squirrel3RNG.Next(4) + 3)); //medQualityNanoAura
								if (rnd == 6)
									Item2 = new NanoAura(Squirrel3RNG.Next(5), Squirrel3RNG.Next(4));//lowQualityNanoAura
								if (rnd == 7)
									Item2 = new DoubleShotCapacitor((float)(Squirrel3RNG.NextDouble() * 4.0)); //doubleShotAura
								if (rnd == 8)
									Item2 = new MovementModAura((float)(Squirrel3RNG.NextDouble() * 4.0));//accelAura
							}
							if (selectItem2 == 13 && Squirrel3RNG.Next(2) == 1)
							{
								// lead ore
								Item2 = new InventoryItem(InventoryItemType.lead_ore);
							}

							for (int i = 0; i < cosm.crew.Values.Count; i++)
							{
								var crew = cosm.crew.Values.ToList()[i];
								if (crew.state != CrewState.dead && Vector2.DistanceSquared(crew.position, __instance.position) < 2000f * 2000f && crew.isPlayer)
								{
									if (Squirrel3RNG.Next(0, 80) <= maxdropchance)
									{
										if (PLAYER.avatar.currentCosm.ship != null && Item != null)
										{
											PLAYER.avatar.currentCosm.ship.threadDumpCargo(Item);
											PLAYER.avatar.GetfloatyText().Enqueue("+ 1 loot item deployed into space");
										}
									}
									else
									{
										if (PLAYER.avatar.currentCosm.ship != null && Item2 != null)
										{
											if (Squirrel3RNG.Next(0, 100) <= maxdropchance * 2 + 10)
											{

												if (Item2.type == InventoryItemType.biomass)
												{
													uint num2 = 100U * Item2.stackSize;
													CHARACTER_DATA.exp += unchecked((ulong)num2);
													PLAYER.currentGame.team.grantExp(PLAYER.avatar.currentCosm.ship, num2);
													PLAYER.avatar.GetfloatyText().Enqueue("+" + SCREEN_MANAGER.formatCreditString(unchecked((ulong)(checked(100U * Item2.stackSize)))) + " exp");
												}
												else
												{
													PLAYER.avatar.currentCosm.ship.threadDumpCargo(Item2);
													PLAYER.avatar.GetfloatyText().Enqueue("+ 1 loot item deployed into space");
												}
											}
										}
									}

								}
							}
						}
					}
				}
			}
		}
	

		//fixing monster animation bug (possible obsolete, just to be sure)
		[HarmonyPatch(typeof(Monster), "animateMovement")]
		public class Monster_animateMovement
		{
			[HarmonyPrefix]
			private static bool Prefix(Monster __instance, SheetAnimation ___idleAnim, SheetAnimation ___walkingAnim, List<SheetAnimation> ___attackAnims, SheetAnimation ___deathAnim, SheetAnimation ___currentAnim)
			{
				if (__instance.animState == MonsterState.idle && ___walkingAnim != null)
				{
					__instance.animState = MonsterState.moving;
				}
				else if(___walkingAnim == null)
				{
					___idleAnim = new SheetAnimation(120, 120, 9, 49, 0.033333335f, 28, true);
					___walkingAnim = new SheetAnimation(120, 120, 9, 0, 0.033333335f, 24, true);
					___attackAnims = new List<SheetAnimation>();
					___attackAnims.Add(new SheetAnimation(120, 120, 9, 25, 0.033333335f, 11, false, 0.26666668f));
					___attackAnims.Add(new SheetAnimation(120, 120, 9, 37, 0.033333335f, 11, false, 0.26666668f));
					___currentAnim = ___idleAnim;
				}
				return false;
			}
		}
		

		[HarmonyPatch(typeof(Monster), "update")]
		public class Monster_update
		{
			[HarmonyPrefix]
			private static void Postfix(Monster __instance)
			{
				if(Globals.initialized && !__instance.Getscaled())
				{
					Globals.upscaleMonster(__instance);
                }
			}
		}

		[HarmonyPatch(typeof(Crew))]
		[HarmonyPatch(MethodType.Constructor)]
		[HarmonyPatch(new Type[] { })]
		public class Crew_Crew
		{

			[HarmonyPostfix]
			private static void Postfix(Crew __instance) // randomizing and scaling all npc outfits
			{

				/*
				int difficulty;
				int shipsUnlocked;
				if (PLAYER.currentGame != null && PLAYER.currentWorld != null && PLAYER.currentSession != null && PLAYER.avatar != null)
				{
					shipsUnlocked = (int)Math.Round(MathHelper.Clamp(((float)CHARACTER_DATA.shipsUnlocked() * HWCONFIG.GlobalDifficulty), 1f, 100f));
					if (PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && !PLAYER.currentShip.cosm.isStation)
					{
						difficulty = Globals.DifficultyFromCost((int)Hull.getCost(PLAYER.currentShip.botD));
					}
					else
					{
						difficulty = Globals.DifficultyFromCost(Globals.mostExpensiveBuildableDesign());
					}
				}
				else
				{
					difficulty = Globals.difficulty;
					shipsUnlocked = Globals.shipsUnlocked;
				}
				float gunQuality = Squirrel3RNG.Next(Math.Max(Math.Min(30, shipsUnlocked) - 10, difficulty * 4), 40);
				float armorQuality = Squirrel3RNG.Next(Math.Max(Math.Min(30, shipsUnlocked) - 10, difficulty * 4), 40);
				bool flag = gunQuality <= 0f;
				bool flag2 = armorQuality > 0f;
				gunQuality = MathHelper.Clamp(gunQuality, 10f, (float)Math.Min(shipsUnlocked + 10, difficulty * 4));
				armorQuality = MathHelper.Clamp(armorQuality, 10f, (float)Math.Min(shipsUnlocked + 10, difficulty * 4));
				*/
				int difficulty = 0;
				int shipsUnlocked = 0;
				float playerarmorquality = -2;
				float playerweaponquality = -2;
				if (PLAYER.currentGame != null && PLAYER.currentWorld != null && PLAYER.currentSession != null && PLAYER.avatar != null)
				{
					if (PLAYER.avatar.heldArmor != null)
						playerarmorquality = PLAYER.avatar.heldArmor.Quality;

					if (PLAYER.avatar.heldItem != null && PLAYER.avatar.heldItem.GetType() == typeof(Gun))
						playerweaponquality = (PLAYER.avatar.heldItem as Gun).Quality;

					if(playerweaponquality <= -2 || playerarmorquality <= -2)
					{ 
						shipsUnlocked = (int)Math.Round(MathHelper.Clamp(((float)CHARACTER_DATA.shipsUnlocked() * HWCONFIG.GlobalDifficulty), 1f, 100f));
						if (PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && !PLAYER.currentShip.cosm.isStation && Globals.currentshipdifficulty > -1)
						{
							difficulty = Globals.currentshipdifficulty;
						}
						else
						{
							difficulty = Globals.mostExpensiveBuildableDesigndifficulty;
						}
					}
				}
				else
				{
					difficulty = Globals.mostExpensiveBuildableDesigndifficulty;
					shipsUnlocked = Globals.shipsUnlocked;
				}


				float gunQuality;
				float armorQuality;

				if (playerarmorquality <= -2)
				{
					gunQuality = Squirrel3RNG.Next(Math.Max(Math.Min(30, shipsUnlocked) - 10, difficulty * 4 - 5), 40);
					gunQuality = MathHelper.Clamp(gunQuality, 10f, Math.Max(10, Math.Min(shipsUnlocked + 10, difficulty * 4 + 5)));
				}
				else
				{
					gunQuality = Squirrel3RNG.Next(Math.Max(Math.Min(15, shipsUnlocked), difficulty * 4), 40);
					gunQuality = MathHelper.Clamp(gunQuality, 10f, Math.Max(10, (int)Math.Round(playerarmorquality)));
				}


				if (playerweaponquality <= -2)
				{ 
					armorQuality = Squirrel3RNG.Next(Math.Max(Math.Min(30, shipsUnlocked) - 10, difficulty * 4 - 5), 40);
					armorQuality = MathHelper.Clamp(armorQuality, 10f, Math.Max(10, Math.Min(shipsUnlocked + 10, difficulty * 4 + 5)));
				}
				else
				{
					armorQuality = Squirrel3RNG.Next(Math.Max(Math.Min(15, shipsUnlocked), difficulty * 4), 40);
					armorQuality = MathHelper.Clamp(armorQuality, 10f, Math.Max(10,(int)Math.Round(playerweaponquality)));
				}


				bool flag = gunQuality <= 0f;
				bool flag2 = armorQuality > 0f;
				gunQuality = MathHelper.Clamp(gunQuality, 10f, 39.9f);
				armorQuality = MathHelper.Clamp(armorQuality, 10f, 39.9f);
				if (flag)
				{
					__instance.heldItem = new Gun(gunQuality, GunSpawnFlags.force_pistol);
				}
				else
				{
					__instance.heldItem = new Gun(gunQuality, GunSpawnFlags.no_oneshot);
				}
				__instance.accuracy = (float)Math.Max(30, (int)(120f - 2f * gunQuality));
				if (flag2)
				{
					__instance.heldArmor = new CrewArmor(armorQuality, ArmorSpawnFlags.none);
				}

			}
		}

		[HarmonyPatch(typeof(Crew), "outfit", new Type[] { typeof(float), typeof(float) })]
		public class Crew_outfit
		{

			[HarmonyPostfix]
			private static void Postfix(Crew __instance, float gunQuality, float armorQuality) // randomizing and scaling special npc outfits
			{
				if (__instance.state != CrewState.dead)
				{
					int difficulty = 0;
					int shipsUnlocked = 0;
					float playerarmorquality = -2;
					float playerweaponquality = -2;
					if (PLAYER.currentGame != null && PLAYER.currentWorld != null && PLAYER.currentSession != null && PLAYER.avatar != null)
					{
						if (PLAYER.avatar.heldArmor != null)
							playerarmorquality = PLAYER.avatar.heldArmor.Quality;

						if (PLAYER.avatar.heldItem != null && PLAYER.avatar.heldItem.GetType() == typeof(Gun))
							playerweaponquality = (PLAYER.avatar.heldItem as Gun).Quality;

						if (playerweaponquality <= -2 || playerarmorquality <= -2)
						{
							shipsUnlocked = (int)Math.Round(MathHelper.Clamp(((float)CHARACTER_DATA.shipsUnlocked() * HWCONFIG.GlobalDifficulty), 1f, 100f));
							if (PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && !PLAYER.currentShip.cosm.isStation && Globals.currentshipdifficulty > -1)
							{
								difficulty = Globals.currentshipdifficulty;
							}
							else
							{
								difficulty = Globals.mostExpensiveBuildableDesigndifficulty;
							}
						}
					}
					else
					{
						difficulty = Globals.mostExpensiveBuildableDesigndifficulty;
						shipsUnlocked = Globals.shipsUnlocked;
					}

					float MygunQuality;
					float MyarmorQuality;


					if (playerweaponquality <= -2)
					{
						MygunQuality = Squirrel3RNG.Next(Math.Max(Math.Min(30, shipsUnlocked) - 10, difficulty * 4 - 5), 40);
						MygunQuality = MathHelper.Clamp(MygunQuality, 10f, Math.Max(10, Math.Min(shipsUnlocked + 10, difficulty * 4 + 5)));
					}
					else
					{
						MygunQuality = Squirrel3RNG.Next(Math.Max(Math.Min(15, shipsUnlocked), difficulty * 4), 40);
						MygunQuality = MathHelper.Clamp(MygunQuality, 10f, Math.Max(10, (int)Math.Round(playerweaponquality)));
					}


					if (playerarmorquality <= -2)
					{
						MyarmorQuality = Squirrel3RNG.Next(Math.Max(Math.Min(30, shipsUnlocked) - 10, difficulty * 4 - 5), 40);
						MyarmorQuality = MathHelper.Clamp(MyarmorQuality, 10f, Math.Max(10, Math.Min(shipsUnlocked + 10, difficulty * 4 + 5)));
					}
					else
					{
						MyarmorQuality = Squirrel3RNG.Next(Math.Max(Math.Min(15, shipsUnlocked), difficulty * 4), 40);
						MyarmorQuality = MathHelper.Clamp(MyarmorQuality, 10f, Math.Max(10, (int)Math.Round(playerarmorquality)));
					}


					bool flag = MygunQuality <= 0f;
					bool flag2 = MyarmorQuality > 0f;
					gunQuality = Math.Max(gunQuality, MygunQuality); // only modify default input if it is lower quality then scaled
					armorQuality = Math.Max(armorQuality, MyarmorQuality); // only modify default input if it is lower quality then scaled
					gunQuality = MathHelper.Clamp(gunQuality, 10f, 39.9f);
					armorQuality = MathHelper.Clamp(armorQuality, 10f, 39.9f);
					if (flag)
					{
						__instance.heldItem = new Gun(gunQuality, GunSpawnFlags.force_pistol);
					}
					else
					{
						__instance.heldItem = new Gun(gunQuality, GunSpawnFlags.no_oneshot);
					}
					__instance.accuracy = (float)Math.Max(30, (int)(120f - 2f * gunQuality));
					if (flag2)
					{
						__instance.heldArmor = new CrewArmor(armorQuality, ArmorSpawnFlags.none);
					}
				}
			}
		}

		[HarmonyPatch(typeof(Crew), "_kill")]
		public class Crew__kill
		{

			[HarmonyPostfix]
			private static void Postfix(Crew __instance)
			{
				if (Globals.initialized && PLAYER.currentGame != null)
				{
					if (PLAYER.avatar != null && !__instance.isPlayer && __instance.faction != PLAYER.avatar.faction) // npcs will drop some of their gear on death
					{
						if (__instance.currentCosm != null && __instance.currentCosm.ship != null)
						{
							if (__instance.inventory != null)
							{
								if (__instance.currentCosm.ship.id == PLAYER.currentGame.homeBaseId) // if death happens on player's homebase
								{
									var inventory = __instance.inventory;
									foreach (InventoryItem inventoryItem in inventory)
									{
										if (inventoryItem != null && inventoryItem.type != InventoryItemType.repair_gun && inventoryItem.type != InventoryItemType.fire_extinguisher && inventoryItem.type != InventoryItemType.fire_extinguisher_2)
										{
											if (inventoryItem.type == InventoryItemType.exotic_matter || inventoryItem.type == InventoryItemType.dense_exotic_matter)
											{
												if (PLAYER.avatar.currentCosm != null && __instance.currentCosm == PLAYER.avatar.currentCosm)
												{
													PLAYER.avatar.GetfloatyText().Enqueue("+" + SCREEN_MANAGER.formatCreditString((ulong)(inventoryItem.refineValue * inventoryItem.stackSize)) + " credits");
												}
												CHARACTER_DATA.credits += (ulong)(inventoryItem.refineValue * inventoryItem.stackSize);
											}
											else if (ITEMBAG.isResource.Contains(inventoryItem.type))
											{
												var inventoryItemType = inventoryItem.type;
												uint num = (uint)inventoryItem.stackSize;
												long amount = CHARACTER_DATA.getResource(inventoryItemType) + (long)((ulong)num);
												CHARACTER_DATA.setResource(inventoryItemType, amount);
												if (PLAYER.avatar.currentCosm != null && __instance.currentCosm == PLAYER.avatar.currentCosm)
												{
													PLAYER.avatar.GetfloatyText().Enqueue("+" + inventoryItem.stackSize.ToString() + " " + inventoryItem.toolTip.tip);
												}
											}
											else
											{
												if (Squirrel3RNG.Next(7) == 1)
												{
													if (PLAYER.avatar.currentCosm != null && __instance.currentCosm == PLAYER.avatar.currentCosm)
													{
														if (PLAYER.avatar.placeInFirstSlot(inventoryItem))
														{
															PLAYER.avatar.GetfloatyText().Enqueue("+" + inventoryItem.stackSize.ToString() + " " + inventoryItem.toolTip.tip);
														}
														else
														{
															__instance.currentCosm.ship.threadDumpCargo(inventoryItem);
														}
													}
													else
													{
														__instance.currentCosm.ship.threadDumpCargo(inventoryItem);
													}
												}
											}
										}
									}
								}
								else // if death happens not on player's homebase
								{
									var inventory = __instance.inventory;
									foreach (InventoryItem inventoryItem in inventory)
									{
										if (inventoryItem != null && inventoryItem.type != InventoryItemType.repair_gun && inventoryItem.type != InventoryItemType.fire_extinguisher && inventoryItem.type != InventoryItemType.fire_extinguisher_2)
										{
											if (Squirrel3RNG.Next(7) == 1)
											{
												__instance.currentCosm.ship.threadDumpCargo(inventoryItem);
											}
										}
									}
								}
							}
						}
						else if (PLAYER.currentSession.corpses.Contains(__instance) && PLAYER.currentShip != null)
						{
							for (int i = 0; i < PLAYER.currentWorld.economy.nodes.Count; i++)
							{
								if (PLAYER.currentWorld.economy.nodes[i].id != PLAYER.currentGame.homeBaseId && Vector2.DistanceSquared(PLAYER.currentWorld.economy.nodes[i].position, __instance.position) <= CONFIG.minViewDist * CONFIG.minViewDist)
								{
									if (Vector2.DistanceSquared(PLAYER.currentShip.position, __instance.position) <= CONFIG.minViewDist * CONFIG.minViewDist && Vector2.DistanceSquared(PLAYER.currentWorld.economy.nodes[i].position, PLAYER.currentShip.position) <= CONFIG.minViewDist * CONFIG.minViewDist)
									{
										if (Globals.globalfactions.ContainsKey(__instance.faction))
										{
											Globals.changeReputation(__instance.faction, -5, "a nearby station detected faction member death in your vicinity.");
										}
										else if (__instance.factionless)
										{
											Globals.changeReputation(5UL, -5, "a nearby station detected civilian death in your vicinity.");
											Globals.changeReputation(3UL, -5, "a nearby station detected civilian death in your vicinity.");
										}
										break;
									}
								}
							}
							var sessionships = PLAYER.currentSession.allShips.Keys.ToArray();
							for (int i = 0; i < sessionships.Length; i++)
							{
								var shipid = sessionships[i];
								if (!Globals.eventflags[GlobalFlag.PiratesCalledForDefense] && shipid != PLAYER.currentGame.homeBaseId && PLAYER.currentSession.allShips[shipid] != PLAYER.currentShip
								&& PLAYER.currentSession.allShips[shipid].cosm?.crew?.FirstOrDefault().Value?.team?.threats != null && Vector2.DistanceSquared(PLAYER.currentSession.allShips[shipid].position, __instance.position) <= CONFIG.minViewDist * CONFIG.minViewDist
								&& PLAYER.currentSession.allShips[shipid].cosm.crew.First().Value.team.threats.Contains(__instance.faction) && !PLAYER.currentSession.allShips[shipid].cosm.crew.First().Value.team.threats.Contains(2UL))
								{
									if (Vector2.DistanceSquared(PLAYER.currentShip.position, __instance.position) <= CONFIG.minViewDist * CONFIG.minViewDist && Vector2.DistanceSquared(PLAYER.currentSession.allShips[shipid].position, PLAYER.currentShip.position) <= CONFIG.minViewDist * CONFIG.minViewDist)
									{
										if (Globals.globalfactions.ContainsKey(PLAYER.currentSession.allShips[shipid].faction))
										{
											Globals.changeReputation(PLAYER.currentSession.allShips[shipid].faction, +3, "for assistance in a battle.");
											break;
										}
									}
								}
								if (!Globals.eventflags[GlobalFlag.PiratesCalledForDefense] && shipid != PLAYER.currentGame.homeBaseId && PLAYER.currentSession.allShips[shipid] != PLAYER.currentShip && PLAYER.currentSession.allShips[shipid].cosm?.crew?.FirstOrDefault().Value?.team?.threats != null
								&& Vector2.DistanceSquared(PLAYER.currentSession.allShips[shipid].position, __instance.position) <= CONFIG.minViewDist * CONFIG.minViewDist
								&& !PLAYER.currentSession.allShips[shipid].cosm.crew.First().Value.team.threats.Contains(__instance.faction) && !PLAYER.currentSession.allShips[shipid].cosm.crew.First().Value.team.threats.Contains(2UL))
								{
									if (Vector2.DistanceSquared(PLAYER.currentShip.position, __instance.position) <= CONFIG.minViewDist * CONFIG.minViewDist && Vector2.DistanceSquared(PLAYER.currentSession.allShips[shipid].position, PLAYER.currentShip.position) <= CONFIG.minViewDist * CONFIG.minViewDist)
									{
										if (Globals.globalfactions.ContainsKey(__instance.faction))
										{
											Globals.changeReputation(__instance.faction, -5, "a nearby neutral ship spoted faction member death in your vicinity.");
											break;
										}
										else if (__instance.factionless)
										{
											Globals.changeReputation(5UL, -5, "a nearby neutral ship detected civilian death in your vicinity.");
											Globals.changeReputation(3UL, -5, "a nearby neutral ship detected civilian death in your vicinity.");
										}
									}
								}
							}
						}
					}

					if (PLAYER.avatar != null && !__instance.isPlayer && __instance.faction != PLAYER.avatar.faction)
					{
						if (__instance.currentCosm != null && __instance.currentCosm.crew != null && __instance.currentCosm.ship != null && __instance.currentCosm.ship.id != PLAYER.currentGame.homeBaseId)
						{
							for (int i = 0; i < __instance.currentCosm.crew.Values.Count; i++)
							{
								var crew = __instance.currentCosm.crew.Values.ToList()[i];
								bool flag4 = crew.faction != __instance.faction && crew.state != CrewState.dead && Vector2.DistanceSquared(crew.position, __instance.position) < 600f * 600f && crew.isPlayer;
								if (flag4)
								{
									bool isStation = __instance.currentCosm.isStation;
									if (isStation)
									{
										bool flag5 = PLAYER.currentWorld != null && PLAYER.currentWorld.economy != null && __instance.currentCosm.ship != null;
										if (flag5)
										{
											foreach (EconomyNode economyNode in PLAYER.currentWorld.economy.nodes)
											{
												bool flag6 = economyNode.id == __instance.currentCosm.ship.id;
												if (flag6)
												{
													__instance.team.threats.Add(crew.faction); // making friends of the killed npc hostile to the player
													if (Globals.globalfactions.ContainsKey(__instance.faction)) //making player loosing reputation with the faction of the killed crew
													{
														Globals.changeReputation(__instance.faction, -5, "for killing a faction member.");
													}
													else if (__instance.factionless)
													{
														Globals.changeReputation(5UL, -5, "for killing civilians on a trading station.");
														Globals.changeReputation(3UL, -5, "for killing civilians on a trading station.");
													}
												}
											}
										}
									}

								}
							}
						}
					}

					if (__instance.isPlayer)
					{
						if (__instance.currentCosm != null && __instance.currentCosm.crew != null && __instance.currentCosm.ship != null && __instance.currentCosm.ship.id != PLAYER.currentGame.homeBaseId)
						{
							for (int i = 0; i < __instance.currentCosm.crew.Values.Count; i++)//foreach (Crew crew in __instance.currentCosm.crew.Values)
							{
								var crew = __instance.currentCosm.crew.Values.ToList()[i];
								bool flag4 = crew.faction != __instance.faction && crew.state != CrewState.dead && Vector2.DistanceSquared(crew.position, __instance.position) < 600f * 600f && crew.faction != PLAYER.avatar.faction;
								if (flag4)
								{
									bool isStation = __instance.currentCosm.isStation;
									if (isStation)
									{
										bool flag5 = PLAYER.currentWorld != null && PLAYER.currentWorld.economy != null && __instance.currentCosm.ship != null;
										if (flag5)
										{
											foreach (EconomyNode economyNode in PLAYER.currentWorld.economy.nodes)
											{
												bool flag6 = economyNode.id == __instance.currentCosm.ship.id;
												if (flag6)
												{
													if (crew.team.threats.Contains(__instance.faction))
														crew.team.threats.Remove(__instance.faction);  // hostile npcs will become friendly again after they see player die
												}
											}
										}
									}

								}
							}
						}
					}
				}
			}

		}

		[HarmonyPatch(typeof(Crew), "update")] //updatig floaty text of the Crew
		public class Crew__update
		{

			[HarmonyPostfix]
			private static void Postfix(Crew __instance, bool ___canBeSeen, float elapsed)
			{
				checked
				{
					if (___canBeSeen)
					{
						__instance.updateFloatyText();
					}
				}
				/* all player faction crew members get assigned here to player team in vanilla
				this.reportTimer += elapsed;
				if (this.reportTimer > 0.5f)
				{
					this.reportTimer = 0f;
					if (!this.isPlayer && this.faction == 2UL && PLAYER.currentGame != null && this.name != null)
					{
						PLAYER.currentGame.team.reportStatus(this);
					}
				}
				*/
			}
		}

		[HarmonyPatch(typeof(Phase1EndQuest), "test")] //SSC Shipyard quest, making npcs on the station hostile after alarm
		public class Phase1EndQuest__test
		{

			[HarmonyPostfix]
			private static void Postfix(Phase1EndQuest __instance)
			{
				checked
				{
					if (PROCESS_REGISTER.currentCosm != null && PROCESS_REGISTER.currentCosm.klaxonOverride == true)
					{
						if (PLAYER.avatar.currentCosm != null && PLAYER.avatar.currentCosm.crew != null)
						{


							for (int i = 0; i < PLAYER.avatar.currentCosm.crew.Values.Count; i++)
							{
								var crew = PLAYER.avatar.currentCosm.crew.Values.ToList()[i];
								bool flag4 = crew.faction != PLAYER.avatar.faction && crew.state != CrewState.dead && Vector2.DistanceSquared(crew.position, PLAYER.avatar.position) < 100000f * 100000f && !crew.isPlayer;
								if (flag4)
								{
									bool isStation = PLAYER.avatar.currentCosm.isStation;
									if (isStation)
									{
										bool flag5 = PLAYER.currentWorld != null && PLAYER.currentWorld.economy != null && PLAYER.avatar.currentCosm.ship != null;
										if (flag5)
										{
											for (int j = 0; j < PLAYER.currentWorld.economy.nodes.Count; j++)
											{
												var economyNode = PLAYER.currentWorld.economy.nodes[j];
												bool flag6 = economyNode.id == PLAYER.avatar.currentCosm.ship.id;
												if (flag6)
												{
													crew.team.threats.Add(PLAYER.avatar.faction);
													crew.attackTarget(PLAYER.avatar);
												}
											}
										}
									}

								}
							}
						}
					}
				}

			}
		}

		[HarmonyPatch(typeof(RespawnTracker), "test")]
		public class RespawnTracker__test // making player drop items on death
		{

			[HarmonyPrefix]
			private static void Prefix(bool ___waiting)
			{
				if (Globals.initialized)
				{
					if (!___waiting && PLAYER.avatar.state == CrewState.dead && HWCONFIG.DropItemsOnDeath)
					{

						if (Globals.eventflags[GlobalFlag.Sige1EventActive]) //special condition for siege event
						{
							Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] = true;
							HWBaseSiegeEvent.targetModule = null;
						}

						if (PLAYER.avatar.inventory != null)
						{
							var inventory = PLAYER.avatar.inventory;
							if (!Globals.eventflags[GlobalFlag.Sige1EventActive])
							{
								foreach (InventoryItem inventoryItem in inventory)
								{
									if (PLAYER.avatar.currentCosm != null)
									{
										if (PLAYER.avatar.currentCosm.ship != null)
										{
											if (inventoryItem != null && inventoryItem.type != InventoryItemType.mining_laser && inventoryItem.type != InventoryItemType.repair_gun && inventoryItem.type != InventoryItemType.fire_extinguisher && inventoryItem.type != InventoryItemType.fire_extinguisher_2)
												PLAYER.avatar.currentCosm.ship.threadDumpCargo(inventoryItem);
										}
										else
										{
											if (inventoryItem != null && inventoryItem.type != InventoryItemType.mining_laser && inventoryItem.type != InventoryItemType.repair_gun && inventoryItem.type != InventoryItemType.fire_extinguisher && inventoryItem.type != InventoryItemType.fire_extinguisher_2)
												PLAYER.currentSession.cargo.Add(new CargoPod(inventoryItem, PLAYER.avatar.position + RANDOM.squareVector(20f)));
										}
									}
									else
									{
										if (inventoryItem != null && inventoryItem.type != InventoryItemType.mining_laser && inventoryItem.type != InventoryItemType.repair_gun && inventoryItem.type != InventoryItemType.fire_extinguisher && inventoryItem.type != InventoryItemType.fire_extinguisher_2)
											PLAYER.currentSession.cargo.Add(new CargoPod(inventoryItem, PLAYER.avatar.position + RANDOM.squareVector(20f)));
									}

								}
							}
							else //special condition for siege event
							{
								for (int i = 4; i < inventory.Length; i++)
								{
									InventoryItem inventoryItem = inventory[i];
									if (inventoryItem != null && inventoryItem.type != InventoryItemType.gun)
									{
										PLAYER.currentSession.cargo.Add(new CargoPod(inventoryItem, PLAYER.avatar.position + RANDOM.squareVector(20f)));
									}
								}
							}
						}
					}
				}
			}
		}


		[HarmonyPatch(typeof(Respawning), "updateInput")]
		public class Respawning__updateInput // making player drop items on manual respawning (clearing inventory after items dropped)
		{

			[HarmonyPrefix]
			private static void Prefix(Respawning __instance, ref Vector2 ___mousePos, float ___wasteTimer, List<Clickable> ___buttons, MouseState ___oldMouse)
			{
				if (Globals.initialized)
				{
					if (HWCONFIG.DropItemsOnDeath)
					{
						MouseState state2 = Mouse.GetState();
						Rectangle test = new Rectangle(state2.X, state2.Y, 1, 1);
						___mousePos.X = (float)state2.X;
						___mousePos.Y = (float)state2.Y;
						bool flag = ___wasteTimer >= 10f;
						checked
						{
							if (flag)
							{
								foreach (Clickable clickable in ___buttons)
								{
									clickable.hover(test);
								}
								bool flag2 = state2.LeftButton == ButtonState.Released && ___oldMouse.LeftButton == ButtonState.Pressed;
								if (flag2)
								{
									foreach (Clickable clickable2 in ___buttons)
									{
										bool flag3 = test.Intersects(clickable2.region);
										if (flag3)
										{
											//if (clickable2.action == 0)
											//{
											if (PLAYER.avatar.inventory != null && PLAYER.avatar.currentCosm != null)
											{
												var inventory = PLAYER.avatar.inventory;
												if (!Globals.eventflags[GlobalFlag.Sige1EventActive])
												{
													foreach (InventoryItem inventoryItem in inventory)
													{

														if (inventoryItem != null && inventoryItem.type != InventoryItemType.mining_laser && inventoryItem.type != InventoryItemType.repair_gun && inventoryItem.type != InventoryItemType.fire_extinguisher && inventoryItem.type != InventoryItemType.fire_extinguisher_2)
														{
															PLAYER.avatar.inventory[Array.IndexOf(inventory, inventoryItem)] = null;
														}

													}
												}
												else  //special condition for siege event
												{
													//PLAYER.avatar.heldItem = null;
													for (int i = 4; i < inventory.Length; i++)
													{
														InventoryItem inventoryItem = inventory[i];
														if (inventoryItem != null && inventoryItem.type != InventoryItemType.gun)
														{
															PLAYER.avatar.inventory[Array.IndexOf(inventory, inventoryItem)] = null;
														}
													}
												}
											}
											//}

										}
									}
								}
							}
						}
					}
				}
			}

		}

		[HarmonyPatch(typeof(LoadingWorldScreen), "Update")]
		public class LoadingWorldScreen_Update                                                          // creating save database for the mod
		{

			[HarmonyPrefix]
			private static void Prefix(LoadingWorldScreen __instance, bool ___rendered)
			{
				checked
				{
					switch (__instance.mode)
					{
						case LoadScreenType.database_continue:
							{
								if (___rendered && MOD_DATA.modCon == null)
								{
									if (PLAYER.currentWorld == null)
									{
										string savedir = Directory.GetCurrentDirectory() + "\\worldsaves";
										if (!Directory.Exists(savedir))
										{
											Directory.CreateDirectory(savedir);
										}										
										if (!File.Exists(savedir + "\\" + __instance.loadSelectName + "_HW_MODSAVE" + ".sqlite"))
										{
											SQLiteConnection.CreateFile(savedir + "\\" + __instance.loadSelectName + "_HW_MODSAVE" + ".sqlite");
											MOD_DATA.modCon = new SQLiteConnection("Data Source=" + savedir + "\\" + __instance.loadSelectName + "_HW_MODSAVE" + ".sqlite;Version=3;");
											MOD_DATA.modCon.Open();
											MOD_DATA.createAllTables();
										}
										else
										{

											MOD_DATA.modCon = new SQLiteConnection("Data Source=" + savedir + "\\" + __instance.loadSelectName + "_HW_MODSAVE" + ".sqlite;Version=3;");
											MOD_DATA.modCon.Open();
											MOD_DATA.createAllTables();											
										}
										if (!File.Exists(savedir + "\\" + __instance.loadSelectName + "_HW_MODSAVE_config" + ".ini"))
										{
											var ConfigPath = savedir + "\\" + __instance.loadSelectName + "_HW_MODSAVE_config" + ".ini";
											using (var configWriter = File.CreateText(ConfigPath))
											{/*
												configWriter.WriteLine($"Max Active Encounters:2");
												configWriter.WriteLine($"Encounter Frequency Multiplier:1.0");
												configWriter.WriteLine($"Global Difficulty Multiplier:1.0");
												configWriter.WriteLine($"Drop Items On Death:True");
												configWriter.WriteLine($"Max Monster Level:35");	
												*/
												configWriter.WriteLine($"Max Active Encounters:{HWCONFIG.MaxInterruptions}");
												configWriter.WriteLine($"Encounter Frequency Multiplier:{HWCONFIG.InterruptionFrequency}");
												configWriter.WriteLine($"Global Difficulty Multiplier:{HWCONFIG.GlobalDifficulty}");
												configWriter.WriteLine($"Drop Items On Death:{HWCONFIG.DropItemsOnDeath}");
												configWriter.WriteLine($"Max Monster Level:{HWCONFIG.MaxMonsterLevel}");
											}
											HWCONFIG.Init(ConfigPath);
											HWCONFIG.LoadConfig();
										}
										else
										{
											HWCONFIG.Init(savedir + "\\" + __instance.loadSelectName + "_HW_MODSAVE_config" + ".ini");
											HWCONFIG.LoadConfig();
										}
									}
									else
									{
									}
								}
								break;
							}

					}
				}
			}

			[HarmonyPostfix]
			private static void Postfix(LoadingWorldScreen __instance, bool ___rendered)         // loading saved mod data
			{
				checked
				{
					switch (__instance.mode)
					{
						case LoadScreenType.database_continue:
							{
								if (___rendered && MOD_DATA.loaded == false)
								{
									if (PLAYER.currentWorld != null)
									{
										Globals.Initialize();
										MOD_DATA.updateSaveFile();
										MOD_DATA.loadModData();
									}
								}
								HWSPAWNMANAGER.TestDespawnSavedShips(); // testing if spawned ships have to be cleared from current game
								break;
							}
					}
				}
			}
		}

		[HarmonyPatch(typeof(LoadingWorldScreen), "newGame")]
		public class LoadingWorldScreen_newGame                                                          // creating save database for the mod
		{

			[HarmonyPrefix]
			private static void Prefix(LoadingWorldScreen __instance)
			{
				Globals.Initialize();
				string savedir = Directory.GetCurrentDirectory() + "\\worldsaves";
				if (!Directory.Exists(savedir))
				{
					Directory.CreateDirectory(savedir);
				}
				if (!File.Exists(savedir + "\\" + __instance.loadSelectName + "_HW_MODSAVE" + ".sqlite"))
				{
					SQLiteConnection.CreateFile(savedir + "\\" + __instance.loadSelectName + "_HW_MODSAVE" + ".sqlite");
					MOD_DATA.modCon = new SQLiteConnection("Data Source=" + savedir + "\\" + __instance.loadSelectName + "_HW_MODSAVE" + ".sqlite;Version=3;");
					MOD_DATA.modCon.Open();
					MOD_DATA.createAllTables();
				}
				else
				{

					MOD_DATA.modCon = new SQLiteConnection("Data Source=" + savedir + "\\" + __instance.loadSelectName + "_HW_MODSAVE" + ".sqlite;Version=3;");
					MOD_DATA.modCon.Open();
					MOD_DATA.createAllTables();
				}
				if (!File.Exists(savedir + "\\" + __instance.loadSelectName + "_HW_MODSAVE_config" + ".ini"))
				{
					var ConfigPath = savedir + "\\" + __instance.loadSelectName + "_HW_MODSAVE_config" + ".ini";
					using (var configWriter = File.CreateText(ConfigPath))
					{   /*
						configWriter.WriteLine($"Max Active Encounters:2");
						configWriter.WriteLine($"Encounter Frequency Multiplier:1.0");
						configWriter.WriteLine($"Global Difficulty Multiplier:1.0");
						configWriter.WriteLine($"Drop Items On Death:True");
						configWriter.WriteLine($"Max Monster Level:35");
						*/
						configWriter.WriteLine($"Max Active Encounters:{HWCONFIG.MaxInterruptions}");
						configWriter.WriteLine($"Encounter Frequency Multiplier:{HWCONFIG.InterruptionFrequency}");
						configWriter.WriteLine($"Global Difficulty Multiplier:{HWCONFIG.GlobalDifficulty}");
						configWriter.WriteLine($"Drop Items On Death:{HWCONFIG.DropItemsOnDeath}");
						configWriter.WriteLine($"Max Monster Level:{HWCONFIG.MaxMonsterLevel}");
					}
					HWCONFIG.Init(ConfigPath);
					HWCONFIG.LoadConfig();
				}
				else
				{
					HWCONFIG.Init(savedir + "\\" + __instance.loadSelectName + "_HW_MODSAVE_config" + ".ini");
					HWCONFIG.LoadConfig();
				}
			}
		}

		[HarmonyPatch(typeof(WorldDatabase), "savePuppetList")]
		public class WorldDatabase_savePuppetList                       //saving mod data this method runs off the main thread in vanilla
		{

			[HarmonyPostfix]
			private static void Postfix()
			{
				MOD_DATA.writeModData();
			}
		}

		[HarmonyPatch(typeof(WorldDatabase), "update")]             //cleaning up before shutdown
		public class WorldDatabase_update
		{

			[HarmonyPostfix]
			private static void Postfix(WorldDatabase __instance)
			{
				if (__instance.shuttingDown && MOD_DATA.modCon != null)
				{
					MOD_DATA.modCon.Close();
					MOD_DATA.loaded = false;
					MOD_DATA.modCon = null;
					Globals.Interruptions = null;
					Globals.Interruptionbag = null;
					Globals.GlobalDespawnQueue = null;
					Globals.GlobalShipRemoveQueue = null;
					Globals.mostExpensiveDesigndifficulty = 0;
					Globals.mostExpensiveBuildableDesigndifficulty = 0;
					Globals.currentshipdifficulty = -1;
					Globals.offer = null;
					Globals.demand = null;
					Globals.globalints.Clear();
					Globals.eventflags.Clear();
					Globals.globaldoubles.Clear();
					Globals.globalstrings.Clear();
					Globals.initialized = false;
				}
			}
		}

		[HarmonyPatch(typeof(Ship), "checkCosm")]  //reducing max speed of player ship while it is being ambushed
		public class Ship_checkCosm
		{

			[HarmonyPostfix]
			private static void Postfix(Ship __instance)
			{
				if (__instance.Getinterrupted() && HWCONFIG.GlobalDifficulty > 0)
				{
					__instance.forwardAccel *= Math.Min((1f / HWCONFIG.GlobalDifficulty) * 0.34f, 2f);
					__instance.topSpeed *= Math.Min((1f / HWCONFIG.GlobalDifficulty) * 0.34f, 2f);
				}
			}
		}


		[HarmonyPatch(typeof(OneAgent), "setupConversations")] // adding dialogue option for One calling friendly Pirates to deliver a free ship for player
		public class OneAgent_setupConversations
		{

			[HarmonyPostfix]
			private static void Postfix(OneAgent __instance, Action[] ___conversations)
			{
				HWOneAgent.instance = __instance;
				___conversations[1] = new Action(HWOneAgent.coreDialogueLoop);
			}
		}

		[HarmonyPatch(typeof(CoOpSpRpG.Console), "givePlayerControl")] // adding dialogue option for One calling friendly Pirates to deliver a free ship for player
		public class Console_givePlayerControl
		{

			[HarmonyPrefix]
			private static void Prefix(CoOpSpRpG.Console __instance)
			{
				if (Globals.initialized)
				{
					if (PLAYER.currentSession.GetType() == typeof(BattleSessionSP))
					{
						if (Globals.eventflags[GlobalFlag.Sige1EventActive] && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId)
						{
							PLAYER.currentShip.scanRange = CONFIG.minViewDist;
							PLAYER.currentShip.signitureRadius = CONFIG.minViewDist;
							PLAYER.currentShip.tempView = 1f;
							PLAYER.currentShip.tempFireRate = 1f;
							PLAYER.currentShip.tempGunAccuracy = 1f;
							PLAYER.currentShip.tempBulletDuration = 1f;
							PLAYER.currentShip.tempBulletSpeed = 1f;
							PLAYER.currentShip.tempBulletDamageMod = 1f;
							PLAYER.currentShip.tempMagazineSize = 1f;
							PLAYER.currentShip.tempMissileHard = 1f;
							PLAYER.currentShip.tempTurReload = 1f;
							PLAYER.currentShip.tempTurTraverse = 1f;
							PLAYER.currentShip.faction = 2UL;													
							__instance.group = 0;
							if (PLAYER.currentShip.turrets == null || __instance.turrets == null || PLAYER.currentShip.turrets.ToList().TrueForAll(Element => Element == null)) // new turrets assigned only once. Or assign new turrets to current console
							{
								PLAYER.currentShip.turrets = TURRET_BAG.makeTurrets(new TurretType[] { TurretType.s_b_rocket, TurretType.s_b_rocket, TurretType.s_b_rocket, TurretType.s_b_rocket }); //give station weapons
								__instance.turrets = PLAYER.currentShip.turrets;
							}
							if (PLAYER.currentShip.turrets != null)
							{
								foreach (Turret t in PLAYER.currentShip.turrets)
								{
									if (t != null)
									{
										t.ship = PLAYER.currentShip;
									}
								}
							}
							//__instance.turrets = PLAYER.currentShip.turrets;
							//give them ammo
							if (PLAYER.currentShip.data == null)
							{
								PLAYER.currentShip.data = new CosmMetaData();
							}
							PLAYER.currentShip.data.reload = true;
						}
						else if (PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId)
						{
							PLAYER.currentShip.scanRange = 0;
							PLAYER.currentShip.signitureRadius = 0;
							PLAYER.currentShip.tempView = 0;
							PLAYER.currentShip.tempFireRate = 0f;
							PLAYER.currentShip.tempGunAccuracy = 0f;
							PLAYER.currentShip.tempBulletDuration = 0f;
							PLAYER.currentShip.tempBulletSpeed = 0f;
							PLAYER.currentShip.tempBulletDamageMod = 0f;
							PLAYER.currentShip.tempMagazineSize = 0f;
							PLAYER.currentShip.tempMissileHard = 0f;
							PLAYER.currentShip.tempTurReload = 0f;
							PLAYER.currentShip.tempTurTraverse = 0f;
						}
						if ((Globals.eventflags[GlobalFlag.PiratesCalledForShip] || (Globals.Interruptionbag != null && !Globals.Interruptionbag.Values.ToList().TrueForAll(element => element.templateUsed != InterruptionType.friendly_pirates_call))) && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId)
						{
							PLAYER.currentShip.scanRange = CONFIG.minViewDist;
							PLAYER.currentShip.signitureRadius = CONFIG.minViewDist;
							PLAYER.currentShip.tempView = 1f;
							__instance.group = 0;
							foreach (var interruption in Globals.Interruptionbag)
							{
								if (interruption.Value.templateUsed == InterruptionType.friendly_pirates_call)
								{
									return;
								}
							}
							HWSPAWNMANAGER.addInterruption(new Interruption(InterruptionType.friendly_pirates_call, PLAYER.currentShip.position, PLAYER.currentShip.grid));
						}
						else if (PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId && !Globals.eventflags[GlobalFlag.Sige1EventActive])
						{
							PLAYER.currentShip.scanRange = 0;
							PLAYER.currentShip.signitureRadius = 0;
							PLAYER.currentShip.tempView = 0;
						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(CoOpSpRpG.Console), "tryUse")] 
		public class Console_tryUse
		{

			[HarmonyPrefix]
			private static void Prefix(CoOpSpRpG.Console __instance, Crew user)
			{
				if (!user.isPlayer || (__instance.crew != null && __instance.crew.isPlayer)) // AI for ally taking control of a ship
				{
					if (__instance.crew == null || __instance.crew.state != CrewState.operating)
					{
						if (!user.isPlayer && user.faction == 2UL)
						{
							if(user.team?.goalType != null && user.team.goalType != ConsoleGoalType.escort && user.team.goalType != ConsoleGoalType.warp_jump && PLAYER.currentShip != null)
							{
								user.team.focus = PLAYER.currentShip.id;
								user.team.destination = default(Vector2);
								user.team.goalType = ConsoleGoalType.escort;
								user.team.ignoreAggression = true;
							}
							if(user.team?.goalType != null && user.team.goalType == ConsoleGoalType.escort && PLAYER.currentShip != null && user.team.focus != PLAYER.currentShip.id)
							{
								user.team.focus = PLAYER.currentShip.id;
								user.team.ignoreAggression = true;
							}
							if (user.conThoughts == null || (user.conThoughts != null && user.conThoughts.goalType != ConsoleGoalType.escort && user.conThoughts.goalType != ConsoleGoalType.warp_jump))
							{
								user.conThoughts = new ConsoleThought(user);
								user.conThoughts.goalType = ConsoleGoalType.escort;
								if (user.team != null)
								{ 
									user.team.ignoreAggression = true;
								}
							}
						}
					}
				}
				if (user == PLAYER.avatar || user.isPlayer) //updating current ship cost for difficulty ajustments
				{
					if (Globals.initialized)
					{
						Task.Run(async () =>
						{
							if (PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && !PLAYER.currentShip.cosm.isStation)
							{
								Globals.currentshipdifficulty = Globals.DifficultyFromCost((int)Hull.getCost(PLAYER.currentShip.botD));
							}
						});
					}
				}
			}
		}
		/*
		[HarmonyPatch(typeof(ConsoleThought), "navChooseTask")] // AI for ally escorting player
		public class ConsoleThought_navChooseTask
		{

			[HarmonyPostfix]
			private static void Postfix(ConsoleThought __instance, BattleSession session, Ship ship, CoOpSpRpG.Console console, Vector2 ___desiredTargetOffset)
			{
				if (__instance.isRunningAway)
				{
					return;
				}
				if (__instance.goalType == ConsoleGoalType.escort)
				{
					if (__instance.owner.team != null)
					{
						if (session.allShips.ContainsKey(__instance.owner.team.focus))
						{
							if(session.allShips[__instance.owner.team.focus].boostStage >= 1 && ship.boostStage < 1)
							{
								//ship.startBoost();
							}
							if (session.allShips[__instance.owner.team.focus].boostStage < 1 && ship.boostStage >= 1)
							{
								//ship.endBoost();
							}
						}
					}
				}
			}
		}
		*/

		[HarmonyPatch(typeof(ConsoleThought), "plotEscapeSession")] // AI for ally escorting player
		public class ConsoleThought_plotEscapeSession
		{

			[HarmonyPostfix]
			private static void Postfix(ConsoleThought __instance, Ship ship)
			{
				if(PLAYER.currentShip != null && __instance.team.focus == PLAYER.currentShip.id)
				{
					__instance.desiredDestination = new Vector2((float)((PLAYER.currentShip.grid.X - ship.grid.X) * 200000), (float)((PLAYER.currentShip.grid.Y - ship.grid.Y) * 200000));
					__instance.desiredDestination += PLAYER.currentShip.position;
				}
			}
		}

		/*
		[HarmonyPatch(typeof(ShipAIManager), "stepAI")] // debuging AI
		public class ShipAIManager_stepAI
		{

			[HarmonyPrefix]
			private static void Prefix()
			{
				CONFIG.debugAI = true;
			}
		}
		*/
		
		[HarmonyPatch(typeof(ConsoleThought), "navUpdate")] // AI for ally escorting player
		public class ConsoleThought_navUpdate
		{

			[HarmonyPostfix]
			private static void Postfix(ConsoleThought __instance, ConcurrentQueue<CrewInteriorAlert> interiorAlerts, BattleSession session, Ship ship, CoOpSpRpG.Console console, float elapsed, ref Vector2 ___desiredTargetOffset)
			{
				if (session != null)
				{
					if (__instance.state == ConsoleState.attacking || __instance.state == ConsoleState.running)
					{
						if (__instance.team != null && __instance.faction == 2UL && PLAYER.currentShip != null && ship != PLAYER.currentShip)
						{
							if (session.allShips.ContainsKey(PLAYER.currentShip.id))
							{
								if (__instance.team.ignoreAggression == true)
								{
									__instance.state = ConsoleState.escorting;
									__instance.goalType = ConsoleGoalType.escort;
									__instance.team.focus = PLAYER.currentShip.id;
								}
								if (ship.boostStage >= 1 &&
								Vector2.DistanceSquared(PLAYER.currentShip.position, ship.position) <
								((PLAYER.currentShip.signitureRadius + (ship.scanRange / 1000f)) * PLAYER.currentShip.tempVis * ship.tempView) * ((PLAYER.currentShip.signitureRadius + (ship.scanRange / 1000f)) * PLAYER.currentShip.tempVis * ship.tempView)
								)
								{
									if (__instance.team.ignoreAggression == false)
									{
										ship.endBoost();

									}

								}
								else if (ship.boostStage >= 1 || (PLAYER.currentShip.boostStage >= 1 && ship.boostStage < 1))
								{
									__instance.state = ConsoleState.escorting;
									__instance.goalType = ConsoleGoalType.escort;
									__instance.team.focus = PLAYER.currentShip.id;
									__instance.team.ignoreAggression = true;
								}
							}
						}
					}

					if (__instance.state == ConsoleState.escorting || __instance.state == ConsoleState.idle)
					{
						if (__instance.team != null && __instance.faction == 2UL && PLAYER.currentShip != null && ship != PLAYER.currentShip)
						{
							if (__instance.team.ignoreAggression == false)
							{
								__instance.goalType = ConsoleGoalType.kill_enemies_escort;
								//__instance.goalType = ConsoleGoalType.kill_target
							}
							else if (__instance.team.focus != PLAYER.currentShip.id)
							{
								__instance.team.focus = PLAYER.currentShip.id;
							}
							if (session.allShips.ContainsKey(__instance.team.focus))
							{
								if (__instance.team.focus != PLAYER.currentShip.id && (session.allShips[__instance.team.focus].faction == ulong.MaxValue || (session.allShips[__instance.team.focus].cosm?.crew?.Values?.First()?.team?.threats != null && !session.allShips[__instance.team.focus].cosm.crew.Values.First().team.threats.Contains(2UL))))
								{
									__instance.team.focus = PLAYER.currentShip.id;
								}
								if (session.allShips[__instance.team.focus].boostStage >= 1 && ship.boostStage < 1)// && ship.engineEnergy > 0)
								{
									ship.startBoost();
									__instance.team.ignoreAggression = true;
								}
								if (Vector2.DistanceSquared(session.allShips[__instance.team.focus].position, ship.position) <
								(ship.signitureRadius + (session.allShips[__instance.team.focus].scanRange / 1000f)) * ship.tempVis * session.allShips[__instance.team.focus].tempView
								*
								((ship.signitureRadius + (session.allShips[__instance.team.focus].scanRange / 1000f)) * ship.tempVis * session.allShips[__instance.team.focus].tempView)
								)
								{
									if(session.allShips[__instance.team.focus].boostStage < 1 && ship.boostStage >= 1)
									{ 
										ship.endBoost();
									}
									
								}
								else 
								{
									if (ship.boostStage < 1)
									{ 
										ship.startBoost();
										__instance.team.ignoreAggression = true;
									}
								}

							}
							else if (!session.allShips.ContainsKey(PLAYER.currentShip.id) && PLAYER.currentShip.GetType() != typeof(Station) && (__instance.team.focus == PLAYER.currentShip.id || __instance.faction == 2UL))
							{
								if (ship.boostStage < 1)
								{
									ship.startBoost();
									__instance.team.ignoreAggression = true;
								}
								if (PLAYER.currentShip.boostStage >= 1 && ship.boostStage >= 1)
								{
									if (!PLAYER.currentShip.GetConvoy().ContainsKey(ship.id))
									{
										PLAYER.currentShip.GetConvoy().Add(ship.id, ship.grid);
									}
									else
									{
										PLAYER.currentShip.GetConvoy()[ship.id] = ship.grid;
									}
								}
								if (PLAYER.currentShip.boostStage >= 1 && ship.boostStage >= 1)
								{
									__instance.desiredDestination = new Vector2((float)((PLAYER.currentShip.grid.X - ship.grid.X) * 200000), (float)((PLAYER.currentShip.grid.Y - ship.grid.Y) * 200000));
									__instance.desiredDestination += PLAYER.currentShip.position;
								}

							}
							else if (PLAYER.currentShip.GetType() == typeof(Station) && (__instance.team.focus == PLAYER.currentShip.id || __instance.faction == 2UL) && __instance.team.goalType != ConsoleGoalType.patrol)
							{
								__instance.team.ignoreAggression = true;
								__instance.team.goalType = ConsoleGoalType.patrol;
								__instance.team.destinationGrid = PLAYER.currentShip.grid;
								__instance.team.destination = PLAYER.currentShip.position;
							}
						}
					}
					else if (PLAYER.currentShip != null && __instance.faction == 2UL && __instance.state != ConsoleState.escorting && !session.allShips.ContainsKey(PLAYER.currentShip.id) && PLAYER.currentShip.GetType() != typeof(Station))
					{
						//ship.position = PLAYER.currentShip.position + RANDOM.squareVector(CONFIG.minViewDist);
						if (__instance.team != null && __instance.team.goalType == ConsoleGoalType.warp_jump)
						{
							return;
						}
						__instance.state = ConsoleState.escorting;
						__instance.goalType = ConsoleGoalType.escort;
						if (__instance.team != null)
						{
							__instance.team.focus = PLAYER.currentShip.id;
							__instance.team.ignoreAggression = true;
						}
					}
					else if (__instance.team != null && __instance.faction == 2UL && PLAYER.currentShip != null && PLAYER.currentShip.GetType() == typeof(Station) && __instance.team.goalType != ConsoleGoalType.patrol && __instance.team.goalType != ConsoleGoalType.warp_jump)
					{
						__instance.team.ignoreAggression = true;
						__instance.team.goalType = ConsoleGoalType.patrol;
						__instance.team.destinationGrid = PLAYER.currentShip.grid;
						__instance.team.destination = PLAYER.currentShip.position;
					}
				}
			}
		}

		
		[HarmonyPatch(typeof(Ship), "endBoost")] 
		public class Ship_endBoost
		{

			[HarmonyPostfix]
			private static void Postfix(Ship __instance)
			{
				if (PLAYER.currentSession == null || PLAYER.currentWorld == null || PLAYER.currentSession.GetType() == typeof(BattleSessionG) || PLAYER.currentSession.GetType() == typeof(BattleSessionSA) || PLAYER.currentSession.GetType() == typeof(BattleSessionTA))
				{
					return;
				}
				if (PLAYER.currentShip != null && __instance.id == PLAYER.currentShip.id)
				{			
					try
					{
						foreach (var shipId in __instance.GetConvoy().Keys)
						{
							if (!PLAYER.currentSession.recurseIterateShipExists(shipId))
							{
								if (PLAYER.currentSession.neighbors.ToList().TrueForAll(session => session != null && !session.allShips.ContainsKey(shipId) || session == null))
								{
									var weakrefsession = new WeakReference(PLAYER.currentWorld.getSession(PLAYER.currentShip.GetConvoy()[shipId]));
									if (weakrefsession.IsAlive)
									{
										var session = weakrefsession.Target as BattleSession;
										if (session.allShips.TryGetValue(shipId, out Ship ship) && ship != null)
										{
											ship.endBoost();
											ship.position = PLAYER.currentShip.position + RANDOM.squareVector(CONFIG.minViewDist);
											session.allShips.Remove(ship.id);
											ship.grid.X = PLAYER.currentShip.grid.X;
											ship.grid.Y = PLAYER.currentShip.grid.Y;
											ship.rotationAngle = SCREEN_MANAGER.VectorToAngle(PLAYER.currentShip.position);
											if (ship.cosm?.crew?.Values?.First()?.team != null && ship.cosm.alive)
											{
												ship.cosm.crew.Values.First().team.destination = ship.position + PLAYER.currentShip.position;
											}
											PLAYER.currentSession.addLocalShip(ship, SessionEntry.flyin);
										}
									}
								}
								else
								{
									var weakrefsession = new WeakReference(PLAYER.currentWorld.getSession(PLAYER.currentShip.GetConvoy()[shipId]));
									if (weakrefsession.IsAlive)
									{
										var session = weakrefsession.Target as BattleSession;
										if (session.allShips.TryGetValue(shipId, out Ship ship) && ship != null)
										{
											ship.endBoost();
											ship.position = PLAYER.currentShip.position + RANDOM.squareVector(CONFIG.minViewDist);
											session.allShips.Remove(ship.id);
											ship.grid.X = PLAYER.currentShip.grid.X;
											ship.grid.Y = PLAYER.currentShip.grid.Y;
											ship.rotationAngle = SCREEN_MANAGER.VectorToAngle(PLAYER.currentShip.position);
											if (ship.cosm?.crew?.Values?.First()?.team != null && ship.cosm.alive)
											{
												ship.cosm.crew.Values.First().team.destination = ship.position + PLAYER.currentShip.position;
											}
											PLAYER.currentSession.addLocalShip(ship, SessionEntry.flyin);
										}
										else
										{
											foreach (var crew in PLAYER.currentGame.team.crew)
											{
												if (crew?.currentCosm?.ship != null && crew.currentCosm.ship.id == shipId)
												{
													var weakrefsession2 = new WeakReference(PLAYER.currentWorld.getSession(PLAYER.currentShip.GetConvoy()[shipId]));
													if (weakrefsession2.IsAlive)
													{
														var session2 = weakrefsession2.Target as BattleSession;
														if (session2.allShips.TryGetValue(shipId, out Ship ship3) && ship3 != null)
														{
															ship3.endBoost();
															ship3.position = PLAYER.currentShip.position + RANDOM.squareVector(CONFIG.minViewDist);
															session2.allShips.Remove(ship3.id);
															ship3.grid.X = PLAYER.currentShip.grid.X;
															ship3.grid.Y = PLAYER.currentShip.grid.Y;
															ship3.rotationAngle = SCREEN_MANAGER.VectorToAngle(PLAYER.currentShip.position);
															if (ship3.cosm?.crew?.Values?.First()?.team != null && ship3.cosm.alive)
															{
																ship3.cosm.crew.Values.First().team.destination = ship3.position + PLAYER.currentShip.position;
															}
															PLAYER.currentSession.addLocalShip(ship3, SessionEntry.flyin);
														}
														break;
													}
												}
											}
										}
									}
								}
							}
							else
							{
								 if(!PLAYER.currentSession.allShips.Keys.Contains(shipId))
								 { 
									foreach (Ship ship2 in PLAYER.currentSession.recurseIterateAllShips())
									{
										if (ship2.id == shipId && ship2.boostStage < 1)
										{
											ship2.startBoost();
										}
									}
								 }
								 else
								 {
									if (PLAYER.currentSession.allShips[shipId].boostStage < 1 && Vector2.DistanceSquared(PLAYER.currentShip.position, PLAYER.currentSession.allShips[shipId].position) <
										   (PLAYER.currentSession.allShips[shipId].signitureRadius + (PLAYER.currentShip.scanRange / 1000f)) * PLAYER.currentSession.allShips[shipId].tempVis * PLAYER.currentShip.tempView
										   *
										   ((PLAYER.currentSession.allShips[shipId].signitureRadius + (PLAYER.currentShip.scanRange / 1000f)) * PLAYER.currentSession.allShips[shipId].tempVis * PLAYER.currentShip.tempView)
										   )
									{
										PLAYER.currentSession.allShips[shipId].startBoost();
									}
								}
							}
						}
					}
					catch
					{}
				}
			}
		}

		[HarmonyPatch(typeof(HailAnimation), "smallTalkLoop")] // adding options to hail dialogue
		public class HailAnimation_smallTalkLoop
		{

			[HarmonyPrefix]
			private static void Prefix(DialogueTree lobby, Crew ___representative, List<ResponseImmediateAction> ___results)
			{
				if (Globals.initialized)
				{
					if (lobby.text.Contains("BEEP"))
					{
						if (___representative.team?.threats != null && !___representative.team.threats.Contains(2UL))
						{
							lobby.text = "I trust you're not planning any trouble. What can I do for you, friend?";
						}
						else
						{
							lobby.text = "Now why would I want to talk to you?";
						}
						if (___representative.faction == 2UL)
						{
							lobby.text = "Aye captain, what can I do for you?";
						}
						if (___representative.faction == 4UL)
						{
							lobby.text = "Ho there, what's yer trouble?";
						}
						if (___representative.faction == 7UL)
						{
							lobby.text = "*static noise*";
						}
					}
					else
					{

					}
					if (Globals.eventflags[GlobalFlag.PiratesCalledForShip])
					{
						HWFriendlyPiratesCalledEvent.addHailDialogue(ref lobby, ___representative, ___results);
					}
					if (Globals.eventflags[GlobalFlag.Sige1EventActive])
					{
						HWBaseSiegeEvent.addHailDialogue(ref lobby, ___representative, ___results);
					}
					HWReputationOptions.addHailDialogue(ref lobby, ___representative, ___results);
				}
			}		

			[HarmonyPostfix]
			private static void Postfix(DialogueTree lobby, Crew ___representative, List<ResponseImmediateAction> ___results)
			{
				
				foreach (var branch in lobby.branches)
				{
					foreach (var secondbranch in branch.branches)
					{
						if(secondbranch.text.Contains("spawned"))
						{
							secondbranch.text = "I am living my dream. And my dream is to " + ___representative.team.goalType.ToString().Replace('_', ' ') + " !"; 
							if (___representative.faction == 7UL)
							{
								string text2 = "BEEP";
								for (int i = 0; i < Squirrel3RNG.Next(2); i++)
								{
									text2 = text2 + " BEEP";
								}
								text2 = text2 + " *static noise*";
								for (int i = 0; i < Squirrel3RNG.Next(2); i++)
								{
									text2 = text2 + " BEEP";
								}
								secondbranch.text = text2;
							}
						}
					}
					if (___representative.faction == 7UL)
					{
						string text = "BEEP";
						for (int i = 0; i < Squirrel3RNG.Next(2); i++)
						{
							text = text + " BEEP";
						}
						text = text + " *static noise*";
						for (int i = 0; i < Squirrel3RNG.Next(2); i++)
						{
							text = text + " BEEP";
						}
						branch.text = text;
					}
				}
				
			}
		}


		[HarmonyPatch(typeof(LogisticsScreenRev3), "doRightClick")] 
		public class LogisticsScreenRev3_doRightClick
		{

			[HarmonyPrefix]
			private static void Prefix(ref string opt, Ship ___selected)
			{
				if (Globals.initialized)
				{
					if (opt == "Scrap" && ___selected.ownershipHistory.Contains(3UL) && ___selected.ownershipHistory.Contains(8UL)) // preventing player from scrapping the bought friendly pirate ship in normal way.
					{
						___selected.performUndock(PLAYER.currentSession);
						foreach (InventoryItem inventoryItem in HWFriendlyPiratesCalledEvent.getRandomScrapLoot())
						{
							if (PLAYER.avatar.placeInFirstSlot(inventoryItem))
								PLAYER.currentShip.floatyText.Enqueue("+" + inventoryItem.stackSize.ToString() + " " + inventoryItem.toolTip.tip);
						}
						PLAYER.currentSession.despawnShip(___selected);
						opt = "";
					}
					if (opt != "" && !___selected.ownershipHistory.Contains(PLAYER.avatar.faction) && Globals.eventflags[GlobalFlag.Sige1EventActive]) //disable menu interaction with not owned ships while being under Siege
					{
						SCREEN_MANAGER.widgetChat.AddMessage("Access to ship systems denied.", MessageTarget.Ship);
						opt = "";
					}
				}
			}

			[HarmonyPostfix]
			private static void Postfix(ref string opt, Ship ___selected)
			{
				if (Globals.initialized)
				{
					if (opt == "Scrap" || opt == "Unload cargo")
					{
						Task.Run(async () =>
						{
							Globals.mostExpensiveBuildableDesigndifficulty = Globals.DifficultyFromCost(Globals.mostExpensiveBuildableDesign());
						});
					}
				}
			}
		}

		[HarmonyPatch(typeof(LogisticsScreenRev3), "TrySubstractResources")]
		public class LogisticsScreenRev3_TrySubstractResources
		{

			[HarmonyPostfix]
			private static void Postfix(bool __result)
			{
				if (Globals.initialized && __result)
				{
					Task.Run(async () =>
					{
						Globals.mostExpensiveBuildableDesigndifficulty = Globals.DifficultyFromCost(Globals.mostExpensiveBuildableDesign());
					});
				}
			}
		}

		[HarmonyPatch(typeof(DockSpot), "receiveCrew")]
		public class DockSpot_receiveCrew
		{
			[HarmonyPostfix]
			private static void Postfix(DockSpot __instance, Crew c)
			{
				if (c == PLAYER.avatar)
				{
					if (Globals.initialized)
					{
						Task.Run(async () =>
						{
							Globals.mostExpensiveBuildableDesigndifficulty = Globals.DifficultyFromCost(Globals.mostExpensiveBuildableDesign());
							if (PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && !PLAYER.currentShip.cosm.isStation)
							{
								Globals.currentshipdifficulty = Globals.DifficultyFromCost((int)Hull.getCost(PLAYER.currentShip.botD));
							}

						});
					}
				}
			}
		}


		[HarmonyPatch(typeof(GameFile), "update")] // update loop for quest events.
		public class GameFile_update
		{

			[HarmonyPostfix]
			private static void Postfix(float elapsed)
			{
				HWBaseSiegeEvent.test(elapsed); // checking dialogue trigger
			}
		}

		[HarmonyPatch(typeof(Turret), "tryFire")] //auto refill ammo of homestation guns if siege effect active
		public class Turret_tryFire
		{

			[HarmonyPrefix]
			private static void Prefix(Turret __instance, BattleSession session)
			{
				if (Globals.initialized)
				{
					if (session.GetType() == typeof(BattleSessionSP) && Globals.eventflags[GlobalFlag.Sige1EventActive] && __instance.ship.id == PLAYER.currentGame.homeBaseId)
					{
						__instance.energy = __instance._maxEnergy;
					}
				}
			}
		}

		[HarmonyPatch(typeof(WidgetJournal), "LoadQuests")] //managing questlog
		public class WidgetJournal_LoadQuests
		{
			[HarmonyPostfix]
			private static void Postfix(WidgetJournal __instance, JournalEntry ___focusedEntry, int ___journalTrackerWidth)
			{
				if (MOD_DATA.loaded)
				{
					BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
					JournalEntry entry = null;
					if (PLAYER.currentGame != null)
					{
						if (Globals.eventflags[GlobalFlag.Sige1EventActive])
						{
							JournalEntry journalEntry = (JournalEntry)__instance.ScrollCanvas.AddJournalEntry("Track base", SCREEN_MANAGER.white, 4, 2, ___journalTrackerWidth - 20, 56, SortType.vertical,
							new JournalEntry.ClickJournalEvent(__instance.DisplayDetails), new JournalEntry.ClickJournalMapEvent((inp) =>
							{
								JournalEntry jEntry = (JournalEntry)inp;
								if (jEntry.quest != null && jEntry.quest.tip != null && jEntry.trackButton != null)
								{
									jEntry.quest.tracked = !jEntry.quest.tracked;
								}
							}));
							__instance.entriesRef.Add(journalEntry);
							HWBaseSiegeEvent.SetupEntry(journalEntry);
							HWBaseSiegeEvent.getDistance(journalEntry);
							if (___focusedEntry != null && ___focusedEntry.quest == null && ___focusedEntry.name == journalEntry.name)
							{
								entry = ___focusedEntry;
							}
						}
					}
					var selectorCanvas = typeof(WidgetJournal).GetField("selectorCanvas", flags).GetValue(__instance) as Canvas;
					int selectorID = (int)typeof(Canvas).Assembly.GetType("CoOpSpRpG.SelectorCanvas", throwOnError: true).GetField("selectorID", flags).GetValue(typeof(WidgetJournal).GetField("selectorCanvas", flags).GetValue(__instance)); // accessing private field of an internal type via reflection
					var args = new object[] { selectorCanvas };
					switch (selectorID)
					{
						case 0:
							typeof(WidgetJournal).GetMethod("SortByName", flags, null, new Type[] { typeof(GuiElement) }, null).Invoke(__instance, args);
							break;
						case 1:
							typeof(WidgetJournal).GetMethod("SortByDistance", flags, null, new Type[] { typeof(GuiElement) }, null).Invoke(__instance, args);
							break;
						case 2:
							typeof(WidgetJournal).GetMethod("SortByName", flags, null, new Type[] { typeof(GuiElement) }, null).Invoke(__instance, args);
							break;
						default:
							typeof(WidgetJournal).GetMethod("SortByName", flags, null, new Type[] { typeof(GuiElement) }, null).Invoke(__instance, args);
							break;
					}
					__instance.DisplayDetails(entry);
				}
			}
		}

		[HarmonyPatch(typeof(AgentTracker), "getBarAgents")] //adding agents to bar screen
		public class AgentTracker_getBarAgents
		{

			[HarmonyPostfix]
			private static void Postfix(AgentTracker __instance, ref List<BarAgentDrawer> __result, ulong stationID, Point grid)
			{
				if (Globals.initialized)
				{
					if (stationID == PLAYER.currentGame.homeBaseId && !Globals.eventflags[GlobalFlag.Sige1EventActive])
					{
						Crew crew = HWBaseSiegeEvent.getFriendlyIntruder(PLAYER.currentSession);
						if (crew != null)
						{
							__result.Add(new BarAgentDrawer(new GenericIntruder(crew.name)));
						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(SCREEN_MANAGER), "initWidgets")] //initiating Screen Manager widgets extensions
		public class SCREEN_MANAGER_initWidgets
		{

			[HarmonyPostfix]
			private static void Postfix()
			{
				HWSCREEN_MANAGER.initWidgets();
			}
		}

		[HarmonyPatch(typeof(WidgetResources), "Draw")] //drawing reputation widget UI in the same batch as WidgetResources
		public class WidgetResources_Draw
		{

			[HarmonyPostfix]
			private static void Postfix(SpriteBatch batch)
			{
				if (HWSCREEN_MANAGER.widgetReputation != null)
				{
					HWSCREEN_MANAGER.widgetReputation.Draw(batch);
				}
			}
		}

		[HarmonyPatch(typeof(WidgetResources), "Update")] //updating reputation widget after WidgetResources
		public class WidgetResources_Update
		{

			[HarmonyPostfix]
			private static void Postfix(float elapsed, MouseAction clickState, Rectangle mousePos)
			{
				if (Globals.initialized)
				{
					if (PLAYER.currentSession.GetType() == typeof(BattleSessionSC))
					{
						//BattleSessionSC battleSessionSC = PLAYER.currentSession as BattleSessionSC;
						//HWSCREEN_MANAGER.widgetReputation.SetReputation(SCREEN_MANAGER.formatCreditStringSeparate(battleSessionSC.credits));
					}
					else
					{
						HWSCREEN_MANAGER.widgetReputation.SetReputation(SCREEN_MANAGER.formatCreditStringSeparate(Globals.globalints[GlobalInt.Bounty]), Globals.globalints[GlobalInt.Bounty] > 0);
					}
					if (HWSCREEN_MANAGER.widgetReputation != null)
					{
						HWSCREEN_MANAGER.widgetReputation.Update(elapsed, clickState, mousePos);
					}
				}
			}
		}

		[HarmonyPatch(typeof(VNavigationRev3), "updateInput")] //making custom tooltips show in ship navigation screen, custom  actions
		public class VNavigationRev3_updateInput
		{


			[HarmonyPrefix]
			private static void Prefix(ref bool __state, VNavigationRev3 __instance, KeyboardState ___oldState, Keys[] ___oldKeys, MouseState ___oldMouse)
			{
				HWSCREEN_MANAGER.toolTip = null;
				KeyboardState state = Keyboard.GetState();
				Keys[] pressedKeys = state.GetPressedKeys();
				MouseState state2 = Mouse.GetState();

				if (PLAYER.debugMode && Globals.initialized)
				{
					if (!Keyboard.GetState().IsKeyDown(Keys.L) && ___oldState.IsKeyDown(Keys.L) && !Globals.eventflags[GlobalFlag.Sige1EventActive]) //debug trigger for homebase siege event
					{
						InterruptionType template = InterruptionType.home_siege_pirate_t1;
						var currentdifficulty = (int)Math.Round(MathHelper.Clamp(((float)CHARACTER_DATA.shipsUnlocked() * HWCONFIG.GlobalDifficulty), 1f, 100f));
						if (currentdifficulty <= 12)
						{
							switch (Squirrel3RNG.Next(3))
							{
								case 0:
									template = InterruptionType.home_siege_pirate_t1;
									break;
								case 1:
									template = InterruptionType.home_siege_pirate_t1;
									break;
								case 2:
									template = InterruptionType.home_siege_pirate_t1;
									break;
								default:
									template = InterruptionType.home_siege_pirate_t1;
									break;
							}
						}
						if (currentdifficulty > 12 && currentdifficulty <= 21)
						{
							switch (Squirrel3RNG.Next(4))
							{
								case 0:
									template = InterruptionType.home_siege_pirate_t2;
									break;
								case 1:
									template = InterruptionType.home_siege_pirate_t2;
									break;
								case 2:
									template = InterruptionType.home_siege_pirate_t2;
									break;
								case 3:
									template = InterruptionType.home_siege_pirate_t2;
									break;
								default:
									template = InterruptionType.home_siege_pirate_t2;
									break;
							}
						}
						if (currentdifficulty > 21)
						{
							switch (Squirrel3RNG.Next(8))
							{
								case 0:
									template = InterruptionType.home_siege_pirate_t25;
									break;
								case 1:
									template = InterruptionType.home_siege_pirate_t25;
									break;
								case 2:
									template = InterruptionType.home_siege_pirate_t25;
									break;
								case 3:
									template = InterruptionType.home_siege_pirate_t25;
									break;
								case 4:
									template = InterruptionType.home_siege_pirate_t25;
									break;
								case 5:
									template = InterruptionType.home_siege_pirate_t25;
									break;
								case 6:
									template = InterruptionType.home_siege_pirate_t25;
									break;
								case 7:
									template = InterruptionType.home_siege_pirate_t25;
									break;
								default:
									template = InterruptionType.home_siege_pirate_t25;
									break;
							}
						}
						var interruption = new Interruption(template, PLAYER.currentGame.position, CONFIG.spHomeGrid);
						interruption.interdictionSpot = PLAYER.currentGame.position;
						HWSPAWNMANAGER.addInterruption(interruption);
						Globals.eventflags[GlobalFlag.Sige1EventActive] = true;
						Globals.eventflags[GlobalFlag.Sige1EventSpawnDialogueActive] = true;
					}
				}

				if ((CONFIG.keyBindings[7].wasJustReleased(___oldKeys, pressedKeys, ___oldMouse, state2) || (state2.LeftButton == ButtonState.Released && ___oldMouse.LeftButton == ButtonState.Pressed && CONFIG.keyBindings[6].isPressed(pressedKeys, state2))) && PLAYER.currentTeam != null)
				{
					__state = true;
				}

			}

			[HarmonyPostfix]
			private static void Postfix(ref bool __state, VNavigationRev3 __instance, Vector2 ___mousePos)
			{
				if (SCREEN_MANAGER.toolTip != null)
				{
					HWSCREEN_MANAGER.toolTip = null;
				}
				if (SCREEN_MANAGER.toolTip == null)
				{
					SCREEN_MANAGER.toolTip = HWSCREEN_MANAGER.toolTip;
				}
				if(Globals.initialized && PLAYER.currentConsole != null && PLAYER.currentShip != null)
				{ 
					if (__state && PLAYER.currentConsole.target != null && PLAYER.currentConsole.target.GetType() == typeof(Ship))
					{
						var actor = PLAYER.currentConsole.target;
						Ship ship2 = actor as Ship;
						if (!PLAYER.currentTeam.threats.Contains(ship2.faction))
						{							
							foreach (var crew in PLAYER.currentGame.team.crew)
							{
								if(crew?.currentCosm?.consoles != null && crew.currentCosm != PLAYER.currentShip.cosm && crew.currentCosm.consoles.Count > 0)
								{
									crew.team.focus = PLAYER.currentShip.id;
									crew.team.destination = default(Vector2);
									crew.team.goalType = ConsoleGoalType.escort;
									crew.team.ignoreAggression = true;
								}
							}
				
						}
						else
						{					
							foreach (var crew in PLAYER.currentGame.team.crew)
							{
								if (crew?.currentCosm?.consoles != null && crew.currentCosm != PLAYER.currentShip.cosm && crew.currentCosm.consoles.Count > 0)
								{
									crew.team.focus = ship2.id;
									crew.team.destination = default(Vector2);
									crew.team.goalType = ConsoleGoalType.kill_target;
									//crew.team.goalType = ConsoleGoalType.kill_enemies_escort;
									crew.team.ignoreAggression = false;
								}
							}
						}
					}
				}
				__state = false;
			}
		}

		[HarmonyPatch(typeof(ProceduralTaxiQuest), "test")] //adding reputation modifications to passenger quest
		public class ProceduralTaxiQuest_test
		{

			[HarmonyPostfix]
			private static void Postfix(bool __result, ProceduralTaxiQuest __instance, Crew ___crewRef, DialogueSelectRev2 ___dialogue)
			{
				if (Globals.initialized)
				{
					if (__result)
					{
						if (___crewRef == null)
						{
							Globals.changeReputation(5UL, -5);
						}
						else if (___crewRef.state == CrewState.dead)
						{
							Globals.changeReputation(5UL, -5);
						}
						else if (___dialogue != null && ___dialogue.removeMe)
						{
							if (__instance.stage != 2U)
							{
								if (__instance.stage == 777U)
								{
									Globals.changeReputation(5UL, 10, "for a successful transportation job.");
								}
							}
						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(AssasinationQuest), "crewDied")] //adding reputation modifications to assasiantion quest
		public class AssasinationQuest_crewDied
		{
			[HarmonyPostfix]
			private static void Postfix(Crew c, string ___targetName)
			{
				if (Globals.initialized)
				{
					if (c.name == ___targetName)
					{
						Globals.changeReputation(5UL, 20, "for a successful assasination job.");
						if (Globals.globalfactions.ContainsKey(c.faction))
						{
							Globals.changeReputation(c.faction, -10, "by assasinating a faction member.");
						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(TargetOverlay), "Update")] //checking faction of the target in target UI
		public class TargetOverlay_Update
		{
			[HarmonyPostfix]
			private static void Postfix(TargetOverlay __instance, Actor target)
			{
				BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
				HWReputationOptions.targetFaction = "Transponder: Unknown";
				if (PLAYER.currentSession  == null || PLAYER.currentWorld == null || PLAYER.currentSession.GetType() == typeof(BattleSessionG) || PLAYER.currentSession.GetType() == typeof(BattleSessionSA) || PLAYER.currentSession.GetType() == typeof(BattleSessionTA) || PLAYER.currentSession.grid != target.grid)
				{
					return;
                }
				var targetobject = typeof(TargetOverlay).GetField("targetType", flags).GetValue(__instance);
				var targettype = (ulong)Convert.ChangeType(targetobject, typeof(ulong));
				if (targettype <= 1)
				{
					if (PLAYER.currentSession.allShips.TryGetValue(target.id, out var ship))
					{
						if (Globals.globalfactions.ContainsKey(ship.faction))
						{
							HWReputationOptions.targetFaction = "Transponder: " + Globals.globalfactions[ship.faction].Item1;
						}
						else if (ship.faction == 2UL)
						{
							HWReputationOptions.targetFaction = "Transponder: Allies";
						}
						else if (ship.faction == 9UL) 
						{
							HWReputationOptions.targetFaction = "Transponder: Civillian";
						}
						else if (ship.faction == ulong.MaxValue)
						{
							HWReputationOptions.targetFaction = "Transponder: No Signal";
						}
					}
					else
					{ 
						for (int i = 0; i < PLAYER.currentSession.stations.Count; i++)
						{
							if(PLAYER.currentSession.stations[i].id == target.id)
							{
								if (Globals.globalfactions.ContainsKey(PLAYER.currentSession.stations[i].faction))
								{
									HWReputationOptions.targetFaction = "Transponder: " + Globals.globalfactions[PLAYER.currentSession.stations[i].faction].Item1;
								}
								else if(PLAYER.currentSession.stations[i].faction == 2UL)
								{
									HWReputationOptions.targetFaction = "Transponder: Allies";
								}
								else if (PLAYER.currentSession.stations[i].faction == 9UL)
								{
									HWReputationOptions.targetFaction = "Transponder: Civillian";
								}
								else if (PLAYER.currentSession.stations[i].faction == ulong.MaxValue)
								{
									HWReputationOptions.targetFaction = "Transponder: No Signal";
								}
							}
						}
					}
				}

			}
		}


		[HarmonyPatch(typeof(TargetOverlay), "Draw")] //adding faction description to target UI
		public class TargetOverlay_Draw
		{
			[HarmonyPostfix]
			private static void Postfix(TargetOverlay __instance, SpriteBatch spriteBatch, bool ___drawName, Vector2[] ___pointsTL, Vector2[] ___pointsBL, Vector2 ___center, int ___screenHeight, float ___secondPhase, bool ___nameReady, Vector2 ___bp_info_pos, Vector2 ___bp_speed_offset, Color ___col_speedText, Vector2 ___massUnder)
			{
				Vector2 vector2 = SCREEN_MANAGER.FF10.MeasureString(HWReputationOptions.targetFaction);
				var faction_pos = new Vector2(___center.X - vector2.X / 2f, -___pointsTL[2].Y + (float)___screenHeight - 20f);
				BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
				var target = typeof(TargetOverlay).GetField("targetType", flags).GetValue(__instance);
				var targettype = (ulong)Convert.ChangeType(target, typeof(ulong));
				if (targettype <= 1)
				{
					if (___drawName && !___nameReady)
					{
						spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, null);
						spriteBatch.DrawString(SCREEN_MANAGER.FF10, HWReputationOptions.targetFaction, faction_pos, new Color(0.5764725f, 0.7352925f, 0.75f));//CONFIG.iconMpPlayer * ___secondPhase);
						spriteBatch.End();
					}
				}
				//drawing help info
				Vector2 targetinfosize = SCREEN_MANAGER.FF10.MeasureString("[MB3] Ally target");
				var targetinfo_pos = new Vector2(___center.X - targetinfosize.X / 2f - 3f, -(MathHelper.Lerp(___pointsBL[3].Y, ___pointsTL[3].Y, 0)) + (float)___screenHeight + 5f);		
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, null);
				spriteBatch.DrawString(SCREEN_MANAGER.FF10, "[MB3] Ally target", targetinfo_pos, ___col_speedText);
				spriteBatch.End();
				

			}
		}

		[HarmonyPatch(typeof(HackingScreenRev2), "win")] //adding reputation loss for hacking faction ships
		public class HackingScreenRev2_win
		{

			[HarmonyPostfix]
			private static void Postfix()
			{
				if (Globals.initialized)
				{
					if (PLAYER.currentShip.ownershipHistory.Count >= 2)
					{
						var sessionships = PLAYER.currentSession.allShips.Keys.ToArray();
						for (int i = 0; i < sessionships.Length; i++)
						{
							var shipid = sessionships[i];
							var signature = PLAYER.currentShip.signitureRadius;
							if (PLAYER.currentSession.allShips[shipid] != PLAYER.currentShip && PLAYER.currentSession.allShips[shipid].cosm != null && PLAYER.currentSession.allShips[shipid].cosm.alive && Vector2.DistanceSquared(PLAYER.currentSession.allShips[shipid].position, PLAYER.currentShip.position) <= ((signature + (PLAYER.currentSession.allShips[shipid].scanRange / 1000f)) * PLAYER.currentShip.tempVis * PLAYER.currentSession.allShips[shipid].tempView) * ((signature + (PLAYER.currentSession.allShips[shipid].scanRange / 1000f)) * PLAYER.currentShip.tempVis * PLAYER.currentSession.allShips[shipid].tempView))
							{
								ulong faction = PLAYER.currentShip.ownershipHistory[PLAYER.currentShip.ownershipHistory.Count - 2];
								Globals.changeReputation(faction, -10, "a nearby ship has spotted you stealing a faction owned ship.");
								break;
							}
						}
					}
					Task.Run(async () =>
					{
						Globals.mostExpensiveDesigndifficulty = Globals.DifficultyFromCost(HW_CHARACTER_DATA_Extensions.mostExpensiveDesign());
					});
				}
			}
		}
		
		/*
		[HarmonyPatch(typeof(WidgetJournal), "DisplayDetails")] //managing questjournal if HWBaseSiegeEvent quest set to NULL
		public class WidgetJournal_DisplayDetails
		{
			[HarmonyPostfix]
			private static void Prefix(WidgetJournal __instance, JournalEntry entry)
			{
				BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
				if (entry != null)
				{		
					if (entry.quest == null && entry.name == HWBaseSiegeEvent.tip.tip)
					{
						var args = new object[] { HWBaseSiegeEvent.tip.description };
						var descriptionText = typeof(WidgetJournal).GetField("descriptionText", flags).GetValue(__instance);
						typeof(GuiElement).Assembly.GetType("CoOpSpRpG.TextBoxStatic").GetMethod("SetText", flags, null, new Type[] { typeof(string) }, null).Invoke(descriptionText, args); // accessing private field of an internal type instance with reflection
						typeof(WidgetJournal).GetField("descriptionText", flags).SetValue(__instance, descriptionText);
					}
				}
			}
		}
		/*

							/*
							[HarmonyPatch(typeof(RootMenuRev2), "actionConfirmDelete")]             //deleting mod data on deleting savegame to be attached to vanilla game method after it gets implemented
							public class RootMenuRev2_actionConfirmDelete
							{

								[HarmonyPostfix]
								private static void Postfix(SaveEntry __focusedSave)
								{
									string[] modfiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\worldsaves\\", __focusedSave.name + "*", SearchOption.AllDirectories);
									for (int i = 0; i < modfiles.Count(); i++)
									{
										var file = modfiles[i];
										if (file.Contains("_HW_MODSAVE"))
											File.Delete(file);
									}
								}
							}
							*/


	}
}
