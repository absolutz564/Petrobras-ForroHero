using System.Collections;
using DG.Tweening;
using ForroHero.Scripts.Audio;
using ForroHero.Scripts.Managers;
using ForroHero.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ForroHero.Scripts.UI
{
    public class ViewCountdown : View
    {
        [Header("Config")]
        [SerializeField] private Image m_BackgroundImage;
        [SerializeField] private TMP_Text m_CountdownText;
        
        private int m_CountdownCounter = 0;
        
        private const int COUNTDOWN_START = 3;

        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        private void Init()
        {
            m_BackgroundImage.sprite = GameManager.Instance.GameData.CountdownBackgroundSprite;
        }
        
        private void Start()
        {
            RestartCounter();
        }

        public override void OnShow()
        {
            base.OnShow();
            StartCoroutine(CountdownCoroutine());
        }

        public override void OnHide()
        {
            base.OnHide();
            RestartCounter();
        }
        
        private IEnumerator CountdownCoroutine()
        {
            RestartCounter();
            
            while (m_CountdownCounter > 0)
            {
                m_CountdownText.text = m_CountdownCounter.ToString();
                m_CountdownText.transform.localScale = Vector3.zero;
                m_CountdownText.transform.DOScale(1, 0.5f);
                AudioManager.Instance.PlaySFX("Countdown");
                yield return new WaitForSeconds(1f);
                m_CountdownCounter--;
            }
            
            ViewManager.Instance.ShowView(ViewName.Game, false);
        }
        
        private void RestartCounter()
        {
            m_CountdownText.transform.localScale = Vector3.zero;
            m_CountdownCounter = COUNTDOWN_START;
        }
    }
}
