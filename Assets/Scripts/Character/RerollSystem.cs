using UnityEngine;

namespace ElectionEmpire.Character
{
    /// <summary>
    /// Manages reroll system with free rerolls and Purrkoin costs
    /// </summary>
    public class RerollSystem
    {
        private static RerollSystem _instance;
        public static RerollSystem Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RerollSystem();
                return _instance;
            }
        }
        
        public int FreeRerollsRemaining = 3;
        public int PurrkoinCostPerReroll = 10;
        
        private CharacterGenerator _generator;
        private RandomMode _currentMode;
        private Character _currentCharacter;
        
        // Locked traits
        public bool LockBackground { get; set; }
        public bool LockSkills { get; set; }
        public bool LockWeapon { get; set; }
        public bool LockQuirks { get; set; }
        
        private RerollSystem()
        {
            _generator = new CharacterGenerator();
        }
        
        public Character Reroll(RandomMode mode, Character currentCharacter = null)
        {
            _currentMode = mode;
            _currentCharacter = currentCharacter;
            
            // Check if we can reroll
            if (FreeRerollsRemaining > 0)
            {
                FreeRerollsRemaining--;
                return GenerateWithLocks();
            }
            else if (HasPurrkoin(PurrkoinCostPerReroll))
            {
                DeductPurrkoin(PurrkoinCostPerReroll);
                return GenerateWithLocks();
            }
            else
            {
                Debug.LogWarning("No rerolls left. Use manual builder or earn Purrkoin.");
                return null;
            }
        }
        
        private Character GenerateWithLocks()
        {
            if (_currentCharacter == null || (!LockBackground && !LockSkills && !LockWeapon && !LockQuirks))
            {
                // No locks, generate fresh
                return _generator.GenerateRandom(_currentMode);
            }
            
            // Generate new character
            var newCharacter = _generator.GenerateRandom(_currentMode);
            
            // Apply locks
            if (LockBackground && _currentCharacter.Background != null)
            {
                newCharacter.Background = _currentCharacter.Background;
            }
            
            if (LockSkills && _currentCharacter.Skills != null && _currentCharacter.Skills.Count > 0)
            {
                newCharacter.Skills = new System.Collections.Generic.List<SkillData>(_currentCharacter.Skills);
            }
            
            if (LockWeapon && _currentCharacter.Weapon != null)
            {
                newCharacter.Weapon = _currentCharacter.Weapon;
            }
            
            if (LockQuirks && _currentCharacter.PositiveQuirks != null && _currentCharacter.NegativeQuirks != null)
            {
                newCharacter.PositiveQuirks = new System.Collections.Generic.List<QuirkData>(_currentCharacter.PositiveQuirks);
                newCharacter.NegativeQuirks = new System.Collections.Generic.List<QuirkData>(_currentCharacter.NegativeQuirks);
            }
            
            // Recalculate stats
            newCharacter.ChaosRating = _generator.CalculateChaosRating(newCharacter);
            newCharacter.Name = _generator.GenerateCharacterName(newCharacter);
            newCharacter.GeneratedNickname = _generator.GenerateNickname(newCharacter);
            
            return newCharacter;
        }
        
        public void ResetLocks()
        {
            LockBackground = false;
            LockSkills = false;
            LockWeapon = false;
            LockQuirks = false;
        }
        
        public void ResetFreeRerolls()
        {
            FreeRerollsRemaining = 3;
        }
        
        private bool HasPurrkoin(int amount)
        {
            // Check Purrkoin balance
            int currentPurrkoin = PlayerPrefs.GetInt("Purrkoin", 0);
            return currentPurrkoin >= amount;
        }
        
        private void DeductPurrkoin(int amount)
        {
            // Deduct Purrkoin
            int currentPurrkoin = PlayerPrefs.GetInt("Purrkoin", 0);
            int newBalance = Mathf.Max(0, currentPurrkoin - amount);
            PlayerPrefs.SetInt("Purrkoin", newBalance);
            PlayerPrefs.Save();
            Debug.Log($"Deducted {amount} Purrkoin for reroll. New balance: {newBalance}");
        }
    }
}

