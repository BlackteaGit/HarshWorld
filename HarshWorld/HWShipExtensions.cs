using CoOpSpRpG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

	}
}
