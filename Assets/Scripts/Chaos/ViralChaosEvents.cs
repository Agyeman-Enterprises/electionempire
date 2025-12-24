using System.Collections.Generic;
using UnityEngine;

namespace ElectionEmpire.Chaos
{
    /// <summary>
    /// Viral chaos events that generate massive media attention
    /// </summary>
    public class ViralChaosEvents
    {
        public enum ChaosType
        {
            // Media chaos
            OnAirMeltdown,
            PunchJournalist,
            RacistRantCaught,
            DrunkDebate,
            
            // Rally chaos
            InciteViolence,
            StageRush,
            WeaponBrandished,
            
            // Sexual chaos
            SexAtCampaignHQ,
            PublicMasturbation,
            ProstitutionSting,
            
            // Violence chaos
            PhysicalAssault,
            GunPulled,
            StabOpponent,
            
            // Absolute madness
            CannibalismJoke,
            NaziSalute,
            HitlerQuote,
            ExecutionThreat
        }
        
        public ViralEventResult TriggerChaosEvent(ChaosType type, string playerName)
        {
            var result = new ViralEventResult();
            
            switch (type)
            {
                case ChaosType.PunchJournalist:
                    result.ViralVideo = true;
                    result.Title = $"{playerName} PUNCHES REPORTER ON LIVE TV";
                    result.Views = 50000000;
                    result.ImpactEffects = new Dictionary<string, float>
                    {
                        { "Trust", -30f },
                        { "MediaRelations", -80f },
                        { "AlphaVoters", 40f },
                        { "Memes", 1000f }
                    };
                    result.Consequences = "Assault charges filed. Banned from press conferences.";
                    result.ViralPotential = "MEGA VIRAL";
                    result.GameplayEffect = "Media now HOSTILE. But... you're famous.";
                    break;
                
                case ChaosType.DrunkDebate:
                    result.ViralVideo = true;
                    result.Title = $"{playerName} DRUNK AT DEBATE - CALLS OPPONENT SLURS";
                    result.Views = 30000000;
                    result.ImpactEffects = new Dictionary<string, float>
                    {
                        { "Trust", -40f },
                        { "ReligiousVoters", -60f },
                        { "FratBros", 80f },
                        { "Memes", 500f }
                    };
                    result.Consequences = "Intervention demanded. Rehab offers pour in.";
                    result.ViralPotential = "EXTREMELY VIRAL";
                    break;
                
                case ChaosType.GunPulled:
                    result.ViralVideo = true;
                    result.Title = $"{playerName} PULLS GUN AT RALLY";
                    result.Views = 100000000;
                    result.ImpactEffects = new Dictionary<string, float>
                    {
                        { "Trust", -70f },
                        { "SecurityVoters", 50f },
                        { "Everyone", -50f },
                        { "Extremists", 100f }
                    };
                    result.Consequences = "Secret Service investigation. Possible criminal charges.";
                    result.ViralPotential = "LEGENDARY VIRAL";
                    result.AchievementUnlocked = "Literally Insane";
                    break;
                
                case ChaosType.HitlerQuote:
                    result.ViralVideo = true;
                    result.Title = $"{playerName} QUOTES HITLER APPROVINGLY";
                    result.Views = 75000000;
                    result.ImpactEffects = new Dictionary<string, float>
                    {
                        { "Trust", -90f },
                        { "AllNormalVoters", -85f },
                        { "Nazis", 100f },
                        { "InternationalIncident", 100f }
                    };
                    result.Consequences = "Party demands resignation. International condemnation.";
                    result.ViralPotential = "NUCLEAR VIRAL";
                    result.CareerEnding = true;
                    result.But = "... some people support you anyway. WTF.";
                    break;
                
                case ChaosType.ExecutionThreat:
                    result.ViralVideo = true;
                    result.Title = $"{playerName}: 'OPPONENTS SHOULD BE EXECUTED'";
                    result.Views = 80000000;
                    result.ImpactEffects = new Dictionary<string, float>
                    {
                        { "Trust", -80f },
                        { "Dictator", 100f },
                        { "Fear", 100f },
                        { "Authoritarians", 90f }
                    };
                    result.Consequences = "Secret Service visit. Mental health evaluation demanded.";
                    result.ViralPotential = "APOCALYPTIC VIRAL";
                    result.SpecialPath = "AUTHORITARIAN PATH UNLOCKED";
                    break;
            }
            
            return result;
        }
    }
    
    public class ViralEventResult
    {
        public bool ViralVideo;
        public string Title;
        public int Views;
        public Dictionary<string, float> ImpactEffects;
        public string Consequences;
        public string ViralPotential;
        public string GameplayEffect;
        public string AchievementUnlocked;
        public bool CareerEnding;
        public string But;
        public string SpecialPath;
    }
}

