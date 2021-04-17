using System;
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
							var shipids = PLAYER.currentSession.allShips.Keys.ToArray();
							for (int i = 0; i < shipids.Length; i++)
							{
								var shipid = shipids[i];
								if (PLAYER.currentShip.id != shipid && PLAYER.currentSession.allShips[shipid].cosm != null && Vector2.DistanceSquared(PLAYER.currentShip.position, PLAYER.currentSession.allShips[shipid].position) < ((signature + (PLAYER.currentSession.allShips[shipid].scanRange / 1000f)) * PLAYER.currentShip.tempVis * PLAYER.currentSession.allShips[shipid].tempView) * ((signature + (PLAYER.currentSession.allShips[shipid].scanRange / 1000f)) * PLAYER.currentShip.tempVis * PLAYER.currentSession.allShips[shipid].tempView))
								{
									// random ambush
									if (!interrupted && Squirrel3RNG.Next(1, Math.Max((int)(250000 / HWCONFIG.InterruptionFrequency), 2)) == 1 && PLAYER.currentShip.boostStage > 1 && PLAYER.currentShip.position.X < 89000f && PLAYER.currentShip.position.X > -89000f && PLAYER.currentShip.position.Y < 89000f && PLAYER.currentShip.position.Y > -89000f)
									{
										HWSPAWNMANAGER.addInterruption(new Interruption(INTERRUPTION_BAG.GetRandomTemplate(), PLAYER.currentShip.position, PLAYER.currentShip.grid));
										interrupted = true;
									}

									// ambush if player has demanded goods in inventory
									if (!interrupted && Squirrel3RNG.Next(1, Math.Max((int)(200000 / HWCONFIG.InterruptionFrequency), 2)) == 1 && PLAYER.currentShip.position.X < 89000f && PLAYER.currentShip.position.X > -89000f && PLAYER.currentShip.position.Y < 89000f && PLAYER.currentShip.position.Y > -89000f)
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
											if (PLAYER.currentSession.allShips[shipid].ownershipHistory.Contains(3UL))
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
											}
										}
									}

								}
							}
						}

						// spawn random ambush on traveling with Higgs drive
						if (!interrupted && Squirrel3RNG.Next(1, Math.Max((int)(80000 / HWCONFIG.InterruptionFrequency), 2)) == 1 && PLAYER.currentShip.boostStage > 2 && PLAYER.currentShip.position.X < 89000f && PLAYER.currentShip.position.X > -89000f && PLAYER.currentShip.position.Y < 89000f && PLAYER.currentShip.position.Y > -89000f)
						{
							HWSPAWNMANAGER.addInterruption(new Interruption(INTERRUPTION_BAG.GetRandomTemplate(), PLAYER.currentShip.position, PLAYER.currentShip.grid));
							interrupted = true;
						}


					}

					if (PLAYER.currentSession != null && PLAYER.currentShip != null && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId && !Globals.eventflags[GlobalFlag.Sige1EventActive])
					{
						// homebase siege1 pirate event
						if ( Squirrel3RNG.Next(1, Math.Max((int)(450000 / HWCONFIG.InterruptionFrequency), 2)) == 1 || PLAYER.debugMode)
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
							HWSPAWNMANAGER.addInterruption(new Interruption(template, PLAYER.currentShip.position, PLAYER.currentShip.grid));
							Globals.eventflags[GlobalFlag.Sige1EventActive] = true;
							Globals.eventflags[GlobalFlag.Sige1EventSpawnDialogueActive] = true;
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
				if (Globals.GlobalShipRemoveQueue != null && !Globals.GlobalShipRemoveQueue.IsEmpty)  // removing collected garbage ship ids (already set to despawn by the Interruption.DespawnEnqueqedShipAsync) from active sessions
				{
					List<Tuple<ulong, Point>> list = new List<Tuple<ulong, Point>>();
					while (Globals.GlobalShipRemoveQueue.TryDequeue(out var tuple))
					{
						if (__instance.grid == tuple.Item2)
						{							
							if(__instance.allShips.TryGetValue(tuple.Item1, out var ship) && ship.removeThis)
							{ 
								__instance.allShips.Remove(tuple.Item1);
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
				if(Globals.Interruptions != null)
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

		[HarmonyPatch(typeof(SHIPBAG), "loadShips")]
		public class SHIPBAG_loadShips //ship lodouts
		{

			[HarmonyPostfix]
			private static void Postfix(ref Dictionary<int, TurretType[]> ___aiLoadouts, ref Dictionary<int, float> ___aiValues, ref Dictionary<int, Texture2D> ___aiTops, ref Dictionary<int, Texture2D> ___aiBots, ref Dictionary<int, int> ___shaderLayers, ref Dictionary<int, Texture2D> ___aiNorms, ref Dictionary<int, Texture2D> ___aiSpecs, ref Dictionary<int, Texture2D> ___aiEmits, ref Dictionary<int, bool> ___aiEmpty)
			{
				// Blood_eagle
				___aiLoadouts[90] = new TurretType[] { TurretType.t_b_machinegun1, TurretType.t_b_machinegun1, TurretType.m_b_380mmautocannon, TurretType.m_b_380mmautocannon, TurretType.m_b_cluster, TurretType.m_b_cluster, TurretType.h_b_antimatter, TurretType.h_b_antimatter, TurretType.h_b_railgun_charged, TurretType.h_b_railgun_charged, TurretType.h_b_railgun_charged, TurretType.h_b_railgun_charged, TurretType.m_b_cluster, TurretType.m_b_cluster, TurretType.t_b_machinegun1, TurretType.t_b_machinegun1 };
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

		[HarmonyPatch(typeof(NonPlayerShip), "tryBuyBudget")] //counting every purchase attempt of to asses their market value
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
							bool flag3 = num < 30f * 30f;
							if (flag3)
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
												PLAYER.currentGame.team.grantExp(num2);
												__instance.floatyText.Enqueue("+" + SCREEN_MANAGER.formatCreditString(unchecked((ulong)(checked(100U * inventoryItem2.stackSize)))) + " exp");
											}
											else
											{
												bool flag7 = inventoryItem2.type == InventoryItemType.core_node;
												if (flag7)
												{
													CHARACTER_DATA.coreNodes += unchecked((ulong)inventoryItem2.stackSize);
													SCREEN_MANAGER.alerts.Enqueue("Core node gained");
												}
												else
												{
													__instance.cosm.threadDumpCargo(inventoryItem2);
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
							bool flag9 = num < 120f * 120f;
							if (flag9)
							{
								Vector2 value = Vector2.Normalize(__instance.position - cargoPod4.position) * 55f * elapsed;
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
				int shipsUnlocked = (int)Math.Round(MathHelper.Clamp(((float)CHARACTER_DATA.shipsUnlocked() * HWCONFIG.GlobalDifficulty), 1f, 100f));
				//shipsUnlocked = 30; //for debugging
				int monsterHealthScaled = 100;
				float monsterSpeedScaled = 100f;
				float monsterDetectRangeScaled = 400f;
				float monsterInterestRangeScaled = 1000f;
				float monsterAttackRangeScaled = 10f;
				int monsterDamageScaled = 1;
				int monsterThicknessScaled = 1;

				monsterHealthScaled += Squirrel3RNG.Next(Math.Max(0, (shipsUnlocked - 4) * 20), Math.Max(10, Convert.ToInt32(((float)shipsUnlocked - 4) * 20 * 1.5)));
				monsterSpeedScaled *= 1f + (0.04f * Squirrel3RNG.Next(Math.Max(1, (shipsUnlocked - 4)), Math.Max(2, Convert.ToInt32(((float)shipsUnlocked - 4) * 4))));
				monsterDamageScaled += Squirrel3RNG.Next(Math.Max(0, (shipsUnlocked - 3) * 3), Math.Max(1, Convert.ToInt32(((float)shipsUnlocked - 3) * 4)));
				monsterThicknessScaled += Squirrel3RNG.Next(Math.Max(0, (shipsUnlocked - 3) * 3), Math.Max(1, Convert.ToInt32(((float)shipsUnlocked - 3) * 4)));

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

			}
		}

		[HarmonyPatch(typeof(Monster), "applyDamage")] // making monsters drop items on death.
		public class Monster_applyDamage
		{
			[HarmonyPostfix]
			private static void Postfix(Monster __instance, MicroCosm cosm)
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

						InventoryItem Item = new InventoryItem();
						InventoryItem Item2 = new InventoryItem();
						int shipsUnlocked = CHARACTER_DATA.shipsUnlocked();
						//shipsUnlocked = 30; //debugging

						int selectItem = Squirrel3RNG.Next(1, 7);
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
								Item = new Gun(gunQuality, GunSpawnFlags.no_oneshot);
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

						if (selectItem == 3 && Squirrel3RNG.Next(30) == 1)
						{
							// repair gun
							float repairgunQuality = Squirrel3RNG.Next(Math.Min(30, shipsUnlocked) - 10, 40);
							repairgunQuality = MathHelper.Clamp(repairgunQuality, 1f, (float)shipsUnlocked + 10f);
							repairgunQuality = MathHelper.Clamp(repairgunQuality, 1f, 39.9f) / 5;
							Item = new RepairGun(repairgunQuality);
						}
						if (selectItem == 4 && Squirrel3RNG.Next(30) == 1)
						{
							// fire extinguisher
							float extinguisherQuality = Squirrel3RNG.Next(Math.Min(30, shipsUnlocked) - 10, 40);
							extinguisherQuality = MathHelper.Clamp(extinguisherQuality, 10f, (float)shipsUnlocked + 10f);
							extinguisherQuality = MathHelper.Clamp(extinguisherQuality, 10f, 39.9f) / 2;
							Item = new Extinguisher(extinguisherQuality);
						}
						if (selectItem == 5)
						{
							// mining laser
							if (Squirrel3RNG.Next(50) == 1)
								Item = new Digger();
						}

						if (selectItem == 6)
						{
							// artifact
							if (Squirrel3RNG.Next(50) == 1)
								Item = new ArtifactItem((float)(1.0 + Squirrel3RNG.NextDouble() * 24.0));
						}

						int selectItem2 = Squirrel3RNG.Next(1, 6);
						if (selectItem2 == 1)
						{
							// Shirt1
							Item2 = new Shirt(vanityItemType.crew_shirt);
						}
						if (selectItem2 == 2)
						{
							// Shirt2
							Item2 = new Shirt(vanityItemType.red_shirt);
						}
						if (selectItem2 == 3)
						{
							// Shirt3
							Item2 = new Shirt(vanityItemType.tank_top);
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


						for (int i = 0; i < cosm.crew.Values.Count; i++)
						{
							var crew = cosm.crew.Values.ToList()[i];
							if (crew.state != CrewState.dead && Vector2.DistanceSquared(crew.position, __instance.position) < 600f * 600f && crew.isPlayer)
							{
								if (Squirrel3RNG.Next(0, 100) < 5)
								{
									if (PLAYER.avatar.currentCosm.ship != null)
									{
										PLAYER.avatar.currentCosm.ship.threadDumpCargo(Item);
										PLAYER.avatar.GetfloatyText().Enqueue("+ 1 loot item deployed into space");
									}
								}
								else
								{
									if (PLAYER.avatar.currentCosm.ship != null)
									{
										if (Squirrel3RNG.Next(0, 100) < 15)
										{

											if (Item2.type == InventoryItemType.biomass)
											{
												uint num2 = 100U * Item2.stackSize;
												CHARACTER_DATA.exp += unchecked((ulong)num2);
												PLAYER.currentGame.team.grantExp(num2);
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
		/* debugging
		[HarmonyPatch(typeof(Monster), "calculateBbox")]
		public class Monster_calculateBbox
		{
			[HarmonyPrefix]
			private static void Prefix(Monster __instance)
			{
				if (float.IsNaN(__instance.position.X))
				{
					__instance.position.X = 0;
				}
				if (float.IsNaN(__instance.position.Y))
				{
					__instance.position.Y = 0;
				}
			}
		}
		*/

		[HarmonyPatch(typeof(Crew))]
		[HarmonyPatch(MethodType.Constructor)]
		[HarmonyPatch(new Type[] { })]
		public class Crew_Crew
		{

			[HarmonyPostfix]
			private static void Postfix(Crew __instance) // randomizing and scaling all npc outfits
			{

				
				int cost = HW_CHARACTER_DATA_Extensions.mostExpensiveDesign();
				int difficulty = Globals.DifficultyFromCost(cost);
				int shipsUnlocked = (int)Math.Round(MathHelper.Clamp(((float)CHARACTER_DATA.shipsUnlocked() * HWCONFIG.GlobalDifficulty), 1f, 100f));
				float gunQuality = Squirrel3RNG.Next(Math.Max(Math.Min(30, shipsUnlocked) - 10, difficulty * 4), 40);
				float armorQuality = Squirrel3RNG.Next(Math.Max(Math.Min(30, shipsUnlocked) - 10, difficulty * 4), 40);
				bool flag = gunQuality <= 0f;
				bool flag2 = armorQuality > 0f;
				gunQuality = MathHelper.Clamp(gunQuality, 10f, (float)Math.Max(shipsUnlocked + 10, difficulty * 4));
				armorQuality = MathHelper.Clamp(armorQuality, 10f, (float)Math.Max(shipsUnlocked + 10, difficulty * 4));
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
					int cost = HW_CHARACTER_DATA_Extensions.mostExpensiveDesign();
					int difficulty = Globals.DifficultyFromCost(cost);
					int shipsUnlocked = (int)Math.Round(MathHelper.Clamp(((float)CHARACTER_DATA.shipsUnlocked() * HWCONFIG.GlobalDifficulty), 1f, 100f));
					float MygunQuality = Squirrel3RNG.Next(Math.Max(Math.Min(30, shipsUnlocked) - 10, difficulty * 4), 40);
					float MyarmorQuality = Squirrel3RNG.Next(Math.Max(Math.Min(30, shipsUnlocked) - 10, difficulty * 4), 40);
					bool flag = MygunQuality <= 0f;
					bool flag2 = MyarmorQuality > 0f;
					MygunQuality = MathHelper.Clamp(MygunQuality, 10f, (float)Math.Max(shipsUnlocked + 10, difficulty * 4));
					MyarmorQuality = MathHelper.Clamp(MyarmorQuality, 10f, (float)Math.Max(shipsUnlocked + 10, difficulty * 4));
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



				if (!__instance.isPlayer && __instance.faction != PLAYER.avatar.faction) // npcs will drop some of their gear on death
				{
					bool flag7 = __instance.currentCosm != null && __instance.currentCosm.ship != null;
					if (flag7)
					{
						bool flag8 = __instance.inventory != null && __instance.currentCosm != null && __instance.currentCosm.ship != null;
						if (flag8)
						{
							var inventory = __instance.inventory;
							foreach (InventoryItem inventoryItem in inventory)
							{
								if (Squirrel3RNG.Next(7) == 1)
									__instance.currentCosm.ship.threadDumpCargo(inventoryItem);

							}

						}
					}
				}


				if (!__instance.isPlayer && __instance.faction != PLAYER.avatar.faction)
				{
					if (__instance.currentCosm != null && __instance.currentCosm.crew != null)
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
					if (__instance.currentCosm != null && __instance.currentCosm.crew != null)
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

		[HarmonyPatch(typeof(Crew), "testLOS")] //fixing the crew fiering out of range bug
		public class Crew_testLOS
		{
			[HarmonyPostfix]
			private static void Postfix(Crew __instance, ref bool __result, Vector2 target, MicroCosm cosm)
			{
				if (__instance.heldItem != null && __instance.heldItem.GetType() == typeof(Gun))
				{
					float num = (__instance.heldItem as Gun).range;
					float num2 = Vector2.Distance(__instance.position, target);
					if (num2 > num)
					{
						__result = false;
					}
				}
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
					if (PROCESS_REGISTER.currentCosm.klaxonOverride == true)
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
				if (!___waiting && PLAYER.avatar.state == CrewState.dead)
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


		[HarmonyPatch(typeof(Respawning), "updateInput")]
		public class Respawning__updateInput // making player drop items on manual respawning (clearing inventory after items dropped)
		{

			[HarmonyPrefix]
			private static void Prefix(Respawning __instance, ref Vector2 ___mousePos, float ___wasteTimer, List<Clickable> ___buttons, MouseState ___oldMouse)
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
											{
												configWriter.WriteLine($"Max Active Encounters:2");
												configWriter.WriteLine($"Encounter Frequency Multiplier:1.0");
												configWriter.WriteLine($"Global Difficulty Multiplier:1.0");
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
					{
						configWriter.WriteLine($"Max Active Encounters:2");
						configWriter.WriteLine($"Encounter Frequency Multiplier:1.0");
						configWriter.WriteLine($"Global Difficulty Multiplier:1.0");
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
					Globals.offer = null;
					Globals.demand = null;
					Globals.eventflags.Clear();
					HWBaseSiegeEvent.targetModule = null;
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
				if(PLAYER.currentSession.GetType() == typeof(BattleSessionSP))
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
					if ((Globals.eventflags[GlobalFlag.PiratesCalled] || (Globals.Interruptionbag != null && !Globals.Interruptionbag.Values.ToList().TrueForAll(element => element.templateUsed != InterruptionType.friendly_pirates_call))) && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId)
					{
						PLAYER.currentShip.scanRange = CONFIG.minViewDist;
						PLAYER.currentShip.signitureRadius = CONFIG.minViewDist;
						PLAYER.currentShip.tempView = 1f;
						__instance.group = 0;
						foreach (var interruption in Globals.Interruptionbag)
						{
							if(interruption.Value.templateUsed == InterruptionType.friendly_pirates_call)
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


		[HarmonyPatch(typeof(HailAnimation), "smallTalkLoop")] // adding dialogue option for hailing friendly Pirates to deliver a free ship for player
		public class HailAnimation_smallTalkLoop
		{

			[HarmonyPrefix]
			private static void Prefix(DialogueTree lobby, Crew ___representative, List<ResponseImmediateAction> ___results)
			{
				DialogueTree dialogueTree = new DialogueTree();
				DialogueTree dialogueTree1 = new DialogueTree();
				DialogueTree dialogueTree2 = new DialogueTree();
				dialogueTree2.action = new ResponseImmediateAction(() => ___results.ForEach(i => i()));
				___results.Add(new ResponseImmediateAction(PLAYER.currentSession.unpause));
				DialogueTree dialogueTree3 = new DialogueTree();
				DialogueTree dialogueTree4 = new DialogueTree();
				DialogueTree dialogueTree5 = new DialogueTree();
				DialogueTree dialogueTree6 = new DialogueTree();
				DialogueTree dialogueTree7 = new DialogueTree();
				DialogueTree dialogueTree8 = new DialogueTree();
				DialogueTree dialogueTree9 = new DialogueTree();
				DialogueTree dialogueTree10 = new DialogueTree();
				DialogueTree dialogueTree11 = new DialogueTree();
				DialogueTree dialogueTree12 = new DialogueTree();
				DialogueTree dialogueTree13 = new DialogueTree();
				DialogueTree dialogueTree14 = new DialogueTree();
				DialogueTextMaker buyship = delegate ()
				{
					CHARACTER_DATA.credits -= 1120u;
					foreach (var interruption in Globals.Interruptionbag)
					{
						if (interruption.Value.templateUsed == InterruptionType.friendly_pirates_call)
						{
							var direction = Vector2.Transform(interruption.Value.spawnPoints[RANDOM.getRandomNumber(interruption.Value.spawnPoints.Count<Vector2>())], interruption.Value.rotationMatrix);
							foreach (var tupleship in interruption.Value.activeShips)
							{
								tupleship.Item2.Clear();
								tupleship.Item2.AddRange(new List<string> { "We are going home.", "Enjoy your new ride.", "Let's go, boys.", "Powering engines.", "He actually bought it. What a moron." });
								if (PLAYER.currentSession.allShips.TryGetValue(tupleship.Item1, out Ship ship))
								{
									if (ship.spawnIndex == 33 || ship.spawnIndex == 34)
									{
										SCREEN_MANAGER.alerts.Enqueue("Use the logistic room to tractor your new ship to the dock.");
										//ship.aimMove(PLAYER.currentShip.position); needs piloting
										ship.velocity = Vector2.Normalize(PLAYER.currentShip.position - ship.position) * 40f;
										ship.rotationVelocity = 7.5f;
										foreach (Crew crew in ship.cosm.crew.Values)
										{
											crew.kill();
										}
										ship.ownershipHistory.Add(3UL);
										ship.ownershipHistory.Add(PLAYER.avatar.faction);
										ship.faction = PLAYER.avatar.faction;
										PLAYER.currentTeam.ownedShip = ship.id;
										ship.hackingAvailable = 0f;
									}
									else
									{
										if (ship.cosm?.crew != null)
										{
											foreach (var crew in ship.cosm.crew.Values)
											{
												crew.team.destination = ship.position + (ship.position + direction); //setting a destination to fly away
												if (crew.team.threats.Contains(PLAYER.avatar.faction))
													crew.team.threats.Remove(PLAYER.avatar.faction);  // hostile npcs will become friendly again.
											}
										}
									}
								}
							}
							for (int j = 0; j < interruption.Value.activeShips.Count<Tuple<ulong, List<String>>>(); j++)
							{
								if (PLAYER.currentSession.allShips.TryGetValue(interruption.Value.activeShips[j].Item1, out Ship ship2))
								{
									if (ship2.spawnIndex == 33 || ship2.spawnIndex == 34)
									{
										interruption.Value.activeShips.Remove(interruption.Value.activeShips[j]);
									}
								}
								else
								{
									interruption.Value.activeShips.Remove(interruption.Value.activeShips[j]);
								}
							}
						}
					}
					Globals.eventflags[GlobalFlag.PiratesCalledHostile] = false;
					Globals.eventflags[GlobalFlag.PiratesCalled] = false;
					return "Thanks.";
				};
				DialogueTextMaker payvisit = delegate ()
				{
					CHARACTER_DATA.credits -= 120u;
					foreach (var interruption in Globals.Interruptionbag)
					{
						if (interruption.Value.templateUsed == InterruptionType.friendly_pirates_call)
						{
							var direction = Vector2.Transform(interruption.Value.spawnPoints[RANDOM.getRandomNumber(interruption.Value.spawnPoints.Count<Vector2>())], interruption.Value.rotationMatrix);
							foreach (var tupleship in interruption.Value.activeShips)
							{
								tupleship.Item2.Clear();
								tupleship.Item2.AddRange(new List<string> { "We are going home.", "Let's go, boys.", "Time to leave this shithole.", "Powering engines.", "What a shitty place." });
								if (PLAYER.currentSession.allShips.TryGetValue(tupleship.Item1, out Ship ship))
								{
									if (ship.cosm?.crew != null)
									{
										foreach (var crew in ship.cosm.crew.Values)
										{
											crew.team.destination = ship.position + (ship.position + direction); //setting a destination to fly away
											if (crew.team.threats.Contains(PLAYER.avatar.faction))
												crew.team.threats.Remove(PLAYER.avatar.faction);  // hostile npcs will become friendly again.
										}
									}
								}
							}
						}
					}
					Globals.eventflags[GlobalFlag.PiratesCalledHostile] = false;
					Globals.eventflags[GlobalFlag.PiratesCalled] = false;
					return "...";
				};
				DialogueTextMaker waithostile = delegate ()
				{
					foreach (var interruption in Globals.Interruptionbag)
					{
						if (interruption.Value.templateUsed == InterruptionType.friendly_pirates_call)
						{
							foreach(var tupleship in interruption.Value.activeShips)
							{
								tupleship.Item2.Clear();
								tupleship.Item2.AddRange(INTERRUPTION_BAG.GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)));

								if (PLAYER.currentSession.allShips.TryGetValue(tupleship.Item1, out Ship ship))
								{
									if(ship.cosm?.crew != null)
									{
										foreach (var crew in ship.cosm.crew.Values)
											crew.team.threats.Add(PLAYER.avatar.faction);
									}
								}
                            }
						}
					}
					Globals.eventflags[GlobalFlag.PiratesCalledHostile] = true;
					return "...";
				};
				DialogueTextMaker waitnonhostile = delegate ()
				{
					foreach (var interruption in Globals.Interruptionbag)
					{
						if (interruption.Value.templateUsed == InterruptionType.friendly_pirates_call)
						{
							foreach (var tupleship in interruption.Value.activeShips)
							{
								tupleship.Item2.Clear();
								tupleship.Item2.AddRange(new List<string> { "You better hurry up.", "Don't test our patience.", "What's taking so long?", "Just pay already.", "I think he is going to screw us." });
								if (PLAYER.currentSession.allShips.TryGetValue(tupleship.Item1, out Ship ship))
								{
									if (ship.cosm?.crew != null)
									{
										foreach (var crew in ship.cosm.crew.Values)
										{
											if (crew.team.threats.Contains(PLAYER.avatar.faction))
												crew.team.threats.Remove(PLAYER.avatar.faction);  // hostile npcs will become friendly again.
										}
									}
								}
							}
						}
					}
					Globals.eventflags[GlobalFlag.PiratesCalledHostile] = false;
					return "Allright, I will see if I can get some money to pay you.";
				};
				lobby.addOption("Hey I am glad you are here, I have an emergency.", dialogueTree, () => ___representative.faction == 8UL && Globals.eventflags[GlobalFlag.PiratesCalled] && !Globals.eventflags[GlobalFlag.PiratesCalledHostile] && (___representative.currentCosm.ship.spawnIndex != 33 || ___representative.currentCosm.ship.spawnIndex != 34));
				lobby.addOption("Hey I am glad you are here, I have an emergency.", dialogueTree8, () => ___representative.faction == 8UL && Globals.eventflags[GlobalFlag.PiratesCalled] && Globals.eventflags[GlobalFlag.PiratesCalledHostile] && (___representative.currentCosm.ship.spawnIndex != 33 || ___representative.currentCosm.ship.spawnIndex != 34));
				lobby.addOption("Hey I am glad you are here, I have an emergency.", dialogueTree14, () => ___representative.faction == 8UL && Globals.eventflags[GlobalFlag.PiratesCalled] && (___representative.currentCosm.ship.spawnIndex == 33 || ___representative.currentCosm.ship.spawnIndex == 34));
				dialogueTree.text = "Yes, your master told us that you blew up your ship and now stranded at your station.";
				dialogueTree.addOption("Oh .. Uh .. yes. I mean, NO. He ist NOT my master.", dialogueTree1, () => Globals.Interruptionbag != null && Globals.Interruptionbag.Values.ToList().TrueForAll(element => (element.templateUsed == InterruptionType.friendly_pirates_call && element.activeShips.Count == 3) || (element.templateUsed != InterruptionType.friendly_pirates_call))); // check if the ship is still avaible.
				dialogueTree.addOption("Actually we are more like partners. I am working for him and he is making bad jokes.", dialogueTree9, () => Globals.Interruptionbag != null && Globals.Interruptionbag.Values.ToList().TrueForAll(element => (element.templateUsed == InterruptionType.friendly_pirates_call && element.activeShips.Count != 3) || (element.templateUsed != InterruptionType.friendly_pirates_call))); // check if the ship is still avaible, if not, then this option without the ship offer

				dialogueTree1.text = "Whatever. We have a ship for you. Thare is a catch though.";
				dialogueTree1.addOption("What's the catch?", dialogueTree3);

				dialogueTree3.text = "This ship is kind of a hot ride. We jacked it from SSC a while ago, but we couln't find and remove the traveldrive signature transponder. If you happen to fly into SSC territory in this ship with activated travel drive, they could receive the signal and ambush you.";
				dialogueTree3.addOption("Yes, please. I have no fear of those SSC assholes.", dialogueTree4);
				dialogueTree3.addOption("Nah, it's to much trouble for me.", dialogueTree5);

				dialogueTree4.text = "Alright then, 120 bucks for the trouble of gettin' here plus 1000 bucks for the ship, and it's yours.";
				dialogueTree4.addOption("Here you go.", dialogueTree11, () => (CHARACTER_DATA.credits >= 1120u));//check if can be payed
				dialogueTree5.addOption("I don't have the money.", dialogueTree5, () => (CHARACTER_DATA.credits < 1120u)); //check if can be payed
				dialogueTree4.addOption("No, I've changed my mind.", dialogueTree5);

				dialogueTree5.text = "You will have to pay for our visit though. 120 bucks.";
				dialogueTree5.addOption("Here you go.", dialogueTree10, () => (CHARACTER_DATA.credits >= 120u));//check if can be payed
				dialogueTree5.addOption("I don't have the money.", dialogueTree7, () => (CHARACTER_DATA.credits < 120u)); //check if can be payed
				dialogueTree5.addOption("Bite me! I won't pay for nothing.", dialogueTree6);

				dialogueTree6.text = "Well we'll wait here and feed you with bullets until you pay us asshole.";
				dialogueTree6.addOption("Allright, no need for hostility, here you go.", dialogueTree10, () => (CHARACTER_DATA.credits >= 120u)); //check if can be payed
				dialogueTree6.addOption("You sure can try.", dialogueTree12);  //pirates wait hostile

				dialogueTree7.text = "You sure have some equipment to sell in your station's shop for paying us.";
				dialogueTree7.addOption("Allright, I will see if I can get some money to pay you.", dialogueTree13); //pirates wait non-hostile
				dialogueTree7.addOption("Bite me! I won't pay for nothing.", dialogueTree6);

				dialogueTree8.text = "I bet you do, asshole. You still have to pay 120 bucks for our visit.";
				dialogueTree8.addOption("Here you go.", dialogueTree10, () => (CHARACTER_DATA.credits >= 120u)); //check if can be payed
				dialogueTree8.addOption("I want to buy your ship.", dialogueTree4, () => Globals.Interruptionbag != null && Globals.Interruptionbag.Values.ToList().TrueForAll(element => (element.templateUsed == InterruptionType.friendly_pirates_call && element.activeShips.Count == 3) || (element.templateUsed != InterruptionType.friendly_pirates_call))); // check if the ship is still avaible
				dialogueTree8.addOption("I don't have the money.", dialogueTree7, () => (CHARACTER_DATA.credits < 120u)); //check if can be payed
				dialogueTree8.addOption("Bite me! I won't pay for nothing.", dialogueTree6);

				dialogueTree9.text = "What a sad story. Anyway, we had a ship for you, but some jerk attacked us. Now we need a ship ourself.";
				dialogueTree9.addOption("...", dialogueTree5);

				dialogueTree10.text = "Allright, we are heading back now.";
				dialogueTree10.addOption(payvisit, lobby);

				dialogueTree11.text = "Allright, ship is yours.";
				dialogueTree11.addOption(buyship, lobby);

				dialogueTree12.text = "Enjoy your bullet meal.";
				dialogueTree12.addOption(waithostile, dialogueTree2);

				dialogueTree13.text = "Don't screw with us.";
				dialogueTree13.addOption(waitnonhostile, dialogueTree2);

				dialogueTree14.text = "You are speaking to autopilot AI '"+ ___representative.name + "' version 0.9 dev build. I am not designed to provide a full range of service. Please hail one of the escort pilots to handle any emergency situation.";
				dialogueTree14.addOption("Uh..Sure.", dialogueTree2);
			}
		}

		[HarmonyPatch(typeof(LogisticsScreenRev3), "doRightClick")] // preventing player from scrapping friendly pirate ship in normal way.
		public class LogisticsScreenRev3_doRightClick
		{

			[HarmonyPrefix]
			private static void Prefix(ref string opt, Ship ___selected)
			{
				if (opt == "Scrap" && ___selected.ownershipHistory.Contains(3UL) && ___selected.ownershipHistory.Contains(8UL))
				{
					___selected.performUndock(PLAYER.currentSession);
					foreach (InventoryItem inventoryItem in HWFriendlyPiratesCalledEvent.getRandomScrapLoot())
					{
						if(PLAYER.avatar.placeInFirstSlot(inventoryItem))
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
				if(session.GetType() == typeof(BattleSessionSP) && Globals.eventflags[GlobalFlag.Sige1EventActive] && __instance.ship.id == PLAYER.currentGame.homeBaseId)
				{
					__instance.energy = __instance._maxEnergy;
				}
			}
		}

		[HarmonyPatch(typeof(WidgetJournal), "LoadQuests")] //managing questlog
		public class WidgetJournal_LoadQuests
		{
			[HarmonyPostfix]
			private static void Postfix(WidgetJournal __instance, JournalEntry ___focusedEntry, int ___journalTrackerWidth)
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
				int selectorID = (int)typeof(Canvas).Assembly.GetType("CoOpSpRpG.SelectorCanvas", throwOnError: true).GetField("selectorID", flags).GetValue(typeof(WidgetJournal).GetField("selectorCanvas", flags).GetValue(__instance)); // accessing private field of an internal type with reflection
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

		[HarmonyPatch(typeof(BattleSession), "doCloudInteractions")]		//testing optimisations for cloud distance checks
		public class BattleSession_doCloudInteractions
		{

			[HarmonyPrefix]
			private static bool Prefix(BattleSession __instance, ConcealmentEffect ___cloudCoverEffectInstance)
			{
				if (__instance.cloudyColliders != null)
				{
					for (int i = 0; i < __instance.allShips.Values.Count; i++)
					{
						Ship ship = __instance.allShips.Values.ToList()[i];
						ship.status.Remove(___cloudCoverEffectInstance);
						Point point = checked(new Point((int)ship.position.X, (int)ship.position.Y));
						BattleSession.CloudCollider[] items = __instance.cloudyColliders.GetItems(point); 
						for (int j = 0; j < items.Count(); j++)
						{
							BattleSession.CloudCollider cloudCollider = items[j];
							if (Vector2.DistanceSquared(ship.position, cloudCollider.position) <= cloudCollider.radius * cloudCollider.radius)
							{
								string type = cloudCollider.type;
								string text = type;
								if (text != null)
								{
									if (text == "e")
									{
										ship.status.Add(___cloudCoverEffectInstance);
									}
								}
								break;
							}
						}
					}
				}
				return false;
			}
		}

		*/
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
