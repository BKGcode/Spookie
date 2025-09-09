
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace Brainamics.UI.Editor
{
    public static class UIPrototypesGenerator
    {
        private const string PrefabPath = "Assets/_Game/Prefabs/UI/Generated_Prototypes";

        [MenuItem("Tools/Brainamics/Generate UI Prototype Prefabs")]
        public static void GeneratePrefabs()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Prefabs/UI"))
            {
                AssetDatabase.CreateFolder("Assets/_Game/Prefabs", "UI");
            }
            if (!AssetDatabase.IsValidFolder(PrefabPath))
            {
                AssetDatabase.CreateFolder("Assets/_Game/Prefabs/UI", "Generated_Prototypes");
            }

            CreatePanelPrefab();
            CreateTextPrefab();
            CreateImagePrefab();
            CreateButtonPrefab();
            CreateSliderPrefab();
            CreateTogglePrefab();
            CreateCurrencyDisplayPrefab();
            CreateHeaderPrefab();
            CreateFooterPrefab();
            CreateListItemPrefab();
            CreateNotificationBadgePrefab();
            CreateScrollViewVerticalPrefab();
            CreateTabViewPrefab();
            CreateModalWindowPrefab();

            Debug.Log("[UIPrototypesGenerator] Librería de prefabs de UI generada en: " + PrefabPath);
        }

        // Métodos auxiliares para crear cada prefab...
        // (Implementaciones detalladas para cada uno)

        private static void CreatePanelPrefab()
        {
            var go = new GameObject("P_UI_Panel");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 200);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            SavePrefab(go);
        }

        private static void CreateTextPrefab()
        {
            var go = new GameObject("P_UI_Text");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160, 30);
            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = "Sample Text";
            txt.fontSize = 24;
            txt.alignment = TextAlignmentOptions.Center;
            SavePrefab(go);
        }
        
        private static void CreateImagePrefab()
        {
            var go = new GameObject("P_UI_Image");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 100);
            go.AddComponent<Image>();
            SavePrefab(go);
        }

        private static void CreateButtonPrefab()
        {
            var go = new GameObject("P_UI_Button");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160, 60);
            var img = go.AddComponent<Image>();
            go.AddComponent<Button>();
            go.AddComponent<Prototypes.CustomButton>();

            var textGo = new GameObject("Text (TMP)");
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            var txt = textGo.AddComponent<TextMeshProUGUI>();
            txt.text = "Button";
            txt.color = Color.black;
            txt.alignment = TextAlignmentOptions.Center;

            SavePrefab(go);
        }

        private static void CreateSliderPrefab()
        {
            var go = new GameObject("P_UI_Slider");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 30);
            var slider = go.AddComponent<Slider>();

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one; bgRect.sizeDelta = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0,0,0,0.35f);

            // Fill Area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.offsetMin = new Vector2(10,0);
            fillAreaRect.offsetMax = new Vector2(-10,0);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0,0); fillRect.anchorMax = new Vector2(0,1); fillRect.sizeDelta = Vector2.zero;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.2f,0.6f,1f,0.9f);

            // Handle Slide Area
            var handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(go.transform, false);
            var handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = new Vector2(0,0); handleAreaRect.anchorMax = new Vector2(1,1); handleAreaRect.sizeDelta = Vector2.zero;

            var handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            var handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20,20);
            var handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;

            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;

            SavePrefab(go);
        }

        private static void CreateTogglePrefab()
        {
            var go = new GameObject("P_UI_Toggle");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160, 30);
            var toggle = go.AddComponent<Toggle>();

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0,0.5f); bgRect.anchorMax = new Vector2(0,0.5f);
            bgRect.sizeDelta = new Vector2(24,24);
            bgRect.anchoredPosition = new Vector2(12,0);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0,0,0,0.5f);

            // Checkmark
            var ck = new GameObject("Checkmark");
            ck.transform.SetParent(bg.transform, false);
            var ckRect = ck.AddComponent<RectTransform>();
            ckRect.anchorMin = new Vector2(0.2f,0.2f); ckRect.anchorMax = new Vector2(0.8f,0.8f); ckRect.sizeDelta = Vector2.zero;
            var ckImg = ck.AddComponent<Image>();
            ckImg.color = new Color(0.2f,0.8f,0.2f,0.95f);

            // Label
            var label = new GameObject("Label");
            label.transform.SetParent(go.transform, false);
            var labelRect = label.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0,0); labelRect.anchorMax = new Vector2(1,1); labelRect.offsetMin = new Vector2(40,0); labelRect.offsetMax = new Vector2(0,0);
            var txt = label.AddComponent<TextMeshProUGUI>();
            txt.text = "Toggle";
            txt.alignment = TextAlignmentOptions.MidlineLeft;

            toggle.graphic = ckImg;
            toggle.targetGraphic = bgImg;
            toggle.isOn = true;

            SavePrefab(go);
        }

        private static void CreateCurrencyDisplayPrefab()
        {
            var go = new GameObject("P_UI_CurrencyDisplay");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(180, 40);

            var icon = new GameObject("Icon");
            icon.transform.SetParent(go.transform, false);
            var iconRect = icon.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0,0.5f); iconRect.anchorMax = new Vector2(0,0.5f); iconRect.sizeDelta = new Vector2(32,32); iconRect.anchoredPosition = new Vector2(20,0);
            var iconImg = icon.AddComponent<Image>();
            iconImg.color = new Color(1f,0.85f,0.2f,1f);

            var amount = new GameObject("Amount");
            amount.transform.SetParent(go.transform, false);
            var amountRect = amount.AddComponent<RectTransform>();
            amountRect.anchorMin = new Vector2(0,0); amountRect.anchorMax = new Vector2(1,1); amountRect.offsetMin = new Vector2(60,0); amountRect.offsetMax = new Vector2(0,0);
            var txt = amount.AddComponent<TextMeshProUGUI>();
            txt.text = "999";
            txt.fontSize = 28;
            txt.alignment = TextAlignmentOptions.MidlineLeft;

            SavePrefab(go);
        }

        private static void CreateHeaderPrefab()
        {
            var go = new GameObject("P_UI_Header");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(800, 80);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.08f,0.08f,0.1f,0.95f);

            var title = new GameObject("Title");
            title.transform.SetParent(go.transform, false);
            var tRect = title.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0,0); tRect.anchorMax = new Vector2(1,1); tRect.offsetMin = new Vector2(24,0); tRect.offsetMax = new Vector2(-24,0);
            var txt = title.AddComponent<TextMeshProUGUI>();
            txt.text = "HEADER";
            txt.fontSize = 42;
            txt.alignment = TextAlignmentOptions.MidlineLeft;

            SavePrefab(go);
        }

        private static void CreateFooterPrefab()
        {
            var go = new GameObject("P_UI_Footer");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(800, 50);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.08f,0.08f,0.1f,0.85f);

            var text = new GameObject("Info");
            text.transform.SetParent(go.transform, false);
            var tRect = text.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0,0); tRect.anchorMax = new Vector2(1,1); tRect.offsetMin = new Vector2(16,0); tRect.offsetMax = new Vector2(-16,0);
            var txt = text.AddComponent<TextMeshProUGUI>();
            txt.text = "Footer text";
            txt.fontSize = 20;
            txt.alignment = TextAlignmentOptions.MidlineRight;

            SavePrefab(go);
        }

        private static void CreateListItemPrefab()
        {
            var go = new GameObject("P_UI_ListItem");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 60);
            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.18f,0.18f,0.22f,0.9f);

            var icon = new GameObject("Icon");
            icon.transform.SetParent(go.transform, false);
            var iconRect = icon.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0,0.5f); iconRect.anchorMax = new Vector2(0,0.5f); iconRect.sizeDelta = new Vector2(40,40); iconRect.anchoredPosition = new Vector2(30,0);
            var iconImg = icon.AddComponent<Image>();
            iconImg.color = new Color(0.3f,0.7f,0.9f,1f);

            var label = new GameObject("Label");
            label.transform.SetParent(go.transform, false);
            var labelRect = label.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0,0); labelRect.anchorMax = new Vector2(1,1); labelRect.offsetMin = new Vector2(80,0); labelRect.offsetMax = new Vector2(-16,0);
            var txt = label.AddComponent<TextMeshProUGUI>();
            txt.text = "List Item";
            txt.alignment = TextAlignmentOptions.MidlineLeft;
            txt.fontSize = 26;

            SavePrefab(go);
        }

        private static void CreateNotificationBadgePrefab()
        {
            var go = new GameObject("P_UI_NotificationBadge");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(32, 32);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.9f,0.15f,0.15f,1f);
            var txtGo = new GameObject("Count");
            txtGo.transform.SetParent(go.transform, false);
            var tRect = txtGo.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero; tRect.anchorMax = Vector2.one; tRect.sizeDelta = Vector2.zero;
            var txt = txtGo.AddComponent<TextMeshProUGUI>();
            txt.text = "9";
            txt.alignment = TextAlignmentOptions.Center;
            txt.fontSize = 22;
            SavePrefab(go);
        }

        private static void CreateScrollViewVerticalPrefab()
        {
            var root = new GameObject("P_UI_ScrollViewVertical");
            var rect = root.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 400);
            var maskImg = root.AddComponent<Image>();
            maskImg.color = new Color(0,0,0,0.4f);
            var mask = root.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(root.transform, false);
            var contentRect = contentGO.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0,1); contentRect.anchorMax = new Vector2(1,1); contentRect.pivot = new Vector2(0.5f,1f);
            contentRect.sizeDelta = new Vector2(0,0);
            var layout = contentGO.AddComponent<VerticalLayoutGroup>();
            layout.childForceExpandWidth = true; layout.childForceExpandHeight = false; layout.spacing = 4;
            contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = root.AddComponent<ScrollRect>();
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.viewport = rect;

            SavePrefab(root);
        }

        private static void CreateTabViewPrefab()
        {
            var go = new GameObject("P_UI_TabView");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600, 80);
            var hLayout = go.AddComponent<HorizontalLayoutGroup>();
            hLayout.childForceExpandHeight = true; hLayout.childForceExpandWidth = true; hLayout.spacing = 4;

            for (int i = 0; i < 3; i++)
            {
                var tab = new GameObject("Tab_" + (i+1));
                tab.transform.SetParent(go.transform, false);
                var tRect = tab.AddComponent<RectTransform>();
                tRect.sizeDelta = new Vector2(0,0);
                var img = tab.AddComponent<Image>();
                img.color = new Color(0.15f + 0.1f*i,0.15f,0.2f + 0.1f*i,0.95f);
                var label = new GameObject("Label");
                label.transform.SetParent(tab.transform,false);
                var lRect = label.AddComponent<RectTransform>();
                lRect.anchorMin = Vector2.zero; lRect.anchorMax = Vector2.one; lRect.sizeDelta = Vector2.zero;
                var txt = label.AddComponent<TextMeshProUGUI>();
                txt.text = "TAB " + (i+1);
                txt.alignment = TextAlignmentOptions.Center;
                txt.fontSize = 30;
            }

            SavePrefab(go);
        }

        // ... y así sucesivamente para todos los prefabs de la lista ...

        private static void CreateModalWindowPrefab()
        {
            var go = new GameObject("P_UI_ModalWindow");
            go.AddComponent<RectTransform>(); // Full screen
            var bg = go.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);

            // ... crear panel, textos, botones como hijos ...

            go.AddComponent<Prototypes.ModalWindow>();
            go.SetActive(false);
            SavePrefab(go);
        }


        private static void SavePrefab(GameObject go)
        {
            // Asegurar ruta completa (crear directorios intermedios si faltan)
            EnsureFolderPath(PrefabPath);
            string path = PrefabPath + "/" + go.name + ".prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Undo.RegisterCreatedObjectUndo(prefab, "Create " + prefab.name);
        }

        private static void EnsureFolderPath(string fullPath)
        {
            // fullPath estilo Assets/AAA/BBB/CCC
            if (AssetDatabase.IsValidFolder(fullPath)) return;
            string[] parts = fullPath.Split('/');
            string accum = parts[0]; // Assets
            for (int i = 1; i < parts.Length; i++)
            {
                string next = accum + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(accum, parts[i]);
                }
                accum = next;
            }
        }
    }
}
