using System;
using System.Collections;
using System.IO;
using ForroHero.Scripts.Audio;
using ForroHero.Scripts.Managers;
using ForroHero.Scripts.Utils;
using ForroHero.Scripts.Video;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

namespace ForroHero.Scripts.UI
{
    /// <summary>
    /// Mostra o vídeo gravado, faz o upload e exibe o QR Code de download.
    /// Portado do FinalScreenController do SempreLivre para o sistema de Views.
    /// </summary>
    public class ViewVideoQR : View
    {
        [Header("Preview do Vídeo")]
        [SerializeField] private VideoPlayer m_VideoPlayer;
        [SerializeField] private RawImage m_VideoDisplay;
        [SerializeField] private RenderTexture m_VideoRenderTexture;

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

        private string m_Token;
        private string m_AccessToken;
        private string m_Base64QRCode;
        private bool m_CanRestart;

        public override void OnShow()
        {
            base.OnShow();

            InputManager.Instance.OnNoteReleased += HandleNoteReleased;

            m_CanRestart = false;

            if (m_LoadingPanel) m_LoadingPanel.SetActive(true);
            if (m_ContentPanel) m_ContentPanel.SetActive(false);
            if (m_QrCodePanel) m_QrCodePanel.SetActive(false);

            LoadCredentials();
            StartCoroutine(WaitThenProcess(VideoRecordingController.VideoPath));
        }

        public override void OnHide()
        {
            base.OnHide();
            InputManager.Instance.OnNoteReleased -= HandleNoteReleased;

            // Para o preview (estava em loop) ao sair da view.
            if (m_VideoPlayer != null && m_VideoPlayer.isPlaying)
                m_VideoPlayer.Stop();
        }

        private void HandleNoteReleased(int lane)
        {
            // Só permite reiniciar depois do conteúdo carregado (evita pular o QR
            // com o mesmo input que veio do Results).
            if (!m_CanRestart) return;

            AudioManager.Instance.PlaySFX("Click");
            ViewManager.Instance.ShowView(ViewName.Obrigado, true);
        }

        private void LoadCredentials()
        {
            m_Token = PlayerPrefs.GetString("token", "");
            m_AccessToken = PlayerPrefs.GetString("accessToken", "");

            if (string.IsNullOrEmpty(m_Token) || string.IsNullOrEmpty(m_AccessToken))
                Debug.LogWarning("[ViewVideoQR] Credenciais ausentes — o TokenLoader rodou no boot?");
        }

        private IEnumerator WaitThenProcess(string videoPath)
        {
            float waitTime = 0f;
            while (!File.Exists(videoPath) && waitTime < 30f)
            {
                yield return new WaitForSeconds(0.5f);
                waitTime += 0.5f;
            }

            if (!File.Exists(videoPath))
            {
                Debug.LogError("[ViewVideoQR] Vídeo não encontrado após 30s: " + videoPath);
                if (m_LoadingPanel) m_LoadingPanel.SetActive(false);
                if (m_ContentPanel) m_ContentPanel.SetActive(true);
                m_CanRestart = true;
                yield break;
            }

            // espera o FFmpeg liberar o arquivo
            bool accessible = false;
            while (!accessible)
            {
                bool locked = false;
                try { using (File.Open(videoPath, FileMode.Open, FileAccess.Read, FileShare.None)) { } accessible = true; }
                catch (IOException) { locked = true; }
                if (locked) yield return new WaitForSeconds(1f);
            }

            if (m_LoadingPanel) m_LoadingPanel.SetActive(false);
            if (m_ContentPanel) m_ContentPanel.SetActive(true);
            m_CanRestart = true;

            InitVideoPreview(videoPath);

            if (!string.IsNullOrEmpty(m_ServerUrl))
                StartCoroutine(UploadVideo(videoPath));
            else
                Debug.Log("[ViewVideoQR] serverUrl vazio — upload ignorado.");
        }

        private void InitVideoPreview(string videoPath)
        {
            if (m_VideoPlayer == null || m_VideoDisplay == null) return;

            if (m_VideoRenderTexture == null)
                m_VideoRenderTexture = new RenderTexture(1080, 1920, 0);

            m_VideoDisplay.texture = m_VideoRenderTexture;
            m_VideoPlayer.renderMode = VideoRenderMode.RenderTexture;
            m_VideoPlayer.targetTexture = m_VideoRenderTexture;
            m_VideoPlayer.source = VideoSource.Url;
            m_VideoPlayer.url = new Uri(videoPath).AbsoluteUri;
            m_VideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            m_VideoPlayer.isLooping = true;
            m_VideoPlayer.prepareCompleted += OnVideoPrepared;
            m_VideoPlayer.errorReceived += (vp, msg) => Debug.LogError("[ViewVideoQR] VideoPlayer: " + msg);
            m_VideoPlayer.Prepare();
        }

        private void OnVideoPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnVideoPrepared;
            vp.Play();
        }

        private IEnumerator UploadVideo(string videoPath)
        {
            byte[] videoBytes = File.ReadAllBytes(videoPath);

            WWWForm form = new WWWForm();
            form.AddField("isFileIdentify", "false");
            form.AddField("identify", "false");
            form.AddField("url_redirect", m_RedirectLink);
            form.AddBinaryData("file", videoBytes, "capture.mp4", "video/mp4");

            string fullUrl = m_ServerUrl + m_UploadEndpoint;
            Debug.Log("[ViewVideoQR] Enviando vídeo para: " + fullUrl);

            using (UnityWebRequest req = UnityWebRequest.Post(fullUrl, form))
            {
                req.SetRequestHeader("Authorization", "Bearer " + m_Token);
                req.SetRequestHeader("access_token", m_AccessToken);

                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                    Debug.LogError($"[ViewVideoQR] Erro no upload: {req.error} | {req.downloadHandler?.text}");
                else
                {
                    Debug.Log("[ViewVideoQR] Upload concluído: " + req.downloadHandler.text);
                    ParseAndShowQRCode(req.downloadHandler.text);
                }
            }
        }

        private void ParseAndShowQRCode(string json)
        {
            QRCodeData data = JsonUtility.FromJson<QRCodeData>(json);
            if (data == null || string.IsNullOrEmpty(data.qrcode))
            {
                Debug.LogError("[ViewVideoQR] QR code não encontrado na resposta.");
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
