// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - NEWS SYSTEM INTERFACES
// Interface for game state access by news translation system
// ═══════════════════════════════════════════════════════════════════════════════

using ElectionEmpire.News.Templates;
using ElectionEmpire.Core;

namespace ElectionEmpire.News.Translation
{
    /// <summary>
    /// Interface for accessing game state from the news translation system.
    /// Implemented by GameStateProvider in Core.
    /// Extends INewsTranslationCoreGameStateProvider for compatibility with all news subsystems.
    /// </summary>
    public interface IGameStateProvider : INewsTranslationCoreGameStateProvider
    {
        // Additional method not in base interface
        AlignmentState GetPlayerAlignmentState();
    }
}
