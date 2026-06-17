using UnityEngine;
using UnityEngine.UI;

namespace ForroHero.Scripts.Video
{
    /// <summary>
    /// Gira RawImages em passos de 90° (tecla configurável), persistindo a rotação
    /// no PlayerPrefs — útil para corrigir a orientação da webcam por totem.
    /// Portado do SempreLivre.
    /// </summary>
    public class RawImageRotator : MonoBehaviour
    {
        [Header("Imagens a rotacionar")]
        public RawImage[] targets;

        [Header("Configuração")]
        public KeyCode rotateKey = KeyCode.R;
        public float angleStep = 90f;

        private const string PrefKey = "RawImageRotator_Steps";

        private void Start()
        {
            int steps = PlayerPrefs.GetInt(PrefKey, 0);
            Debug.Log($"[RawImageRotator] Aplicando rotação salva: {steps * angleStep}° ({steps} passo(s))");

            for (int i = 0; i < steps; i++)
                RotateAll();
        }

        private void Update()
        {
            if (Input.GetKeyDown(rotateKey))
            {
                RotateAll();

                int steps = (PlayerPrefs.GetInt(PrefKey, 0) + 1) % 4;
                PlayerPrefs.SetInt(PrefKey, steps);
                PlayerPrefs.Save();

                Debug.Log($"[RawImageRotator] Rotação atualizada: {steps * angleStep}° ({steps} passo(s)) — salvo no PlayerPrefs");
            }
        }

        public void RotateAll()
        {
            if (targets == null) return;
            foreach (var img in targets)
            {
                if (img == null) continue;
                RotateOne(img.rectTransform);
            }
        }

        private void RotateOne(RectTransform rt)
        {
            var euler = rt.localEulerAngles;
            euler.z = (euler.z + angleStep) % 360f;
            rt.localEulerAngles = euler;

            var size = rt.sizeDelta;
            rt.sizeDelta = new Vector2(size.y, size.x);
        }

        // Útil para resetar a config durante testes (ex.: chamar de um botão de "Reset").
        public void ResetRotation()
        {
            PlayerPrefs.DeleteKey(PrefKey);
            PlayerPrefs.Save();
            Debug.Log("[RawImageRotator] Rotação resetada no PlayerPrefs — reinicie a cena para aplicar.");
        }
    }
}
