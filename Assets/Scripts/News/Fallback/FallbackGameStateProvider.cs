using ElectionEmpire.Core;
using ElectionEmpire.News.Fallback;

namespace ElectionEmpire.News
{
    /// <summary>
    /// Adapter to provide game state to fallback system
    /// </summary>
    public class FallbackGameStateProvider : IGameStateProvider
    {
        private Core.GameManager gameManager;
        
        public FallbackGameStateProvider(Core.GameManager manager)
        {
            gameManager = manager;
        }
        
        public int GetPlayerOfficeTier()
        {
            return gameManager?.CurrentPlayer?.CurrentOffice?.Tier ?? 1;
        }
        
        public float GetPlayerApproval()
        {
            return gameManager?.CurrentPlayer?.ApprovalRating ?? 50f;
        }
        
        public int GetTurnsUntilElection()
        {
            if (gameManager?.CurrentPlayer?.TermEndDate != default)
            {
                var daysRemaining = (gameManager.CurrentPlayer.TermEndDate - System.DateTime.Now).TotalDays;
                return UnityEngine.Mathf.Max(0, (int)daysRemaining);
            }
            return 365;
        }
    }
}

