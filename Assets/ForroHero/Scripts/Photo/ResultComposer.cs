using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ForroHero.Scripts.Photo
{
    /// <summary>
    /// Compõe a imagem final: foto da webcam + moldura (+ textos opcionais).
    /// A foto é colocada CRUA no PhotoTarget; é a RawImage do PhotoTarget que cuida da
    /// exibição (tamanho/rotação). Configure o PhotoTarget IGUAL à RawImage do preview
    /// (mesmo RectTransform: tamanho + rotação Z) para o resultado bater com o preview.
    /// Uma câmera dedicada renderiza o canvas de composição (com a moldura por cima) para
    /// um RenderTexture sRGB e salva PNG. Portado do Pippos (Encaixe.Capture).
    /// </summary>
    public class ResultComposer : MonoBehaviour
    {
        [Header("Composição")]
        [Tooltip("Câmera dedicada que enxerga apenas o canvas de composição (foto + moldura).")]
        [SerializeField] private Camera m_RenderCamera;
        [Tooltip("RawImage que recebe a foto. Configure-a IGUAL à RawImage do preview.")]
        [SerializeField] private RawImage m_PhotoTarget;
        [Tooltip("Opcional — só se quiser texto na imagem. Para 'só moldura', deixe vazio.")]
        [SerializeField] private TMP_Text m_NameText;
        [SerializeField] private TMP_Text m_ScoreText;

        [Header("Recorte arredondado da foto (opcional)")]
        [Tooltip("Raio dos cantos da foto, em px. 0 = sem arredondar (foto crua, igual ao preview).")]
        [SerializeField] private float m_PhotoCornerRadius = 0f;

        [Header("Saída")]
        [SerializeField] private Vector2Int m_OutputSize = new Vector2Int(1080, 1920);
        [SerializeField] private string m_FileName = "final.png";

        [Header("Formato dos textos")]
        [SerializeField] private string m_NameFormat = "{0}";
        [SerializeField] private string m_ScoreFormat = "{0}%";

        private Texture2D m_LastRounded;

        /// <summary>Monta a imagem final e salva PNG. Devolve a textura (para preview) e o caminho.</summary>
        public Texture2D Compose(Texture2D photo, string playerName, int score, out string path)
        {
            path = "";

            if (m_RenderCamera == null)
            {
                Debug.LogError("[ForroFoto] ResultComposer: RenderCamera não atribuída.");
                return null;
            }

            if (m_PhotoTarget != null && photo != null)
                m_PhotoTarget.texture = m_PhotoCornerRadius > 0f ? RoundedCopy(photo) : photo;

            if (m_NameText != null) m_NameText.text = string.Format(m_NameFormat, playerName);
            if (m_ScoreText != null) m_ScoreText.text = string.Format(m_ScoreFormat, score);

            Canvas.ForceUpdateCanvases();

            // Render do canvas de composição. sRGB explícito evita imagem escura em Linear.
            RenderTexture rt = RenderTexture.GetTemporary(m_OutputSize.x, m_OutputSize.y, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            RenderTexture previousTarget = m_RenderCamera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;

            m_RenderCamera.targetTexture = rt;
            m_RenderCamera.Render();

            RenderTexture.active = rt;
            Texture2D result = new Texture2D(m_OutputSize.x, m_OutputSize.y, TextureFormat.RGB24, false);
            result.ReadPixels(new Rect(0, 0, m_OutputSize.x, m_OutputSize.y), 0, 0);
            result.Apply();

            RenderTexture.active = previousActive;
            m_RenderCamera.targetTexture = previousTarget;
            RenderTexture.ReleaseTemporary(rt);

            path = PhotoUtils.SavePng(result, m_FileName);
            return result;
        }

        /// <summary>Cópia da foto com cantos arredondados (no espaço da própria textura).</summary>
        private Texture2D RoundedCopy(Texture2D src)
        {
            Texture2D copy = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
            copy.SetPixels32(src.GetPixels32());

            int w = copy.width, h = copy.height;
            float r = Mathf.Min(m_PhotoCornerRadius, Mathf.Min(w, h) * 0.5f);
            Color32[] px = copy.GetPixels32();
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float ccx = Mathf.Clamp(x, r, w - 1 - r);
                    float ccy = Mathf.Clamp(y, r, h - 1 - r);
                    float dx = x - ccx, dy = y - ccy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(r - dist + 0.5f);
                    int idx = y * w + x;
                    if (a < 1f) px[idx].a = (byte)(px[idx].a * a);
                }
            }
            copy.SetPixels32(px);
            copy.Apply();

            if (m_LastRounded != null) Destroy(m_LastRounded);
            m_LastRounded = copy;
            return copy;
        }
    }
}
