using System;
using ElectionEmpire.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionEmpire.World
{
    /// <summary>
    /// Generates procedural worlds with regions, states, and districts
    /// </summary>
    public class WorldGenerator
    {
        private WorldNameGenerator nameGen;
        
        public World GenerateWorld(string seed = null)
        {
            // Use seed for reproducible generation
            if (seed == null)
                seed = System.Guid.NewGuid().ToString();
            
            UnityEngine.Random.InitState(seed.GetHashCode());
            
            nameGen = new WorldNameGenerator();
            nameGen.Reset();
            
            var world = new World
            {
                Seed = seed,
                CreatedDate = DateTime.Now,
                Nation = GenerateNation(seed)
            };
            
            return world;
        }
        
        private Nation GenerateNation(string seed)
        {
            var nation = new Nation
            {
                Name = "The Republic",
                Regions = new List<Region>()
            };
            
            // Generate 10 distinct regions
            for (int i = 0; i < 10; i++)
            {
                var region = GenerateRegion(i, seed);
                nation.Regions.Add(region);
            }
            
            // Calculate total population
            nation.TotalPopulation = nation.Regions
                .SelectMany(r => r.States)
                .Sum(s => s.Population);
            
            return nation;
        }
        
        private Region GenerateRegion(int regionIndex, string seed)
        {
            var region = new Region
            {
                ID = $"region_{regionIndex}",
                Name = nameGen.GenerateUniqueName(() => 
                    nameGen.GenerateRegionName((seed + regionIndex).GetHashCode())),
                States = new List<State>(),
                Profile = GenerateRegionProfile(regionIndex)
            };
            
            // Generate 3-6 states per region
            int stateCount = UnityEngine.Random.Range(3, 7);
            for (int i = 0; i < stateCount; i++)
            {
                var state = GenerateState(region, i, seed);
                region.States.Add(state);
            }
            
            return region;
        }
        
        private RegionProfile GenerateRegionProfile(int regionIndex)
        {
            // Make regions distinct from each other
            float baseUrban = Mathf.Lerp(0.3f, 0.9f, (regionIndex % 3) / 2f);
            int baseWealth = (regionIndex % 5) + 1;
            float basePolitical = UnityEngine.Random.Range(-40f, 40f);
            
            return new RegionProfile
            {
                Urbanization = Mathf.Clamp01(baseUrban + UnityEngine.Random.Range(-0.1f, 0.1f)),
                WealthLevel = baseWealth,
                Education = UnityEngine.Random.Range(30f, 80f),
                PoliticalLean = Mathf.Clamp(basePolitical, -50f, 50f),
                KeyIndustries = SelectKeyIndustries(3),
                DominantBlocs = SelectDominantBlocs(2)
            };
        }
        
        private State GenerateState(Region region, int stateIndex, string seed)
        {
            var state = new State
            {
                ID = $"{region.ID}_state_{stateIndex}",
                Name = nameGen.GenerateUniqueName(() => 
                    nameGen.GenerateStateName((seed + region.ID + stateIndex).GetHashCode())),
                Population = UnityEngine.Random.Range(500000, 15000000),
                Districts = new List<District>(),
                PoliticalLean = Mathf.Clamp(
                    region.Profile.PoliticalLean + UnityEngine.Random.Range(-15f, 15f),
                    -50f, 50f)
            };
            
            // Generate 30 districts per state
            for (int i = 0; i < 30; i++)
            {
                var district = GenerateDistrict(state, region, i);
                state.Districts.Add(district);
            }
            
            // Calculate state demographics from districts
            state.Demographics = CalculateStateDemographics(state.Districts);
            
            return state;
        }
        
        private District GenerateDistrict(State state, Region region, int districtIndex)
        {
            // Determine type based on region urbanization
            DistrictType type = DetermineDistrictType(region.Profile.Urbanization);
            
            var district = new District
            {
                ID = $"{state.ID}_district_{districtIndex}",
                Name = nameGen.GenerateDistrictName(state.Name, districtIndex + 1, type),
                Population = UnityEngine.Random.Range(10000, 500000),
                Type = type,
                Demographics = GenerateDemographics(type, region),
                PoliticalLean = Mathf.Clamp(
                    state.PoliticalLean + UnityEngine.Random.Range(-10f, 10f),
                    -50f, 50f)
            };
            
            // Calculate voter bloc strengths from demographics
            district.BlocStrength = CalculateBlocStrengths(district.Demographics, type);
            
            // Determine priority issues based on district type and demographics
            district.PriorityIssues = DeterminePriorityIssues(district);
            
            return district;
        }
        
        private DistrictType DetermineDistrictType(float urbanization)
        {
            float roll = UnityEngine.Random.value;
            
            if (roll < urbanization * 0.4f)
                return DistrictType.Urban;
            else if (roll < urbanization * 0.8f)
                return DistrictType.Suburban;
            else
                return DistrictType.Rural;
        }
        
        private DemographicData GenerateDemographics(DistrictType type, Region region)
        {
            var demo = new DemographicData();
            
            // Age distribution (varies by type)
            switch (type)
            {
                case DistrictType.Urban:
                    demo.Youth18to29 = UnityEngine.Random.Range(25f, 35f);
                    demo.Adults30to49 = UnityEngine.Random.Range(30f, 40f);
                    demo.MiddleAge50to64 = UnityEngine.Random.Range(20f, 25f);
                    demo.Seniors65Plus = UnityEngine.Random.Range(10f, 15f);
                    break;
                
                case DistrictType.Suburban:
                    demo.Youth18to29 = UnityEngine.Random.Range(15f, 25f);
                    demo.Adults30to49 = UnityEngine.Random.Range(35f, 45f);
                    demo.MiddleAge50to64 = UnityEngine.Random.Range(25f, 30f);
                    demo.Seniors65Plus = UnityEngine.Random.Range(10f, 20f);
                    break;
                
                case DistrictType.Rural:
                    demo.Youth18to29 = UnityEngine.Random.Range(10f, 20f);
                    demo.Adults30to49 = UnityEngine.Random.Range(25f, 35f);
                    demo.MiddleAge50to64 = UnityEngine.Random.Range(30f, 35f);
                    demo.Seniors65Plus = UnityEngine.Random.Range(20f, 30f);
                    break;
            }
            
            // Normalize age distribution to 100%
            NormalizePercentages(
                ref demo.Youth18to29, 
                ref demo.Adults30to49, 
                ref demo.MiddleAge50to64, 
                ref demo.Seniors65Plus);
            
            // Income distribution (influenced by region wealth)
            float wealthFactor = region.Profile.WealthLevel / 5f;
            demo.LowIncome = Mathf.Lerp(40f, 20f, wealthFactor) + UnityEngine.Random.Range(-5f, 5f);
            demo.MiddleIncome = 50f + UnityEngine.Random.Range(-5f, 5f);
            demo.HighIncome = Mathf.Lerp(10f, 30f, wealthFactor) + UnityEngine.Random.Range(-5f, 5f);
            
            NormalizePercentages(ref demo.LowIncome, ref demo.MiddleIncome, ref demo.HighIncome);
            
            // Education (influenced by region)
            demo.CollegeEducated = Mathf.Clamp(
                region.Profile.Education + UnityEngine.Random.Range(-10f, 10f),
                20f, 80f);
            demo.PostGrad = demo.CollegeEducated * UnityEngine.Random.Range(0.2f, 0.4f);
            demo.HighSchoolOrLess = 100f - demo.CollegeEducated - demo.PostGrad;
            
            // Employment sectors (based on region industries)
            demo.EmploymentSectors = GenerateEmploymentSectors(region, type);
            
            return demo;
        }
        
        private Dictionary<VoterBloc, float> CalculateBlocStrengths(
            DemographicData demo, DistrictType type)
        {
            var blocs = new Dictionary<VoterBloc, float>();
            
            // Working class: Correlates with manufacturing, low income
            blocs[VoterBloc.WorkingClass] = 
                demo.LowIncome * 0.6f + 
                demo.EmploymentSectors.GetValueOrDefault("Manufacturing", 0f) * 0.4f;
            
            // Business owners: Correlates with high income
            blocs[VoterBloc.BusinessOwners] = 
                demo.HighIncome * 0.5f + 
                demo.EmploymentSectors.GetValueOrDefault("Finance", 0f) * 0.5f;
            
            // Educators: Directly from employment
            blocs[VoterBloc.Educators] = 
                demo.EmploymentSectors.GetValueOrDefault("Education", 0f);
            
            // Healthcare workers: Directly from employment
            blocs[VoterBloc.HealthcareWorkers] = 
                demo.EmploymentSectors.GetValueOrDefault("Healthcare", 0f);
            
            // Security personnel: Government + type factor
            blocs[VoterBloc.SecurityPersonnel] = 
                demo.EmploymentSectors.GetValueOrDefault("Government", 0f) * 0.3f;
            
            // Media professionals: Urban bias + services
            blocs[VoterBloc.MediaProfessionals] = 
                (type == DistrictType.Urban ? 15f : 5f) * 
                demo.EmploymentSectors.GetValueOrDefault("Services", 0f) * 0.2f;
            
            // Activists: Urban + youth
            blocs[VoterBloc.Activists] = 
                (type == DistrictType.Urban ? demo.Youth18to29 * 0.3f : demo.Youth18to29 * 0.1f);
            
            // Religious: Rural bias
            blocs[VoterBloc.Religious] = 
                (type == DistrictType.Rural ? 40f : 20f) + UnityEngine.Random.Range(-10f, 10f);
            
            // Secular: Urban bias + education
            blocs[VoterBloc.Secular] = 
                (type == DistrictType.Urban ? 50f : 20f) + demo.CollegeEducated * 0.2f;
            
            // Youth: From demographics
            blocs[VoterBloc.Youth] = demo.Youth18to29;
            
            // Seniors: From demographics
            blocs[VoterBloc.Seniors] = demo.Seniors65Plus;
            
            // Minorities: Random with urban bias
            blocs[VoterBloc.Minorities] = 
                (type == DistrictType.Urban ? UnityEngine.Random.Range(20f, 50f) : UnityEngine.Random.Range(5f, 20f));
            
            // Normalize (blocs don't need to total 100% - they overlap)
            // But cap each at reasonable maximums
            var keys = new List<VoterBloc>(blocs.Keys);
            foreach (var key in keys)
            {
                blocs[key] = Mathf.Clamp(blocs[key], 0f, 100f);
            }
            
            return blocs;
        }
        
        private List<Issue> DeterminePriorityIssues(District district)
        {
            var priorities = new List<Issue>();
            
            // Urban priorities
            if (district.Type == DistrictType.Urban)
            {
                priorities.Add(Issue.Housing);
                priorities.Add(Issue.Crime);
                priorities.Add(Issue.Transportation);
                priorities.Add(Issue.Education);
            }
            // Suburban priorities
            else if (district.Type == DistrictType.Suburban)
            {
                priorities.Add(Issue.Education);
                priorities.Add(Issue.Taxes);
                priorities.Add(Issue.Infrastructure);
                priorities.Add(Issue.Economy);
            }
            // Rural priorities
            else
            {
                priorities.Add(Issue.Agriculture);
                priorities.Add(Issue.Infrastructure);
                priorities.Add(Issue.Jobs);
                priorities.Add(Issue.Healthcare);
            }
            
            // Add 1-2 random issues for variety
            var allIssues = System.Enum.GetValues(typeof(Issue)).Cast<Issue>().ToList();
            while (priorities.Count < 5)
            {
                var random = allIssues[UnityEngine.Random.Range(0, allIssues.Count)];
                if (!priorities.Contains(random))
                    priorities.Add(random);
            }
            
            return priorities.Take(5).ToList();
        }
        
        private DemographicProfile CalculateStateDemographics(List<District> districts)
        {
            var profile = new DemographicProfile();
            int totalPop = districts.Sum(d => d.Population);
            
            if (totalPop == 0) return profile;
            
            profile.Youth18to29 = districts.Sum(d => d.Demographics.Youth18to29 * d.Population) / totalPop;
            profile.Adults30to49 = districts.Sum(d => d.Demographics.Adults30to49 * d.Population) / totalPop;
            profile.MiddleAge50to64 = districts.Sum(d => d.Demographics.MiddleAge50to64 * d.Population) / totalPop;
            profile.Seniors65Plus = districts.Sum(d => d.Demographics.Seniors65Plus * d.Population) / totalPop;
            
            return profile;
        }
        
        // Helper methods
        private void NormalizePercentages(ref float a, ref float b, ref float c, ref float d)
        {
            float total = a + b + c + d;
            if (total > 0)
            {
                a = (a / total) * 100f;
                b = (b / total) * 100f;
                c = (c / total) * 100f;
                d = (d / total) * 100f;
            }
        }
        
        private void NormalizePercentages(ref float a, ref float b, ref float c)
        {
            float total = a + b + c;
            if (total > 0)
            {
                a = (a / total) * 100f;
                b = (b / total) * 100f;
                c = (c / total) * 100f;
            }
        }
        
        private List<string> SelectKeyIndustries(int count)
        {
            var industries = new List<string> 
            { 
                "Manufacturing", "Technology", "Agriculture", "Finance",
                "Healthcare", "Education", "Tourism", "Energy" 
            };
            
            return industries.OrderBy(x => UnityEngine.Random.value).Take(count).ToList();
        }
        
        private List<VoterBloc> SelectDominantBlocs(int count)
        {
            var blocs = System.Enum.GetValues(typeof(VoterBloc))
                .Cast<VoterBloc>()
                .ToList();
            
            return blocs.OrderBy(x => UnityEngine.Random.value).Take(count).ToList();
        }
        
        private Dictionary<string, float> GenerateEmploymentSectors(Region region, DistrictType type)
        {
            var sectors = new Dictionary<string, float>();
            
            // Base distribution by type
            if (type == DistrictType.Urban)
            {
                sectors["Services"] = UnityEngine.Random.Range(30f, 50f);
                sectors["Technology"] = UnityEngine.Random.Range(10f, 30f);
                sectors["Finance"] = UnityEngine.Random.Range(5f, 20f);
                sectors["Healthcare"] = UnityEngine.Random.Range(10f, 20f);
                sectors["Education"] = UnityEngine.Random.Range(5f, 15f);
                sectors["Government"] = UnityEngine.Random.Range(5f, 10f);
                sectors["Manufacturing"] = UnityEngine.Random.Range(5f, 15f);
                sectors["Agriculture"] = UnityEngine.Random.Range(0f, 5f);
            }
            else if (type == DistrictType.Suburban)
            {
                sectors["Services"] = UnityEngine.Random.Range(25f, 40f);
                sectors["Technology"] = UnityEngine.Random.Range(5f, 20f);
                sectors["Healthcare"] = UnityEngine.Random.Range(10f, 20f);
                sectors["Education"] = UnityEngine.Random.Range(10f, 20f);
                sectors["Manufacturing"] = UnityEngine.Random.Range(10f, 20f);
                sectors["Government"] = UnityEngine.Random.Range(5f, 10f);
                sectors["Finance"] = UnityEngine.Random.Range(5f, 10f);
                sectors["Agriculture"] = UnityEngine.Random.Range(0f, 10f);
            }
            else // Rural
            {
                sectors["Agriculture"] = UnityEngine.Random.Range(30f, 50f);
                sectors["Manufacturing"] = UnityEngine.Random.Range(15f, 30f);
                sectors["Services"] = UnityEngine.Random.Range(10f, 20f);
                sectors["Healthcare"] = UnityEngine.Random.Range(5f, 10f);
                sectors["Education"] = UnityEngine.Random.Range(5f, 10f);
                sectors["Government"] = UnityEngine.Random.Range(5f, 10f);
                sectors["Technology"] = UnityEngine.Random.Range(0f, 5f);
                sectors["Finance"] = UnityEngine.Random.Range(0f, 5f);
            }
            
            // Normalize to 100%
            float total = sectors.Values.Sum();
            if (total > 0)
            {
                var keys = new List<string>(sectors.Keys);
                foreach (var key in keys)
                {
                    sectors[key] = (sectors[key] / total) * 100f;
                }
            }
            
            return sectors;
        }
    }
}

