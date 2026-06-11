using ForroHero.Scripts.Managers;
using ForroHero.Scripts.Utils;
using UnityEngine;

namespace ForroHero.Scripts.Game
{
    public class OnGoodNoteHit : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private RandomSprite m_RandomSprite;
        
        private void OnEnable()
        {
            m_RandomSprite.SetRandomSprite();
            ScoreManager.Instance.AddGoodScore();
            GameManager.Instance.MuteAccordion(false);
        }
    }
}
