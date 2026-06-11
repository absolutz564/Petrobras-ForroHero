using Dypsloom.RhythmTimeline.Core;
using Dypsloom.RhythmTimeline.Core.Managers;
using ForroHero.Scripts.Data;
using ForroHero.Scripts.Utils;
using UnityEngine;

namespace ForroHero.Scripts.Managers
{
    public class GameManager : SingletonBehaviour<GameManager>
    {
        [Header("Config")]
        [SerializeField] private AudioData m_AudioData;
        [SerializeField] private GameData m_GameData;
        [SerializeField] private ThemeData m_ThemeData;
        [SerializeField] private SongsData m_SongsData;

        [Header("Game")] 
        [SerializeField] private RhythmDirector m_RhythmDirector;
        [SerializeField] private AudioSource m_AccordionAudioSource;
        
        [Header("ONLY VIEW | DO NOT CHANGE")]
        public SongData CurrentSongData;
        public RhythmTimelineAsset CurrentSongTimeline;
        
        public AudioData AudioData => m_AudioData;
        public GameData GameData => m_GameData;
        public ThemeData ThemeData => m_ThemeData;
        public SongsData SongsData => m_SongsData;
        
        private bool? m_LastAccordionMuteState;

        private void Start()
        {
            ViewManager.Instance.ShowView(ViewName.Loading);
            
            m_RhythmDirector.OnSongEnd += () =>
            {
                MuteAccordion(false);
                ViewManager.Instance.ShowView(ViewName.Results);
            };
        }

        public void StartSong()
        {
            MuteAccordion(false);
            m_RhythmDirector.PlaySong(CurrentSongTimeline);
            m_RhythmDirector.SetBpm(CurrentSongData.BPM);
        }
        
        public void MuteAccordion(bool mute)
        {
            if (!m_AccordionAudioSource) 
                return;

            if (m_LastAccordionMuteState == null || m_LastAccordionMuteState.Value != mute)
            {
                m_AccordionAudioSource.mute = mute;
                m_LastAccordionMuteState = mute;
                
                Debug.Log($">> The Accordion audio source mute state changed to: {mute}");
            }
        }
    }
}
