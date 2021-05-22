using CoOpSpRpG;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarshWorld
{
	public class WidgetReputation
    {

		private List<GuiElement> baseCanvas;

		private Label bountyLabel;

		private Label capLabel;

		public SectorProgressBar progressBar;

		public HighlightButton baseButton;

		private ScrollCanvas reputationCanvas;

		static Color transparentBlack = new Color(0, 0, 0, 0);
		public WidgetReputation()
		{
			this.baseCanvas = new List<GuiElement>();
			this.CreateElemenets();
			RankScreen.RankInit();
		}

		public void Resize()
		{
		}

		private void CreateElemenets()
		{
			int tempwidth = 130;
			int height = 45;
			int posX = 325;
			int posY = 0;
			new Color(174, 250, 255, 210);
			if (!CONFIG.openMP)
			{
				posX = 278;
			}
			this.baseCanvas.Add(new Canvas("underlay", SCREEN_MANAGER.white, posX, posY, 0, 0, tempwidth, height, SortType.none, transparentBlack));
			GuiElement guiElement = this.baseCanvas.Last<GuiElement>();
			this.baseButton = (HighlightButton)guiElement.AddHighlightButton(null, HWSCREEN_MANAGER.GameArt[1], 0, 0, tempwidth, height, new HighlightButton.ClickEvent(this.ToggleReputations), new Rectangle(0, 45, 130, 45), Rectangle.Empty, new Rectangle(0, 0, 130, 45), Color.White, Color.White);
			this.baseButton.hasIcon = false;
			this.baseButton.mouseOverEvent = new HighlightButton.MouseOverEvent((sender) => {
				MouseState state = Mouse.GetState();
				SCREEN_MANAGER.toolTip = new ToolTip();
				/*
				SCREEN_MANAGER.toolTip._position.X *= 2;
				SCREEN_MANAGER.toolTip._position.X += 47;
				SCREEN_MANAGER.toolTip._position.Y = 0;
				*/
				SCREEN_MANAGER.toolTip._position.X = state.X;
				SCREEN_MANAGER.toolTip._position.Y = state.Y;
				SCREEN_MANAGER.toolTip.tip = "Bounty on your head";
				SCREEN_MANAGER.toolTip.botLeftText = SCREEN_MANAGER.formatCreditStringSeparate(Globals.globalints[GlobalInt.Bounty]) + " credits";
				HWSCREEN_MANAGER.toolTip = SCREEN_MANAGER.toolTip;
			});
			this.bountyLabel = (Label)guiElement.AddLabel("", SCREEN_MANAGER.FF12, 0, 0, tempwidth, 23, CONFIG.textBrightColor, VerticalAlignment.center, 0, HorizontalAlignment.right, 23);
			this.capLabel = (Label)guiElement.AddLabel("", SCREEN_MANAGER.FF12, 0, 23, tempwidth, 23, CONFIG.textBrightColor, VerticalAlignment.center, 0, HorizontalAlignment.right, 23);
			this.baseCanvas.Add(new Canvas("button under", SCREEN_MANAGER.white, tempwidth, 0, 0, 0, 195, height, SortType.horizontal, transparentBlack));
			guiElement = this.baseCanvas.Last<GuiElement>();
			this.baseCanvas.Add(new Canvas("underlay", SCREEN_MANAGER.MenuArt[295], 240, 95, 0, 0, 652, 388, SortType.none, Color.White, new Rectangle(0, 0, 652, 388)));
			Color color = new Color(127, 161, 164, 255);
			this.CloseStation(null);
			int width = 130;
			this.baseCanvas.Add(new ScrollCanvas("Reputation scroll", SCREEN_MANAGER.white, posX, 48, 0, 0, width, 230, SortType.vertical));
			this.reputationCanvas = (ScrollCanvas)this.baseCanvas.Last<GuiElement>();
			this.reputationCanvas.baseColor = new Color(9, 12, 12, 120);
			this.reputationCanvas.setVisibilityInstantSelf(false);
			//EVENTS.resourcesChanged += this.respondReputationChanged;
		}

		public void PlaceholderFunction(GuiElement sender)
		{
		}

		private void respondReputationChanged()
		{
			this.SetReputation(Globals.globalfactions);
		}

		public void SetReputation(Dictionary<ulong, Tuple<string, GlobalInt>> available)
		{
			this.reputationCanvas.elementList.Clear();
			this.reputationCanvas.headPosition = 0;
			foreach (KeyValuePair<ulong, Tuple<string, GlobalInt>> keyValuePair in available)
			{
				
				((ReputationEntry)this.reputationCanvas.AddReputationEntry("entry", SCREEN_MANAGER.white, 0, 1, 117, 30)).SetupEntry(keyValuePair.Value.Item1, Globals.getAccumulatedReputation(keyValuePair.Key), keyValuePair.Key);
			}
		}

		public void ToggleReputations(GuiElement sender)
		{
			this.reputationCanvas.setVisibilityInstantSelf(!this.reputationCanvas.isVisible);
			if (this.reputationCanvas.isVisible)
			{
				this.SetReputation(Globals.globalfactions);
			}
		}

		public void CloseStation(GuiElement sender)
		{
			this.baseCanvas.ElementAt(2).setVisibilityFadeSelf(false);
		}

		public void OpenStation(GuiElement sender)
		{
			if (this.baseCanvas.ElementAt(2).isVisible)
			{
				this.baseCanvas.ElementAt(2).setVisibilityFadeSelf(false);
				return;
			}
			this.baseCanvas.ElementAt(2).setVisibilityFadeSelf(true);
		}


		public void SetReputation(string credits, bool isRed)
		{
			this.bountyLabel.setText(credits);
			if (isRed)
			{
				this.bountyLabel.baseColor = CONFIG.textColorRed;
			}
			else
			{
				this.bountyLabel.baseColor = CONFIG.textBrightColor;
			}
			this.capLabel.setText("Reputation");
		}


		public void Update(float elapsed, MouseAction clickState, Rectangle mousePos)
		{
			foreach (GuiElement guiElement in this.baseCanvas)
			{
				if (Game1.instance.IsActive)
				{
					guiElement.mouseCheck(mousePos, clickState);
				}
				guiElement.update(elapsed, mousePos, clickState);
			}
		}

		public void Draw(SpriteBatch batch)
		{
			foreach (GuiElement guiElement in this.baseCanvas)
			{
				guiElement.Draw(batch);
			}
		}

	}
}
