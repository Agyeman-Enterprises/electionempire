using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.World;
using ElectionEmpire.Core;

namespace ElectionEmpire.Scandal
{
    /// <summary>
    /// Library of all scandal templates
    /// </summary>
    public class ScandalTemplateLibrary
    {
        private Dictionary<string, ScandalTemplate> templates;
        
        public void LoadTemplates()
        {
            templates = new Dictionary<string, ScandalTemplate>();
            
            // FINANCIAL SCANDALS (8 templates)
            LoadFinancialTemplates();
            
            // PERSONAL SCANDALS (8 templates)
            LoadPersonalTemplates();
            
            // POLICY SCANDALS (8 templates)
            LoadPolicyTemplates();
            
            // ADMINISTRATIVE SCANDALS (8 templates)
            LoadAdministrativeTemplates();
            
            // ELECTORAL SCANDALS (8 templates)
            LoadElectoralTemplates();
            
            Debug.Log($"Loaded {templates.Count} scandal templates");
        }
        
        private void LoadFinancialTemplates()
        {
            templates["FIN-001"] = new ScandalTemplate
            {
                ID = "FIN-001",
                Name = "Tax Irregularity",
                Category = ScandalCategory.Financial,
                SeverityMin = 1,
                SeverityMax = 3,
                BaseProbability = 0.02f,
                
                TriggerConditions = new List<TriggerCondition>
                {
                    new() { Type = "Resource", Condition = "CampaignFunds > 100000", Weight = 1.5f },
                    new() { Type = "Background", Condition = "Businessman", Weight = 2.0f }
                },
                
                TitleTemplates = new[]
                {
                    "Tax Irregularity Discovered",
                    "Questions Raised About Tax Returns",
                    "Potential Tax Issues Surface"
                },
                
                DescriptionTemplates = new[]
                {
                    "Financial analysts have discovered potential irregularities in {PLAYER}'s tax filings from {YEAR}. " +
                    "While not necessarily illegal, the discrepancies raise questions about financial transparency.",
                    "Documents obtained by investigators show {PLAYER} may have underreported income in {YEAR}, " +
                    "leading to questions about tax compliance."
                },
                
                HeadlineTemplates = new[]
                {
                    "{PLAYER} Tax Questions Mount",
                    "Tax Irregularities Dog {PLAYER} Campaign",
                    "Financial Transparency Concerns for {PLAYER}"
                },
                
                CanEvolve = true,
                TerminalEvolutions = new[] { "FIN-004" },
                
                ImpactFormula = new ScandalImpactFormula
                {
                    TrustImpactBase = -5f,
                    TrustImpactPerSeverity = -2f,
                    CapitalImpactBase = -2f,
                    BlocSensitivity = new Dictionary<VoterBloc, float>
                    {
                        { VoterBloc.BusinessOwners, 0.5f },
                        { VoterBloc.WorkingClass, 1.5f },
                        { VoterBloc.Educators, 1.2f }
                    }
                },
                
                ResponseEffectiveness = new Dictionary<ResponseType, float>
                {
                    { ResponseType.Deny, 0.5f },
                    { ResponseType.FullTransparency, 0.9f },
                    { ResponseType.LegalDefense, 0.7f },
                    { ResponseType.Apologize, 0.6f }
                }
            };
            
            templates["FIN-002"] = new ScandalTemplate
            {
                ID = "FIN-002",
                Name = "Campaign Finance Violation",
                Category = ScandalCategory.Financial,
                SeverityMin = 2,
                SeverityMax = 4,
                BaseProbability = 0.015f,
                
                TriggerConditions = new List<TriggerCondition>
                {
                    new() { Type = "Resource", Condition = "CampaignFunds > 500000", Weight = 1.5f }
                },
                
                TitleTemplates = new[]
                {
                    "Campaign Finance Violation Alleged",
                    "Illegal Contribution Questions Surface",
                    "FEC Investigating Campaign Finances"
                },
                
                DescriptionTemplates = new[]
                {
                    "Election watchdogs are raising concerns about potential campaign finance violations in {PLAYER}'s " +
                    "fundraising operation. Records show contributions that may exceed legal limits."
                },
                
                CanEvolve = true,
                TerminalEvolutions = new[] { "ELEC-003" },
                
                ImpactFormula = new ScandalImpactFormula
                {
                    TrustImpactBase = -8f,
                    TrustImpactPerSeverity = -3f,
                    CapitalImpactBase = -5f,
                    BlocSensitivity = new Dictionary<VoterBloc, float>
                    {
                        { VoterBloc.Activists, 2.0f },
                        { VoterBloc.WorkingClass, 1.5f }
                    }
                },
                
                ResponseEffectiveness = new Dictionary<ResponseType, float>
                {
                    { ResponseType.Deny, 0.4f },
                    { ResponseType.LegalDefense, 0.8f },
                    { ResponseType.SacrificeStaff, 0.7f }
                }
            };
            
            templates["FIN-004"] = new ScandalTemplate
            {
                ID = "FIN-004",
                Name = "Tax Evasion",
                Category = ScandalCategory.Financial,
                SeverityMin = 4,
                SeverityMax = 7,
                BaseProbability = 0.005f,
                
                TriggerConditions = new List<TriggerCondition>
                {
                    new() { Type = "Evolution", Condition = "FromFIN-001", Weight = 5.0f }
                },
                
                TitleTemplates = new[]
                {
                    "Tax Evasion Charges Loom",
                    "Federal Investigation: Tax Evasion",
                    "Prosecutors Building Tax Evasion Case"
                },
                
                DescriptionTemplates = new[]
                {
                    "Federal prosecutors are building a case for tax evasion against {PLAYER}. Evidence suggests " +
                    "deliberate underreporting of income totaling {AMOUNT} over {YEARS} years."
                },
                
                CanEvolve = false,
                
                ImpactFormula = new ScandalImpactFormula
                {
                    TrustImpactBase = -20f,
                    TrustImpactPerSeverity = -5f,
                    CapitalImpactBase = -15f,
                    BlocSensitivity = new Dictionary<VoterBloc, float>
                    {
                        { VoterBloc.WorkingClass, 2.5f },
                        { VoterBloc.Seniors, 2.0f }
                    }
                },
                
                ResponseEffectiveness = new Dictionary<ResponseType, float>
                {
                    { ResponseType.Deny, 0.2f },
                    { ResponseType.LegalDefense, 0.9f },
                    { ResponseType.FullTransparency, 0.6f }
                }
            };
            
            // Add more FIN templates (FIN-003, FIN-005 through FIN-008)
            // Simplified for now - can expand later
        }
        
