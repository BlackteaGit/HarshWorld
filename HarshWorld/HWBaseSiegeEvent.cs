using CoOpSpRpG;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarshWorld
{
    public static class HWBaseSiegeEvent
    {
		private static DialogueSelectRev2 dialogue;
		private static float intruderTimer = 0f;
		private static float lootTimer = 0f;
		private static ModTile targetModule = null;
		private static ModTile targetConsole = null;
		private static ModTile targetCargobay = null;

		public static void interruptionUpdate(float elapsed, BattleSession session, Interruption InterruptionInstance)
		{
			if (InterruptionInstance.activeShips.Count() == 0 && (InterruptionInstance.wavesQueued == 0 || InterruptionInstance.currentWave >= InterruptionInstance.maxWaves) && InterruptionInstance.initWaveQueued == false)
			{
				Globals.eventflags[GlobalFlag.Sige1EventActive] = false;
			}

			if (Globals.eventflags[GlobalFlag.Sige1EventActive])
			{
				intruderTimer += elapsed;
				if (intruderTimer >= 150f) //spawning intruders
				{
					intruderTimer = 0f;
					if (PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId)
					{
						if (HWCONFIG.GlobalDifficulty > 0 && !Globals.eventflags[GlobalFlag.Sige1EventPlayerDead])
						{
							if (SCREEN_MANAGER.dialogue == null && !PROCESS_REGISTER.currentCosm.klaxonOverride)
							{
								intrudersDialogue();  //Spawning intruders with dialogue
								SCREEN_MANAGER.widgetChat.AddMessage("Intruders detected.", MessageTarget.Ship);
								PROCESS_REGISTER.currentCosm.klaxonOverride = true;
							}
							else if(PROCESS_REGISTER.currentCosm.klaxonOverride) //Spawning intruders
							{
								int num = RANDOM.Next(3) + 1;
								CrewTeam crewTeam = new CrewTeam();
								crewTeam.threats.Add(PLAYER.avatar.faction);
								for (int i = 0; i < num; i++)
								{
									Crew crew = new Crew();
									crew.outfit(0f);
									crew.faction = 3UL;
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
				}

				

				if(PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId && Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] && PLAYER.avatar.state != CrewState.dead)
				{
					if (PLAYER.animateRespawn)
					{
						if (PROCESS_REGISTER.currentCosm.interiorLightType != InteriorLightType.battlestations)
						{
							SCREEN_MANAGER.widgetChat.AddMessage("Station operator missing. Initiating emergency lockdown.", MessageTarget.Ship);
							if (!PLAYER.avatar.currentCosm.crew.Values.ToList().TrueForAll(element => Vector2.DistanceSquared(element.position, PLAYER.avatar.position) > 1f * 1f))
							{
								SCREEN_MANAGER.widgetChat.AddMessage("Enemies near cloning facility detected, activating annihilation protocols.", MessageTarget.Ship);
							}
							PROCESS_REGISTER.currentCosm.interiorLightType = InteriorLightType.battlestations;
							foreach (Module module in PROCESS_REGISTER.currentCosm.modules)
							{
								if (module.type == ModuleType.door)
								{
									if (!(module as Door).locked)
									{
										(module as Door).open = false;
										(module as Door).locked = true;
									}
								}
							}
						}
					}
				}
			}

			if (PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.battlestations && !PLAYER.animateRespawn)
			{
				//Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] = false;
				PROCESS_REGISTER.currentCosm.interiorLightType = InteriorLightType.normal;
				foreach (Module module in PROCESS_REGISTER.currentCosm.modules)
				{
					if (module.type == ModuleType.door)
					{
						if ((module as Door).locked)
						{
							(module as Door).open = true;
							(module as Door).locked = false;
						}
					}
				}
				targetModule = null;
			}

			if (PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId)
			{
				for (int i = 0; i < PLAYER.avatar.currentCosm.crew.Values.Count; i++) //managing intruders
				{
					var crew = PLAYER.avatar.currentCosm.crew.Values.ToList()[i];
					if (!crew.isPlayer && crew.faction != PLAYER.avatar.faction && crew.state != CrewState.dead)
					{
						if (crew.team.threats.Contains(PLAYER.avatar.faction) && Vector2.DistanceSquared(crew.position, PLAYER.avatar.position) < 100000f * 100000f) //goals for hostile intruders
						{
							if (!Globals.eventflags[GlobalFlag.Sige1EventPlayerDead])
							{
								crew.attackTarget(PLAYER.avatar);//set goal to move towards player
							}
							if (Globals.eventflags[GlobalFlag.Sige1EventPlayerDead])
							{
								//crew.state = CrewState.idle;
								//if (PLAYER.animateRespawn)
								//{	
									if(targetModule == null)
									{ 
										foreach (Module module in PROCESS_REGISTER.currentCosm.modules)
										{

											if (module.type == ModuleType.cargo_bay)
											{
												targetModule = targetCargobay = (module as CargoBay).tiles[0];
												goto CargoBayFound;
											}

										}

										CargoBayFound:
										crew.setGoal(targetModule); //reset to stealing items every time lockdown initiated

										foreach (Module module in PROCESS_REGISTER.currentCosm.modules)
										{

											if (module.type == ModuleType.Console_Access)
											{
												targetConsole = (module as ConsoleAccess).tiles[0];
												goto ConsoleFound;
											}
										}

										ConsoleFound:;
									}
									else if(crew.state == CrewState.attacking)
									{
										crew.setGoal(targetModule); //reset to stealing items every time lockdown initiated
									}
									Vector2 CBPos = new Vector2((float)(targetCargobay.X % crew.currentCosm.width * 16), (float)(targetCargobay.X / crew.currentCosm.width * 16));
									var distance = Vector2.DistanceSquared(CBPos, crew.position);
									if (crew.state != CrewState.attacking && crew.target == crew.position && distance < 66f * 66f ) //check if at cargo bay and try to steal item
									{
										lootTimer += elapsed;
										if (lootTimer >= 1f)
										{
											lootTimer = 0f;
											if (!StealRandomResource(crew)) //if some random ressourcers stolen, go to the console to transfer them to the ship.
											{
												crew.setGoal(targetConsole);
											}
										}
									}
								//}	
							}
						}
					}
				}
			}

			if (Globals.eventflags[GlobalFlag.Sige1EventActive] == false )// || ShipsOutOfRange(session, InterruptionInstance)) //despawn conditions
			{
				foreach (var tupleship in InterruptionInstance.activeShips)
				{
					tupleship.Item2.Clear();  // try to debug if memory is not cleared from old event before new event spawns
				}

				if (PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId) //disabling station alarm if encounter is about to despawn
				{
					PROCESS_REGISTER.currentCosm.klaxonOverride = false;
				}
				else
				{
					if (session != null)
					{
						foreach (Station station in session.stations)
						{
							if (station.id == PLAYER.currentGame.homeBaseId && station.cosm != null && station.cosm.alive)
							{
								station.cosm.klaxonOverride = false;
							}
						}
					}
				}
				//Globals.flags[GlobalFlag.Sige1EventActive] = false;
				HWSPAWNMANAGER.DespawnInterruptionAsync(InterruptionInstance.id).SafeFireAndForget();
			}

		}

		private static bool StealRandomResource(Crew crew)
		{
			var ressourses = CHARACTER_DATA.getAllResources();
			var toSteal = ressourses.Keys.ToList()[Squirrel3RNG.Next(ressourses.Keys.ToList().Count)];
			if(ressourses[toSteal] >= 1)
			{ 
				if(crew.containsItemOfType(toSteal))
				{
					return false;
                }
				CHARACTER_DATA.setResource(toSteal, ressourses[toSteal] - 1);
				InventoryItem inventoryItem = new InventoryItem(toSteal);
				inventoryItem.stackSize = 1;
				crew.placeInFirstSlot(inventoryItem);
				crew.GetfloatyText().Enqueue("+" + inventoryItem.stackSize.ToString() + " " + inventoryItem.toolTip.tip);
				/*
				String Tooltip;
				if (ITEMBAG.defaultTip.ContainsKey(toSteal))
				{
					Tooltip = ITEMBAG.defaultTip[toSteal].tip;
				}
				else
				{
					Tooltip = "cargo";
				}
				crew.GetfloatyText().Enqueue("+ 1 " + Tooltip);
				*/
			}
			return true;
		}

		private static bool ShipsOutOfRange(BattleSession session, Interruption InterruptionInstance)
		{
			for (int i = 0; i < InterruptionInstance.activeShips.Count<Tuple<ulong, List<String>>>(); i++)
			{
				if (session.allShips.TryGetValue(InterruptionInstance.activeShips[i].Item1, out Ship ship))
				{
					if (Vector2.DistanceSquared(PLAYER.currentShip.position, ship.position) >= 1800f * 1800f && !ship.velocity.Equals(new Vector2(0, 0)))
					{
						return true;
					}
				}
			}
			return false;
		}

		private static int CheckHostileIntrudersCount(BattleSession HomeBaseSession)
		{
			int intruders = 0;
			if(PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId)
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
						if (station.id == PLAYER.currentGame.homeBaseId && station.cosm != null && station.cosm.alive)
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
		public static bool test(float elapsed)
		{

			if (Globals.eventflags[GlobalFlag.Sige1EventActive] && Globals.eventflags[GlobalFlag.Sige1EventSpawnDialogueActive] && SCREEN_MANAGER.dialogue == null && PLAYER.currentSession != null && !PLAYER.currentSession.paused)
			{
				SpawnDialogue();
				Globals.eventflags[GlobalFlag.Sige1EventSpawnDialogueActive] = false;
				return true;
			}
			if (dialogue != null && dialogue.removeMe)
			{
				dialogue = null;
				PLAYER.currentSession.unpause();
			}
			return false;
		}

		private static void intrudersDialogue()
		{
			DialogueTree dialogueTree = new DialogueTree();
			DialogueTree dialogueTree2 = new DialogueTree();
			dialogueTree.text = "Intruder alert. They've breached our hull!";
			switch (RANDOM.Next(3))
			{
				case 0:
					dialogueTree.addOption("That's bad!", dialogueTree2);
					break;
				case 1:
					dialogueTree.addOption("Oh how nice I wonder what they want.", dialogueTree2);
					break;
				case 2:
					dialogueTree.addOption("Let's kill them all!", dialogueTree2);
					break;
			}
			dialogueTree2.action = delegate ()
			{
				if (PLAYER.currentShip != null && PLAYER.currentShip.cosm != null)
				{
					int num = RANDOM.Next(3) + 1;
					CrewTeam crewTeam = new CrewTeam();
					crewTeam.threats.Add(PLAYER.avatar.faction);
					for (int i = 0; i < num; i++)
					{
						Crew crew = new Crew();
						crew.outfit(0f);
						crew.faction = 3UL;
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
		}
		public static void SpawnDialogue()
		{
			PLAYER.currentSession.pause();
			DialogueTree dialogueTree = new DialogueTree();
			DialogueTree dialogueTree2 = new DialogueTree();
			DialogueTree result = new DialogueTree();
			dialogueTree.text = "Are you throwing a party or something? What are all those ships doing outside?";
			dialogueTree.addOption("Wait, what ships?", dialogueTree2);
			dialogueTree.addOption("You are joking, right?", dialogueTree2);
			dialogueTree2.text = "Go ahead and see for yourself.";
			dialogueTree2.addOption("Okay...", result);
			dialogue = new DialogueSelectRev2(PLAYER.currentGame.agentTracker.getAgent("One"), dialogueTree);
			SCREEN_MANAGER.dialogue = dialogue;
		}

	}
}
