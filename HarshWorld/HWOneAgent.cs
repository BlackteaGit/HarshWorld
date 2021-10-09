using CoOpSpRpG;

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
			addSiegeEventDialogue(dialogueTree);
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
				Globals.eventflags[GlobalFlag.PiratesCalledForShip] = true;
				return "...";
			};

			dialogueTree.text = "Why am I not surprised? You have been blowing up ships left and right like a jackass...";
			tree.addOption("I lost my ship and I am out of materials to build a new one.", dialogueTree, () => !Globals.eventflags[GlobalFlag.PiratesCalledForShip] && (PLAYER.currentShip.docked == null || PLAYER.currentShip.docked.Count < 1) && !Globals.canBuildShip());
			tree.addOption("I lost my ship and I am out of materials to build a new one.", dialogueTree6, () => Globals.eventflags[GlobalFlag.PiratesCalledForShip] && (PLAYER.currentShip.docked == null || PLAYER.currentShip.docked.Count < 1) && !Globals.canBuildShip());
			dialogueTree.addOption("Nevermind, I will solve this myself.", tree);

			dialogueTree2.text = "Go to the shop, get yourself a mining tool and harvest some minerals from the crystal farm.";
			dialogueTree.addOption("Great, so...what should i do?", dialogueTree2);


			dialogueTree3.text = "You can wait for them to regrow or I could call your pirate friends for help.";
			dialogueTree2.addOption("I already mined them all out.", dialogueTree3, () => (PLAYER.currentGame.completedQuests.Contains("phase_1_end") && !Globals.eventflags[GlobalFlag.Sige1EventActive] && Globals.getAccumulatedReputation(8UL) > - 200) || PLAYER.debugMode);
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
		private static void addSiegeEventDialogue(DialogueTree tree)
		{
			DialogueTree result = new DialogueTree();
			DialogueTree dialogueTree = new DialogueTree();
			DialogueTree dialogueTree1 = new DialogueTree();
			DialogueTree dialogueTree2 = new DialogueTree();
			DialogueTree dialogueTree3 = new DialogueTree();
			DialogueTree dialogueTree4 = new DialogueTree();
			DialogueTree dialogueTree5 = new DialogueTree();
			DialogueTree dialogueTree6 = new DialogueTree();
			DialogueTree dialogueTree7 = new DialogueTree();
			DialogueTree dialogueTree8 = new DialogueTree();
			DialogueTextMaker text = delegate ()
			{
				Globals.eventflags[GlobalFlag.PiratesCalledForDefense] = true;
				return "...";
			};

			
			tree.addOption("About our current situation..", dialogueTree, () => Globals.eventflags[GlobalFlag.Sige1EventActive]);

			dialogueTree.text = "What are you doing here? You should be taking care of our guests.";
			dialogueTree.addOption("I don't know how to make them leave.", dialogueTree1, () => !Globals.eventflags[GlobalFlag.PiratesCalledForDefense]);
			dialogueTree.addOption("I need your help.", dialogueTree6, () => Globals.eventflags[GlobalFlag.PiratesCalledForDefense]);
			dialogueTree.addOption("Can you deactivate the lockdown? I can't enter my ship through the latched airlocks.", dialogueTree7, () => Globals.eventflags[GlobalFlag.Sige1EventLockdown]);
			dialogueTree.addOption("I'm on my way.", tree);

			dialogueTree1.text = "Why am I not surprised?";
			dialogueTree1.addOption("Nevermind, I will solve this myself.", tree);
			dialogueTree1.addOption("Great, so...what should i do?", dialogueTree2);

			dialogueTree2.text = "Use the station's mining lasers to destroy their ships if you can't do it with your ship. They will be forced to leave, if they have no options to transport our goods away. You also could try to hail them and negotiate.";
			dialogueTree2.addOption("I already tried my best.", dialogueTree3, () => PLAYER.currentGame.completedQuests.Contains("bust_pirates") && Globals.getAccumulatedReputation(8UL) > -200 && PLAYER.debugMode);
			dialogueTree2.addOption("This is a great idea, i will do that.", tree);

			dialogueTree3.text = "There is an option to call your pirate friends for help. I belive they would consider helping you out, since you already helped them out a lot.";
			dialogueTree3.addOption("Yes, please do that.", dialogueTree4);
			dialogueTree3.addOption("No, I will find a solution myself.", tree);

			dialogueTree4.text = "I activated a distress beacon on your friends frequences. You should go to the bridge and wait for them to respond.";
			dialogueTree4.addOption(text, dialogueTree5);

			dialogueTree5.text = "There is nothing else I can do for you. Go away now.";
			dialogueTree5.addOption("Your help sucked.", tree);
			dialogueTree5.addOption("I have more questions for you.", tree);
			dialogueTree5.addOption("I'm feeling better already. Thanks!", result);


			dialogueTree6.text = "I already called your pirate friends. Go to the bridge and wait for their response.";
			dialogueTree6.addOption("...", dialogueTree5);

			dialogueTree7.text = "Are you sure? The intruders will be able to haul our goods to their ships.";
			dialogueTree7.addOption("Yes, I need access to my ship to solve this situation.", dialogueTree8);
			dialogueTree7.addOption("You are right, I will try to stop them while they can't escape.", tree);

			dialogueTree8.text = "Well don't say I didn't warn you. Done.";
			dialogueTree8.addOption("I'm on my way.", tree);
			dialogueTree8.action = delegate ()
			{
				HWBaseSiegeEvent.unlockAirlocks = false;
				foreach (Module module in PROCESS_REGISTER.currentCosm.modules)
				{
					if (module.type == ModuleType.airlock)
					{
						if ((module as Airlock).locked)
						{
							(module as Airlock).locked = false;
						}
					}
				}
				PROCESS_REGISTER.currentCosm.interiorLightType = InteriorLightType.normal;
				Globals.eventflags[GlobalFlag.Sige1EventLockdown] = false;
				SCREEN_MANAGER.widgetChat.AddMessage("Emergency lockdown deactivated. Airlocks unlatched.", MessageTarget.Ship);
				HWBaseSiegeEvent.targetModule = null;
			};
		}
	}
}
