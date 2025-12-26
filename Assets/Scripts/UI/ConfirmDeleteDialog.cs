using UnityEngine;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// Helper for confirmation dialogs (placeholder until UI system is complete)
    /// </summary>
    public static class ConfirmDeleteDialog
    {
        public static bool Show(string itemName)
        {
            // In full implementation, this would show a Unity UI dialog
            // For now, use Unity's built-in dialog in editor, or return true in play mode
            #if UNITY_EDITOR
            return UnityEditor.EditorUtility.DisplayDialog(
                "Confirm Delete",
                $"Are you sure you want to delete '{itemName}'?",
                "Delete",
                "Cancel"
            );
            #else
            // In play mode, could use a custom UI dialog
            // For now, always confirm
            return true;
            #endif
        }
    }
}

