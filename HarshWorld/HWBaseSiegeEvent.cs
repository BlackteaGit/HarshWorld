using CoOpSpRpG;
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
		public static Interruption interruption = null;
		private static Vector2 eventposition;
		private static Point grid;
		public static uint eventfaction = 3U;
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
		public static int seed = 0;
		public static bool seconddeath = false;
		private static bool thirddeath = false;
		public static bool unlockAirlocks = false;
		private static bool buildShip = false;
		private static bool stealShip = false;
		private static bool leaving = false;

		public static void initialize ()
		{
			interruption = null;
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
			seed = 0;
		}

		public static int getSeed(string source)
		{
			int result = 0;
			foreach (char c in source)
			{
				result += Convert.ToInt32(c);
			}
			return result;
		}
		public class HWBaseSiege1EventQuest : TriggerEvent //placeholder event class for quest description in questjournal
		{
			public static string staticName = "homebase_siege";
			public HWBaseSiege1EventQuest()
			{
				this.name = HWBaseSiege1EventQuest.staticName;
				this.tip = new ToolTip();
				this.tip.tip = "Defend your homestation";
				tip.setDescription("Find out what the intruders want and get rid of them.");
			}

			public override bool test(float elapsed)
			{
				if (this.tip == null)
				{
					this.tip = new ToolTip();
					this.tip.tip = "Defend your homestation";
					tip.setDescription("Find out what the intruders want and get rid of them.");
					return true;
				}
				return false;
			}
		}

		public static void interruptionUpdate(float elapsed, BattleSession session, Interruption InterruptionInstance)
		{
			bool leave = false;
			if (interruption == null)
			{
				interruption = InterruptionInstance;
			}
            if (seed == 0)
            {
                seed = getSeed(InterruptionInstance.id);
            }
            eventposition = InterruptionInstance.position;
			grid = InterruptionInstance.grid;
			eventfaction = InterruptionInstance.interruptersFaction;

            bool intrudersonboard;
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
				buildShip = true; //No active besieger ships left, some crew is still on the homebase, try to build a ship from players resources.
			}

			if (Globals.eventflags[GlobalFlag.Sige1EventActive] && Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] && !intrudersonboard) //all intruders are killed or left the station after phase2 and some have stolen goods in inventory
			{
				for(int i = 0; i < InterruptionInstance.activeShips.Count; i++)
				{
					if (session.allShips.TryGetValue(InterruptionInstance.activeShips[i].Item1, out Ship ship))
					{

						if (PLAYER.currentShip != null && ship.id == PLAYER.currentShip.id)
						{

						}
						else
						{
							if (ship.cosm?.crew != null)
							{
								for (int c = 0; c < ship.cosm.crew.Values.ToList().Count; c++)
								{
									if (hasStolenResources(ship.cosm.crew.Values.ToList()[c]) || stealShip)
									{
										//win condition for besiegers, they consider looting player's homebase a success and no longer profitable and fly away
										leave = true;
										goto finished;
									}
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
				tip.setDescription("Find out what the intruders want and get rid of them.");
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
			if (InterruptionInstance.activeShips.Count > 0 && intruderTimer >= Math.Min(InterruptionInstance.activeShips.Count * 20f, 160f)) //spawning intruders
			{
				
				if (HWCONFIG.GlobalDifficulty > 0 && PROCESS_REGISTER.currentCosm.interiorLightType != InteriorLightType.battlestations
				&& Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] == false && seconddeath == false && PLAYER.currentShip.cosm.crew.Count() < Squirrel3RNG.Next(12,17)) //spawning only in 1st phase
				{
					if (SCREEN_MANAGER.dialogue == null && !PROCESS_REGISTER.currentCosm.klaxonOverride)
					{
						intrudersDialogue(InterruptionInstance.interruptersFaction);  //Spawning intruders with dialogue (if no intruders are present on station)
						SCREEN_MANAGER.widgetChat.AddMessage("Intruders detected.", MessageTarget.Ship);
						intruderTimer = 0f;
					}
					else if (PROCESS_REGISTER.currentCosm.klaxonOverride && !PLAYER.currentShip.cosm.crew.Values.ToList().TrueForAll( c => (c.team.threats.Contains(PLAYER.avatar.faction) && Vector2.DistanceSquared(c.position, PLAYER.avatar.position) > 2000f * 2000f) || !c.team.threats.Contains(PLAYER.avatar.faction)))//Spawning intruders without dialogue (if some intruders are already present on station)
					{
						Task.Run(async () =>
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
							}
						});
						SCREEN_MANAGER.widgetChat.AddMessage("Intruders detected.", MessageTarget.Ship);
						intruderTimer = 0f;
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
				tip.setDescription("Kill the intruders before they can steal your resources and hack the airlocks open. Or make them leave by destroying their ships.");
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

			if (Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] && seconddeath && PROCESS_REGISTER.currentCosm.klaxonOverride == true && tip.description != "Do not let the intruders escape with your resources.") //restoring state flags after game load if saved in phase 3 (after lockdown)
			{
				tip.tip = "Defend your homestation";
				tip.setDescription("Do not let the intruders escape with your resources.");
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
						SCREEN_MANAGER.widgetChat.AddMessage("Initiating emergency lockdown.", MessageTarget.Ship);
						SCREEN_MANAGER.widgetChat.AddMessage("Airlocks latched.", MessageTarget.Ship);
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
						tip.setDescription("Kill the intruders before they can steal your resources and hack the airlocks open. Or make them leave by destroying their ships.");
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
											if (CHARACTER_DATA.getAllResources().Values.ToList().TrueForAll(amount => amount <= 0)) // if the reason of unsuccessful stealing is insufficent player resources
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
													if (CHARACTER_DATA.credits < (Item.refineValue * Item.stackSize)) // If player has no resources and no money
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
												if (session.allShips.TryGetValue(InterruptionInstance.activeShips[i].Item1, out var ship))
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
													if (dockSpot != null && ship.docking != null && ship.docking.Count != 0 && ship.cosm?.crew?.Values?.FirstOrDefault()?.team != null && ship.cosm.alive)
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
//TODO>>build ship from players resources>>>
												if (crew.team.threats.Contains(PLAYER.avatar.faction)) //if building a ship is impossible intruders will give up and become neutral
													crew.team.threats.Remove(PLAYER.avatar.faction);
												//SCREEN_MANAGER.widgetChat.AddMessage("Intruders can't find a ship to escape and surrender.", MessageTarget.Ship);
												intrudersSurrenderDialogue(crew);
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
													if (session.allShips.TryGetValue(InterruptionInstance.activeShips[i].Item1, out var ship))
													{
														if (POIdockSpot != null && ship.docking != null && ship.docking.Count != 0 && ship.cosm?.crew?.Values?.FirstOrDefault()?.team != null && ship.cosm.alive)
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
//TODO>>build ship from players resources>>>
												if (crew.team.threats.Contains(PLAYER.avatar.faction)) //if building a ship is impossible intruders will give up and become neutral
													crew.team.threats.Remove(PLAYER.avatar.faction);
												//SCREEN_MANAGER.widgetChat.AddMessage("Intruders can't find a ship to escape and surrender.", MessageTarget.Ship);
												intrudersSurrenderDialogue(crew);

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
											//SCREEN_MANAGER.widgetChat.AddMessage("Phase1 reset 1. ( kill player without lockdown)", MessageTarget.Ship);
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
											else if (seconddeath)
											{
												if(!thirddeath)
												{ 
													Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] = false; //if no escape ship in phase 3 reset to phase 1, but skip the phase 2 after player is killed (seconddeath flag still true)												
													//SCREEN_MANAGER.widgetChat.AddMessage("Phase1 reset 2. (kill player with lockdown)", MessageTarget.Ship);
													seconddeath = true;
													thirddeath = true;
													targetModule = POIConsole;
													crew.setGoal(targetModule);
												}
												else
												{
													if (thirddeath)
													{
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
												if (thirddeath)
												{
													crew.setGoal(POIConsole);
												}
												else
												{
													crew.setGoal(POIAirlock);
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
                                            else if (seconddeath)
                                            {
												if (!thirddeath)
												{
													Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] = false; //if no escape ship in phase 3 reset to phase 1, but skip the phase 2 after player is killed (seconddeath flag still true)												
													//SCREEN_MANAGER.widgetChat.AddMessage("Phase1 reset 2. (kill player with lockdown)", MessageTarget.Ship);
													seconddeath = true;
													thirddeath = true;
													targetModule = POIConsole;
													crew.setGoal(targetModule);
												}
												else
												{
													if (thirddeath)
													{
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
												if(thirddeath)
												{ 
													crew.setGoal(POIConsole);
												}
												else
												{
													crew.setGoal(POIAirlock);
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
					crew.GetfloatyText().Enqueue("+" + Item.stackSize.ToString() + " " + Item.toolTip.tip + " stolen");
					return false;  // stealing successful (do not attempt furter stealing)
				}
				CHARACTER_DATA.setResource(toSteal, ressourses[toSteal] - 1);
				InventoryItem inventoryItem = new InventoryItem(toSteal);
				inventoryItem.stackSize = 1;
				crew.placeInFirstSlot(inventoryItem);
				crew.GetfloatyText().Enqueue("+" + inventoryItem.stackSize.ToString() + " " + inventoryItem.toolTip.tip + " stolen");
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
					crew.GetfloatyText().Enqueue("+" + SCREEN_MANAGER.formatCreditString((ulong)(Item.refineValue * Item.stackSize)) + " credits" + " stolen");
					CHARACTER_DATA.credits -= (ulong)(Item.refineValue * Item.stackSize);
					return false;  // stealing successful (do not attempt furter stealing)
				}				
				crew.placeInFirstSlot(Item);
				crew.GetfloatyText().Enqueue("+" + SCREEN_MANAGER.formatCreditString((ulong)(Item.refineValue * Item.stackSize)) + " credits" + " stolen");
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
					var posToHomebase = Vector2.DistanceSquared(eventposition, ship.position);
					if(PLAYER.currentShip != null && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId)
					{
						if(ship.cosm?.crew?.FirstOrDefault().Value?.team != null && ship.cosm.crew.First().Value.team.goalType != ConsoleGoalType.warp_jump)
						{
							foreach (var crew in ship.cosm.crew.Values)
							{
								crew.team.goalType = ConsoleGoalType.warp_jump;
							}
						}
                    }
					else
					{
						if (ship.cosm?.crew?.FirstOrDefault().Value?.team != null && ship.cosm.crew.First().Value.team.goalType != ConsoleGoalType.kill_enemies)
						{
							foreach (var crew in ship.cosm.crew.Values)
							{
								crew.team.goalType = ConsoleGoalType.kill_enemies;
							}
						}
					}
					if (posToHomebase >= 15000 * 15000)
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
								ship.rotationAngle = SCREEN_MANAGER.VectorToAngle(eventposition - ship.position);
								crew.team.destination = eventposition;
								crew.team.goalType = ConsoleGoalType.warp_jump;
								InterruptionInstance.activeEffects.Add(new ActiveEffect("WarpIn", ship.position, 3f, 0f));
								ship.velocity *= -1;
							}
							//DEBUG message>>>>>>>>>>	
							//SCREEN_MANAGER.widgetChat.AddMessage("Ship signature detected entering sensor range.", MessageTarget.Ship);
						}
					}
					else if (ship.cosm?.crew?.FirstOrDefault().Value?.team != null && ship.cosm.crew.First().Value.team.goalType == ConsoleGoalType.reach_destination && posToHomebase <= 500 * 500)
					{
						foreach (var crew in ship.cosm.crew.Values)
						{
							ship.rotationAngle = SCREEN_MANAGER.VectorToAngle(eventposition - ship.position);
							crew.team.destination = eventposition;
							crew.team.goalType = ConsoleGoalType.warp_jump;
							ship.velocity *= -1;
						}
					}
					else if (ship.engineEnergy <= 0 && leaving && InterruptionInstance.interdictTimer >= 2f)//ship.velocity.Equals(new Vector2(0, 0)))
					{
						remove = InterruptionInstance.activeShips[i];
					}
					else if (leaving && ship.cosm?.crew != null && ship.cosm?.crew.Count > 0 && !ship.cosm.crew.Values.ToList().TrueForAll(crew => crew.team.threats.Count <= 0))
					{
						if (ship.dockedAt != null)
						{
							SCREEN_MANAGER.widgetChat.AddMessage("Undocking ship at airlock 1.", MessageTarget.Ship);
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
				//SCREEN_MANAGER.widgetChat.AddMessage("Ship abandoned.", MessageTarget.Ship);
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
					if (crew.state != CrewState.dead && !crew.isPlayer && crew.faction != PLAYER.avatar.faction && crew.team?.threats != null && !crew.team.threats.Contains(PLAYER.avatar.faction))
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

		private static ulong getPayOffCost()
		{
			int activeships = 1;
			if(interruption != null)
			{
				activeships = interruption.activeShips.Count();
			}
			return (ulong)Math.Max(1200,(CHARACTER_DATA.credits * 0.1 * activeships * HWCONFIG.GlobalDifficulty));
		}
		public static bool test(float elapsed)
		{

			if (Globals.eventflags[GlobalFlag.Sige1EventActive] && Globals.eventflags[GlobalFlag.Sige1EventSpawnDialogueActive] && SCREEN_MANAGER.dialogue == null && !PLAYER.currentSession.paused && PLAYER.currentSession != null && PLAYER.currentShip != null && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId)
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
			if (Globals.eventflags[GlobalFlag.Sige1EventActive] && PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId &&
			(Globals.Interruptionbag == null || (Globals.Interruptionbag != null && 
			Globals.Interruptionbag.Values.ToList().TrueForAll(element => (element.templateUsed != InterruptionType.home_siege_pirate_t1 && element.templateUsed != InterruptionType.home_siege_pirate_t2 && element.templateUsed != InterruptionType.home_siege_pirate_t25)))))
			{
				Globals.eventflags[GlobalFlag.Sige1EventActive] = false;
				Globals.eventflags[GlobalFlag.Sige1EventSpawnDialogueActive] = false;
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
				entry.SetTarget(PLAYER.currentGame.position, CONFIG.spHomeGrid);
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
			dialogueTree.action = delegate ()
			{
				Task.Run(async () => {
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
				});
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
			dialogueTree.addOption("You are joking, right?", dialogueTree2);
			dialogueTree.addOption("They just came out of nowhere, it has nothing to do with me.", dialogueTree2);
			dialogueTree2.text = "This station has some old mining lasers installed. I activated them just in case. Be cautious while using them though, they might just explode in your face.";
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

		private static void LockdownDialogue() //phase 2 initial dialogue (lockdown)
		{
			PLAYER.currentSession.pause();
			DialogueTree dialogueTree = new DialogueTree();
			DialogueTree dialogueTree2 = new DialogueTree();
			DialogueTree result = new DialogueTree();
			dialogueTree.text = "I see you died again? I activated the lockdown protocol and blocked the airlocks. The intruders have been busy stealing our resources while I was rebuilding your body.";
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
			DialogueTree dialogueTree3 = new DialogueTree();
			DialogueTree result = new DialogueTree();
			dialogueTree.text = "It looks like your efforts to manage this situation are not very effective. Maybe you should focus on damaging their ships so they have no choice but to leave.";
			dialogueTree.addOption("...", dialogueTree2);
			dialogueTree2.text = "Have you even tried hailing them yet? Anyway I managed to lock down the airlocks again. It won't hold them for long.";
			dialogueTree2.addOption("Sure.", result);
			dialogueTree2.addOption("But I already destroyed all of their ships, what now?", dialogueTree3, () => interruption.activeShips.Count() <= 0 );
			dialogueTree3.text = "Are you sure? Well, maybe they didn't realize this yet. I guess you could just let them hack our bridge again to see what's going on.";
			dialogueTree3.addOption("I hope you are right.", result);
			dialogue = new DialogueSelectRev2(PLAYER.currentGame.agentTracker.getAgent("One"), dialogueTree);
			SCREEN_MANAGER.dialogue = dialogue;
		}
		private static void ShiphackedDialogue() 
		{
			PLAYER.currentSession.pause();
			DialogueTree dialogueTree = new DialogueTree();
			DialogueTree result = new DialogueTree();
			dialogueTree.text = "We have a little problem. They hacked our airlocks and your ship. You have to prevent them from flying away with our stuff.";
			dialogueTree.addOption("...", result);
			dialogue = new DialogueSelectRev2(PLAYER.currentGame.agentTracker.getAgent("One"), dialogueTree);
			SCREEN_MANAGER.dialogue = dialogue;
		}

		private static void intrudersSurrenderDialogue(Crew crew)
		{
			//TODO surrender dialogue >>>>>>
			NPCAgent agent = new GenericIntruder(crew.name);
			PLAYER.currentSession.pause();
			DialogueTree dialogueTree = new DialogueTree();
			DialogueTree result = new DialogueTree();
			dialogueTree.text = "So you destroyed our ships. We are surrendering now, please don't kill us.";
			dialogueTree.addOption("...", result);
			dialogue = new DialogueSelectRev2(agent, dialogueTree);
			SCREEN_MANAGER.dialogue = dialogue;
			for (int i = 0; i < crew.currentCosm.crew.Values.Count; i++) //managing intruders
			{
				var crew2 = crew.currentCosm.crew.Values.ToList()[i];
				if (!crew2.isPlayer && crew2.faction != 2U && crew2.state != CrewState.dead)
				{
					if (crew2.team.threats.Contains(2U))
					{
						crew2.team.threats.Remove(2U);
					}
				}
			}
		}

		public static void addHailDialogue(ref DialogueTree lobby, Crew ___representative, List<ResponseImmediateAction> ___results)
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

			lobby.addOption("Who are you and what are you doing here?", dialogueTree, () => ___representative.faction == eventfaction && Globals.eventflags[GlobalFlag.Sige1EventActive]);
			dialogueTree.text = "We are here to take your stuff.";
			if (eventfaction == 4U)
			{ 
				switch (new Random(seed).Next(3))
				{
					case 0:
						dialogueTree.text = "We are protectors of this sector, and you haven't been paying your part of the protection fee. We are here to collect.";
						dialogueTree.addOption("What fee? I didn't ask for your protection.", dialogueTree3);
						dialogueTree.addOption("Sure, I will pay you. What do you want?", dialogueTree4);
						dialogueTree.addOption("You are messing with a wrong person here.", dialogueTree2);
						break;
					case 1:
						dialogueTree.text = "We're the free spacefarers, and we are seizing your property for our needs.";
						dialogueTree.addOption("On whose authority? You can't just take what you want.", dialogueTree3);
						dialogueTree.addOption("Wait, let's not rush into a conflict here. We can surely work out a better solution.", dialogueTree4);
						dialogueTree.addOption("You can start by seizing my bullets assholes.", dialogueTree2);
						break;
					case 2:
						dialogueTree.text = "We arr, here to ravish yer booty! Open up yer bay, will ye";
						dialogueTree.addOption("Are you serious? I'm sensing some bad vibes here.", dialogueTree3);
						dialogueTree.addOption("No, please! Take my credits instead.", dialogueTree4);
						dialogueTree.addOption("So you have chosen... death.", dialogueTree2);
						break;
				}
			}
			if (eventfaction == 3U)
			{
				switch (new Random(seed).Next(3))
				{
					case 0:
						dialogueTree.text = "We are the Seven Stars Coalition, and we are here to terminate the illegal activities comming off this station.";
						dialogueTree.addOption("...", dialogueTree3);
						break;
					case 1:
						dialogueTree.text = "We are here to seize your property for the Seven Stars Coalition.";
						dialogueTree.addOption("...", dialogueTree3);
						break;
					case 2:
						dialogueTree.text = "The Seven Stars Coalition sent us to collect your tax fee.";
						dialogueTree.addOption("...", dialogueTree3);
						break;
				}
			}
			dialogueTree1.text = "We're taking yer station apart and see what we can find.";
			dialogueTree1.addOption("We'll see about that.", dialogueTree2);
			dialogueTree1.addOption("No, wait. I'll pay you.", dialogueTree5, () => (CHARACTER_DATA.credits >= getPayOffCost()));

			dialogueTree3.text = "We've been watchin ye gathering stolen goods on this station, it's a crime.";
			dialogueTree3.addOption("What do you want then?", dialogueTree4);
			dialogueTree3.addOption("I'm just wasting my time by talking to you.", dialogueTree2);

			dialogueTree4.text = "Pay us " + getPayOffCost().ToString() + " credits, and we're gone.";
			dialogueTree4.addOption("Allright, no need for hostility, here you go.", dialogueTree5, () => (CHARACTER_DATA.credits >= getPayOffCost())); //check if can be payed
			dialogueTree4.addOption("I don't have the money.", dialogueTree1);
			dialogueTree4.addOption("How about me puttin' " + getPayOffCost().ToString() + " bullets in your ass instead?", dialogueTree2);

			dialogueTree5.text = "Good decision. We'll be watching you.";
			dialogueTree5.addOption("Just leave.", dialogueTree2);
			dialogueTree5.action = delegate ()
			{
				InventoryItem Item = new InventoryItem(InventoryItemType.exotic_matter);
				InventoryItem Item2 = new InventoryItem(InventoryItemType.dense_exotic_matter);
				Item.stackSize = 0;
				Item2.stackSize = 0;
				ulong topay = getPayOffCost();
				while(topay >= Item.refineValue)
				{
					if(topay >= Item2.refineValue)
					{
						Item2.stackSize ++;
						topay -= Item2.refineValue;
					}
					if (topay >= Item.refineValue && topay < Item2.refineValue)
					{
						Item.stackSize++;
						topay -= Item.refineValue;
					}
				}
				if(Item.stackSize > 0)
				{ 
					___representative.placeInFirstSlot(Item);
				}
				if (Item2.stackSize > 0)
				{
					___representative.placeInFirstSlot(Item2);
				}
				CHARACTER_DATA.credits -= getPayOffCost();
				Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] = true;
				interruption.maxWaves = interruption.currentWave;
				if (PLAYER.currentShip != null && PLAYER.currentShip.cosm?.crew != null && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId)
				{
					for (int i = 0; i < PLAYER.currentShip.cosm.crew.Values.Count; i++)
					{
						var crew = PLAYER.currentShip.cosm.crew.Values.ToList()[i];
						if (!crew.isPlayer && crew.faction != PLAYER.avatar.faction && crew.state != CrewState.dead)
						{
							if (crew.team.threats.Contains(PLAYER.avatar.faction))
							{
								crew._kill();
							}
						}
					}
				}
				else
				{
					foreach (Station station in PLAYER.currentSession.stations)
					{
						if (station.id == PLAYER.currentGame.homeBaseId && station.cosm?.crew != null)
						{
							for (int i = 0; i < station.cosm.crew.Values.Count; i++)
							{
								var crew = station.cosm.crew.Values.ToList()[i];
								if (!crew.isPlayer && crew.faction != PLAYER.avatar.faction && crew.state != CrewState.dead)
								{
									if (crew.team.threats.Contains(PLAYER.avatar.faction))
									{
										crew._kill();
									}
								}
							}
						}
					}
				}
			};
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
			dialogueTree2.text = "With pleasure";
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
