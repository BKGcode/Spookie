#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Spookie.EditorTools
{
    public class PrefabPackGenerator : EditorWindow
    {
        // Config
        private string prefabFolder = "Assets/Prefabs/Modules";

        // Required assets
        private DayNightSystem.DayNightConfig dayNightConfig;
        private DayNightSystem.FeedbackMessagesSO feedbackMessages;
        private PlayerController.PlayerSettingsSO playerSettings; // requerido si se incluye Player

        // Options
        private bool includeDayNightCore = true;
        private bool includeTransitions = true;
        private bool includeAudio = true;
        private bool includeMessageSystem = true;
        private bool includeHUD = true;
        private bool includeGameState = true;
        private bool includeSpawnPoint = true;
        private bool includePlayer = false; // opcional

        [MenuItem("Spookie/Tools/Prefab Pack Generator")] 
        public static void OpenWindow()
        {
            var window = GetWindow<PrefabPackGenerator>(true, "Prefab Pack Generator");
            window.minSize = new Vector2(460, 560);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Output", EditorStyles.boldLabel);
            prefabFolder = EditorGUILayout.TextField("Folder", prefabFolder);

            EditorGUILayout.Space();
            GUILayout.Label("Required Assets (per module)", EditorStyles.boldLabel);
            dayNightConfig = (DayNightSystem.DayNightConfig)EditorGUILayout.ObjectField("DayNightConfig", dayNightConfig, typeof(DayNightSystem.DayNightConfig), false);
            feedbackMessages = (DayNightSystem.FeedbackMessagesSO)EditorGUILayout.ObjectField("FeedbackMessagesSO", feedbackMessages, typeof(DayNightSystem.FeedbackMessagesSO), false);
            if (includePlayer)
            {
                playerSettings = (PlayerController.PlayerSettingsSO)EditorGUILayout.ObjectField("PlayerSettingsSO (Player)", playerSettings, typeof(PlayerController.PlayerSettingsSO), false);
            }

            EditorGUILayout.Space();
            GUILayout.Label("Modules to Generate (separate prefabs)", EditorStyles.boldLabel);
            includeDayNightCore = EditorGUILayout.ToggleLeft("DayNightSystem Core", includeDayNightCore);
            includeTransitions = EditorGUILayout.ToggleLeft("Transitions (Visual+Audio)", includeTransitions);
            includeAudio = EditorGUILayout.ToggleLeft("Audio Manager", includeAudio);
            includeMessageSystem = EditorGUILayout.ToggleLeft("Message System (Canvas + TMP)", includeMessageSystem);
            includeHUD = EditorGUILayout.ToggleLeft("Day/Night HUD (Canvas + TMP + Icons)", includeHUD);
            includeGameState = EditorGUILayout.ToggleLeft("Game State (GameStateManager + Messages + InputGate)", includeGameState);
            includeSpawnPoint = EditorGUILayout.ToggleLeft("SpawnPoint", includeSpawnPoint);
            includePlayer = EditorGUILayout.ToggleLeft("Player (CharacterController + Movement/Look/Interaction)", includePlayer);

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(!AnySelected()))
            {
                if (GUILayout.Button("Generate Prefabs"))
                {
                    CreatePrefabs();
                }
            }

            if (!AnySelected())
            {
                var msg = "Selecciona al menos un módulo para generar.";
                EditorGUILayout.HelpBox(msg, MessageType.Warning);
            }
        }

        private bool AnySelected()
        {
            if (string.IsNullOrWhiteSpace(prefabFolder)) return false;
            return includeDayNightCore || includeTransitions || includeAudio || includeMessageSystem || includeHUD || includeGameState || includeSpawnPoint || includePlayer;
        }

        private void CreatePrefabs()
        {
            // Ensure folder
            if (!AssetDatabase.IsValidFolder(prefabFolder))
            {
                var parent = "Assets";
                foreach (var part in prefabFolder.Replace("\\", "/").Split('/'))
                {
                    if (string.IsNullOrEmpty(part) || part == "Assets") continue;
                    var path = parent + "/" + part;
                    if (!AssetDatabase.IsValidFolder(path))
                    {
                        AssetDatabase.CreateFolder(parent, part);
                    }
                    parent = path;
                }
            }
            System.Text.StringBuilder missingLinks = new System.Text.StringBuilder();
            System.Text.StringBuilder skipped = new System.Text.StringBuilder();

            // DayNight Core
            if (includeDayNightCore)
            {
                if (dayNightConfig == null)
                {
                    skipped.AppendLine("- DayNightSystem Core: falta DayNightConfig");
                }
                else
                {
                    var go = BuildDayNightCore(null);
                    SetSerializedObjectReference(go.GetComponent<DayNightSystem.Core.DayNightManager>(), "config", dayNightConfig);
                    SaveAndDispose(go, "DayNightSystem_Core");
                }
            }

            // Transitions
            if (includeTransitions)
            {
                var go = BuildTransitions(null, null);
                SaveAndDispose(go, "TransitionSystem");
                missingLinks.AppendLine("- TransitionSystem: asigna AudioManager a AudioTransitionController/TransitionManager (opcional)");
            }

            // Audio
            if (includeAudio)
            {
                var go = BuildAudio(null);
                SaveAndDispose(go, "AudioManager");
            }

            // MessageSystem
            if (includeMessageSystem)
            {
                var go = BuildMessageSystem(null, null);
                SaveAndDispose(go, "MessageSystem");
                missingLinks.AppendLine("- MessageSystem: asigna AudioManager (opcional) en el componente MessageSystem");
            }

            // HUD
            if (includeHUD)
            {
                if (feedbackMessages == null)
                {
                    skipped.AppendLine("- UI_DayNightHUD: falta FeedbackMessagesSO");
                }
                else
                {
                    var go = BuildHud(null, feedbackMessages);
                    SaveAndDispose(go, "UI_DayNightHUD");
                    missingLinks.AppendLine("- UI_DayNightHUD: asigna AudioManager en DayNightUIManager (opcional). Asegura que DayNight Core exista en la escena");
                }
            }

            // GameState
            if (includeGameState)
            {
                if (feedbackMessages == null)
                {
                    skipped.AppendLine("- GameState: falta FeedbackMessagesSO");
                }
                else
                {
                    var go = BuildGameState(null, feedbackMessages);
                    SaveAndDispose(go, "GameState");
                }
            }

            // SpawnPoint
            if (includeSpawnPoint)
            {
                var go = BuildSpawnPoint(null);
                SaveAndDispose(go, "SpawnPoint");
            }

            // Player
            if (includePlayer)
            {
                if (playerSettings == null)
                {
                    skipped.AppendLine("- Player: falta PlayerSettingsSO");
                }
                else
                {
                    var go = BuildPlayer(null);
                    SaveAndDispose(go, "Player");
                }
            }

            // Summary
            if (skipped.Length > 0)
            {
                Debug.LogWarning("[PrefabPackGenerator] Prefabs omitidos por requisitos no cubiertos:\n" + skipped.ToString());
            }
            if (missingLinks.Length > 0)
            {
                Debug.Log("[PrefabPackGenerator] Vínculos a completar manualmente en escena:\n" + missingLinks.ToString());
            }
            Debug.Log($"[PrefabPackGenerator] Generación de prefabs completada en {prefabFolder}");
        }

        private GameObject BuildDayNightCore(Transform parent)
        {
            var go = new GameObject("DayNightSystem_Modular");
            if (parent != null) go.transform.SetParent(parent);

            var coreManager = go.AddComponent<DayNightSystem.Core.DayNightManager>();
            go.AddComponent<DayNightSystem.Core.DayNightTimeController>();
            go.AddComponent<DayNightSystem.Core.DayNightStateController>();
            go.AddComponent<DayNightSystem.Core.DayNightEventController>();
            go.AddComponent<DayNightSystem.Validation.DayNightValidationController>();
            go.AddComponent<DayNightSystem.Persistence.DayNightPersistenceController>();

            Undo.RegisterCreatedObjectUndo(go, "Create DayNight Core");
            return go;
        }

        private GameObject BuildTransitions(Transform parent, GameObject audio)
        {
            var go = new GameObject("TransitionSystem");
            if (parent != null) go.transform.SetParent(parent);

            var tm = go.AddComponent<TransitionSystem.Core.TransitionManager>();
            var visual = go.AddComponent<TransitionSystem.Visual.VisualTransitionController>();
            var audioCtrl = go.AddComponent<TransitionSystem.Audio.AudioTransitionController>();
            var sleep = go.AddComponent<TransitionSystem.Specific.SleepTransitionController>();
            var faint = go.AddComponent<TransitionSystem.Specific.FaintTransitionController>();
            var eventsCtrl = go.AddComponent<TransitionSystem.Events.TransitionEventController>();

            // Create Fade Canvas child
            var fadeCanvasGO = new GameObject("FadeCanvas");
            fadeCanvasGO.transform.SetParent(go.transform);
            var canvasGroup = fadeCanvasGO.AddComponent<CanvasGroup>();
            // Link fade canvas
            visual.SetFadeCanvas(canvasGroup);
            tm.SetFadeCanvas(canvasGroup);

            // Link audio manager if exists
            if (audio != null)
            {
                var am = audio.GetComponent<DayNightSystem.AudioManager>();
                if (am != null)
                {
                    audioCtrl.SetAudioManager(am);
                    tm.SetAudioManager(am);
                }
            }

            Undo.RegisterCreatedObjectUndo(go, "Create Transition System");
            return go;
        }

        private GameObject BuildAudio(Transform parent)
        {
            var go = new GameObject("Audio");
            if (parent != null) go.transform.SetParent(parent);
            go.AddComponent<DayNightSystem.AudioManager>();
            Undo.RegisterCreatedObjectUndo(go, "Create Audio Manager");
            return go;
        }

        private GameObject BuildMessageSystem(Transform parent, GameObject audio)
        {
            var root = new GameObject("MessageSystem");
            if (parent != null) root.transform.SetParent(parent);

            // Canvas root
            var canvasGO = new GameObject("MessageCanvas");
            canvasGO.transform.SetParent(root.transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            var canvasGroup = canvasGO.AddComponent<CanvasGroup>();

            // Text
            var textGO = new GameObject("MessageText");
            textGO.transform.SetParent(canvasGO.transform);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 32f;
            var rect = tmp.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.1f);
            rect.anchorMax = new Vector2(0.5f, 0.1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(900, 120);

            // Component
            var ms = canvasGO.AddComponent<DayNightSystem.MessageSystem>();
            SetSerializedObjectReference(ms, "messageText", tmp);
            SetSerializedObjectReference(ms, "messageCanvas", canvasGroup);
            if (audio != null)
            {
                SetSerializedObjectReference(ms, "audioManager", audio.GetComponent<DayNightSystem.AudioManager>());
            }

            Undo.RegisterCreatedObjectUndo(root, "Create Message System");
            return root;
        }

        private GameObject BuildHud(Transform parent, DayNightSystem.FeedbackMessagesSO messages)
        {
            var hudRoot = new GameObject("DayNightHUD");
            if (parent != null) hudRoot.transform.SetParent(parent);

            // Canvas
            var canvasGO = new GameObject("HUDCanvas");
            canvasGO.transform.SetParent(hudRoot.transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // UI elements
            var timeGO = new GameObject("TimeText");
            timeGO.transform.SetParent(canvasGO.transform);
            var timeTMP = timeGO.AddComponent<TextMeshProUGUI>();
            timeTMP.text = "00:00";
            timeTMP.fontSize = 28;
            timeTMP.alignment = TextAlignmentOptions.TopRight;
            var timeRect = timeTMP.GetComponent<RectTransform>();
            timeRect.anchorMin = new Vector2(1, 1);
            timeRect.anchorMax = new Vector2(1, 1);
            timeRect.pivot = new Vector2(1, 1);
            timeRect.anchoredPosition = new Vector2(-20, -20);
            timeRect.sizeDelta = new Vector2(240, 60);

            var statusGO = new GameObject("StatusText");
            statusGO.transform.SetParent(canvasGO.transform);
            var statusTMP = statusGO.AddComponent<TextMeshProUGUI>();
            statusTMP.text = "";
            statusTMP.fontSize = 24;
            statusTMP.alignment = TextAlignmentOptions.TopLeft;
            var statusRect = statusTMP.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 1);
            statusRect.anchorMax = new Vector2(0, 1);
            statusRect.pivot = new Vector2(0, 1);
            statusRect.anchoredPosition = new Vector2(20, -20);
            statusRect.sizeDelta = new Vector2(720, 80);

            var warningGO = new GameObject("WarningIcon");
            warningGO.transform.SetParent(canvasGO.transform);
            var warningImg = warningGO.AddComponent<Image>();
            warningImg.color = Color.red;
            var warnRect = warningImg.GetComponent<RectTransform>();
            warnRect.anchorMin = new Vector2(0.5f, 0.9f);
            warnRect.anchorMax = new Vector2(0.5f, 0.9f);
            warnRect.pivot = new Vector2(0.5f, 0.5f);
            warnRect.anchoredPosition = Vector2.zero;
            warnRect.sizeDelta = new Vector2(24, 24);
            warningImg.gameObject.SetActive(false);

            var proxGO = new GameObject("ProximityIcon");
            proxGO.transform.SetParent(canvasGO.transform);
            var proxImg = proxGO.AddComponent<Image>();
            proxImg.color = Color.green;
            var proxRect = proxImg.GetComponent<RectTransform>();
            proxRect.anchorMin = new Vector2(0.5f, 0.85f);
            proxRect.anchorMax = new Vector2(0.5f, 0.85f);
            proxRect.pivot = new Vector2(0.5f, 0.5f);
            proxRect.anchoredPosition = new Vector2(0, 0);
            proxRect.sizeDelta = new Vector2(16, 16);
            proxImg.gameObject.SetActive(false);

            // Controller hub
            var uiGO = new GameObject("DayNightUI");
            uiGO.transform.SetParent(hudRoot.transform);
            var uiManager = uiGO.AddComponent<DayNightSystem.UI.DayNightUIManager>();
            var timeCtrl = uiGO.AddComponent<DayNightSystem.UI.TimeDisplay.TimeDisplayController>();
            var statusCtrl = uiGO.AddComponent<DayNightSystem.UI.StatusDisplay.StatusDisplayController>();
            var warnCtrl = uiGO.AddComponent<DayNightSystem.UI.WarningSystem.WarningSystemController>();
            var proxCtrl = uiGO.AddComponent<DayNightSystem.UI.ProximitySystem.ProximitySystemController>();

            // Link serialized refs
            SetSerializedObjectReference(timeCtrl, "timeDisplay", timeTMP);
            SetSerializedObjectReference(statusCtrl, "statusDisplay", statusTMP);
            SetSerializedObjectReference(statusCtrl, "feedbackMessages", messages);
            SetSerializedObjectReference(warnCtrl, "warningIcon", warningImg);
            SetSerializedObjectReference(warnCtrl, "feedbackMessages", messages);
            SetSerializedObjectReference(proxCtrl, "spawnProximityIcon", proxImg);
            SetSerializedObjectReference(proxCtrl, "feedbackMessages", messages);

            Undo.RegisterCreatedObjectUndo(hudRoot, "Create HUD");
            return uiGO;
        }

        private GameObject BuildGameState(Transform parent, DayNightSystem.FeedbackMessagesSO messages)
        {
            var go = new GameObject("GameState");
            if (parent != null) go.transform.SetParent(parent);
            var gsm = go.AddComponent<DayNightSystem.GameStateManager>();
            var gsmMsgs = go.AddComponent<GameStateController.GameStateMessages>();
            var gate = go.AddComponent<GameStateController.PlayerInputGate>();

            SetSerializedObjectReference(gsm, "feedbackMessages", messages);

            Undo.RegisterCreatedObjectUndo(go, "Create Game State");
            return go;
        }

        private GameObject BuildSpawnPoint(Transform parent)
        {
            var go = new GameObject("SpawnPoint");
            if (parent != null) go.transform.SetParent(parent);
            go.AddComponent<DayNightSystem.SpawnPoint>();
            Undo.RegisterCreatedObjectUndo(go, "Create SpawnPoint");
            return go;
        }

        private GameObject BuildPlayer(Transform parent)
        {
            var playerGO = new GameObject("Player");
            if (parent != null) playerGO.transform.SetParent(parent);
            // Intentar etiquetar como Player si existe la etiqueta
            try { playerGO.tag = "Player"; } catch {}
            var controller = playerGO.AddComponent<CharacterController>();
            controller.center = new Vector3(0, 1, 0);
            controller.height = 2f;

            var playerMove = playerGO.AddComponent<PlayerController.PlayerMovement>();
            var cameraGO = new GameObject("PlayerCamera");
            cameraGO.transform.SetParent(playerGO.transform);
            cameraGO.transform.localPosition = new Vector3(0, 1.6f, 0);
            var cam = cameraGO.AddComponent<Camera>();
            var mouseLook = cameraGO.AddComponent<PlayerController.MouseLook>();
            var interact = playerGO.AddComponent<PlayerController.PlayerInteraction>();

            // Link camera to movement/mouselook if possible (serialized are private; use reflection set)
            SetSerializedObjectReference(playerMove, "cameraTransform", cameraGO.transform);
            SetSerializedObjectReference(mouseLook, "playerBody", playerGO.transform);
            SetSerializedObjectReference(interact, "playerCamera", cam);

            // Asignar PlayerSettingsSO requerido por los tres componentes
            if (playerSettings != null)
            {
                SetSerializedObjectReference(playerMove, "playerSettings", playerSettings);
                SetSerializedObjectReference(mouseLook, "playerSettings", playerSettings);
                SetSerializedObjectReference(interact, "playerSettings", playerSettings);
            }

            Undo.RegisterCreatedObjectUndo(playerGO, "Create Player");
            return playerGO;
        }

        private void SetSerializedObjectReference(Object target, string fieldName, Object value)
        {
            if (target == null) return;
            var so = new SerializedObject(target);
            var sp = so.FindProperty(fieldName);
            if (sp != null)
            {
                sp.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                // Try private backing field (if any)
                var alt = so.FindProperty("_" + fieldName);
                if (alt != null)
                {
                    alt.objectReferenceValue = value;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        private void SaveAndDispose(GameObject go, string prefabName)
        {
            var savePath = System.IO.Path.Combine(prefabFolder, prefabName + ".prefab").Replace("\\", "/");
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, savePath);
            if (prefab != null)
            {
                Debug.Log($"[PrefabPackGenerator] Prefab creado: {savePath}");
            }
            else
            {
                Debug.LogError($"[PrefabPackGenerator] Error al guardar {prefabName}");
            }
            Object.DestroyImmediate(go);
        }
    }
}
#endif


