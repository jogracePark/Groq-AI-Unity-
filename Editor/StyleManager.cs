#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using SweetHome.Editor.Models;
using TMPro;

public class ThemeManager
{
    private readonly GameObjectManager _gameObjectManager = new GameObjectManager();
    private readonly ScriptableObjectManager _scriptableObjectManager = new ScriptableObjectManager();

    public CommandResult CreateTheme(GroqUnityCommand command)
    {
        if (string.IsNullOrEmpty(command.assetPath) || string.IsNullOrEmpty(command.assetName))
        {
            return new CommandResult { success = false, message = "테마를 생성하려면 'assetPath'와 'assetName'이 필요합니다." };
        }

        // Use ScriptableObjectManager to create the asset instance
        var createSOCommand = new GroqUnityCommand
        {
            commandType = "CreateScriptableObject",
            typeName = "UITheme",
            folderPath = command.assetPath,
            assetName = command.assetName,
            overwrite = command.overwrite,
            patches = command.patches // Pass along any initial property patches
        };

        CommandResult result = _scriptableObjectManager.CreateScriptableObject(createSOCommand);
        if(result.success)
        {
            result.message = $"UI 테마 '{command.assetName}'이(가) '{command.assetPath}'에 생성되었습니다.";
            Debug.Log(result.message);
        }
        return result;
    }

    public CommandResult ApplyThemeStyle(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target) || string.IsNullOrEmpty(command.themeAssetPath) || string.IsNullOrEmpty(command.styleType))
        {
            result.success = false;
            result.message = "테마 스타일을 적용하려면 'target', 'themeAssetPath', 'styleType'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        // Load the theme asset
        UITheme theme = AssetDatabase.LoadAssetAtPath<UITheme>(command.themeAssetPath);
        if (theme == null)
        {
            result.success = false;
            result.message = $"UI 테마 에셋을 찾을 수 없습니다: {command.themeAssetPath}";
            Debug.LogWarning(result.message);
            return result;
        }

        // Find the target GameObject
        GameObject targetGO = _gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.target });
        if (targetGO == null)
        {
            result.success = false;
            result.message = $"GameObject '{command.target}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        // Apply styles based on styleType
        bool applied = false;
        switch (command.styleType.ToLower())
        {
            case "primarycolor":
                applied = TryApplyColor(targetGO, theme.primaryColor);
                break;
            case "secondarycolor":
                applied = TryApplyColor(targetGO, theme.secondaryColor);
                break;
            case "backgroundcolor":
                applied = TryApplyColor(targetGO, theme.backgroundColor);
                break;
            case "accentcolor":
                applied = TryApplyColor(targetGO, theme.accentColor);
                break;
            case "textcolor":
                applied = TryApplyTextColor(targetGO, theme.textColor);
                break;
            case "h1":
                applied = TryApplyFontStyle(targetGO, theme.h1);
                break;
            case "body":
                applied = TryApplyFontStyle(targetGO, theme.body);
                break;
            case "caption":
                applied = TryApplyFontStyle(targetGO, theme.caption);
                break;
            default:
                result.success = false;
                result.message = $"알 수 없는 스타일 유형: {command.styleType}";
                Debug.LogWarning(result.message);
                return result;
        }

        if (applied)
        {
            EditorUtility.SetDirty(targetGO);
            result.success = true;
            result.message = $"스타일 '{command.styleType}'이(가) 테마 '{theme.name}'에서 '{command.target}'(으)로 적용되었습니다.";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"스타일 '{command.styleType}'을(를) '{command.target}'에 적용할 수 없습니다. 호환되는 컴포넌트(Image, TextMeshProUGUI)가 있는지 확인하세요.";
            Debug.LogWarning(result.message);
        }

        return result;
    }

    private bool TryApplyColor(GameObject go, Color color)
    {
        UnityEngine.UI.Image image = go.GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            image.color = color;
            return true;
        }
        return false;
    }

    private bool TryApplyTextColor(GameObject go, Color color)
    {
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.color = color;
            return true;
        }
        return false;
    }

    private bool TryApplyFontStyle(GameObject go, UITheme.FontStyle fontStyle)
    {
        if (fontStyle == null) return false;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            if (fontStyle.fontAsset != null)
            {
                tmp.font = fontStyle.fontAsset;
            }
            tmp.fontSize = fontStyle.fontSize;
            tmp.fontStyle = fontStyle.style;
            return true;
        }
        return false;
    }
}
#endif
