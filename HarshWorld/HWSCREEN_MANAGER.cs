using CoOpSpRpG;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarshWorld
{
	public static class HWSCREEN_MANAGER
	{
		public static WidgetReputation widgetReputation = null;
		public static Texture2D[] GameArt;
		public static ToolTip toolTip = null;
		public static void initWidgets()
		{
			loadWidgetArt();
			HWSCREEN_MANAGER.widgetReputation = new WidgetReputation();
		}

		public static void loadWidgetArt()
		{
			String SteamModsDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), System.IO.Path.Combine(@"..\..\workshop\content\392080")));
			String ModsDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), System.IO.Path.Combine(@"Mods")));
			List<string> Folders = new List<string>();

			Texture2D[] array1 = new Texture2D[]
				  {
							null,
							SCREEN_MANAGER.GameArt[191]
				  };

			HWSCREEN_MANAGER.GameArt = array1;

			if (System.IO.Directory.Exists(SteamModsDirectory))
			{
				var dirs = from dir in
				 System.IO.Directory.EnumerateDirectories(SteamModsDirectory, "*",
					System.IO.SearchOption.AllDirectories)
						   select dir;

				foreach (var dir in dirs)
				{
					Folders.Add(dir);
				}
			}

			if (System.IO.Directory.Exists(ModsDirectory))
			{
				Folders.Add(ModsDirectory);
				var dirs = from dir in
				 System.IO.Directory.EnumerateDirectories(ModsDirectory, "*",
					System.IO.SearchOption.AllDirectories)
						   select dir;

				foreach (var dir in dirs)
				{
					Folders.Add(dir);
				}
			}

			foreach (var folder in Folders)
			{
				if (System.IO.File.Exists(System.IO.Path.Combine(folder, "left_widget_rep.xnb")))
				{
					Game1.instance.Content.RootDirectory = folder;
					Texture2D[] array = new Texture2D[]
					{
							null,
							Game1.instance.Content.Load<Texture2D>("left_widget_rep")
					};

					HWSCREEN_MANAGER.GameArt = array;
				}
			}
			Game1.instance.Content.RootDirectory = "Content";
		}
	}
}
