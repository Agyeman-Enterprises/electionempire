using UnityEngine;

namespace ElectionEmpire.UI.Screens
{
    /// <summary>
    /// Base class for all UI screens.
    /// </summary>
    public abstract class BaseScreen : MonoBehaviour
    {
        public virtual void OnScreenEnter(object data = null)
        {
            gameObject.SetActive(true);
        }
        
        public virtual void OnScreenExit()
        {
            gameObject.SetActive(false);
        }
        
        public virtual bool CanNavigateBack()
        {
            return true;
        }
        
        protected void NavigateTo(ScreenType screenType, object data = null)
        {
            UIManager.Instance?.NavigateToScreen(screenType, data);
        }
        
        protected void NavigateBack()
        {
            UIManager.Instance?.NavigateBack();
        }
    }
}

