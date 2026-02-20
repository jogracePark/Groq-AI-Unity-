using UnityEditor;
using UnityEngine;

public class GroqApiSettingsEditor : EditorWindow
{
    private string groqApiUrl;
    private string groqApiKey;

    private const string GroqApiUrlKey = "GroqApiUrl";
    private const string GroqApiKeyKey = "GroqApiKey";

    [MenuItem("Tools/Groq API Settings")]
    public static void ShowWindow()
    {
        GetWindow<GroqApiSettingsEditor>("Groq API Settings");
    }

    private void OnEnable()
    {
        // EditorPrefs에서 저장된 값 로드
        groqApiUrl = EditorPrefs.GetString(GroqApiUrlKey, "https://api.groq.com/openai/v1/chat/completions"); // 기본값 설정
        groqApiKey = EditorPrefs.GetString(GroqApiKeyKey, "");
    }

    private void OnGUI()
    {
        GUILayout.Label("Groq API Configuration", EditorStyles.boldLabel);

        groqApiUrl = EditorGUILayout.TextField("API URL", groqApiUrl);
        groqApiKey = EditorGUILayout.TextField("API Key", groqApiKey);

        if (GUILayout.Button("Save Settings"))
        {
            EditorPrefs.SetString(GroqApiUrlKey, groqApiUrl);
            EditorPrefs.SetString(GroqApiKeyKey, groqApiKey);
            Debug.Log("Groq API Settings saved.");
        }

        if (GUILayout.Button("Clear Settings"))
        {
            EditorPrefs.DeleteKey(GroqApiUrlKey);
            EditorPrefs.DeleteKey(GroqApiKeyKey);
            groqApiUrl = "https://api.groq.com/openai/v1/chat/completions"; // 기본값으로 재설정
            groqApiKey = "";
            Debug.Log("Groq API Settings cleared.");
        }
    }
}
