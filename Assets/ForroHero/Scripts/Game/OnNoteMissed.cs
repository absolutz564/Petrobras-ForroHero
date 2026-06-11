using ForroHero.Scripts.Audio;
using ForroHero.Scripts.Managers;
using UnityEngine;

namespace ForroHero.Scripts.Game
{
    public class OnNoteMissed : MonoBehaviour
    {
        private void OnEnable()
        {
            AudioManager.Instance.PlaySFX("MissedNote");
            ScoreManager.Instance.MissedNote();
            GameManager.Instance.MuteAccordion(true);
        }
    }
}
