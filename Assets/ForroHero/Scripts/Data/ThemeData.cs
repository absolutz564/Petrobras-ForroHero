using TMPro;
using UnityEngine;

namespace ForroHero.Scripts.Data
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "ThemeData", menuName = "ScriptableObjects/ThemeData")]
    public class ThemeData : ScriptableObject
    {
        [Header("Fonts")] 
        public TMP_FontAsset RegularFont;
        public TMP_FontAsset BoldFont;
        public TMP_FontAsset ExtraBoldFont;
        
        [Header("Colors")]
        public Color PrimaryColor;
        public Color SecondaryColor;
        public Color TertiaryColor;
        public Color WhiteColor;
        public Color DisabledColor;
    }
}
