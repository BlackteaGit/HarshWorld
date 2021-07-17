using CoOpSpRpG;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace HarshWorld
{
   public static class ShipExtensions
   {
		static readonly ConditionalWeakTable<Ship, InterruptedObject> interrupted = new ConditionalWeakTable<Ship, InterruptedObject>(); // "hack" to add interrupted field to Ship class using extension methods. Access with crew.Getinterrupted(), crew.Setinterrupted()
		public static bool Getinterrupted(this Ship ship) { return interrupted.GetOrCreateValue(ship).Value; }

		public static void Setinterrupted(this Ship ship, bool newinterrupted) { interrupted.GetOrCreateValue(ship).Value = newinterrupted; }

		class InterruptedObject
		{
			public bool Value = new bool();
		}

		

		static readonly ConditionalWeakTable<Ship, ConvoyObject> convoy = new ConditionalWeakTable<Ship, ConvoyObject>();

		public static Dictionary<ulong, Point> GetConvoy(this Ship ship) { return convoy.GetOrCreateValue(ship).Value; }

		public static void SetConvoy(this Ship ship, Dictionary<ulong, Point> newconvoy) { convoy.GetOrCreateValue(ship).Value = newconvoy; }

		class ConvoyObject
		{
			public Dictionary<ulong, Point> Value = new Dictionary<ulong, Point>();
		}

	}
}
