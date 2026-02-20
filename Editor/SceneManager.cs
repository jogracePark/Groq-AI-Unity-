#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using SweetHome.Editor.Models;
using TMPro; // Added for TextMeshProUGUI
using System; // Added for Exception
using System.Text; // Added for StringBuilder

public class SceneManager
{
    public CommandResult GetSceneHierarchy(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        UnityEngine.SceneManagement.Scene activeScene = EditorSceneManager.GetActiveScene();
        List<GameObject> rootObjects = new List<GameObject>();
        activeScene.GetRootGameObjects(rootObjects);

        StringBuilder hierarchyBuilder = new StringBuilder();
        hierarchyBuilder.AppendLine($"씬 '{activeScene.name}' 계층 구조:");

        foreach (GameObject rootObject in rootObjects)
        {
            AppendHierarchy(rootObject.transform, 0, command.maxDepth, command.includeTransform, hierarchyBuilder);
        }

        result.success = true;
        result.message = $"씬 '{activeScene.name}' 계층 구조를 성공적으로 가져왔습니다.";
        result.output = hierarchyBuilder.ToString();
        return result;
    }

    private void AppendHierarchy(Transform transform, int depth, int maxDepth, bool includeTransform, StringBuilder builder)
    {
        if (maxDepth > 0 && depth >= maxDepth) return;

        string indent = new string(' ', depth * 2);
        string transformInfo = "";
        if (includeTransform)
        {
            transformInfo = $" (Pos: {transform.localPosition}, Rot: {transform.localEulerAngles}, Scale: {transform.localScale})";
        }
        builder.AppendLine($"{indent}- {transform.gameObject.name}{transformInfo}");

        foreach (Transform child in transform)
        {
            AppendHierarchy(child, depth + 1, maxDepth, includeTransform, builder);
        }
    }

    public CommandResult GetActiveSceneInfo(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        UnityEngine.SceneManagement.Scene activeScene = EditorSceneManager.GetActiveScene();
        result.success = true;
        result.message = $"활성화된 씬: {activeScene.name}, 경로: {activeScene.path}, 루트 GameObject 수: {activeScene.rootCount}, 빌드 인덱스: {activeScene.buildIndex}";
        result.output = JsonUtility.ToJson(new SceneInfoOutput { name = activeScene.name, path = activeScene.path, rootObjectCount = activeScene.rootCount, buildIndex = activeScene.buildIndex });
        Debug.Log(result.message);
        return result;
    }

    public CommandResult LoadScene(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (!string.IsNullOrEmpty(command.name))
        {
            string[] guids = AssetDatabase.FindAssets($"t:Scene {command.name}");
            if (guids.Length > 0)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                EditorSceneManager.OpenScene(scenePath);
                result.success = true;
                result.message = $"씬 로드됨: {scenePath}";
                Debug.Log(result.message);
            }
            else
            {
                result.success = false;
                result.message = $"씬 '{{command.name}}'을(를) 찾을 수 없습니다.";
                Debug.LogWarning(result.message);
            }
        }
        else if (!string.IsNullOrEmpty(command.path))
        {
            EditorSceneManager.OpenScene(command.path);
            result.success = true;
            result.message = $"씬 로드됨: {command.path}";
            Debug.Log(result.message);
        }
        else if (command.buildIndex >= 0)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(command.buildIndex);
            if (!string.IsNullOrEmpty(scenePath))
            {
                EditorSceneManager.OpenScene(scenePath);
                result.success = true;
                result.message = $"씬 로드됨 (빌드 인덱스 {command.buildIndex}): {scenePath}";
                Debug.Log(result.message);
            }
            else
            {
                result.success = false;
                result.message = $"빌드 인덱스 '{{command.buildIndex}}'에 해당하는 씬을 찾을 수 없습니다.";
                Debug.LogWarning(result.message);
            }
        }
        else
        {
            result.success = false;
            result.message = "씬을 로드하려면 'path', 'name' 또는 'buildIndex'가 필요합니다.";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    public CommandResult SaveScene(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        UnityEngine.SceneManagement.Scene activeScene = EditorSceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(command.path))
        {
            EditorSceneManager.SaveScene(activeScene, command.path);
            result.success = true;
            result.message = $"씬 저장됨: {command.path}";
            Debug.Log(result.message);
        }
        else
        {
            EditorSceneManager.SaveScene(activeScene);
            result.success = true;
            result.message = $"현재 씬 저장됨: {activeScene.name}";
            Debug.Log(result.message);
        }
        return result;
    }

    public CommandResult CreateScene(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.path))
        {
            result.success = false;
            result.message = "씬을 생성하려면 'path'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        string scenePath = command.path;
        if (!scenePath.EndsWith(".unity"))
        {
            scenePath += ".unity";
        }

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        result.success = true;
        result.message = $"씬 생성됨: {scenePath}";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult GetBuildSettingsScenes(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        List<string> sceneNames = new List<string>();
        foreach (EditorBuildSettingsScene editorScene in EditorBuildSettings.scenes)
        {
            if (editorScene.enabled)
            {
                sceneNames.Add(Path.GetFileNameWithoutExtension(editorScene.path));
            }
        }
        result.success = true;
        result.message = $"빌드 설정에 있는 씬: {string.Join(", ", sceneNames)}";
        result.output = JsonUtility.ToJson(new BuildSettingsScenesOutput { scenes = sceneNames });
        Debug.Log(result.message);
        return result;
    }

    public CommandResult CaptureScreenshot(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        string filename = command.screenshotFileName ?? $"Screenshot_{{System.DateTime.Now:yyyyMMdd_HHmmss}}.png";
        if (!filename.ToLower().EndsWith(".png")) filename += ".png";
        
        string screenshotDir = Path.Combine(Application.dataPath, "Screenshots");
        if (!Directory.Exists(screenshotDir))
        {
            Directory.CreateDirectory(screenshotDir);
            Debug.Log($"[CaptureScreenshot] Created directory: {screenshotDir}");
        }
        string fullPath = Path.Combine(screenshotDir, filename);
        
        Debug.Log($"[CaptureScreenshot] Attempting to capture screenshot to: {fullPath} (Camera-based)");

        try
        {
            // Use UnityEngine.ScreenCapture.CaptureScreenshot for more reliable editor screenshots, especially for UI
            UnityEngine.ScreenCapture.CaptureScreenshot(fullPath);
            AssetDatabase.Refresh(); // Refresh AssetDatabase to show the new file

            result.success = true;
            result.message = $"스크린샷 캡처됨: {fullPath}";
            result.output = JsonUtility.ToJson(new SweetHome.Editor.Models.ScreenshotOutput { filePath = fullPath });
            Debug.Log(result.message);
        }
        catch (Exception e)
        {
            result.success = false;
            result.message = $"스크린샷 캡처 실패: {e.Message}";
            Debug.LogError($"[CaptureScreenshot] Error capturing screenshot to {fullPath}: {e.Message}\n{{e.StackTrace}}");
            result.output = JsonUtility.ToJson(new SweetHome.Editor.Models.ScreenshotOutput { error = e.Message, stackTrace = e.StackTrace });
        }
        
        return result;
    }
}
#endif