        private void LoadPersonalTemplates()
        {
            templates["PERS-001"] = new ScandalTemplate
            {
                ID = "PERS-001",
                Name = "Minor Gaffe",
                Category = ScandalCategory.Personal,
                SeverityMin = 1,
                SeverityMax = 2,
                BaseProbability = 0.05f,
                
                TriggerConditions = new List<TriggerCondition>
                {
                    new() { Type = "Trait", Condition = "FootInMouth", Weight = 5.0f }
                },
                
                TitleTemplates = new[]
                {
                    "Awkward Moment at Rally",
                    "Candidate Misspeaks",
                    "Gaffe During Interview"
                },
                
                DescriptionTemplates = new[]
                {
                    "During a {EVENT}, {PLAYER} made an awkward comment that quickly went viral. " +
                    "While not seriously damaging, the gaffe reinforces concerns about communication skills."
                },
                
                CanEvolve = false,
                
                ImpactFormula = new ScandalImpactFormula
                {
                    TrustImpactBase = -2f,
                    TrustImpactPerSeverity = -1f,
                    BlocSensitivity = new Dictionary<VoterBloc, float>
                    {
                        { VoterBloc.Youth, 0.5f },
                        { VoterBloc.Seniors, 1.2f }
                    }
                },
                
                ResponseEffectiveness = new Dictionary<ResponseType, float>
                {
                    { ResponseType.Apologize, 0.8f },
                    { ResponseType.SpinCampaign, 0.7f }
                }
            };
            
            templates["PERS-005"] = new ScandalTemplate
            {
                ID = "PERS-005",
                Name = "Personal Relationship Scandal",
                Category = ScandalCategory.Personal,
                SeverityMin = 3,
                SeverityMax = 7,
                BaseProbability = 0.01f,
                
                TitleTemplates = new[]
                {
                    "Inappropriate Relationship Alleged",
                    "Staff Relationship Raises Questions"
                },
                
                CanEvolve = true,
                TerminalEvolutions = new[] { "PERS-007" },
                
                ImpactFormula = new ScandalImpactFormula
                {
                    TrustImpactBase = -12f,
                    TrustImpactPerSeverity = -4f,
                    BlocSensitivity = new Dictionary<VoterBloc, float>
                    {
                        { VoterBloc.Religious, 2.5f },
                        { VoterBloc.Seniors, 1.8f }
                    }
                },
                
                ResponseEffectiveness = new Dictionary<ResponseType, float>
                {
                    { ResponseType.Deny, 0.5f },
                    { ResponseType.FullTransparency, 0.7f }
                }
            };
            
            templates["PERS-007"] = new ScandalTemplate
            {
                ID = "PERS-007",
                Name = "Affair Allegation",
                Category = ScandalCategory.Personal,
                SeverityMin = 5,
                SeverityMax = 8,
                BaseProbability = 0.008f,
                
                TriggerConditions = new List<TriggerCondition>
                {
                    new() { Type = "Evolution", Condition = "FromPERS-005", Weight = 4.0f }
                },
                
                TitleTemplates = new[]
                {
                    "Affair Allegation Rocks Campaign",
                    "Extramarital Affair Alleged"
                },
                
                CanEvolve = false,
                
                ImpactFormula = new ScandalImpactFormula
                {
                    TrustImpactBase = -25f,
                    TrustImpactPerSeverity = -5f,
                    BlocSensitivity = new Dictionary<VoterBloc, float>
                    {
                        { VoterBloc.Religious, 3.0f },
                        { VoterBloc.Seniors, 2.5f }
                    }
                },
                
                ResponseEffectiveness = new Dictionary<ResponseType, float>
                {
                    { ResponseType.Deny, 0.4f },
                    { ResponseType.Apologize, 0.6f }
                }
            };
            
            // Add more PERS templates
        }
        
