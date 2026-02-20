using UnityEngine;
using UnityEditor;
using SweetHome.Editor.Models;

#if UNITY_EDITOR
public class EditorManager
{
    public CommandResult PlayEditor(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        EditorApplication.EnterPlaymode();
        result.success = true;
        result.message = "플레이 모드 진입.";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult PauseEditor(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        EditorApplication.isPaused = !EditorApplication.isPaused;
        result.success = true;
        result.message = $"플레이 모드 {(EditorApplication.isPaused ? "일시정지" : "재개")}.";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult StopEditor(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        EditorApplication.ExitPlaymode();
        result.success = true;
        result.message = "플레이 모드 중지.";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult AddTag(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.tagName))
        {
            result.success = false;
            result.message = "태그를 추가하려면 'tagName'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(command.tagName))
            {
                result.success = false;
                result.message = $"태그 '{command.tagName}'이(가) 이미 존재합니다.";
                Debug.LogWarning(result.message);
                return result;
            }
        }

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
        newTag.stringValue = command.tagName;

        tagManager.ApplyModifiedProperties();
        result.success = true;
        result.message = $"태그 '{command.tagName}' 추가됨.";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult RemoveTag(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.tagName))
        {
            result.success = false;
            result.message = "태그를 제거하려면 'tagName'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(command.tagName))
            {
                tagsProp.DeleteArrayElementAtIndex(i);
                found = true;
                break;
            }
        }

        if (found)
        {
            tagManager.ApplyModifiedProperties();
            result.success = true;
            result.message = $"태그 '{command.tagName}' 제거됨.";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"태그 '{command.tagName}'을(를) 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    public CommandResult AddLayer(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.layerName))
        {
            result.success = false;
            result.message = "레이어를 추가하려면 'layerName'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");

        for (int i = 8; i < layersProp.arraySize; i++)
        {
            SerializedProperty l = layersProp.GetArrayElementAtIndex(i);
            if (l.stringValue.Equals(command.layerName))
            {
                result.success = false;
                result.message = $"레이어 '{command.layerName}'이(가) 이미 존재합니다.";
                Debug.LogWarning(result.message);
                return result;
            }
        }

        for (int i = 8; i < layersProp.arraySize; i++)
        {
            SerializedProperty l = layersProp.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(l.stringValue))
            {
                l.stringValue = command.layerName;
                tagManager.ApplyModifiedProperties();
                result.success = true;
                result.message = $"레이어 '{command.layerName}' 추가됨.";
                Debug.Log(result.message);
                return result;
            }
        }
        result.success = false;
        result.message = "사용 가능한 레이어 슬롯이 없습니다 (최대 31개).";
        Debug.LogWarning(result.message);
        return result;
    }

    public CommandResult ExecuteMenuItem(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.menuPath))
        {
            result.success = false;
            result.message = "메뉴 항목을 실행하려면 'menuPath'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        EditorApplication.ExecuteMenuItem(command.menuPath);
        result.success = true;
        result.message = $"메뉴 항목 실행됨: {command.menuPath}";
        Debug.Log(result.message);
        return result;
    }
}
#endif