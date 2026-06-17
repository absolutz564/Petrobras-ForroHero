using UnityEngine;
using UnityEngine.UI;
using Nexweron.WebCamPlayer;

namespace ForroHero.Scripts.Video
{
    /// <summary>
    /// Coloca a textura AO VIVO da webcam (WebCamStream.webCamTexture) numa RawImage.
    /// A textura da webcam só existe em runtime, por isso não dá pra ligar no Inspector —
    /// este componente faz a atribuição assim que ela fica disponível.
    /// Coloque no mesmo GameObject da RawImage da webcam (no CanvasCapture).
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class WebcamDisplay : MonoBehaviour
    {
        [SerializeField] private WebCamStream m_WebCamStream;

        private RawImage m_RawImage;

        private void Awake()
        {
            m_RawImage = GetComponent<RawImage>();
        }

        private void Update()
        {
            if (m_WebCamStream == null) return;

            var tex = m_WebCamStream.webCamTexture;
            if (tex != null && m_RawImage.texture != tex)
                m_RawImage.texture = tex;
        }
    }
}
