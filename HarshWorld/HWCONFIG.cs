using System;
using System.Collections.Generic;
using System.IO;

namespace HarshWorld
{	
    class HWCONFIG
    {
		private static string ConfigFilePath = "";
		private static Dictionary<string, HWCONFIG.VariableReference> exportDictionary;

		public static int MaxInterruptions = 2;
		public static float InterruptionFrequency = 1.0f;
		public static float GlobalDifficulty = 1.0f;

		public sealed class VariableReference
		{
			public Func<object> Get { get; private set; }

			public Action<object> Set { get; private set; }

			public VariableReference(Func<object> getter, Action<object> setter)
			{
				this.Get = getter;
				this.Set = setter;
			}
		}

		public static void Init(string path)
		{
			HWCONFIG.ConfigFilePath = path;
			HWCONFIG.exportDictionary = new Dictionary<string, HWCONFIG.VariableReference>();
			HWCONFIG.SaveVar(HWCONFIG.exportDictionary, "Max Active Encounters", () => HWCONFIG.MaxInterruptions, delegate (object v)
			{
				HWCONFIG.MaxInterruptions = (int)v;
			});
			HWCONFIG.SaveVar(HWCONFIG.exportDictionary, "Encounter Frequency Multiplier", () => HWCONFIG.InterruptionFrequency, delegate (object v)
			{
				HWCONFIG.InterruptionFrequency = (float)v;
			});
			HWCONFIG.SaveVar(HWCONFIG.exportDictionary, "Global Difficulty Multiplier", () => HWCONFIG.GlobalDifficulty, delegate (object v)
			{
				HWCONFIG.GlobalDifficulty = (float)v;
			});
		}

		public static void SaveVar(Dictionary<string, HWCONFIG.VariableReference> dic, string key, Func<object> getter, Action<object> setter)
		{
			dic.Add(key, new HWCONFIG.VariableReference(getter, setter));
		}

		private static void setDefaults()
		{
			HWCONFIG.MaxInterruptions = 2;
			HWCONFIG.InterruptionFrequency = 1.0f;
			HWCONFIG.GlobalDifficulty = 1.0f;
		}

		public static void LoadConfig()
        {
            string path = HWCONFIG.ConfigFilePath;
            bool flag = File.Exists(path);
            if (flag)
            {
                HWCONFIG.Load();
            }
            else
            {
                HWCONFIG.setDefaults();
            }
        }

		public static void ChangeVar(Dictionary<string, HWCONFIG.VariableReference> dic, string key, string value)
		{
			bool flag = dic[key].Get() is int;
			if (flag)
			{
				dic[key].Set(Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture));
			}
			else
			{
				bool flag2 = dic[key].Get() is bool;
				if (flag2)
				{
					dic[key].Set(Convert.ToBoolean(value, System.Globalization.CultureInfo.InvariantCulture));
				}
				else
				{
					bool flag3 = dic[key].Get() is float;
					if (flag3)
					{
						dic[key].Set(Convert.ToSingle(value, System.Globalization.CultureInfo.InvariantCulture));
					}
				}
			}
			bool flag4 = dic[key].Get() is Enum;
			if (flag4)
			{
				Type type = HWCONFIG.exportDictionary[key].Get().GetType();
				Enum @enum = (Enum)HWCONFIG.exportDictionary[key].Get();
				Type underlyingType = Enum.GetUnderlyingType(type);
				dic[key].Set(Convert.ChangeType(value, underlyingType, System.Globalization.CultureInfo.InvariantCulture));
			}
		}

		public static void Load()
		{
			string path = HWCONFIG.ConfigFilePath;
			bool flag = !File.Exists(path);
			if (flag)
			{
			}
			using (StreamReader streamReader = new StreamReader(path))
			{ 
				string text = streamReader.ReadToEnd();
				streamReader.Close();
				string text2 = "";
				string text3 = "";
				string[] array = text.Split(new string[]
				{
					"\r\n",
					"\r",
					"\n"
				}, StringSplitOptions.None);
				checked
				{
					for (int i = 0; i < array.Length; i++)
					{
						bool flag2 = false;
						for (int j = 0; j < array[i].Length; j++)
						{
							bool flag3 = !flag2;
							if (flag3)
							{
								bool flag4 = array[i][j] != ':';
								if (flag4)
								{
									text2 += array[i][j].ToString();
								}
								else
								{
									flag2 = true;
								}
							}
							else
							{
								text3 += array[i][j].ToString();
							}
						}
						bool flag5 = text2 != "" && text3 != "";
						if (flag5)
						{
							try
							{
								HWCONFIG.ChangeVar(HWCONFIG.exportDictionary, text2, text3);
							}
							catch
							{
								HWCONFIG.setDefaults();
							}
						}
						text2 = "";
						text3 = "";
					}
				}
			}
		}

	}
}
