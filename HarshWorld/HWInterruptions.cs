using CoOpSpRpG;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HarshWorld
{
    public enum InterruptionType : uint
	{
		// 0
		ambush_starter,
		// 1
		ambush_small_pirate_t1,
		// 2
		ambush_small_ssc_t1,
		// 3
		gold_medium_pirate_t1,
		// 4
		iron_large_ssc_t1,
		// 5
		ithacit_small_ssc_t1,
		// 6
		mo_iron_free_small_fre_t1,
		// 7
		mo_gold_free_small_fre_t1,
		// 8
		mo_ithacit_pirates_small_t1,
		// 9
		mo_iron_pirates_small_t1,
		// 10
		mo_gold_pirates_small_t1,
		// 11
		titanium_small_ssc_t2,
		// 12
		rhodium_small_ssc_t2,
		// 13
		titanium_medium_ssc_t2,
		// 14
		rhodium_medium_ssc_t2,

		blood_pirates_t2,

		blood_pirates_2_t2,

		blood_pirates_3_t2,

		blood_pirates_4_t2,

		mo_iron_goliath_medium_ssc_t2,

		titanium_large_ssc_t25,

		rhodium_large_ssc_t25,

		mo_titanium_goliath_medium_ssc_t25,

		mo_titanium_goliath_large_ssc_t25,

		mo_rhodium_goliath_medium_ssc_t25,

		mo_rhodium_goliath_large_ssc_t25,

		mo_mitraxit_goliath_medium_ssc_t25,

		mo_gold_goliath_medium_ssc_t25,

		friendly_pirates_call,

		home_siege_pirate_t1,

		home_siege_pirate_t2,

		home_siege_pirate_t25,

		home_siege_help_t1,

		home_siege_help_t2,

		home_siege_help_t25,

		home_siege_ssc_t1,

		home_siege_ssc_t2,

		home_siege_ssc_t25

	}
	public enum ElementSpawnerType : uint
	{
		// Token: 0x04000FC8 RID: 4040
		starter_cluster,
		// Token: 0x04000FC9 RID: 4041
		small_cluster,
		// Token: 0x04000FCA RID: 4042
		small_crescent,
		// Token: 0x04000FCB RID: 4043
		small_line,
		// Token: 0x04000FCC RID: 4044
		medium_cluster,
		// Token: 0x04000FCD RID: 4045
		medium_line,
		// Token: 0x04000FCE RID: 4046
		medium_circle,
		// Token: 0x04000FCF RID: 4047
		large_cluster,
		// Token: 0x04000FD0 RID: 4048
		large_crescent
	}
	public class InterruptionTemplate
	{
		// Token: 0x06001231 RID: 4657 RVA: 0x0016EEE9 File Offset: 0x0016D0E9
		public InterruptionTemplate()
		{
			this.shipWaves = new Dictionary<Wave, List<String>>();
			this.configurations = new Dictionary<ElementSpawnerType, float>();
		}

		public void AddInitialWave(List<int> ships, ConsoleGoalType goal, float waveWeight, List<Tuple<InventoryItemType, int>> loot)
		{
			this.initialWave = new Tuple<Wave, List<String>>(new Wave(), new List<String>());
			this.initialWave.Item1.shipIds = ships;
			this.initialWave.Item1.goal = goal;
			this.initialWave.Item1.waveWeight = waveWeight;
			this.initialWave.Item1.loot = loot;
		}
		public void AddInitialWave(List<int> ships, ConsoleGoalType goal, float waveWeight, List<Tuple<InventoryItemType, int>> loot, List<String> conversations)
		{
			this.initialWave = new Tuple<Wave, List<String>>(new Wave(), new List<String>());
			this.initialWave.Item1.shipIds = ships;
			this.initialWave.Item1.goal = goal;
			this.initialWave.Item1.waveWeight = waveWeight;
			this.initialWave.Item1.loot = loot;
			this.initialWave.Item2.AddRange(conversations);
		}

		// Token: 0x06001233 RID: 4659 RVA: 0x0016EF5C File Offset: 0x0016D15C
		public void AddWave(List<int> ships, ConsoleGoalType goal, float waveWeight, List<Tuple<InventoryItemType, int>> loot, List<String> conversations)
		{

			Wave wave = new Wave();
			this.shipWaves.Add(wave, conversations);
			wave.shipIds = ships;
			wave.goal = goal;
			wave.waveWeight = waveWeight;
			wave.loot = loot;
		}

		// Token: 0x06001234 RID: 4660 RVA: 0x0016EFA4 File Offset: 0x0016D1A4
		public void AddWave(List<int> ships, float waveWeight)
		{
			Wave wave = new Wave();
			this.shipWaves.Add(wave, new List<String>());
			wave.shipIds = ships;
			wave.goal = ConsoleGoalType.warp_jump;
			wave.waveWeight = waveWeight;
		}

		public void AddWave(List<int> ships, float waveWeight, List<String> conversations)
		{
			Wave wave = new Wave();
			this.shipWaves.Add(wave, conversations);
			wave.shipIds = ships;
			wave.goal = ConsoleGoalType.warp_jump;
			wave.waveWeight = waveWeight;
		}


		// Token: 0x04002057 RID: 8279
		public Dictionary<Wave, List<String>> shipWaves;

		// Token: 0x04002058 RID: 8280
		public Tuple<Wave, List<String>> initialWave;

		// Token: 0x04002059 RID: 8281
		//public Dictionary<AssetsNodeType, float> assetsNodes;

		// Token: 0x0400205A RID: 8282
		public uint interruptersFaction;

		// Token: 0x0400205B RID: 8283
		public float averageWaveInterval;

		// Token: 0x0400205C RID: 8284
		public Dictionary<ElementSpawnerType, float> configurations;

		// Token: 0x0400205D RID: 8285
		public InterruptionType type;

		// Token: 0x0400205E RID: 8286
		public int maxWaves = 3;
	}
	public static class INTERRUPTION_BAG
	{
		// Token: 0x06001244 RID: 4676 RVA: 0x00170354 File Offset: 0x0016E554
		public static void Init()
		{

			INTERRUPTION_BAG.templates = new Dictionary<InterruptionType, InterruptionTemplate>();

			INTERRUPTION_BAG.AddTemplate(InterruptionType.ambush_starter, new InterruptionTemplate //no waves
			{
				interruptersFaction = 3U,
				averageWaveInterval = 30f,
				configurations =
				{
					{
						ElementSpawnerType.small_cluster,
						0.3f
					},
					{
						ElementSpawnerType.small_line,
						0.3f
					}
				}
			});

			InterruptionTemplate interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.AddWave(new List<int>
			{
				35, //tiny pirate // SSC Runner
				35 //tiny pirate // SSC Runner
			}, ConsoleGoalType.warp_jump, 0.65f, null, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));

			List<Tuple<InventoryItemType, int>> loot = new List<Tuple<InventoryItemType, int>>
			{
				Tuple.Create<InventoryItemType, int>(InventoryItemType.iron_ore, 500)
			};
			interruptionTemplate.AddWave(new List<int>
			{
				36 //small pirate // SSC Messager
			}, ConsoleGoalType.warp_jump, 0.35f, null, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				36,//small pirate // SSC Messager
				35 //tiny pirate // SSC Runner
			}, ConsoleGoalType.warp_jump, 0.15f, null, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.interruptersFaction = 4U;
			interruptionTemplate.averageWaveInterval = 1f;//240f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			interruptionTemplate.maxWaves = 5;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.ambush_small_pirate_t1, interruptionTemplate); // 3 waves 5 ships Runner, Messager

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.AddWave(new List<int>
			{
				68 //SSC broadsword
			}, 0.65f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				70,//SSC Dagger rev2-B
				70 //SSC Dagger rev2-B
			}, 0.65f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				71,//SSC Glaive rev2
				70,//SSC Dagger rev2-B
				70//SSC Dagger rev2-B
			}, 0.65f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.interruptersFaction = 3U;
			interruptionTemplate.averageWaveInterval = 1f;//300f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.large_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.large_crescent, 0.3f);
			INTERRUPTION_BAG.AddTemplate(InterruptionType.iron_large_ssc_t1, interruptionTemplate); //3 waves 6 ships broadsword, Dagger rev2 - B, Glaive rev2

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.AddWave(new List<int>
			{
				35,//tiny pirate // SSC Runner
				35,//tiny pirate // SSC Runner
				35//tiny pirate // SSC Runner
			}, 0.65f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				36//small pirate // SSC Messager
			}, 0.35f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				36,//small pirate // SSC Messager
				35,//tiny pirate // SSC Runner
				35//tiny pirate // SSC Runner
			}, 0.15f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.interruptersFaction = 3U;
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			interruptionTemplate.maxWaves = 5;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.ambush_small_ssc_t1, interruptionTemplate); // 3 waves 7 ships Runner, Messager

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.interruptersFaction = 4U;
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.AddWave(new List<int>
			{
				35, //tiny pirate // SSC Runner
				35, //tiny pirate // SSC Runner
				66 // Boulder
			}, 0.65f, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				36, //small pirate // SSC Messager
				63, //low pirate A // Pirate Vulture
				63 //low pirate A // Pirate Vulture
			}, 0.15f, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.configurations.Add(ElementSpawnerType.medium_circle, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.medium_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.medium_line, 0.3f);
			interruptionTemplate.maxWaves = 5;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.gold_medium_pirate_t1, interruptionTemplate); // 2 waves 6 ships Runner, Messager, Boulder, Vulture

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.interruptersFaction = 3U;
			interruptionTemplate.averageWaveInterval = 1f;// 240f;
			interruptionTemplate.AddWave(new List<int>
			{
				36,//small pirate // SSC Messager
				36//small pirate // SSC Messager
			}, 0.35f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				36,//small pirate // SSC Messager
				35,//tiny pirate // SSC Runner
				35//tiny pirate // SSC Runner
			}, 0.65f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			interruptionTemplate.maxWaves = 5;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.ithacit_small_ssc_t1, interruptionTemplate); // 2 waves 5 ships Messager, Runner

			interruptionTemplate = new InterruptionTemplate();
			List<int> list = new List<int>();
			list.Add(25);//SSC Hauler SSC_D
			list.Add(35);//tiny pirate // SSC Runner
			list.Add(35);//tiny pirate // SSC Runner
			list.Add(35);//tiny pirate // SSC Runner
			loot = new List<Tuple<InventoryItemType, int>>
			{
				Tuple.Create<InventoryItemType, int>(InventoryItemType.iron_ore, 3000)
			};
			interruptionTemplate.AddInitialWave(list, ConsoleGoalType.kill_enemies, 1f, loot, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.interruptersFaction = 4U;
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			interruptionTemplate.maxWaves = 5;
//needs testing
			INTERRUPTION_BAG.AddTemplate(InterruptionType.mo_iron_pirates_small_t1, interruptionTemplate); // initial wave 4 ships Hauler, Runner

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.interruptersFaction = 5U;
			list = new List<int>();
			list.Add(72);//Hammer Low_G T1 ship with 4 turrets and separate front and back sections
			list.Add(72);//Hammer Low_G T1 ship with 4 turrets and separate front and back sections
			list.Add(72);//Hammer Low_G T1 ship with 4 turrets and separate front and back sections
			loot = new List<Tuple<InventoryItemType, int>>
			{
				Tuple.Create<InventoryItemType, int>(InventoryItemType.iron_ore, 2500)
			};
			interruptionTemplate.AddInitialWave(list, ConsoleGoalType.kill_enemies, 1f, loot, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.AddWave(new List<int>
			{
				66// Boulder
			}, 1f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			interruptionTemplate.maxWaves = 1;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.mo_iron_free_small_fre_t1, interruptionTemplate);

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.interruptersFaction = 5U;
			list = new List<int>();
			list.Add(72);//Hammer Low_G T1 ship with 4 turrets and separate front and back sections
			list.Add(72);//Hammer Low_G T1 ship with 4 turrets and separate front and back sections
			list.Add(72);//Hammer Low_G T1 ship with 4 turrets and separate front and back sections
			loot = new List<Tuple<InventoryItemType, int>>
			{
				Tuple.Create<InventoryItemType, int>(InventoryItemType.gold_ore, 1000)
			};
			interruptionTemplate.AddInitialWave(list, ConsoleGoalType.kill_enemies, 1f, loot, ShufflePhrases(new List<List<string>> { GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.AddWave(new List<int>
			{
				66// Boulder
			}, 0.5f, ShufflePhrases(new List<List<string>> { GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				73//Low_H advanced start ship with rep drone
			}, 0.5f, ShufflePhrases(new List<List<string>> { GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			interruptionTemplate.maxWaves = 1;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.mo_gold_free_small_fre_t1, interruptionTemplate);

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.interruptersFaction = 4U;
			list = new List<int>();
			list.Add(60);//low pirate C
			list.Add(61);//low pirate D
			list.Add(61);//low pirate D
			list.Add(61);//low pirate D
			loot = new List<Tuple<InventoryItemType, int>>
			{
				Tuple.Create<InventoryItemType, int>(InventoryItemType.gold_ore, 1000)
			};
			interruptionTemplate.AddInitialWave(list, ConsoleGoalType.kill_enemies, 1f, loot, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.AddWave(new List<int>
			{
				53//Pirate cutthroat // SSC Hauler
			}, 0.5f, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				53,//Pirate cutthroat // SSC Hauler
				61//low pirate D
			}, 0.5f, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			interruptionTemplate.maxWaves = 5;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.mo_gold_pirates_small_t1, interruptionTemplate);

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.interruptersFaction = 4U;
			list = new List<int>();
			list.Add(61);//low pirate D
			list.Add(61);//low pirate D
			list.Add(61);//low pirate D
			loot = new List<Tuple<InventoryItemType, int>>
			{
				Tuple.Create<InventoryItemType, int>(InventoryItemType.ithacit_ore, 600)
			};
			interruptionTemplate.AddInitialWave(list, ConsoleGoalType.kill_enemies, 1f, loot, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.AddWave(new List<int>
			{
				53,//Pirate cutthroat // SSC Hauler
				53//Pirate cutthroat // SSC Hauler
			}, 0.5f, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				53,//Pirate cutthroat // SSC Hauler
				61,//low pirate D
				61//low pirate D
			}, 0.5f, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 1f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 1f);
			interruptionTemplate.maxWaves = 5;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.mo_ithacit_pirates_small_t1, interruptionTemplate);


			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.AddWave(new List<int>
			{
				90// Blood_eagle
			}, 0.35f, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				89,// Blood_harpy
				89 // Blood_harpy
			}, 0.35f, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				89 // Blood_harpy
			}, 0.15f, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.interruptersFaction = 4U;
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			interruptionTemplate.maxWaves = 5;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.blood_pirates_t2, interruptionTemplate);  //

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.AddWave(new List<int>
			{
				89,// Blood_harpy
				89,// Blood_harpy
				89 // Blood_harpy
			}, 0.35f, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				89,// Blood_harpy
				89 // Blood_harpy
			}, 0.35f, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				89 // Blood_harpy
			}, 0.15f, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.interruptersFaction = 4U;
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			interruptionTemplate.maxWaves = 5;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.blood_pirates_2_t2, interruptionTemplate);  //

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.AddWave(new List<int>
			{
				87,//pirate missile interdiction frigate // SSC Runner
				87,//pirate missile interdiction frigate // SSC Runner
				87,//pirate missile interdiction frigate // SSC Runner
				87 //pirate missile interdiction frigate // SSC Runner
			}, 0.35f, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				87,//pirate missile interdiction frigate // SSC Runner
				89,// Blood_harpy
				89 // Blood_harpy
			}, 0.35f, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				90// Blood_eagle
			}, 0.15f, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.interruptersFaction = 4U;
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			interruptionTemplate.maxWaves = 5;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.blood_pirates_3_t2, interruptionTemplate);  //

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.AddWave(new List<int>
			{
				90,// Blood_eagle
				87 //pirate missile interdiction frigate // SSC Runner
			}, 0.35f, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				87,//pirate missile interdiction frigate // SSC Runner
				89,// Blood_harpy
				90// Blood_eagle
			}, 0.35f, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				90// Blood_eagle
			}, 0.15f, ShufflePhrases(new List<List<string>> { GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.interruptersFaction = 4U;
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			interruptionTemplate.maxWaves = 5;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.blood_pirates_4_t2, interruptionTemplate);  //


			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.AddWave(new List<int>
			{
				81//SSC Glaive revMissile
			}, 0.35f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				38//SSC Adamantine Dagger Rev2-A
			}, 0.35f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				39//SSC Dagger rev1
			}, 0.15f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.interruptersFaction = 3U;
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			INTERRUPTION_BAG.AddTemplate(InterruptionType.titanium_small_ssc_t2, interruptionTemplate);  //3 waves 3 ships Glaive revMissile, Adamantine Dagger Rev2-A, Dagger rev1

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.interruptersFaction = 3U;
			interruptionTemplate.AddWave(new List<int>
			{
				38,//SSC Adamantine Dagger Rev2-A
				38//SSC Adamantine Dagger Rev2-A
			}, 0.35f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				39,//SSC Dagger rev1
				39//SSC Dagger rev1
			}, 0.15f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				44//able civilian military cruiser
			}, 0.15f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.medium_circle, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.medium_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.medium_line, 0.3f);
			INTERRUPTION_BAG.AddTemplate(InterruptionType.titanium_medium_ssc_t2, interruptionTemplate); //3 waves 5 ships Adamantine Dagger Rev2-A, Dagger rev1, able civilian military cruiser

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.AddWave(new List<int>
			{
				109// SSC_Spear
			}, 0.1f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.interruptersFaction = 3U;
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			INTERRUPTION_BAG.AddTemplate(InterruptionType.rhodium_small_ssc_t2, interruptionTemplate); //1 wave 1 ship Spear

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.interruptersFaction = 3U;
			interruptionTemplate.AddWave(new List<int>
			{
				108,// SSC_Pike
				109// SSC_Spear
			}, 0.1f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.medium_circle, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.medium_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.medium_line, 0.3f);
			INTERRUPTION_BAG.AddTemplate(InterruptionType.rhodium_medium_ssc_t2, interruptionTemplate); //1 wave 2 ships Pike, Spear

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.interruptersFaction = 3U;
			list = new List<int>();
			list.Add(115);// Goliath_Pickaxe
			list.Add(115);// Goliath_Pickaxe
			list.Add(115);// Goliath_Pickaxe
			list.Add(115);// Goliath_Pickaxe
			loot = new List<Tuple<InventoryItemType, int>>
			{
				Tuple.Create<InventoryItemType, int>(InventoryItemType.iron_ore, 8000)
			};
			interruptionTemplate.AddInitialWave(list, ConsoleGoalType.kill_enemies, 1f, loot, ShufflePhrases(new List<List<string>> { GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.AddWave(new List<int>
			{
				88,// SSC_Rapier
				88// SSC_Rapier
			}, 1f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			interruptionTemplate.maxWaves = 1;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.mo_iron_goliath_medium_ssc_t2, interruptionTemplate);


			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.AddWave(new List<int>
			{
				27,//SSC broadsword
				27//SSC broadsword
			}, 0.5f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				119,// SSC Elite halberd
				117,// SSC Elite spear
				117// SSC Elite spear
			}, 0.1f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				64//SSC claymore battleship
			}, 0.5f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.interruptersFaction = 3U;
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.large_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.large_crescent, 0.3f);
			INTERRUPTION_BAG.AddTemplate(InterruptionType.titanium_large_ssc_t25, interruptionTemplate);// 3 waves 6 ships broadsword, Elite halberd, Elite spear, claymore battleship

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.interruptersFaction = 3U;
			interruptionTemplate.AddWave(new List<int>
			{
				119,// SSC Elite halberd
				117,// SSC Elite spear
				117// SSC Elite spear
			}, 0.1f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				118,// SSC Elite pike
				118// SSC Elite pike
			}, 0.1f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.large_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.large_crescent, 0.3f);
			INTERRUPTION_BAG.AddTemplate(InterruptionType.rhodium_large_ssc_t25, interruptionTemplate); //2 waves 5 ships Elite halberd, Elite spear, Elite pike


			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.interruptersFaction = 3U;
			list = new List<int>();
			list.Add(116);// Goliath_Drillbore
			list.Add(115);// Goliath_Pickaxe
			list.Add(115);// Goliath_Pickaxe
			loot = new List<Tuple<InventoryItemType, int>>
			{
				Tuple.Create<InventoryItemType, int>(InventoryItemType.titanium_ore, 5400)
			};
			interruptionTemplate.AddInitialWave(list, ConsoleGoalType.kill_enemies, 1f, loot, ShufflePhrases(new List<List<string>> { GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.AddWave(new List<int>
			{
				111// SSC_Halberd (beam boss)
			}, 0.45f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				121// Goliath Berserker
			}, 0.45f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				119// SSC Elite halberd
			}, 0.05f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 8)) }));
			interruptionTemplate.configurations.Add(ElementSpawnerType.medium_circle, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.medium_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.medium_line, 0.3f);
			interruptionTemplate.maxWaves = 1;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.mo_titanium_goliath_medium_ssc_t25, interruptionTemplate);// initial wave 3 waves 6 ships  Goliath_Drillbore, Goliath_Pickaxe; SSC_Halberd, Goliath Berserker, SSC Elite halberd

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.interruptersFaction = 3U;
			list = new List<int>();
			list.Add(120);// Goliath Excavator
			list.Add(115);// Goliath_Pickaxe
			list.Add(115);// Goliath_Pickaxe
			loot = new List<Tuple<InventoryItemType, int>>
			{
				Tuple.Create<InventoryItemType, int>(InventoryItemType.titanium_ore, 15400)
			};
			interruptionTemplate.AddInitialWave(list, ConsoleGoalType.kill_enemies, 1f, loot, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			list = new List<int>();
			list.Add(111);// SSC_Halberd (beam boss)
			interruptionTemplate.AddWave(list, 0.56f);
			list.Add(121);// Goliath Berserker
			interruptionTemplate.AddWave(list, 0.36f);
			interruptionTemplate.AddWave(new List<int>
			{
				119,// SSC Elite halberd
				117// SSC Elite spear
			}, 0.04f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				118,// SSC Elite pike
				118// SSC Elite pike
			}, 0.04f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.configurations.Add(ElementSpawnerType.medium_circle, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.medium_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.medium_line, 0.3f);
			interruptionTemplate.maxWaves = 1;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.mo_titanium_goliath_large_ssc_t25, interruptionTemplate);

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.interruptersFaction = 3U;
			list = new List<int>();
			list.Add(116);// Goliath_Drillbore
			list.Add(115);// Goliath_Pickaxe
			list.Add(115);// Goliath_Pickaxe
			loot = new List<Tuple<InventoryItemType, int>>
			{
				Tuple.Create<InventoryItemType, int>(InventoryItemType.rhodium_ore, 3200)
			};
			interruptionTemplate.AddInitialWave(list, ConsoleGoalType.kill_enemies, 1f, loot, ShufflePhrases(new List<List<string>> { GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.AddWave(new List<int>
			{
				111// SSC_Halberd (beam boss)
			}, 0.9f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				117,// SSC Elite spear
				117// SSC Elite spear
			}, 0.05f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				118// SSC Elite pike
			}, 0.05f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 8)) }));
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			interruptionTemplate.maxWaves = 1;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.mo_rhodium_goliath_medium_ssc_t25, interruptionTemplate);

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.interruptersFaction = 3U;
			list = new List<int>();
			list.Add(120);// Goliath Excavator
			list.Add(115);// Goliath_Pickaxe
			list.Add(115);// Goliath_Pickaxe
			loot = new List<Tuple<InventoryItemType, int>>
			{
				Tuple.Create<InventoryItemType, int>(InventoryItemType.rhodium_ore, 7400)
			};
			interruptionTemplate.AddInitialWave(list, ConsoleGoalType.kill_enemies, 1f, loot, ShufflePhrases(new List<List<string>> { GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.averageWaveInterval = 1f;// 240f;
			interruptionTemplate.AddWave(new List<int>
			{
				111// SSC_Halberd (beam boss)
			}, 0.95f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				119,// SSC Elite halberd
				117,// SSC Elite spear
				117// SSC Elite spear
			}, 0.05f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.configurations.Add(ElementSpawnerType.large_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.large_crescent, 0.3f);
			interruptionTemplate.maxWaves = 1;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.mo_rhodium_goliath_large_ssc_t25, interruptionTemplate);

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.interruptersFaction = 3U;
			list = new List<int>();
			list.Add(116);// Goliath_Drillbore
			list.Add(116);// Goliath_Drillbore
			list.Add(115);// Goliath_Pickaxe
			loot = new List<Tuple<InventoryItemType, int>>
			{
				Tuple.Create<InventoryItemType, int>(InventoryItemType.mitraxit_ore, 1000)
			};
			interruptionTemplate.AddInitialWave(list, ConsoleGoalType.kill_enemies, 1f, loot, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.averageWaveInterval = 1f;//150f;
			interruptionTemplate.AddWave(new List<int>
			{
				92,// SSC_Inquisitor
				92// SSC_Inquisitor
			}, 0.9f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				117,// SSC Elite spear
				117,// SSC Elite spear
				118// SSC Elite pike
			}, 0.05f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				117,// SSC Elite spear
				117,// SSC Elite spear
				113// SSC_Exorcist (Heretic variant)
			}, 0.05f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 8)) }));
			interruptionTemplate.configurations.Add(ElementSpawnerType.medium_circle, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.medium_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.medium_line, 0.3f);
			interruptionTemplate.maxWaves = 2;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.mo_mitraxit_goliath_medium_ssc_t25, interruptionTemplate);


			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.interruptersFaction = 3U;
			list = new List<int>();
			list.Add(115);// Goliath_Pickaxe
			list.Add(115);// Goliath_Pickaxe
			list.Add(115);// Goliath_Pickaxe
			loot = new List<Tuple<InventoryItemType, int>>
			{
				Tuple.Create<InventoryItemType, int>(InventoryItemType.gold_ore, 3000)
			};
			interruptionTemplate.AddInitialWave(list, ConsoleGoalType.kill_enemies, 1f, loot, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.averageWaveInterval = 1f;// 300f;
			interruptionTemplate.AddWave(new List<int>
			{
				88,// SSC_Rapier
				88// SSC_Rapier
			}, 1f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 6)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				117,// SSC Elite spear
				117// SSC Elite spear
			}, 0.05f, ShufflePhrases(new List<List<string>> { GetRandomSSCFightPhrases(Squirrel3RNG.Next(4)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 7)) }));
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			interruptionTemplate.maxWaves = 1;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.mo_gold_goliath_medium_ssc_t25, interruptionTemplate);

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.AddWave(new List<int>
			{
				35, //tiny pirate // SSC Runner
				33, // SSC Stilleto
				35 //tiny pirate // SSC Runner
			}, ConsoleGoalType.warp_jump, 0.65f, null, new List<string> { "Hello, friend.", "We received your distress call.", "Waiting for your hail.", "We are here to help you, please respond." });
			interruptionTemplate.interruptersFaction = 8U;
			interruptionTemplate.averageWaveInterval = 1f;//240f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			INTERRUPTION_BAG.AddTemplate(InterruptionType.friendly_pirates_call, interruptionTemplate); // friendly pirates bringing new ship
			
			interruptionTemplate = new InterruptionTemplate();
			list = new List<int>();
			list.Add(35);//tiny pirate // SSC Runner
			list.Add(35);//tiny pirate // SSC Runner
			list.Add(35);//tiny pirate // SSC Runner
			loot = new List<Tuple<InventoryItemType, int>>
			{
			};
			interruptionTemplate.AddInitialWave(list, ConsoleGoalType.reach_destination, 1f, loot, ShufflePhrases(new List<List<string>> { GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				35, //tiny pirate // SSC Runner
				35, //tiny pirate // SSC Runner
			}, ConsoleGoalType.reach_destination, 0.65f, null, ShufflePhrases(new List<List<string>> { new List<string> { "We are here to collect.", "Pay us or die."}, GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				36, //small pirate // SSC Messager
				37, //med pirate // SSC Courier
			}, ConsoleGoalType.reach_destination, 0.65f, null, ShufflePhrases(new List<List<string>> { new List<string> { "Let the party begin.", "Now you're screwed." }, GetRandomPirateInsults(Squirrel3RNG.Next(5)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				36, //small pirate // SSC Messager
				36 //small pirate // SSC Messager
			}, ConsoleGoalType.reach_destination, 0.65f, null, ShufflePhrases(new List<List<string>> { new List<string> { "Ready to destroy.", "Comin' in hot." }, GetRandomPirateInsults(Squirrel3RNG.Next(1)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));

			interruptionTemplate.interruptersFaction = 4U;
			interruptionTemplate.averageWaveInterval = 1f;//240f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			interruptionTemplate.maxWaves = 10;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.home_siege_pirate_t1, interruptionTemplate); // homebase siege pirate T1

			interruptionTemplate = new InterruptionTemplate();
			list = new List<int>();
			list.Add(37);//med pirate // SSC Courier
			list.Add(37);//med pirate // SSC Courier
			list.Add(37);//med pirate // SSC Courier
			loot = new List<Tuple<InventoryItemType, int>>
			{
			};
			interruptionTemplate.AddInitialWave(list, ConsoleGoalType.reach_destination, 1f, loot, ShufflePhrases(new List<List<string>> { GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));

			interruptionTemplate.AddWave(new List<int>
			{
				37, //med pirate // SSC Courier
				37, //med pirate // SSC Courier
				37 //med pirate // SSC Courier
			}, ConsoleGoalType.reach_destination, 0.65f, null, ShufflePhrases(new List<List<string>> { new List<string> { "We are here to collect.", "Pay us or die." }, GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				35, //tiny pirate // SSC Runner
				37, //med pirate // SSC Courier
			}, ConsoleGoalType.reach_destination, 0.65f, null, ShufflePhrases(new List<List<string>> { new List<string> { "Let the party begin.", "Now you're screwed." }, GetRandomPirateInsults(Squirrel3RNG.Next(5)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				35, //tiny pirate // SSC Runner
				53//Pirate cutthroat // SSC Hauler
			}, ConsoleGoalType.reach_destination, 0.65f, null, ShufflePhrases(new List<List<string>> { new List<string> { "What a nice station you have.", "Comin' in hot." }, GetRandomPirateInsults(Squirrel3RNG.Next(1)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));

			interruptionTemplate.interruptersFaction = 4U;
			interruptionTemplate.averageWaveInterval = 1f;//240f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			interruptionTemplate.maxWaves = 10;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.home_siege_pirate_t2, interruptionTemplate); // homebase siege pirate T2

			interruptionTemplate = new InterruptionTemplate();
			list = new List<int>();
			list.Add(37); //med pirate // SSC Courier
			list.Add(37); //med pirate // SSC Courier
			list.Add(37); //med pirate // SSC Courier
			loot = new List<Tuple<InventoryItemType, int>>
			{
			};
			interruptionTemplate.AddInitialWave(list, ConsoleGoalType.reach_destination, 1f, loot, ShufflePhrases(new List<List<string>> { GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));

			interruptionTemplate.AddWave(new List<int>
			{
				37, //med pirate // SSC Courier
				37, //med pirate // SSC Courier
				37, //med pirate // SSC Courier
				37, //med pirate // SSC Courier
				37, //med pirate // SSC Courier
				37 //med pirate // SSC Courier
			}, ConsoleGoalType.reach_destination, 0.65f, null, ShufflePhrases(new List<List<string>> { new List<string> { "We are here to collect.", "Pay us or die." }, GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				53,//Pirate cutthroat // SSC Hauler
				37, //med pirate // SSC Courier
				53//Pirate cutthroat // SSC Hauler
			}, ConsoleGoalType.reach_destination, 0.65f, null, ShufflePhrases(new List<List<string>> { new List<string> { "Let the party begin.", "Now you're screwed." }, GetRandomPirateInsults(Squirrel3RNG.Next(5)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				53,//Pirate cutthroat // SSC Hauler
				53//Pirate cutthroat // SSC Hauler
			}, ConsoleGoalType.reach_destination, 0.65f, null, ShufflePhrases(new List<List<string>> { new List<string> { "What a nice station you have.", "Comin' in hot." }, GetRandomPirateInsults(Squirrel3RNG.Next(1)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));

			interruptionTemplate.interruptersFaction = 4U;
			interruptionTemplate.averageWaveInterval = 1f;//240f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			interruptionTemplate.maxWaves = 10;
			INTERRUPTION_BAG.AddTemplate(InterruptionType.home_siege_pirate_t25, interruptionTemplate); // homebase siege pirate T2.5

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.AddWave(new List<int>
			{
				35, //tiny pirate // SSC Runner
				35, //tiny pirate // SSC Runner
				35, //tiny pirate // SSC Runner
			}, ConsoleGoalType.warp_jump, 0.65f, null, ShufflePhrases(new List<List<string>> { new List<string> { "We will destroy those assholes.", "Somebody called for help?" }, GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				36, //small pirate // SSC Messager
				37, //med pirate // SSC Courier
			}, ConsoleGoalType.warp_jump, 0.65f, null, ShufflePhrases(new List<List<string>> { new List<string> { "Hang in there.", "What a mess." }, GetRandomPirateInsults(Squirrel3RNG.Next(5)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				36, //small pirate // SSC Messager
				36 //small pirate // SSC Messager
			}, ConsoleGoalType.warp_jump, 0.65f, null, ShufflePhrases(new List<List<string>> { new List<string> { "Come on, you're gonna love it", "This is gonna be quick." }, GetRandomPirateInsults(Squirrel3RNG.Next(1)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));

			interruptionTemplate.interruptersFaction = 8U;
			interruptionTemplate.averageWaveInterval = 1f;//240f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			INTERRUPTION_BAG.AddTemplate(InterruptionType.home_siege_help_t1, interruptionTemplate); // homebase siege help T1

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.AddWave(new List<int>
			{
				37, //med pirate // SSC Courier
				37, //med pirate // SSC Courier
				37 //med pirate // SSC Courier
			}, ConsoleGoalType.warp_jump, 0.65f, null, ShufflePhrases(new List<List<string>> { new List<string> { "We will destroy those assholes.", "Somebody called for help?" }, GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				35, //tiny pirate // SSC Runner
				37, //med pirate // SSC Courier
			}, ConsoleGoalType.warp_jump, 0.65f, null, ShufflePhrases(new List<List<string>> { new List<string> { "Hang in there.", "What a mess." }, GetRandomPirateInsults(Squirrel3RNG.Next(5)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				35, //tiny pirate // SSC Runner
				53//Pirate cutthroat // SSC Hauler
			}, ConsoleGoalType.warp_jump, 0.65f, null, ShufflePhrases(new List<List<string>> { new List<string> { "Come on, you're gonna love it", "This is gonna be quick." }, GetRandomPirateInsults(Squirrel3RNG.Next(1)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));

			interruptionTemplate.interruptersFaction = 8U;
			interruptionTemplate.averageWaveInterval = 1f;//240f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			INTERRUPTION_BAG.AddTemplate(InterruptionType.home_siege_help_t2, interruptionTemplate); // homebase siege help T2

			interruptionTemplate = new InterruptionTemplate();
			interruptionTemplate.AddWave(new List<int>
			{
				37, //med pirate // SSC Courier
				37, //med pirate // SSC Courier
				37 //med pirate // SSC Courier
			}, ConsoleGoalType.warp_jump, 0.65f, null, ShufflePhrases(new List<List<string>> { new List<string> { "We will destroy those assholes.", "Somebody called for help?" }, GetRandomPirateInsults(Squirrel3RNG.Next(2)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				53,//Pirate cutthroat // SSC Hauler
				37, //med pirate // SSC Courier
				53//Pirate cutthroat // SSC Hauler
			}, ConsoleGoalType.warp_jump, 0.65f, null, ShufflePhrases(new List<List<string>> { new List<string> { "Hang in there.", "What a mess." }, GetRandomPirateInsults(Squirrel3RNG.Next(5)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));
			interruptionTemplate.AddWave(new List<int>
			{
				53,//Pirate cutthroat // SSC Hauler
				53//Pirate cutthroat // SSC Hauler
			}, ConsoleGoalType.warp_jump, 0.65f, null, ShufflePhrases(new List<List<string>> { new List<string> { "Come on, you're gonna love it", "This is gonna be quick." }, GetRandomPirateInsults(Squirrel3RNG.Next(1)), GetRandomFightPhrases(Squirrel3RNG.Next(3, 5)) }));

			interruptionTemplate.interruptersFaction = 8U;
			interruptionTemplate.averageWaveInterval = 1f;//240f;
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_cluster, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_crescent, 0.3f);
			interruptionTemplate.configurations.Add(ElementSpawnerType.small_line, 0.3f);
			INTERRUPTION_BAG.AddTemplate(InterruptionType.home_siege_help_t25, interruptionTemplate); // homebase siege help T2.5

		}

		public static void AddTemplate(InterruptionType type, InterruptionTemplate template)
		{
			template.type = type;
			INTERRUPTION_BAG.templates.Add(type, template);
		}

		public static List<String> GetRandomPirateInsults(int count)
		{
			List<String> insults = new List<String>();
			List<String> threats = new List<String>{
				"Ye fight like a lass,",
				"I'll hang ye by yer ears 'til ye rot,",
				"Yer mother be so fat, she be falling off both sides of yer ship,",
				"I'll dance the hornpipe o'er yer grave,",
				"Prepare for yer doom,",
				"Yer face be good fer stoppin' me torpedos,",
				"I'll space yer balls,",
				"Yer doom be at hand,",
				"I'll cut yer ship in pieces,",
				"We'll plunder yer booty,",
				"Curse you for breathin',",
				"'Tis good ye're here. I'll dress ye as a lettuce and feed ye to the snails,",
				"Dead men tell no tales,",
				"Blimey! 'Tis a space monster! Oh. 'Tis ye,",
				"Yer breath be deadlier than yer cannons,",
				"I'll reduce yer ship to rubble,",
				"Ye're cruising for a bruising,",
				"Welcome to the good ship FACE PUNCH. I'm the Captain!",
				"Have ye ever heard cannon music? Well dance to THIS",
				"Welcome to PAIN TIME with a Pirate! Starring YOU!",
				"Bite ME will ye? Chew on THIS",
				"Happy now? Ye have me FULL attention!",
				"Go ahead an' run! I LIKE a movin' target, ",
				"I can't hear ye! Scream a bit louder",
				"Have a meal of me bullets,",
				"Ye can surrender after I kill yer sorry arse,"
			};
			List<String> humiliationsp1 = new List<String>{
				" ye poxy,",
				" ye scurvy-ridden,",
				" ye flea-bitten,",
				" ye vile,",
				" ye salty,",
				" ye sorry,",
				" ye mutinous,",
				" ye knee-knockin',",
				" ye pestilent,",
				" ye chum-smellin',",
				" ye slack-jawed,",
				" ye wretched,",
				" ye squiffy,",
				" ye pitiful,",
				" ye foul-smellin',",
				" ye scabby arsed,",
				" ye pee soaked,",
				" ye puke faced,",
				" ye butt ugly,"
			};
			List<String> humiliationsp2 = new List<String>{
				" mangy swab!",
				" blasted swindler!",
				" plagued maggot!",
				" ghastly dullard!",
				" lice-infested scalawag!",
				" bloody cretin!",
				" spineless cockroach!",
				" dreadful buffoon!",
				" pitiful clod!",
				" scabby hornswaggler!",
				" foul lummox!",
				" worthless blaggart!",
				" rotten simpleton!",
				" stumblin' idiot!",
				" feckless monkey!",
				" cowardly gob!",
				" greasey son of a punk alley harlot!",
				" whiny little crap sack!",
				" slimey gobshyte!"
			};

			List<String> happypiratenoises = new List<String>{
				" ... Arrr!",
				" Arggh!",
				" Arrrrgh!",
				" ... Yarr!",
				" ... Garr!"
			};

			for (int i = 0; i < count; i++)
			{
				string insult = threats[Squirrel3RNG.Next(threats.Count)] + humiliationsp1[Squirrel3RNG.Next(humiliationsp1.Count)] + humiliationsp2[Squirrel3RNG.Next(humiliationsp2.Count)];
				if (Squirrel3RNG.Next(10) == 1)
				{
					insult += happypiratenoises[Squirrel3RNG.Next(happypiratenoises.Count)];
				}
				insults.Add(insult);
			}

			return insults;
		}

		public static List<String> GetRandomSSCFightPhrases(int count)
		{
			List<String> phrases = new List<String>();
			List<String> thoughts = new List<String>{
			"Engaging the enemy!",
			"The enemy has discovered us.",
			"We meet our fate.",
			"I have met the enemy!",
			"Battle is upon us!",
			"Let us finish this!",
			"Leave no mercy!",
			"The end has come!",
			"Death comes to all!",
			"They shall fall!",
			"Spare none who oppose us.",
			"Time for battle.",
			"Prepare to engage.",
			"Engage!",
			"Break on through!",
			"Ready to fire!",
			"It shall be done.",
			"Eradication!",
			"May your death be swift.",
			"Balls to the walls!",
			"Roger that.",
			"We have met the enemy and they are ours!",
			"Full speed ahead!",
			"Take him down!",
			"Don't let the enemy escape!",
			"Victory will be ours!",
			"Today is a good day to win!",
			"Clear the Way!",
			"No fear! No mercy! No remorse!",
			"Let them taste our bullets!",
			"We will be victorious!",
			"Tear them down!",
			"Destroy everything!",
			"Victory or death!",
			"Bring it on!",
			"Cut them down!",
			"Witness our might!",
			"Fire at will!",
			"Leave them with nothing!",
			"We fight as one!",
			"Feel our might!",
			"Let's go!",
			"To victory!",
			"Here we come!",
			"Time to pay!",
			"Don't run, you're already dead!",
			"Let them feel true pain!",
			"You belong to us now!",
			"Tear them to pieces!",
			"Take no prisoners!",
			"Let them meet our best!",
			"Let's teach them how it's done!",
			"Time to have fun!",
			"Burn!",
			"Kill every last one of them!",
			"Time for a warm up.",
			"Death to the weak!",
			"You're already dead!",
			"Turn them to ash!",
			"Death comes tomorrow!",
			"Bring on the pain!",
			"Prepare to die!",
			"The end is here!",
			"Die!",
			"Make them fear our name!",
			"Prepare for destruction!",
			"Leave only ashes!",
			"For justice!",
			"That one is mine!"

			};
			for (int i = 0; i < count; i++)
			{
				string phrase = thoughts[Squirrel3RNG.Next(thoughts.Count)];
				phrases.Add(phrase);
			}

			return phrases;
		}
		public static List<String> GetRandomFightPhrases(int count)
		{
			List<String> phrases = new List<String>();
			List<String> thoughts = new List<String>{
			"Give me " + new List<String> {"banana", "chocolate" }[Squirrel3RNG.Next(2)] + " and nobody gets hurt.",
			"What am i doing in this shithole.",
			"Just give up already",
			"I am going to tear your " + new List<String> {"engine", "rear" }[Squirrel3RNG.Next(2)] + " apart.",
			"I used to be a space pirate like you, but then they ravished my booty.",
			"I used to be a space adventurer like you, but then i took a bullet in the knee!",
			"Life is PAIN.",
			"Take my love rocket in your face!",
			"Let me carve a smile on your ship with my gun!",
			"You fight like a " + new List<String> {"boy", "girl", "baby", "wimp", "pussy", "weeb"}[Squirrel3RNG.Next(6)] + ".",
			"Do you like the taste of my bullets?",
			"Santa got a big nasty present for you!",
			"This is gonna be fun!",
			"I like making you cry.",
			"I'm gonna spray you with bullets till you love me!",
			"Its bullet time, punk!",
			"I love pain!",
			"Hit me with whatever you want, I'll still hit you harder!",
			"I'm not impressed",
			"You can't outrun me!",
			"I'm gaining on you, and it's only a matter of time now!",
			"I don't have time to chase you around!",
			"Fly over here, so i can hit you!",
			"One day i want to fight a gorilla!",
			"I can't take this anymore.",
			"Hit me harder!",
			"This is too much for me.",
			"I'm gonna kick your ass!",
			"Get back here!",
			"I eat wimps for breakfast!",
			"Here comes the pain!",
			"Guess what time it is! Yep, time for PAIN",
			"You want a piece of me?",
			"Let's make this quick!",
			"And i thought today was gonna be boring!",
			"Let's have some fun!",
			"You're dead meat!",
			"You.. Bastrad!",
			"Oh, this is gonna be fun!",
			"You think you could challenge me?",
			"Comet me bro, i will destroy Uranus!",
			"Hey, you won't know what hit you, punk",
			"And i thought this was gonna be a boring day.",
			"I'm ready to cause some real damage!",
			"Let's dance!",
			"I've come to kick ass and chew bubble gum, but i'm all outta gum.",
			"I've got time to kill.",
			"I can't wait to make you love me!",
			"Let's see you pick someone my size!",
			"This oughta be fun!",
			"Ok, buddy, you asked for this!",
			"Me and you, lovers forever!",
			"You bought a ticket for the pain train, choo choo!",
			"This is the end for you!",
			"You ready for the pain?",
			"You wanna be tough? Come on, be tough with me!",
			"You're going down, smartass!",
			"Let's see how tough you really are!",
			"Not so tough anymore, are we?",
			"I can watch you cry all day.",
			"I can do this all day!",
			"Now I'm gonna whoop your ass!",
			"I am going to destroy you now!",
			"Prepare to get smacked!",
			"I have decided to make you suffer!",
			"You don't mess with me! ever!",
			"Been waiting for this!",
			"You bore me to death!",
			"I'm so going to hurt you!",
			"You're getting jacked up, punk!",
			"You'll be sorry!",
			"You're messing with wrong people!",
			"Feel the wrath of Nerd rage!",
			"It's time for a new turn offensive!",
			"You are so totally dead!",
			"You brought this upon yourself!",
			"Come here, scum!",
			"My momma says i'm fearsome!",
			"Now i'm gonna hurt you.",
			"You're in trouble now.",
			"I'll punish you for your existance.",
			"This jerk is going to suffer."
			};
			for (int i = 0; i < count; i++)
			{
				string insult = thoughts[Squirrel3RNG.Next(thoughts.Count)];
				phrases.Add(insult);
			}

			return phrases;
		}

		public static List<String> ShufflePhrases(List<List<String>> toshuffle)
		{
			List<String> shuffledphrases = new List<String>();
			//List<String> secondlist = new List<String>();


			foreach (List<String> list in toshuffle)
			{
				foreach (string phrase in list)
				{
					/*
					if (Squirrel3RNG.Next(2) == 1)
					{
						shuffledphrases.Add(phrase);
					}
					else
					{
						secondlist.Add(phrase);
					}
					*/
					shuffledphrases.Add(phrase);
				}
			}
			//shuffledphrases.AddRange(secondlist);
			RANDOM.shuffle(ref shuffledphrases);
			return shuffledphrases;
		}

		


		public static InterruptionType GetRandomTemplate(ulong faction)
		{
			//int maxdifficulty = Globals.DifficultyFromCost(HW_CHARACTER_DATA_Extensions.mostExpensiveDesign());
			int maxdifficulty = Globals.mostExpensiveBuildableDesigndifficulty;
			int difficulty = maxdifficulty;
			int currentdifficulty;
			if (PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && !PLAYER.currentShip.cosm.isStation && Globals.currentshipdifficulty > -1)
			{
				difficulty = Globals.currentshipdifficulty;
			}
			if(difficulty >= maxdifficulty)
			{ 
				currentdifficulty = Math.Max((int)Math.Round(MathHelper.Clamp(((float)CHARACTER_DATA.shipsUnlocked() * HWCONFIG.GlobalDifficulty), 1f, 100f )), difficulty * 3);
			}
			else
			{
				currentdifficulty = difficulty * 3;
			}

			InterruptionType result = InterruptionType.mo_iron_free_small_fre_t1;

			if (faction == 3UL)
			{
				if (currentdifficulty <= 12)
				{
					switch (Squirrel3RNG.Next(3))
					{
						case 0:
							result = InterruptionType.ambush_small_ssc_t1;
							break;
						case 1:
							result = InterruptionType.iron_large_ssc_t1;
							break;
						case 2:
							result = InterruptionType.ithacit_small_ssc_t1;
							break;
						default:
							result = InterruptionType.ambush_small_ssc_t1;
							break;
					}
				}
				if (currentdifficulty > 12 && currentdifficulty <= 21)
				{
					switch (Squirrel3RNG.Next(4))
					{
						case 0:
							result = InterruptionType.titanium_small_ssc_t2;
							break;
						case 1:
							result = InterruptionType.rhodium_small_ssc_t2;
							break;
						case 2:
							result = InterruptionType.titanium_medium_ssc_t2;
							break;
						case 3:
							result = InterruptionType.rhodium_medium_ssc_t2;
							break;
						default:
							result = InterruptionType.titanium_small_ssc_t2;
							break;
					}
				}
				if (currentdifficulty > 21)
				{
					switch (Squirrel3RNG.Next(8))
					{
						case 0:
							result = InterruptionType.titanium_large_ssc_t25;
							break;
						case 1:
							result = InterruptionType.rhodium_large_ssc_t25;
							break;
						case 2:
							result = InterruptionType.mo_titanium_goliath_medium_ssc_t25;
							break;
						case 3:
							result = InterruptionType.mo_titanium_goliath_large_ssc_t25;
							break;
						case 4:
							result = InterruptionType.mo_rhodium_goliath_medium_ssc_t25;
							break;
						case 5:
							result = InterruptionType.mo_rhodium_goliath_large_ssc_t25;
							break;
						case 6:
							result = InterruptionType.mo_mitraxit_goliath_medium_ssc_t25;
							break;
						case 7:
							result = InterruptionType.mo_gold_goliath_medium_ssc_t25;
							break;
						default:
							result = InterruptionType.titanium_large_ssc_t25;
							break;
					}
				}
			}

			if (faction == 4UL)
			{
				if (currentdifficulty < 24)
				{
					switch (Squirrel3RNG.Next(5))
					{
						case 0:
							result = InterruptionType.ambush_small_pirate_t1;
							break;
						case 1:
							result = InterruptionType.gold_medium_pirate_t1;
							break;
						case 2:
							result = InterruptionType.mo_ithacit_pirates_small_t1;
							break;
						case 3:
							result = InterruptionType.mo_iron_pirates_small_t1;
							break;
						case 4:
							result = InterruptionType.mo_gold_pirates_small_t1;
							break;
						default:
							result = InterruptionType.ambush_small_pirate_t1;
							break;
					}
				}
				if (currentdifficulty >= 24)// && shipsUnlocked <= 20)
				{
					switch (Squirrel3RNG.Next(4))
					{
						case 0:
							result = InterruptionType.blood_pirates_t2;
							break;
						case 1:
							result = InterruptionType.blood_pirates_2_t2;
							break;
						case 2:
							result = InterruptionType.blood_pirates_3_t2;
							break;
						case 3:
							result = InterruptionType.blood_pirates_4_t2;
							break;
						default:
							result = InterruptionType.blood_pirates_t2;
							break;
					}
				}
				/*
				if (shipsUnlocked > 20)
				{
					switch (Squirrel3RNG.Next(8))
					{
						case 0:
							result = InterruptionType.titanium_large_ssc_t25;
							break;
						case 1:
							result = InterruptionType.rhodium_large_ssc_t25;
							break;
						case 2:
							result = InterruptionType.mo_titanium_goliath_medium_ssc_t25;
							break;
						case 3:
							result = InterruptionType.mo_titanium_goliath_large_ssc_t25;
							break;
						case 4:
							result = InterruptionType.mo_rhodium_goliath_medium_ssc_t25;
							break;
						case 5:
							result = InterruptionType.mo_rhodium_goliath_large_ssc_t25;
							break;
						case 6:
							result = InterruptionType.mo_mitraxit_goliath_medium_ssc_t25;
							break;
						case 7:
							result = InterruptionType.mo_gold_goliath_medium_ssc_t25;
							break;
						default:
							result = InterruptionType.titanium_large_ssc_t25;
							break;
					}
				}
				*/
			}

			return result;

		}

		public static InterruptionType GetRandomTemplate()
		{
			//int maxdifficulty = Globals.DifficultyFromCost(HW_CHARACTER_DATA_Extensions.mostExpensiveDesign());
			int maxdifficulty = Globals.mostExpensiveBuildableDesigndifficulty;
			int difficulty = maxdifficulty;
			int currentdifficulty;
			if (PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && !PLAYER.currentShip.cosm.isStation && Globals.currentshipdifficulty > -1)
			{
				difficulty = Globals.currentshipdifficulty;
			}
			if (difficulty >= maxdifficulty)
			{
				currentdifficulty = Math.Max((int)Math.Round(MathHelper.Clamp(((float)CHARACTER_DATA.shipsUnlocked() * HWCONFIG.GlobalDifficulty), 1f, 100f)), difficulty * 3);
			}
			else
			{
				currentdifficulty = difficulty * 3;
			}

			InterruptionType result = InterruptionType.ambush_small_pirate_t1;
			if (currentdifficulty <= 12)
			{
				switch (Squirrel3RNG.Next(10))
				{
					case 0:
						result = InterruptionType.ambush_small_pirate_t1;
						break;
					case 1:
						result = InterruptionType.ambush_small_ssc_t1;
						break;
					case 2:
						result = InterruptionType.gold_medium_pirate_t1;
						break;
					case 3:
						result = InterruptionType.iron_large_ssc_t1;
						break;
					case 4:
						result = InterruptionType.ithacit_small_ssc_t1;
						break;
					case 5:
						result = InterruptionType.mo_iron_free_small_fre_t1;
						break;
					case 6:
						result = InterruptionType.mo_gold_free_small_fre_t1;
						break;
					case 7:
						result = InterruptionType.mo_ithacit_pirates_small_t1;
						break;
					case 8:
						result = InterruptionType.mo_iron_pirates_small_t1;
						break;
					case 9:
						result = InterruptionType.mo_gold_pirates_small_t1;
						break;
					default:
						result = InterruptionType.ambush_small_pirate_t1;
						break;
				}
			}
			if (currentdifficulty > 12 && currentdifficulty <= 24)
			{
				switch (Squirrel3RNG.Next(4))
				{
					case 0:
						result = InterruptionType.titanium_small_ssc_t2;
						break;
					case 1:
						result = InterruptionType.rhodium_small_ssc_t2;
						break;
					case 2:
						result = InterruptionType.titanium_medium_ssc_t2;
						break;
					case 3:
						result = InterruptionType.rhodium_medium_ssc_t2;
						break;
					default:
						result = InterruptionType.titanium_small_ssc_t2;
						break;
				}
			}
			if (currentdifficulty > 24)
			{
				switch (Squirrel3RNG.Next(12))
				{
					case 0:
						result = InterruptionType.titanium_large_ssc_t25;
						break;
					case 1:
						result = InterruptionType.rhodium_large_ssc_t25;
						break;
					case 2:
						result = InterruptionType.mo_titanium_goliath_medium_ssc_t25;
						break;
					case 3:
						result = InterruptionType.mo_titanium_goliath_large_ssc_t25;
						break;
					case 4:
						result = InterruptionType.mo_rhodium_goliath_medium_ssc_t25;
						break;
					case 5:
						result = InterruptionType.mo_rhodium_goliath_large_ssc_t25;
						break;
					case 6:
						result = InterruptionType.mo_mitraxit_goliath_medium_ssc_t25;
						break;
					case 7:
						result = InterruptionType.mo_gold_goliath_medium_ssc_t25;
						break;
					case 8:
						result = InterruptionType.blood_pirates_t2;
						break;
					case 9:
						result = InterruptionType.blood_pirates_2_t2;
						break;
					case 10:
						result = InterruptionType.blood_pirates_3_t2;
						break;
					case 11:
						result = InterruptionType.blood_pirates_4_t2;
						break;


					default:
						result = InterruptionType.titanium_large_ssc_t25;
						break;
				}
			}
			return result;
		}


		public static InterruptionTemplate GetTemplate(InterruptionType type)
		{
			if (INTERRUPTION_BAG.templates == null) { INTERRUPTION_BAG.Init(); }

			bool flag = INTERRUPTION_BAG.templates.ContainsKey(type);
			InterruptionTemplate result;
			if (flag)
			{
				result = INTERRUPTION_BAG.templates[type];
			}
			else
			{
				result = INTERRUPTION_BAG.templates[InterruptionType.ambush_small_pirate_t1];
			}
			return result;
		}

		public static Dictionary<InterruptionType, InterruptionTemplate> templates;

	}
	public readonly struct InterruptionBasic
	{
		public InterruptionBasic(string interruptionid, Point grid, Vector2 position)
		{
			InterruptionId = interruptionid;
			Grid = grid;
			Position = position;
		}
		public readonly string InterruptionId;
		public readonly Point Grid;
		public readonly Vector2 Position;
	}
	public class Interruption
	{
		public string id = Guid.NewGuid().ToString();
		//private Dictionary<AssetsNodeType, float> assetsNodes;
		public float rotation;
		public Matrix rotationMatrix;
		private float returnTimer = 0f;
		private float returnDuration = 600f;
		private Dictionary<Wave, List<String>> waves;
		//private Dictionary<Ship, List<String>> waves;
		private float wavesWeight;
		private float waveTimer;
		private float waveIntervalAverage;
		private float currentWaveInterval;
		public int maxWaves = 3;
		public int currentWave = 0;
		private Tuple<Wave, List<String>> initialWave;
		private Tuple<Wave, List<String>> toSpawn;
		public Vector2 position;
		public Point grid;
		private float timer;
		private float signatureTimer;
		private float accumulatedSignature;
		public List<Tuple<ulong, List<String>>> activeShips;
		public List<Vector2> spawnPoints;
		//private List<Vector2> elementPositions;
		//private InterruptionType interruptionType;
		public InterruptionType templateUsed;
		public uint interruptersFaction = 3U;
		private ElementSpawnerType elementConfiguration;
		public bool assetsSpawned = false;
		private TimeSpan spawnStamp;
		public float timeToReset;
		//public List<Interruption.NodeSlot> assetsSlots;
		public List<String> conversations;
		private int currentmessage = 0;
		private int maxmessages = 0;
		public bool interdicting = false;
		public bool shuffle = false;
		public float interdictTimer = 0f;
		private float speakTimer = 0f;
		public Vector2 interdictionSpot;
		public List<ActiveEffect> activeEffects;
		public bool initWaveQueued; // representing a spawning stage of the interruption for save/load
		public int wavesQueued = 0; // representing a spawning stage of the interruption for save/load
		private ConcurrentQueue<Tuple<Ship, List<String>>> shipQueue = new ConcurrentQueue<Tuple<Ship, List<String>>>();
		private ConcurrentQueue<Ship> QuewedShipsToRecycle = new ConcurrentQueue<Ship>();


		public Interruption(InterruptionType type, Vector2 position, Point grid)
		{
			this.templateUsed = type;
			InterruptionTemplate template = INTERRUPTION_BAG.GetTemplate(type);
			this.rotation = RANDOM.randomRotation();
			this.position = position;
			this.grid = grid;
			this.spawnPoints = new List<Vector2>();
			//this.assetsSlots = new List<Interruption.NodeSlot>();
			//this.elementPositions = new List<Vector2>();
			this.rotationMatrix = Matrix.CreateRotationZ(this.rotation);
			this.conversations = new List<String>();
			this.SetTempalte(template, false);
			this.activeEffects = new List<ActiveEffect>();
			this.activeShips = new List<Tuple<ulong, List<String>>>();
		}

		public Interruption(InterruptionType type, Vector2 position, Point grid, List<String> conversations, bool shuffle)
		{
			this.templateUsed = type;
			InterruptionTemplate template = INTERRUPTION_BAG.GetTemplate(type);
			this.position = position;
			this.grid = grid;
			this.spawnPoints = new List<Vector2>();
			//this.assetsSlots = new List<Interruption.NodeSlot>();
			//this.elementPositions = new List<Vector2>();
			this.rotation = RANDOM.randomRotation();
			this.rotationMatrix = Matrix.CreateRotationZ(this.rotation);
			this.conversations = conversations;
			this.shuffle = shuffle;
			this.SetTempalte(template, false);
			this.activeEffects = new List<ActiveEffect>();
			this.activeShips = new List<Tuple<ulong, List<String>>>();
		}

		public Interruption(InterruptionType type, Vector2 position, Point grid, List<String> conversations, bool shuffle, bool interdicting, string id, bool initWaveQueued, int wavesQueued)
		{
			this.templateUsed = type;
			InterruptionTemplate template = INTERRUPTION_BAG.GetTemplate(this.templateUsed);
			this.position = position;
			this.interdictionSpot = this.position;
			this.grid = grid;
			this.spawnPoints = new List<Vector2>();
			//this.assetsSlots = new List<Interruption.NodeSlot>();
			//this.elementPositions = new List<Vector2>();
			this.rotation = RANDOM.randomRotation();
			this.rotationMatrix = Matrix.CreateRotationZ(this.rotation);
			this.conversations = conversations;
			this.shuffle = shuffle;
			this.initWaveQueued = initWaveQueued;
			this.wavesQueued = wavesQueued;
			this.SetTempalte(template, true);
			this.interdicting = interdicting;
			this.activeShips = new List<Tuple<ulong, List<String>>>();
			this.activeEffects = new List<ActiveEffect>();
			this.id = id;
		}

		public class ActiveEffect
		{
			public ActiveEffect(string name, Vector2 pos, float timer, float steps)
			{
				Name = name;
				Pos = pos;
				Timer = timer;
				Steps = steps;
			}
			public string Name { get; set; }
			public Vector2 Pos { get; set; }
			public float Timer { get; set; }
			public float Steps { get; set; }

		}
		public void SetTempalte(InterruptionTemplate template, bool loading)
		{
			//this.interruptionType = template.type;
			if(this.templateUsed != InterruptionType.friendly_pirates_call) // excluding special crafted not random templates from difficulty adjustment
			{
				template = AdjustDifficulty(template);
			}
			this.interruptersFaction = template.interruptersFaction;
			this.waveIntervalAverage = template.averageWaveInterval;
			this.initialWave = template.initialWave;
			if (this.initialWave != null && this.initialWave.Item1 != null && !loading) //check if an initial Wave has to spawn at first initialisation
			{
				this.initWaveQueued = true;
			}
			this.waves = template.shipWaves;
			if (this.waves != null && this.waves.Count != 0 && !loading) //check how many Waves have to spawn at first initialisation
			{
				this.wavesQueued = this.waves.Count;
			}
			this.maxWaves = template.maxWaves;
			this.elementConfiguration = (ElementSpawnerType)RANDOM.GetWeightedRandom<ElementSpawnerType>(template.configurations);
			this.SetSpawner();
		}

		private InterruptionTemplate AdjustDifficulty(InterruptionTemplate template)
		{
			//int maxdifficulty = Globals.DifficultyFromCost(HW_CHARACTER_DATA_Extensions.mostExpensiveDesign());
			int maxdifficulty = Globals.mostExpensiveBuildableDesigndifficulty;
			int currentdifficulty = maxdifficulty;
			if (PLAYER.currentShip != null && PLAYER.currentShip.cosm != null && !PLAYER.currentShip.cosm.isStation && Globals.currentshipdifficulty > -1)
			{ 
				currentdifficulty = Globals.currentshipdifficulty;
			}
			int shipstospawn = Math.Max(1, currentdifficulty / 3);
			if (currentdifficulty == 5)
			{
				shipstospawn = 1 + Squirrel3RNG.Next(2);
			}
			if (currentdifficulty == 8)
			{
				shipstospawn = 2 + Squirrel3RNG.Next(2);
			}
			if (currentdifficulty == 10)
			{
				shipstospawn = 3 + Squirrel3RNG.Next(3);
			}
			/*
			if(PLAYER.currentSession != null && PLAYER.currentSession.grid == this.grid)
			{
				currentdifficulty = (int)(currentdifficulty * (1f + PLAYER.currentSession.allShips.Values.Where(ship => ship.faction == 2UL && ship != PLAYER.currentShip).Count() * 0.5f));
			}
			else if(PLAYER.currentSession == null)
			{		
				currentdifficulty = (int)(currentdifficulty * (1f + PLAYER.currentWorld.getSession(this.grid).allShips.Values.Where(ship => ship.faction == 2UL && ship != PLAYER.currentShip).Count() * 0.5f));
			}
			currentdifficulty = Math.Max((int)Math.Ceiling((float)((currentdifficulty / 3 ) * HWCONFIG.GlobalDifficulty)), 1);
			*/

			if (template.initialWave != null && template.initialWave.Item1 != null)
			{
				List<int> temp = new List<int>();
				if (shipstospawn <= template.initialWave.Item1.shipIds.Count)
				{
					temp.AddRange(template.initialWave.Item1.shipIds.GetRange(0, shipstospawn));
				}
				else
				{
					temp.AddRange(template.initialWave.Item1.shipIds.GetRange(0, template.initialWave.Item1.shipIds.Count));
					for(int i = 0; i < shipstospawn - template.initialWave.Item1.shipIds.Count; i ++)
					{
						temp.AddRange(template.initialWave.Item1.shipIds.GetRange(Squirrel3RNG.Next(template.initialWave.Item1.shipIds.Count), 1));
					}
				}
				template.initialWave.Item1.shipIds = temp;
				template.maxWaves = Math.Max(Math.Min(template.maxWaves, (int)Math.Ceiling((float)(((float)currentdifficulty / 2 - 1) * HWCONFIG.GlobalDifficulty))), 1);
			}
			else
			{
				var tempvawesvalue = (float)currentdifficulty / 2;
				tempvawesvalue = tempvawesvalue * HWCONFIG.GlobalDifficulty;
				int wavestospawn = (int)Math.Ceiling(tempvawesvalue);
				template.maxWaves = Math.Max(Math.Min(template.maxWaves, wavestospawn), 1);
            }
			for(int i = 0 ; i < template.shipWaves.Count; i++ )
			{
				List<int> temp = new List<int>();
				if (shipstospawn <= template.shipWaves.Keys.ToList()[i].shipIds.Count)
				{
					temp.AddRange(template.shipWaves.Keys.ToList()[i].shipIds.GetRange(0, shipstospawn));
				}
				else
				{
					temp.AddRange(template.shipWaves.Keys.ToList()[i].shipIds.GetRange(0, template.shipWaves.Keys.ToList()[i].shipIds.Count));
					for (int j = 0; j < shipstospawn - template.shipWaves.Keys.ToList()[i].shipIds.Count; j++)
					{
						temp.AddRange(template.shipWaves.Keys.ToList()[i].shipIds.GetRange(Squirrel3RNG.Next(template.shipWaves.Keys.ToList()[i].shipIds.Count), 1));
					}
				}
				template.shipWaves.Keys.ToList()[i].shipIds = temp;
			}
			return template;
		}


		public bool getEffect(float elapsed, string name, Vector2 spawnPos, ref float Timer, ref float Steps)
		{
			//PARTICLE.systems[index].addParticle(particle.Position, particle.Velocity, particle.Parameters.X, particle.Parameters.Y);
			switch (name)
			{
				case "Wormhole":
					spawnPos += this.position;
					if (Steps <= 15f)
					{
						Timer += elapsed;
						Steps += elapsed;
						if (Timer >= 3f)
						{
							Timer = 0;
							if (this.interdicting)
							{
								PARTICLE.systems[30].addParticle(new Vector3(spawnPos.X, spawnPos.Y, 0f), new Vector3(0f, 0f, 1f), new Vector3(131f, 201f, 141f), 0.75f, 8.75f);
							}
						}
					}
					else
					{
						return false;
					}
					break;
				case "WarpIn":
					if (Steps <= 1f)
					{
						Timer += elapsed;
						Steps += elapsed;
						if (Timer >= 3f)
						{
							Timer = 0;
							if (this.interdicting)
							{ //0.85f, 2.15f
								PARTICLE.systems[10].addParticle(new Vector3(spawnPos.X, spawnPos.Y, 0f), 0.85f, 2.15f, 400f, 0f, 1f, 4, 5);
							}
						}
					}
					else
					{
						return false;
					}
					break;
			}
			return true;
		}
		public void GeneratePlacement()
		{
			switch (this.elementConfiguration)
			{
				case ElementSpawnerType.small_cluster:
					/*
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(250f, 0f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-250f, 0f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(0f, 250f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(0f, -250f) + RANDOM.getCirclePoint2(50f), 0f));
					this.elementPositions.Add(new Vector2(-300f, -300f));
					this.elementPositions.Add(new Vector2(300f, -320f));#
					*/
					this.spawnPoints.Add(new Vector2(0f, -5400f));
					break;
				case ElementSpawnerType.small_crescent:
					/*
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-300f, -200f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-200f, -80f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(0f, 0f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(200f, -85f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(400f, -215f) + RANDOM.getCirclePoint2(50f), 0f));
					this.elementPositions.Add(new Vector2(-300f, 20f));
					this.elementPositions.Add(new Vector2(-100f, 240f));
					this.elementPositions.Add(new Vector2(100f, 250f));
					this.elementPositions.Add(new Vector2(300f, 50f));
					*/
					this.spawnPoints.Add(new Vector2(0f, -5400f));
					break;
				case ElementSpawnerType.small_line:
					/*
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-400f, -20f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-200f, 40f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(0f, -80f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(200f, 45f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(400f, -60f) + RANDOM.getCirclePoint2(50f), 0f));
					this.elementPositions.Add(new Vector2(-200f, 300f));
					this.elementPositions.Add(new Vector2(-300f, -240f));
					*/
					this.spawnPoints.Add(new Vector2(0f, -5400f));
					break;
				case ElementSpawnerType.medium_cluster:
					/*
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(500f, 400f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(300f, 0f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(480f, -360f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(640f, -80f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-620f, 160f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-340f, 440f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-120f, -50f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-300f, -300f) + RANDOM.getCirclePoint2(50f), 0f));
					*/
					this.spawnPoints.Add(new Vector2(0f, -5400f));
					break;
				case ElementSpawnerType.medium_line:
					/*
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-600f, 0f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-400f, -40f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-200f, 80f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(0f, -180f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(200f, 90f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(400f, -80f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-600f, 0f) + RANDOM.getCirclePoint2(50f), 0f));
					*/
					this.spawnPoints.Add(new Vector2(0f, -5400f));
					break;
				case ElementSpawnerType.medium_circle:
					/*
					this.assetsSlots.Add(new Interruption.NodeSlot(SCREEN_MANAGER.AngleToVector(0f) * 500f + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(SCREEN_MANAGER.AngleToVector(0.8975979f) * 550f + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(SCREEN_MANAGER.AngleToVector(1.80519581f) * 470f + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(SCREEN_MANAGER.AngleToVector(2.69279385f) * 460f + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(SCREEN_MANAGER.AngleToVector(3.62139153f) * 560f + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(SCREEN_MANAGER.AngleToVector(4.49798965f) * 500f + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(SCREEN_MANAGER.AngleToVector(5.37558746f) * 480f + RANDOM.getCirclePoint2(50f), 0f));
					*/
					this.spawnPoints.Add(new Vector2(0f, -5400f));
					break;
				case ElementSpawnerType.large_cluster:
					/*
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(500f, 400f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(300f, 0f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(480f, -360f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(640f, -80f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-620f, 160f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-340f, 440f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-120f, -50f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-300f, -300f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-120f, -50f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-300f, -300f) + RANDOM.getCirclePoint2(50f), 0f));
					*/
					this.spawnPoints.Add(new Vector2(0f, -5400f));
					break;
				case ElementSpawnerType.large_crescent:
					/*
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-680f, -410f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-520f, -310f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-330f, -200f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(-200f, -80f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(0f, 0f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(200f, -85f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(370f, -215f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(500f, -325f) + RANDOM.getCirclePoint2(50f), 0f));
					this.assetsSlots.Add(new Interruption.NodeSlot(new Vector2(630f, -400f) + RANDOM.getCirclePoint2(50f), 0f));
					*/
					this.spawnPoints.Add(new Vector2(0f, -5400f));
					break;
			}
		}

		public void SetSpawner()
		{
			foreach (Wave wave in this.waves.Keys)
			{
				this.wavesWeight += wave.waveWeight;
			}
			this.GeneratePlacement();
		}
		private Vector2 GetShipSpawnOffset(int id, float radius, float step)
		{
			Vector2 value = SCREEN_MANAGER.AngleToVector(step * (float)(checked(id - 1)));
			return value * radius;
		}

		private void DespawnEnqueuedShip(Ship ship)
		{
			if (ship.cosm != null)
			{
				var allCrew = ship.cosm.crew.Values.ToArray();
				for (int i = 0; i > allCrew.Length; i++)
				{
					Crew crew = allCrew[i];
					crew.kill();
				}
				ship.clearAnims();
				ship.disconnectConsoles();
				ship.spaceJunk = true;
			}
			if (PLAYER.currentWorld != null)
			{
				PLAYER.currentWorld.returnShip(ship);
			}
			ship.removeThis = true;
			Globals.GlobalShipRemoveQueue.Enqueue(new Tuple<ulong, Point>(ship.id, ship.grid));
		}

		private void SpawnWave(Tuple<Wave, List<String>> wave, BattleSession session, Vector2 spawnPos, bool isDefensiveWave)
		{
			if (!Globals.GlobalDespawnQueue.IsEmpty)
			{
				while (Globals.GlobalDespawnQueue.TryDequeue(out var despawning))
				{
					if (PLAYER.currentSession.grid != despawning.Item2)
					{
							var weakdespawnsession = new WeakReference(PLAYER.currentWorld.getSession(despawning.Item2));
							if (weakdespawnsession.IsAlive)
							{
								if ((weakdespawnsession.Target as BattleSession).allShips.TryGetValue(despawning.Item1, out var ship))
								{
									QuewedShipsToRecycle.Enqueue(ship);
								}
							}
							else
							{
								var despawnsession = PLAYER.currentWorld.getSession(despawning.Item2);
								despawnsession.allShips.TryGetValue(despawning.Item1, out var ship);
								QuewedShipsToRecycle.Enqueue(ship);
							}
					}
					else
					{
						PLAYER.currentSession.allShips.TryGetValue(despawning.Item1, out var ship);
						QuewedShipsToRecycle.Enqueue(ship);
						PLAYER.currentSession.allShips.Remove(despawning.Item1);
					}
				}
			}
			spawnPos += this.position;
			var conversations = wave.Item2;
			Vector2 direction = PLAYER.currentShip.position - spawnPos;
			if (wave.Item1.goal == ConsoleGoalType.reach_destination)
			{
				direction = this.interdictionSpot - spawnPos;
			}
			this.SpawnShip(wave.Item1.shipIds[0], spawnPos, direction, session, wave.Item1.goal, ref wave.Item1.loot, isDefensiveWave, conversations);
				checked
				{
					if (wave.Item1.shipIds.Count > 1)
					{
						float step = (float)(6.2831853071795862 / (double)(wave.Item1.shipIds.Count - 1));
						float radius = 200f;
						for (int i = 1; i < wave.Item1.shipIds.Count; i++)
						{
							Vector2 vector = spawnPos + this.GetShipSpawnOffset(i, radius, step);
							this.SpawnShip(wave.Item1.shipIds[i], vector, direction, session, wave.Item1.goal, ref wave.Item1.loot, isDefensiveWave, conversations);
						}
					}
				}
				if (!QuewedShipsToRecycle.IsEmpty)
				{
					while (QuewedShipsToRecycle.TryDequeue(out var ship))
					{
						try
						{
							DespawnEnqueuedShip(ship);
						}
						catch { }
					}
				}


		}

		private async Task DespawnEnqueuedShipAsync(Ship ship)
		{
			if (ship.cosm != null)
			{
				var allCrew = ship.cosm.crew.Values.ToArray();
				for (int i = 0; i > allCrew.Length; i++)
				{
					Crew crew = allCrew[i];
					crew.kill();
				}
				ship.clearAnims();
				ship.disconnectConsoles();
				ship.spaceJunk = true;
			}
			if (PLAYER.currentWorld != null)
			{
				PLAYER.currentWorld.returnShip(ship);
			}
			ship.removeThis = true;
			if (PLAYER.currentSession.grid != ship.grid || PLAYER.currentSession.allShips.ContainsKey(ship.id))
			{
				Globals.GlobalShipRemoveQueue.Enqueue(new Tuple<ulong, Point>(ship.id, ship.grid));
			}
			else
			{
				//ship.Dispose();
			}
		}
		
		private async Task SpawnWaveAsync(Tuple<Wave, List<String>> wave, BattleSession session, Vector2 spawnPos, bool isDefensiveWave)
		{
			if (!Globals.GlobalDespawnQueue.IsEmpty)
			{
				while (Globals.GlobalDespawnQueue.TryDequeue(out var despawning))
				{
					if(PLAYER.currentSession.grid != despawning.Item2)
					{
						await Task.Run(async () => {
							var weakdespawnsession = new WeakReference(PLAYER.currentWorld.getSession(despawning.Item2));
							if (weakdespawnsession.IsAlive)
							{
								if ((weakdespawnsession.Target as BattleSession).allShips.TryGetValue(despawning.Item1, out var ship))
								{
									QuewedShipsToRecycle.Enqueue(ship);
									//(weakdespawnsession.Target as BattleSession).allShips.Remove(ship.id);//run if sync
								}
							}
							else
							{
								var despawnsession = PLAYER.currentWorld.getSession(despawning.Item2);
								despawnsession.allShips.TryGetValue(despawning.Item1, out var ship);
								QuewedShipsToRecycle.Enqueue(ship);
								//despawnsession.allShips.Remove(despawning.Item1);//run if sync
							}
						});
					}
					else
					{
						PLAYER.currentSession.allShips.TryGetValue(despawning.Item1, out var ship);
						QuewedShipsToRecycle.Enqueue(ship);
						//PLAYER.currentSession.allShips.Remove(despawning.Item1);
					}
				}
			}
			await Task.Run(async () => {
				//A new thread to counter freezes on spawning big ships
				spawnPos += this.position;
				var conversations = wave.Item2;
				Vector2 direction = PLAYER.currentShip.position - spawnPos;
				if (wave.Item1.goal == ConsoleGoalType.reach_destination)
				{
					direction = this.interdictionSpot - spawnPos;
				}
				this.SpawnShip(wave.Item1.shipIds[0], spawnPos, direction, session, wave.Item1.goal, ref wave.Item1.loot, isDefensiveWave, conversations);
				checked
				{
					if (wave.Item1.shipIds.Count > 1)
					{
						float step = (float)(6.2831853071795862 / (double)(wave.Item1.shipIds.Count - 1));
						float radius = 200f;
						for (int i = 1; i < wave.Item1.shipIds.Count; i++)
						{
							Vector2 vector = spawnPos + this.GetShipSpawnOffset(i, radius, step);
							this.SpawnShip(wave.Item1.shipIds[i], vector, direction, session, wave.Item1.goal, ref wave.Item1.loot, isDefensiveWave, conversations);
						}
					}
				}
				Parallel.ForEach(QuewedShipsToRecycle, ship =>
				{
					DespawnEnqueuedShipAsync(ship).SafeFireAndForget();
				});
				QuewedShipsToRecycle = new ConcurrentQueue<Ship>();

			});
		}

		/*
		public void Despawn()
		{
			for (int i = 0; i < this.activeShips.Count<Tuple<ulong, List<String>>>(); i++)
			{
				Ship ship;
				try
				{
					var session = PLAYER.currentWorld.getSession(this.grid);
					bool flag2 = session.allShips.TryGetValue(this.activeShips[i].Item1, out ship);
					if (flag2)
					{
						DespawnShipAsync(ship, session).SafeFireAndForget();
						session.allShips.Remove(ship.id);
					}
				}
				catch { }
			}
		}

		private async Task DespawnShipAsync(Ship ship, BattleSession session)
		{
			await Task.Run(() => {
				try { ship.despawn(session); }
				catch { }
			});
		}
		*/

		public Tuple<Wave, List<String>> GetRandomWave()
		{
			float num = (float)RANDOM.getRandomNumber(0.0, (double)this.wavesWeight);
			var key = this.waves.Keys.ToList()[0];
			Tuple<Wave, List<String>> result = new Tuple<Wave, List<String>>(key, this.waves[key]);
			foreach (Wave wave in this.waves.Keys)
			{
				float waveWeight = wave.waveWeight;
				bool flag = num >= waveWeight;
				if (!flag)
				{
					result = new Tuple<Wave, List<String>>(wave, this.waves[wave]);
					break;
				}
				num -= waveWeight;
			}
			return result;
		}
		private void SpawnShip(int id, Vector2 position, Vector2 direction, BattleSession session, ConsoleGoalType goalType, ref List<Tuple<InventoryItemType, int>> loot, bool isDefensiveWave, List<String> conversations)
		{
			if(this.templateUsed == InterruptionType.friendly_pirates_call && id == 33)
			{
				id = Squirrel3RNG.Next(33, 35); // randomize ships for InterruptionType.friendly_pirates_call event
			}

			Ship ship = SHIPBAG.makeEnemy(id);
			ship.id = PLAYER.currentWorld.getUID();
			ship.setFaction((ulong)this.interruptersFaction);
			ship.position = position;
			if (PLAYER.currentShip != null)
			{
				ship.aggro(PLAYER.currentShip.id, session);
			}
			ship.rotationAngle = SCREEN_MANAGER.VectorToAngle(direction);
			if (ship.cosm != null && ship.cosm.crew.Count > 0)
			{
				CrewTeam crewTeam = new CrewTeam();
				crewTeam.ownedShip = ship.id;
				crewTeam.destinationGrid = session.grid;
				crewTeam.destination = position + direction;
				crewTeam.goalType = goalType;
				checked
				{
					foreach (FactionControllerRev2 factionControllerRev in PLAYER.currentWorld.factions)
					{
						bool flag2 = factionControllerRev.faction == ship.faction;
						if (flag2)
						{
							bool flag3 = factionControllerRev.threats != null;
							if (flag3)
							{
								for (int i = 0; i < factionControllerRev.threats.Length; i++)
								{
									crewTeam.threats.Add(factionControllerRev.threats[i]);
								}
							}
							break;
						}
					}
					if (isDefensiveWave && this.templateUsed != InterruptionType.friendly_pirates_call) // special setting for InterruptionType.friendly_pirates_call event
					{
						crewTeam.threats.Add(2UL);
					}
					if (goalType == ConsoleGoalType.warp_jump)
					{
						ship.boostStage = 4;
						ship.velocity = Vector2.Normalize(direction) * (250f * HWCONFIG.GlobalDifficulty);
					}
					bool flag5 = ship.cosm != null && !ship.cosm.crew.IsEmpty;
					if (flag5)
					{
						for (int i = 0; i < ship.cosm.crew.Values.Count; i++)
						{
							Crew crew = ship.cosm.crew.Values.ToArray()[i];
							crew.team = crewTeam;
							crew.faction = (ulong)this.interruptersFaction;
						}
					}
					ship.cosm.rearm = true;
					if (loot != null && loot.Count > 0)
					{
						ship.provision(ref loot);
					}
					else
					{
						if (ship.data != null)
						{
							this.getRandomLoot(ship);
						}
						else
						{
							ship.data = new CosmMetaData();
							ship.data.buildStorage(ship);
							this.getRandomLoot(ship);
						}
					}
				}
				ship.cosm.init();
				//session.addLocalShip(ship, SessionEntry.flyin); sync
				//this.activeShips.Add(new Tuple<ulong, List<String>>(ship.id, conversations)); sync
				//this.activeEffects.Add(new ActiveEffect("WarpIn", position, 3f, 0f)); sync
				shipQueue.Enqueue(new Tuple<Ship, List<String>>(ship, conversations)); //async			
			}
		}

		public void getRandomLoot(Ship ship)
		{
			int quantity = Squirrel3RNG.Next(0, 4);
			//int shipsUnlocked = CHARACTER_DATA.shipsUnlocked();
			InventoryItem Item = null;//new InventoryItem();

			for (int i = 0; i < quantity; i++)
			{
				int selectItem = Squirrel3RNG.Next(1, 26);
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
					// titanium  ingots
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.titanium_ingots);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 4)
				{
					// steel  ingots
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.steel_ingots);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 5)
				{
					// iron  ingots
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.iron_ingots);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 6)
				{
					// Data Core
					int category = Squirrel3RNG.Next(30, 33);
					if (category == 32)
					{
						DataCore dataCore = new DataCore(5);
						dataCore.addCategory(category);
						Item = dataCore;
					}
					else
					{
						DataCore dataCore = new DataCore(2);
						dataCore.addCategory(category);
						Item = dataCore;
					}
				}
				if (selectItem == 7)
				{
					// consumer goods
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.consumer_goods);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 8)
				{
					// assorted parts
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.assorted_parts);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 9)
				{
					// snacks
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.snacks);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 10)
				{
					//structural components
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.structural_components);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 11)
				{
					// thermal plating
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.thermal_plating);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 12)
				{
					//armor plating
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.armor_plating);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 13)
				{
					//electronic components
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.electronic_components);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 14)
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
				if (selectItem == 15)
				{
					//field coils
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.field_coils);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 16)
				{
					//carbon nanotubes
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.carbon_nanotubes);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 17)
				{
					//structural components
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.structural_components);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 18)
				{
					//gold wire
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.gold_wire);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 19)
				{
					//gold wire
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.gold_wire);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 20)
				{
					//titanium alloy
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.titanium_alloy);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 21)
				{
					//graphite
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.graphite);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 22)
				{
					//grey goo
					InventoryItem inventoryItem = new InventoryItem(InventoryItemType.grey_goo);
					inventoryItem.stackSize = 1U + (uint)Squirrel3RNG.Next((int)(ITEMBAG.getStackSize(inventoryItem.type) - 1U));
					Item = inventoryItem;
				}
				if (selectItem == 23)
				{
					//auras
					int rnd = Squirrel3RNG.Next(1, 9);
					if (rnd == 1)
						Item = new BulletEnhancer(checked(1 + Squirrel3RNG.Next(2))); //wussBulletAura
					if (rnd == 2)
						Item = new EmergencyDisplacementOrb((float)(Squirrel3RNG.NextDouble() * 6.0)); //wussDodgeAura
					if (rnd == 3)
						Item = new EmergencyDisplacementOrb((float)(2.0 + Squirrel3RNG.NextDouble() * 6.0));//medDodgeAura
					if (rnd == 4)
						Item = new NanoAura(Squirrel3RNG.Next(6), checked(Squirrel3RNG.Next(7) + 6)); //higherQualityNanoAura
					if (rnd == 5)
						Item = new NanoAura(Squirrel3RNG.Next(6), checked(Squirrel3RNG.Next(4) + 3)); //medQualityNanoAura
					if (rnd == 6)
						Item = new NanoAura(Squirrel3RNG.Next(5), Squirrel3RNG.Next(4));//lowQualityNanoAura
					if (rnd == 7)
						Item = new DoubleShotCapacitor((float)(Squirrel3RNG.NextDouble() * 4.0)); //doubleShotAura
					if (rnd == 8)
						Item = new MovementModAura((float)(Squirrel3RNG.NextDouble() * 4.0));//accelAura
				}
				if (selectItem == 24)
				{
					//crystals

					var data = ship.data;

					bool flag = RANDOM.chance(0.1);

					if (RANDOM.chance(0.1))
					{
						data.addLoot(new InventoryItem(InventoryItemType.crystal_spore));
						break;
					}
					if (RANDOM.chance(0.9))
					{
						switch (Squirrel3RNG.Next(16))
						{
							case 0:
								data.addLoot(new CrystalGene(CrystalType.collector));
								break;
							case 1:
								data.addLoot(new CrystalGene(CrystalType.collector));
								break;
							case 2:
								data.addLoot(new CrystalGene(CrystalType.collector));
								break;
							case 3:
								data.addLoot(new CrystalGene(CrystalType.collector));
								break;
							case 4:
								{
									InventoryItem i1 = new CrystalGene(CrystalType.flower);
									data.addLoot(i1);
									break;
								}
							case 5:
								{
									InventoryItem i2 = new CrystalGene(CrystalType.root);
									data.addLoot(i2);
									break;
								}
							case 6:
								{
									InventoryItem i3 = new CrystalGene(CrystalType.growth);
									data.addLoot(i3);
									break;
								}
							case 7:
								data.addLoot(new CrystalGene(CrystalType.lense));
								break;
							default:
								data.addLoot(new CrystalGene(CrystalType.iron_bulk));
								break;
						}
					}
					else
					{
						switch (Squirrel3RNG.Next(12))
						{
							case 0:
								data.addLoot(new CrystalBranch(2));
								break;
							case 1:
								data.addLoot(new CrystalBranch(2));
								break;
							case 2:
								data.addLoot(new CrystalBranch(3));
								break;
							case 3:
								data.addLoot(new CrystalGene(CrystalType.resonator));
								break;
							case 4:
								data.addLoot(new CrystalGene(CrystalType.resonator));
								break;
							case 5:
								data.addLoot(new CrystalGene(CrystalType.resonator));
								break;
							case 6:
								data.addLoot(new CrystalGene(CrystalType.shell));
								break;
							case 7:
								data.addLoot(new CrystalGene(CrystalType.shell));
								break;
							case 8:
								data.addLoot(new CrystalGene(CrystalType.shell));
								break;
							case 9:
								data.addLoot(new CrystalGene(CrystalType.battery));
								break;
							case 10:
								data.addLoot(new CrystalGene(CrystalType.battery));
								break;
							case 11:
								data.addLoot(new CrystalGene(CrystalType.battery));
								break;
							default:
								data.addLoot(new CrystalGene(CrystalType.resonator));
								break;
						}
					}

					ship.data = data;
				}
				if (selectItem == 25)
				{
					//specialUtility
					int num = Squirrel3RNG.Next(5);
					bool flag = num == 4;
					var data = ship.data;
					if (flag)
					{
						data.addLoot(new BulletEnhancer(Squirrel3RNG.Next(5)));
					}
					else
					{
						bool flag2 = num == 3;
						if (flag2)
						{
							data.addLoot(new DoubleShotCapacitor((float)Squirrel3RNG.Next(5)));
						}
						else
						{
							bool flag3 = num == 2;
							if (flag3)
							{
								data.addLoot(new EmergencyDisplacementOrb((float)Squirrel3RNG.Next(5)));
							}
						}
					}
					ship.data = data;
				}
				if (Item != null)
				{
					ship.data.addLoot(Item);
				}
				/*
				if (selectItem == 1)
				{
					//weapon
					float gunQuality = Squirrel3RNG.Next(Math.Min(30, shipsUnlocked) - 10, 40);
					gunQuality = MathHelper.Clamp(gunQuality, 10f, (float)shipsUnlocked + 10f);
					gunQuality = MathHelper.Clamp(gunQuality, 10f, 39.9f);
					if (gunQuality <= 0f)
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
				if (selectItem == 3)
				{
					// repair gun
					float repairgunQuality = Squirrel3RNG.Next(Math.Min(30, shipsUnlocked) - 10, 40);
					repairgunQuality = MathHelper.Clamp(repairgunQuality, 1f, (float)shipsUnlocked + 10f);
					repairgunQuality = MathHelper.Clamp(repairgunQuality, 1f, 39.9f) / 5;
					Item = new RepairGun(repairgunQuality);
				}
				if (selectItem == 4)
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
					Item = new Digger();
				}
				 */
			}

			/*
			LootTableType type;
			LootFunnel data = new LootFunnel(ship.cosm);
			LOOTBAG.getLoot(quantity, data, LOOTBAG.lootTables[type]);
			*/
		}


		public void returnRoamingShip(Ship ship)
		{
			bool flag = PLAYER.currentWorld != null && PLAYER.currentWorld.factions != null && PLAYER.currentWorld.factions.Count > 0;
			if (flag)
			{
				foreach (FactionControllerRev2 factionControllerRev in PLAYER.currentWorld.factions)
				{
					bool flag2 = factionControllerRev.faction == ship.faction;
					if (flag2)
					{
						factionControllerRev.receiveMiningReturnShip(ship);
						break;
					}
				}
			}
		}

		public void TryRespawn(BattleSession session)
		{
			double num = SCREEN_MANAGER.GameTimeRef.TotalGameTime.TotalSeconds - this.spawnStamp.TotalSeconds;
			bool flag = num > (double)this.timeToReset;
			checked
			{
				if (flag)
				{
					this.assetsSpawned = false;
					this.returnTimer = 0f;
					this.signatureTimer = 0f;
					this.timer = 0f;
					for (int i = 0; i < this.activeShips.Count<Tuple<ulong, List<String>>>(); i++)
					{
						bool flag2 = session.allShips.TryGetValue(this.activeShips[i].Item1, out Ship ship);
						if (flag2)
						{
							session.despawnShip(ship);
						}
					}
					/*
					for (int j = 0; j < this.assetsSlots.Count; j++)
					{
						bool flag3 = this.assetsSlots[j].oreRock != null;
						if (flag3)
						{
							this.assetsSlots[j].oreRock.despawn(session);
						}
					}
					*/
				}
			}
		}

		private List<ulong> getActiveShipIds()
		{
			List<ulong> ids = new List<ulong>();
			for (int i = 0; i < this.activeShips.Count; i++)
			{
				ids.Add(this.activeShips[i].Item1);
			}
			return ids;
		}
		public void TryClearSector(BattleSession session)
		{
			var ids = session.allShips.Keys.ToList();
			for (int i = 0; i < ids.Count; i++)
			{
				Ship ship;
				if (session.allShips.TryGetValue(ids[i], out ship))
				{
					if (session.allShips.Count > 20 && (ship.faction == CONFIG.deadShipFaction || ship.faction == 18446744073709551615UL) && !this.getActiveShipIds().Contains(ids[i]) && !(ship is OreRock))
					{
						bool playerowned = false;
						foreach (var owner in ship.ownershipHistory)
						{
							if (owner == CONFIG.playerFaction)
							{
								playerowned = true;
							}
						}
						if (!playerowned)
							session.despawnShip(ship);
					}
				}
			}

		}

		private void doInterdict(float elapsed)
		{
			this.interdictTimer += elapsed;
			bool flag = this.interdictTimer >= 3f;
			if (flag)
			{
				this.interdictTimer = 0f;
				bool flag2 = this.interdicting;
				bool flag3 = false;
				if (flag2)
				{
					foreach (Ship ship in PLAYER.currentSession.allShips.Values)
					{
						if (PLAYER.currentShip != null && ship.id == PLAYER.currentShip.id)
						{
							ship.Setinterrupted(false);
							flag3 = Vector2.DistanceSquared(this.interdictionSpot, ship.position) < 5000f * 5000f * 0.8f * Math.Max(HWCONFIG.GlobalDifficulty, 0.2f);
							if (flag3)
							{	
								if(HWCONFIG.GlobalDifficulty > 0)
								{
									ship.endBoost();
									ship.Setinterrupted(true);
								}
							}
						}
					}
					if(flag3)
					PARTICLE.systems[10].addParticle(new Vector3(this.interdictionSpot.X, this.interdictionSpot.Y, 0f), 0.38f, 0.3f, 0f, 20000f * 0.8f * Math.Max(HWCONFIG.GlobalDifficulty, 0.2f), 1f, 4, 4);
				}
			}
		}

		private void doEffects(float elapsed)
		{
			List<ActiveEffect> List = new List<ActiveEffect>(this.activeEffects);
			for (int i = 0; i < List.Count; i++)
			{
				string name = List[i].Name;
				Vector2 Pos = List[i].Pos;
				float timer = List[i].Timer;
				float steps = List[i].Steps;
				bool active = this.getEffect(elapsed, name, Pos, ref timer, ref steps);
				if (active)
				{
					List[i].Timer = timer;
					List[i].Steps = steps;
				}
				else
				{
					this.activeEffects.Remove(List[i]);
				}
			}
		}

		private void doConversations(float elapsed, BattleSession session, bool shuffle)
		{
			this.speakTimer += elapsed;
			bool flag = this.speakTimer >= 10f;
			if (flag)
			{
				this.speakTimer = 0f;
				bool flag2 = this.interdicting;
				if (flag2)
				{
					foreach (Tuple<ulong, List<String>> Ship in this.activeShips)
					{
						List<String> messages = new List<String>();
						messages.AddRange(conversations);
						messages.AddRange(Ship.Item2);
						maxmessages = Math.Max(maxmessages, Ship.Item2.Count + conversations.Count);
						if (session.allShips.TryGetValue(Ship.Item1, out Ship ship))
						{
							bool flag3 = Vector2.DistanceSquared(this.interdictionSpot, ship.position) < 10000f * 10000f && Squirrel3RNG.Next(10) <= 3;
							if (flag3)
							{
								if (messages.Count > 0)
								{
									if (shuffle)
									{
										ship.AddChatMessage(messages[Squirrel3RNG.Next(messages.Count - 1)]);
									}
									else
									{
										ship.AddChatMessage(messages[Math.Min(currentmessage, messages.Count - 1)]);
										if (currentmessage < messages.Count)
										{
											currentmessage++;
										}
										else
										{
											if (currentmessage >= maxmessages && currentmessage >= this.activeShips.Count)
											{
												currentmessage = 0;
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

		public void SpawnAssets(BattleSession session)//, Interruption.NodeSlot slot )
		{

			/*AssetsNodeType type = (AssetsNodeType)RANDOM.GetWeightedRandom<AssetsNodeType>(this.assetsNodes, slot.bonusChance);
			OreRock oreRock = ORE_BAG.GetOreRock(type, !CONFIG.rendering);
			bool debugID = CONFIG.debugID;
			if (debugID)
			{
				bool flag = session.allShips.ContainsKey(oreRock.id);
				if (flag)
				{
					throw new Exception("duplicate id");
				}
			}
			session.allShips[oreRock.id] = oreRock;
			session.ore.Add(oreRock);
			oreRock.position = Vector2.Transform(slot.position, this.rotationMatrix);
			oreRock.position += this.position;
			oreRock.rotationAngle = RANDOM.randomRotation();
			oreRock.grid = session.grid;
			oreRock.cosm.init();
			slot.oreRock = oreRock;
			*/

			if (this.interdictionSpot == null || (this.interdictionSpot.X == 0 && this.interdictionSpot.Y == 0))
			{
				this.interdictionSpot = PLAYER.currentShip.position; //Vector2.Transform(slot.position, this.rotationMatrix);
			}
			this.currentWaveInterval = this.waveIntervalAverage + (float)RANDOM.getRandomNumber((double)(-(double)this.waveIntervalAverage * 0.1f), (double)(-(double)this.waveIntervalAverage * 0.1f));
		}
		public void Update(float elapsed, BattleSession session)
		{

			if (!this.shipQueue.IsEmpty)
			{
				while (!this.shipQueue.IsEmpty && this.shipQueue.TryDequeue(out Tuple<Ship, List<String>> ship)) //spawning async instantiated queued ships
				{
					if (ship != null)
					{
						this.activeEffects.Add(new ActiveEffect("WarpIn", ship.Item1.position, 3f, 0f));
						session.addLocalShip(ship.Item1, SessionEntry.flyin);
						this.activeShips.Add(new Tuple<ulong, List<String>>(ship.Item1.id, ship.Item2));
						this.interdicting = true;
					}
				}
			}

			bool flag = session.grid != PLAYER.currentSession.grid;
			if (!flag)
			{
				this.returnTimer += elapsed;
				bool flag2 = this.returnTimer > this.returnDuration;
				if (flag2)
				{
					this.returnTimer = 0f;
					for (int i = 0; i < this.activeShips.Count<Tuple<ulong, List<String>>>(); i++)
					{
						if (session.allShips.TryGetValue(this.activeShips[i].Item1, out Ship ship))
						{
							this.returnRoamingShip(ship);
						}
					}
				}
				this.timer += elapsed;
				bool flag3 = this.timer >= 4f;
				checked
				{
					if (flag3)
					{
						this.timer = 0f;
						for (int j = 0; j < this.activeShips.Count<Tuple<ulong, List<String>>>(); j++)
						{
							if (session.allShips.TryGetValue(this.activeShips[j].Item1, out Ship ship2))
							{
								if (ship2.faction == CONFIG.deadShipFaction)
								{
									this.activeShips.Remove(this.activeShips[j]);
								}								
							}
							else
							{
								this.activeShips.Remove(this.activeShips[j]);
							}
						}
					}
				}

				this.signatureTimer += elapsed;
				bool flag5 = this.signatureTimer >= 10f;
				if (flag5)
				{
					this.accumulatedSignature = 0f;
					this.signatureTimer = 0f;
					bool flag6 = PLAYER.currentShip != null && Vector2.DistanceSquared(PLAYER.currentShip.position, this.position) < 9000f * 9000f;
					if (flag6)
					{
						this.accumulatedSignature += PLAYER.currentShip.signitureRadius;
					}
					else
					{
						this.TryRespawn(session);
					}
				}
				checked
				{
					if (!this.assetsSpawned)
					{

						if (PLAYER.currentShip != null)
						{
							PLAYER.currentShip.endBoost();
						}

						if (this.activeShips.Count() != 0 || this.wavesQueued != 0 || this.initWaveQueued == true)
						{
							this.interdicting = true;
						}

						this.assetsSpawned = true;
						/*
						for (int j = 0; j < this.assetsSlots.Count; j++)
						{
							this.SpawnAssets(this.assetsSlots[j], session);
						}
						*/
						TryClearSector(session);
						this.SpawnAssets(session);

						if (this.initialWave != null && this.initialWave.Item1 != null && this.initWaveQueued == true)
						{
							//this.SpawnWave(this.initialWave, session, Vector2.Zero, true);// false);
							this.SpawnWaveAsync(this.initialWave, session, Vector2.Zero, true).SafeFireAndForget();// false);
							this.initWaveQueued = false;
						}
						this.timeToReset = 3600f;
						this.spawnStamp = SCREEN_MANAGER.GameTimeRef.TotalGameTime;
					}
				}
				if (this.waves != null && this.waves.Count != 0 && this.wavesQueued > 0 && this.spawnPoints.Count<Vector2>() > 0 && (this.accumulatedSignature > 0f /*|| PLAYER.avatar.currentCosm.isStation*/) && this.currentWave < this.maxWaves)
				{
					if (this.activeShips.Count<Tuple<ulong, List<String>>>() < 2 || (this.initialWave != null && this.initialWave.Item1 != null)) // enqueue reinforcements if less then 2 ships remain or there was an initial Wave to spawn.
					{
						this.waveTimer += elapsed;
						float num = this.accumulatedSignature / this.currentWaveInterval;
						if (this.waveTimer >= this.currentWaveInterval)
						{
							this.waveTimer = 0f;
							this.accumulatedSignature = 0f;
							checked
							{
								this.currentWave++;
								this.toSpawn = this.GetRandomWave();
								Vector2 spawnPos = Vector2.Transform(this.spawnPoints[RANDOM.getRandomNumber(this.spawnPoints.Count<Vector2>())], this.rotationMatrix);
								//this.SpawnWave(this.toSpawn, session, spawnPos, true);
								this.SpawnWaveAsync(this.toSpawn, session, spawnPos, true).SafeFireAndForget();
								this.wavesQueued -= 1;
								this.rotation = RANDOM.randomRotation();
								this.rotationMatrix = Matrix.CreateRotationZ(this.rotation);
							}
							this.currentWaveInterval = this.waveIntervalAverage + (float)RANDOM.getRandomNumber((double)(-(double)this.waveIntervalAverage * 0.1f), (double)(-(double)this.waveIntervalAverage * 0.1f));
						}
					}
				}
				if (this.assetsSpawned)
				{
					this.doEffects(elapsed);
					this.doConversations(elapsed, session, shuffle);
				}
				this.doInterdict(elapsed);

				EVENTS.playerDeath += this.watchForDeath;

				if (this.activeShips.Count() == 0 && (this.wavesQueued == 0 || this.currentWave >= this.maxWaves) && this.initWaveQueued == false)
					this.interdicting = false;

				// InterruptionType.friendly_pirates_call event update
				if (this.templateUsed == InterruptionType.friendly_pirates_call)
				{ 
					HWFriendlyPiratesCalledEvent.interruptionUpdate(elapsed, session, this);
				}

				// InterruptionType.home_siege_pirate event update
				if (this.templateUsed == InterruptionType.home_siege_pirate_t1 || this.templateUsed == InterruptionType.home_siege_pirate_t2 || this.templateUsed == InterruptionType.home_siege_pirate_t25)
				{
					HWBaseSiegeEvent.interruptionUpdate(elapsed, session, this);
				}
			}
		}

		public void watchForDeath(object sender, EventArgs e)
		{
			EVENTS.playerDeath -= this.watchForDeath;
			if(Globals.globalints[GlobalInt.Bounty] > 0 && this.activeShips.Count > 0)
			{
				Globals.globalints[GlobalInt.Bounty] = 0;
				SCREEN_MANAGER.widgetChat.AddMessage("Enemy found your corpse and collected your bounty on the black market.", MessageTarget.Ship);
			}
		}
		/*
		public class NodeSlot
		{
			// Token: 0x060008C4 RID: 2244 RVA: 0x000AE1C9 File Offset: 0x000AC3C9
			public NodeSlot(Vector2 pos, float chance)
			{
				this.position = pos;
				this.bonusChance = chance;
			}

			// Token: 0x04001447 RID: 5191
			public Vector2 position;

			// Token: 0x04001448 RID: 5192
			public float bonusChance;

			// Token: 0x04001449 RID: 5193
			//public OreRock oreRock; // Asteroid/Station
		}
		*/

	}

}
