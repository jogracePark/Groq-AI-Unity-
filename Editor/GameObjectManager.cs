using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System; // Added for Func<,>
using SweetHome.Editor.Models;
using UnityEditor;
using SweetHome.Editor.Models; // Added for CommandRegistry

#if UNITY_EDITOR
public class GameObjectManager
{
    static GameObjectManager()
    {
        CommandRegistry.RegisterCommand(new CommandMetadata(
            commandType: "CreateGameObject",
            description: "Creates a new GameObject in the scene.",
            requiredParameters: new List<ParameterMetadata> {
                new ParameterMetadata("name", typeof(string))
            },
            optionalParameters: new List<ParameterMetadata> {
                new ParameterMetadata("parent", typeof(string)),
                new ParameterMetadata("primitiveType", typeof(string)),
                new ParameterMetadata("position", typeof(float[])),
                new ParameterMetadata("rotation", typeof(float[])),
                new ParameterMetadata("scale", typeof(float[])),
                new ParameterMetadata("setActive", typeof(bool)),
                new ParameterMetadata("tag", typeof(string)),
                new ParameterMetadata("layer", typeof(string))
            },
            managerName: "GameObjectManager"
        ));

        CommandRegistry.RegisterCommand(new CommandMetadata(
            commandType: "ModifyGameObject",
            description: "Modifies an existing GameObject in the scene.",
            requiredParameters: new List<ParameterMetadata> {
                new ParameterMetadata("target", typeof(string))
            },
            optionalParameters: new List<ParameterMetadata> {
                new ParameterMetadata("name", typeof(string)),
                new ParameterMetadata("position", typeof(float[])),
                new ParameterMetadata("rotation", typeof(float[])),
                new ParameterMetadata("scale", typeof(float[])),
                new ParameterMetadata("parent", typeof(string)),
                new ParameterMetadata("tag", typeof(string)),
                new ParameterMetadata("layer", typeof(string)),
                new ParameterMetadata("setActive", typeof(bool))
            },
            managerName: "GameObjectManager"
        ));

        CommandRegistry.RegisterCommand(new CommandMetadata(
            commandType: "DeleteGameObject",
            description: "Deletes a GameObject from the scene.",
            isDestructive: true,
            requiredParameters: new List<ParameterMetadata> {
                new ParameterMetadata("target", typeof(string))
            },
            managerName: "GameObjectManager"
        ));

        CommandRegistry.RegisterCommand(new CommandMetadata(
            commandType: "SetConditionalActive",
            description: "Sets conditional activation for a GameObject.",
            requiredParameters: new List<ParameterMetadata> {
                new ParameterMetadata("target", typeof(string)),
                new ParameterMetadata("conditionType", typeof(string)),
                new ParameterMetadata("conditionValue", typeof(float))
            },
            managerName: "GameObjectManager"
        ));
    }
    public CommandResult CreateGameObject(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        GameObject newGameObject = null;
        Vector3 position = command.position != null && command.position.Length == 3 ? new Vector3(command.position[0], command.position[1], command.position[2]) : Vector3.zero;
        Quaternion rotation = command.rotation != null && command.rotation.Length == 3 ? Quaternion.Euler(command.rotation[0], command.rotation[1], command.rotation[2]) : Quaternion.identity;
        Vector3 scale = command.scale != null && command.scale.Length == 3 ? new Vector3(command.scale[0], command.scale[1], command.scale[2]) : Vector3.one;

        if (!string.IsNullOrEmpty(command.primitiveType))
        {
            switch (command.primitiveType.ToLower())
            {
                case "cube": newGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube); break;
                case "sphere": newGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere); break;
                case "capsule": newGameObject = GameObject.CreatePrimitive(PrimitiveType.Capsule); break;
                case "cylinder": newGameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder); break;
                case "plane": newGameObject = GameObject.CreatePrimitive(PrimitiveType.Plane); break;
                case "quad": newGameObject = GameObject.CreatePrimitive(PrimitiveType.Quad); break;
                default:
                    result.success = false;
                    result.message = "알 수 없는 프리미티브 유형: " + command.primitiveType;
                    Debug.LogWarning(result.message);
                    return result;
            }
        }
        else
        {
            newGameObject = new GameObject();
        }

        if (newGameObject != null)
        {
            newGameObject.name = command.name ?? "NewGameObject";
            newGameObject.transform.position = position;
            newGameObject.transform.rotation = rotation;
            newGameObject.transform.localScale = scale;

            if (!string.IsNullOrEmpty(command.parent))
            {
                var parentFindCommand = new GroqUnityCommand { target = command.parent };
                GameObject parentGO = FindGameObject(parentFindCommand);
                if (parentGO != null)
                {
                    newGameObject.transform.SetParent(parentGO.transform);
                }
                else
                {
                    Debug.LogWarning($"부모 GameObject '{command.parent}'를 찾을 수 없습니다.");
                }
            }

            if (!string.IsNullOrEmpty(command.tag))
            {
                newGameObject.tag = command.tag;
            }
            if (!string.IsNullOrEmpty(command.layer))
            {
                int layerId = LayerMask.NameToLayer(command.layer);
                if (layerId != -1)
                {
                    newGameObject.layer = layerId;
                }
                else
                {
                    Debug.LogWarning($"레이어 '{command.layer}'를 찾을 수 없습니다.");
                }
            }
            Debug.Log($"[GameObjectManager] Creating GameObject '{newGameObject.name}' with setActive: {command.setActive}");
            newGameObject.SetActive(command.setActive);
            
            result.success = true;
            result.message = $"GameObject 생성: {newGameObject.name} 위치: {newGameObject.transform.position}";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = "GameObject 생성 실패.";
            Debug.LogError(result.message);
        }
        return result;
    }

    public CommandResult ModifyGameObject(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        GameObject targetGameObject = FindGameObject(command);
        if (targetGameObject != null)
        {
            if (!string.IsNullOrEmpty(command.name)) targetGameObject.name = command.name;
            if (command.position != null && command.position.Length == 3) targetGameObject.transform.position = new Vector3(command.position[0], command.position[1], command.position[2]);
            if (command.rotation != null && command.rotation.Length == 3) targetGameObject.transform.rotation = Quaternion.Euler(command.rotation[0], command.rotation[1], command.rotation[2]);
            if (command.scale != null && command.scale.Length == 3) targetGameObject.transform.localScale = new Vector3(command.scale[0], command.scale[1], command.scale[2]);
            
            if (!string.IsNullOrEmpty(command.parent))
            {
                var parentFindCommand = new GroqUnityCommand { target = command.parent };
                GameObject parentGO = FindGameObject(parentFindCommand);
                if (parentGO != null)
                {
                    targetGameObject.transform.SetParent(parentGO.transform);
                }
                else
                {
                    Debug.LogWarning($"부모 GameObject '{command.parent}'를 찾을 수 없습니다.");
                }
            }

            if (!string.IsNullOrEmpty(command.tag))
            {
                targetGameObject.tag = command.tag;
            }
            if (!string.IsNullOrEmpty(command.layer))
            {
                int layerId = LayerMask.NameToLayer(command.layer);
                if (layerId != -1)
                {
                    targetGameObject.layer = layerId;
                }
                else
                {
                    Debug.LogWarning($"레이어 '{command.layer}'를 찾을 수 없습니다.");
                }
            }
            Debug.Log($"[GameObjectManager] Modifying GameObject '{targetGameObject.name}' with setActive: {command.setActive}");
            targetGameObject.SetActive(command.setActive);

            result.success = true;
            result.message = $"GameObject 수정: {targetGameObject.name}";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"수정할 GameObject를 찾을 수 없습니다: {command.target}";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    public CommandResult DeleteGameObject(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        GameObject targetGameObject = FindGameObject(command);
        if (targetGameObject != null)
        {
            UnityEngine.Object.DestroyImmediate(targetGameObject);
            result.success = true;
            result.message = $"GameObject 삭제: {command.target}";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"삭제할 GameObject를 찾을 수 없습니다: {command.target}";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    public GameObject FindGameObject(GroqUnityCommand findCommand)
    {
        if (findCommand == null || string.IsNullOrEmpty(findCommand.target)) return null;
        // Determine the name matching predicate
        Func<GameObject, bool> namePredicate;
        if (findCommand.searchMethod?.ToLower() == "by_partial_name")
        {
            namePredicate = g => g.name.Contains(findCommand.target);
        }
        else // Default to exact match
        {
            namePredicate = g => g.name == findCommand.target;
        }
        GameObject foundGameObject = null;
        // 1. Search by parentName first if provided
        if (!string.IsNullOrEmpty(findCommand.parentName))
        {
            // Create a temporary command to find the parent, ensuring it doesn't use parentName/ancestorName for itself
            var parentFindCommand = new GroqUnityCommand { target = findCommand.parentName, searchMethod = findCommand.searchMethod };
            if (!string.IsNullOrEmpty(findCommand.ancestorName))
            {
                parentFindCommand.ancestorName = findCommand.ancestorName;
            }
            GameObject parentGO = FindGameObject(parentFindCommand); // Recursive call to find parent
            if (parentGO != null)
            {
                // Search only within the children of the found parent, using the predicate
                foreach (Transform child in parentGO.transform)
                {
                    if (namePredicate(child.gameObject)) // Use the predicate here
                    {
                        foundGameObject = child.gameObject;
                        break;
                    }
                }
                if (foundGameObject == null)
                {
                    Debug.LogWarning($"GameObject '{findCommand.target}'를 부모 '{findCommand.parentName}'의 자식으로 찾을 수 없습니다.");
                }
                return foundGameObject; // Return even if null, as we specifically searched within parent
            }
            else
            {
                Debug.LogWarning($"부모 GameObject '{findCommand.parentName}'를 찾을 수 없습니다. '{findCommand.target}' 검색 실패.");
                return null;
            }
        }
        // 2. If no parentName, use original logic (search all active scenes)
        // This part is for finding top-level objects or objects without a specified parent.
        var candidates = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(g => namePredicate(g) && g.scene.isLoaded).ToList();
        if (candidates.Count == 0)
        {
            Debug.LogWarning($"GameObject '{findCommand.target}'를 찾을 수 없습니다.");
            return null;
        }
        if (candidates.Count == 1)
        {
            return candidates[0];
        }
        // 3. Scoring logic for multiple candidates (if no parentName was used for direct search)
        List<(GameObject go, int score)> scoredCandidates = new List<(GameObject, int)>();
        foreach (var candidate in candidates)
        {
            int score = 0;
            if (!string.IsNullOrEmpty(findCommand.requiredComponent) && candidate.GetComponent(findCommand.requiredComponent) != null)
            {
                score += 10;
            }
            if (!string.IsNullOrEmpty(findCommand.ancestorName))
            {
                Transform parent = candidate.transform.parent;
                while (parent != null)
                {
                    if (parent.name == findCommand.ancestorName)
                    {
                        score += 15;
                        break;
                    }
                    parent = parent.parent;
                }
            }
            scoredCandidates.Add((candidate, score));
        }
        var bestCandidates = scoredCandidates
            .GroupBy(c => c.score)
            .OrderByDescending(g => g.Key)
            .FirstOrDefault();
        if (bestCandidates == null || bestCandidates.Key == 0)
        {
            Debug.LogWarning($"'{findCommand.target}'에 대한 명확한 후보를 찾지 못했습니다. 첫 번째 대상을 반환합니다.");
            return candidates[0];
        }
        var topScorers = bestCandidates.ToList();
        if (topScorers.Count == 1)
        {
            return topScorers[0].go;
        }
        else
        {
            Debug.LogWarning($"'{findCommand.target}'에 대한 동점 후보가 {topScorers.Count}개 발생했습니다. targetIndex를 사용하세요.");
            if (findCommand.targetIndex >= 0 && findCommand.targetIndex < topScorers.Count)
            {
                return topScorers[findCommand.targetIndex].go;
            }
            else
            {
                return topScorers[0].go;
            }
        }
    }

    public CommandResult SetConditionalActive(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target))
        {
            result.success = false;
            result.message = "조건부 활성화를 설정할 GameObject의 'target'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        if (string.IsNullOrEmpty(command.conditionType))
        {
            result.success = false;
            result.message = "조건 유형 ('conditionType')이 필요합니다 (예: ScreenWidthGreaterThan).";
            Debug.LogWarning(result.message);
            return result;
        }

        GameObject targetGO = FindGameObject(new GroqUnityCommand { target = command.target });
        if (targetGO == null)
        {
            result.success = false;
            result.message = $"GameObject '{command.target}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        ConditionalActivator activator = targetGO.GetComponent<ConditionalActivator>();
        if (activator == null)
        {
            activator = targetGO.AddComponent<ConditionalActivator>();
        }

        activator.conditionType = command.conditionType;
        activator.conditionValue = command.conditionValue;

        EditorUtility.SetDirty(targetGO);
        result.success = true;
        result.message = $"GameObject '{command.target}'에 조건부 활성화가 설정되었습니다. 조건: {command.conditionType} {command.conditionValue}";
        Debug.Log(result.message);
        return result;
    }
}
#endif