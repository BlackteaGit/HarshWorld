using CoOpSpRpG;
using System;
using System.Collections.Generic;
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
	}
}
