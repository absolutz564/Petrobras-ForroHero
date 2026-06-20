using UnityEngine;

namespace ForroHero.Scripts.Photo
{
    /// <summary>
    /// Estado compartilhado da foto entre a View de captura (ViewFoto) e a View do QR.
    /// Estático e leve — a foto final (com moldura) e o caminho do PNG salvo.
    /// </summary>
    public static class PhotoSession
    {
        public static Texture2D FinalImage;
        public static string FinalImagePath = "";

        public static void Reset()
        {
            FinalImage = null;
            FinalImagePath = "";
        }
    }
}
