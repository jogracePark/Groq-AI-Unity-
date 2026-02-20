using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using SweetHome.Editor.Models;

#if UNITY_EDITOR
public class ScriptableObjectManager
{
    public CommandResult CreateScriptableObject(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.typeName) || string.IsNullOrEmpty(command.folderPath) || string.IsNullOrEmpty(command.assetName))
        {
            result.success = false;
            result.message = "ScriptableObject를 생성하려면 typeName, folderPath, assetName이 모두 필요합니다.";
            Debug.LogError(result.message);
            return result;
        }

        Type type = FindType(command.typeName);
        if (type == null)
        {
            result.success = false;
            result.message = $"타입을 찾을 수 없습니다: {command.typeName}";
            Debug.LogError(result.message);
            return result;
        }

        if (!typeof(ScriptableObject).IsAssignableFrom(type))
        {
            result.success = false;
            result.message = $"타입이 ScriptableObject를 상속하지 않습니다: {command.typeName}";
            Debug.LogError(result.message);
            return result;
        }

        if (!Directory.Exists(command.folderPath))
        {
            Directory.CreateDirectory(command.folderPath);
        }

        string assetPath = Path.Combine(command.folderPath, command.assetName + ".asset");

        if (!command.overwrite && File.Exists(assetPath))
        {
            result.success = false;
            result.message = $"에셋이 이미 존재합니다: {assetPath}. 덮어쓰려면 overwrite를 true로 설정하세요.";
            Debug.LogWarning(result.message);
            return result;
        }

        ScriptableObject instance = ScriptableObject.CreateInstance(type);
        AssetDatabase.CreateAsset(instance, assetPath);
        
        if (command.patches != null && command.patches.Length > 0)
        {
            GroqUnityCommand modifyCmd = new GroqUnityCommand
            {
                commandType = "ModifyScriptableObject",
                assetPath = assetPath,
                patches = command.patches
            };
            CommandResult modifyResult = ModifyScriptableObject(modifyCmd);
            if (!modifyResult.success)
            {
                Debug.LogWarning($"생성된 ScriptableObject의 초기 속성 설정 실패: {modifyResult.message}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        result.success = true;
        result.message = $"ScriptableObject 에셋 생성됨: {assetPath}";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult ModifyScriptableObject(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.assetPath) || command.patches == null || command.patches.Length == 0)
        {
            result.success = false;
            result.message = "ScriptableObject를 수정하려면 assetPath와 patches가 필요합니다.";
            Debug.LogError(result.message);
            return result;
        }

        ScriptableObject targetAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(command.assetPath);
        if (targetAsset == null)
        {
            result.success = false;
            result.message = $"에셋을 찾을 수 없습니다: {command.assetPath}";
            Debug.LogError(result.message);
            return result;
        }

        SerializedObject serializedObject = new SerializedObject(targetAsset);
        bool modified = false;

        foreach (var patch in command.patches)
        {
            SerializedProperty property = serializedObject.FindProperty(patch.propertyName);
            if (property == null)
            {
                Debug.LogWarning($"프로퍼티를 찾을 수 없습니다: {patch.propertyName}");
                continue;
            }

            // This is a bit of a hack, but it reuses the conversion logic from ComponentManager
            // In a pure refactor, this logic would be moved to a shared utility class.
            // But per instructions, managers are self-contained, so this is the simplest way.
            try
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        property.intValue = (int)ComponentManager.ConvertValue(patch.value, typeof(int));
                        break;
                    case SerializedPropertyType.Boolean:
                        property.boolValue = (bool)ComponentManager.ConvertValue(patch.value, typeof(bool));
                        break;
                    case SerializedPropertyType.Float:
                        property.floatValue = (float)ComponentManager.ConvertValue(patch.value, typeof(float));
                        break;
                    case SerializedPropertyType.String:
                        property.stringValue = (string)ComponentManager.ConvertValue(patch.value, typeof(string));
                        break;
                    case SerializedPropertyType.Color:
                        property.colorValue = (Color)ComponentManager.ConvertValue(patch.value, typeof(Color));
                        break;
                    case SerializedPropertyType.ObjectReference:
                        property.objectReferenceValue = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(patch.value);
                        break;
                    case SerializedPropertyType.Enum:
                        property.enumValueIndex = Array.IndexOf(property.enumNames, patch.value);
                        break;
                    case SerializedPropertyType.Vector2:
                        property.vector2Value = (Vector2)ComponentManager.ConvertValue(patch.value, typeof(Vector2));
                        break;
                    case SerializedPropertyType.Vector3:
                        property.vector3Value = (Vector3)ComponentManager.ConvertValue(patch.value, typeof(Vector3));
                        break;
                    case SerializedPropertyType.Vector4:
                        property.vector4Value = (Vector4)ComponentManager.ConvertValue(patch.value, typeof(Vector4));
                        break;
                    default:
                        Debug.LogWarning($"지원되지 않는 프로퍼티 타입: {property.propertyType}");
                        continue;
                }
                modified = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"프로퍼티 '{property.name}'에 값 '{patch.value}'를 적용하는 중 오류 발생: {e.Message}");
            }
        }

        if (modified)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(targetAsset);
            AssetDatabase.SaveAssets();
            result.success = true;
            result.message = $"ScriptableObject 에셋 수정됨: {command.assetPath}";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"ScriptableObject 에셋 '{command.assetPath}' 수정 실패 또는 변경 사항 없음.";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    private Type FindType(string typeName)
    {
        var type = Type.GetType(typeName);
        if (type != null) return type;
        foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = a.GetType(typeName);
            if (type != null)
                return type;
        }
        return null;
    }
}
#endif