using UnityEngine;
using ElectionEmpire.UI;

namespace ElectionEmpire.World
{
    /// <summary>
    /// Component attached to district objects on the map
    /// </summary>
    public class DistrictMapElement : MonoBehaviour
    {
        public District District;
        
        private void OnMouseEnter()
        {
            // Show tooltip
            var uiManager = FindFirstObjectByType<WorldDisplay>();
            if (uiManager != null && District != null)
            {
                uiManager.ShowDistrictTooltip(District);
            }
        }
        
        private void OnMouseExit()
        {
            // Hide tooltip
            var uiManager = FindFirstObjectByType<WorldDisplay>();
            if (uiManager != null)
            {
                uiManager.HideDistrictTooltip();
            }
        }
        
        private void OnMouseDown()
        {
            // Show detailed district info
            var uiManager = FindFirstObjectByType<WorldDisplay>();
            if (uiManager != null && District != null)
            {
                uiManager.ShowDistrictDetails(District);
            }
        }
    }
}

