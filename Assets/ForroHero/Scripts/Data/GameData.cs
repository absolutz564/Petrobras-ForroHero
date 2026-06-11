using UnityEngine;

namespace ForroHero.Scripts.Data
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "GameData", menuName = "ScriptableObjects/GameData")]
    public class GameData : ScriptableObject
    {
        [Header("View - Home")]
        public Sprite HomeBackgroundSprite;
        public string HomeButtonText1;
        public string HomeButtonText2;
        
        [Header("View - Music Selection")]
        public Sprite MusicSelectionBackgroundSprite;
        
        [Header("View - Countdown")]
        public Sprite CountdownBackgroundSprite;
        
        [Header("View - Game")]
        public Sprite GameBackgroundSprite;
        public Sprite GameLogoSprite;
        public Sprite GameSliderHandleSprite;
        
        [Header("View - Results")]
        public Sprite ResultsBackgroundSprite;
        public Sprite ResultsLogoSprite;
        public string ResultsTitleText;
        public string ResultsButtonText;
    }
}
