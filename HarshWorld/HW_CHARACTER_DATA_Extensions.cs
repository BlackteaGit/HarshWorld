using CoOpSpRpG;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HarshWorld
{
   public static class HW_CHARACTER_DATA_Extensions
    {
		public static int mostExpensiveDesign()
		{
			int cost = 0;
			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
			SQLiteConnection dBCon = typeof(CHARACTER_DATA).GetField("dBCon", flags).GetValue(null) as SQLiteConnection; //using reflections to access the static field dBCon from the class CHARACTER_DATA
			SQLiteDataReader sqliteDataReader = new SQLiteCommand("select * from designs where name = '" + CHARACTER_DATA.selected + "'", dBCon).ExecuteReader();
			while (sqliteDataReader.Read())
			{
			  cost = Math.Max(cost, (int)sqliteDataReader["cost"]);
			}
			return cost;
		}

		public static List<Tuple<string, int>> storedDesignsWithCost(string md5)
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
			List<Tuple<string, int>> list = new List<Tuple<string, int>>();
			SQLiteConnection dBCon = typeof(CHARACTER_DATA).GetField("dBCon", flags).GetValue(null) as SQLiteConnection; //using reflections to access the static field dBCon from the class CHARACTER_DATA
			SQLiteCommand sqliteCommand = new SQLiteCommand("select design, cost from designs where name = @name and checksum = @check", dBCon);
			sqliteCommand.Parameters.Add("@name", DbType.String).Value = CHARACTER_DATA.selected;
			sqliteCommand.Parameters.Add("@check", DbType.String).Value = md5;
			SQLiteDataReader sqliteDataReader = sqliteCommand.ExecuteReader();
			while (sqliteDataReader.Read())
			{
				list.Add(new Tuple<string, int>(sqliteDataReader["design"].ToString(), (int)sqliteDataReader["cost"]));
			}
			/*
			list.Sort(delegate (string a, string b)
			{
				if (a == "default")
				{
					return -1;
				}
				if (b == "default")
				{
					return 1;
				}
				if (a.ToLower().StartsWith("variant") && b.ToLower().StartsWith("variant"))
				{
					return a.Substring(7).CompareTo(b.Substring(7));
				}
				if (a.ToLower().StartsWith("variant"))
				{
					return -1;
				}
				if (b.ToLower().StartsWith("variant"))
				{
					return 1;
				}
				return 0;
			});
			*/
			return list;
		}

	}
}
