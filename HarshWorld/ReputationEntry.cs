using CoOpSpRpG;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaywardExtensions;

namespace HarshWorld
{
	public static class CanvasReputationEntryExtension
	{	
		public static GuiElement AddReputationEntry(this Canvas canvas, string name, Texture2D texture, int offsetX, int offsetY, int bwidth, int bheight)
		{
			switch (canvas.sortType)
			{
				case SortType.horizontal:
					canvas.elementList.Add(new ReputationEntry(name, texture, canvas.posX + offsetX + canvas.headPosition, canvas.posY + offsetY, offsetX, offsetY, bwidth, bheight));
					canvas.headPosition += bwidth + offsetX;
					break;
				case SortType.vertical:
					canvas.elementList.Add(new ReputationEntry(name, texture, canvas.posX + offsetX, canvas.posY + offsetY + canvas.headPosition, offsetX, offsetY, bwidth, bheight));
					canvas.headPosition += bheight + offsetY;
					break;
				case SortType.none:
					canvas.elementList.Add(new ReputationEntry(name, texture, canvas.posX + offsetX, canvas.posY + offsetY, offsetX, offsetY, bwidth, bheight));
					break;
			}
			return canvas.elementList.Last<GuiElement>();
		}

		public static void setVisibilityInstantSelf(this Canvas canvas, bool visibility)
		{
			if (canvas.isVisible != visibility)
			{
				canvas.isVisible = visibility;
			}
		}

	}
	class ReputationEntry : Canvas
	{	
		public ReputationEntry(string name, Texture2D texture, int posX, int posY, int offsetX, int offsetY, int width, int height) : base(name, texture, posX, posY, offsetX, offsetY, width, height, SortType.horizontal)
		{
			this.textureBase = texture;
			this.width = width;
			this.height = height;
			this.offsetX = offsetX;
			this.offsetY = offsetY;
			this.baseColor = new Color(24, 31, 31, 15);
			this.currentValue = 1f;
			this.posX = posX;
			this.posY = posY;
			this.region = new Rectangle(posX, posY, width, height);
			base.inicialize();
		}

		public void SetupEntry(string name, int reputation, ulong faction)
		{
			this.name = name;
			this.Faction = faction;
			Color repcolor = CONFIG.textBrightColor;
			if(reputation < 0)
			{
				repcolor = CONFIG.textColorRed;
			}
			if (reputation > 0)
			{
				repcolor = Color.LightGreen;
			}
			this.AddCanvas("wrap", SCREEN_MANAGER.white, 0, 0, this.height + 5, this.height, SortType.horizontal, new Color(197, 250, 255, 25)).AddLabel(name.Substring(0,3), SCREEN_MANAGER.FF14, 0, 0, this.height + 5 - 1, this.height, CONFIG.textBrightColor, VerticalAlignment.center, 0, HorizontalAlignment.left, 4);
			this.AddCanvas("wrap", SCREEN_MANAGER.white, 2, 0, this.width - this.height - 5 - 2, this.height, SortType.horizontal, new Color(197, 250, 255, 12)).AddLabel(ToolBox.FormatNumber(reputation), SCREEN_MANAGER.FF14, 0, 0, this.width - this.height - 5 - 1, this.height, repcolor, VerticalAlignment.center, 0, HorizontalAlignment.right, 4);
		}


		public override void mouseCheck(Rectangle mousePos, MouseAction mouseClick)
		{
			if (this.region.Intersects(mousePos) && this.isVisible)
			{
				SCREEN_MANAGER.toolTip = new ToolTip();
				SCREEN_MANAGER.toolTip._position.X *= 2;
				SCREEN_MANAGER.toolTip._position.X += 47;
				SCREEN_MANAGER.toolTip.tip = Globals.globalfactions[this.Faction].Item1;
				foreach (var entry in Globals.getFactionRepDeeds(this.Faction))
				{
					SCREEN_MANAGER.toolTip.addStat(entry.Key, ToolBox.FormatNumber(entry.Value), false, entry.Value < 0);
				}
				this.mouseOver = true;
				foreach (GuiElement guiElement in this.elementList)
				{
					if (mouseClick == MouseAction.leftClick)
					{
						guiElement.hasFocus = false;
					}
					if (guiElement.region.Intersects(mousePos))
					{
						guiElement.mouseCheck(mousePos, mouseClick);
					}
				}
			}
		}

		public void Placeholder(object sender)
		{
		}

		public override void reposition(bool autoscale)
		{
			this.headPosition = 0;
			foreach (GuiElement guiElement in this.elementList)
			{
				if (guiElement.isVisible)
				{
					guiElement.reposition(this.posX + guiElement.offsetX, this.posY + guiElement.offsetY + this.headPosition, false);
					this.headPosition += guiElement.height + guiElement.offsetY;
				}
			}
		}

		public override void update(float elapsed, Rectangle mousePos, MouseAction mouseState)
		{
			this.mouseOver = false;
			base.update(elapsed, mousePos, mouseState);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (this.isVisible)
			{
				if (this.alphaMultiplier != 1f)
				{
					Color baseColor = this.baseColor;
					baseColor.A = (byte)((float)baseColor.A * this.alphaMultiplier);
					spriteBatch.Draw(this.textureBase, this.region, baseColor);
				}
				else
				{
					spriteBatch.Draw(this.textureBase, this.region, this.baseColor);
				}
				foreach (GuiElement guiElement in this.elementList)
				{
					guiElement.Draw(spriteBatch);
				}
			}
		}

		private ulong Faction;
	}
}

