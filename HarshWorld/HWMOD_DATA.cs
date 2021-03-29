using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Data.SQLite;
using System.Data;
using System.Collections.Concurrent;
using CoOpSpRpG;

namespace HarshWorld
{
	public class MOD_DATA
	{
		public static SQLiteConnection modCon;
		public static bool loaded = false;

		public static void createAllTables()
		{
			string commandText = "create table IF NOT EXISTS interruptions (id varchar(20), gridx INT, gridy INT, positionx INT, positiony INT, template varchar(20), shuffle BOOL, interdicting BOOL, initwavequeued BOOL, wavesqueued INT, maxwaves INT, currentwave INT, PRIMARY KEY (id))";
			SQLiteCommand sqliteCommand = new SQLiteCommand(commandText, MOD_DATA.modCon);
			sqliteCommand.ExecuteNonQuery();
			commandText = "create table IF NOT EXISTS conversations (id varchar(20), idx INT, text varchar(20), PRIMARY KEY (id, idx))";
			sqliteCommand = new SQLiteCommand(commandText, MOD_DATA.modCon);
			sqliteCommand.ExecuteNonQuery();
			commandText = "create table IF NOT EXISTS activeships (id varchar(20), UID BIGINT, PRIMARY KEY (id, UID))";
			sqliteCommand = new SQLiteCommand(commandText, MOD_DATA.modCon);
			sqliteCommand.ExecuteNonQuery();
			commandText = "create table IF NOT EXISTS activeshipslines (id varchar(20), UID BIGINT, idx INT, text varchar(20), PRIMARY KEY (id, UID, idx))";
			sqliteCommand = new SQLiteCommand(commandText, MOD_DATA.modCon);
			sqliteCommand.ExecuteNonQuery();
			commandText = "create table IF NOT EXISTS globalshipremovequeue (UID BIGINT, gridx INT, gridy INT, PRIMARY KEY (UID))";
			sqliteCommand = new SQLiteCommand(commandText, MOD_DATA.modCon);
			sqliteCommand.ExecuteNonQuery();
			commandText = "create table IF NOT EXISTS eventflags (name varchar(20), value BOOL, PRIMARY KEY (name))";
			sqliteCommand = new SQLiteCommand(commandText, MOD_DATA.modCon);
			sqliteCommand.ExecuteNonQuery();
		}

