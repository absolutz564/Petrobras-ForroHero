using System;
using ForroHero.Scripts.Utils;
using UnityEngine;

namespace ForroHero.Scripts.Managers
{
    public class ScoreManager : SingletonBehaviour<ScoreManager>
    {
        [Header("ONLY VIEW | DO NOT CHANGE")]
        public int Multiplier;
        public int Score;
        
        public Action<string> OnScoreUpdated;
        public Action<string> OnMultiplierUpdated;
        
        private const int GoodScore = 5;
        private const int GreatScore = 10;
        private const int MaxMultiplier = 4;
        private const int NotesToHitForMultiplier = 5;
        
        private int m_NotesHitCount;

        private void Start()
        {
            ResetScore();
        }

        public void AddGoodScore()
        {
            HandleHit();
            
            Score += GoodScore * Multiplier;
           
            OnScoreUpdated?.Invoke(Score.ToString());
            OnMultiplierUpdated?.Invoke(Multiplier.ToString());
        }
        
        public void AddGreatScore()
        {
            HandleHit();
            
            Score += GreatScore * Multiplier;
            
            OnScoreUpdated?.Invoke(Score.ToString());
            OnMultiplierUpdated?.Invoke(Multiplier.ToString());
        }
        
        public void MissedNote()
        {
            Multiplier = 1;
            m_NotesHitCount = 0;
            OnMultiplierUpdated?.Invoke(Multiplier.ToString());
        }
        
        public void ResetScore()
        {
            Score = 0;
            Multiplier = 1;
            m_NotesHitCount = 0;
            OnScoreUpdated?.Invoke(Score.ToString());
            OnMultiplierUpdated?.Invoke(Multiplier.ToString());
        }
        
        private void HandleHit()
        {
            if (Multiplier < MaxMultiplier)
            {
                m_NotesHitCount++;

                if (m_NotesHitCount >= NotesToHitForMultiplier)
                {
                    m_NotesHitCount = 0;
                    Multiplier++;
                }
            }
        }
    }
}
