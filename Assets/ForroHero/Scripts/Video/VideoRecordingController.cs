using System.Collections;
using System.IO;
using UnityEngine;
using FfmpegUnity;
using Nexweron.WebCamPlayer;
using Dypsloom.Shared;
using Dypsloom.RhythmTimeline.Core.Managers;

namespace ForroHero.Scripts.Video
{
    /// <summary>
    /// Grava a webcam do jogador (com moldura) durante a música inteira.
    /// Inicia a captura FFmpeg em RhythmDirector.OnSongPlay e para em OnSongEnd.
    /// O vídeo final fica em persistentDataPath/capture.mp4 (configurado no FfmpegCaptureCommand).
    /// </summary>
    public class VideoRecordingController : MonoBehaviour
    {
        [Header("Webcam")]
        [SerializeField] private WebCamStream m_CameraStream;

        [Header("Moldura fixa (no canvas de captura)")]
        [SerializeField] private GameObject m_Moldura;

        [Header("FFmpeg")]
        [SerializeField] private FfmpegCaptureCommand m_FfmpegCapture;

        public static string VideoPath =>
            Path.Combine(Application.persistentDataPath, "capture.mp4").Replace('\\', '/');

        public static bool VideoReady { get; private set; }

        private RhythmDirector m_Director;
        private bool m_Recording;

        private void Start()
        {
            DeleteOldCapture();
            VideoReady = false;

            if (m_Moldura != null) m_Moldura.SetActive(true);
            // Play(true) => isAutoAuthorize: cria/autoriza a WebCamTexture na 1a chamada.
            if (m_CameraStream != null) m_CameraStream.Play(true);

            m_Director = Toolbox.Get<RhythmDirector>();
            if (m_Director != null)
            {
                m_Director.OnSongPlay += HandleSongPlay;
                m_Director.OnSongEnd += HandleSongEnd;
            }
            else
            {
                Debug.LogError("[ForroVideo] RhythmDirector não encontrado (Toolbox). Gravação não vai iniciar.");
            }
        }

        private void OnDestroy()
        {
            if (m_Director != null)
            {
                m_Director.OnSongPlay -= HandleSongPlay;
                m_Director.OnSongEnd -= HandleSongEnd;
            }

            if (m_FfmpegCapture != null && m_FfmpegCapture.IsRunning) m_FfmpegCapture.StopFfmpeg();
            if (m_CameraStream != null) m_CameraStream.Stop();
        }

        private void HandleSongPlay()
        {
            if (m_Recording) return;
            StartCoroutine(StartCaptureNextFrame());
        }

        private IEnumerator StartCaptureNextFrame()
        {
            // Reset por música: apaga o vídeo anterior e zera o flag, senão o QR da
            // próxima música pegaria o capture.mp4 antigo (Start() só roda 1x).
            DeleteOldCapture();
            VideoReady = false;

            // Após a 1ª música a textura da webcam pode ter sido liberada; garante que
            // ela esteja viva e produzindo frames, senão o FFmpeg grava tela azul.
            EnsureCameraPlaying();

            float timeout = 0f;
            while (!CameraReady() && timeout < 5f)
            {
                timeout += Time.deltaTime;
                yield return null;
            }

            yield return null; // 1 frame renderizado antes de capturar
            m_FfmpegCapture.StartFfmpeg();
            m_Recording = true;
            Debug.Log("[ForroVideo] Gravação iniciada. Opções: " + m_FfmpegCapture.CaptureOptions);
        }

        private void EnsureCameraPlaying()
        {
            if (m_CameraStream == null) return;
            var tex = m_CameraStream.webCamTexture;
            if (tex == null || !tex.isPlaying)
                m_CameraStream.Play(true); // recria/reautoriza a textura se preciso
        }

        private bool CameraReady()
        {
            if (m_CameraStream == null) return true;
            var tex = m_CameraStream.webCamTexture;
            // width <= 16 = placeholder; só está pronta quando entrega frames reais.
            return tex != null && tex.isPlaying && tex.width > 16;
        }

        private void HandleSongEnd()
        {
            if (!m_Recording) return;
            m_Recording = false;
            StartCoroutine(StopAndFinalize());
        }

        private IEnumerator StopAndFinalize()
        {
            Debug.Log("[ForroVideo] Parando gravação...");
            m_FfmpegCapture.StopFfmpeg();

            float timeout = 0f;
            while (m_FfmpegCapture.IsRunning && timeout < 15f)
            {
                timeout += Time.deltaTime;
                yield return null;
            }

            string path = VideoPath;
            while (!File.Exists(path)) yield return new WaitForSeconds(0.3f);

            bool ready = false;
            while (!ready)
            {
                bool locked = false;
                try
                {
                    using (File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None)) { }
                    ready = true;
                }
                catch (IOException) { locked = true; }
                if (!ready && locked) yield return new WaitForSeconds(0.5f);
            }

            // NÃO parar a câmera aqui: WebCamStream.Stop() destrói a textura e o Play()
            // só roda no Start() (1x). Manter viva garante a gravação da próxima música.
            VideoReady = true;
            Debug.Log($"[ForroVideo] Vídeo pronto ({new FileInfo(path).Length / 1024} KB) em {path}");
        }

        private void DeleteOldCapture()
        {
            string path = VideoPath;
            if (File.Exists(path))
            {
                try { File.Delete(path); }
                catch (IOException ex) { Debug.LogWarning("[ForroVideo] Não apagou capture anterior: " + ex.Message); }
            }
        }
    }
}