		public static void writeModData()
		{
			try
			{/*
				if (MOD_DATA.modCon != null) // saving PiratesCalled flag
				{
					string commandText1 = "delete from eventflags";
					SQLiteCommand sqliteCommand1 = new SQLiteCommand(commandText1, MOD_DATA.modCon);
					sqliteCommand1.ExecuteNonQuery();
					string commandText2 = "insert or replace into eventflags (piratescalled, piratescalledhostile) values (@piratescalled, @piratescalledhostile)";
					SQLiteCommand sqliteCommand2 = new SQLiteCommand(commandText2, MOD_DATA.modCon);
					sqliteCommand2.Parameters.Add("@piratescalled", DbType.Boolean).Value = Globals.PiratesCalled;
					sqliteCommand2.Parameters.Add("@piratescalledhostile", DbType.Boolean).Value = Globals.PiratesCalledHostile;
					sqliteCommand2.ExecuteNonQuery();	
				}
				*/
				if (MOD_DATA.modCon != null) // saving flags
				{
					string commandText1 = "delete from eventflags";
					SQLiteCommand sqliteCommand1 = new SQLiteCommand(commandText1, MOD_DATA.modCon);
					sqliteCommand1.ExecuteNonQuery();
					foreach (var flag in Globals.eventflags)
					{
					string commandText2 = "insert or replace into eventflags (name, value) values (@name, @value)";
					SQLiteCommand sqliteCommand2 = new SQLiteCommand(commandText2, MOD_DATA.modCon);
					sqliteCommand2.Parameters.Add("@name", DbType.String).Value = flag.Key;
					sqliteCommand2.Parameters.Add("@value", DbType.Boolean).Value = flag.Value;
					sqliteCommand2.ExecuteNonQuery();
					}
				}
				if (Globals.GlobalShipRemoveQueue != null && MOD_DATA.modCon != null) // saving recycle shipIds queue
				{
					string commandText = "delete from globalshipremovequeue";
					SQLiteCommand sqliteCommand = new SQLiteCommand(commandText, MOD_DATA.modCon);
					sqliteCommand.ExecuteNonQuery();
					if (!Globals.GlobalShipRemoveQueue.IsEmpty)
					{ 
						foreach (var ShipIdTuple in Globals.GlobalShipRemoveQueue)
						{
							string commandText2 = "insert or replace into globalshipremovequeue (UID, gridx, gridy) values (@UID, @gridx, @gridy)";
							SQLiteCommand sqliteCommand2 = new SQLiteCommand(commandText2, MOD_DATA.modCon);
							sqliteCommand2.Parameters.Add("@UID", DbType.Int64, 64).Value = ShipIdTuple.Item1;
							sqliteCommand2.Parameters.Add("@gridx", DbType.Int32, 32).Value = ShipIdTuple.Item2.X;
							sqliteCommand2.Parameters.Add("@gridy", DbType.Int32, 32).Value = ShipIdTuple.Item2.Y;
							sqliteCommand2.ExecuteNonQuery();
						}
					}
				}
				if (Globals.Interruptions != null && MOD_DATA.modCon != null)
				{
					for (int i = 0; i < Globals.Interruptions.Count(); i++)
					{
						var basicinterruption = Globals.Interruptions[i];
						Interruption interruption = null;
						if (basicinterruption.InterruptionId != null && Globals.Interruptionbag.TryGetValue(basicinterruption.InterruptionId, out interruption) && interruption != null)
						{
							deleteInterruptionsData(interruption.id);
							string commandText = "insert or replace into interruptions (id, gridx, gridy, positionx, positiony, template, shuffle, interdicting, initwavequeued, wavesqueued, maxwaves, currentwave) values (@id, @gridx, @gridy, @positionx, @positiony, @template, @shuffle, @interdicting, @initwavequeued, @wavesqueued, @maxwaves, @currentwave)";
							SQLiteCommand sqliteCommand = new SQLiteCommand(commandText, MOD_DATA.modCon);
							sqliteCommand.Parameters.Add("@id", DbType.String).Value = interruption.id;
							sqliteCommand.Parameters.Add("@gridx", DbType.Int32, 32).Value = interruption.grid.X;
							sqliteCommand.Parameters.Add("@gridy", DbType.Int32, 32).Value = interruption.grid.Y;
							checked
							{
								sqliteCommand.Parameters.Add("@positionx", DbType.Int32, 32).Value = (int)interruption.position.X;
								sqliteCommand.Parameters.Add("@positiony", DbType.Int32, 32).Value = (int)interruption.position.Y;
								sqliteCommand.Parameters.Add("@template", DbType.String).Value = interruption.templateUsed;
								sqliteCommand.Parameters.Add("@shuffle", DbType.Boolean).Value = interruption.shuffle;
								sqliteCommand.Parameters.Add("@interdicting", DbType.Boolean).Value = interruption.interdicting;
								sqliteCommand.Parameters.Add("@initwavequeued", DbType.Boolean).Value = interruption.initWaveQueued;
								sqliteCommand.Parameters.Add("@wavesqueued", DbType.Int32, 32).Value = interruption.wavesQueued;
								sqliteCommand.Parameters.Add("@maxwaves", DbType.Int32, 32).Value = interruption.maxWaves;
								sqliteCommand.Parameters.Add("@currentwave", DbType.Int32, 32).Value = interruption.currentWave;
								sqliteCommand.ExecuteNonQuery();
							}
							for (int l = 0; l < interruption.conversations.Count; l++)
							{
								var line = interruption.conversations[l];
								string commandText2 = "insert or replace into conversations (id, idx, text) values (@id, @idx, @text)";
								SQLiteCommand sqliteCommand2 = new SQLiteCommand(commandText2, MOD_DATA.modCon);
								sqliteCommand2.Parameters.Add("@id", DbType.String).Value = interruption.id;
								sqliteCommand2.Parameters.Add("@idx", DbType.Int32, 32).Value = l;
								sqliteCommand2.Parameters.Add("@text", DbType.String).Value = line;
								sqliteCommand2.ExecuteNonQuery();
							}

							for (int s = 0; s < interruption.activeShips.Count; s++)
							{
								var ship = interruption.activeShips[s];
								string commandText3 = "insert or replace into activeships (id, UID) values (@id, @UID)";
								SQLiteCommand sqliteCommand3 = new SQLiteCommand(commandText3, MOD_DATA.modCon);
								sqliteCommand3.Parameters.Add("@id", DbType.String).Value = interruption.id;
								sqliteCommand3.Parameters.Add("@UID", DbType.Int64, 64).Value = ship.Item1;
								sqliteCommand3.ExecuteNonQuery();

								for (int sl = 0; sl < ship.Item2.Count; sl++)
								{
									var shipline = ship.Item2[sl];
									string commandText4 = "insert or replace into activeshipslines (id, UID, idx, text) values (@id, @UID, @idx, @text)";
									SQLiteCommand sqliteCommand4 = new SQLiteCommand(commandText4, MOD_DATA.modCon);
									sqliteCommand4.Parameters.Add("@id", DbType.String).Value = interruption.id;
									sqliteCommand4.Parameters.Add("@UID", DbType.Int64, 64).Value = ship.Item1;
									sqliteCommand4.Parameters.Add("@idx", DbType.Int32, 32).Value = sl;
									sqliteCommand4.Parameters.Add("@text", DbType.String).Value = shipline;
									sqliteCommand4.ExecuteNonQuery();
								}
							}
						}
					}
				}
			}
			catch
			{
			}
		}

