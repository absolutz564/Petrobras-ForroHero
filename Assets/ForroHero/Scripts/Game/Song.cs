using ForroHero.Scripts.Data;
using ForroHero.Scripts.Managers;
using TMPro;
using UnityEngine;

namespace ForroHero.Scripts.Game
{
    public class Song : MonoBehaviour
    {
        [Header("Config")] 
        [SerializeField] private TMP_Text m_TitleText;
        
        [Header("ONLY VIEW | DO NOT CHANGE")]
        [SerializeField] private SongData m_SongData;
        
        public SongData SongData => m_SongData;
        
        public string Title
        {
            get => m_TitleText.text;
            set => m_TitleText.text = value;
        }
        
        public void SetSongData(SongData songData)
        {
            m_SongData = songData;
            m_TitleText.text = songData.SongName;
        }

        public void SetCurrentOnSelection(bool isCurrent)
        {
            var lDefaultColor = GameManager.Instance.ThemeData.SecondaryColor;
            var lSelectedColor = GameManager.Instance.ThemeData.TertiaryColor;

            m_TitleText.color = isCurrent ? lSelectedColor : lDefaultColor;
        }
    }
}
