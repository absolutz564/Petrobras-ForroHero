using System.Collections;
using System.Collections.Generic;
using ForroHero.Scripts.UI;
using ForroHero.Scripts.Utils;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ForroHero.Scripts.Managers
{
    [System.Serializable]
    public struct ViewData
    {
        public ViewName Name;
        public View Prefab;
    }
    
    public class ViewManager : SingletonBehaviour<ViewManager>
    {
        [Header("Config")] 
        [SerializeField] private List<ViewData> m_Views;
        [SerializeField] private float m_TransitionDuration = 0.5f;
        [SerializeField] private Image m_TransitionImage;

        private View m_CurrentView;

        public View CurrentView => m_CurrentView;

        protected override void Awake()
        {
            base.Awake();
            
            foreach (var view in m_Views)
            {
                CanvasGroup lCanvasGroup = view.Prefab.GetComponent<CanvasGroup>();
                lCanvasGroup.alpha = 0f;
            }
        }

        public void ShowView(ViewName viewName, bool isBackgroundEnabled = true)
        {
            m_TransitionImage.gameObject.SetActive(isBackgroundEnabled);
            
            View nextView = null;

            foreach (var viewData in m_Views)
            {
                if (viewData.Name == viewName)
                {
                    nextView = viewData.Prefab;
                    break;
                }
            }

            if (nextView == null)
            {
                Debug.LogError($"ViewManager: View '{viewName}' não encontrada.");
                return;
            }

            if (m_CurrentView == nextView)
                return;

            StopAllCoroutines();
            StartCoroutine(TransitionToView(nextView));
        }

        private IEnumerator TransitionToView(View nextView)
        {
            View lPreviousView = m_CurrentView;
            m_CurrentView = nextView;

            CanvasGroup lFromGroup = lPreviousView != null ? lPreviousView.GetComponent<CanvasGroup>() : null;
            CanvasGroup lToGroup = nextView.GetComponent<CanvasGroup>();
            
            lToGroup.alpha = 0f;
            
            lPreviousView?.OnHide();
            nextView.OnShow();

            Sequence lCrossfade = DOTween.Sequence();

            if (lFromGroup)
                lCrossfade.Join(lFromGroup.DOFade(0f, m_TransitionDuration).SetEase(Ease.Linear));

            lCrossfade.Join(lToGroup.DOFade(1f, m_TransitionDuration).SetEase(Ease.Linear));

            yield return lCrossfade.WaitForCompletion();
        }
    }
}
