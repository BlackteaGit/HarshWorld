using CoOpSpRpG;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace HarshWorld
{
    class HWSPAWNMANAGER
    {
		private static ConcurrentQueue<Ship> QuewedShipsToRecycle = new ConcurrentQueue<Ship>();
		public static void addInterruption(Interruption interruption)
		{
			checked
			{
				if (Globals.Interruptions.Count() > 0)
				{ 
					int last = Globals.Interruptions.Count() - 1;
					/*
					if (Globals.Interruptions[last] != null)
					{
						Globals.Interruptions[last].Despawn();
					}
					//Globals.DespawnInterruptionAsync(lastinterruption).SafeFireAndForget();
					*/

					if (Globals.Interruptions[last].InterruptionId != null)
					{
						Globals.Interruptionbag.TryGetValue(Globals.Interruptions[last].InterruptionId, out Interruption lastinterruption);
						if (lastinterruption != null)
						{
							for (int i = 0; i < lastinterruption.activeShips.Count; i++)
							{
								Globals.GlobalDespawnQueue.Enqueue(new Tuple<ulong, Point>(lastinterruption.activeShips[i].Item1, lastinterruption.grid));
							}
							//lastinterruption.Despawn();
							MOD_DATA.deleteInterruptionsData(lastinterruption.id);
							Globals.Interruptionbag[lastinterruption.id] = null;
							Globals.Interruptionbag.Remove(lastinterruption.id);
						}
					}
					for (int i = last; i > 0; i--)
					{
						Globals.Interruptions[i] = Globals.Interruptions[i - 1];
					}
					Globals.Interruptions[0] = new InterruptionBasic(interruption.id, interruption.grid, interruption.position);
					Globals.Interruptionbag.Add(interruption.id, interruption);
				}
			}
		}

		public static async Task DespawnInterruptionAsync(string InterruptionId)
		{
			checked
			{
				for (int i = 0; i < Globals.Interruptions.Length; i++)
				{
					if (Globals.Interruptions[i].InterruptionId == InterruptionId)
					{
						Globals.Interruptionbag.TryGetValue(Globals.Interruptions[i].InterruptionId, out Interruption toremove);
						if (toremove != null)
						{
							for (int j = 0; j < toremove.activeShips.Count; j++)
							{
								Globals.GlobalDespawnQueue.Enqueue(new Tuple<ulong, Point>(toremove.activeShips[j].Item1, toremove.grid));
							}
							MOD_DATA.deleteInterruptionsData(toremove.id);
							Globals.Interruptionbag[toremove.id] = null;
							Globals.Interruptionbag.Remove(toremove.id);
							Globals.Interruptions[i] = new InterruptionBasic();
							if (!Globals.GlobalDespawnQueue.IsEmpty)
							{
								while (Globals.GlobalDespawnQueue.TryDequeue(out var despawning))
								{
									if (PLAYER.currentSession.grid != despawning.Item2)
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
										PLAYER.currentSession.allShips.Remove(despawning.Item1);
									}
								}
							}
						}
					}
				}
			}
		}

		public static void TestDespawnSavedShips()
		{
			if (PLAYER.currentWorld != null && PLAYER.currentSession != null && HWCONFIG.MaxInterruptions < 0)
			{
				var ships = MOD_DATA.getAllActiveShips();
				for (int i = 0; i < ships.Count; i++)
				{
					Globals.GlobalDespawnQueue.Enqueue(new Tuple<ulong, Point>(ships[i].Item1, ships[i].Item2));
				}
				HWSPAWNMANAGER.TryDespawnAll().SafeFireAndForget();
				MOD_DATA.deleteAllActiveShips();
				if (HWCONFIG.MaxInterruptions < -1)
					MOD_DATA.deleteAllInterruptions();
			}
		}

		public static async Task TryDespawnAll()
		{
			if (!Globals.GlobalDespawnQueue.IsEmpty)
			{
				while (Globals.GlobalDespawnQueue.TryDequeue(out var despawning))
				{
					var despawnsession = PLAYER.currentWorld.getSession(despawning.Item2);
					if (despawnsession.allShips.TryGetValue(despawning.Item1, out Ship ship))
					{
						despawnsession.allShips.Remove(ship.id);
						QuewedShipsToRecycle.Enqueue(ship);
					}
				}
			}
			await Task.Run(async () => {
				if (!QuewedShipsToRecycle.IsEmpty)
				{
					while (QuewedShipsToRecycle.TryDequeue(out var ship))
					{
						try
						{
							await DespawnEnqueuedShipAsync(ship);
						}
						catch { }
					}
				}
			});
		}

		private static async Task DespawnEnqueuedShipAsync(Ship ship)
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
		}

	}
}
