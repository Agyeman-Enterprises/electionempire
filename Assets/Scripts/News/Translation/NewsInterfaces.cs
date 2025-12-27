// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - NEWS SYSTEM INTERFACES
// Interface for game state access by news translation system
// ═══════════════════════════════════════════════════════════════════════════════

using ElectionEmpire.News.Templates;

namespace ElectionEmpire.News.Translation
{
    /// <summary>
    /// Interface for accessing game state from the news translation system.
    /// Implemented by GameStateProvider in Core.
    /// </summary>
    public interface IGameStateProvider
    {
        int GetPlayerOfficeTier();
        string GetPlayerOfficeTitle();
        string GetPlayerName();
        string GetPlayerParty();
        string GetPlayerState();
        int GetCurrentTurn();
        int GetTurnsUntilElection();
        float GetPlayerApproval();
        PlayerAlignment GetPlayerAlignment();
        string GetPlayerPartyPosition(PoliticalCategory category);
        bool IsChaosModeEnabled();
    }
}
