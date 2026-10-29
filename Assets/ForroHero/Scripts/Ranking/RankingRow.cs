using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ForroHero.Scripts.Ranking
{
    /// <summary>
    /// Uma linha do ranking: posição (1..10), foto do jogador e pontuação.
    /// </summary>
    public class RankingRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text m_RankText;
        [SerializeField] private RawImage m_Photo;
        [SerializeField] private TMP_Text m_ScoreText;
        [Tooltip("Formato da pontuação. Ex.: '{0}' -> 60 ; '{0} Pontos' -> 60 Pontos.")]
        [SerializeField] private string m_ScoreFormat = "{0}";

        private Texture2D m_Loaded;

        /// <summary>Linha preenchida com um colocado.</summary>
        public void Set(int rank, int score, string photoPath)
        {
            if (m_RankText != null) m_RankText.text = rank.ToString();
            if (m_ScoreText != null) m_ScoreText.text = string.Format(m_ScoreFormat, score);
            SetPhoto(LoadPhoto(photoPath));
        }

        /// <summary>Linha vazia (sem colocado ainda) — mostra só o número da posição.</summary>
        public void SetEmpty(int rank)
        {
            if (m_RankText != null) m_RankText.text = rank.ToString();
            if (m_ScoreText != null) m_ScoreText.text = "";
            SetPhoto(null);
        }

        private void SetPhoto(Texture2D tex)
        {
            if (m_Loaded != null && m_Loaded != tex) Destroy(m_Loaded);
            m_Loaded = tex;
            if (m_Photo != null) m_Photo.texture = tex;
        }

        private static Texture2D LoadPhoto(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
            try
            {
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(File.ReadAllBytes(path));
                return tex;
            }
            catch { return null; }
        }
    }
}
