#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using SweetHome.Editor.Models;
using UnityEngine.UI;

public class ComponentManager
{
    static ComponentManager()
    {
        CommandRegistry.RegisterCommand(new CommandMetadata(
            commandType: "AddComponent",
            description: "Adds a component to a GameObject.",
            requiredParameters: new List<string> { "target", "componentType" },
            optionalParameters: new List<string> { "patches", "searchMethod", "parentName", "ancestorName", "requiredComponent", "targetIndex" },
            managerName: "ComponentManager"
        ));

        CommandRegistry.RegisterCommand(new CommandMetadata(
            commandType: "RemoveComponent",
            description: "Removes a component from a GameObject.",
            isDestructive: true,
            requiredParameters: new List<string> { "target", "componentType" },
            optionalParameters: new List<string> { "searchMethod", "parentName", "ancestorName", "requiredComponent", "targetIndex" },
            managerName: "ComponentManager"
        ));

        CommandRegistry.RegisterCommand(new CommandMetadata(
            commandType: "ModifyComponentProperties",
            description: "Modifies properties of a component on a GameObject.",
            requiredParameters: new List<string> { "target", "componentType", "patches" },
            optionalParameters: new List<string> { "searchMethod", "parentName", "ancestorName", "requiredComponent", "targetIndex" },
            managerName: "ComponentManager"
        ));
    }

    private static readonly Dictionary<string, Dictionary<string, string>> _propertyMappings = new Dictionary<string, Dictionary<string, string>>
    {
        { "Image", new Dictionary<string, string> { { "color", "m_Color" } } },
        { "TextMeshProUGUI", new Dictionary<string, string> { { "fontSize", "m_fontSize" }, { "color", "m_fontColor" }, { "font", "m_fontAsset" } } },
        { "RectTransform", new Dictionary<string, string> { { "offsetMin", "m_OffsetMin" }, { "offsetMax", "m_OffsetMax" } } },
        { "CanvasScaler", new Dictionary<string, string> { { "uiScaleMode", "m_UiScaleMode" }, { "referenceResolution", "m_ReferenceResolution" }, { "screenMatchMode", "m_ScreenMatchMode" }, { "match", "m_MatchWidthOrHeight" } } },
        { "Button", new Dictionary<string, string> {
            { "interactable", "m_Interactable" },
            { "targetGraphic", "m_TargetGraphic" },
            { "transition", "m_Transition" },
            { "colors", "m_ColorBlock" }, // Note: This is for ColorBlock struct
            { "spriteState", "m_SpriteState" }, // Note: This is for SpriteState struct
            { "animationTriggers", "m_AnimationTriggers" },
            { "navigation", "m_Navigation" },
            { "onClick", "m_OnClick" } // Note: This is for UnityEvent
        }},
        { "Toggle", new Dictionary<string, string> {
            { "graphic", "m_Graphic" },
            { "group", "m_Group" },
            { "isOn", "m_IsOn" },
            { "onValueChanged", "m_OnValueChanged" },
            { "toggleTransition", "m_ToggleTransition" }
            // Inherits from Selectable, so interactable, targetGraphic, transition, colors, spriteState, animationTriggers, navigation are covered by Selectable's m_ names.
        }},
        { "Slider", new Dictionary<string, string> {
            { "fillRect", "m_FillRect" },
            { "handleRect", "m_HandleRect" },
            { "direction", "m_Direction" },
            { "minValue", "m_MinValue" },
            { "maxValue", "m_MaxValue" },
            { "wholeNumbers", "m_WholeNumbers" },
            { "value", "m_Value" },
            { "onValueChanged", "m_OnValueChanged" }
            // Inherits from Selectable
        }},
        { "TMP_InputField", new Dictionary<string, string> {
            { "text", "m_Text" },
            { "characterLimit", "m_CharacterLimit" },
            { "contentType", "m_ContentType" },
            { "lineType", "m_LineType" },
            { "placeholder", "m_Placeholder" },
            { "caretColor", "m_CaretColor" },
            { "selectionColor", "m_SelectionColor" },
            { "fontAsset", "m_FontAsset" },
            { "pointSize", "m_PointSize" },
            { "onValueChanged", "m_OnValueChanged" },
            { "onEndEdit", "m_OnEndEdit" },
            { "keyboardType", "m_KeyboardType" },
            { "inputType", "m_InputType" },
            { "asteriskChar", "m_AsteriskChar" },
            { "caretBlinkRate", "m_CaretBlinkRate" },
            { "customCaretColor", "m_CustomCaretColor" },
            { "textViewport", "m_TextViewport" },
            { "richText", "m_RichText" },
            { "readOnly", "m_ReadOnly" }
            // Inherits from Selectable
        }},
        { "ScrollRect", new Dictionary<string, string> {
            { "content", "m_Content" },
            { "horizontal", "m_Horizontal" },
            { "vertical", "m_Vertical" },
            { "movementType", "m_MovementType" },
            { "elasticity", "m_Elasticity" },
            { "inertia", "m_Inertia" },
            { "decelerationRate", "m_DecelerationRate" },
            { "scrollSensitivity", "m_ScrollSensitivity" },
            { "viewport", "m_Viewport" },
            { "horizontalScrollbar", "m_HorizontalScrollbar" },
            { "verticalScrollbar", "m_VerticalScrollbar" },
            { "horizontalScrollbarVisibility", "m_HorizontalScrollbarVisibility" },
            { "verticalScrollbarVisibility", "m_VerticalScrollbarVisibility" },
            { "horizontalScrollbarSpacing", "m_HorizontalScrollbarSpacing" },
            { "verticalScrollbarSpacing", "m_VerticalScrollbarSpacing" },
            { "onValueChanged", "m_OnValueChanged" }
        }},
        { "Canvas", new Dictionary<string, string> {
            { "renderMode", "m_RenderMode" },
            { "worldCamera", "m_WorldCamera" },
            { "pixelPerfect", "m_PixelPerfect" },
            { "planeDistance", "m_PlaneDistance" },
            { "sortingLayerID", "m_SortingLayerID" },
            { "sortingOrder", "m_SortingOrder" },
            { "additionalShaderChannels", "m_AdditionalShaderChannels" },
            { "scaleFactor", "m_ScaleFactor" },
            { "referencePixelsPerUnit", "m_ReferencePixelsPerUnit" },
            { "overrideSorting", "m_OverrideSorting" },
            { "overridePixelPerfect", "m_OverridePixelPerfect" },
            { "normalizedCanvasSize", "m_NormalizedCanvasSize" }
        }},
        { "ToggleGroup", new Dictionary<string, string> {
            { "allowSwitchOff", "m_AllowSwitchOff" }
        }},
        { "Scrollbar", new Dictionary<string, string> {
            { "handleRect", "m_HandleRect" },
            { "direction", "m_Direction" },
            { "value", "m_Value" },
            { "size", "m_Size" },
            { "numberOfSteps", "m_NumberOfSteps" },
            { "onValueChanged", "m_OnValueChanged" }
            // Inherits from Selectable
        }},
        { "Mask", new Dictionary<string, string> {
            { "showMaskGraphic", "m_ShowMaskGraphic" }
        }},
        { "RectMask2D", new Dictionary<string, string> {
            { "padding", "m_Padding" },
            { "softness", "m_Softness" }
        }}
    };

