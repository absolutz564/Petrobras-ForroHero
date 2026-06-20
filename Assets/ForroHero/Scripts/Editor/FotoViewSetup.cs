using ForroHero.Scripts.Managers;
using ForroHero.Scripts.Photo;
using ForroHero.Scripts.UI;
using ForroHero.Scripts.Utils;
using ForroHero.Scripts.Video;
using Nexweron.WebCamPlayer;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ForroHero.EditorTools
{
    /// <summary>
    /// Monta a "View - Foto" completa na cena: preview da webcam (WebcamDisplay) + botão
    /// Capturar + texto de countdown + rig de composição (câmera dedicada + PhotoTarget +
    /// moldura), liga todas as referências da ViewFoto, registra no ViewManager e atribui
    /// a moldura.png. Espelha o EncaixeFlowSetup do Pippos, adaptado ao ViewManager.
    /// </summary>
    public static class FotoViewSetup
    {
        private const string MolduraPath = "Assets/ForroHero/Clients/UniFacisa/Sprites/moldura.png";
        private static readonly Vector2 Output = new Vector2(1080f, 1920f);
        private static readonly Vector3 IsolatedPos = new Vector3(10000f, 0f, 0f);
        private const float WorldScale = 0.01f; // 100 px por unidade

        [MenuItem("Forró Hero/Montar View - Foto")]
        public static void Build()
        {
            ViewManager vm = Object.FindFirstObjectByType<ViewManager>(FindObjectsInactive.Include);
            if (vm == null)
            {
                EditorUtility.DisplayDialog("Forró Hero",
                    "ViewManager não encontrado na cena. Abra a cena Game antes de rodar.", "OK");
                return;
            }

            // Pai = mesmo container das outras views (uma View qualquer da cena).
            View anyView = Object.FindFirstObjectByType<View>(FindObjectsInactive.Include);
            Transform parent = anyView != null ? anyView.transform.parent : vm.transform;

            // --- Raiz da View - Foto (RectTransform + CanvasGroup + ViewFoto) ---
            GameObject root = new GameObject("View - Foto", typeof(RectTransform), typeof(CanvasGroup));
            Undo.RegisterCreatedObjectUndo(root, "Montar View - Foto");
            root.transform.SetParent(parent, false);
            Stretch(root.GetComponent<RectTransform>());
            root.GetComponent<CanvasGroup>().alpha = 1f; // ViewManager zera em runtime

            ViewFoto viewFoto = root.AddComponent<ViewFoto>();

            // --- Preview da webcam (a foto é capturada DESTA RawImage) ---
            RawImage cameraView = CreateRawImage("WebcamPreview", root.transform,
                new Vector2(810f, 1440f), new Vector2(0f, 90f), Color.white);
            WebcamDisplay display = cameraView.gameObject.AddComponent<WebcamDisplay>();

            // WebCamStream: reusa o que já existe na cena, ou cria um novo.
            WebCamStream cam = Object.FindFirstObjectByType<WebCamStream>(FindObjectsInactive.Include);
            if (cam == null)
            {
                GameObject camGO = new GameObject("WebCamStream");
                Undo.RegisterCreatedObjectUndo(camGO, "Montar View - Foto");
                camGO.transform.SetParent(root.transform, false);
                cam = camGO.AddComponent<WebCamStream>();
            }
            SetRef(display, "m_WebCamStream", cam);

            // --- Countdown (grande, por cima do preview) ---
            TMP_Text countdown = CreateText("Countdown", root.transform, "",
                300f, new Vector2(0f, 90f), new Vector2(800f, 800f));

            // --- Botão Capturar ---
            Button captureBtn = CreateButton("BotaoCapturar", root.transform, "CAPTURAR",
                new Vector2(560f, 170f), new Vector2(0f, -800f));

            // --- Rig de composição (foto + moldura) ---
            Camera composeCam = CreateCompositionCamera();
            ResultComposer composer = CreateCompositionCanvas(composeCam);

            // --- Liga a ViewFoto ---
            SetRef(viewFoto, "m_WebCamStream", cam);
            SetRef(viewFoto, "m_CameraView", cameraView);
            SetRef(viewFoto, "m_CaptureButton", captureBtn);
            SetRef(viewFoto, "m_CountdownText", countdown);
            SetRef(viewFoto, "m_Composer", composer);

            // --- Registra no ViewManager (Name = Foto) ---
            RegisterInViewManager(vm, viewFoto);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = root;

            Debug.Log(
                "[Forró Hero] View - Foto montada e registrada no ViewManager.\n" +
                "  • Ajuste posição/tamanho do WebcamPreview e do botão como quiser.\n" +
                "  • O PhotoTarget (no CompositionCanvas) já está no mesmo aspecto do preview;\n" +
                "    se rotacionar o preview, rotacione o PhotoTarget igual.\n" +
                "  • Confira se a moldura.png foi atribuída (senão arraste na RawImage 'Moldura').\n" +
                "  • Ligue a RawImage da foto final em ViewVideoQR.m_PhotoDisplay (na View - QR).");
        }

        // ------------------------------------------------------------------ Composição

        private static Camera CreateCompositionCamera()
        {
            GameObject camGO = new GameObject("CompositionCamera", typeof(Camera));
            Undo.RegisterCreatedObjectUndo(camGO, "Montar View - Foto");

            Camera cam = camGO.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = Output.y * WorldScale / 2f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            cam.enabled = false; // renderizada manualmente pelo ResultComposer

            camGO.transform.position = IsolatedPos + new Vector3(0f, 0f, -10f);
            return cam;
        }

        private static ResultComposer CreateCompositionCanvas(Camera composeCam)
        {
            GameObject canvasGO = new GameObject("CompositionCanvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasGO, "Montar View - Foto");

            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = composeCam;

            RectTransform crt = canvasGO.GetComponent<RectTransform>();
            crt.sizeDelta = Output;
            canvasGO.transform.position = IsolatedPos;
            canvasGO.transform.localScale = Vector3.one * WorldScale;

            // Foto crua (preenche tudo) — DEVE espelhar o RectTransform do preview.
            RawImage photo = CreateRawImage("PhotoTarget", canvasGO.transform, Output, Vector2.zero, Color.white);
            Stretch(photo.rectTransform);

            // Moldura por cima.
            RawImage moldura = CreateRawImage("Moldura", canvasGO.transform, Output, Vector2.zero, Color.white);
            Stretch(moldura.rectTransform);
            Texture2D molduraTex = AssetDatabase.LoadAssetAtPath<Texture2D>(MolduraPath);
            if (molduraTex != null) moldura.texture = molduraTex;
            else Debug.LogWarning("[Forró Hero] moldura.png não encontrada em " + MolduraPath + " — atribua na mão.");

            ResultComposer composer = canvasGO.AddComponent<ResultComposer>();
            SetRef(composer, "m_RenderCamera", composeCam);
            SetRef(composer, "m_PhotoTarget", photo);
            return composer;
        }

        // ------------------------------------------------------------------ ViewManager

        private static void RegisterInViewManager(ViewManager vm, View view)
        {
            SerializedObject so = new SerializedObject(vm);
            SerializedProperty list = so.FindProperty("m_Views");
            if (list == null) { Debug.LogError("[Forró Hero] m_Views não encontrado no ViewManager."); return; }

            // Já existe entrada Foto? então só atualiza o prefab.
            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty e = list.GetArrayElementAtIndex(i);
                if (e.FindPropertyRelative("Name").enumValueIndex == (int)ViewName.Foto)
                {
                    e.FindPropertyRelative("Prefab").objectReferenceValue = view;
                    so.ApplyModifiedProperties();
                    return;
                }
            }

            list.arraySize++;
            SerializedProperty novo = list.GetArrayElementAtIndex(list.arraySize - 1);
            novo.FindPropertyRelative("Name").enumValueIndex = (int)ViewName.Foto;
            novo.FindPropertyRelative("Prefab").objectReferenceValue = view;
            so.ApplyModifiedProperties();
        }

        // ------------------------------------------------------------------ Helpers UI

        private static RawImage CreateRawImage(string name, Transform parent, Vector2 size, Vector2 pos, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            RawImage img = go.AddComponent<RawImage>();
            img.color = color;
            return img;
        }

        private static TMP_Text CreateText(string name, Transform parent, string text, float fontSize, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            return tmp;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 size, Vector2 pos)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            go.GetComponent<Image>().color = new Color(0.15f, 0.6f, 0.2f, 1f);

            TMP_Text txt = CreateText("Texto", go.transform, label, 60f, Vector2.zero, size);
            txt.color = Color.white;

            return go.GetComponent<Button>();
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SetRef(Object target, string field, Object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(field);
            if (prop != null) prop.objectReferenceValue = value;
            else Debug.LogWarning("[Forró Hero] campo não encontrado: " + field + " em " + target.GetType().Name);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
