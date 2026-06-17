using System.Collections;
using ForroHero.Scripts.Audio;
using ForroHero.Scripts.Managers;
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

        private Coroutine m_AutoReturn;

        public override void OnShow()
        {
            base.OnShow();

            InputManager.Instance.OnNoteReleased += HandleNoteReleased;

            if (m_AutoReturnSeconds > 0f)
                m_AutoReturn = StartCoroutine(AutoReturnToHome());
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
