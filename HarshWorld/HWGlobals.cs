﻿using CoOpSpRpG;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace HarshWorld
{
    public enum GlobalFlag : uint
	{
		PiratesCalledForShip,
		PiratesCalledForShipHostile,
		Sige1EventActive,
		Sige1EventSpawnDialogueActive,
		Sige1EventPlayerDead,
		Sige1EventLockdown,
		PiratesCalledForDefense
	}

	public enum GlobalInt : uint
	{
		Bounty,
		FriendlyPiratesRep,
		PiratesRep,
		SSCRep,
		FreelancersRep
	}

	public enum GlobalDouble : uint
	{
		ModVersion
	}
	public enum GlobalString: uint
	{
	}
	public static class Globals  //WTFModLoader.WTFModLoader._modManager.Mods.Find(mod => mod.ModMetadata.Name == "HarshWorld.HarshWorld_Patches").ModInstance.GetType().Assembly.GetType("HarshWorld.Globals", throwOnError: true).GetField("").GetValue(null); //to access fields of this class from other harmony patches via reflection
	{
		public static InterruptionBasic[] Interruptions = new InterruptionBasic[Math.Max(0, HWCONFIG.MaxInterruptions)];
		public static Dictionary<string, Interruption> Interruptionbag = new Dictionary<string, Interruption>();
		public static ConcurrentQueue<Tuple<ulong, Point>> GlobalDespawnQueue = new ConcurrentQueue<Tuple<ulong, Point>>();
		public static ConcurrentQueue<Tuple<ulong, Point>> GlobalShipRemoveQueue = new ConcurrentQueue<Tuple<ulong, Point>>();
		public static Dictionary<InventoryItemType, int> offer = new Dictionary<InventoryItemType, int>();
		public static Dictionary<InventoryItemType, int> demand = new Dictionary<InventoryItemType, int>();
		public static ConcurrentDictionary<GlobalFlag, bool> eventflags = new ConcurrentDictionary<GlobalFlag, bool>();
		public static ConcurrentDictionary<GlobalInt, int> globalints = new ConcurrentDictionary<GlobalInt, int>();
		public static ConcurrentDictionary<GlobalDouble, double> globaldoubles = new ConcurrentDictionary<GlobalDouble, double>();
		public static ConcurrentDictionary<GlobalString, string> globalstrings = new ConcurrentDictionary<GlobalString, string>();
		public static Dictionary<ulong, Tuple<string, GlobalInt>> globalfactions = new Dictionary<ulong, Tuple<string, GlobalInt>>();
		public static int mostExpensiveDesigndifficulty = Globals.DifficultyFromCost(HW_CHARACTER_DATA_Extensions.mostExpensiveDesign());
		public static int mostExpensiveBuildableDesigndifficulty = Globals.DifficultyFromCost(Globals.mostExpensiveBuildableDesign());
		public static int currentshipdifficulty = -1;
		public static int shipsUnlocked = (int)Math.Round(MathHelper.Clamp(((float)CHARACTER_DATA.shipsUnlocked() * HWCONFIG.GlobalDifficulty), 1f, 100f));
		public static bool initialized;
		public static void Initialize()
		{
			//difficulty = Globals.DifficultyFromCost(HW_CHARACTER_DATA_Extensions.mostExpensiveDesign());
			mostExpensiveDesigndifficulty = Globals.DifficultyFromCost(HW_CHARACTER_DATA_Extensions.mostExpensiveDesign());
			mostExpensiveBuildableDesigndifficulty = Globals.DifficultyFromCost(Globals.mostExpensiveBuildableDesign());
			if (PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && !PLAYER.currentShip.cosm.isStation)
			{
				currentshipdifficulty = Globals.DifficultyFromCost((int)Hull.getCost(PLAYER.currentShip.botD));
			}
			else
			{
				currentshipdifficulty = -1;
			}
			shipsUnlocked = (int)Math.Round(MathHelper.Clamp(((float)CHARACTER_DATA.shipsUnlocked() * HWCONFIG.GlobalDifficulty), 1f, 100f));
			Interruptions = new InterruptionBasic[Math.Max(0, HWCONFIG.MaxInterruptions)];
			Interruptionbag = new Dictionary<string, Interruption>();
			GlobalDespawnQueue = new ConcurrentQueue<Tuple<ulong, Point>>();
			GlobalShipRemoveQueue = new ConcurrentQueue<Tuple<ulong, Point>>();
			offer = new Dictionary<InventoryItemType, int>();
			demand = new Dictionary<InventoryItemType, int>();

			globalfactions = new Dictionary<ulong, Tuple<string, GlobalInt>>();
			//globalfactions.Add(1UL, "");
			//globalfactions.Add(2UL, new Tuple<string, GlobalInt>("Allies"));
			globalfactions.Add(3UL, new Tuple<string, GlobalInt>("SSC", GlobalInt.SSCRep));
			globalfactions.Add(4UL, new Tuple<string, GlobalInt>("Pirates", GlobalInt.PiratesRep));
			globalfactions.Add(5UL, new Tuple<string, GlobalInt>("Freelancer", GlobalInt.FreelancersRep));
			//globalfactions.Add(6UL, new Tuple<string, GlobalInt>("",));
			//globalfactions.Add(7UL, "Delerict");
			globalfactions.Add(8UL, new Tuple<string, GlobalInt>("Friendly Pirates", GlobalInt.FriendlyPiratesRep));
			//globalfactions.Add(9UL, new Tuple<string, GlobalInt>("Civilian",));
			//globalfactions.Add(ulong.MaxValue, new Tuple<string, GlobalInt>("Abandoned Wreck",));

			eventflags = new ConcurrentDictionary<GlobalFlag, bool>();
			eventflags.TryAdd(GlobalFlag.PiratesCalledForShip, false);
			eventflags.TryAdd(GlobalFlag.PiratesCalledForShipHostile, false);
			eventflags.TryAdd(GlobalFlag.Sige1EventActive, false);
			eventflags.TryAdd(GlobalFlag.Sige1EventSpawnDialogueActive, false);
			eventflags.TryAdd(GlobalFlag.Sige1EventPlayerDead, false);
			eventflags.TryAdd(GlobalFlag.Sige1EventLockdown, false);
			eventflags.TryAdd(GlobalFlag.PiratesCalledForDefense, false);

			globalints = new ConcurrentDictionary<GlobalInt, int>();
			globalints.TryAdd(GlobalInt.Bounty, 0);
			globalints.TryAdd(GlobalInt.FriendlyPiratesRep, 0);
			globalints.TryAdd(GlobalInt.PiratesRep, 0);
			globalints.TryAdd(GlobalInt.SSCRep, 0);
			globalints.TryAdd(GlobalInt.FreelancersRep, 0);

			globaldoubles = new ConcurrentDictionary<GlobalDouble, double>();
			globaldoubles.TryAdd(GlobalDouble.ModVersion, 0.72d);

			globalstrings = new ConcurrentDictionary<GlobalString, string>();

			HWBaseSiegeEvent.initialize();
			SCREEN_MANAGER.widgetChat = new WidgetChat();
			initialized = true;
		}
		public static int DifficultyFromCost(int cost)
		{
			int difficulty = 0;
			if (cost > 7000)
				difficulty = 1;
			if (cost > 10000)
				difficulty = 2;
			if (cost > 15000)
				difficulty = 3;
			if (cost > 20000)
				difficulty = 4;
			if (cost > 35000)
				difficulty = 5;
			if (cost > 50000)
				difficulty = 6;
			if (cost > 130000)
				difficulty = 7;
			if (cost > 150000)
				difficulty = 8;
			if (cost > 220000)
				difficulty = 9;
			if (cost > 350000)
				difficulty = 10;

			return difficulty;
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
					if (canAffordShip(selectedCost))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static int mostExpensiveBuildableDesign()
		{
			var hulls = CHARACTER_DATA.unlockedHulls();
			List<int> affordable = new List<int>();
			foreach (string text in hulls)
			{
				var selectedDesigns = HW_CHARACTER_DATA_Extensions.storedDesignsWithCost(text);
				foreach (var design in selectedDesigns)
				{
					var selectedCost = TILEBAG.designCost(CHARACTER_DATA.getBot(text, design.Item1));
					if (canAffordShip(selectedCost))
					{
						affordable.Add(design.Item2);
					}
				}
			}
			if (affordable.Count > 0)
			{ 
				return affordable.Max();
			}
			else
			{ 
				return 0;
			}
		}

		private static bool canAffordShip(Dictionary<InventoryItemType, int> selectedCost)
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

		public static Dictionary<string, int> getFactionRepDeeds(ulong faction)
		{
			Dictionary<string, int> deeds = new Dictionary<string, int>();

			switch (faction)
			{
				case 3: //SSC
					if (PLAYER.currentGame.completedQuests.Contains("phase_1_end"))
					{
						deeds.Add("raided the shipyard", -200);
					}
					if (PLAYER.currentGame.completedQuests.Contains("kill_budd"))
					{
						deeds.Add("killed the pirate leader", -100);
					}
					if (PLAYER.currentGame.completedQuests.Contains("the_data"))
					{
						deeds.Add("datacenter assault", -200);
					}
					if (PLAYER.currentGame.completedQuests.Contains("mega_fort"))
					{
						deeds.Add("destroyed the SSC gate", -500);
					}
					
					break;
				case 4: //Pirates
					if (PLAYER.currentGame.completedQuests.Contains("bust_pirates"))
					{
						deeds.Add("Budd's lair assault", -100);
					}
					if (PLAYER.currentGame.completedQuests.Contains("bust_pirates_2"))
					{
						deeds.Add("cleared out Budd's lair", -300);
					}
					if (PLAYER.currentGame.completedQuests.Contains("kill_budd"))
					{
						deeds.Add("killed Budd", -500);
					}
					if (PLAYER.currentGame.completedQuests.Contains("gary_v_greg"))
					{
						deeds.Add("killed Greg", -200);
					}
					break;
				case 6: //Freelancer

					break;
				case 8: //Friendly Pirates
					if (PLAYER.currentGame.completedQuests.Contains("phase_1_end"))
					{
						deeds.Add("raided the SSC shipyard", 50);
					}
					if (PLAYER.currentGame.completedQuests.Contains("bust_blockade"))
					{
						deeds.Add("busted the SSC blockade", 100);
					}
					if (PLAYER.currentGame.completedQuests.Contains("feed the rats"))
					{
						deeds.Add("smuggled food over the blockade", 50);
					}
					if (PLAYER.currentGame.completedQuests.Contains("bust_pirates_2"))
					{
						deeds.Add("cleared out Budd's lair", 300);
					}
					if (PLAYER.currentGame.completedQuests.Contains("kill_budd"))
					{
						deeds.Add("killed Budd", 500);
					}
					break;
			}
			deeds.Add("other deeds", Globals.globalints[Globals.globalfactions[faction].Item2]);
			return deeds;
		}

		public static int getAccumulatedReputation(ulong faction)
		{
			int accumulated = 0;
			if (PLAYER.currentGame != null && PLAYER.currentWorld != null && Globals.globalfactions.ContainsKey(faction))
			{
				foreach (var entry in Globals.getFactionRepDeeds(faction).Values)
				{
					accumulated = accumulated + entry;
				}
			}
			return accumulated;
		}

		public static void changeReputation(ulong faction, int modifier, string reason)
		{
			if (Globals.globalfactions.ContainsKey(faction))
			{
				string modification;
				if (modifier < 0)
				{
					modification = "lost";
				}
				else
				{
					modification = "gained";
				}
				Globals.globalints[Globals.globalfactions[faction].Item2] += modifier;
				SCREEN_MANAGER.widgetChat.AddMessage("You " + modification + " " + Math.Abs(modifier).ToString() + " reputation with the " + Globals.globalfactions[faction].Item1 + " faction, " + reason, MessageTarget.Ship);
				if (modifier < 0)
					assignBounty(faction);
			}
		}
		public static void changeReputation(ulong faction, int modifier)
		{
			if (Globals.globalfactions.ContainsKey(faction))
			{
				string modification;
				if (modifier < 0)
				{
					modification = "lost";
				}
				else
				{
					modification = "gained";
				}
				Globals.globalints[Globals.globalfactions[faction].Item2] += modifier;
				SCREEN_MANAGER.widgetChat.AddMessage("You " + modification + " " + Math.Abs(modifier).ToString() + " reputation with the " + Globals.globalfactions[faction].Item1 + " faction.", MessageTarget.Ship);
				if (modifier < 0)
					assignBounty(faction);
			}
		}

		/*
		 * Example of accessing changeReputation(ulong faction, int modifier) from another mod via reflection
		 * 
		 *try
            {
               var modentry = WTFModLoader.WTFModLoader._modManager.ActiveMods.Find(mod => mod.ModMetadata.Name == "HarshWorld.HarshWorld_Patches");
               if(modentry?.ModInstance != null)
               {
                   BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
                   var args = new object[] { 3UL, -5, "" };
                   var assembly = modentry.ModInstance.GetType().Assembly;
                   var type = assembly.GetType("HarshWorld.Globals", throwOnError: true);
                   var method = type.GetMethod("changeReputation", flags, null, new Type[] { typeof(ulong), typeof(int), typeof(string) }, null);
                   method.Invoke(null, args);
                }
            }
            catch(Exception e)
            {
            }
		 */

		public static void assignBounty(ulong faction)
		{
			var rep = Globals.globalints[Globals.globalfactions[faction].Item2];
			if (rep < 0)
			{
				rep = Math.Abs(rep);
				rep = Squirrel3RNG.Next(rep, rep * 2 + 1);
			}
			else
			{
				rep = 0;
			}
			if (rep >= 100 && Squirrel3RNG.Next(10) == 1)
			{
				var modifier = Squirrel3RNG.Next(rep, 2000);
				Globals.globalints[GlobalInt.Bounty] += modifier;
				SCREEN_MANAGER.widgetChat.AddMessage("The " + Globals.globalfactions[faction].Item1 + " faction placed a " + Globals.globalints[GlobalInt.Bounty].ToString() + " credits bounty on your head.", MessageTarget.Ship);
			}
		}
		public static void upscaleMonster(Monster m)
		{
			int difficulty;
			if (PLAYER.currentGame != null && PLAYER.currentWorld != null && PLAYER.currentSession != null && PLAYER.avatar != null && PLAYER.currentShip != null)
			{
				difficulty = (int)Math.Round(MathHelper.Clamp(((Math.Max(CHARACTER_DATA.shipsUnlocked(), Globals.mostExpensiveDesigndifficulty * 3) + Globals.mostExpensiveBuildableDesigndifficulty * 3) / 2 * HWCONFIG.GlobalDifficulty), 1f, 100f));
				float distance;
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


				float multiplier = Math.Max(6155714f / Math.Max(distance * (1 + ((Math.Max(1, playerweaponquality) + Math.Max(1, playerarmorquality)) / 2 / 50)), 1), 1);
				difficulty = Math.Min(difficulty, difficulty / (int)Math.Max((multiplier / (Math.Max(multiplier, difficulty) / difficulty)), 1));
				difficulty = Math.Min(HWCONFIG.MaxMonsterLevel, difficulty);
				//shipsUnlocked = 30; //for debugging

				//int monsterHealthScaled = 100;
				//float monsterSpeedScaled = 100f;
				//int monsterDamageScaled = 1;
				//int monsterThicknessScaled = 1;

				int monsterHealthScaled = m.health;
				float monsterSpeedScaled = m.speed;
				int monsterDamageScaled = m.attackDamage;
				int monsterThicknessScaled = m.thickness;
				//float monsterDetectRangeScaled = 400f;
				//float monsterInterestRangeScaled = 1000f;
				//float monsterAttackRangeScaled = 10f;

				if(PLAYER.avatar.currentCosm?.monsters != null && PLAYER.avatar.currentCosm.monsters.Contains(m) && Squirrel3RNG.Next(PLAYER.avatar.currentCosm.monsters.Count) <= 10)
				{ 
					int points = difficulty * 4;
					int healthpoints = Squirrel3RNG.Next(difficulty / 4, difficulty);
					monsterHealthScaled += Squirrel3RNG.Next(Math.Max(0, (healthpoints - 4) * 20 * 2), Math.Max(10, Convert.ToInt32(((float)healthpoints - 4) * 22 * 3)));
					difficulty = (points - healthpoints) / 4;
					points = difficulty * 3;
					int speedpoints = Squirrel3RNG.Next(difficulty / 3, difficulty);
					if(Squirrel3RNG.Next(10) == 1)
						monsterSpeedScaled *= 1f + (0.04f * Squirrel3RNG.Next(Math.Max(1, (speedpoints - 5)), Math.Max(2, Convert.ToInt32(((float)speedpoints - 5) * 3))));
					difficulty = (points - speedpoints) / 4;
					points = difficulty * 2;
					int damagepoints = Squirrel3RNG.Next(difficulty / 2, difficulty / 2 * 3);
					if (Squirrel3RNG.Next(5) == 1)
						monsterDamageScaled += Squirrel3RNG.Next(Math.Max(0, (damagepoints - 3) * 3), Math.Max(1, Convert.ToInt32(((float)damagepoints - 3) * 4)));
					points -= damagepoints;
					if (Squirrel3RNG.Next(5) == 1)
						monsterThicknessScaled += Squirrel3RNG.Next(Math.Max(0, (points - 3) * 2), Math.Max(1, Convert.ToInt32(((float)points - 3) * 5)));
				}

				m.health = Math.Max(m.health, monsterHealthScaled);
				m.healthMax = m.health;
				m.speed = Math.Max(m.speed, monsterSpeedScaled);
				//m.detectRange = Math.Max(m.detectRange, monsterDetectRangeScaled);
				//m.loseInterestRange = Math.Max(m.loseInterestRange, monsterInterestRangeScaled);
				//m.attackRange = Math.Max(m.attackRange, monsterAttackRangeScaled);	
				//m.fightRange = m.attackRange - 6f;
				m.attackDamage = Math.Max(m.attackDamage, monsterDamageScaled);
				m.thickness = Math.Max(m.thickness, monsterThicknessScaled);
				m.Setscaled(true);
			}
		}
	}	
}
