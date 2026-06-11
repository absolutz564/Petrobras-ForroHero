using DG.Tweening;
using ForroHero.Scripts.Audio;
using ForroHero.Scripts.Managers;
using ForroHero.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ForroHero.Scripts.UI
{
    public class ViewHome : View
    {
        [Header("References")]
        [SerializeField] private Image m_BackgroundImage;
        [SerializeField] private RectTransform m_Button;
        [SerializeField] private TMP_Text m_ButtonText1;
        [SerializeField] private Image m_ButtonNoteImage;
        [SerializeField] private TMP_Text m_ButtonText2;
        
        private Tween m_ButtonAnimation;
        
        protected override void Awake()
        {
            base.Awake();
            Init();
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
                    () => { ViewManager.Instance.ShowView(ViewName.MusicSelection); });
            }
        }

        private void Init()
        {
            if (GameManager.Instance.ThemeData == null) return;
            if (GameManager.Instance.GameData == null) return;

            m_ButtonText1.color = GameManager.Instance.ThemeData.PrimaryColor;
            m_ButtonText2.color = GameManager.Instance.ThemeData.PrimaryColor;
            m_ButtonText1.font = GameManager.Instance.ThemeData.BoldFont;
            m_ButtonText2.font = GameManager.Instance.ThemeData.BoldFont;

            m_BackgroundImage.sprite = GameManager.Instance.GameData.HomeBackgroundSprite;
            m_ButtonText1.text = GameManager.Instance.GameData.HomeButtonText1;
            m_ButtonText2.text = GameManager.Instance.GameData.HomeButtonText2;
        }
    }
}
