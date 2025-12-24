using System.Collections.Generic;
using UnityEngine;

namespace ElectionEmpire.World
{
    /// <summary>
    /// Generates procedural names for regions, states, and districts
    /// </summary>
    public class WorldNameGenerator
    {
        // Regional name patterns
        private string[] regionPrefixes = new string[]
        {
            "Northern", "Southern", "Eastern", "Western", "Central",
            "Coastal", "Mountain", "Prairie", "Delta", "Highland",
            "Frontier", "Great", "New", "Upper", "Lower"
        };
        
        private string[] regionSuffixes = new string[]
        {
            "Coalition", "Alliance", "Federation", "Union", "League",
            "States", "Territories", "Provinces", "Commonwealth", "Assembly"
        };
        
        // State name components
        private string[] stateFeatures = new string[]
        {
            "Liberty", "Freedom", "Unity", "Justice", "Progress",
            "Harbor", "Valley", "Ridge", "Plains", "Summit",
            "River", "Lake", "Mountain", "Forest", "Desert",
            "Hope", "Victory", "Pioneer", "Heritage", "Frontier"
        };
        
        private string[] stateDirections = new string[]
        {
            "North", "South", "East", "West", "New"
        };
        
        private string[] stateSuffixes = new string[]
        {
            "State", "Territory", "Province", "Land", "ia"
        };
        
        private HashSet<string> usedNames = new HashSet<string>();
        
        /// <summary>
        /// Generate region name
        /// </summary>
        public string GenerateRegionName(int seed)
        {
            Random.InitState(seed);
            
            string prefix = regionPrefixes[Random.Range(0, regionPrefixes.Length)];
            string suffix = regionSuffixes[Random.Range(0, regionSuffixes.Length)];
            
            return $"{prefix} {suffix}";
        }
        
        /// <summary>
        /// Generate state name
        /// </summary>
        public string GenerateStateName(int seed)
        {
            Random.InitState(seed);
            
            int pattern = Random.Range(0, 5);
            
            switch (pattern)
            {
                case 0: // "New Liberty"
                    return $"New {stateFeatures[Random.Range(0, stateFeatures.Length)]}";
                
                case 1: // "Harbor State"
                    return $"{stateFeatures[Random.Range(0, stateFeatures.Length)]} State";
                
                case 2: // "North Valley"
                    return $"{stateDirections[Random.Range(0, stateDirections.Length)]} " +
                           $"{stateFeatures[Random.Range(0, stateFeatures.Length)]}";
                
                case 3: // "Progressia"
                    return $"{stateFeatures[Random.Range(0, stateFeatures.Length)]}ia";
                
                case 4: // "Liberty Territory"
                    return $"{stateFeatures[Random.Range(0, stateFeatures.Length)]} " +
                           $"{stateSuffixes[Random.Range(0, stateSuffixes.Length)]}";
            }
            
            return "State";
        }
        
        /// <summary>
        /// Generate district name
        /// </summary>
        public string GenerateDistrictName(string stateName, int districtNumber, DistrictType type)
        {
            switch (type)
            {
                case DistrictType.Urban:
                    return $"{stateName} District {districtNumber}";
                
                case DistrictType.Suburban:
                    return $"{stateName} County {districtNumber}";
                
                case DistrictType.Rural:
                    string feature = stateFeatures[Random.Range(0, stateFeatures.Length)];
                    return $"{feature} Township";
            }
            
            return $"{stateName} District {districtNumber}";
        }
        
        /// <summary>
        /// Generate unique name (ensures no duplicates)
        /// </summary>
        public string GenerateUniqueName(System.Func<string> generator)
        {
            string name;
            int attempts = 0;
            
            do
            {
                name = generator();
                attempts++;
            } 
            while (usedNames.Contains(name) && attempts < 100);
            
            usedNames.Add(name);
            return name;
        }
        
        /// <summary>
        /// Reset used names (call when generating new world)
        /// </summary>
        public void Reset()
        {
            usedNames.Clear();
        }
    }
}

