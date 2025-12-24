using System.Collections.Generic;
using UnityEngine;

namespace ElectionEmpire.Chaos
{
    /// <summary>
    /// Extreme dirty tricks and tactics for chaos mode
    /// </summary>
    public class DirtyTricksExtreme
    {
        public enum ExtremeTactic
        {
            // Tier 1: Dirty but Common
            PlantFakeNews,
            DeepfakePorn,
            SwattingCall,
            DoxOpponent,
            
            // Tier 2: Seriously Messed Up
            FrameForCrime,
            PlantDrugs,
            FakeObituary,
            ImpersonateAtRally,
            
            // Tier 3: Absolutely Unhinged
            StagedAssassinationAttempt,
            FamilyThreatening,
            BlackmailWithFabrication,
            
            // Tier 4: Beyond The Pale
            ArrangeOpponentAccident,
            PoisonFood,
            FrameForTerrorism,
            
            // Tier 5: Irredeemably Evil
            OrderHit,
            MassFrameOperation,
            StartRiot
        }
        
        public DirtyTrickResult ExecuteTrick(ExtremeTactic tactic, string targetID)
        {
            var result = new DirtyTrickResult();
            
            switch (tactic)
            {
                case ExtremeTactic.DeepfakePorn:
                    result.Description = "AI-generated deepfake porn of opponent released";
                    result.SuccessChance = 0.7f;
                    result.TargetDamage = -35f;
                    result.Cost = 50000f;
                    result.BackfireRisk = 0.4f;
                    result.IfSuccessful = "Opponent's family life destroyed";
                    result.IfFails = "YOU get exposed as the creator. Career over.";
                    result.AlignmentShift = 25f;
                    break;
                
                case ExtremeTactic.PlantDrugs:
                    result.Description = "Plant drugs at opponent's home, tip off police";
                    result.SuccessChance = 0.5f;
                    result.TargetDamage = -50f;
                    result.Cost = 100000f;
                    result.BackfireRisk = 0.6f;
                    result.IfSuccessful = "Opponent arrested on camera. Campaign over.";
                    result.IfFails = "Police trace it back to YOU. Federal charges.";
                    result.RequiresContacts = "Corrupt cop";
                    result.AlignmentShift = 35f;
                    break;
                
                case ExtremeTactic.StagedAssassinationAttempt:
                    result.Description = "Fake assassination attempt on YOURSELF for sympathy";
                    result.SuccessChance = 0.6f;
                    result.SelfBenefit = 40f;
                    result.Cost = 250000f;
                    result.BackfireRisk = 0.5f;
                    result.IfSuccessful = "Sympathy vote surge! +40% approval!";
                    result.IfFails = "Crisis actors exposed. Scandal worse than any attack.";
                    result.RequiresSkill = "Acting 70+";
                    result.AlignmentShift = 30f;
                    break;
                
                case ExtremeTactic.FamilyThreatening:
                    result.Description = "Threaten opponent's family. They withdraw.";
                    result.SuccessChance = 0.8f;
                    result.TargetWithdrawal = true;
                    result.Cost = 0f;
                    result.BackfireRisk = 0.7f;
                    result.IfSuccessful = "Opponent drops out citing 'family reasons'";
                    result.IfFails = "Caught on recording. Criminal charges + career death.";
                    result.RequiresAlignment = "Chaotic Evil";
                    result.AlignmentShift = 50f;
                    result.PoliceInvestigation = true;
                    break;
                
                case ExtremeTactic.ArrangeOpponentAccident:
                    result.Description = "Arrange for opponent to have a 'car accident'";
                    result.SuccessChance = 0.4f;
                    result.TargetIncapacitation = true;
                    result.Cost = 500000f;
                    result.BackfireRisk = 0.8f;
                    result.IfSuccessful = "Opponent hospitalized. Out for months.";
                    result.IfFails = "FBI investigation. Murder conspiracy charges.";
                    result.RequiresContacts = "Organized crime";
                    result.AlignmentShift = 70f;
                    result.PermanentConsequences = "Cannot be undone. You're a monster.";
                    break;
                
                case ExtremeTactic.OrderHit:
                    result.Description = "Order assassination of opponent";
                    result.SuccessChance = 0.3f;
                    result.TargetDeath = true;
                    result.Cost = 1000000f;
                    result.BackfireRisk = 0.9f;
                    result.IfSuccessful = "Opponent found dead. You win by default.";
                    result.IfFails = "Hitman flips. Death penalty prosecution.";
                    result.RequiresAlignment = "Chaotic Evil + Sociopathic trait";
                    result.AlignmentShift = 100f;
                    result.LawEnforcement = "Federal murder investigation";
                    result.HistoricalLegacy = "You will be remembered as a political assassin";
                    result.AchievementUnlocked = "Literally Hitler";
                    break;
                
                case ExtremeTactic.StartRiot:
                    result.Description = "Incite violent riot at opponent's rally";
                    result.SuccessChance = 0.5f;
                    result.TargetDamage = -30f;
                    result.Cost = 200000f;
                    result.BackfireRisk = 0.6f;
                    result.IfSuccessful = "Rally turns violent. Opponent blamed.";
                    result.IfFails = "Agents identified. Domestic terrorism charges.";
                    result.Casualties = "Possible civilian deaths";
                    result.AlignmentShift = 60f;
                    break;
            }
            
            // Add warnings
            if (result.AlignmentShift >= 50f)
            {
                result.Warning = "⚠️ THIS ACTION IS EVIL. You cannot return from this.";
            }
            
            if (result.BackfireRisk > 0.7f)
            {
                result.Warning += "\n🔴 EXTREME RISK. This could END your campaign.";
            }
            
            return result;
        }
    }
    
    public class DirtyTrickResult
    {
        public string Description;
        public float SuccessChance;
        public float TargetDamage;
        public float SelfBenefit;
        public float Cost;
        public float BackfireRisk;
        public string IfSuccessful;
        public string IfFails;
        public string RequiresContacts;
        public string RequiresSkill;
        public string RequiresAlignment;
        public float AlignmentShift;
        public bool TargetWithdrawal;
        public bool TargetIncapacitation;
        public bool TargetDeath;
        public bool PoliceInvestigation;
        public string PermanentConsequences;
        public string LawEnforcement;
        public string HistoricalLegacy;
        public string AchievementUnlocked;
        public string Casualties;
        public string Warning;
    }
}

