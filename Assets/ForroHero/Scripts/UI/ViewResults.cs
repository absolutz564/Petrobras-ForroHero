using DG.Tweening;
using ForroHero.Scripts.Audio;
using ForroHero.Scripts.Managers;
using ForroHero.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ForroHero.Scripts.UI
{
    public class ViewResults : View
    {
        [Header("Config")]
        [SerializeField] private TMP_Text m_PointsText;
        [SerializeField] private Image m_BackgroundImage;
        [SerializeField] private Image m_LogoImage;
        [SerializeField] private RectTransform m_Button;
        [SerializeField] private TMP_Text m_ButtonText;
        [SerializeField] private TMP_Text m_TitleText;
        
        private Tween m_ButtonAnimation;
        
        protected override void Awake()
        {
            base.Awake();
            Init();
        }
        
        private void Init()
        {
            ScoreManager.Instance.OnScoreUpdated += UpdateScoreText;
            
            m_BackgroundImage.sprite = GameManager.Instance.GameData.ResultsBackgroundSprite;
            m_LogoImage.sprite = GameManager.Instance.GameData.ResultsLogoSprite;
            m_ButtonText.text = GameManager.Instance.GameData.ResultsButtonText;
            m_TitleText.text = GameManager.Instance.GameData.ResultsTitleText;
        }
        
        private void UpdateScoreText(string score)
        {
            m_PointsText.text = score;
        }
        
        public override void OnShow()
        {
            base.OnShow();
            
            InputManager.Instance.OnNotePressed += HandleNotePressed;
            InputManager.Instance.OnNoteReleased += HandleNoteReleased;
        }

        public override void OnHide()
        {
            base.OnHide();
            
            InputManager.Instance.OnNotePressed -= HandleNotePressed;
            InputManager.Instance.OnNoteReleased -= HandleNoteReleased;
        }
        
        private void HandleNotePressed(int lane)
        {
            if (lane == 0)
            {
                AudioManager.Instance.PlaySFX("Click");
                UIUtils.ButtonKeyDown(m_Button, ref m_ButtonAnimation);
            }
        }
        
        private void HandleNoteReleased(int lane)
        {
            if (lane == 0)
            {
                UIUtils.ButtonKeyUp(m_Button, ref m_ButtonAnimation,
                    () => { ViewManager.Instance.ShowView(ViewName.Foto, true); });
            }
        }
    }
}