		public static void loadModData()
		{
			getGlobalFlagsData();
			getGlobalShipRemoveQueueData();
			string commandText = "select * from interruptions";
			SQLiteCommand sqliteCommand = new SQLiteCommand(commandText, MOD_DATA.modCon);
			SQLiteDataReader sqliteDataReader = sqliteCommand.ExecuteReader();
			while (sqliteDataReader.Read())
			{
				Point point = default(Point);
				point.X = (int)sqliteDataReader["gridx"];
				point.Y = (int)sqliteDataReader["gridy"];
				Vector2 vector = new Vector2
				{
					X = (float)((int)sqliteDataReader["positionx"]),
					Y = (float)((int)sqliteDataReader["positiony"])
				};
				string template = sqliteDataReader["template"].ToString();
				if (!Enum.TryParse(template, true, out InterruptionType type))
				{
					type = InterruptionType.ambush_starter;
				}
				Interruption newInterruption = new Interruption(type, vector, point, getConversationData(sqliteDataReader["id"].ToString()), (bool)sqliteDataReader["shuffle"], (bool)sqliteDataReader["interdicting"], sqliteDataReader["id"].ToString(), (bool)sqliteDataReader["initwavequeued"], (int)sqliteDataReader["wavesqueued"]);
				HWSPAWNMANAGER.addInterruption(newInterruption);
				newInterruption.activeShips = getActiveShips(sqliteDataReader["id"].ToString());
				try{ 
				newInterruption.maxWaves = (int)sqliteDataReader["maxwaves"];
				newInterruption.currentWave = (int)sqliteDataReader["currentwave"];
				}
				catch
				{ }
			}
			loaded = true;
		}

		public static void deleteInterruptionsData(string id)
		{
			string commandText = "delete from interruptions where id = @id";
			SQLiteCommand sqliteCommand = new SQLiteCommand(commandText, MOD_DATA.modCon);
			sqliteCommand.Parameters.Add("@id", DbType.String, 32).Value = id;
			sqliteCommand.ExecuteNonQuery();
			string commandText2 = "delete from conversations where id = @id";
			SQLiteCommand sqliteCommand2 = new SQLiteCommand(commandText2, MOD_DATA.modCon);
			sqliteCommand2.Parameters.Add("@id", DbType.String, 32).Value = id;
			sqliteCommand2.ExecuteNonQuery();
			string commandText3 = "delete from activeships where id = @id";
			SQLiteCommand sqliteCommand3 = new SQLiteCommand(commandText3, MOD_DATA.modCon);
			sqliteCommand3.Parameters.Add("@id", DbType.String, 32).Value = id;
			sqliteCommand3.ExecuteNonQuery();
			string commandText4 = "delete from activeshipslines where id = @id";
			SQLiteCommand sqliteCommand4 = new SQLiteCommand(commandText4, MOD_DATA.modCon);
			sqliteCommand4.Parameters.Add("@id", DbType.String, 32).Value = id;
			sqliteCommand4.ExecuteNonQuery();
		}

		public static void deleteAllActiveShips()
		{
			string commandText1 = "delete from activeships";
			SQLiteCommand sqliteCommand1 = new SQLiteCommand(commandText1, MOD_DATA.modCon);
			sqliteCommand1.ExecuteNonQuery();
			string commandText2 = "delete from activeshipslines";
			SQLiteCommand sqliteCommand2 = new SQLiteCommand(commandText2, MOD_DATA.modCon);
			sqliteCommand2.ExecuteNonQuery();
		}

		public static void deleteAllInterruptions()
		{
			string commandText = "delete from interruptions";
			SQLiteCommand sqliteCommand = new SQLiteCommand(commandText, MOD_DATA.modCon);
			sqliteCommand.ExecuteNonQuery();
			string commandText2 = "delete from conversations";
			SQLiteCommand sqliteCommand2 = new SQLiteCommand(commandText2, MOD_DATA.modCon);
			sqliteCommand2.ExecuteNonQuery();
			string commandText3 = "delete from activeships";
			SQLiteCommand sqliteCommand3 = new SQLiteCommand(commandText3, MOD_DATA.modCon);
			sqliteCommand3.ExecuteNonQuery();
			string commandText4 = "delete from activeshipslines";
			SQLiteCommand sqliteCommand4 = new SQLiteCommand(commandText4, MOD_DATA.modCon);
			sqliteCommand4.ExecuteNonQuery();
		}

