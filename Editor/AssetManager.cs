using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using SweetHome.Editor.Models;

#if UNITY_EDITOR
public class AssetManager
{
    public CommandResult SearchAssets(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        string filter = command.searchPattern;
        if (!string.IsNullOrEmpty(command.filterType))
        {
            filter += $" t:{command.filterType}";
        }

        string[] guids = AssetDatabase.FindAssets(filter, new string[] { "Assets" });
        List<string> assetPaths = new List<string>();
        foreach (string guid in guids)
        {
            assetPaths.Add(AssetDatabase.GUIDToAssetPath(guid));
        }
        result.success = true;
        result.message = $"에셋 검색 결과 ({assetPaths.Count}개): {string.Join(", ", assetPaths)}";
        result.output = JsonUtility.ToJson(new { assets = assetPaths });
        Debug.Log(result.message);
        return result;
    }

    public CommandResult GetAssetInfo(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.path))
        {
            result.success = false;
            result.message = "에셋 정보를 가져오려면 'path'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(command.path);
        if (asset != null)
        {
            result.success = true;
            result.message = $"에셋 정보: {command.path}, 타입: {asset.GetType().Name}, GUID: {AssetDatabase.AssetPathToGUID(command.path)}";
            result.output = JsonUtility.ToJson(new { path = command.path, type = asset.GetType().Name, guid = AssetDatabase.AssetPathToGUID(command.path) });
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"에셋 정보를 가져올 수 없습니다: {command.path}";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    public CommandResult CreateAsset(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.path) || string.IsNullOrEmpty(command.assetType))
        {
            result.success = false;
            result.message = "에셋을 생성하려면 'path'와 'assetType'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        string fullPath = command.path;
        string directory = Path.GetDirectoryName(fullPath);
        if (!AssetDatabase.IsValidFolder(directory))
        {
            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }

        Object newAsset = null;
        switch (command.assetType.ToLower())
        {
            case "material":
                newAsset = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(newAsset, fullPath);
                break;
            case "folder":
                AssetDatabase.CreateFolder(Path.GetDirectoryName(fullPath), Path.GetFileName(fullPath));
                break;
            default:
                result.success = false;
                result.message = $"지원되지 않는 에셋 유형: {command.assetType}";
                Debug.LogWarning(result.message);
                return result;
        }

        if (newAsset != null)
        {
            AssetDatabase.SaveAssets();
            result.success = true;
            result.message = $"에셋 생성됨: {fullPath} (타입: {command.assetType})";
            Debug.Log(result.message);
        }
        else if (command.assetType.ToLower() == "folder")
        {
            result.success = true;
            result.message = $"폴더 생성됨: {fullPath}";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"에셋 생성 실패: {fullPath} (타입: {command.assetType})";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    public CommandResult DeleteAsset(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.path))
        {
            result.success = false;
            result.message = "에셋을 삭제하려면 'path'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        bool success = AssetDatabase.DeleteAsset(command.path);
        if (success)
        {
            result.success = true;
            result.message = $"에셋 삭제됨: {command.path}";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"에셋을 삭제할 수 없습니다: {command.path}";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    public CommandResult DuplicateAsset(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.source) || string.IsNullOrEmpty(command.destination))
        {
            result.success = false;
            result.message = "에셋을 복제하려면 'source'와 'destination'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        bool success = AssetDatabase.CopyAsset(command.source, command.destination);
        if (success)
        {
            result.success = true;
            result.message = $"에셋 복제됨: {command.source} -> {command.destination}";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"에셋을 복제할 수 없습니다: {command.source}";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    public CommandResult MoveAsset(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.source) || string.IsNullOrEmpty(command.destination))
        {
            result.success = false;
            result.message = "에셋을 이동하려면 'source'와 'destination'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        string error = AssetDatabase.MoveAsset(command.source, command.destination);
        if (string.IsNullOrEmpty(error))
        {
            result.success = true;
            result.message = $"에셋 이동됨: {command.source} -> {command.destination}";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"에셋을 이동할 수 없습니다: {command.source}. 오류: {error}";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    public CommandResult RenameAsset(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.path) || string.IsNullOrEmpty(command.newName))
        {
            result.success = false;
            result.message = "에셋 이름을 변경하려면 'path'와 'newName'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        string error = AssetDatabase.RenameAsset(command.path, command.newName);
        if (string.IsNullOrEmpty(error))
        {
            result.success = true;
            result.message = $"에셋 이름 변경됨: {command.path} -> {command.newName}";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"에셋 이름을 변경할 수 없습니다: {command.path}. 오류: {error}";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    public CommandResult ImportAsset(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.path))
        {
            result.success = false;
            result.message = "에셋을 임포트하려면 'path'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        AssetDatabase.ImportAsset(command.path);
        result.success = true;
        result.message = $"에셋 임포트됨: {command.path}";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult CreateFolder(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.path))
        {
            result.success = false;
            result.message = "폴더를 생성하려면 'path'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        string guid = AssetDatabase.CreateFolder(Path.GetDirectoryName(command.path), Path.GetFileName(command.path));
        if (!string.IsNullOrEmpty(guid))
        {
            result.success = true;
            result.message = $"폴더 생성됨: {command.path}";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"폴더를 생성할 수 없습니다: {command.path}";
            Debug.LogWarning(result.message);
        }
        return result;
    }
}
#endif