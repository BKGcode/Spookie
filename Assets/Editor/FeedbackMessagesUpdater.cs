using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq; // Needed for FirstOrDefault

public class FeedbackMessagesUpdater : EditorWindow
{
    private string messagesTextInput = ""; // Campo para pegar el texto
    private FeedbackMessagesSO targetSO;
    private Vector2 scrollPos; // Para el scroll del TextArea

    // Añade un menú en Unity para abrir esta ventana
    [MenuItem("Tools/Spookie/Feedback Messages Updater")]
    public static void ShowWindow()
    {
        GetWindow<FeedbackMessagesUpdater>("Feedback Updater");
    }

    void OnGUI()
    {
        GUILayout.Label("Actualizar FeedbackMessages SO", EditorStyles.boldLabel);

        // Campo para asignar el SO manualmente (opcional, si la búsqueda falla o hay varios)
        targetSO = (FeedbackMessagesSO)EditorGUILayout.ObjectField("Target Feedback SO", targetSO, typeof(FeedbackMessagesSO), false);

        GUILayout.Label("Pega aquí la lista de claves y mensajes (formato: clave: \"mensaje\")");

        // Área de texto con scroll
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
        messagesTextInput = EditorGUILayout.TextArea(messagesTextInput, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();


        if (GUILayout.Button("Buscar y Actualizar SO"))
        {
            UpdateFeedbackMessages();
        }
    }

    private void UpdateFeedbackMessages()
    {
        // Intenta encontrar el SO si no está asignado
        if (targetSO == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:FeedbackMessagesSO");
            if (guids.Length == 0)
            {
                Debug.LogError("FeedbackMessagesUpdater: No se encontró ningún asset FeedbackMessagesSO en el proyecto.");
                return;
            }
            if (guids.Length > 1)
            {
                 Debug.LogWarning("FeedbackMessagesUpdater: Se encontró más de un FeedbackMessagesSO. Usando el primero. Asigna uno manualmente si es incorrecto.");
            }
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            targetSO = AssetDatabase.LoadAssetAtPath<FeedbackMessagesSO>(path);
        }

        if (targetSO == null)
        {
             Debug.LogError("FeedbackMessagesUpdater: No se pudo cargar el FeedbackMessagesSO.");
             return;
        }

        // Parsear el texto
        var lines = messagesTextInput.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        int updatedCount = 0;
        int addedCount = 0;

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || !trimmedLine.Contains(':')) continue; // Ignorar líneas vacías o sin ':'

            int separatorIndex = trimmedLine.IndexOf(':');
            string key = trimmedLine.Substring(0, separatorIndex).Trim();
            string message = trimmedLine.Substring(separatorIndex + 1).Trim();

            // Quitar comillas del inicio y final si existen
            if (message.StartsWith("\"") && message.EndsWith("\""))
            {
                message = message.Substring(1, message.Length - 2);
            }
            // Reemplazar comillas escapadas si es necesario (opcional)
            // message = message.Replace("\\"", "\"");

            if (string.IsNullOrEmpty(key)) continue; // Ignorar si la clave está vacía

            // Buscar si la clave ya existe en el SO
            FeedbackMessage existingMessage = targetSO.messages.FirstOrDefault(m => m.key == key);

            if (existingMessage != null)
            {
                // Actualizar mensaje existente
                if (existingMessage.message != message)
                {
                    existingMessage.message = message;
                    updatedCount++;
                }
            }
            else
            {
                // Añadir nuevo mensaje
                targetSO.messages.Add(new FeedbackMessage { key = key, message = message });
                addedCount++;
            }
        }

        if (updatedCount > 0 || addedCount > 0)
        {
            // Marcar el SO como modificado y guardar los cambios
            EditorUtility.SetDirty(targetSO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(); // Refrescar para que Unity vea los cambios
            Debug.Log($"FeedbackMessagesUpdater: SO actualizado. {addedCount} claves añadidas, {updatedCount} claves actualizadas.");

             // Forzar la reconstrucción del caché interno del SO si está cargado
            // Esto requiere que OnEnable se llame de nuevo, lo cual SaveAssets/Refresh debería inducir.
            // Si no, una forma más directa sería añadir un método público al SO para reconstruir el caché.
            // targetSO.RebuildCache(); // Si tuvieras este método

        }
        else
        {
            Debug.Log("FeedbackMessagesUpdater: No se realizaron cambios en el SO (claves y mensajes coincidían).");
        }

        // Limpiar el campo de texto después de procesar (opcional)
        // messagesTextInput = "";
    }
}

// --- Summary Block ---
// ScriptRole: Provides an Editor Window to bulk update a FeedbackMessagesSO asset by pasting key-value pairs from a text field.
// RelatedScripts: FeedbackMessagesSO
// UsesSO: FeedbackMessagesSO 