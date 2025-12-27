// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Game State Provider Adapter
// Adapts IGameStateProvider to INewsTranslationCoreGameStateProvider
// ═══════════════════════════════════════════════════════════════════════════════

using ElectionEmpire.Core;
using ElectionEmpire.News.Templates;

namespace ElectionEmpire.News.Translation
{
    /// <summary>
    /// Adapter that converts IGameStateProvider to INewsTranslationCoreGameStateProvider
    /// </summary>
    public class GameStateProviderAdapter : INewsTranslationCoreGameStateProvider
    {
        private readonly IGameStateProvider _baseProvider;

        public GameStateProviderAdapter(IGameStateProvider baseProvider)
        {
            _baseProvider = baseProvider;
        }

        public int GetPlayerOfficeTier()
        {
            return _baseProvider.GetPlayerOfficeTier();
        }

        public string GetPlayerOfficeTitle()
        {
            return _baseProvider.GetPlayerOfficeTitle();
        }

        public string GetPlayerName()
        {
            return _baseProvider.GetPlayerName();
        }

        public string GetPlayerParty()
        {
            return _baseProvider.GetPlayerParty();
        }

        public string GetPlayerState()
        {
            return _baseProvider.GetPlayerState();
        }

        public int GetCurrentTurn()
        {
            return _baseProvider.GetCurrentTurn();
        }

        public int GetTurnsUntilElection()
        {
            return _baseProvider.GetTurnsUntilElection();
        }

        public float GetPlayerApproval()
        {
            return _baseProvider.GetPlayerApproval();
        }

        public PlayerAlignment GetPlayerAlignment()
        {
            // IGameStateProvider returns AlignmentState, need to convert to PlayerAlignment
            var alignmentState = _baseProvider.GetPlayerAlignmentState();
            return new PlayerAlignment
            {
                LawChaos = alignmentState.LawChaosScore,
                GoodEvil = alignmentState.GoodEvilScore
            };
        }

        public string GetPlayerPartyPosition(PoliticalCategory category)
        {
            // Map PoliticalCategory to a position string based on player's party
            string party = _baseProvider.GetPlayerParty();

            return category switch
            {
                PoliticalCategory.EconomicPolicy => party == "Republican" ? "fiscally conservative" : "supports regulation",
                PoliticalCategory.HealthcarePolicy => party == "Democrat" ? "supports expansion" : "favors market solutions",
                PoliticalCategory.Immigration => party == "Democrat" ? "supports reform" : "favors enforcement",
                PoliticalCategory.ClimateEnvironment => party == "Democrat" ? "supports climate action" : "prioritizes economy",
                PoliticalCategory.CrimeJustice => party == "Republican" ? "law and order" : "criminal justice reform",
                PoliticalCategory.Education => party == "Democrat" ? "increase funding" : "school choice",
                _ => "considering all perspectives"
            };
        }

        public bool IsChaosModeEnabled()
        {
            return _baseProvider.IsChaosModeEnabled();
        }
    }
}
