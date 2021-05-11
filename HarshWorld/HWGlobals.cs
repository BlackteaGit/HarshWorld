using CoOpSpRpG;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
		public static Dictionary<GlobalFlag, bool> eventflags = new Dictionary<GlobalFlag, bool>();
		public static Dictionary<GlobalInt, int> globalints = new Dictionary<GlobalInt, int>();
		public static Dictionary<GlobalDouble, double> globaldoubles = new Dictionary<GlobalDouble, double>();
		public static Dictionary<GlobalString, string> globalstrings = new Dictionary<GlobalString, string>();
		public static Dictionary<ulong, Tuple<string, GlobalInt>> globalfactions = new Dictionary<ulong, Tuple<string, GlobalInt>>();
		public static void Initialize()
		{
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

			eventflags = new Dictionary<GlobalFlag, bool>();
			eventflags.Add(GlobalFlag.PiratesCalledForShip, false);
			eventflags.Add(GlobalFlag.PiratesCalledForShipHostile, false);
			eventflags.Add(GlobalFlag.Sige1EventActive, false);
			eventflags.Add(GlobalFlag.Sige1EventSpawnDialogueActive, false);
			eventflags.Add(GlobalFlag.Sige1EventPlayerDead, false);
			eventflags.Add(GlobalFlag.Sige1EventLockdown, false);
			eventflags.Add(GlobalFlag.PiratesCalledForDefense, false);

			globalints = new Dictionary<GlobalInt, int>();
			globalints.Add(GlobalInt.Bounty, 0);
			globalints.Add(GlobalInt.FriendlyPiratesRep, 0);
			globalints.Add(GlobalInt.PiratesRep, 0);
			globalints.Add(GlobalInt.SSCRep, 0);
			globalints.Add(GlobalInt.FreelancersRep, 0);

			globaldoubles = new Dictionary<GlobalDouble, double>();
			globaldoubles.Add(GlobalDouble.ModVersion, 0.9d);

			globalstrings = new Dictionary<GlobalString, string>();

			HWBaseSiegeEvent.initialize();
			SCREEN_MANAGER.widgetChat = new WidgetChat();
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

		public static Dictionary<string, int> getFactionRepDeeds(ulong faction)
		{
			Dictionary<string, int> deeds = new Dictionary<string, int>();

			switch (faction)
			{
				case 3: //SSC
					if (PLAYER.currentGame.completedQuests.Contains("phase_1_end"))
					{
						deeds.Add("raided the SSC shipyard", -300);
					}
					if (PLAYER.currentGame.completedQuests.Contains("kill_budd"))
					{
						deeds.Add("killed a pirate leader", 100);
					}
					break;
				case 4: //Pirates
					if (PLAYER.currentGame.completedQuests.Contains("phase_1_end"))
					{
						deeds.Add("raided the SSC shipyard", 100);
					}
					if (PLAYER.currentGame.completedQuests.Contains("bust_pirates"))
					{
						deeds.Add("pirate's cove assault", -100);
					}
					if (PLAYER.currentGame.completedQuests.Contains("bust_pirates_2"))
					{
						deeds.Add("cleared out pirate's cove", -300);
					}
					if (PLAYER.currentGame.completedQuests.Contains("kill_budd"))
					{
						deeds.Add("killed Budd", -500);
					}
					break;
				case 6: //Freelancer

					break;
				case 8: //Friendly Pirates
					if (PLAYER.currentGame.completedQuests.Contains("bust_blockade"))
					{
						deeds.Add("busted the SSC blockade", 100);
					}
					if (PLAYER.currentGame.completedQuests.Contains("bust_pirates_2"))
					{
						deeds.Add("cleared out pirate's cove", 300);
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

		public static int getAccFactionReputation(ulong faction)
		{
			int accumulated = 0;
			foreach (var entry in Globals.getFactionRepDeeds(faction).Values)
			{
				accumulated = accumulated + entry;
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
				SCREEN_MANAGER.widgetChat.AddMessage("You " + modification + " " + Math.Abs(modifier).ToString() + " reputation with the " + Globals.globalfactions[faction].Item1 + " faction " + reason, MessageTarget.Ship);
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
			}
		}
	}
}
