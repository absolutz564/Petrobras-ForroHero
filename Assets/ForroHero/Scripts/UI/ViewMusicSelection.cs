using System.Collections.Generic;
using DG.Tweening;
using ForroHero.Scripts.Audio;
using ForroHero.Scripts.Game;
using ForroHero.Scripts.Managers;
using ForroHero.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ForroHero.Scripts.UI
{
    public class ViewMusicSelection : View
    {
        [Header("Config")]
        [SerializeField] private Image m_BackgroundImage;
        [SerializeField] private RectTransform m_UpButton;
        [SerializeField] private RectTransform m_DownButton;
        [SerializeField] private RectTransform m_EasyButton;
        [SerializeField] private RectTransform m_HardButton;
        [SerializeField] private Song m_SongPrefab;
        [SerializeField] private Transform m_SongListContainer;
        [SerializeField] private AudioSource m_AudioSource;
        
        [Header("ONLY VIEW | DO NOT CHANGE")]
        [SerializeField] private List<Song> m_Songs;
        public AudioClip CurrentSongAudioClip;
        
        private Tween m_UpButtonAnimation;
        private Tween m_DownButtonAnimation;
        private Tween m_EasyButtonAnimation;
        private Tween m_HardButtonAnimation;

        private Difficulty? m_SelectedDifficulty;
        private int m_CurrentSongIndex = 0;
        
        protected override void Awake()
        {
            base.Awake();
            Init();
        }
        
        private void Init()
        {
            m_BackgroundImage.sprite = GameManager.Instance.GameData.MusicSelectionBackgroundSprite;
            
            m_SelectedDifficulty = null;
            LoadSongs();
        }
        
        private void LoadSongs()
        {
            if (m_Songs.Count > 0)
            {
                foreach (var song in m_Songs)
                {
                    Destroy(song.gameObject);
                }
                m_Songs.Clear();
            }

            var lSongsData = GameManager.Instance.SongsData.Songs;
            
            foreach (var songData in lSongsData)
            {
                var lSongInstance = Instantiate(m_SongPrefab, m_SongListContainer);
                lSongInstance.SetSongData(songData);
                lSongInstance.gameObject.SetActive(true);
                m_Songs.Add(lSongInstance);
            }
            
            m_CurrentSongIndex = 0;
            HighlightCurrentSong();
        }
        
        public override void OnShow()
        {
            base.OnShow();
            
            InputManager.Instance.OnNotePressed += HandleNotePressed;
            InputManager.Instance.OnNoteReleased += HandleNoteReleased;
            
            ScoreManager.Instance.ResetScore();
            PlayPreviewSong(true);
        }

        public override void OnHide()
        {
            base.OnHide();
            
            InputManager.Instance.OnNotePressed -= HandleNotePressed;
            InputManager.Instance.OnNoteReleased -= HandleNoteReleased;
        }
        
        private void HandleNotePressed(int lane)
        {
            if (m_SelectedDifficulty == null)
            {
                m_SelectedDifficulty = lane switch
                {
                    2 => Difficulty.Easy,
                    3 => Difficulty.Hard,
                    _ => m_SelectedDifficulty
                };

                Debug.Log($"[Difficulty] selecionada: {m_SelectedDifficulty}");
            }
            
            switch (lane)
            {
                case 0:
                    AudioManager.Instance.PlaySFX("Previous");
                    UIUtils.ButtonKeyDown(m_UpButton, ref m_UpButtonAnimation);
                    PreviousSong();
                    break;
                case 1:
                    AudioManager.Instance.PlaySFX("Next");
                    UIUtils.ButtonKeyDown(m_DownButton, ref m_DownButtonAnimation);
                    NextSong();
                    break;
                case 2:
                    AudioManager.Instance.PlaySFX("Click");
                    UIUtils.ButtonKeyDown(m_EasyButton, ref m_EasyButtonAnimation);
                    break;
                case 3:
                    AudioManager.Instance.PlaySFX("Click");
                    UIUtils.ButtonKeyDown(m_HardButton, ref m_HardButtonAnimation);
                    break;
            }
        }
        
        private void HandleNoteReleased(int lane)
        {
            switch (lane)
            {
                case 0:
                    UIUtils.ButtonKeyUp(m_UpButton, ref m_UpButtonAnimation);
                    break;
                
                case 1:
                    UIUtils.ButtonKeyUp(m_DownButton, ref m_DownButtonAnimation);
                    break;
                
                case 2:
                    UIUtils.ButtonKeyUp(m_EasyButton, ref m_EasyButtonAnimation, () =>
                    {
                        if (m_SelectedDifficulty == Difficulty.Easy)
                        {
                            GameManager.Instance.CurrentSongTimeline = GameManager.Instance.CurrentSongData.EasyTimeline;
                            GoToCountdown();
                        }
                            
                    });
                    break;
                case 3:
                    UIUtils.ButtonKeyUp(m_HardButton, ref m_HardButtonAnimation, () =>
                    {
                        if (m_SelectedDifficulty == Difficulty.Hard)
                        {
                            GameManager.Instance.CurrentSongTimeline = GameManager.Instance.CurrentSongData.HardTimeline;
                            GoToCountdown();
                        }
                    });
                    break;
            }
        }

        private void PreviousSong()
        {
            if (m_Songs.Count == 0) return;
            
            m_CurrentSongIndex = (m_CurrentSongIndex - 1 + m_Songs.Count) % m_Songs.Count;
            HighlightCurrentSong();
            PlayPreviewSong(true);
            
            Debug.Log($"[Song] selecionada: {m_Songs[m_CurrentSongIndex].Title}");
        }
        
        private void NextSong()
        {
            if (m_Songs.Count == 0) return;
            
            m_CurrentSongIndex = (m_CurrentSongIndex + 1) % m_Songs.Count;
            HighlightCurrentSong();
            PlayPreviewSong(true);
            
            Debug.Log($"[Song] selecionada: {m_Songs[m_CurrentSongIndex].Title}");
        }
        
        private void HighlightCurrentSong()
        {
            for (int i = 0; i < m_Songs.Count; i++)
            {
                bool isCurrent = (i == m_CurrentSongIndex);
                m_Songs[i].SetCurrentOnSelection(isCurrent);
            }
            
            GameManager.Instance.CurrentSongData = m_Songs[m_CurrentSongIndex].SongData;
            CurrentSongAudioClip = GameManager.Instance.CurrentSongData.PreviewAudioClip;
        }

        private void PlayPreviewSong(bool status)
        {
            m_AudioSource.clip = CurrentSongAudioClip;
            
            if (status)
                m_AudioSource.Play();
            else
                m_AudioSource.Stop();
        }

        private void GoToCountdown()
        {
            Debug.Log($"[Difficulty] aplicada: {m_SelectedDifficulty}");
            
            PlayPreviewSong(false);
            ViewManager.Instance.ShowView(ViewName.Countdown);
            m_SelectedDifficulty = null; // reseta
        }
    }
}