        private void LoadPolicyTemplates()
        {
            templates["POL-001"] = new ScandalTemplate
            {
                ID = "POL-001",
                Name = "Unintended Consequences",
                Category = ScandalCategory.Policy,
                SeverityMin = 1,
                SeverityMax = 4,
                BaseProbability = 0.03f,
                
                TitleTemplates = new[]
                {
                    "Policy Backfires",
                    "Unintended Consequences Emerge"
                },
                
                CanEvolve = true,
                TerminalEvolutions = new[] { "POL-003" },
                
                ImpactFormula = new ScandalImpactFormula
                {
                    TrustImpactBase = -5f,
                    TrustImpactPerSeverity = -2f
                },
                
                ResponseEffectiveness = new Dictionary<ResponseType, float>
                {
                    { ResponseType.Apologize, 0.7f },
                    { ResponseType.SpinCampaign, 0.6f }
                }
            };
            
            // Add more POL templates
        }
        
        private void LoadAdministrativeTemplates()
        {
            templates["ADM-001"] = new ScandalTemplate
            {
                ID = "ADM-001",
                Name = "Staff Mismanagement",
                Category = ScandalCategory.Administrative,
                SeverityMin = 2,
                SeverityMax = 5,
                BaseProbability = 0.02f,
                
                TitleTemplates = new[]
                {
                    "Staff Mismanagement Alleged",
                    "Office Chaos Revealed"
                },
                
                ImpactFormula = new ScandalImpactFormula
                {
                    TrustImpactBase = -6f,
                    TrustImpactPerSeverity = -2f
                }
            };
            
            // Add more ADM templates
        }
        
        private void LoadElectoralTemplates()
        {
            templates["ELEC-001"] = new ScandalTemplate
            {
                ID = "ELEC-001",
                Name = "Voter Suppression Allegation",
                Category = ScandalCategory.Electoral,
                SeverityMin = 3,
                SeverityMax = 6,
                BaseProbability = 0.01f,
                
                TitleTemplates = new[]
                {
                    "Voter Suppression Alleged",
                    "Election Interference Claims"
                },
                
                ImpactFormula = new ScandalImpactFormula
                {
                    TrustImpactBase = -15f,
                    TrustImpactPerSeverity = -4f
                }
            };
            
            // Add more ELEC templates
        }
        
        public ScandalTemplate GetTemplate(string templateID)
        {
            return templates.GetValueOrDefault(templateID);
        }
        
        public List<ScandalTemplate> GetTemplatesByCategory(ScandalCategory category)
        {
            return templates.Values
                .Where(t => t.Category == category)
                .ToList();
        }
        
        public List<ScandalTemplate> GetAllTemplates()
        {
            return templates.Values.ToList();
        }
    }
}

