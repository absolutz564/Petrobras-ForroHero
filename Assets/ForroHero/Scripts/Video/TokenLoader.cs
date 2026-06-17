using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ForroHero.Scripts.Video
{
    /// <summary>
    /// Busca as credenciais (bearer + access_token) na API e salva no PlayerPrefs,
    /// para o upload do vídeo (ViewVideoQR) usar depois. Portado do SempreLivre.
    /// Deve rodar no boot (ex.: junto da tela de Loading), antes do upload.
    /// </summary>
    public class TokenLoader : MonoBehaviour
    {
        [Header("API Settings")]
        [SerializeField] private string apiUrl = "https://api.tmj.dilis.com.br";
        [SerializeField] private string credentialsEndpoint = "/agent/unity/credentials";
        [SerializeField] private string masterKey = "Dy4APyw0CFHSnwrUSW1QrCV27zFetyQ2";

        [Header("Identificação do Booth (trocar pela chave do Forró Hero)")]
        [SerializeField] private string activationKey = "JOAO_DE_TAL";

        [Header("Credenciais Retornadas (read-only)")]
        public string token;        // bearer
        public string accessToken;  // access_token
        public string experienceName;
        public string expiresAt;

        private void Start()
        {
            StartCoroutine(LoadCredentialsFromApi());
        }

        private IEnumerator LoadCredentialsFromApi()
        {
            string url = apiUrl + credentialsEndpoint;
            string body = JsonUtility.ToJson(new CredentialsRequest { activation_key = activationKey });
            byte[] bodyBytes = Encoding.UTF8.GetBytes(body);

            using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(bodyBytes);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("x-unity-master-key", masterKey);

                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[TokenLoader] Erro ao buscar credenciais: {req.error} | {req.downloadHandler?.text}");
                    yield break;
                }

                try
                {
                    CredentialsResponse data = JsonUtility.FromJson<CredentialsResponse>(req.downloadHandler.text);

                    token = data.bearer;
                    accessToken = data.access_token;
                    expiresAt = data.expires_at;
                    experienceName = data.experience != null ? data.experience.name : null;

                    PlayerPrefs.SetString("token", token ?? "");
                    PlayerPrefs.SetString("accessToken", accessToken ?? "");
                    PlayerPrefs.Save();

                    Debug.Log($"[TokenLoader] Credenciais salvas. Experience: {experienceName} | Expira: {expiresAt}");
                }
                catch (Exception ex)
                {
                    Debug.LogError("[TokenLoader] Erro ao parsear resposta: " + ex.Message + "\n" + req.downloadHandler.text);
                }
            }
        }

        [Serializable] private class CredentialsRequest { public string activation_key; }

        [Serializable]
        private class CredentialsResponse
        {
            public ExperienceInfo experience;
            public string access_token;
            public string bearer;
            public string expires_at;
        }

        [Serializable] private class ExperienceInfo { public string id; public string name; public string logo; }
    }
}
