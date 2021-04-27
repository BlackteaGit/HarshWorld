﻿using CoOpSpRpG;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static HarshWorld.Interruption;
using Module = CoOpSpRpG.Module;

namespace HarshWorld
{
    public static class HWBaseSiegeEvent
	{
		private static Vector2 eventposition;
		private static Point grid;
		private static DialogueSelectRev2 dialogue;
		private static float intruderTimer = 0f;
		private static float lootTimer = 0f;
		public static ModTile targetModule = null;
		private static ModTile POIConsole = null;
		private static ModTile POICargobay = null;
		private static ModTile POIcloneVats = null;
		private static ModTile POIAirlock = null;
		private static DockSpot POIdockSpot = null;
		private static Vector2 CBPos;
		private static Vector2 CSPos;
		private static Vector2 CVPos;
		private static Vector2 ALPos;
		public static ToolTip tip;
		public static bool seconddeath = false;
		private static bool thirddeath = false;
		private static bool unlockAirlocks = false;
		private static bool buildShip = false;
		private static bool stealShip = false;
		private static bool leaving = false;
		
		
		public static void initialize ()
		{
			intruderTimer = 0f;
			lootTimer = 0f;
			targetModule = null;
			POIConsole = null;
			POICargobay = null;
			POIcloneVats = null;
			POIAirlock = null;
			POIdockSpot = null;
			seconddeath = false;
			unlockAirlocks = false;
			thirddeath = false;
			buildShip = false;
			stealShip = false;
			leaving = false;
		}
		public class HWBaseSiege1EventQuest : TriggerEvent //placeholder event class, only for quest description in questjournal
		{
			public static string staticName = "homebase_siege";
			public HWBaseSiege1EventQuest()
			{
				this.name = HWBaseSiege1EventQuest.staticName;
				this.tip = new ToolTip();
				this.tip.tip = "Defend your homestation";
				tip.setDescription("Find out what the attackers want and get rid of them.");
			}

			public override bool test(float elapsed)
			{
				if (this.tip == null)
				{
					this.tip = new ToolTip();
					this.tip.tip = "Defend your homestation";
					tip.setDescription("Find out what the attackers want and get rid of them.");
					return true;
				}
				return false;
			}
		}

		public static void interruptionUpdate(float elapsed, BattleSession session, Interruption InterruptionInstance)
		{
			bool leave = false;
			bool intrudersonboard = false;
			eventposition = InterruptionInstance.position;
			grid = InterruptionInstance.grid;


			 if (Globals.eventflags[GlobalFlag.Sige1EventActive] && CheckHostileIntrudersCount(session) <= 0)
			 {
				intrudersonboard = false;
			 }
			 else
			 {
				intrudersonboard = true;
			 }
			//winnining conditions
			//all ships and intruders are killed
			if (InterruptionInstance.activeShips.Count() == 0 && (InterruptionInstance.wavesQueued == 0 || InterruptionInstance.currentWave >= InterruptionInstance.maxWaves) && InterruptionInstance.initWaveQueued == false && !intrudersonboard)
			{
				Globals.eventflags[GlobalFlag.Sige1EventActive] = false;
			}
			if (Globals.eventflags[GlobalFlag.Sige1EventActive] && intrudersonboard && InterruptionInstance.activeShips.Count() == 0 && (InterruptionInstance.wavesQueued == 0 || InterruptionInstance.currentWave >= InterruptionInstance.maxWaves) && InterruptionInstance.initWaveQueued == false)
			{
				buildShip = true; //No active besieger ships left, some crew is still on the homebase, try to build a ship from players ressources.
			}

			if (Globals.eventflags[GlobalFlag.Sige1EventActive] && Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] && !intrudersonboard) //all intruders are killed or left the station after phase2 and some have stolen goods in inventory
			{
				for(int i = 0; i < InterruptionInstance.activeShips.Count; i++)
				{
					if (session.allShips.TryGetValue(InterruptionInstance.activeShips[i].Item1, out Ship ship))
					{
						if (ship.id != PLAYER.currentShip.id && ship.cosm?.crew != null)
						{
							for(int c = 0; c < ship.cosm.crew.Values.ToList().Count; c++ )
							{
								if ( hasStolenResources(ship.cosm.crew.Values.ToList()[c]) || stealShip)
								{
									//win condition for besiegers, they consider looting player's homebase a success and no longer profitable and fly away
									leave = true;
									goto finished;
								}
                            }
							
						}
					}
				}
				finished:;
            }

			if (tip == null) //initialasing quest description with phase 1 (before lockdown)
			{
				tip = new ToolTip();
				tip.tip = "Defend your homestation";
				tip.setDescription("Find out what the attackers want and get rid of them.");
			}

			// updating event if win/lose conditions not met
			if (Globals.eventflags[GlobalFlag.Sige1EventActive] && PLAYER.currentGame != null && PLAYER.currentWorld != null && PLAYER.currentSession != null && PLAYER.avatar != null && !leave)
			{			
				//if player is on a AI managed ship which he hacked, remove it from AI management
				if (PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && PLAYER.currentShip.ownershipHistory.Contains(PLAYER.avatar.faction) && PLAYER.currentShip.ownershipHistory.Contains(InterruptionInstance.interruptersFaction))
				{
					for (int j = 0; j < InterruptionInstance.activeShips.Count<Tuple<ulong, List<String>>>(); j++)
					{
						if (session.allShips.TryGetValue(InterruptionInstance.activeShips[j].Item1, out Ship ship))
						{
							if (ship.id == PLAYER.currentShip.id)
							{
								ship.ownershipHistory.Remove(InterruptionInstance.interruptersFaction);
								InterruptionInstance.activeShips.Remove(InterruptionInstance.activeShips[j]);
								break;
							}
						}
					}
					PLAYER.currentShip.ownershipHistory.Remove(InterruptionInstance.interruptersFaction);
				}

				//if player is on the homebase
				if (PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId && PLAYER.avatar.currentCosm != null && PLAYER.avatar.currentCosm.crew != null) 
				{
					homeBaseInteriorLogic(elapsed, session, InterruptionInstance, intrudersonboard);
				}
			}
			else if(leave && !leaving) //besiegers are chanceling the siege
			{
				// *code to fly away until reach a certain point and despawn*
				var direction = Vector2.Transform(InterruptionInstance.spawnPoints[RANDOM.getRandomNumber(InterruptionInstance.spawnPoints.Count<Vector2>())], InterruptionInstance.rotationMatrix);
                foreach (var tupleship in InterruptionInstance.activeShips)
                {
                    tupleship.Item2.Clear();
                    tupleship.Item2.AddRange(new List<string> { "We are going home.", "Let's go, boys.", "Time to leave this shithole.", "Powering engines.", "What a shitty place." });
                    if (PLAYER.currentSession.allShips.TryGetValue(tupleship.Item1, out Ship ship))
                    {
						if (ship.dockedAt != null)
						{
							SCREEN_MANAGER.widgetChat.AddMessage("Undocking ship.", MessageTarget.Ship);
							ship.performUndock(session);
						}
						if (ship.cosm?.crew != null)
                        {
                            foreach (var crew in ship.cosm.crew.Values)
                            {
                                crew.team.destination = ship.position + (ship.position + direction); //setting a destination to fly away
								crew.team.threats.Clear();;
								crew.team.goalType = ConsoleGoalType.warp_jump;
								/*
                                if (crew.team.threats.Contains(PLAYER.avatar.faction))
                                    crew.team.threats.Remove(PLAYER.avatar.faction);  // hostile npcs will become friendly again.
								*/
							}
                        }
                    }
                }
				leaving = true;
			}

