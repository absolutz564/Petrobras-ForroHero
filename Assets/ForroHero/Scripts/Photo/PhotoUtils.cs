using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace ForroHero.Scripts.Photo
{
    /// <summary>
    /// Captura a textura exibida numa RawImage (webcam) como Texture2D legível e salva PNG.
    /// Copia a textura COMO ESTÁ (mesma orientação/proporção do preview); a única
    /// transformação é passar por um RT sRGB, que mantém a cor correta em Linear Color Space.
    /// Portado do Pippos (Encaixe.Capture).
    /// </summary>
    public static class PhotoUtils
    {
        /// <summary>Captura a textura atual de uma RawImage como Texture2D (legível), sem alterar orientação.</summary>
        public static Texture2D CaptureRawImage(RawImage source)
        {
            if (source == null || source.texture == null)
            {
                Debug.LogError("[ForroFoto] PhotoUtils: RawImage/textura nula — nada para capturar.");
                return null;
            }

            Texture tex = source.texture;

            RenderTexture rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            RenderTexture previous = RenderTexture.active;

            Graphics.Blit(tex, rt);
            RenderTexture.active = rt;

            Texture2D result = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            result.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            return result;
        }

        public static string SavePng(Texture2D texture, string fileName)
        {
            if (texture == null) return "";

            byte[] png = texture.EncodeToPNG();
            string path = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllBytes(path, png);
            Debug.Log("[ForroFoto] Imagem salva em: " + path);
            return path;
        }
    }
}