    private string GetInternalPropertyName(string componentType, string userFriendlyPropertyName)
    {
        if (string.IsNullOrEmpty(componentType))
        {
            Debug.LogError($"[ComponentManager] GetInternalPropertyName: componentType is null or empty. Returning userFriendlyPropertyName: {userFriendlyPropertyName}");
            return userFriendlyPropertyName; // Should be caught by validation, but defensive
        }

        if (string.IsNullOrEmpty(userFriendlyPropertyName))
        {
            Debug.LogError($"[ComponentManager] GetInternalPropertyName: userFriendlyPropertyName is null or empty for componentType: {componentType}.");
            return userFriendlyPropertyName; // Defensive check
        }

        if (_propertyMappings.ContainsKey(componentType))
        {
            var componentMap = _propertyMappings[componentType];
            if (componentMap.ContainsKey(userFriendlyPropertyName))
            {
                return componentMap[userFriendlyPropertyName];
            }
            else
            {
                Debug.LogWarning($"[ComponentManager] GetInternalPropertyName: No internal mapping found for property '{userFriendlyPropertyName}' in componentType '{componentType}'. Returning userFriendlyPropertyName.");
            }
        }
        else
        {
            Debug.LogWarning($"[ComponentManager] GetInternalPropertyName: No internal mapping found for componentType '{componentType}'. Returning userFriendlyPropertyName.");
        }
        
        return userFriendlyPropertyName; // Default to user-friendly name if no mapping found
    }

