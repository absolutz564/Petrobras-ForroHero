using System.Collections.Generic;
using UnityEngine;

namespace ForroHero.Scripts.Data
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "AudioData", menuName = "ScriptableObjects/AudioData")]
    public class AudioData : ScriptableObject
    {
        [Header("BGMs")] 
        public List<AudioClass> BGMs;
        
        [Header("SFXs")]
        public List<AudioClass> SFXs;
    }
    
    [System.Serializable]
    public struct AudioClass
    {
        public string Tag;
        public AudioClip Clip;
    }
}
