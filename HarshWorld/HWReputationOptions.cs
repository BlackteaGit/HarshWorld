using CoOpSpRpG;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarshWorld
{
    public static class HWReputationOptions
    {
		public static string targetFaction = "Transponder: Unknown";
		public static void addHailDialogue(ref DialogueTree lobby, Crew ___representative, List<ResponseImmediateAction> ___results)
		{
			//lobby.addOption("Nice to meet friendly face out here. Do you have something to trade?", dialogueTree, () => reputation > 50 && !___representative.team.threats.Contains(2U));
			//hire crew
			//hire ship
			//buy a random blueprint from ship's modules
			//barter different things
			ulong payoffCost = 0;
			var reputation = Globals.getAccumulatedReputation(___representative.faction);
			if (Globals.globalfactions.ContainsKey(___representative.faction))
			{
				if (!___representative.team.threats.Contains(2UL))
				{
					payoffCost = (ulong)Squirrel3RNG.Next(Math.Max(1000, Math.Abs(reputation) * 10), 10000);
				}
				else
				{
					payoffCost = (ulong)Squirrel3RNG.Next(Math.Max(1000, -1 * reputation * 20), 10000 / Math.Max(1, reputation)) + (ulong)Globals.globalints[GlobalInt.Bounty];
				}
			}
			else
			{
				payoffCost = (ulong)Squirrel3RNG.Next(100, 5000);
			}
			DialogueTextMaker PayForRep = delegate ()
			{	
				if (Globals.globalfactions.ContainsKey(___representative.faction))
				{
					if(!___representative.team.threats.Contains(2UL))
					{ 
						return "You are from the " + Globals.globalfactions[___representative.faction].Item1 + " right? What can I do to show my appreciation for your cause?";
					}
					else
					{
						return "I want to surrender.";
					}
				}
				if (!___representative.team.threats.Contains(2UL))
				{
					return "I want to donate you some credits as a friendly gesture.";
				}
				else
				{
					return "I want to surrender.";
				}
			};
			DialogueTree exitConversation = new DialogueTree();
			exitConversation.action = new ResponseImmediateAction(() => ___results.ForEach(i => i()));
			___results.Add(new ResponseImmediateAction(PLAYER.currentSession.unpause));
			DialogueTree dialogueTree = new DialogueTree();
			DialogueTree dialogueTree1 = new DialogueTree();
			DialogueTree dialogueTree2 = new DialogueTree();
			DialogueTree dialogueTree3 = new DialogueTree();
			DialogueTree dialogueTree4 = new DialogueTree();
			DialogueTree dialogueTree5 = new DialogueTree();
			DialogueTree dialogueTree6 = new DialogueTree();
			DialogueTree dialogueTree7 = new DialogueTree();
			lobby.addOption(PayForRep, dialogueTree, () => ___representative.team?.threats != null && !Globals.eventflags[GlobalFlag.Sige1EventActive] && !Globals.eventflags[GlobalFlag.PiratesCalledForShip] && !Globals.eventflags[GlobalFlag.PiratesCalledForDefense] && PLAYER.currentGame.completedQuests.Contains("first_battle") );
			dialogueTree.text = "Sure, if you want our favour, a donation of " + payoffCost.ToString() +" credits will make us very happy.";
			dialogueTree.addOption("Let's do it!", dialogueTree1, () => (CHARACTER_DATA.credits >= payoffCost));
			dialogueTree.addOption("No, I think it's to much for me.", exitConversation);
			
			
			dialogueTree1.text = "Thank you, we will remember that.";
			dialogueTree1.addOption("Fly safe.", exitConversation);
			dialogueTree1.action = delegate ()
			{
				CHARACTER_DATA.credits -= payoffCost;
				if (___representative.team.threats.Contains(2UL))
				{
					___representative.team.threats.Remove(2UL);
					var currentGoal = ___representative.team.goalType;
					___representative.team.goalType = ConsoleGoalType.none;
					___representative.team.focus = ___representative.team.ownedShip;
					___representative.team.goalType = currentGoal;
					var sessionships = PLAYER.currentSession.allShips.Keys.ToArray();
					for (int i = 0; i < sessionships.Length; i++)
					{
						var shipid = sessionships[i];
						if (PLAYER.currentSession.allShips[shipid] != PLAYER.currentShip && PLAYER.currentSession.allShips[shipid].cosm?.crew.FirstOrDefault().Value?.team?.threats != null && PLAYER.currentSession.allShips[shipid].faction == ___representative.faction && PLAYER.currentSession.allShips[shipid].cosm.crew.First().Value.team.threats.Contains(2UL) && Vector2.DistanceSquared(PLAYER.currentSession.allShips[shipid].position, ___representative.currentCosm.ship.position) <= CONFIG.minViewDist * CONFIG.minViewDist)
						{
							PLAYER.currentSession.allShips[shipid].cosm.crew.First().Value.team.threats.Remove(2UL);	
							var goaltostore = PLAYER.currentSession.allShips[shipid].cosm.crew.First().Value.team.goalType;
							PLAYER.currentSession.allShips[shipid].cosm.crew.First().Value.team.goalType = ConsoleGoalType.none;
							PLAYER.currentSession.allShips[shipid].cosm.crew.First().Value.team.focus = PLAYER.currentSession.allShips[shipid].cosm.crew.First().Value.team.ownedShip;
							PLAYER.currentSession.allShips[shipid].cosm.crew.First().Value.team.goalType = goaltostore;
						}
					}
				}
				else
				{
					Globals.changeReputation(___representative.faction, 10);
				}
			};
		}

	}
}