    public CommandResult AddComponentToGameObject(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target) || string.IsNullOrEmpty(command.componentType))
        {
            result.success = false;
            result.message = "컴포넌트를 추가하려면 'target' GameObject와 'componentType'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        var gameObjectManager = new GameObjectManager();
        GameObject targetGO = gameObjectManager.FindGameObject(command);
        if (targetGO == null)
        {
            result.success = false;
            result.message = $"타겟 GameObject를 찾을 수 없습니다: {command.target}";
            Debug.LogWarning(result.message);
            return result;
        }

        Type type = FindType(command.componentType);
        if (type == null)
        {
            result.success = false;
            result.message = $"컴포넌트 타입 '{command.componentType}'을(를) 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        if (!typeof(Component).IsAssignableFrom(type))
        {
            result.success = false;
            result.message = $"'{command.componentType}'은(는) Component 타입이 아닙니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        Component newComponent = targetGO.AddComponent(type);
        if (newComponent != null)
        {
            if (command.patches != null && command.patches.Length > 0)
            {
                GroqUnityCommand modifyCmd = new GroqUnityCommand
                {
                    commandType = "ModifyComponentProperties",
                    target = command.target,
                    searchMethod = command.searchMethod,
                    componentType = command.componentType,
                    patches = command.patches
                };
                CommandResult modifyResult = ModifyComponentProperties(modifyCmd);
                if (!modifyResult.success)
                {
                    Debug.LogWarning($"추가된 컴포넌트의 초기 속성 설정 실패: {modifyResult.message}");
                }
            }

            result.success = true;
            result.message = $"GameObject '{targetGO.name}'에 컴포넌트 '{command.componentType}' 추가됨.";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"GameObject '{targetGO.name}'에 컴포넌트 '{command.componentType}' 추가 실패.";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    public CommandResult RemoveComponentFromGameObject(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target) || string.IsNullOrEmpty(command.componentType))
        {
            result.success = false;
            result.message = "컴포넌트를 제거하려면 'target' GameObject와 'componentType'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        var gameObjectManager = new GameObjectManager();
        GameObject targetGO = gameObjectManager.FindGameObject(command);
        if (targetGO == null)
        {
            result.success = false;
            result.message = $"타겟 GameObject를 찾을 수 없습니다: {command.target}";
            Debug.LogWarning(result.message);
            return result;
        }

        Component componentToRemove = targetGO.GetComponent(command.componentType);
        if (componentToRemove != null)
        {
            UnityEngine.Object.DestroyImmediate(componentToRemove);
            result.success = true;
            result.message = $"GameObject '{targetGO.name}'에서 컴포넌트 '{command.componentType}' 제거됨.";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"GameObject '{targetGO.name}'에서 컴포넌트 '{command.componentType}'을(를) 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    public CommandResult ModifyComponentProperties(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target) || string.IsNullOrEmpty(command.componentType) || command.patches == null || command.patches.Length == 0)
        {
            result.success = false;
            result.message = "컴포넌트 속성을 수정하려면 'target' GameObject, 'componentType' 및 'patches'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        var gameObjectManager = new GameObjectManager();
        GameObject targetGO = gameObjectManager.FindGameObject(command);
        if (targetGO == null)
        {
            result.success = false;
            result.message = $"타겟 GameObject를 찾을 수 없습니다: {command.target}";
            Debug.LogWarning(result.message);
            return result;
        }

        Component targetComponent;
        // Type componentType = null; // This line is no longer needed here

        // Direct type mapping for common UI components
        if (command.componentType.Equals("Image", StringComparison.OrdinalIgnoreCase))
        {
            targetComponent = targetGO.GetComponent<UnityEngine.UI.Image>();
        }
        else if (command.componentType.Equals("TextMeshProUGUI", StringComparison.OrdinalIgnoreCase))
        {
            targetComponent = targetGO.GetComponent<TMPro.TextMeshProUGUI>();
        }
        else if (command.componentType.Equals("Canvas", StringComparison.OrdinalIgnoreCase))
        {
            targetComponent = targetGO.GetComponent<UnityEngine.Canvas>();
        }
        else if (command.componentType.Equals("CanvasScaler", StringComparison.OrdinalIgnoreCase))
        {
            targetComponent = targetGO.GetComponent<UnityEngine.UI.CanvasScaler>();
        }
        else if (command.componentType.Equals("RectTransform", StringComparison.OrdinalIgnoreCase))
        {
            targetComponent = targetGO.GetComponent<RectTransform>();
        }
        else // Fallback to FindType for other components
        {
            Type componentType = FindType(command.componentType);
            if (componentType == null)
            {
                result.success = false;
                result.message = $"컴포넌트 타입 '{command.componentType}'을(를) 찾을 수 없습니다.";
                Debug.LogWarning(result.message);
                return result;
            }
            targetComponent = targetGO.GetComponent(componentType);
        }

        if (targetComponent == null)
        {
            result.success = false;
            result.message = $"GameObject '{targetGO.name}'에서 컴포넌트 '{command.componentType}'을(를) 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        bool modified = false;

        if (targetComponent is RectTransform rectTransform)
        {
            foreach (var patch in command.patches)
            {
                try
                {
                    switch (patch.propertyName)
                    {
                        case "anchoredPosition":
                            rectTransform.anchoredPosition = (Vector2)ComponentManager.ConvertValue(patch.value, typeof(Vector2));
                            modified = true;
                            break;
                        case "sizeDelta":
                            rectTransform.sizeDelta = (Vector2)ComponentManager.ConvertValue(patch.value, typeof(Vector2));
                            modified = true;
                            break;
                        case "anchorMin":
                            rectTransform.anchorMin = (Vector2)ComponentManager.ConvertValue(patch.value, typeof(Vector2));
                            modified = true;
                            break;
                        case "anchorMax":
                            rectTransform.anchorMax = (Vector2)ComponentManager.ConvertValue(patch.value, typeof(Vector2));
                            modified = true;
                            break;
                        case "pivot":
                            rectTransform.pivot = (Vector2)ComponentManager.ConvertValue(patch.value, typeof(Vector2));
                            modified = true;
                            break;
                        case "offsetMin": // Direct handling for offsetMin
                            rectTransform.offsetMin = (Vector2)ComponentManager.ConvertValue(patch.value, typeof(Vector2));
                            modified = true;
                            break;
                        case "offsetMax": // Direct handling for offsetMax
                            rectTransform.offsetMax = (Vector2)ComponentManager.ConvertValue(patch.value, typeof(Vector2));
                            modified = true;
                            break;
                        case "localScale":
                            rectTransform.localScale = (Vector3)ComponentManager.ConvertValue(patch.value, typeof(Vector3));
                            modified = true;
                            break;
                        default:
                            // Fallback to SerializedProperty for other RectTransform properties
                            string internalPropertyName = GetInternalPropertyName(command.componentType, patch.propertyName);
                            SerializedObject serializedObjectFallback = new SerializedObject(targetComponent);
                            SerializedProperty propertyFallback = serializedObjectFallback.FindProperty(internalPropertyName);
                            if (propertyFallback != null && ApplyPatch(propertyFallback, patch.value))
                            {
                                serializedObjectFallback.ApplyModifiedProperties();
                                modified = true;
                            }
                            else
                            {
                                result.message = $"RectTransform 프로퍼티 '{patch.propertyName}'에 값 '{patch.value}'를 적용할 수 없습니다.";
                                Debug.LogWarning(result.message);
                                modified = false; // Mark as failed
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    result.message = $"RectTransform 프로퍼티 '{patch.propertyName}'에 값 '{patch.value}'를 적용하는 중 오류 발생: {e.Message}";
                    Debug.LogError(result.message);
                    modified = false; // Mark as failed
                    break; // Stop processing patches on first error
                }
                if (!modified) break; // If a patch failed, stop batch processing
            }
            if (modified)
            {
                EditorUtility.SetDirty(targetComponent);
                result.success = true;
                result.message = $"GameObject '{targetGO.name}'의 컴포넌트 '{command.componentType}' 속성 수정됨.";
            }
            else
            {
                result.success = false;
                if (string.IsNullOrEmpty(result.message))
                {
                    result.message = $"GameObject '{targetGO.name}'의 컴포넌트 '{command.componentType}' 속성 수정 실패 또는 변경 사항 없음.";
                }
            }
        }
        else // Not a RectTransform, use generic SerializedObject approach
        {
            // ... (existing generic SerializedObject approach) ...
            SerializedObject serializedObject = new SerializedObject(targetComponent);
            foreach (var patch in command.patches)
            {
                string internalPropertyName = GetInternalPropertyName(command.componentType, patch.propertyName);
                SerializedProperty property = serializedObject.FindProperty(internalPropertyName);
                if (property == null)
                {
                    result.message = $"프로퍼티를 찾을 수 없습니다: {patch.propertyName}";
                    Debug.LogWarning(result.message);
                    modified = false; // Mark as failed
                    break; // Stop processing patches on first error
                }

                if (ApplyPatch(property, patch.value))
                {
                    modified = true;
                }
                else
                {
                    result.message = $"프로퍼티 '{property.name}'에 값 '{patch.value}'를 적용하는 중 오류 발생.";
                    modified = false; // Mark as failed
                    break; // Stop processing patches on first error
                }
            }

            if (modified)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(targetComponent);
                result.success = true;
                result.message = $"GameObject '{targetGO.name}'의 컴포넌트 '{command.componentType}' 속성 수정됨.";
            }
            else
            {
                result.success = false;
                if (string.IsNullOrEmpty(result.message))
                {
                    result.message = $"GameObject '{targetGO.name}'의 컴포넌트 '{command.componentType}' 속성 수정 실패 또는 변경 사항 없음.";
                }
            }
        }
        return result;
    }
    public static object ConvertValue(string value, Type targetType)
    {
        if (targetType == typeof(int)) return int.Parse(value);
        if (targetType == typeof(bool)) return bool.Parse(value);
        if (targetType == typeof(float)) return float.Parse(value);
        if (targetType == typeof(string)) return value;
        
        string cleanValue = value.Trim();
        if (cleanValue.StartsWith("[") && cleanValue.EndsWith("]"))
        {
            cleanValue = cleanValue.Substring(1, cleanValue.Length - 2);
        }

        if (targetType == typeof(Color))
        {
            string[] components = cleanValue.Split(',');
            if (components.Length >= 3)
            {
                float r = float.Parse(components[0]);
                float g = float.Parse(components[1]);
                float b = float.Parse(components[2]);
                float a = components.Length == 4 ? float.Parse(components[3]) : 1f;
                return new Color(r, g, b, a);
            }
            return Color.white;
        }
        if (targetType == typeof(Vector2))
        {
            string[] components = cleanValue.Split(',');
            if (components.Length == 2) return new Vector2(float.Parse(components[0]), float.Parse(components[1]));
            return Vector2.zero;
        }
        if (targetType == typeof(Vector3))
        {
            string[] components = cleanValue.Split(',');
            if (components.Length == 3) return new Vector3(float.Parse(components[0]), float.Parse(components[1]), float.Parse(components[2]));
            return Vector3.zero;
        }
        if (targetType == typeof(Vector4))
        {
            string[] components = cleanValue.Split(',');
            if (components.Length == 4) return new Vector4(float.Parse(components[0]), float.Parse(components[1]), float.Parse(components[2]), float.Parse(components[3]));
            return Vector4.zero;
        }
        if (targetType.IsEnum)
        {
            return Enum.Parse(targetType, value, true);
        }
        return null;
    }

    private bool ApplyPatch(SerializedProperty property, string value)
    {
        try
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = (int)ConvertValue(value, typeof(int));
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = (bool)ConvertValue(value, typeof(bool));
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = (float)ConvertValue(value, typeof(float));
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = (string)ConvertValue(value, typeof(string));
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = (Color)ConvertValue(value, typeof(Color));
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(value);
                    break;
                case SerializedPropertyType.LayerMask:
                    property.intValue = LayerMask.NameToLayer(value);
                    break;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = Array.IndexOf(property.enumNames, value);
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = (Vector2)ConvertValue(value, typeof(Vector2));
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = (Vector3)ConvertValue(value, typeof(Vector3));
                    break;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = (Vector4)ConvertValue(value, typeof(Vector4));
                    break;
                default:
                    Debug.LogWarning($"지원되지 않는 프로퍼티 타입: {property.propertyType}");
                    return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"프로퍼티 '{property.name}'에 값 '{value}'를 적용하는 중 오류 발생: {e.Message}");
            return false;
        }
        return true;
    }

    private Type FindType(string typeName)
    {
        var type = Type.GetType(typeName);
        if (type != null) return type;
        foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = a.GetType(typeName);
            if (type != null) return type;
        }
        return null;
    }
}
#endif