			if (!Globals.eventflags[GlobalFlag.Sige1EventActive] || (ShipsOutOfRange(session, InterruptionInstance) && leave)) //despawn conditions
			{
				foreach (var tupleship in InterruptionInstance.activeShips)
				{
					tupleship.Item2.Clear();  // try to debug if memory is not cleared from old event before new event spawns
				}

				if (PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId) //restoring cosm defaults if encounter is about to despawn and player is on homebase
				{
					PROCESS_REGISTER.currentCosm.klaxonOverride = false;
					PROCESS_REGISTER.currentCosm.interiorLightType = InteriorLightType.normal;
					Globals.eventflags[GlobalFlag.Sige1EventLockdown] = false;
					foreach (Module module in PROCESS_REGISTER.currentCosm.modules)
					{
						if (module.type == ModuleType.airlock)
						{
							if ((module as Airlock).locked)
							{
								(module as Airlock).locked = false;
							}
						}
					}
				}
				else  //restoring cosm defaults if encounter is about to despawn and player is not on homebase
				{
					if (session != null)
					{
						foreach (Station station in session.stations)
						{
							if (station.id == PLAYER.currentGame.homeBaseId && station.cosm != null && station.cosm.alive)
							{
								station.cosm.klaxonOverride = false;
								station.cosm.interiorLightType = InteriorLightType.normal;
								Globals.eventflags[GlobalFlag.Sige1EventLockdown] = false;
								foreach (Module module in station.cosm.modules)
								{
									if (module.type == ModuleType.airlock)
									{
										if ((module as Airlock).locked)
										{
											(module as Airlock).locked = false;
										}
									}
								}
							}
						}
					}
				}
				initialize();
				Globals.eventflags[GlobalFlag.Sige1EventActive] = false; //if other despawning conditions active
				Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] = false;
				HWSPAWNMANAGER.DespawnInterruptionAsync(InterruptionInstance.id).SafeFireAndForget();
			}

		}

		private static void homeBaseInteriorLogic(float elapsed, BattleSession session, Interruption InterruptionInstance, bool intrudersonboard)
		{
			/*
					 * A lot of spaghetti code, I know.
					 * Please don't judge, LOL
					 */

			if (Globals.eventflags[GlobalFlag.Sige1EventLockdown])
			{
				PROCESS_REGISTER.currentCosm.interiorLightType = InteriorLightType.battlestations; //restoring lockdown on gameload
			}

			bool hackingsuccess = false;
			intruderTimer += elapsed;
			if (intruderTimer >= Math.Min(InterruptionInstance.activeShips.Count * 20f, 160f)) //spawning intruders
			{
				intruderTimer = 0f;

				if (HWCONFIG.GlobalDifficulty > 0 && PROCESS_REGISTER.currentCosm.interiorLightType != InteriorLightType.battlestations
				&& Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] == false && seconddeath == false && PLAYER.currentShip.cosm.crew.Count() < Squirrel3RNG.Next(12,17)) //spawning only in 1st phase
				{
					if (SCREEN_MANAGER.dialogue == null && !PROCESS_REGISTER.currentCosm.klaxonOverride)
					{
						intrudersDialogue(InterruptionInstance.interruptersFaction);  //Spawning intruders with dialogue (if no intruders are present on station)
						SCREEN_MANAGER.widgetChat.AddMessage("Intruders detected.", MessageTarget.Ship);
					}
					else if (PROCESS_REGISTER.currentCosm.klaxonOverride) //Spawning intruders without dialogue (if some intruders are already present on station)
					{
						int num = Squirrel3RNG.Next(3) + 1;
						CrewTeam crewTeam = new CrewTeam();
						crewTeam.threats.Add(PLAYER.avatar.faction);
						for (int i = 0; i < num; i++)
						{
							Crew crew = new Crew();
							crew.outfit(HWCONFIG.GlobalDifficulty * PLAYER.currentShip.cosm.crew.Count() * 2);
							crew.faction = InterruptionInstance.interruptersFaction;
							crew.team = crewTeam;
							List<Vector2> list = new List<Vector2>();
							for (int j = 0; j < 6; j++)
							{
								list.Add(PLAYER.currentShip.cosm.randomCrewLocation());
							}
							Vector2 pendingPosition = PLAYER.avatar.position;
							float num2 = 0f;
							foreach (Vector2 vector in list)
							{
								float num3 = Vector2.Distance(vector, PLAYER.avatar.position);
								if (num3 > num2)
								{
									pendingPosition = vector;
									num2 = num3;
								}
							}
							crew.pendingPosition = pendingPosition;
							PLAYER.currentShip.cosm.addCrew(crew);
							SCREEN_MANAGER.widgetChat.AddMessage("Intruders detected.", MessageTarget.Ship);
						}
					}
				}

			}

			if (intrudersonboard) //setting alarm if intruders are present and station is not in in phase 2 (lockdown)
			{
				unlockAirlocks = false;
				if (PROCESS_REGISTER.currentCosm.interiorLightType != InteriorLightType.battlestations)
				{
					PROCESS_REGISTER.currentCosm.klaxonOverride = true;
				}
				else
				{
					PROCESS_REGISTER.currentCosm.klaxonOverride = false;
				}
			}
			else //reset to phase 1 (before lockdown) if all intruders are killed
			{
				PROCESS_REGISTER.currentCosm.klaxonOverride = false;
				unlockAirlocks = true; //exit phase 2
				Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] = false;
				seconddeath = false;
			}

			if (!seconddeath && PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.battlestations) //restoring state flags after game load if saved in phase 2 (lockdown)
			{
				SCREEN_MANAGER.widgetChat.AddMessage("Emergency lockdown active. Airlocks latched.", MessageTarget.Ship);
				seconddeath = true;
				foreach (Module module in PROCESS_REGISTER.currentCosm.modules)
				{
					if (module.type == ModuleType.airlock)
					{
						if (!(module as Airlock).locked)
						{
							(module as Airlock).locked = true;
						}
					}
				}
				tip.tip = "Defend your homestation";
				tip.setDescription("Kill the attackers before they can steal your ressources and hack the airlocks open. Or make them leave by destroying their ships.");
				try
				{
					if (SCREEN_MANAGER.questJournal != null)
					{
						SCREEN_MANAGER.questJournal.LoadQuests();
					}
				}
				catch
				{
				}
			}

			if (Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] && seconddeath && PROCESS_REGISTER.currentCosm.klaxonOverride == true && tip.description != "Do not let the attackers escape with your ressources.") //restoring state flags after game load if saved in phase 3 (after lockdown)
			{
				tip.tip = "Defend your homestation";
				tip.setDescription("Do not let the attackers escape with your ressources.");
				try
				{
					if (SCREEN_MANAGER.questJournal != null)
					{
						SCREEN_MANAGER.questJournal.LoadQuests();
					}
				}
				catch
				{
				}
			}


			if (Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] && PLAYER.avatar.state != CrewState.dead)
			{
				if (PLAYER.animateRespawn) 
				{
					if (PROCESS_REGISTER.currentCosm.interiorLightType != InteriorLightType.battlestations && (!seconddeath || thirddeath) && PROCESS_REGISTER.currentCosm.klaxonOverride == true) //progressing to phase 2 (lockdown)
					{
						SCREEN_MANAGER.widgetChat.AddMessage("Station operator livesigns not detected. Hostiles detected.", MessageTarget.Ship);
						SCREEN_MANAGER.widgetChat.AddMessage("Initiating emergency lockdown. Latching airlocks.", MessageTarget.Ship);
						PROCESS_REGISTER.currentCosm.interiorLightType = InteriorLightType.battlestations;
						Globals.eventflags[GlobalFlag.Sige1EventLockdown] = true;
						seconddeath = true;										
						if(!thirddeath)
						{ 
							LockdownDialogue();
						}
						else
						{
							LockdownDialogue2();
						}
						thirddeath = false;
						foreach (Module module in PROCESS_REGISTER.currentCosm.modules)
						{
							if (module.type == ModuleType.airlock)
							{
								if (!(module as Airlock).locked)
								{
									(module as Airlock).locked = true;
								}
							}
						}
						tip.tip = "Defend your homestation";
						tip.setDescription("Kill the attackers before they can steal your ressources and hack the airlocks open. Or make them leave by destroying their ships.");
					}
				}
			}



			if (POIcloneVats == null)
			{
				foreach (CoOpSpRpG.Module module in PROCESS_REGISTER.currentCosm.modules)
				{

					if (module.type == ModuleType.cloning_vat)
					{
						CVPos = (module as CloningVat).spawnOffset;
						break;
					}

				}
			}

			if (POICargobay == null)
			{
				foreach (CoOpSpRpG.Module module in PROCESS_REGISTER.currentCosm.modules)
				{

					if (module.type == ModuleType.cargo_bay)
					{
						POICargobay = (module as CargoBay).tiles[0];
						break;
					}
				}
			}

			if (POIConsole == null)
			{
				foreach (CoOpSpRpG.Module module in PROCESS_REGISTER.currentCosm.modules)
				{

					if (module.type == ModuleType.Console_Access)
					{
						POIConsole = (module as ConsoleAccess).tiles[0];
						break;
					}
				}
			}

			if (POIAirlock == null)
			{
				findAirlock();
			}

			if (POIdockSpot == null)
			{
				DockSpot dockSpot = null;
				foreach (DockSpot dockSpot2 in PLAYER.currentShip.docking)
				{
					if (dockSpot2.docked == null && dockSpot2.airlock.tiles[0] == POIAirlock)
					{
						POIdockSpot = dockSpot = dockSpot2;
						break;
					}
				}
			}

			var playerdistanceToCV = Vector2.DistanceSquared(CVPos, PLAYER.avatar.position);

			for (int i = 0; i < PLAYER.avatar.currentCosm.crew.Values.Count; i++) //managing intruders
			{
				var crew = PLAYER.avatar.currentCosm.crew.Values.ToList()[i];
				if (!crew.isPlayer && crew.faction != PLAYER.avatar.faction && crew.state != CrewState.dead)
				{
					if (crew.team.threats.Contains(PLAYER.avatar.faction) && Vector2.DistanceSquared(crew.position, PLAYER.avatar.position) < 100000f * 100000f) //goals for hostile intruders
					{
						if (!Globals.eventflags[GlobalFlag.Sige1EventPlayerDead]) //state before player died once
						{
							if (PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.normal) //default setting
							{
								crew.attackTarget(PLAYER.avatar);//set goal to move towards player
							}
							else
							{
								Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] = true;
								if (hasStolenResources(crew) || stealShip)
								{
									crew.setGoal(POIAirlock);
								}
								else
								{
									crew.setGoal(POICargobay);
								}
							}
						}
						if (Globals.eventflags[GlobalFlag.Sige1EventPlayerDead]) //player died once, phase 1 completed
						{

							if (PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.normal && (hasStolenResources(crew) || stealShip) && testLOS(crew, PLAYER.avatar.position, crew.currentCosm))// && Vector2.DistanceSquared(crew.position, PLAYER.avatar.position) <= 150000) //default setting
							{
								crew.attackTarget(PLAYER.avatar);//set goal to move towards player
							}

							if (PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.battlestations && playerdistanceToCV > 45000 && (hasStolenResources(crew) || stealShip) && testLOS(crew, PLAYER.avatar.position, crew.currentCosm))// && Vector2.DistanceSquared(crew.position, PLAYER.avatar.position) <= 150000) //player is in lockdown and not in CV room
							{
								crew.attackTarget(PLAYER.avatar);//set goal to move towards player
							}

							if (PLAYER.animateRespawn)
							{
								if (targetModule == null)
								{
									if (!hasStolenResources(crew) && !stealShip)
									{
										targetModule = POICargobay;
										crew.setGoal(targetModule); //reset to stealing items every time player is respawning
									}
									else
									{
										targetModule = POIConsole;
										crew.setGoal(targetModule); //reset to stealing items every time player is respawning
									}
								}
								else if (crew.state == CrewState.attacking)
								{
									crew.setGoal(targetModule); //reset to stealing items every time player is respawning
								}
							}

							if (PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.normal && crew.state == CrewState.attacking && (hasStolenResources(crew) || stealShip) && crew.target != crew.position && !testLOS(crew, PLAYER.avatar.position, crew.currentCosm))// && Vector2.DistanceSquared(crew.position, PLAYER.avatar.position) > 150000)
							{
								crew.setGoal(POIAirlock);
							}

							if (PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.battlestations && crew.state == CrewState.attacking && (hasStolenResources(crew) || stealShip) && crew.target != crew.position && !testLOS(crew, PLAYER.avatar.position, crew.currentCosm))// && Vector2.DistanceSquared(crew.position, PLAYER.avatar.position) > 150000)
							{
								crew.setGoal(POIConsole);
							}

							if (crew.state == CrewState.attacking && playerdistanceToCV <= 45000)
							{
								if (targetModule == null)
								{
									if (!hasStolenResources(crew) && !stealShip)
									{
										targetModule = POICargobay;
										crew.setGoal(targetModule);
									}
									else
									{
										targetModule = POIConsole;
										crew.setGoal(targetModule);
									}
								}
								else
								{
									crew.setGoal(targetModule);
								}
							}

							if (crew.state != CrewState.attacking && (crew.target == crew.position || crew.state == CrewState.idle || crew.state == CrewState.operating) && POICargobay != null && POIConsole != null && POIAirlock != null) //check if at target position of his current goal
							{
								//calculating current position of the crewmember relative to POIs
								CBPos = new Vector2((float)(POICargobay.X % crew.currentCosm.width * 16), (float)(POICargobay.X / crew.currentCosm.width * 16));
								CSPos = new Vector2((float)(POIConsole.X % crew.currentCosm.width * 16), (float)(POIConsole.X / crew.currentCosm.width * 16));
								ALPos = new Vector2((float)(POIAirlock.X % crew.currentCosm.width * 16), (float)(POIAirlock.X / crew.currentCosm.width * 16));
								var distanceToCB = Vector2.DistanceSquared(CBPos, crew.position);
								var distanceToCS = Vector2.DistanceSquared(CSPos, crew.position);
								var distanceToAL = Vector2.DistanceSquared(ALPos, crew.position);

								if (distanceToCB < 4400f) //check if target position is near cargobay
								{
									lootTimer += elapsed;
									if (lootTimer >= 1f)
									{
										lootTimer = 0f;
										if (!stealRandomResource(crew)) //if some random ressourcers successfuly stolen, go to the airlock to transfer them to the ship.
										{
											if (PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.normal)
											{
												crew.setGoal(POIAirlock);
											}
											else // if in phase 2 (lockdown) some random ressourcers successfuly stolen, go to console for hacking airlocks open
											{
												crew.setGoal(POIConsole);
											}
										}
										else // stealing item was unsuccessful for whatever reson
										{
											if (CHARACTER_DATA.getAllResources().Values.ToList().TrueForAll(amount => amount <= 0)) // if the reason of unsuccessful stealing is insufficent player ressources
											{
												InventoryItem Item = new InventoryItem(InventoryItemType.exotic_matter);
												Item.stackSize = 1;
												if (!stealSomeCredits(crew, Item)) // try to steal credits
												{
													if (PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.normal)
													{
														crew.setGoal(POIAirlock);
													}
													else // if in phase 2 (lockdown) credits successfuly stolen, go to console for hacking airlocks open
													{
														crew.setGoal(POIConsole);
													}
												}
												else // stealing credits was unsuccessful for whatever reson
												{
													if (CHARACTER_DATA.credits < (Item.refineValue * Item.stackSize)) // If player has no ressources and no money
													{
														// try to steal player's ship or just leave
														if(!stealShip)
														{
															stealShip = true;
															findAirlock();
														}
														crew.setGoal(POIConsole);
													}
												}
											}
										}
									}
									goto Finish;
								}
								if (distanceToCS < 27000f) //check if target position is near console
								{
									if (PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.battlestations) // if phase 2, hack to unlock airlocks
									{
										if (Squirrel3RNG.Next(1000) == 1)
										{
											crew.GetfloatyText().Enqueue("hacking airlocks");
										}
										if (Squirrel3RNG.Next(20000) == 1)
										{
											hackingsuccess = true;
											crew.GetfloatyText().Enqueue("hacking airlocks successful");
											for (i = 0; i < InterruptionInstance.activeShips.Count; i++)
											{
												if (POIdockSpot != null && POIdockSpot.docked != null)
												{
													//make the docked ship enemy and add to interruption.activeships if it is a player ship
													if (POIdockSpot.docked.parent.ownershipHistory.Contains(PLAYER.avatar.faction))
													{
														POIdockSpot.docked.parent.ownershipHistory.Remove(PLAYER.avatar.faction);
														POIdockSpot.docked.parent.ownershipHistory.Add(InterruptionInstance.interruptersFaction);
														POIdockSpot.docked.parent.faction = InterruptionInstance.interruptersFaction;
														POIdockSpot.docked.parent.hackingAvailable = 1;
														InterruptionInstance.activeShips.Add(new Tuple<ulong, List<string>>(POIdockSpot.docked.parent.id, InterruptionInstance.conversations));
														buildShip = false;
														crew.GetfloatyText().Enqueue("ship hacking successful");
														SCREEN_MANAGER.widgetChat.AddMessage("Ship computer at airlock 1 is not responding.", MessageTarget.Ship);
														ShiphackedDialogue();
													}
													break;
												}
												if (session.allShips.TryGetValue(InterruptionInstance.activeShips[i].Item1, out var ship)) ;
												{

													DockSpot dockSpot = null;
													foreach (DockSpot dockSpot2 in PLAYER.currentShip.docking)
													{
														if (dockSpot2.docked == null && dockSpot2.airlock.tiles[0] == POIAirlock)
														{
															POIdockSpot = dockSpot = dockSpot2;
															break;
														}
													}
													if (dockSpot != null && ship.docking != null && ship.docking.Count != 0 && ship.cosm?.crew != null && ship.cosm.alive)
													{
														//check if the ship has a working airlock			
														if (ship.cosm?.airlocks != null && !ship.cosm.airlocks.TrueForAll(airlock => !airlock.functioning))
														{
															PLAYER.currentShip.connectShips(ship, dockSpot.pivotTile, ship.docking.First<DockSpot>().pivotTile);
															ship.cosm.crew.Values.First().team.goalType = ConsoleGoalType.none;
															crew.GetfloatyText().Enqueue("docking initiated");
															break;
														}
													}
												}
											}
											if (POIdockSpot != null && POIdockSpot.docked == null && buildShip)
											{
//TODO>>build ship from players ressources>>>
												if (crew.team.threats.Contains(PLAYER.avatar.faction)) //if building a ship is impossible intruders will give up and become neutral
													crew.team.threats.Remove(PLAYER.avatar.faction);
												SCREEN_MANAGER.widgetChat.AddMessage("Intruders can't find a ship to escape and surrender.", MessageTarget.Ship);
											}
										}
									}
									else //if phase 3 go to the airlock
									{

										if (POIdockSpot != null && POIdockSpot.docked == null && !buildShip) //if in phase 3 and escape ship is missing
										{
											if (Squirrel3RNG.Next(1000) == 1)
											{
												crew.GetfloatyText().Enqueue("initating docking");
											}
											if (Squirrel3RNG.Next(2000) == 1)
											{
												for (i = 0; i < InterruptionInstance.activeShips.Count; i++)
												{
													if (session.allShips.TryGetValue(InterruptionInstance.activeShips[i].Item1, out var ship)) ;
													{
														if (POIdockSpot != null && ship.docking != null && ship.docking.Count != 0 && ship.cosm?.crew != null && ship.cosm.alive)
														{
															//check if the ship has a working airlock			
															if (ship.cosm?.airlocks != null && !ship.cosm.airlocks.TrueForAll(airlock => !airlock.functioning))
															{
																PLAYER.currentShip.connectShips(ship, POIdockSpot.pivotTile, ship.docking.First<DockSpot>().pivotTile);
																ship.cosm.crew.Values.First().team.goalType = ConsoleGoalType.none;
																crew.GetfloatyText().Enqueue("docking initiated");
																if (hasStolenResources(crew) || stealShip)
																{
																	crew.setGoal(POIAirlock);
																}
																else
																{
																	crew.setGoal(POICargobay);
																}
																break;
															}
														}
													}
												}
											}
										}
										else
										{
											if (POIdockSpot != null && POIdockSpot.docked != null)
											{
												if (POIdockSpot.docked.parent.ownershipHistory.Contains(PLAYER.avatar.faction))
												{
													POIdockSpot.docked.parent.ownershipHistory.Remove(PLAYER.avatar.faction);
													POIdockSpot.docked.parent.ownershipHistory.Add(InterruptionInstance.interruptersFaction);
													POIdockSpot.docked.parent.faction = InterruptionInstance.interruptersFaction;
													POIdockSpot.docked.parent.hackingAvailable = 1;
													InterruptionInstance.activeShips.Add(new Tuple<ulong, List<string>>(POIdockSpot.docked.parent.id, InterruptionInstance.conversations));
													buildShip = false;
													crew.GetfloatyText().Enqueue("ship hacking successful");
													SCREEN_MANAGER.widgetChat.AddMessage("Ship computer at airlock 1 is not responding.", MessageTarget.Ship);
													ShiphackedDialogue();
												}
											}
											if (POIdockSpot != null && POIdockSpot.docked == null && buildShip)
											{
//TODO>>build ship from players ressources>>>
												if (crew.team.threats.Contains(PLAYER.avatar.faction)) //if building a ship is impossible intruders will give up and become neutral
													crew.team.threats.Remove(PLAYER.avatar.faction);
												SCREEN_MANAGER.widgetChat.AddMessage("Intruders can't find a ship to escape and surrender.", MessageTarget.Ship);
											}
											if (hasStolenResources(crew) || stealShip)
											{
												crew.setGoal(POIAirlock);
											}
											else
											{
												crew.setGoal(POICargobay);
											}
										}

									}
									goto Finish;
								}

								if (distanceToAL < 27500f) //check if target position is near airlock
								{
									if (POIdockSpot != null && POIdockSpot.docked != null && PROCESS_REGISTER.currentCosm.interiorLightType != InteriorLightType.battlestations) // checking if escape ship is avaible
									{
										if (POIdockSpot.docked.parent != null && !transToShip(crew, POIdockSpot.docked.parent)) // try transfering crew to the ship
										{
											try
											{
												POIdockSpot.docked.parent.performUndock(session); //if transfering crew failed for some reason
											}
											catch
											{

											}
											POIdockSpot.docked = null;					
										}
									}
									else
									{
										if (PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.battlestations)
										{
											if (hasStolenResources(crew) || stealShip)
											{
												crew.setGoal(POIConsole);
											}
											else
											{
												crew.setGoal(POICargobay);
											}
										}
										else
										{
											Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] = false; // if not reset to phase 1 , but skip the phase 2 after player is killed (seconddeath flag still true)
																										 //debug message
											SCREEN_MANAGER.widgetChat.AddMessage("Phase1 reset 1. ( kill player without lockdown)", MessageTarget.Ship);
											seconddeath = true;
										}
									}
									goto Finish;
								}

								//condition if crew is standing outside of the any POI and has no POI assigned to walk to
								if (targetModule == null) //after loading the game
								{
									if (!hasStolenResources(crew) && !stealShip)
									{
										targetModule = POICargobay;
										crew.setGoal(targetModule);
									}
									else
									{
										if (POIdockSpot != null && POIdockSpot.docked != null && Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] == true) // checking if escape ship is avaible and phase 3
										{
											targetModule = POIAirlock;
											crew.setGoal(targetModule);
										}
										else
										{
											if (PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.battlestations) //if no escape ship in phase 2 continue hacking console
											{
												targetModule = POIConsole;
												crew.setGoal(targetModule);
											}
											else if (seconddeath != false)
											{
												Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] = false; //if no escape ship in phase 3 reset to phase 1, but skip the phase 2 after player is killed (seconddeath flag still true)
//debug message														
												SCREEN_MANAGER.widgetChat.AddMessage("Phase1 reset 2. (kill player with lockdown)", MessageTarget.Ship);
												seconddeath = true;
												thirddeath = true;
												targetModule = POIConsole;
												crew.setGoal(targetModule);
											}
											else
											{
												targetModule = POIAirlock;
												crew.setGoal(targetModule);
											}
										}
									}
								}
								else
								{
									if (hasStolenResources(crew) || stealShip)
									{
										if (POIdockSpot != null && POIdockSpot.docked != null && PROCESS_REGISTER.currentCosm.interiorLightType != InteriorLightType.battlestations)
										{
											crew.setGoal(POIAirlock);
										}
										else //no docked ship
										{
											if (PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.battlestations) //if in phase2
											{
												if (crew.state != CrewState.operating)
												{
													crew.setGoal(POIConsole);
												}
												else
												{
													crew.setGoal(POICargobay);// crew operating console however wandered awy and is not wthin "target" distance, reset to cargobay
												}
											}
											else
											{
												if (Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] == false && seconddeath == true)
												{
													crew.setGoal(POIConsole); // if phase 3, but missing ship already detected as missing
												}
												else
												{
													crew.setGoal(POIAirlock);// if phase 3, but missing ship not detected as missing
												}
											}
										}
									}
									else
									{
										crew.setGoal(POICargobay);
									}
								}
							Finish:;
							}
							else if (crew.state != CrewState.attacking && crew.target != crew.position && crew.state == CrewState.idle && Vector2.DistanceSquared(crew.target, crew.position) < 50 * 50) //got stuck along the way on saveload in a inaccurate floating point position
							{
								if (targetModule == null) //after loading the game
								{
									if (!hasStolenResources(crew) && !stealShip)
									{
										targetModule = POICargobay;
										crew.setGoal(targetModule);
									}
									else
									{
										if (POIdockSpot != null && POIdockSpot.docked != null && Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] == true) // checking if escape ship is avaible and phase 3
										{
											targetModule = POIAirlock;
											crew.setGoal(targetModule);
										}
										else
										{
                                            if (PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.battlestations) //if no escape ship in phase 2 continue hacking console
                                            {
                                                targetModule = POIConsole;
                                                crew.setGoal(targetModule);
                                            }
                                            else if (seconddeath != false)
                                            {
                                                Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] = false; //if no escape ship in phase 3 reset to phase 1, but skip the phase 2 after player is killed (seconddeath flag still true)
//debug message														
                                                SCREEN_MANAGER.widgetChat.AddMessage("Phase1 reset 2. (kill player with lockdown)", MessageTarget.Ship);
                                                seconddeath = true;
                                                thirddeath = true;
                                                targetModule = POIConsole;
                                                crew.setGoal(targetModule);
                                            }
											else
											{
												targetModule = POIAirlock;
												crew.setGoal(targetModule);
											}
										}
									}
								}
								else
								{
									if (hasStolenResources(crew) || stealShip)
									{
										if (POIdockSpot != null && POIdockSpot.docked != null && PROCESS_REGISTER.currentCosm.interiorLightType != InteriorLightType.battlestations)
										{
											crew.setGoal(POIAirlock);
										}
										else //no docked ship
										{
											if (PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.battlestations) //if in phase2
											{
												if (crew.state != CrewState.operating)
												{
													crew.setGoal(POIConsole);
												}
												else
												{
													crew.setGoal(POICargobay);// crew operating console however wandered awy and is not wthin "target" distance, reset to cargobay
												}
											}
											else
											{
												if (Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] == false && seconddeath == true)
												{
													crew.setGoal(POIConsole); // if phase 3, but missing ship already detected as missing
												}
												else
												{
													crew.setGoal(POIAirlock);// if phase 3, but missing ship not detected as missing
												}
											}
										}
									}
									else
									{
										CBPos = new Vector2((float)(POICargobay.X % crew.currentCosm.width * 16), (float)(POICargobay.X / crew.currentCosm.width * 16));
										var distanceToCB = Vector2.DistanceSquared(CBPos, crew.position);
										if (distanceToCB > 5000)
										{ 
											crew.setGoal(POICargobay);
										}
										else
										{
											crew.setGoal(POIConsole);
										}
									}
								}
							}
						}
					}
				}
			}

			if (PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.battlestations && !PLAYER.animateRespawn && (hackingsuccess || unlockAirlocks))//PLAYER.avatar.heldItem != null)//if hacking complete, not the held item target will become airlock
			{
				unlockAirlocks = false;
				foreach (Module module in PROCESS_REGISTER.currentCosm.modules)
				{
					if (module.type == ModuleType.airlock)
					{
						if ((module as Airlock).locked)
						{
							(module as Airlock).locked = false;
						}
					}
				}
				PROCESS_REGISTER.currentCosm.interiorLightType = InteriorLightType.normal;
				Globals.eventflags[GlobalFlag.Sige1EventLockdown] = false;
				SCREEN_MANAGER.widgetChat.AddMessage("Emergency lockdown deactivated. Airlocks unlatched.", MessageTarget.Ship);
				targetModule = null;
			}

		}

		private static void findAirlock()
		{
			foreach (CoOpSpRpG.Module module in PROCESS_REGISTER.currentCosm.modules)
			{
				if (module.type == ModuleType.airlock && (!stealShip || POIdockSpot == null))
				{	
					POIAirlock = (module as Airlock).tiles[0];
					foreach (DockSpot dockSpot in PLAYER.currentShip.docking)
					{
						if (dockSpot.airlock.tiles[0] == POIAirlock)
						{
							POIdockSpot = dockSpot;
							break;
						}
					}
					break;
				}
				else if(module.type == ModuleType.airlock && stealShip)
				{
					var airlock = (module as Airlock).tiles[0];
					foreach (DockSpot dockSpot in PLAYER.currentShip.docking)
					{
						if (dockSpot.airlock.tiles[0] == airlock && dockSpot.docked != null && dockSpot.docked.parent.ownershipHistory.Contains(PLAYER.avatar.faction))
						{
							POIAirlock = airlock;
							POIdockSpot = dockSpot;
							break;
						}
					}
					break;
				}
			}
		}
		private static bool testLOS(Crew crew, Vector2 target, MicroCosm cosm)
		{
			if (crew.heldItem != null && crew.heldItem.GetType() == typeof(Gun))
			{
				float n = (crew.heldItem as Gun).range;
				float n2 = Vector2.Distance(crew.position, target);
				if (n2 > n)
				{
					return false;
				}
			}
			float num = 0f;
			float num2 = Vector2.Distance(crew.position, target);
			if (num2 == 0f)
			{
				return true;
			}
			Vector2 value = Vector2.Normalize(target - crew.position) * 8f;
			Vector2 vector = crew.position;
			while (num < num2)
			{
				vector += value;
				num += 8f;
				int num3 = (int)(vector.X / 16f);
				int num4 = (int)(vector.Y / 16f);
				if (num3 >= 0 && num3 < cosm.width && num4 >= 0 && num4 < cosm.height)
				{
					int num5 = num3 + num4 * cosm.width;
					if (cosm.tiles[num5].airBlocking && cosm.tiles[num5].A > 0)
					{
						return false;
					}
				}
			}
			return true;
		}

		private static bool hasStolenResources(Crew crew) //checking if crew has some stolen goods in inventory, encumbering (reducing speed) if that is the case
		{
			if (crew.inventory == null)
			{
				if (crew.speed != 175f)
				{
					crew.speed = 175f;
				}
				return false;
			}
			for (int i = 0; i < crew.inventory.Length; i++)
			{
				if (crew.inventory[i] != null && crew.inventory[i].type != InventoryItemType.grey_goo && crew.inventory[i].type != InventoryItemType.repair_gun && crew.inventory[i].type != InventoryItemType.fire_extinguisher
				&& crew.inventory[i].type != InventoryItemType.fire_extinguisher_2 && crew.inventory[i].type != InventoryItemType.gun)
				{
					if (crew.speed == 175f)
					{
						crew.speed = 100f;
					}
					return true;
				}
			}
			if (crew.speed != 175f)
			{
				crew.speed = 175f;
			}
			return false;
        }
		private static bool stealRandomResource(Crew crew)
		{
			var ressourses = CHARACTER_DATA.getAllResources();
			var toSteal = ressourses.Keys.ToList()[Squirrel3RNG.Next(ressourses.Keys.ToList().Count)];
			if(ressourses.Values.ToList().TrueForAll(amount => amount <= 0))
			{
				return true; // stealing unsuccessful
			}
			if(ressourses[toSteal] >= 1)
			{ 
				if(crew.containsItemOfType(toSteal, 5))
				{
					return false; // stealing successful (do not attempt furter stealing)
				}
				else if(crew.containsItemOfType(toSteal) && ressourses[toSteal] == 1)
				{
					CHARACTER_DATA.setResource(toSteal, ressourses[toSteal] - 1);
					InventoryItem Item = new InventoryItem(toSteal);
					Item.stackSize = 1;
					crew.placeInFirstSlot(Item);
					crew.GetfloatyText().Enqueue("+" + Item.stackSize.ToString() + " " + Item.toolTip.tip);
					return false;  // stealing successful (do not attempt furter stealing)
				}
				CHARACTER_DATA.setResource(toSteal, ressourses[toSteal] - 1);
				InventoryItem inventoryItem = new InventoryItem(toSteal);
				inventoryItem.stackSize = 1;
				crew.placeInFirstSlot(inventoryItem);
				crew.GetfloatyText().Enqueue("+" + inventoryItem.stackSize.ToString() + " " + inventoryItem.toolTip.tip);
			}
			return true; // stealing  not successful
		}

		private static bool transToShip (Crew crew, Ship ship)
		{
			//check if the ship has a working airlock			
			if (ship.cosm?.airlocks != null && ship.cosm.airlocks.TrueForAll(airlock => !airlock.functioning))
			{
				SCREEN_MANAGER.widgetChat.AddMessage("Intruder failed entering docked ship.", MessageTarget.Ship);
				return false;
			}
			if (ship != null && ship.cosm != null && ship.cosm.alive)
			{ 
				Crew crew2;
				crew.goalCompleted();
				crew.currentCosm.crew.TryRemove(crew.id, out crew2);
				ship.cosm.addCrew(crew);
				crew.pendingPosition = ship.cosm.randomCrewLocation();
				SCREEN_MANAGER.widgetChat.AddMessage("Intruder entered docked ship.", MessageTarget.Ship);
				return true;
			}
			if(ship != null && ship.cosm == null && ship.crewCount > 0)
			{
				ship.performUndock(PLAYER.currentSession);
				POIdockSpot.docked = null;
				PLAYER.currentShip.connectShips(ship, POIdockSpot.pivotTile, ship.docking.First<DockSpot>().pivotTile);
				if(ship.cosm != null)
				{
					Crew crew2;
					crew.goalCompleted();
					crew.currentCosm.crew.TryRemove(crew.id, out crew2);
					ship.cosm.addCrew(crew);
					crew.pendingPosition = ship.cosm.randomCrewLocation();
					SCREEN_MANAGER.widgetChat.AddMessage("Intruder entered docked ship.", MessageTarget.Ship);
					return true;
				}
			}
			SCREEN_MANAGER.widgetChat.AddMessage("Intruder failed entering docked ship.", MessageTarget.Ship);
			return false;
        }

		private static bool stealSomeCredits(Crew crew, InventoryItem Item)
		{
			if (CHARACTER_DATA.credits < (Item.refineValue * Item.stackSize))
			{
				return true; // stealing unsuccessful
			}
			else
			{
				if (crew.containsItemOfType(Item.type, (int)Item.stackSize))
				{
					return false;  // stealing successful (do not attempt furter stealing)
				}
				else if (crew.containsItemOfType(InventoryItemType.exotic_matter) && CHARACTER_DATA.credits - (ulong)(Item.refineValue * Item.stackSize) >= 0 && CHARACTER_DATA.credits - (ulong)(Item.refineValue * Item.stackSize) <= (ulong)(Item.refineValue * Item.stackSize))
				{				
					crew.placeInFirstSlot(Item);
					crew.GetfloatyText().Enqueue("+" + SCREEN_MANAGER.formatCreditString((ulong)(Item.refineValue * Item.stackSize)) + " credits");
					CHARACTER_DATA.credits -= (ulong)(Item.refineValue * Item.stackSize);
					return false;  // stealing successful (do not attempt furter stealing)
				}				
				crew.placeInFirstSlot(Item);
				crew.GetfloatyText().Enqueue("+" + SCREEN_MANAGER.formatCreditString((ulong)(Item.refineValue * Item.stackSize)) + " credits");
				CHARACTER_DATA.credits -= (ulong)(Item.refineValue * Item.stackSize);
			}
			return true; // stealing unsuccessful
		}

		private static bool ShipsOutOfRange(BattleSession session, Interruption InterruptionInstance)
		{
			int j = 0;
			Tuple<ulong, List<String>> remove = null;
			for (int i = 0; i < InterruptionInstance.activeShips.Count<Tuple<ulong, List<String>>>(); i++)
			{
				if (session.allShips.TryGetValue(InterruptionInstance.activeShips[i].Item1, out Ship ship))
				{
					if (Vector2.DistanceSquared(eventposition, ship.position) >= 15000 * 15000)
					{
						j++;
						if (!leaving && ship.cosm?.crew != null && ship.cosm?.crew.Count > 0)// && ship.cosm.crew.Values.First().team.goalType != ConsoleGoalType.warp_jump)
						{
							j--;
							ship.position = eventposition + RANDOM.squareVector(CONFIG.minViewDist);
							foreach (var crew in ship.cosm.crew.Values)
							{
								// interrupt any fights going on and get the ships to the station
								crew.team.threats.Clear();
								crew.team.threats.Add(2UL);
								crew.team.destination = eventposition;
								crew.team.goalType = ConsoleGoalType.warp_jump;
								InterruptionInstance.activeEffects.Add(new ActiveEffect("WarpIn", ship.position, 3f, 0f));
//DEBUG message>>>>>>>>>>		
							}
							SCREEN_MANAGER.widgetChat.AddMessage("Ship signature detected entering sensor range.", MessageTarget.Ship);
						}
					}
					else if (ship.engineEnergy <= 0 && leaving && InterruptionInstance.interdictTimer >= 3f)//ship.velocity.Equals(new Vector2(0, 0)))
					{
						remove = InterruptionInstance.activeShips[i];
					}
					else if (leaving && ship.cosm?.crew != null && ship.cosm?.crew.Count > 0 && !ship.cosm.crew.Values.ToList().TrueForAll(crew => crew.team.threats.Count <= 0))
					{
						if (ship.dockedAt != null)
						{
							SCREEN_MANAGER.widgetChat.AddMessage("Undocking ship.", MessageTarget.Ship);
							ship.performUndock(session);
						}
						var direction = Vector2.Transform(InterruptionInstance.spawnPoints[RANDOM.getRandomNumber(InterruptionInstance.spawnPoints.Count<Vector2>())], InterruptionInstance.rotationMatrix);
						foreach (var crew in ship.cosm.crew.Values)
						{
							crew.team.destination = ship.position + (ship.position + direction); //setting a destination to fly away
							crew.team.goalType = ConsoleGoalType.warp_jump;
							crew.team.threats.Clear();
							/*
							if (crew.team.threats.Contains(PLAYER.avatar.faction))
								crew.team.threats.Remove(PLAYER.avatar.faction);  // hostile npcs will become friendly again.
							*/
						}

					}				
				}
			}
			if(remove != null)
			{
				InterruptionInstance.activeShips.Remove(remove);
//DEBUG message>>>>>>>>>>
				SCREEN_MANAGER.widgetChat.AddMessage("Ship abandoned.", MessageTarget.Ship);
			}
			if(InterruptionInstance.activeShips.Count == j)
			{
				return true;
			}
			return false;
		}

		private static int CheckHostileIntrudersCount(BattleSession HomeBaseSession)
		{
			int intruders = 0;
			if(PLAYER.currentShip != null && PLAYER.currentShip.cosm?.crew != null && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId)
			{
				foreach (var crew in PLAYER.currentShip.cosm.crew.Values)
				{
					if(crew.state != CrewState.dead && crew.team.threats.Contains(PLAYER.avatar.faction))
					{
						intruders++;
					}
                }
            }
			else
			{
				if (HomeBaseSession != null)
				{
					foreach (Station station in HomeBaseSession.stations)
					{
						if (station.id == PLAYER.currentGame.homeBaseId && station.cosm?.crew != null && station.cosm.alive)
						{
							foreach (var crew in station.cosm.crew.Values)
							{
								if (crew.state != CrewState.dead && crew.team.threats.Contains(PLAYER.avatar.faction))
								{
									intruders++;
								}
							}
						}
					}
				}
			}
			return intruders;
        }

		public static Crew getFriendlyIntruder(BattleSession HomeBaseSession)
		{
			Crew intruder = null;
			if (PLAYER.currentShip != null && PLAYER.currentShip.cosm?.crew != null && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId)
			{
				foreach (var crew in PLAYER.currentShip.cosm.crew.Values)
				{
					if (crew.state != CrewState.dead && !crew.isPlayer && crew.faction != PLAYER.avatar.faction && !crew.team.threats.Contains(PLAYER.avatar.faction))
					{
						intruder = crew;
						break;
					}
				}
			}
			else
			{
				if (HomeBaseSession != null)
				{
					foreach (Station station in HomeBaseSession.stations)
					{
						if (station.id == PLAYER.currentGame.homeBaseId && station.cosm?.crew != null && station.cosm.alive)
						{
							foreach (var crew in station.cosm.crew.Values)
							{
								if (crew.state != CrewState.dead && !crew.isPlayer && crew.faction != PLAYER.avatar.faction && !crew.team.threats.Contains(PLAYER.avatar.faction))
								{
									intruder = crew;
									break;
								}
							}
						}
					}
				}
			}
			return intruder;
		}

		public static bool test(float elapsed)
		{

			if (Globals.eventflags[GlobalFlag.Sige1EventActive] && Globals.eventflags[GlobalFlag.Sige1EventSpawnDialogueActive] && SCREEN_MANAGER.dialogue == null && PLAYER.currentSession != null && !PLAYER.currentSession.paused)
			{
				SpawnDialogue();
				Globals.eventflags[GlobalFlag.Sige1EventSpawnDialogueActive] = false;
				Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] = false;
				return true;
			}

			if (dialogue != null && dialogue.removeMe)
			{
				dialogue = null;
				PLAYER.currentSession.unpause();
			}

			//restoring homebase cosm defaults if event was despawned by max active events restriction or other unpredicted circumstances
			if (Globals.eventflags[GlobalFlag.Sige1EventActive] && !Globals.eventflags[GlobalFlag.Sige1EventSpawnDialogueActive] && PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId &&
			(Globals.Interruptionbag == null || (Globals.Interruptionbag != null && 
			Globals.Interruptionbag.Values.ToList().TrueForAll(element => (element.templateUsed != InterruptionType.home_siege_pirate_t1 && element.templateUsed != InterruptionType.home_siege_pirate_t2 && element.templateUsed != InterruptionType.home_siege_pirate_t25)))))
			{
				Globals.eventflags[GlobalFlag.Sige1EventActive] = false;
				Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] = false;
				PROCESS_REGISTER.currentCosm.klaxonOverride = false;
				PROCESS_REGISTER.currentCosm.interiorLightType = InteriorLightType.normal;
				Globals.eventflags[GlobalFlag.Sige1EventLockdown] = false;
				foreach (Module module in PROCESS_REGISTER.currentCosm.modules)
				{
					if (module.type == ModuleType.airlock)
					{
						if ((module as Airlock).locked)
						{
							(module as Airlock).locked = false;
						}
					}
				}
				for (int i = 0; i < PLAYER.avatar.currentCosm.crew.Values.Count; i++)
				{
					var crew = PLAYER.avatar.currentCosm.crew.Values.ToList()[i];
					if (!crew.isPlayer && crew.faction != PLAYER.avatar.faction && crew.state != CrewState.dead)
					{
						if (crew.team.threats.Contains(PLAYER.avatar.faction))
						{
							crew._kill();
						}
					}
				}
				//resetting all static flags to defaults
				initialize();
			}
			return false;
		}

		public static void SetupEntry(JournalEntry entry)
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
			entry.quest = new HWBaseSiege1EventQuest();//null;
			if (tip != null)
			{
				entry.name = tip.tip;
				entry.quest.tip = tip;
			}
			else
			{
				entry.name = entry.quest.tip.tip;
			}
			int num = 28;
			entry.addLabel(entry.name, SCREEN_MANAGER.FF16, 4, -2, entry.width, 30, CONFIG.textBrightColor);
			entry.AddCanvas("test canvas", SCREEN_MANAGER.white, 0, 0, entry.width, entry.height - num, SortType.horizontal);
			entry.elementList.Last<GuiElement>().addLabel("Distance", SCREEN_MANAGER.FF12, 4, 0, 60, entry.height - num, new Color(122, 142, 144));
			typeof(JournalEntry).GetField("distanceName", flags).SetValue(entry, entry.elementList.Last<GuiElement>().elementList.Last<GuiElement>()); //entry.distanceName = entry.elementList.Last<GuiElement>().elementList.Last<GuiElement>();
			entry.elementList.Last<GuiElement>().addLabel("n/a", SCREEN_MANAGER.FF12, 4, 0, 70, entry.height - num, new Color(167, 192, 195));
			entry.distanceLabel = entry.elementList.Last<GuiElement>().elementList.Last<GuiElement>();
			entry.trackButton = entry.elementList.Last<GuiElement>().AddCheckBoxAdv("tracked", "untracked", SCREEN_MANAGER.white, SCREEN_MANAGER.MenuArt[269], 0, 0, entry.width - 130 - 8, entry.height - num, new CheckBoxAdv.ClickEvent((element) => entry.clickMapEvent(entry)), SCREEN_MANAGER.FF12, Color.White);
			if (entry.quest != null)
			{
				entry.trackButton.state = entry.quest.tracked;
			}
			entry.currentValue = -1f;
			entry.distanceLabel.isVisible = false;
			GuiElement distanceName = typeof(JournalEntry).GetField("distanceName", flags).GetValue(entry) as GuiElement; //retreiving value of private field via reflection
			distanceName.isVisible = false; //entry.distanceName.isVisible = false;
			typeof(JournalEntry).GetField("distanceName", flags).SetValue(entry, distanceName); //saving changed value to the field via reflection
		}
		public static void getDistance(JournalEntry entry)
		{
			if(eventposition != new Vector2() && grid != new Point())
			{
				entry.SetTarget(eventposition, grid);
			}
			else
			{
				entry.SetTarget(new Vector2(-4000, 4000), new Point(316, -183));
			}
		}
		private static void intrudersDialogue(ulong faction)
		{
			DialogueTree dialogueTree = new DialogueTree();
			DialogueTree dialogueTree2 = new DialogueTree();
			dialogueTree.text = "Intruder alert. They've breached our hull!";
			switch (Squirrel3RNG.Next(3))
			{
				case 0:
					dialogueTree.addOption("That's bad!", dialogueTree2);
					break;
				case 1:
					dialogueTree.addOption("Oh how nice I wonder what they want.", dialogueTree2);
					break;
				case 2:
					dialogueTree.addOption("Maybe they are friendly.", dialogueTree2);
					break;
			}
			dialogueTree.addOption("Let's kill them all!", dialogueTree2);
			dialogueTree2.action = delegate ()
			{
				if (PLAYER.currentShip != null && PLAYER.currentShip.cosm != null)
				{
					int num = Squirrel3RNG.Next(2) + 3;
					CrewTeam crewTeam = new CrewTeam();
					crewTeam.threats.Add(PLAYER.avatar.faction);
					for (int i = 0; i < num; i++)
					{
						Crew crew = new Crew();
						crew.outfit(0f);
						crew.faction = faction;
						crew.team = crewTeam;
						List<Vector2> list = new List<Vector2>();
						for (int j = 0; j < 6; j++)
						{
							list.Add(PLAYER.currentShip.cosm.randomCrewLocation());
						}
						Vector2 pendingPosition = PLAYER.avatar.position;
						float num2 = 0f;
						foreach (Vector2 vector in list)
						{
							float num3 = Vector2.Distance(vector, PLAYER.avatar.position);
							if (num3 > num2)
							{
								pendingPosition = vector;
								num2 = num3;
							}
						}
						crew.pendingPosition = pendingPosition;
						PLAYER.currentShip.cosm.addCrew(crew);
					}
				}
			};
			dialogue = new DialogueSelectRev2(PLAYER.currentGame.agentTracker.getAgent("One"), dialogueTree);
			SCREEN_MANAGER.dialogue = dialogue;
			tip.tip = "Defend your homestation";
			tip.setDescription("Kill the intruders. Or make them leave by destroying their ships.");
			try
			{
				if (SCREEN_MANAGER.questJournal != null)
				{
					SCREEN_MANAGER.questJournal.LoadQuests();
				}
			}
			catch
			{
			}
		}
		public static void SpawnDialogue() //phase 1 initial dialogue (before lockdown)
		{
			PLAYER.currentSession.pause();
			DialogueTree dialogueTree = new DialogueTree();
			DialogueTree dialogueTree2 = new DialogueTree();
			DialogueTree result = new DialogueTree();
			dialogueTree.text = "Are you throwing a party or something? What are all those ships doing outside?";
			dialogueTree.addOption("Wait, what ships?", dialogueTree2);
			dialogueTree.addOption("You are joking, right?", dialogueTree2);
			dialogueTree2.text = "Go to the bridge and see for yourself.";
			dialogueTree2.addOption("Okay...", result);
			dialogue = new DialogueSelectRev2(PLAYER.currentGame.agentTracker.getAgent("One"), dialogueTree);
			SCREEN_MANAGER.dialogue = dialogue;
			try
			{
				if (SCREEN_MANAGER.questJournal != null)
				{
					SCREEN_MANAGER.questJournal.LoadQuests();
				}
			}
			catch
			{
			}

			//resetting all current worldsave flags to defaults on every spawn of the event
			initialize();
	}

		private static void LockdownDialogue() //phase 2 initial dialogue (lockfown)
		{
			PLAYER.currentSession.pause();
			DialogueTree dialogueTree = new DialogueTree();
			DialogueTree dialogueTree2 = new DialogueTree();
			DialogueTree result = new DialogueTree();
			dialogueTree.text = "I see you died again? I activated the lockdown protocol and blocked the airlocks. The intruders have been busy stealing our ressources while I was rebuilding your body.";
			dialogueTree.addOption("...", dialogueTree2);
			dialogueTree2.text = "They are trying to hack our security system to dock their ships and store our cargo. Get rid of them before they succeed.";
			dialogueTree2.addOption("I will try.", result);
			dialogueTree2.addOption("Are you not going to help me?", result);
			dialogue = new DialogueSelectRev2(PLAYER.currentGame.agentTracker.getAgent("One"), dialogueTree);
			SCREEN_MANAGER.dialogue = dialogue;
			try
			{
				if (SCREEN_MANAGER.questJournal != null)
				{
					SCREEN_MANAGER.questJournal.LoadQuests();
				}
			}
			catch
			{
			}
		}

		private static void LockdownDialogue2() 
		{
			PLAYER.currentSession.pause();
			DialogueTree dialogueTree = new DialogueTree();
			DialogueTree dialogueTree2 = new DialogueTree();
			DialogueTree result = new DialogueTree();
			dialogueTree.text = "It looks like your efforts to manage this situation are lacking any results. Maybe you should focus on damaging their ships so they can't transport our goods away and have to leave.";
			dialogueTree.addOption("...", dialogueTree2);
			dialogueTree2.text = "Have you tried to talking to them? Anyway I managed to lock down the airlocks again. Don't think it will hold them up for long though.";
			dialogueTree2.addOption("...", result);
			dialogue = new DialogueSelectRev2(PLAYER.currentGame.agentTracker.getAgent("One"), dialogueTree);
			SCREEN_MANAGER.dialogue = dialogue;
		}
		private static void ShiphackedDialogue() 
		{
			PLAYER.currentSession.pause();
			DialogueTree dialogueTree = new DialogueTree();
			DialogueTree result = new DialogueTree();
			dialogueTree.text = "We have a little problem. They hacked our airlocks open and your ship. You have to prevent them from entering the ship and fly away with our goods.";
			dialogueTree.addOption("...", result);
			dialogue = new DialogueSelectRev2(PLAYER.currentGame.agentTracker.getAgent("One"), dialogueTree);
			SCREEN_MANAGER.dialogue = dialogue;
			
		}
	}

	public class GenericIntruder : NPCAgent
	{
		public GenericIntruder(string n)
		{
			this.name = n;
			this.portraitSmall = "Gene";
			this.portraitLarge = "GeneL";
		}
		public override void startConvo()
		{
			DialogueTree dialogueTree = new DialogueTree();
			DialogueTree dialogueTree2 = new DialogueTree();
			dialogueTree.text = "How can I help you?";
			DialogueTree result = new DialogueTree();
			dialogueTree.addOption("Join me", dialogueTree2, () => (PLAYER.currentGame.team.openSlots > 0));
			dialogueTree.addOption("Nevermind", result);
			dialogueTree.text = "With pleasure";
			dialogueTree2.addOption("Let's go", result);
			dialogueTree2.action = delegate ()
			{
				var crew = PLAYER.currentShip.cosm.crew.Values.ToList().Find(c => c.name == this.name);
				if(crew != null && crew.name == this.name)
				{
					//add to players team
					crew.team = PLAYER.currentTeam;
					crew.faction = PLAYER.avatar.faction;
					crew.factionless = false;
					PLAYER.currentGame.team.reportStatus(crew);
					PLAYER.currentGame.updateCrew();
				}
			};
			SCREEN_MANAGER.dialogue = new DialogueSelectRev2(this, dialogueTree);
		}
	}

}
