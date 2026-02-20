using SweetHome.Editor.Models;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic; // Added for List

#if UNITY_EDITOR
public class LayoutManager
{
    static LayoutManager()
    {
        CommandRegistry.RegisterCommand(new CommandMetadata(
            commandType: "AddLayoutGroup",
            description: "Adds a layout group (Horizontal, Vertical, Grid) to a GameObject.",
            requiredParameters: new List<string> { "target", "layoutGroupType" },
            optionalParameters: new List<string> { "spacing", "padding", "cellSize", "startCorner", "startAxis", "childAlignment", "constraint", "constraintCount", "childControlWidth", "childControlHeight", "childForceExpandWidth", "childForceExpandHeight" },
            managerName: "LayoutManager"
        ));

        CommandRegistry.RegisterCommand(new CommandMetadata(
            commandType: "AddContentSizeFitter",
            description: "Adds a ContentSizeFitter component to a GameObject.",
            requiredParameters: new List<string> { "target" },
            optionalParameters: new List<string> { "horizontalFit", "verticalFit" },
            managerName: "LayoutManager"
        ));

        CommandRegistry.RegisterCommand(new CommandMetadata(
            commandType: "SetLayoutElementProperties",
            description: "Sets properties of a LayoutElement component on a GameObject.",
            requiredParameters: new List<string> { "target" },
            optionalParameters: new List<string> { "minWidth", "minHeight", "preferredWidth", "preferredHeight", "flexibleWidth", "flexibleHeight", "layoutPriority", "ignoreLayout" },
            managerName: "LayoutManager"
        ));
    }

    private readonly GameObjectManager _gameObjectManager = new GameObjectManager();

