using CoOpSpRpG;
using System.Runtime.CompilerServices;

namespace HarshWorld
{
	public static class MonsterExtensions
	{
		static readonly ConditionalWeakTable<Monster, ScaledObject> scaled = new ConditionalWeakTable<Monster, ScaledObject>(); // "hack" to add scaled field to Monster class using extension methods. Access with monster.Getscaled(), crew.Setscaled()

		public static bool Getscaled(this Monster monster) { return scaled.GetOrCreateValue(monster).Value; }

		public static void Setscaled(this Monster monster, bool newscaled) { scaled.GetOrCreateValue(monster).Value = newscaled; }

		class ScaledObject
		{
			public bool Value = new bool();
		}
	}
}
