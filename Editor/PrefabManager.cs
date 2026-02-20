#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using SweetHome.Editor.Models;

public class PrefabManager
{
    public CommandResult CreatePrefab(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target) || string.IsNullOrEmpty(command.assetPath))
        {
            result.success = false;
            result.message = "프리팹을 생성하려면 'target' GameObject와 'assetPath'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        var gameObjectManager = new GameObjectManager();
        GameObject targetGameObject = gameObjectManager.FindGameObject(command);
        if (targetGameObject != null)
        {
            try
            {
                PrefabUtility.SaveAsPrefabAsset(targetGameObject, command.assetPath, out bool success);
                if (success)
                {
                    result.success = true;
                    result.message = $"프리팹 생성됨: {command.assetPath}";
                    Debug.Log(result.message);
                }
                else
                {
                    result.success = false;
                    result.message = $"프리팹 생성 실패: {command.assetPath}";
                    Debug.LogError(result.message);
                }
            }
            catch (System.Exception e)
            {
                result.success = false;
                result.message = $"프리팹 생성 중 오류 발생: {e.Message}";
                Debug.LogError(result.message);
            }
        }
        else
        {
            result.success = false;
            result.message = $"프리팹을 생성할 GameObject '{command.target}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    public CommandResult InstantiatePrefab(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.assetPath))
        {
            result.success = false;
            result.message = "프리팹을 인스턴스화하려면 'assetPath'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(command.assetPath);
        if (prefabAsset != null)
        {
            Vector3 position = command.position != null && command.position.Length == 3 ? new Vector3(command.position[0], command.position[1], command.position[2]) : Vector3.zero;
            Quaternion rotation = command.rotation != null && command.rotation.Length == 3 ? Quaternion.Euler(command.rotation[0], command.rotation[1], command.rotation[2]) : Quaternion.identity;
            
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
            instance.transform.position = position;
            instance.transform.rotation = rotation;

            if (!string.IsNullOrEmpty(command.name))
            {
                instance.name = command.name;
            }

            result.success = true;
            result.message = $"프리팹 '{command.assetPath}'가 씬에 인스턴스화되었습니다.";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"프리팹 에셋을 찾을 수 없습니다: {command.assetPath}";
            Debug.LogWarning(result.message);
        }
        return result;
    }
}
#endif