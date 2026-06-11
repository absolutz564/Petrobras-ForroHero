using System.Collections.Generic;
using UnityEngine;

namespace ForroHero.Scripts.Utils
{
    public class RandomSprite : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private List<Sprite> m_Sprites;
        [SerializeField] private SpriteRenderer m_SpriteRenderer;
        
        private void Start()
        {
            if (m_Sprites.Count == 0)
            {
                Debug.LogWarning("No sprites assigned to RandomSprite component.");
                return;
            }

            SetRandomSprite();
        }

        public void SetRandomSprite()
        {
            if (m_Sprites.Count == 0 || m_SpriteRenderer == null)
            {
                Debug.LogWarning("Cannot set random sprite. Check sprites and sprite renderer.");
                return;
            }

            int randomIndex = Random.Range(0, m_Sprites.Count);
            m_SpriteRenderer.sprite = m_Sprites[randomIndex];
        }
    }
}
