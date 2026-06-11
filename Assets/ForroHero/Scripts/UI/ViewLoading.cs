using ForroHero.Scripts.Audio;
using ForroHero.Scripts.Managers;
using ForroHero.Scripts.Utils;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ForroHero.Scripts.UI
{
    public class ViewLoading : View
    {
        [Header("Config")] 
        [SerializeField] private Slider m_ProgressBar;

        private const float PROGRESS_DURATION = 1f; 
        
        public override void OnShow()
        {
            base.OnShow();
            Init();
        }

        private void Init()
        {
            m_ProgressBar.value = 0f;
            
            m_ProgressBar.DOValue(1f, PROGRESS_DURATION).SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    ViewManager.Instance.ShowView(ViewName.Home);
                    //AudioManager.Instance.PlayBGM("BGM", 0.7f);
                });
        } 
    }
}
