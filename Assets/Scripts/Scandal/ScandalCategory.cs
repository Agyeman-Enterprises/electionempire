// File: Assets/Scripts/Scandal/ScandalCategory.cs
// Add this file to your project

namespace ElectionEmpire.Scandal
{
    /// <summary>
    /// Categories of political scandals
    /// </summary>
    public enum ScandalCategory
    {
        None = 0,
        
        // Financial Scandals (FIN-)
        Financial,
        TaxIrregularity,
        CampaignFinanceViolation,
        ConflictOfInterest,
        TaxEvasion,
        BriberyAllegation,
        MisuseOfPublicFunds,
        InsiderTrading,
        CorruptionNetwork,
        
        // Personal Scandals (PERS-)
        Personal,
        MinorGaffe,
        EmbarrassingPhoto,
        FamilyControversy,
        ControversialStatement,
        PersonalRelationship,
        PastAssociations,
        AffairAllegation,
        SecretDoubleLife,
        
        // Policy Scandals (POL-)
        Policy,
        UnintendedConsequences,
        BudgetShortfall,
        PolicyFailure,
        HarmfulSideEffects,
        DiscriminatoryImpact,
        SystemicCollapse,
        ConstitutionalChallenge,
        CatastrophicOutcome,
        
        // Administrative Scandals (ADM-)
        Administrative,
        StaffMisstatement,
        OfficeMismanagement,
        StaffMisconduct,
        InformationLeak,
        SystemicMismanagement,
        Cronyism,
        CoverUpAttempt,
        CriminalConspiracy,
        
        // Electoral Scandals (ELEC-)
        Electoral,
        CampaignPromiseBroken,
        NegativeCampaignTactics,
        DonorInfluence,
        DivisiveRhetoric,
        SmearCampaign,
        ExtremistSupport,
        VoterManipulation,
        ElectionFraud
    }
    
    /// <summary>
    /// Severity levels for scandals
    /// </summary>
    public enum ScandalSeverity
    {
        Trivial = 1,
        Minor = 2,
        Moderate = 3,
        Serious = 4,
        Major = 5,
        Severe = 6,
        Critical = 7,
        Catastrophic = 8,
        CareerEnding = 9,
        Historic = 10
    }
    
    
}
