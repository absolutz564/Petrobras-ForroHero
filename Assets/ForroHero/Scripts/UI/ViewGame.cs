using System.Collections;
using ForroHero.Scripts.Audio;
using ForroHero.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ForroHero.Scripts.UI
{
    public class ViewGame : View
    {        
        [Header("Config")]
        [SerializeField] private TMP_Text m_PointsText;
        [SerializeField] private TMP_Text m_MultiplierText;
        [SerializeField] private Slider m_TimeSlider;
        [SerializeField] private Image m_LogoImage;
        [SerializeField] private Image m_SliderHandleImage;

        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        private void Init()
        {
            ScoreManager.Instance.OnScoreUpdated += UpdateScoreText;
            ScoreManager.Instance.OnMultiplierUpdated += UpdateMultiplierText;
            
            m_LogoImage.sprite = GameManager.Instance.GameData.GameLogoSprite;
            m_SliderHandleImage.sprite = GameManager.Instance.GameData.GameSliderHandleSprite;
        }
        
        private void UpdateScoreText(string score)
        {
            m_PointsText.text = score;
        }
        
        private void UpdateMultiplierText(string multiplier)
        {
            m_MultiplierText.text = $"x{multiplier}";
        }

        public override void OnShow()
        {
            base.OnShow();
            GameManager.Instance.StartSong();
            
            StartSlider();
        }

        public override void OnHide()
        {
            base.OnHide();
            
            m_TimeSlider.value = 0f;
        }
        
        private void StartSlider()
        {
            if (m_TimeSlider == null) 
                return;

            var lMaxValue = GameManager.Instance.CurrentSongData.AccordionAudioClip.length + 2f; // +2 seconds for buffer
            
            m_TimeSlider.minValue = 0f;
            m_TimeSlider.maxValue = lMaxValue;
            m_TimeSlider.value = 0f;
            
            Invoke(nameof(PlayCrowdSFX), lMaxValue - 5f);
            
            StartCoroutine(StartSliderCo(lMaxValue));
        }

        private void PlayCrowdSFX()
        {
            AudioManager.Instance.PlaySFX("Crowd");
        }
        
        private IEnumerator StartSliderCo(float maxValue)
        {
            float lElapsed = 0f;
            
            while (lElapsed < maxValue)
            {
                lElapsed += Time.deltaTime;
                m_TimeSlider.value = lElapsed;
                yield return null;
            }
            
            m_TimeSlider.value = maxValue; // Garante que termine exatamente no fim
        }
    }
}
