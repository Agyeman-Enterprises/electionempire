// File: Assets/Scripts/Core/Enums/VoterBloc.cs
// Add this file to your project

namespace ElectionEmpire.Core
{
    /// <summary>
    /// Represents different voter demographic blocs that can be targeted or influenced
    /// </summary>
    public enum VoterBloc
    {
        None = 0,
        
        // Economic blocs
        WorkingClass,
        MiddleClass,
        Wealthy,
        SmallBusiness,
        Corporate,
        
        // Demographic blocs
        Youth,
        Seniors,
        Suburban,
        Urban,
        Rural,
        
        // Identity blocs
        Religious,
        Secular,
        Minority,
        Veterans,
        
        // Issue-based blocs
        Environmentalist,
        SecurityFocused,
        Education,
        Healthcare,
        
        // Political alignment
        Progressive,
        Moderate,
        Conservative,
        Independent,
        
        // Professional blocs
        UnionWorkers,
        TechWorkers,
        Agricultural,
        
        // Special blocs
        Undecided,
        BaseSupporter,
        SwingVoter,
        
        // Additional blocs (for compatibility)
        Educators,
        Activists,
        General,
        Business,
        BusinessOwners,
        HealthcareWorkers,
        MediaProfessionals,
        Minorities,
        SecurityPersonnel
    }
}
