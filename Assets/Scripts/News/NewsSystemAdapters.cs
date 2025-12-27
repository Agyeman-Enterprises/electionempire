// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - News System Adapters
// Wrapper adapters to convert between different interface types
// ═══════════════════════════════════════════════════════════════════════════════

using ElectionEmpire.News.Translation;
using ElectionEmpire.News.Consequences;
using ElectionEmpire.News.Templates;

namespace ElectionEmpire.News
{
    /// <summary>
    /// Adapter to convert IGameStateProvider to INewsTranslationCoreGameStateProvider
    /// </summary>
    public class NewsTranslationAdapter : INewsTranslationCoreGameStateProvider
    {
        private readonly IGameStateProvider _provider;

        public NewsTranslationAdapter(IGameStateProvider provider)
        {
            _provider = provider;
        }

        public int GetPlayerOfficeTier() => _provider.GetPlayerOfficeTier();
        public string GetPlayerOfficeTitle() => _provider.GetPlayerOfficeTitle();
        public string GetPlayerName() => _provider.GetPlayerName();
        public string GetPlayerParty() => _provider.GetPlayerParty();
        public string GetPlayerState() => _provider.GetPlayerState();
        public int GetCurrentTurn() => _provider.GetCurrentTurn();
        public int GetTurnsUntilElection() => _provider.GetTurnsUntilElection();
        public float GetPlayerApproval() => _provider.GetPlayerApproval();
        public PlayerAlignment GetPlayerAlignment() => _provider.GetPlayerAlignment();
        public string GetPlayerPartyPosition(PoliticalCategory category) => _provider.GetPlayerPartyPosition(category);
        public bool IsChaosModeEnabled() => _provider.IsChaosModeEnabled();
    }

    /// <summary>
    /// Adapter to convert IGameStateProvider to IConsequenceEngineGameStateProvider
    /// </summary>
    public class ConsequenceEngineAdapter : IConsequenceEngineGameStateProvider
    {
        private readonly IGameStateProvider _provider;

        public ConsequenceEngineAdapter(IGameStateProvider provider)
        {
            _provider = provider;
        }

        public int GetPlayerOfficeTier() => _provider.GetPlayerOfficeTier();
        public string GetPlayerOfficeTitle() => _provider.GetPlayerOfficeTitle();
        public string GetPlayerName() => _provider.GetPlayerName();
        public string GetPlayerParty() => _provider.GetPlayerParty();
        public int GetCurrentTurn() => _provider.GetCurrentTurn();
        public int GetTurnsUntilElection() => _provider.GetTurnsUntilElection();
        public float GetPlayerApproval() => _provider.GetPlayerApproval();

        public (float LawChaos, float GoodEvil) GetPlayerAlignment()
        {
            var alignment = _provider.GetPlayerAlignment();
            return (alignment.LawChaos, alignment.GoodEvil);
        }

        public bool IsChaosModeEnabled() => _provider.IsChaosModeEnabled();
    }
}
