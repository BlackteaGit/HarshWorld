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
			if (Globals.eventflags[GlobalFlag.PiratesCalled] == false && ((PLAYER.currentShip.docked != null && PLAYER.currentShip.docked.Count > 0) || ShipsOutOfRange(session, InterruptionInstance))) //despawn condition for friendly pirates event using InterruptionType.friendly_pirates_call
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

	}
}