		private static void getGlobalShipRemoveQueueData()
		{
			string commandText = "select * from globalshipremovequeue";
			SQLiteCommand sqliteCommand = new SQLiteCommand(commandText, MOD_DATA.modCon);
			SQLiteDataReader sqliteDataReader = sqliteCommand.ExecuteReader();
			while (sqliteDataReader.Read())
			{
				var uid = (ulong)(long)sqliteDataReader["UID"];
				Point point = default(Point);
				point.X = (int)sqliteDataReader["gridx"];
				point.Y = (int)sqliteDataReader["gridy"];
				Globals.GlobalShipRemoveQueue.Enqueue(new Tuple<ulong, Point>(uid, point));
			}
		}
		private static void getGlobalFlagsData()
		{
			string commandText = "select * from eventflags";
			SQLiteCommand sqliteCommand = new SQLiteCommand(commandText, MOD_DATA.modCon);
			SQLiteDataReader sqliteDataReader = sqliteCommand.ExecuteReader();
			while (sqliteDataReader.Read())
			{
				string template = "unknown";
				try
				{ 
					template = sqliteDataReader["name"].ToString();
				}
				catch
				{
					SCREEN_MANAGER.alerts.Enqueue("Failed to load flags from savefile for HarshWorld mod. Please tell the mod author."); //error message for debug
					return;
				}
				if (!Enum.TryParse(template, true, out GlobalFlag type))
				{
					SCREEN_MANAGER.alerts.Enqueue("Failed to load " + template + " flag for HarshWorld mod. Please tell the mod author."); //error message for debug
					return;
				}
				Globals.eventflags[type] = (bool)sqliteDataReader["value"];
			}
		}
		private static List<String> getConversationData(string id)
		{
			List<String> list = new List<String>();
			string commandText = "select idx, text from conversations where id = @id";
			SQLiteCommand sqliteCommand = new SQLiteCommand(commandText, MOD_DATA.modCon);
			sqliteCommand.Parameters.Add("@id", DbType.String, 32).Value = id;
			SQLiteDataReader sqliteDataReader = sqliteCommand.ExecuteReader();
			while (sqliteDataReader.Read())
			{
				list.Add(sqliteDataReader["text"].ToString());
			}
			return list;
		}

		private static List<String> getShipConversationData(string id, ulong UID)
		{
			List<String> list = new List<String>();
			string commandText = "select idx, text from activeshipslines where id = @id and UID = @UID";
			SQLiteCommand sqliteCommand = new SQLiteCommand(commandText, MOD_DATA.modCon);
			sqliteCommand.Parameters.Add("@id", DbType.String, 32).Value = id;
			sqliteCommand.Parameters.Add("@UID", DbType.Int64, 64).Value = UID;
			SQLiteDataReader sqliteDataReader = sqliteCommand.ExecuteReader();
			while (sqliteDataReader.Read())
			{
				list.Add(sqliteDataReader["text"].ToString());
			}
			return list;
		}


		private static List<Tuple<ulong, List<string>>> getActiveShips(string id)
		{
			List<Tuple<ulong, List<string>>> list = new List<Tuple<ulong, List<string>>>();
			string commandText = "select * from activeships where id = @id";
			SQLiteCommand sqliteCommand = new SQLiteCommand(commandText, MOD_DATA.modCon);
			sqliteCommand.Parameters.Add("@id", DbType.String, 32).Value = id;
			SQLiteDataReader sqliteDataReader = sqliteCommand.ExecuteReader();
			while (sqliteDataReader.Read())
			{
				var uid = (ulong)(long)sqliteDataReader["UID"];
				list.Add(new Tuple<ulong, List<string>>(uid, getShipConversationData(id, uid)));
			}
			return list;
		}

		public static List<Tuple<ulong, Point>> getAllActiveShips()
		{
			List<Tuple<ulong, Point>> list = new List<Tuple<ulong, Point>>();
			string commandText = "select activeships.UID, interruptions.gridx, interruptions.gridy from activeships inner join interruptions on activeships.id = interruptions.id";
			SQLiteCommand sqliteCommand = new SQLiteCommand(commandText, MOD_DATA.modCon);
			SQLiteDataReader sqliteDataReader = sqliteCommand.ExecuteReader();
			while (sqliteDataReader.Read())
			{
				var uid = (ulong)(long)sqliteDataReader["UID"];
				Point point = default(Point);
				point.X = (int)sqliteDataReader["gridx"];
				point.Y = (int)sqliteDataReader["gridy"];
				list.Add(new Tuple<ulong, Point>(uid, point));
			}
			return list;
		}
	}
}
