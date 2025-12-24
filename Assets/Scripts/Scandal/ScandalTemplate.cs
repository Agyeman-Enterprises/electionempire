using System.Collections.Generic;
using ElectionEmpire.World;

namespace ElectionEmpire.Scandal
{
    /// <summary>
    /// Template for generating scandals
    /// </summary>
    [Serializable]
    public class ScandalTemplate
    {
        public string ID;
        public string Name;
        public ScandalCategory Category;
        public int SeverityMin;          // 1-10
        public int SeverityMax;
        
        // Triggers
        public List<TriggerCondition> TriggerConditions;
        public float BaseProbability;    // Per turn
        
        // Narrative
        public string[] TitleTemplates;
        public string[] DescriptionTemplates;
        public string[] HeadlineTemplates;
        
        // Evolution
        public List<EvolutionPath> EvolutionPaths;
        public bool CanEvolve;
        public string[] TerminalEvolutions; // IDs of scandals this evolves to
        
        // Impact formulas
        public ScandalImpactFormula ImpactFormula;
        
        // Response effectiveness
        public Dictionary<ResponseType, float> ResponseEffectiveness;
        
        public ScandalTemplate()
        {
            TriggerConditions = new List<TriggerCondition>();
            EvolutionPaths = new List<EvolutionPath>();
            ImpactFormula = new ScandalImpactFormula();
            ResponseEffectiveness = new Dictionary<ResponseType, float>();
            TerminalEvolutions = new string[0];
        }
    }
    
    [Serializable]
    public class TriggerCondition
    {
        public string Type;              // "Action", "Resource", "Background", "Office"
        public string Condition;         // Specific condition to check
        public float Weight;             // How much does this increase probability
        
        public TriggerCondition()
        {
            Weight = 1.0f;
        }
    }
    
    [Serializable]
    public class EvolutionPath
    {
        public string PathName;
        public List<string> RequiredConditions;
        public float Probability;
        public string EvolvesToTemplateID;
        
        public EvolutionPath()
        {
            RequiredConditions = new List<string>();
        }
    }
    
    [Serializable]
    public class ScandalImpactFormula
    {
        public float TrustImpactBase;    // -X% public trust
        public float TrustImpactPerSeverity; // -Y% per severity point
        
        public float CapitalImpactBase;
        public float CapitalImpactPerSeverity;
        
        public Dictionary<VoterBloc, float> BlocSensitivity;
        
        public ScandalImpactFormula()
        {
            BlocSensitivity = new Dictionary<VoterBloc, float>();
        }
    }
}