    public CommandResult AddLayoutGroup(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target))
        {
            result.success = false;
            result.message = "레이아웃 그룹을 추가할 GameObject의 'target'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        if (string.IsNullOrEmpty(command.layoutGroupType))
        {
            result.success = false;
            result.message = "추가할 레이아웃 그룹의 'layoutGroupType'이 필요합니다 (Horizontal, Vertical, Grid).";
            Debug.LogWarning(result.message);
            return result;
        }

        GameObject targetGO = _gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.target });
        if (targetGO == null)
        {
            result.success = false;
            result.message = $"GameObject '{command.target}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        LayoutGroup layoutGroup = null;
        switch (command.layoutGroupType.ToLower())
        {
            case "horizontal":
                layoutGroup = targetGO.AddComponent<HorizontalLayoutGroup>();
                break;
            case "vertical":
                layoutGroup = targetGO.AddComponent<VerticalLayoutGroup>();
                break;
            case "grid":
                layoutGroup = targetGO.AddComponent<GridLayoutGroup>();
                break;
            default:
                result.success = false;
                result.message = $"알 수 없는 layoutGroupType: {command.layoutGroupType}. (Horizontal, Vertical, Grid 중 하나여야 합니다)";
                Debug.LogWarning(result.message);
                return result;
        }

        if (layoutGroup != null)
        {
            // Apply common layout group properties
            if (command.spacing != null) // Original check
            {
                if (layoutGroup is HorizontalOrVerticalLayoutGroup horizVertLayout)
                {
                    horizVertLayout.spacing = (float)command.spacing; // Original cast
                }
                else if (layoutGroup is GridLayoutGroup gridLayoutCommon)
                {
                    gridLayoutCommon.spacing = new Vector2(command.spacing, command.spacing); // Assign as Vector2
                }
            }
            if (command.padding != null && command.padding.Length == 4)
            {
                layoutGroup.padding.left = (int)command.padding[0];
                layoutGroup.padding.right = (int)command.padding[1];
                layoutGroup.padding.top = (int)command.padding[2];
                layoutGroup.padding.bottom = (int)command.padding[3];
            }

            if (layoutGroup is GridLayoutGroup gridLayoutSpecific)
            {
                if (command.cellSize != null && command.cellSize.Length == 2)
                {
                    gridLayoutSpecific.cellSize = new Vector2(command.cellSize[0], command.cellSize[1]);
                }
                if (command.startCorner != null)
                {
                    GridLayoutGroup.Corner corner;
                    if (System.Enum.TryParse(command.startCorner, true, out corner))
                    {
                        gridLayoutSpecific.startCorner = corner;
                    }
                }
                if (command.startAxis != null)
                {
                    GridLayoutGroup.Axis axis;
                    if (System.Enum.TryParse(command.startAxis, true, out axis))
                    {
                        gridLayoutSpecific.startAxis = axis;
                    }
                }
                if (command.childAlignment != null)
                {
                    TextAnchor alignment;
                    if (System.Enum.TryParse(command.childAlignment, true, out alignment))
                    {
                        gridLayoutSpecific.childAlignment = alignment;
                    }
                }
                if (command.constraint != null)
                {
                    GridLayoutGroup.Constraint constraint;
                    if (System.Enum.TryParse(command.constraint, true, out constraint))
                    {
                        gridLayoutSpecific.constraint = constraint;
                    }
                }
                if (command.constraintCount != null) // Original check
                {
                    gridLayoutSpecific.constraintCount = command.constraintCount; // Original cast
                }
            }
            else // Horizontal or Vertical Layout Group
            {
                if (command.childAlignment != null)
                {
                    TextAnchor alignment;
                    if (System.Enum.TryParse(command.childAlignment, true, out alignment))
                    {
                        layoutGroup.childAlignment = alignment;
                    }
                }
                if (command.childForceExpandWidth != null) // Original check
                {
                    if (layoutGroup is HorizontalLayoutGroup hGroup) hGroup.childForceExpandWidth = command.childForceExpandWidth; // Original cast
                    if (layoutGroup is VerticalLayoutGroup vGroup) vGroup.childForceExpandWidth = command.childForceExpandWidth; // Original cast
                }
                if (command.childForceExpandHeight != null) // Original check
                {
                    if (layoutGroup is HorizontalLayoutGroup hGroup) hGroup.childForceExpandHeight = command.childForceExpandHeight; // Original cast
                    if (layoutGroup is VerticalLayoutGroup vGroup) vGroup.childForceExpandHeight = command.childForceExpandHeight; // Original cast
                }
            }

            Undo.RegisterCreatedObjectUndo(layoutGroup, $"Add {command.layoutGroupType} Layout Group");
            EditorUtility.SetDirty(targetGO);
            result.success = true;
            result.message = $"{command.layoutGroupType} Layout Group이 '{command.target}'에 추가되었습니다.";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"레이아웃 그룹을 '{command.target}'에 추가하지 못했습니다.";
            Debug.LogError(result.message);
        }
        return result;
    }

    public CommandResult AddContentSizeFitter(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target))
        {
            result.success = false;
            result.message = "ContentSizeFitter를 추가할 GameObject의 'target'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        GameObject targetGO = _gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.target });
        if (targetGO == null)
        {
            result.success = false;
            result.message = $"GameObject '{command.target}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        ContentSizeFitter fitter = targetGO.AddComponent<ContentSizeFitter>();
        if (command.horizontalFit != null)
        {
            ContentSizeFitter.FitMode mode;
            if (System.Enum.TryParse(command.horizontalFit, true, out mode))
            {
                fitter.horizontalFit = mode;
            }
        }
        if (command.verticalFit != null)
        {
            ContentSizeFitter.FitMode mode;
            if (System.Enum.TryParse(command.verticalFit, true, out mode))
            {
                fitter.verticalFit = mode;
            }
        }

        Undo.RegisterCreatedObjectUndo(fitter, "Add Content Size Fitter");
        EditorUtility.SetDirty(targetGO);
        result.success = true;
        result.message = $"ContentSizeFitter가 '{command.target}'에 추가되었습니다.";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult SetLayoutElementProperties(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target))
        {
            result.success = false;
            result.message = "LayoutElement 속성을 설정할 GameObject의 'target'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        GameObject targetGO = _gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.target });
        if (targetGO == null)
        {
            result.success = false;
            result.message = $"GameObject '{command.target}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        LayoutElement layoutElement = targetGO.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = targetGO.AddComponent<LayoutElement>();
        }

        if (command.minWidth != null) layoutElement.minWidth = command.minWidth; // Original check and cast
        if (command.minHeight != null) layoutElement.minHeight = command.minHeight; // Original check and cast
        if (command.preferredWidth != null) layoutElement.preferredWidth = command.preferredWidth; // Original check and cast
        if (command.preferredHeight != null) layoutElement.preferredHeight = command.preferredHeight; // Original check and cast
        if (command.flexibleWidth != null) layoutElement.flexibleWidth = command.flexibleWidth; // Original check and cast
        if (command.flexibleHeight != null) layoutElement.flexibleHeight = command.flexibleHeight; // Original check and cast
        if (command.layoutPriority != null) layoutElement.layoutPriority = command.layoutPriority; // Original check and cast

        EditorUtility.SetDirty(layoutElement);
        result.success = true;
        result.message = $"LayoutElement 속성이 '{command.target}'에 설정되었습니다.";
        Debug.Log(result.message);
        return result;
    }
}
#endif
