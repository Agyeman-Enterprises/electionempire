namespace ElectionEmpire.Chaos
{
    /// <summary>
    /// Victory paths for evil/chaos characters
    /// </summary>
    public enum EvilVictory
    {
        // Traditional evil
        StealElection,          // Fraud your way to victory
        BlackmailEveryone,      // Control through fear
        BuyElection,            // Outspend by 100:1
        
        // Authoritarian
        MilitaryCoup,           // Seize power by force
        DeclareEmergency,       // Suspend elections
        EstablishDictatorship,  // Remove term limits
        
        // Chaos
        BurnItAllDown,          // Destroy the system
        AnarchyVictory,         // No rules, just chaos
        MadKingEnding,          // Everyone fears you
        
        // Crime
        MafiaState,             // Run government as crime family
        NarcoState,             // Drug empire disguised as democracy
        KleptocracyComplete,    // Steal everything
        
        // Cult
        ReligiousDictatorship,  // Theocracy through cult
        MassHypnosis,           // Literal mind control
        FanaticArmy,            // Army of true believers
        
        // Legendary
        FakeYourDeath,          // Win then disappear
        FrameSuccessor,         // Set up your replacement for failure
        SellCountry,            // Literally sell nation to highest bidder
        NuclearBlackmail        // Hold nation hostage
    }
    
    public class EvilVictoryManager
    {
        public static string GetVictoryDescription(EvilVictory victory, string playerName)
        {
            return victory switch
            {
                EvilVictory.StealElection => 
                    $"You won the Presidency through massive election fraud. " +
                    $"Half the country knows you cheated. They can't prove it.",
                
                EvilVictory.MilitaryCoup => 
                    $"You seized power in a military coup. Democracy is dead. " +
                    $"Long live {playerName}.",
                
                EvilVictory.EstablishDictatorship => 
                    $"You removed term limits, suspended the constitution, and declared yourself " +
                    $"President for Life. The world watches in horror.",
                
                EvilVictory.MafiaState => 
                    $"You run the government like a crime family. Every official pays tribute. " +
                    $"The country is your racket.",
                
                EvilVictory.NuclearBlackmail => 
                    $"You hold the nation hostage with nuclear blackmail. " +
                    $"Everyone fears you. You are untouchable.",
                
                _ => $"You achieved victory through {victory}. The world will never be the same."
            };
        }
        
        public static int CalculateLegacyPoints(EvilVictory victory, int scandals, int investigations)
        {
            int basePoints = 1000;
            
            // Chaos bonus
            basePoints += scandals * 100;
            basePoints += investigations * 500;
            
            // Victory type bonus
            basePoints += victory switch
            {
                EvilVictory.NuclearBlackmail => 10000,
                EvilVictory.EstablishDictatorship => 8000,
                EvilVictory.MilitaryCoup => 7000,
                _ => 2000
            };
            
            return basePoints;
        }
    }
}

