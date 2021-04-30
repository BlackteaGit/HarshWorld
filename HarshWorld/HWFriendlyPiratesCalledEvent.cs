using CoOpSpRpG;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarshWorld
{
    public static class HWFriendlyPiratesCalledEvent
    {

		public static void interruptionUpdate (float elapsed, BattleSession session, Interruption InterruptionInstance)
		{
			if (Globals.eventflags[GlobalFlag.PiratesCalledForShip] == false && ((PLAYER.currentShip.docked != null && PLAYER.currentShip.docked.Count > 0) || ShipsOutOfRange(session, InterruptionInstance))) //despawn condition for friendly pirates event using InterruptionType.friendly_pirates_call
			{
				foreach (var tupleship in InterruptionInstance.activeShips)
				{
					tupleship.Item2.Clear();  // try to debug if memory is not cleared from old event before new nterruptionType.friendly_pirates_call event spawns
				}
				HWSPAWNMANAGER.DespawnInterruptionAsync(InterruptionInstance.id).SafeFireAndForget();
			}
			else if (InterruptionInstance.activeShips.TrueForAll(element => (element.Item2 != null && element.Item2.Count == 0))) // try to debug if memory is not cleared from old event before new nterruptionType.friendly_pirates_call event spawns and same UIDs are reused
			{
				foreach (var tupleship in InterruptionInstance.activeShips)
				{
					tupleship.Item2.AddRange(new List<string> { "Hello, friend.", "We received your distress call.", "Waiting for your hail.", "We are here to help you, please respond." });
				}
			}
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

		public static bool canBuildShip()
		{
			var hulls = CHARACTER_DATA.unlockedHulls();
			foreach (string text in hulls)
			{
				var selectedDesigns = CHARACTER_DATA.storedDesigns(text);
				foreach (string design in selectedDesigns)
				{
					var selectedCost = TILEBAG.designCost(CHARACTER_DATA.getBot(text, design));
					if (canAfford(selectedCost))
					{
						return true;
					}
				}
			}
			return false;
		}
		private static bool canAfford(Dictionary<InventoryItemType, int> selectedCost)
		{
			if (PLAYER.debugMode)
			{
				return false;
			}
			foreach (InventoryItemType inventoryItemType in selectedCost.Keys)
			{
				int num = selectedCost[inventoryItemType];
				long resource = CHARACTER_DATA.getResource(inventoryItemType);
				if (resource < (long)num)
				{
					return false;
				}
			}
			return true;
		}

		public static List<InventoryItem> getRandomScrapLoot()
		{
			int quantity = Squirrel3RNG.Next(0, 4);
			//int shipsUnlocked = CHARACTER_DATA.shipsUnlocked();
			InventoryItem Item = null;//new InventoryItem();
			List<InventoryItem> list = new List<InventoryItem>();

			for (int i = 0; i < quantity; i++)
			{
				int selectItem = Squirrel3RNG.Next(1, 17);
				if (selectItem == 1)
				{
					// graphene sheets
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.graphene_sheets);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 2)
				{
					// silicon
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.silicon);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 3)
				{
					// assorted parts
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.assorted_parts);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 4)
				{
					// snacks
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.snacks);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 5)
				{
					//structural components
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.structural_components);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 6)
				{
					// thermal plating
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.thermal_plating);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 7)
				{
					//armor plating
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.armor_plating);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 8)
				{
					//electronic components
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.electronic_components);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 9)
				{
					//blueprints
					int num = Squirrel3RNG.Next(LOOTBAG.researchCategories.Length);
					if (LOOTBAG.researchCategories[num].Count > 0)
					{
						int index = Squirrel3RNG.Next(LOOTBAG.researchCategories[num].Count);
						uint id = LOOTBAG.researchCategories[num][index];
						ResearchLoot inventoryItem = new ResearchLoot(id);
						Item = inventoryItem;
					}
				}
				if (selectItem == 10)
				{
					//field coils
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.field_coils);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 11)
				{
					//carbon nanotubes
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.carbon_nanotubes);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 12)
				{
					//structural components
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.structural_components);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 13)
				{
					//gold wire
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.gold_wire);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 14)
				{
					//titanium alloy
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.titanium_alloy);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 15)
				{
					//graphite
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.graphite);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 16)
				{
					//grey goo
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.grey_goo);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (Item != null)
				{
					list.Add(Item);
				}
			}
			return list;
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
				Globals.eventflags[GlobalFlag.PiratesCalledForShipHostile] = false;
				Globals.eventflags[GlobalFlag.PiratesCalledForShip] = false;
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
				Globals.eventflags[GlobalFlag.PiratesCalledForShipHostile] = false;
				Globals.eventflags[GlobalFlag.PiratesCalledForShip] = false;
				return "...";
			};
			DialogueTextMaker waithostile = delegate ()
			{
				foreach (var interruption in Globals.Interruptionbag)
				{
					if (interruption.Value.templateUsed == InterruptionType.friendly_pirates_call)
					{
						foreach (var tupleship in interruption.Value.activeShips)
						{
							tupleship.Item2.Clear();
							tupleship.Item2.AddRange(INTERRUPTION_BAG.GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)));

							if (PLAYER.currentSession.allShips.TryGetValue(tupleship.Item1, out Ship ship))
							{
								if (ship.cosm?.crew != null)
								{
									foreach (var crew in ship.cosm.crew.Values)
										crew.team.threats.Add(PLAYER.avatar.faction);
								}
							}
						}
					}
				}
				Globals.eventflags[GlobalFlag.PiratesCalledForShipHostile] = true;
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
				Globals.eventflags[GlobalFlag.PiratesCalledForShipHostile] = false;
				return "Allright, I will see if I can get some money to pay you.";
			};
			lobby.addOption("Hey I am glad you are here, I have an emergency.", dialogueTree, () => ___representative.faction == 8UL && Globals.eventflags[GlobalFlag.PiratesCalledForShip] && !Globals.eventflags[GlobalFlag.PiratesCalledForShipHostile] && (___representative.currentCosm.ship.spawnIndex != 33 || ___representative.currentCosm.ship.spawnIndex != 34));
			lobby.addOption("Hey I am glad you are here, I have an emergency.", dialogueTree8, () => ___representative.faction == 8UL && Globals.eventflags[GlobalFlag.PiratesCalledForShip] && Globals.eventflags[GlobalFlag.PiratesCalledForShipHostile] && (___representative.currentCosm.ship.spawnIndex != 33 || ___representative.currentCosm.ship.spawnIndex != 34));
			lobby.addOption("Hey I am glad you are here, I have an emergency.", dialogueTree14, () => ___representative.faction == 8UL && Globals.eventflags[GlobalFlag.PiratesCalledForShip] && (___representative.currentCosm.ship.spawnIndex == 33 || ___representative.currentCosm.ship.spawnIndex == 34));
			dialogueTree.text = "Yes, your master told us that you blew up your ship and now stranded at your station.";
			dialogueTree.addOption("Oh .. Uh .. yes. I mean, NO. He ist NOT my master.", dialogueTree1, () => Globals.Interruptionbag != null && Globals.Interruptionbag.Values.ToList().TrueForAll(element => (element.templateUsed == InterruptionType.friendly_pirates_call && element.activeShips.Count == 3) || (element.templateUsed != InterruptionType.friendly_pirates_call))); // check if the ship is still avaible.
			dialogueTree.addOption("Actually we are more like partners. I am getting things done and he is making bad jokes.", dialogueTree9, () => Globals.Interruptionbag != null && Globals.Interruptionbag.Values.ToList().TrueForAll(element => (element.templateUsed == InterruptionType.friendly_pirates_call && element.activeShips.Count != 3) || (element.templateUsed != InterruptionType.friendly_pirates_call))); // check if the ship is still avaible, if not, then this option without the ship offer

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

			dialogueTree9.text = "What a sad story. Anyway, we brought a ship for you, but some jerk attacked us. Now we can't spare this ship.";
			dialogueTree9.addOption("...", dialogueTree5);

			dialogueTree10.text = "Allright, we are heading back now.";
			dialogueTree10.addOption(payvisit, lobby);

			dialogueTree11.text = "Allright, ship is yours.";
			dialogueTree11.addOption(buyship, lobby);

			dialogueTree12.text = "Enjoy your bullet meal.";
			dialogueTree12.addOption(waithostile, dialogueTree2);

			dialogueTree13.text = "Don't screw with us.";
			dialogueTree13.addOption(waitnonhostile, dialogueTree2);

			dialogueTree14.text = "You are speaking to autopilot AI '" + ___representative.name + "' version 0.9 dev build. I am not designed to provide a full range of service. Please hail one of the escort pilots to handle any emergency situation.";
			dialogueTree14.addOption("Uh..Sure.", dialogueTree2);
		}
	}
}
