using CoOpSpRpG;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarshWorld
{
	public static class HWOneAgent
	{
		public static OneAgent instance;
		public static void coreDialogueLoop()
		{
			DialogueTree dialogueTree = new DialogueTree();
			dialogueTree.text = "Are you still here?";
			addResearchLoop(dialogueTree);
			addNoResourcesLoop(dialogueTree);
			DialogueTree result = new DialogueTree();
			dialogueTree.addOption("Just leaving actually.", result);
			SCREEN_MANAGER.dialogue = new DialogueSelectRev2(instance, dialogueTree);
		}

		private static void addResearchLoop(DialogueTree tree)
		{
			DialogueTree dialogueTree = new DialogueTree();
			dialogueTree.text = "Yeah, I was going to do a whole song and dance routine where I walked you through the research screen step by step but you know, couldn't be bothered.";
			tree.addOption("You were going to explain the research screen to me.", dialogueTree);
			dialogueTree.addOption("Oh I changed my mind, tell me later.", tree);
			DialogueTree dialogueTree2 = new DialogueTree();
			dialogueTree2.text = "When you are out exploring you will often find data files that will allow us to unlock new technolgy. I'll need to decrypt it and research it first which costs resources.";
			dialogueTree.addOption("Great, so...maybe a summary?", dialogueTree2);
			DialogueTree dialogueTree3 = new DialogueTree();
			dialogueTree3.text = "After sufficient resources have been spent, the research items will be unlocked. These can be anything from a new gun type to another cloning bay to increase your crew size.";
			dialogueTree2.addOption("...", dialogueTree3);
			DialogueTree dialogueTree4 = new DialogueTree();
			dialogueTree4.text = "You can access the research screen by pressing the " + CONFIG.keyBindings[30].tip + "key most of the time, or you can just click the little info box in the top right which says research.";
			dialogueTree3.addOption("...", dialogueTree4);
			DialogueTree dialogueTree5 = new DialogueTree();
			dialogueTree5.text = "Aaaand that's my summary of the research screen. Go away now.";
			dialogueTree4.addOption("...", dialogueTree5);
			dialogueTree5.addOption("Your summary sucked.", tree);
			dialogueTree5.addOption("Didn't listen, I'll ask you again later.", tree);
			dialogueTree5.addOption("I'm feeling almost independent enough to try this strange new research screen on my own. Thanks!", tree);

		}
		private static void addNoResourcesLoop(DialogueTree tree)
		{
			DialogueTree result = new DialogueTree();
			DialogueTree dialogueTree = new DialogueTree();
			DialogueTree dialogueTree2 = new DialogueTree();
			DialogueTree dialogueTree3 = new DialogueTree();
			DialogueTree dialogueTree4 = new DialogueTree();
			DialogueTree dialogueTree5 = new DialogueTree();
			DialogueTree dialogueTree6 = new DialogueTree();
			DialogueTextMaker text = delegate ()
			{
				Globals.eventflags[GlobalFlag.PiratesCalled] = true;
				return "...";
			};

			dialogueTree.text = "Why am I not surprised? You have been blowing up ships left and right like a jackass...";
			tree.addOption("I lost my ship and I am out of materials to build a new one.", dialogueTree, () => !Globals.eventflags[GlobalFlag.PiratesCalled] && (PLAYER.currentShip.docked == null || PLAYER.currentShip.docked.Count < 1) && !HWFriendlyPiratesCalledEvent.canBuildShip());
			tree.addOption("I lost my ship and I am out of materials to build a new one.", dialogueTree6, () => Globals.eventflags[GlobalFlag.PiratesCalled] && (PLAYER.currentShip.docked == null || PLAYER.currentShip.docked.Count < 1) && !HWFriendlyPiratesCalledEvent.canBuildShip());
			dialogueTree.addOption("Nevermind, I will solve this myself.", tree);

			dialogueTree2.text = "Go to the shop, get yourself a mining tool and harvest some minerals from the crystal farm.";
			dialogueTree.addOption("Great, so...what should i do?", dialogueTree2);


			dialogueTree3.text = "You can wait for them to regrow or I could call your pirate friends for help.";
			dialogueTree2.addOption("I already mined them all out.", dialogueTree3, () => (PLAYER.currentGame.completedQuests.Contains("phase_1_end") && !Globals.eventflags[GlobalFlag.Sige1EventActive]) || PLAYER.debugMode);
			dialogueTree2.addOption("This is a great idea, i will do that.", tree);


			dialogueTree4.text = "I activated a distress beacon on your friends frequences. You should go to the bridge and wait for them to respond.";
			//dialogueTree4.setCameraPanText("...", new Vector3(PLAYER.currentShip.position, 10000f));
			dialogueTree3.addOption("Yes, please do that.", dialogueTree4);
			dialogueTree3.addOption("No, I will find a solution myself.", tree);


			dialogueTree5.text = "There is nothing else I can do for you. Go away now.";
			dialogueTree4.addOption(text, dialogueTree5);
			//dialogueTree5.resetCameraPanText("Great, They have responded");
			dialogueTree5.addOption("Your help sucked.", tree);
			dialogueTree5.addOption("I have more questions for you.", tree);
			dialogueTree5.addOption("I'm feeling better already. Thanks!", result);


			dialogueTree6.text = "I already called your pirate friends. Go to the bridge and wait for their response.";
			dialogueTree6.addOption("...", dialogueTree5);
		}
	}
}
