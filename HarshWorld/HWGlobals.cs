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
	}

	public enum GlobalDouble : uint
	{
		ModVersion
	}
	public enum GlobalString: uint
	{
	}
	class Globals
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
		public static void Initialize()
		{
			Interruptions = new InterruptionBasic[Math.Max(0, HWCONFIG.MaxInterruptions)];
			Interruptionbag = new Dictionary<string, Interruption>();
			GlobalDespawnQueue = new ConcurrentQueue<Tuple<ulong, Point>>();
			GlobalShipRemoveQueue = new ConcurrentQueue<Tuple<ulong, Point>>();
			offer = new Dictionary<InventoryItemType, int>();
			demand = new Dictionary<InventoryItemType, int>();
			eventflags = new Dictionary<GlobalFlag, bool>();
			eventflags.Add(GlobalFlag.PiratesCalledForShip, false);
			eventflags.Add(GlobalFlag.PiratesCalledForShipHostile, false);
			eventflags.Add(GlobalFlag.Sige1EventActive, false);
			eventflags.Add(GlobalFlag.Sige1EventSpawnDialogueActive, false);
			eventflags.Add(GlobalFlag.Sige1EventPlayerDead, false);
			eventflags.Add(GlobalFlag.Sige1EventLockdown, false);
			eventflags.Add(GlobalFlag.PiratesCalledForDefense, false);
			globalints = new Dictionary<GlobalInt, int>();
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

	}
}
