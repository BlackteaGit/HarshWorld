using CoOpSpRpG;
using Microsoft.Xna.Framework;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;


namespace HarshWorld
{
	public static class CrewExtension
	{
		static readonly ConditionalWeakTable<Crew, FloatyTextObject> floatyText = new ConditionalWeakTable<Crew, FloatyTextObject>(); // "hack" to add floatyText field to Crew class using extension methods. Access with crew.GetfloatyText(), crew.SetfloatyText()

		public static ConcurrentQueue<string> GetfloatyText(this Crew crew) { return floatyText.GetOrCreateValue(crew).Value; }

		public static void SetfloatyText(this Crew crew, ConcurrentQueue<string> newfloatyText) { floatyText.GetOrCreateValue(crew).Value = newfloatyText; }

		class FloatyTextObject
		{
			public ConcurrentQueue<string> Value = new ConcurrentQueue<string>();
		}

		public static void updateFloatyText(this Crew crew) //adding extension method updateFloatyText() to Crew class
		{
			while (crew.GetfloatyText().TryDequeue(out string t))
			{
				bool flag = PLAYER.avatar != null && crew.currentCosm == PLAYER.avatar.currentCosm && Vector2.DistanceSquared(crew.position, PLAYER.avatar.position) <= 5000f * 5000f;
				if (flag)
				{
					crew.presentFloatyText(t, crew.position);
				}
			}
		}
		public static FloatyText presentFloatyText(this Crew crew, string t, Vector2 where)
		{
			bool flag = FloatyText.buffer.Count == 0;
			FloatyText result;
			if (flag)
			{
				FloatyText floatyText = new FloatyText(t, where);
				PLAYER.avatar.currentCosm.newAnims.Enqueue(floatyText);
				result = floatyText;
			}
			else
			{
				FloatyText floatyText2 = FloatyText.buffer.Dequeue();
				floatyText2.reset(t, where);
				PLAYER.avatar.currentCosm.newAnims.Enqueue(floatyText2);
				result = floatyText2;
			}
			return result;
		}
	}

	public static class FloatyTextExtension
	{
		public static void stepInteriorFloatyText(this FloatyText floatytext, float elapsed, MicroCosm cosm)
		{

			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
			float timer = (float)typeof(FloatyText).GetField("timer", flags).GetValue(floatytext); //using reflections to access the static field timer from the class FloatyText
			Color color = (Color)typeof(FloatyText).GetField("color", flags).GetValue(floatytext);
			Vector2 position = (Vector2)typeof(FloatyText).GetField("position", flags).GetValue(floatytext);

			bool flag = floatytext.visible && cosm == PLAYER.avatar.currentCosm;
			if (flag)
			{
				timer += elapsed;
				typeof(FloatyText).GetField("timer", flags).SetValue(floatytext, timer);
				checked
				{
					int num = (int)(unchecked(timer / 5f * 255f));
					bool flag2 = num > 255;
					if (flag2)
					{
						floatytext.visible = false;
						FloatyText.buffer.Enqueue(floatytext);
					}
					else
					{
						color.A = (byte)(255 - num);
						position.Y -= timer;
						typeof(FloatyText).GetField("color", flags).SetValue(floatytext, color);
						typeof(FloatyText).GetField("position", flags).SetValue(floatytext, position);
					}
				}
			}
		}
	}
}
