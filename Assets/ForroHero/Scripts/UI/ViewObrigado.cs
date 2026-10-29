using System.Collections;
using System.Collections.Generic;
using ForroHero.Scripts.Audio;
using ForroHero.Scripts.Managers;
using ForroHero.Scripts.Photo;
using ForroHero.Scripts.Ranking;
using ForroHero.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ForroHero.Scripts.UI
{
    /// <summary>
    /// Última tela: agradecimento. Volta para a Home por input ou após um tempo.
    /// </summary>
    public class ViewObrigado : View
    {
        [Header("Config")]
        [SerializeField] private Image m_BackgroundImage;
        [SerializeField] private TMP_Text m_TitleText;

        [Header("Auto-retorno")]
        [SerializeField] private float m_AutoReturnSeconds = 8f;

        [Header("Ranking diário (top 10)")]
        [Tooltip("As 10 linhas do ranking, em ordem (1º a 10º). Use o menu Forró Hero > Montar Ranking (Obrigado) para gerar.")]
        [SerializeField] private List<RankingRow> m_Rows = new List<RankingRow>();

        private Coroutine m_AutoReturn;

        public override void OnShow()
        {
            base.OnShow();

            InputManager.Instance.OnNoteReleased += HandleNoteReleased;

            // Registra a jogada atual (pontuação + foto) e mostra o ranking do dia.
            DailyRanking.AddEntry(ScoreManager.Instance.Score, PhotoSession.RawPhotoPath);
            PopulateRanking();

            if (m_AutoReturnSeconds > 0f)
                m_AutoReturn = StartCoroutine(AutoReturnToHome());
        }

        private void PopulateRanking()
        {
            if (m_Rows == null || m_Rows.Count == 0) return;

            List<RankingEntry> top = DailyRanking.GetTop10();
            for (int i = 0; i < m_Rows.Count; i++)
            {
                if (m_Rows[i] == null) continue;
                if (i < top.Count) m_Rows[i].Set(i + 1, top[i].Score, top[i].PhotoPath);
                else m_Rows[i].SetEmpty(i + 1);
            }
        }

        public override void OnHide()
        {
            base.OnHide();

            InputManager.Instance.OnNoteReleased -= HandleNoteReleased;

            if (m_AutoReturn != null)
            {
                StopCoroutine(m_AutoReturn);
                m_AutoReturn = null;
            }
        }

        private void HandleNoteReleased(int lane)
        {
            AudioManager.Instance.PlaySFX("Click");
            ReturnToHome();
        }

        private IEnumerator AutoReturnToHome()
        {
            yield return new WaitForSeconds(m_AutoReturnSeconds);
            ReturnToHome();
        }

        // Pública para poder ser chamada pelo OnClick de um Button no Inspector.
        // "Soft reset": para todo o áudio que ficou tocando (BGM em loop, etc.) ANTES de
        // voltar pra Home — senão o áudio antigo soma com o da Home. Nesta arquitetura os
        // managers são DontDestroyOnLoad, então recarregar a cena não resetaria o áudio
        // (o AudioManager antigo sobreviveria); parar o áudio aqui é o reset correto.
        public void ReturnToHome()
        {
            AudioManager.Instance.StopAllAudio();
            ViewManager.Instance.ShowView(ViewName.Home, true);
        }
    }
}
