using System;
using System.Collections;
using System.IO;
using ForroHero.Scripts.Audio;
using ForroHero.Scripts.Managers;
using ForroHero.Scripts.Photo;
using ForroHero.Scripts.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ForroHero.Scripts.UI
{
    /// <summary>
    /// Mostra a FOTO final (com moldura), faz o upload (image/png) e exibe o QR Code de
    /// download. A foto vem do PhotoSession (preenchido pela ViewFoto). Mesma config de
    /// servidor do fluxo anterior; só mudou de vídeo (.mp4) para imagem (.png).
    /// </summary>
    public class ViewVideoQR : View
    {
        [Header("Preview da foto final")]
        [SerializeField] private RawImage m_PhotoDisplay;

        [Header("QR Code")]
        [SerializeField] private GameObject m_QrCodePanel;
        [SerializeField] private RawImage m_QrCodeDisplay;

        [Header("Estados de UI")]
        [SerializeField] private GameObject m_LoadingPanel;
        [SerializeField] private GameObject m_ContentPanel;

        [Header("Configuração do Servidor (mesma do SempreLivre)")]
        [SerializeField] private string m_ServerUrl;
        [SerializeField] private string m_UploadEndpoint;
        [SerializeField] private string m_RedirectLink;

        [Header("Navegação (lanes dos botões físicos)")]
        [Tooltip("Lane que avança para a tela de Obrigado.")]
        [SerializeField] private int m_AdvanceLane = 0;
        [Tooltip("Lane que refaz a captura de foto (volta para a View - Foto).")]
        [SerializeField] private int m_RetakeLane = 1;

        private string m_Token;
        private string m_AccessToken;
        private string m_Base64QRCode;
        private bool m_CanRestart;

        public override void OnShow()
        {
            base.OnShow();

            InputManager.Instance.OnNoteReleased += HandleNoteReleased;

            m_CanRestart = false;

            if (m_QrCodePanel) m_QrCodePanel.SetActive(false);
            if (m_LoadingPanel) m_LoadingPanel.SetActive(true);
            if (m_ContentPanel) m_ContentPanel.SetActive(true);

            // Mostra a foto final composta (com moldura).
            if (m_PhotoDisplay != null && PhotoSession.FinalImage != null)
                m_PhotoDisplay.texture = PhotoSession.FinalImage;

            LoadCredentials();

            string path = PhotoSession.FinalImagePath;
            if (!string.IsNullOrEmpty(m_ServerUrl) && !string.IsNullOrEmpty(path) && File.Exists(path))
            {
                StartCoroutine(UploadImage(path));
            }
            else
            {
                Debug.Log("[ViewQR] Upload ignorado (serverUrl vazio ou imagem ausente).");
                if (m_LoadingPanel) m_LoadingPanel.SetActive(false);
                m_CanRestart = true;
            }
        }

        public override void OnHide()
        {
            base.OnHide();
            InputManager.Instance.OnNoteReleased -= HandleNoteReleased;
        }

        private void HandleNoteReleased(int lane)
        {
            // Só permite navegar depois do conteúdo carregado (evita pular o QR com o
            // mesmo input que veio do countdown da captura).
            if (!m_CanRestart) return;

            if (lane == m_AdvanceLane) GoToObrigado();
            else if (lane == m_RetakeLane) RetakePhoto();
        }

        // Públicos para também poderem ser ligados ao OnClick de um Button de toque.
        public void GoToObrigado()
        {
            if (!m_CanRestart) return;
            AudioManager.Instance.PlaySFX("Click");
            ViewManager.Instance.ShowView(ViewName.Obrigado, true);
        }

        // Refaz a captura: volta para a tela de foto (que reabre a câmera).
        public void RetakePhoto()
        {
            if (!m_CanRestart) return;
            AudioManager.Instance.PlaySFX("Click");
            ViewManager.Instance.ShowView(ViewName.Foto, true);
        }

        private void LoadCredentials()
        {
            m_Token = PlayerPrefs.GetString("token", "");
            m_AccessToken = PlayerPrefs.GetString("accessToken", "");

            if (string.IsNullOrEmpty(m_Token) || string.IsNullOrEmpty(m_AccessToken))
                Debug.LogWarning("[ViewQR] Credenciais ausentes — o TokenLoader rodou no boot?");
        }

        private IEnumerator UploadImage(string imagePath)
        {
            byte[] imageBytes = File.ReadAllBytes(imagePath);

            WWWForm form = new WWWForm();
            form.AddField("isFileIdentify", "false");
            form.AddField("identify", "false");
            form.AddField("url_redirect", m_RedirectLink);
            form.AddBinaryData("file", imageBytes, "capture.png", "image/png");

            string fullUrl = m_ServerUrl + m_UploadEndpoint;
            Debug.Log("[ViewQR] Enviando foto para: " + fullUrl);

            using (UnityWebRequest req = UnityWebRequest.Post(fullUrl, form))
            {
                req.SetRequestHeader("Authorization", "Bearer " + m_Token);
                req.SetRequestHeader("access_token", m_AccessToken);

                yield return req.SendWebRequest();

                if (m_LoadingPanel) m_LoadingPanel.SetActive(false);
                m_CanRestart = true;

                if (req.result != UnityWebRequest.Result.Success)
                    Debug.LogError($"[ViewQR] Erro no upload: {req.error} | {req.downloadHandler?.text}");
                else
                {
                    Debug.Log("[ViewQR] Upload concluído: " + req.downloadHandler.text);
                    ParseAndShowQRCode(req.downloadHandler.text);
                }
            }
        }

        private void ParseAndShowQRCode(string json)
        {
            QRCodeData data = JsonUtility.FromJson<QRCodeData>(json);
            if (data == null || string.IsNullOrEmpty(data.qrcode))
            {
                Debug.LogError("[ViewQR] QR code não encontrado na resposta.");
                return;
            }

            string raw = data.qrcode;
            m_Base64QRCode = raw.Contains(",") ? raw.Substring(raw.IndexOf(',') + 1) : raw;
            DisplayQRCode();
        }

        private void DisplayQRCode()
        {
            byte[] bytes = Convert.FromBase64String(m_Base64QRCode);
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(bytes);

            m_QrCodeDisplay.texture = tex;
            if (m_QrCodePanel) m_QrCodePanel.SetActive(true);
        }

        [Serializable]
        public class QRCodeData
        {
            public string qrcode;
            public string image;
        }
    }
}
