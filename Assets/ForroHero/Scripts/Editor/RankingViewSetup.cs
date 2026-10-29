using ForroHero.Scripts.Ranking;
using ForroHero.Scripts.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ForroHero.EditorTools
{
    /// <summary>
    /// Monta a lista do ranking (top 10) dentro da View - Obrigado: um container com
    /// 10 linhas (posição + foto + pontuação + "Pontos"), liga cada RankingRow e
    /// preenche o m_Rows da ViewObrigado. Layout simples — restilize para bater com o mock.
    /// </summary>
    public static class RankingViewSetup
    {
        private const int Rows = 10;

        [MenuItem("Forró Hero/Montar Ranking (Obrigado)")]
        public static void Build()
        {
            ViewObrigado obrigado = Object.FindFirstObjectByType<ViewObrigado>(FindObjectsInactive.Include);
            if (obrigado == null)
            {
                EditorUtility.DisplayDialog("Forró Hero",
                    "ViewObrigado não encontrada na cena. Abra a cena Game antes de rodar.", "OK");
                return;
            }

            // Container do ranking (ocupa a metade de baixo da view).
            GameObject listGO = new GameObject("RankingList",
                typeof(RectTransform), typeof(VerticalLayoutGroup));
            Undo.RegisterCreatedObjectUndo(listGO, "Montar Ranking");
            listGO.transform.SetParent(obrigado.transform, false);

            RectTransform lrt = listGO.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0.08f, 0.02f);
            lrt.anchorMax = new Vector2(0.92f, 0.62f);
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = listGO.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;

            var rows = new RankingRow[Rows];
            for (int i = 0; i < Rows; i++)
                rows[i] = BuildRow(listGO.transform, i + 1);

            // Liga no m_Rows da ViewObrigado.
            SerializedObject so = new SerializedObject(obrigado);
            SerializedProperty list = so.FindProperty("m_Rows");
            list.arraySize = Rows;
            for (int i = 0; i < Rows; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = rows[i];
            so.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = listGO;

            Debug.Log("[Forró Hero] Ranking montado (10 linhas) e ligado na ViewObrigado.\n" +
                      "  • Restilize cores/fontes/posições para bater com o mock.\n" +
                      "  • Para a foto ficar circular, adicione um Mask + sprite circular na 'Foto'.");
        }

        private static RankingRow BuildRow(Transform parent, int rank)
        {
            GameObject row = new GameObject("Row " + rank,
                typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);

            HorizontalLayoutGroup h = row.GetComponent<HorizontalLayoutGroup>();
            h.spacing = 16f;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = true;
            h.childAlignment = TextAnchor.MiddleLeft;

            row.GetComponent<LayoutElement>().preferredHeight = 64f;

            TMP_Text rankText = CreateText("Rank", row.transform, rank.ToString(), 44f, TextAlignmentOptions.Center, 64f);
            RawImage photo = CreatePhoto("Foto", row.transform, 64f);
            TMP_Text scoreText = CreateText("Score", row.transform, "", 44f, TextAlignmentOptions.Right, 0f, flexible: true);
            CreateText("Pontos", row.transform, "Pontos", 26f, TextAlignmentOptions.Left, 90f);

            RankingRow rr = row.AddComponent<RankingRow>();
            SetRef(rr, "m_RankText", rankText);
            SetRef(rr, "m_Photo", photo);
            SetRef(rr, "m_ScoreText", scoreText);
            return rr;
        }

        // ------------------------------------------------------------------ helpers

        private static TMP_Text CreateText(string name, Transform parent, string text, float fontSize,
            TextAlignmentOptions align, float preferredWidth, bool flexible = false)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.enableWordWrapping = false;
            tmp.color = new Color(0.05f, 0.4f, 0.1f, 1f);

            LayoutElement le = go.GetComponent<LayoutElement>();
            if (flexible) le.flexibleWidth = 1f;
            else le.preferredWidth = preferredWidth;
            return tmp;
        }

        private static RawImage CreatePhoto(string name, Transform parent, float size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(RawImage), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<RawImage>().color = new Color(0.1f, 0.55f, 0.2f, 1f); // verde placeholder
            LayoutElement le = go.GetComponent<LayoutElement>();
            le.preferredWidth = size;
            le.preferredHeight = size;
            return go.GetComponent<RawImage>();
        }

        private static void SetRef(Object target, string field, Object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(field);
            if (prop != null) prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
