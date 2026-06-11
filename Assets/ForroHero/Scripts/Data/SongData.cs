using Dypsloom.RhythmTimeline.Core;
using UnityEngine;

namespace ForroHero.Scripts.Data
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "SongData", menuName = "ScriptableObjects/SongData")]
    public class SongData : ScriptableObject
    {
        [Header("Config")]
        public string SongName;
        public float BPM;
        public RhythmTimelineAsset EasyTimeline;
        public RhythmTimelineAsset HardTimeline;
        public AudioClip AccordionAudioClip;
        public AudioClip PreviewAudioClip;
    }
}