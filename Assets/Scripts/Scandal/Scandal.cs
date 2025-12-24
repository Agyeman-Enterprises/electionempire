using System;
using System.Collections.Generic;
using ElectionEmpire.World;

namespace ElectionEmpire.Scandal
{
    /// <summary>
    /// Represents a scandal affecting the player
    /// </summary>
    [Serializable]
    public class Scandal
    {
        // Identity
        public string ID;
        public string Title;              // "Tax Irregularity", "Affair Allegation"
        public ScandalCategory Category;
        public string TemplateID;         // Reference to template
        
        // State
        public ScandalStage CurrentStage;
        public int TurnsInStage;
        public DateTime DiscoveryDate;
        
        // Severity & Evidence
        public int BaseSeverity;          // 1-10 (from template)
        public int CurrentSeverity;       // Can increase with evolution
        public float EvidenceStrength;    // 0-100%
        public List<EvidenceItem> Evidence;
        
        // Media & Public
        public float MediaCoverage;       // 0-100 (how much attention)
        public float PublicInterest;      // 0-100 (voter concern)
        public int MediaIntensity;        // 0-10 (coverage intensity)
        
        // Response & Resolution
        public List<ScandalResponse> ResponseHistory;
        public bool IsResolved;
        public DateTime ResolvedDate;
        public ResolutionType Resolution;
        
        // Evolution
        public string EvolutionPath;      // Which path is it taking
        public bool CanEvolve;
        public List<string> PossibleEvolutions;
        
        // Narrative
        public string Description;        // Generated narrative
        public List<string> Headlines;    // Media headlines
        public List<string> Developments; // Story beats
        
        // Impact tracking
        public Dictionary<string, float> ResourceImpacts;
        public Dictionary<VoterBloc, float> BlocImpacts;
        
        public Scandal()
        {
            Evidence = new List<EvidenceItem>();
            ResponseHistory = new List<ScandalResponse>();
            Headlines = new List<string>();
            Developments = new List<string>();
            ResourceImpacts = new Dictionary<string, float>();
            BlocImpacts = new Dictionary<VoterBloc, float>();
            PossibleEvolutions = new List<string>();
        }
    }
    
    [Serializable]
    public enum ScandalCategory
    {
        Financial,      // Money, taxes, corruption
        Personal,       // Affairs, behavior, past
        Policy,         // Failed policies, harm
        Administrative, // Staff, mismanagement
        Electoral       // Campaign violations, fraud
    }
    
    [Serializable]
    public enum ScandalStage
    {
        Emergence,      // Just discovered, limited awareness
        Development,    // Growing evidence, media picking up
        Crisis,         // Peak impact, full public awareness
        Resolution      // Being resolved or fading
    }
    
    [Serializable]
    public enum ResolutionType
    {
        Denied,         // Successfully denied
        Apologized,     // Admitted and apologized
        Deflected,      // Redirected attention
        Sacrificed,     // Scapegoated someone
        CounterAttacked,// Turned on accuser
        Covered,        // Successfully covered up
        Failed,         // Overwhelmed by scandal
        TimeFaded       // Just got old and people forgot
    }
    
    [Serializable]
    public class EvidenceItem
    {
        public string ID;
        public string Description;     // "Bank records show..."
        public float Strength;         // 0-100 (how damning)
        public DateTime DiscoveredDate;
        public string Source;          // "Anonymous leak", "Investigation"
        public bool IsPublic;          // Has it been released?
        
        public EvidenceItem()
        {
            ID = System.Guid.NewGuid().ToString();
            DiscoveredDate = DateTime.Now;
        }
    }
    
    [Serializable]
    public class ScandalResponse
    {
        public string ID;
        public ResponseType Type;
        public DateTime ResponseDate;
        public string Statement;       // What was said
        public bool Successful;        // Did it work?
        public float ImpactReduction;  // How much did it help?
        public Dictionary<string, float> Costs; // Resources spent
        
        public ScandalResponse()
        {
            ID = System.Guid.NewGuid().ToString();
            ResponseDate = DateTime.Now;
            Costs = new Dictionary<string, float>();
        }
    }
    
    [Serializable]
    public enum ResponseType
    {
        Deny,
        Apologize,
        CounterAttack,
        Distract,
        SacrificeStaff,
        LegalDefense,
        FullTransparency,
        SpinCampaign
    }
}

