using CoOpSpRpG;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HarshWorld
{
    public static class HWBaseSiegeEvent
	{
		private static Vector2 position;
		private static Point grid;
		private static DialogueSelectRev2 dialogue;
		private static float intruderTimer = 0f;
		private static float lootTimer = 0f;
		private static ModTile targetModule = null;
		private static ModTile targetConsole = null;
		private static ModTile targetCargobay = null;
		private static ModTile cloneVats = null;
		private static ModTile targetAirlock = null;
		private static Vector2 CBPos;
		private static Vector2 CSPos;
		private static Vector2 CVPos;
		private static Vector2 ALPos;
		public static ToolTip tip;
		private static Dictionary<InventoryItemType, int> ressourses = null;

		public class HWBaseSiegeEventQuest : TriggerEvent
		{
			public static string staticName = "homebase_siege";
			public HWBaseSiegeEventQuest()
			{
				this.name = HWBaseSiegeEventQuest.staticName;
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

			position = InterruptionInstance.position;
			grid = InterruptionInstance.grid;
			
			if (tip == null)
			{
				tip = new ToolTip();
				tip.tip = "Defend your homestation";
				tip.setDescription("Find out what the attackers want and get rid of them.");
			}

			if (ressourses == null)
			{
				ressourses = CHARACTER_DATA.getAllResources();
			}

			if (InterruptionInstance.activeShips.Count() == 0 && (InterruptionInstance.wavesQueued == 0 || InterruptionInstance.currentWave >= InterruptionInstance.maxWaves) && InterruptionInstance.initWaveQueued == false)
			{
				Globals.eventflags[GlobalFlag.Sige1EventActive] = false;
			}

			if (Globals.eventflags[GlobalFlag.Sige1EventActive])
			{
				intruderTimer += elapsed;
				if (intruderTimer >= 80f) //spawning intruders
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
							/*
							if (!PLAYER.avatar.currentCosm.crew.Values.ToList().TrueForAll(element => Vector2.DistanceSquared(element.position, PLAYER.avatar.position) > 1f * 1f))
							{
								SCREEN_MANAGER.widgetChat.AddMessage("Enemies near cloning facility detected, activating annihilation protocols.", MessageTarget.Ship);
							}
							*/
							PROCESS_REGISTER.currentCosm.klaxonOverride = false;
							PROCESS_REGISTER.currentCosm.interiorLightType = InteriorLightType.battlestations;
							/*
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
							*/
						}
					}
				}
			}

			if (PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId)
			{
				if (cloneVats == null)
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
				foreach (CoOpSpRpG.Module module in PROCESS_REGISTER.currentCosm.modules)
				{

					if (module.type == ModuleType.cargo_bay)
					{
						targetCargobay = (module as CargoBay).tiles[0];
						break;
					}
				}
				foreach (CoOpSpRpG.Module module in PROCESS_REGISTER.currentCosm.modules)
				{

					if (module.type == ModuleType.Console_Access)
					{
						targetConsole = (module as ConsoleAccess).tiles[0];
						break;
					}
				}
				foreach (CoOpSpRpG.Module module in PROCESS_REGISTER.currentCosm.modules)
				{

					if (module.type == ModuleType.airlock)
					{
						targetAirlock = (module as Airlock).tiles[0];
						break;
					}
				}
				var distanceToCV = Vector2.DistanceSquared(CVPos, PLAYER.avatar.position);

				for (int i = 0; i < PLAYER.avatar.currentCosm.crew.Values.Count; i++) //managing intruders
				{
					var crew = PLAYER.avatar.currentCosm.crew.Values.ToList()[i];
					if (!crew.isPlayer && crew.faction != PLAYER.avatar.faction && crew.state != CrewState.dead)
					{
						if (crew.team.threats.Contains(PLAYER.avatar.faction) && Vector2.DistanceSquared(crew.position, PLAYER.avatar.position) < 100000f * 100000f) //goals for hostile intruders
						{
							if (!Globals.eventflags[GlobalFlag.Sige1EventPlayerDead])
							{
								if(PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.normal) //default setting
								{
									PROCESS_REGISTER.currentCosm.klaxonOverride = true;
									crew.attackTarget(PLAYER.avatar);//set goal to move towards player
								}								
								if (PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.battlestations && distanceToCV > 45000) //player is in lockdown and not in CV room
								{
									PROCESS_REGISTER.currentCosm.klaxonOverride = true;
									crew.attackTarget(PLAYER.avatar);//set goal to move towards player
								}
								if (hasStolenResources(crew) && Vector2.DistanceSquared(crew.position, PLAYER.avatar.position) > 45000 && crew.target != crew.position)
								{
									crew.setGoal(targetAirlock);
								}
							}
							if (Globals.eventflags[GlobalFlag.Sige1EventPlayerDead]) //player presumed dead
							{
								if (PLAYER.animateRespawn || PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.battlestations)
								{
									if (targetModule == null)
									{
										targetModule = targetCargobay;
										crew.setGoal(targetModule); //reset to stealing items every time player is respawning
									}
									else if (crew.state == CrewState.attacking)
									{
										crew.setGoal(targetModule); //reset to stealing items every time player is respawning
									}
								}
								/*
								if (hasStolenResources(crew) && Vector2.DistanceSquared(crew.position, PLAYER.avatar.position) > 45000 && crew.target != crew.position)
								{
									crew.setGoal(targetAirlock);
								}
								*/
								if (crew.state != CrewState.attacking && crew.target == crew.position && targetCargobay != null && targetConsole != null && targetAirlock != null) //check if at target position of his current goal
								{
									CBPos = new Vector2((float)(targetCargobay.X % crew.currentCosm.width * 16), (float)(targetCargobay.X / crew.currentCosm.width * 16));
									CSPos = new Vector2((float)(targetConsole.X % crew.currentCosm.width * 16), (float)(targetConsole.X / crew.currentCosm.width * 16));
									ALPos = new Vector2((float)(targetAirlock.X % crew.currentCosm.width * 16), (float)(targetAirlock.X / crew.currentCosm.width * 16));
									var distanceToCB = Vector2.DistanceSquared(CBPos, crew.position);									
									var distanceToCS = Vector2.DistanceSquared(CSPos, crew.position);
									var distanceToAL = Vector2.DistanceSquared(ALPos, crew.position);
									if (distanceToCB < 4400f) //check if target position is cargobay and try to steal item
										{ 
											lootTimer += elapsed;
											if (lootTimer >= 1f)
											{
												lootTimer = 0f;
												if (!stealRandomResource(crew)) //if some random ressourcers successfuly stolen, go to the airlock to transfer them to the ship.
												{
													crew.setGoal(targetAirlock);
												}
											}
										}
									if (distanceToCS < 27000f) //check if target position is console
									{
										//crew.attackTarget(PLAYER.avatar);//set goal to move towards player
									}
									if (distanceToAL < 27000f) //check if target position is airlock
									{
										if(transRestToShip(crew)) // try transfering stolen ressources back to the ship
										{
											crew._kill();//"leave" the station
										}
									}
								}

							}
						}
					}
				}
			}
			
			if (PROCESS_REGISTER.currentCosm.interiorLightType == InteriorLightType.battlestations && !PLAYER.animateRespawn && PLAYER.avatar.heldItem != null)
			{
				Globals.eventflags[GlobalFlag.Sige1EventPlayerDead] = false;
				//PROCESS_REGISTER.currentCosm.interiorLightType = InteriorLightType.normal;
				/*
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
				*/
				targetModule = null;
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

		private static bool hasStolenResources(Crew crew)
		{
			foreach (var item in ressourses.Keys)
			{ 
				if(crew.containsItemOfType(item))
				{
					return true;
				}
			}
			return false;
        }
		private static bool stealRandomResource(Crew crew)
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

		private static bool transRestToShip (Crew crew)
		{
			return false;
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
			return false;
		}

		public static void SetupEntry(JournalEntry entry)
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
			entry.quest = new HWBaseSiegeEventQuest();//null;
			if (tip != null)
			{
				entry.name = tip.tip;
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
			if(position != new Vector2() && grid != new Point())
			{
				entry.SetTarget(position, grid);
			}
			else
			{
				entry.SetTarget(new Vector2(-4000, 4000), new Point(316, -183));
			}
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

		private static void LockdownDialogue()
		{
			PLAYER.currentSession.pause();
			DialogueTree dialogueTree = new DialogueTree();
			DialogueTree result = new DialogueTree();
			dialogueTree.text = "I see you died again? I activated the lockdown protocol and blocked the door to the cloning room. For now the enemy doesen't know that you are not dead, so don't make any noise and eqip your weapon if you are reday. I will deactivate the lockdown as soon as you do.";
			dialogueTree.addOption("...", result);
			dialogue = new DialogueSelectRev2(PLAYER.currentGame.agentTracker.getAgent("One"), dialogueTree);
			SCREEN_MANAGER.dialogue = dialogue;
		}

	}
}
