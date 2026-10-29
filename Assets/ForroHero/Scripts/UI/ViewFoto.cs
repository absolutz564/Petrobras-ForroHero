using System.Collections;
using ForroHero.Scripts.Audio;
using ForroHero.Scripts.Managers;
using ForroHero.Scripts.Photo;
using ForroHero.Scripts.Utils;
using Nexweron.WebCamPlayer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ForroHero.Scripts.UI
{
    /// <summary>
    /// Tela de captura de foto: webcam ao vivo + botão "Capturar". Ao clicar, faz um
    /// countdown de N segundos, captura a foto CRUA da RawImage da webcam, compõe com a
    /// moldura (ResultComposer) e segue para a View do QR.
    /// </summary>
    public class ViewFoto : View
    {
        [Header("Webcam")]
        [SerializeField] private WebCamStream m_WebCamStream;
        [Tooltip("RawImage que exibe a webcam ao vivo — a foto é capturada DESTA textura.")]
        [SerializeField] private RawImage m_CameraView;

        [Header("Captura")]
        [SerializeField] private Button m_CaptureButton;
        [SerializeField] private TMP_Text m_CountdownText;
        [SerializeField] private int m_CountdownSeconds = 3;
        [SerializeField] private string m_PhotoFileName = "photo.png";
        [Tooltip("Lane do botão físico que dispara a captura (0=Cima na seleção). -1 = só toque.")]
        [SerializeField] private int m_CaptureLane = 0;

        [Header("Composição (foto + moldura)")]
        [SerializeField] private ResultComposer m_Composer;

        private bool m_Busy;

        protected override void Awake()
        {
            base.Awake();
            if (m_CaptureButton != null) m_CaptureButton.onClick.AddListener(OnCaptureClick);
        }

        public override void OnShow()
        {
            base.OnShow();

            m_Busy = false;
            if (m_CountdownText != null) m_CountdownText.text = "";
            if (m_CaptureButton != null) m_CaptureButton.interactable = true;

            // Botão físico (lane) além do toque. Assina só agora, então o release que
            // veio do Results não dispara a captura.
            InputManager.Instance.OnNoteReleased += HandleNoteReleased;

            // Liga a câmera (recria a textura se foi parada na sessão anterior).
            if (m_WebCamStream != null) m_WebCamStream.Play(true);
        }

        public override void OnHide()
        {
            base.OnHide();
            InputManager.Instance.OnNoteReleased -= HandleNoteReleased;
            if (m_WebCamStream != null) m_WebCamStream.Stop();
        }

        private void HandleNoteReleased(int lane)
        {
            if (lane == m_CaptureLane) OnCaptureClick();
        }

        // Ligado ao OnClick do botão Capturar (também chamável pelo Inspector).
        public void OnCaptureClick()
        {
            if (m_Busy) return;
            m_Busy = true;
            if (m_CaptureButton != null) m_CaptureButton.interactable = false;
            AudioManager.Instance.PlaySFX("Click");
            StartCoroutine(CountdownThenCapture());
        }

        private IEnumerator CountdownThenCapture()
        {
            for (int i = m_CountdownSeconds; i > 0; i--)
            {
                if (m_CountdownText != null) m_CountdownText.text = i.ToString();
                yield return new WaitForSeconds(1f);
            }
            if (m_CountdownText != null) m_CountdownText.text = "";

            // Garante que o frame mais recente da webcam já foi desenhado.
            yield return new WaitForEndOfFrame();

            // Captura a webcam como está (só normaliza color space) e compõe com a moldura.
            Texture2D raw = PhotoUtils.CaptureRawImage(m_CameraView);
            PhotoSession.RawPhotoPath = PhotoUtils.SavePng(raw, m_PhotoFileName);

            string finalPath = "";
            Texture2D finalImg = raw;
            if (m_Composer != null)
                finalImg = m_Composer.Compose(raw, "", 0, out finalPath);
            else
                finalPath = PhotoUtils.SavePng(raw, "final.png"); // fallback sem moldura

            PhotoSession.FinalImage = finalImg;
            PhotoSession.FinalImagePath = finalPath;

            if (m_WebCamStream != null) m_WebCamStream.Stop();

            ViewManager.Instance.ShowView(ViewName.QR, true);
        }
    }
}
