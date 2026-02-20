using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using SweetHome.Editor.Models;

#if UNITY_EDITOR
public class UIManager
{
    private void ApplyRectTransformProperties(GameObject go, GroqUnityCommand command)
    {
        RectTransform rectTransform = go.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        if (command.sizeDelta != null && command.sizeDelta.Length == 2)
        {
            rectTransform.sizeDelta = new Vector2(command.sizeDelta[0], command.sizeDelta[1]);
        }
        if (command.anchoredPosition != null) // Vector2 is a struct, so it's never null. Check if it's default.
        {
            // Only apply if it's not the default (0,0) or if explicitly set in command
            if (command.anchoredPosition != Vector2.zero || (command.parameters != null && command.parameters["anchoredPosition"] != null))
            {
                rectTransform.anchoredPosition = command.anchoredPosition;
            }
        }
        if (command.uiPosition != null && command.uiPosition.Length == 2)
        {
            rectTransform.anchoredPosition = new Vector2(command.uiPosition[0], command.uiPosition[1]);
        }
        if (command.anchorMin != null)
        {
            rectTransform.anchorMin = command.anchorMin;
        }
        if (command.anchorMax != null)
        {
            rectTransform.anchorMax = command.anchorMax;
        }
        if (command.pivot != null)
        {
            rectTransform.pivot = command.pivot;
        }
        if (!string.IsNullOrEmpty(command.anchorPreset))
        {
            // This logic is duplicated from SetRectTransformAnchors for convenience during creation.
            // For more complex scenarios, SetRectTransformAnchors should be called separately.
            switch (command.anchorPreset.ToLower())
            {
                case "topleft":
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(0, 1);
                    rectTransform.pivot = new Vector2(0, 1);
                    break;
                case "topcenter":
                    rectTransform.anchorMin = new Vector2(0.5f, 1);
                    rectTransform.anchorMax = new Vector2(0.5f, 1);
                    rectTransform.pivot = new Vector2(0.5f, 1);
                    break;
                case "topright":
                    rectTransform.anchorMin = new Vector2(1, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(1, 1);
                    break;
                case "middleleft":
                    rectTransform.anchorMin = new Vector2(0, 0.5f);
                    rectTransform.anchorMax = new Vector2(0, 0.5f);
                    rectTransform.pivot = new Vector2(0, 0.5f);
                    break;
                case "middlecenter":
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    break;
                case "middleright":
                    rectTransform.anchorMin = new Vector2(1, 0.5f);
                    rectTransform.anchorMax = new Vector2(1, 0.5f);
                    rectTransform.pivot = new Vector2(1, 0.5f);
                    break;
                case "bottomleft":
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(0, 0);
                    rectTransform.pivot = new Vector2(0, 0);
                    break;
                case "bottomcenter":
                    rectTransform.anchorMin = new Vector2(0.5f, 0);
                    rectTransform.anchorMax = new Vector2(0.5f, 0);
                    rectTransform.pivot = new Vector2(0.5f, 0);
                    break;
                case "bottomright":
                    rectTransform.anchorMin = new Vector2(1, 0);
                    rectTransform.anchorMax = new Vector2(1, 0);
                    rectTransform.pivot = new Vector2(1, 0);
                    break;
                case "stretchleft":
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(0, 1);
                    rectTransform.pivot = new Vector2(0, 0.5f);
                    break;
                case "stretchcenter":
                    rectTransform.anchorMin = new Vector2(0.5f, 0);
                    rectTransform.anchorMax = new Vector2(0.5f, 1);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    break;
                case "stretchright":
                    rectTransform.anchorMin = new Vector2(1, 0);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(1, 0.5f);
                    break;
                case "stretchtop":
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(0.5f, 1);
                    break;
                case "stretchmiddle":
                    rectTransform.anchorMin = new Vector2(0, 0.5f);
                    rectTransform.anchorMax = new Vector2(1, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    break;
                case "stretchbottom":
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(1, 0);
                    rectTransform.pivot = new Vector2(0.5f, 0);
                    break;
                case "stretchall":
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    break;
                default:
                    Debug.LogWarning($"알 수 없는 anchorPreset: {command.anchorPreset}. 기본값으로 설정하지 않습니다.");
                    break;
            }
        }
        if (command.padding != null && command.padding.Length == 4)
        {
            rectTransform.offsetMin = new Vector2(command.padding[0], command.padding[3]); // left, bottom
            rectTransform.offsetMax = new Vector2(-command.padding[1], -command.padding[2]); // -right, -top
        }
    }

    public CommandResult CreateCanvas(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        var gameObjectManager = new GameObjectManager();
        // var componentManager = new ComponentManager(); // Not directly used here

        GroqUnityCommand createGOCommand = new GroqUnityCommand
        {
            commandType = "CreateGameObject",
            name = command.name,
            setActive = true
        };
        CommandResult createGOResult = gameObjectManager.CreateGameObject(createGOCommand);
        if (!createGOResult.success)
        {
            return createGOResult;
        }
        
        GameObject canvasGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.name });
        if (canvasGO == null)
        {
            result.success = false;
            result.message = $"Canvas GameObject '{command.name}' 생성 후 찾기 실패.";
            Debug.LogError(result.message);
            return result;
        }

        canvasGO.AddComponent<Canvas>();
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        Canvas canvas = canvasGO.GetComponent<Canvas>();
        if (!string.IsNullOrEmpty(command.renderMode))
        {
            RenderMode renderModeEnum;
            if (System.Enum.TryParse(command.renderMode, true, out renderModeEnum))
            {
                canvas.renderMode = renderModeEnum;
            }
            else
            {
                Debug.LogWarning($"알 수 없는 RenderMode: {command.renderMode}");
            }
        }
        
        result.success = true;
        result.message = $"Canvas 생성: {command.name}";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult CreateText(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.parent))
        {
            result.success = false;
            result.message = "UI Text를 생성하려면 'parent' Canvas 또는 UI 요소가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        var gameObjectManager = new GameObjectManager();
        GameObject parentGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.parent });
        if (parentGO == null)
        {
            result.success = false;
            result.message = $"부모 GameObject '{command.parent}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        GameObject textGO = new GameObject(command.name, typeof(RectTransform));
        textGO.transform.SetParent(parentGO.transform, false);
        textGO.SetActive(command.setActive);
        ApplyRectTransformProperties(textGO, command); // Apply RectTransform properties

        TMP_Text tmpText = textGO.AddComponent<TextMeshProUGUI>();
        if (!string.IsNullOrEmpty(command.uiText))
        {
            tmpText.text = command.uiText;
        }
        
        if (!string.IsNullOrEmpty(command.fontAssetPath))
        {
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(command.fontAssetPath);
            if (fontAsset != null)
            {
                tmpText.font = fontAsset;
            }
            else
            {
                Debug.LogWarning($"[UIManager] Failed to load font asset at path: {command.fontAssetPath} for '{command.name}'.");
            }
        }

        if (command.patches != null && command.patches.Length > 0)
        {
            var componentManager = new ComponentManager();
            var modifyCmd = new GroqUnityCommand
            {
                commandType = "ModifyComponentProperties",
                target = command.name,
                searchMethod = "by_name",
                parentName = command.parent,
                componentType = "TextMeshProUGUI",
                patches = command.patches
            };
            CommandResult modifyResult = componentManager.ModifyComponentProperties(modifyCmd);
            if (!modifyResult.success)
            {
                Debug.LogWarning($"생성된 Text의 초기 속성 설정 실패: {modifyResult.message}");
            }
        }
        
        result.success = true;
        result.message = $"UI Text 생성: {command.name}";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult CreateButton(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.parent))
        {
            result.success = false;
            result.message = "UI Button을 생성하려면 'parent' Canvas 또는 UI 요소가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        var gameObjectManager = new GameObjectManager();
        GameObject parentGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.parent });
        if (parentGO == null)
        {
            result.success = false;
            result.message = $"부모 GameObject '{command.parent}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        GameObject buttonGO = new GameObject(command.name, typeof(RectTransform));
        buttonGO.transform.SetParent(parentGO.transform, false);
        buttonGO.SetActive(command.setActive);
        ApplyRectTransformProperties(buttonGO, command); // Apply RectTransform properties

        buttonGO.AddComponent<Image>();
        buttonGO.AddComponent<Button>();

        GameObject labelGO = new GameObject("Text (TMP)", typeof(RectTransform));
        labelGO.transform.SetParent(buttonGO.transform, false);
        
        TMP_Text tmpText = labelGO.AddComponent<TextMeshProUGUI>();
        tmpText.text = command.uiText ?? "Button";
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.black;

        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;

        result.success = true;
        result.message = $"UI Button 생성: {command.name}";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult CreateImage(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.parent))
        {
            result.success = false;
            result.message = "UI Image를 생성하려면 'parent' Canvas 또는 UI 요소가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        var gameObjectManager = new GameObjectManager();
        GameObject parentGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.parent });
        if (parentGO == null)
        {
            result.success = false;
            result.message = $"부모 GameObject '{command.parent}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        GameObject imageGO = new GameObject(command.name, typeof(RectTransform));
        imageGO.transform.SetParent(parentGO.transform, false);
        imageGO.SetActive(command.setActive);
        ApplyRectTransformProperties(imageGO, command); // Apply RectTransform properties

        Image image = imageGO.AddComponent<Image>();
        if (!string.IsNullOrEmpty(command.uiSprite))
        {
            Sprite spriteAsset = AssetDatabase.LoadAssetAtPath<Sprite>(command.uiSprite);
            if (spriteAsset != null)
            {
                image.sprite = spriteAsset;
            }
            else
            {
                Debug.LogWarning($"스프라이트 에셋을 찾을 수 없습니다: {command.uiSprite}");
            }
        }
        
        result.success = true;
        result.message = $"UI Image 생성: {command.name}";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult CreateToggle(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.parent))
        {
            result.success = false;
            result.message = "UI Toggle을 생성하려면 'parent' Canvas 또는 UI 요소가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        var gameObjectManager = new GameObjectManager();
        GameObject parentGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.parent });
        if (parentGO == null)
        {
            result.success = false;
            result.message = $"부모 GameObject '{command.parent}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        // 1. 메인 Toggle GameObject 생성
        GameObject toggleGO = new GameObject(command.name ?? "NewToggle", typeof(RectTransform));
        toggleGO.transform.SetParent(parentGO.transform, false);
        toggleGO.AddComponent<CanvasRenderer>();
        Toggle toggle = toggleGO.AddComponent<Toggle>();
        ApplyRectTransformProperties(toggleGO, command); // Apply RectTransform properties

        // RectTransform 설정 (기본값 또는 command에서 가져옴)
        RectTransform toggleRect = toggleGO.GetComponent<RectTransform>();
        if (command.uiPosition != null && command.uiPosition.Length == 2)
        {
            toggleRect.anchoredPosition = new Vector2(command.uiPosition[0], command.uiPosition[1]);
        }
        if (command.sizeDelta != null && command.sizeDelta.Length == 2)
        {
            toggleRect.sizeDelta = new Vector2(command.sizeDelta[0], command.sizeDelta[1]);
        }
        else
        {
            toggleRect.sizeDelta = new Vector2(160, 30); // Default size
        }


        // 2. 배경 이미지 생성 (Toggle Background)
        GameObject backgroundGO = new GameObject("Background", typeof(RectTransform));
        backgroundGO.transform.SetParent(toggleGO.transform, false);
        backgroundGO.AddComponent<CanvasRenderer>();
        Image backgroundImage = backgroundGO.AddComponent<Image>();
        backgroundImage.color = new Color(0.8f, 0.8f, 0.8f, 1f); // Light gray background

        RectTransform backgroundRect = backgroundGO.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0f);
        backgroundRect.anchorMax = new Vector2(0f, 1f);
        backgroundRect.pivot = new Vector2(0f, 0.5f);
        backgroundRect.anchoredPosition = new Vector2(15, 0); // Position to the left
        backgroundRect.sizeDelta = new Vector2(20, 20); // Size of the toggle box

        // 3. 체크마크 이미지 생성 (Toggle Checkmark)
        GameObject checkmarkGO = new GameObject("Checkmark", typeof(RectTransform));
        checkmarkGO.transform.SetParent(backgroundGO.transform, false); // Child of background
        checkmarkGO.AddComponent<CanvasRenderer>();
        Image checkmarkImage = checkmarkGO.AddComponent<Image>();
        checkmarkImage.color = new Color(0.2f, 0.6f, 0.2f, 1f); // Green checkmark
        checkmarkImage.gameObject.SetActive(command.isOn); // Initial state

        RectTransform checkmarkRect = checkmarkGO.GetComponent<RectTransform>();
        checkmarkRect.anchorMin = new Vector2(0f, 0f);
        checkmarkRect.anchorMax = new Vector2(1f, 1f);
        checkmarkRect.pivot = new Vector2(0.5f, 0.5f);
        checkmarkRect.anchoredPosition = Vector2.zero;
        checkmarkRect.sizeDelta = Vector2.zero;

        // 4. 레이블 텍스트 생성 (Toggle Label)
        GameObject labelGO = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(toggleGO.transform, false);
        labelGO.AddComponent<CanvasRenderer>();
        TextMeshProUGUI labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text = command.uiText ?? "Toggle";
        labelTMP.color = Color.white;
        labelTMP.fontSize = 18;
        labelTMP.alignment = TextAlignmentOptions.Left;

        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.pivot = new Vector2(0f, 0.5f);
        labelRect.anchoredPosition = new Vector2(40, 0); // Position to the right of the toggle box
        labelRect.sizeDelta = new Vector2(-40, 0); // Adjust width

        // 5. Toggle 컴포넌트 참조 연결 및 속성 설정
        toggle.targetGraphic = backgroundImage;
        toggle.graphic = checkmarkImage;
        toggle.isOn = command.isOn;

        toggleGO.SetActive(command.setActive);

        result.success = true;
        result.message = $"Toggle 생성됨: {toggleGO.name}";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult CreateSlider(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.parent))
        {
            result.success = false;
            result.message = "UI Slider를 생성하려면 'parent' Canvas 또는 UI 요소가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        var gameObjectManager = new GameObjectManager();
        GameObject parentGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.parent });
        if (parentGO == null)
        {
            result.success = false;
            result.message = $"부모 GameObject '{command.parent}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        // 1. 메인 Slider GameObject 생성
        GameObject sliderGO = new GameObject(command.name ?? "NewSlider", typeof(RectTransform));
        sliderGO.transform.SetParent(parentGO.transform, false);
        sliderGO.AddComponent<CanvasRenderer>();
        Slider slider = sliderGO.AddComponent<Slider>();
        ApplyRectTransformProperties(sliderGO, command); // Apply RectTransform properties

        // RectTransform 설정 (기본값 또는 command에서 가져옴)
        RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
        if (command.uiPosition != null && command.uiPosition.Length == 2)
        {
            sliderRect.anchoredPosition = new Vector2(command.uiPosition[0], command.uiPosition[1]);
        }
        if (command.sizeDelta != null && command.sizeDelta.Length == 2)
        {
            sliderRect.sizeDelta = new Vector2(command.sizeDelta[0], command.sizeDelta[1]);
        }
        else
        {
            sliderRect.sizeDelta = new Vector2(160, 20); // Default size
        }

        // 2. 배경 이미지 생성 (Slider Background)
        GameObject backgroundGO = new GameObject("Background", typeof(RectTransform));
        backgroundGO.transform.SetParent(sliderGO.transform, false);
        backgroundGO.AddComponent<CanvasRenderer>();
        Image backgroundImage = backgroundGO.AddComponent<Image>();
        backgroundImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Dark gray background

        RectTransform backgroundRect = backgroundGO.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.25f);
        backgroundRect.anchorMax = new Vector2(1f, 0.75f);
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundRect.anchoredPosition = Vector2.zero;
        backgroundRect.sizeDelta = Vector2.zero;

        // 3. 채우기 영역 생성 (Fill Area)
        GameObject fillAreaGO = new GameObject("Fill Area", typeof(RectTransform));
        fillAreaGO.transform.SetParent(sliderGO.transform, false);

        RectTransform fillAreaRect = fillAreaGO.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.pivot = new Vector2(0.5f, 0.5f);
        fillAreaRect.anchoredPosition = Vector2.zero;
        fillAreaRect.sizeDelta = Vector2.zero;

        // 4. 채우기 이미지 생성 (Fill)
        GameObject fillGO = new GameObject("Fill", typeof(RectTransform));
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        fillGO.AddComponent<CanvasRenderer>();
        Image fillImage = fillGO.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.6f, 0.2f, 1f); // Green fill

        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = Vector2.zero;

        // 5. 핸들 슬라이드 영역 생성 (Handle Slide Area)
        GameObject handleSlideAreaGO = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleSlideAreaGO.transform.SetParent(sliderGO.transform, false);

        RectTransform handleSlideAreaRect = handleSlideAreaGO.GetComponent<RectTransform>();
        handleSlideAreaRect.anchorMin = new Vector2(0f, 0f);
        handleSlideAreaRect.anchorMax = new Vector2(1f, 1f);
        handleSlideAreaRect.pivot = new Vector2(0.5f, 0.5f);
        handleSlideAreaRect.anchoredPosition = Vector2.zero;
        handleSlideAreaRect.sizeDelta = new Vector2(-20, 0); // Adjust for handle size

        // 6. 핸들 생성 (Handle)
        GameObject handleGO = new GameObject("Handle", typeof(RectTransform));
        handleGO.transform.SetParent(handleSlideAreaGO.transform, false);
        handleGO.AddComponent<CanvasRenderer>();
        Image handleImage = handleGO.AddComponent<Image>();
        handleImage.color = Color.white; // White handle

        RectTransform handleRect = handleGO.GetComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.anchoredPosition = Vector2.zero;
        handleRect.sizeDelta = new Vector2(20, 20); // Size of the handle

        // 7. Slider 컴포넌트 참조 연결 및 속성 설정
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage; // Handle is the target graphic for interaction

        slider.minValue = command.minValue;
        slider.maxValue = command.maxValue;
        slider.value = command.value;
        slider.wholeNumbers = command.wholeNumbers;

        sliderGO.SetActive(command.setActive);

        result.success = true;
        result.message = $"Slider 생성됨: {sliderGO.name}";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult CreateScrollView(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.parent))
        {
            result.success = false;
            result.message = "UI ScrollView를 생성하려면 'parent' Canvas 또는 UI 요소가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        var gameObjectManager = new GameObjectManager();
        GameObject parentGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.parent });
        if (parentGO == null)
        {
            result.success = false;
            result.message = $"부모 GameObject '{command.parent}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        // 1. 메인 ScrollView GameObject 생성
        GameObject scrollViewGO = new GameObject(command.name ?? "NewScrollView", typeof(RectTransform));
        scrollViewGO.transform.SetParent(parentGO.transform, false);
        scrollViewGO.AddComponent<CanvasRenderer>();
        Image scrollViewImage = scrollViewGO.AddComponent<Image>(); // ScrollView 배경 역할
        scrollViewImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f); // Semi-transparent dark gray
        ApplyRectTransformProperties(scrollViewGO, command); // Apply RectTransform properties

        // RectTransform 설정 (기본값 또는 command에서 가져옴)
        RectTransform scrollViewRect = scrollViewGO.GetComponent<RectTransform>();
        if (command.uiPosition != null && command.uiPosition.Length == 2)
        {
            scrollViewRect.anchoredPosition = new Vector2(command.uiPosition[0], command.uiPosition[1]);
        }
        if (command.sizeDelta != null && command.sizeDelta.Length == 2)
        {
            scrollViewRect.sizeDelta = new Vector2(command.sizeDelta[0], command.sizeDelta[1]);
        }
        else
        {
            scrollViewRect.sizeDelta = new Vector2(200, 150); // Default size
        }

        // 2. Viewport GameObject 생성
        GameObject viewportGO = new GameObject("Viewport", typeof(RectTransform));
        viewportGO.transform.SetParent(scrollViewGO.transform, false);
        viewportGO.AddComponent<CanvasRenderer>();
        Image viewportImage = viewportGO.AddComponent<Image>();
        viewportImage.color = Color.clear; // Transparent
        viewportGO.AddComponent<Mask>().showMaskGraphic = false; // 뷰포트 밖의 콘텐츠를 가림

        RectTransform viewportRect = viewportGO.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.pivot = Vector2.zero;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        // 3. Content GameObject 생성
        GameObject contentGO = new GameObject(command.contentName ?? "Content", typeof(RectTransform));
        contentGO.transform.SetParent(viewportGO.transform, false);
        contentGO.AddComponent<CanvasRenderer>();

        RectTransform contentRect = contentGO.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 300); // 초기 콘텐츠 크기 (예시)

        // 필요에 따라 VerticalLayoutGroup 또는 ContentSizeFitter 추가
        // contentGO.AddComponent<VerticalLayoutGroup>();
        // contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 4. ScrollRect 컴포넌트 설정
        ScrollRect scrollRect = scrollViewGO.AddComponent<ScrollRect>();
        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = command.horizontal;
        scrollRect.vertical = command.vertical;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.1f;

        // 스크롤바 추가 (선택 사항, 여기서는 생략)

        scrollViewGO.SetActive(command.setActive);

        result.success = true;
        result.message = $"ScrollView 생성됨: {scrollViewGO.name}";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult CreateInputField(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.parent))
        {
            result.success = false;
            result.message = "UI InputField를 생성하려면 'parent' Canvas 또는 UI 요소가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        var gameObjectManager = new GameObjectManager();
        GameObject parentGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.parent });
        if (parentGO == null)
        {
            result.success = false;
            result.message = $"부모 GameObject '{command.parent}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        // 1. 메인 InputField GameObject 생성
        GameObject inputFieldGO = new GameObject(command.name ?? "NewInputField", typeof(RectTransform));
        inputFieldGO.transform.SetParent(parentGO.transform, false);
        inputFieldGO.AddComponent<CanvasRenderer>();
        Image inputFieldImage = inputFieldGO.AddComponent<Image>(); // InputField 배경 역할
        inputFieldImage.color = new Color(0.15f, 0.15f, 0.15f, 1f); // Dark background
        ApplyRectTransformProperties(inputFieldGO, command); // Apply RectTransform properties

        // RectTransform 설정 (기본값 또는 command에서 가져옴)
        RectTransform inputFieldRect = inputFieldGO.GetComponent<RectTransform>();
        if (command.uiPosition != null && command.uiPosition.Length == 2)
        {
            inputFieldRect.anchoredPosition = new Vector2(command.uiPosition[0], command.uiPosition[1]);
        }
        if (command.sizeDelta != null && command.sizeDelta.Length == 2)
        {
            inputFieldRect.sizeDelta = new Vector2(command.sizeDelta[0], command.sizeDelta[1]);
        }
        else
        {
            inputFieldRect.sizeDelta = new Vector2(200, 40); // Default size
        }

        // 2. Text GameObject 생성 (입력된 텍스트 표시)
        GameObject textGO = new GameObject("Text", typeof(RectTransform));
        textGO.transform.SetParent(inputFieldGO.transform, false);
        textGO.AddComponent<CanvasRenderer>();
        TextMeshProUGUI textTMP = textGO.AddComponent<TextMeshProUGUI>();
        textTMP.color = Color.white;
        textTMP.fontSize = 20;
        textTMP.alignment = TextAlignmentOptions.Left;
        textTMP.enableWordWrapping = false;
        if (!string.IsNullOrEmpty(command.fontAssetPath))
        {
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(command.fontAssetPath);
            if (fontAsset != null)
            {
                textTMP.font = fontAsset;
            }
            else
            {
                Debug.LogWarning($"[UIManager] Failed to load font asset at path: {command.fontAssetPath} for InputField Text.");
            }
        }

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.offsetMin = new Vector2(5, 0); // Padding
        textRect.offsetMax = new Vector2(-5, 0); // Padding

        // 3. Placeholder GameObject 생성
        GameObject placeholderGO = new GameObject("Placeholder", typeof(RectTransform));
        placeholderGO.transform.SetParent(inputFieldGO.transform, false);
        placeholderGO.AddComponent<CanvasRenderer>();
        TextMeshProUGUI placeholderTMP = placeholderGO.AddComponent<TextMeshProUGUI>();
        placeholderTMP.text = command.placeholderText ?? "Enter text...";
        placeholderTMP.color = new Color(0.7f, 0.7f, 0.7f, 0.5f); // Light gray, semi-transparent
        placeholderTMP.fontSize = 20;
        placeholderTMP.alignment = TextAlignmentOptions.Left;
        if (!string.IsNullOrEmpty(command.fontAssetPath))
        {
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(command.fontAssetPath);
            if (fontAsset != null)
            {
                placeholderTMP.font = fontAsset;
            }
            else
            {
                Debug.LogWarning($"[UIManager] Failed to load font asset at path: {command.fontAssetPath} for InputField Placeholder.");
            }
        }

        RectTransform placeholderRect = placeholderGO.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.pivot = new Vector2(0.5f, 0.5f);
        placeholderRect.offsetMin = new Vector2(5, 0); // Padding
        placeholderRect.offsetMax = new Vector2(-5, 0); // Padding

        // 4. TMP_InputField 컴포넌트 추가 및 설정
        TMP_InputField inputField = inputFieldGO.AddComponent<TMP_InputField>();
        inputField.textComponent = textTMP;
        inputField.placeholder = placeholderTMP;
        inputField.characterLimit = command.characterLimit;

        if (!string.IsNullOrEmpty(command.contentType))
        {
            TMP_InputField.ContentType contentTypeEnum;
            if (System.Enum.TryParse(command.contentType, true, out contentTypeEnum))
            {
                inputField.contentType = contentTypeEnum;
            }
            else
            {
                Debug.LogWarning($"알 수 없는 ContentType: {command.contentType}");
            }
        }

        inputFieldGO.SetActive(command.setActive);

        result.success = true;
        result.message = $"InputField 생성됨: {inputFieldGO.name}";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult CreateDropdown(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.parent))
        {
            result.success = false;
            result.message = "UI Dropdown을 생성하려면 'parent' Canvas 또는 UI 요소가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        var gameObjectManager = new GameObjectManager();
        GameObject parentGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.parent });
        if (parentGO == null)
        {
            result.success = false;
            result.message = $"부모 GameObject '{command.parent}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        // 1. 메인 Dropdown GameObject 생성
        GameObject dropdownGO = new GameObject(command.name ?? "NewDropdown", typeof(RectTransform));
        dropdownGO.transform.SetParent(parentGO.transform, false);
        dropdownGO.AddComponent<CanvasRenderer>();
        Image dropdownImage = dropdownGO.AddComponent<Image>();
        dropdownImage.color = new Color(0.15f, 0.15f, 0.15f, 1f); // Dark background
        ApplyRectTransformProperties(dropdownGO, command); // Apply RectTransform properties

        // RectTransform 설정 (기본값 또는 command에서 가져옴)
        RectTransform dropdownRect = dropdownGO.GetComponent<RectTransform>();
        if (command.uiPosition != null && command.uiPosition.Length == 2)
        {
            dropdownRect.anchoredPosition = new Vector2(command.uiPosition[0], command.uiPosition[1]);
        }
        if (command.sizeDelta != null && command.sizeDelta.Length == 2)
        {
            dropdownRect.sizeDelta = new Vector2(command.sizeDelta[0], command.sizeDelta[1]);
        }
        else
        {
            dropdownRect.sizeDelta = new Vector2(160, 30); // Default size
        }

        TMP_Dropdown dropdown = dropdownGO.AddComponent<TMP_Dropdown>();

        // 2. Caption Text (선택된 항목 표시 텍스트)
        GameObject labelGO = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(dropdownGO.transform, false);
        labelGO.AddComponent<CanvasRenderer>();
        TextMeshProUGUI captionText = labelGO.AddComponent<TextMeshProUGUI>();
        captionText.color = Color.white;
        captionText.fontSize = 18;
        captionText.alignment = TextAlignmentOptions.Left;

        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(10, 0); // Padding
        labelRect.offsetMax = new Vector2(-25, 0); // Padding for arrow

        // 3. Arrow Image (드롭다운 화살표)
        GameObject arrowGO = new GameObject("Arrow", typeof(RectTransform));
        arrowGO.transform.SetParent(dropdownGO.transform, false);
        arrowGO.AddComponent<CanvasRenderer>();
        Image arrowImage = arrowGO.AddComponent<Image>();
        arrowImage.color = Color.white; // White arrow
        // You might want to assign a default arrow sprite here if available
        // arrowImage.sprite = AssetDatabase.GetBuiltinResource<Sprite>("DropdownArrow.psd");

        RectTransform arrowRect = arrowGO.GetComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1, 0.5f);
        arrowRect.anchorMax = new Vector2(1, 0.5f);
        arrowRect.pivot = new Vector2(0.5f, 0.5f);
        arrowRect.anchoredPosition = new Vector2(-15, 0);
        arrowRect.sizeDelta = new Vector2(20, 20);

        // 4. Template GameObject (드롭다운 목록)
        GameObject templateGO = new GameObject("Template", typeof(RectTransform));
        templateGO.transform.SetParent(dropdownGO.transform, false);
        templateGO.AddComponent<CanvasRenderer>();
        Image templateImage = templateGO.AddComponent<Image>();
        templateImage.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Darker background for template
        templateGO.SetActive(false); // Initially hidden

        RectTransform templateRect = templateGO.GetComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0f, 0f);
        templateRect.anchorMax = new Vector2(1f, 0f);
        templateRect.pivot = new Vector2(0.5f, 1f);
        templateRect.anchoredPosition = new Vector2(0, -dropdownRect.sizeDelta.y); // Position below dropdown
        templateRect.sizeDelta = new Vector2(0, 150); // Default height for list

        // 5. Scroll View for Template
        ScrollRect templateScrollRect = templateGO.AddComponent<ScrollRect>();
        templateScrollRect.horizontal = false;
        templateScrollRect.movementType = ScrollRect.MovementType.Clamped;

        // 6. Viewport for Template
        GameObject viewportGO = new GameObject("Viewport", typeof(RectTransform));
        viewportGO.transform.SetParent(templateGO.transform, false);
        viewportGO.AddComponent<CanvasRenderer>();
        Image viewportImage = viewportGO.AddComponent<Image>();
        viewportImage.color = Color.clear;
        viewportGO.AddComponent<Mask>().showMaskGraphic = false;

        RectTransform viewportRect = viewportGO.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(0, 0);
        viewportRect.offsetMax = new Vector2(-17, 0); // Make space for scrollbar

        templateScrollRect.viewport = viewportRect;

        // 7. Content for Template
        GameObject contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(viewportGO.transform, false);
        contentGO.AddComponent<CanvasRenderer>();
        VerticalLayoutGroup contentLayout = contentGO.AddComponent<VerticalLayoutGroup>();
        contentLayout.childForceExpandHeight = false;
        contentLayout.childForceExpandWidth = true;
        contentLayout.spacing = 2;

        RectTransform contentRect = contentGO.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 0); // Will be adjusted by content

        templateScrollRect.content = contentRect;

        // 8. Scrollbar for Template (Vertical)
        GameObject scrollbarGO = new GameObject("Scrollbar", typeof(RectTransform));
        scrollbarGO.transform.SetParent(templateGO.transform, false);
        scrollbarGO.AddComponent<CanvasRenderer>();
        Image scrollbarImage = scrollbarGO.AddComponent<Image>();
        scrollbarImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        Scrollbar scrollbar = scrollbarGO.AddComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        templateScrollRect.verticalScrollbar = scrollbar;

        RectTransform scrollbarRect = scrollbarGO.GetComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1, 0);
        scrollbarRect.anchorMax = new Vector2(1, 1);
        scrollbarRect.pivot = new Vector2(1, 1);
        scrollbarRect.sizeDelta = new Vector2(17, 0);

        GameObject slidingAreaGO = new GameObject("Sliding Area", typeof(RectTransform));
        slidingAreaGO.transform.SetParent(scrollbarGO.transform, false);
        RectTransform slidingAreaRect = slidingAreaGO.GetComponent<RectTransform>();
        slidingAreaRect.anchorMin = Vector2.zero;
        slidingAreaRect.anchorMax = Vector2.one;
        slidingAreaRect.sizeDelta = new Vector2(-20, -20); // Padding for handle

        GameObject handleGO = new GameObject("Handle", typeof(RectTransform));
        handleGO.transform.SetParent(slidingAreaGO.transform, false);
        handleGO.AddComponent<CanvasRenderer>();
        Image handleImage = handleGO.AddComponent<Image>();
        handleImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        scrollbar.handleRect = handleGO.GetComponent<RectTransform>();
        scrollbar.targetGraphic = handleImage;

        // 9. Item Template for Dropdown
        GameObject itemGO = new GameObject("Item", typeof(RectTransform));
        itemGO.transform.SetParent(contentGO.transform, false);
        itemGO.AddComponent<CanvasRenderer>();
        Toggle itemToggle = itemGO.AddComponent<Toggle>();
        

        RectTransform itemRect = itemGO.GetComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0f, 0.5f);
        itemRect.anchorMax = new Vector2(1f, 0.5f);
        itemRect.pivot = new Vector2(0.5f, 0.5f);
        itemRect.sizeDelta = new Vector2(0, 25); // Height of each item

        // Item Background
        GameObject itemBackgroundGO = new GameObject("Item Background", typeof(RectTransform));
        itemBackgroundGO.transform.SetParent(itemGO.transform, false);
        itemBackgroundGO.AddComponent<CanvasRenderer>();
        Image itemBackgroundImage = itemBackgroundGO.AddComponent<Image>();
        itemBackgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Item background color
        itemToggle.targetGraphic = itemBackgroundImage;

        RectTransform itemBackgroundRect = itemBackgroundGO.GetComponent<RectTransform>();
        itemBackgroundRect.anchorMin = Vector2.zero;
        itemBackgroundRect.anchorMax = Vector2.one;
        itemBackgroundRect.sizeDelta = Vector2.zero;

        // Item Checkmark
        GameObject itemCheckmarkGO = new GameObject("Item Checkmark", typeof(RectTransform));
        itemCheckmarkGO.transform.SetParent(itemGO.transform, false);
        itemCheckmarkGO.AddComponent<CanvasRenderer>();
        Image itemCheckmarkImage = itemCheckmarkGO.AddComponent<Image>();
        itemCheckmarkImage.color = Color.white; // Checkmark color
        // You might want to assign a default checkmark sprite here
        // itemCheckmarkImage.sprite = AssetDatabase.GetBuiltinResource<Sprite>("Checkmark.psd");

        RectTransform itemCheckmarkRect = itemCheckmarkGO.GetComponent<RectTransform>();
        itemCheckmarkRect.anchorMin = new Vector2(0f, 0.5f);
        itemCheckmarkRect.anchorMax = new Vector2(0f, 0.5f);
        itemCheckmarkRect.pivot = new Vector2(0.5f, 0.5f);
        itemCheckmarkRect.anchoredPosition = new Vector2(10, 0);
        itemCheckmarkRect.sizeDelta = new Vector2(20, 20);
        itemToggle.graphic = itemCheckmarkImage;

        // Item Label
        GameObject itemLabelGO = new GameObject("Item Label", typeof(RectTransform));
        itemLabelGO.transform.SetParent(itemGO.transform, false);
        itemLabelGO.AddComponent<CanvasRenderer>();
        TextMeshProUGUI itemLabelText = itemLabelGO.AddComponent<TextMeshProUGUI>();
        itemLabelText.color = Color.white;
        itemLabelText.fontSize = 16;
        itemLabelText.alignment = TextAlignmentOptions.Left;

        RectTransform itemLabelRect = itemLabelGO.GetComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(30, 0); // Padding for checkmark
        itemLabelRect.offsetMax = new Vector2(0, 0);
        
        // Link item template to dropdown
        dropdown.template = templateRect;
        dropdown.captionText = captionText;
        dropdown.itemText = itemLabelText; // This is for the text component of the item template
        dropdown.captionImage = arrowImage; // Using arrow image as caption image for consistency

        // Set options
        if (command.options != null && command.options.Length > 0)
        {
            dropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (string option in command.options)
            {
                options.Add(new TMP_Dropdown.OptionData(option));
            }
            dropdown.AddOptions(options);
        }
        else
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string> { "Option A", "Option B", "Option C" }); // Default options
        }

        dropdownGO.SetActive(command.setActive);

        result.success = true;
        result.message = $"Dropdown 생성됨: {dropdownGO.name}";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult InstantiateUIPrefab(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.assetPath))
        {
            result.success = false;
            result.message = "인스턴스화할 프리팹의 'assetPath'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(command.assetPath);
        if (prefab == null)
        {
            result.success = false;
            result.message = $"프리팹 '{command.assetPath}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        GameObject parentGO = null;
        if (!string.IsNullOrEmpty(command.parent))
        {
            var gameObjectManager = new GameObjectManager();
            parentGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.parent });
            if (parentGO == null)
            {
                result.success = false;
                result.message = $"부모 GameObject '{command.parent}'를 찾을 수 없습니다.";
                Debug.LogWarning(result.message);
                return result;
            }
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parentGO != null ? parentGO.transform : null);
        if (instance == null)
        {
            result.success = false;
            result.message = $"프리팹 '{command.assetPath}'를 인스턴스화하지 못했습니다.";
            Debug.LogError(result.message);
            return result;
        }

        if (!string.IsNullOrEmpty(command.name))
        {
            instance.name = command.name;
        }

        ApplyRectTransformProperties(instance, command); // Apply RectTransform properties

        // For non-UI prefabs, apply regular transform properties
        if (instance.GetComponent<RectTransform>() == null)
        {
            if (command.position != null && command.position.Length == 3)
            {
                instance.transform.position = new Vector3(command.position[0], command.position[1], command.position[2]);
            }
            if (command.rotation != null && command.rotation.Length == 3)
            {
                instance.transform.rotation = Quaternion.Euler(command.rotation[0], command.rotation[1], command.rotation[2]);
            }
            if (command.scale != null && command.scale.Length == 3)
            {
                instance.transform.localScale = new Vector3(command.scale[0], command.scale[1], command.scale[2]);
            }
        }

        Undo.RegisterCreatedObjectUndo(instance, $"Instantiate UI Prefab: {instance.name}");
        EditorUtility.SetDirty(instance);
        result.success = true;
        result.message = $"UI 프리팹 '{command.assetPath}'가 '{instance.name}'으로 인스턴스화되었습니다.";
        Debug.Log(result.message);
        return result;
    }
    
    public CommandResult SetRectTransformAnchors(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target))
        {
            result.success = false;
            result.message = "RectTransform 앵커를 설정할 GameObject의 'target'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        var gameObjectManager = new GameObjectManager();
        GameObject targetGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.target });
        if (targetGO == null)
        {
            result.success = false;
            result.message = $"GameObject '{command.target}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        RectTransform rectTransform = targetGO.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            result.success = false;
            result.message = $"GameObject '{command.target}'에 RectTransform 컴포넌트가 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        // 부모 RectTransform 확인
        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect != null && parentRect.rect.width == 0 && parentRect.rect.height == 0)
        {
            Debug.LogWarning($"[UIManager] 경고: '{targetGO.name}'의 부모 '{parentRect.name}'의 크기가 (0,0)입니다. 앵커 설정이 예기치 않게 동작할 수 있습니다. 부모의 레이아웃이 업데이트된 후 앵커를 설정해야 합니다.", parentRect.gameObject);
        }

        Vector2 anchorMin = rectTransform.anchorMin;
        Vector2 anchorMax = rectTransform.anchorMax;
        Vector2 offsetMin = rectTransform.offsetMin;
        Vector2 offsetMax = rectTransform.offsetMax;

        // Apply preset
        if (!string.IsNullOrEmpty(command.anchorPreset))
        {
            switch (command.anchorPreset.ToLower())
            {
                case "topleft":
                    anchorMin = new Vector2(0, 1);
                    anchorMax = new Vector2(0, 1);
                    break;
                case "topcenter":
                    anchorMin = new Vector2(0.5f, 1);
                    anchorMax = new Vector2(0.5f, 1);
                    break;
                case "topright":
                    anchorMin = new Vector2(1, 1);
                    anchorMax = new Vector2(1, 1);
                    break;
                case "middleleft":
                    anchorMin = new Vector2(0, 0.5f);
                    anchorMax = new Vector2(0, 0.5f);
                    break;
                case "middlecenter":
                    anchorMin = new Vector2(0.5f, 0.5f);
                    anchorMax = new Vector2(0.5f, 0.5f);
                    break;
                case "middleright":
                    anchorMin = new Vector2(1, 0.5f);
                    anchorMax = new Vector2(1, 0.5f);
                    break;
                case "bottomleft":
                    anchorMin = new Vector2(0, 0);
                    anchorMax = new Vector2(0, 0);
                    break;
                case "bottomcenter":
                    anchorMin = new Vector2(0.5f, 0);
                    anchorMax = new Vector2(0.5f, 0);
                    break;
                case "bottomright":
                    anchorMin = new Vector2(1, 0);
                    anchorMax = new Vector2(1, 0);
                    rectTransform.pivot = new Vector2(1, 0); // Set pivot for bottomright
                    break;
                case "stretchleft":
                    anchorMin = new Vector2(0, 0);
                    anchorMax = new Vector2(0, 1);
                    break;
                case "stretchcenter":
                    anchorMin = new Vector2(0.5f, 0);
                    anchorMax = new Vector2(0.5f, 1);
                    break;
                case "stretchright":
                    anchorMin = new Vector2(1, 0);
                    anchorMax = new Vector2(1, 1);
                    break;
                case "stretchtop":
                    anchorMin = new Vector2(0, 1);
                    anchorMax = new Vector2(1, 1);
                    break;
                case "stretchmiddle":
                    anchorMin = new Vector2(0, 0.5f);
                    anchorMax = new Vector2(1, 0.5f);
                    break;
                case "stretchbottom":
                    anchorMin = new Vector2(0, 0);
                    anchorMax = new Vector2(1, 0);
                    break;
                case "stretchall":
                    anchorMin = new Vector2(0, 0);
                    anchorMax = new Vector2(1, 1);
                    break;
                default:
                    Debug.LogWarning($"알 수 없는 anchorPreset: {command.anchorPreset}. 기본값으로 설정하지 않습니다.");
                    break;
            }
        }

        // Apply explicit anchorMin, anchorMax, pivot, anchoredPosition if provided
        if (command.anchorMin != null)
        {
            anchorMin = command.anchorMin;
        }
        if (command.anchorMax != null)
        {
            anchorMax = command.anchorMax;
        }
        if (command.pivot != null)
        {
            rectTransform.pivot = command.pivot;
        }
        if (command.anchoredPosition != null)
        {
            rectTransform.anchoredPosition = command.anchoredPosition;
        }

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        // Apply padding if provided
        if (command.padding != null && command.padding.Length == 4)
        {
            // When anchors are stretched, offsets act as padding
            if (anchorMin.x != anchorMax.x || anchorMin.y != anchorMax.y)
            {
                offsetMin = new Vector2(command.padding[0], command.padding[3]); // left, bottom
                offsetMax = new Vector2(-command.padding[1], -command.padding[2]); // -right, -top
            }
            else // When anchors are not stretched, offsets define position relative to anchor
            {
                // For non-stretched anchors, padding can be interpreted as sizeDelta + position adjustment
                // This is a simplification and might need more complex logic depending on desired behavior
                // For now, we'll just set sizeDelta if it's provided, and position if uiPosition is provided.
                // If only padding is given for non-stretched, it's ambiguous.
                // We'll prioritize sizeDelta and uiPosition if they are also provided.
            }
        }

        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;

        EditorUtility.SetDirty(targetGO);
        result.success = true;
        result.message = $"GameObject '{command.target}'의 RectTransform 앵커가 '{command.anchorPreset}' 프리셋으로 설정되었습니다.";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult CreatePanel(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.parent))
        {
            result.success = false;
            result.message = "UI Panel을 생성하려면 'parent' Canvas 또는 UI 요소가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        var gameObjectManager = new GameObjectManager();
        GameObject parentGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.parent });
        if (parentGO == null)
        {
            result.success = false;
            result.message = $"부모 GameObject '{command.parent}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        GameObject panelGO = new GameObject(command.name, typeof(RectTransform));
        panelGO.transform.SetParent(parentGO.transform, false);
        panelGO.SetActive(command.setActive);
        
        panelGO.AddComponent<Image>();
        ApplyRectTransformProperties(panelGO, command); // Apply RectTransform properties

        result.success = true;
        result.message = $"UI Panel 생성: {command.name}";
        Debug.Log(result.message);
        return result;
    }
    public CommandResult CreateRawImage(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.parent))
        {
            result.success = false;
            result.message = "UI RawImage를 생성하려면 'parent' Canvas 또는 UI 요소가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        var gameObjectManager = new GameObjectManager();
        GameObject parentGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.parent });
        if (parentGO == null)
        {
            result.success = false;
            result.message = $"부모 GameObject '{command.parent}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        GameObject rawImageGO = new GameObject(command.name, typeof(RectTransform));
        rawImageGO.transform.SetParent(parentGO.transform, false);
        rawImageGO.SetActive(command.setActive);
        ApplyRectTransformProperties(rawImageGO, command); // Apply RectTransform properties

        RawImage rawImage = rawImageGO.AddComponent<RawImage>();
        if (!string.IsNullOrEmpty(command.texturePath))
        {
            Texture textureAsset = AssetDatabase.LoadAssetAtPath<Texture>(command.texturePath);
            if (textureAsset != null)
            {
                rawImage.texture = textureAsset;
            }
            else
            {
                Debug.LogWarning($"텍스처 에셋을 찾을 수 없습니다: {command.texturePath}");
            }
        }

        result.success = true;
        result.message = $"UI RawImage 생성: {command.name}";
        Debug.Log(result.message);
        return result;
    }
    public CommandResult AddVerticalLayoutGroup(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target))
        {
            result.success = false;
            result.message = "VerticalLayoutGroup을 추가할 GameObject의 'target'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        var gameObjectManager = new GameObjectManager();
        GameObject targetGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.target });
        if (targetGO == null)
        {
            result.success = false;
            result.message = $"GameObject '{command.target}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        VerticalLayoutGroup layoutGroup = targetGO.AddComponent<VerticalLayoutGroup>();
        if (command.padding != null && command.padding.Length == 4)
        {
            layoutGroup.padding.left = (int)command.padding[0];
            layoutGroup.padding.right = (int)command.padding[1];
            layoutGroup.padding.top = (int)command.padding[2];
            layoutGroup.padding.bottom = (int)command.padding[3];
        }
        layoutGroup.spacing = command.spacing;
        if (!string.IsNullOrEmpty(command.childAlignment))
        {
            TextAnchor alignment;
            if (System.Enum.TryParse(command.childAlignment, true, out alignment))
            {
                layoutGroup.childAlignment = alignment;
            }
        }
        layoutGroup.childControlWidth = command.childControlWidth;
        layoutGroup.childControlHeight = command.childControlHeight;
        layoutGroup.childForceExpandWidth = command.childForceExpandWidth;
        layoutGroup.childForceExpandHeight = command.childForceExpandHeight;
        result.success = true;
        result.message = $"GameObject '{command.target}'에 VerticalLayoutGroup이 추가되었습니다.";
        Debug.Log(result.message);
        return result;
    }
    public CommandResult AddHorizontalLayoutGroup(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target))
        {
            result.success = false;
            result.message = "HorizontalLayoutGroup을 추가할 GameObject의 'target'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        var gameObjectManager = new GameObjectManager();
        GameObject targetGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.target });
        if (targetGO == null)
        {
            result.success = false;
            result.message = $"GameObject '{command.target}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        HorizontalLayoutGroup layoutGroup = targetGO.AddComponent<HorizontalLayoutGroup>();
        if (command.padding != null && command.padding.Length == 4)
        {
            layoutGroup.padding.left = (int)command.padding[0];
            layoutGroup.padding.right = (int)command.padding[1];
            layoutGroup.padding.top = (int)command.padding[2];
            layoutGroup.padding.bottom = (int)command.padding[3];
        }
        layoutGroup.spacing = command.spacing;
        if (!string.IsNullOrEmpty(command.childAlignment))
        {
            TextAnchor alignment;
            if (System.Enum.TryParse(command.childAlignment, true, out alignment))
            {
                layoutGroup.childAlignment = alignment;
            }
        }
        layoutGroup.childControlWidth = command.childControlWidth;
        layoutGroup.childControlHeight = command.childControlHeight;
        layoutGroup.childForceExpandWidth = command.childForceExpandWidth;
        layoutGroup.childForceExpandHeight = command.childForceExpandHeight;
        result.success = true;
        result.message = $"GameObject '{command.target}'에 HorizontalLayoutGroup이 추가되었습니다.";
        Debug.Log(result.message);
        return result;
    }
    public CommandResult AddGridLayoutGroup(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target))
        {
            result.success = false;
            result.message = "GridLayoutGroup을 추가할 GameObject의 'target'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        var gameObjectManager = new GameObjectManager();
        GameObject targetGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.target });
        if (targetGO == null)
        {
            result.success = false;
            result.message = $"GameObject '{command.target}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        GridLayoutGroup layoutGroup = targetGO.AddComponent<GridLayoutGroup>();
        if (command.padding != null && command.padding.Length == 4)
        {
            layoutGroup.padding.left = (int)command.padding[0];
            layoutGroup.padding.right = (int)command.padding[1];
            layoutGroup.padding.top = (int)command.padding[2];
            layoutGroup.padding.bottom = (int)command.padding[3];
        }
        layoutGroup.spacing = new Vector2(command.spacing, command.spacing);
        if (!string.IsNullOrEmpty(command.childAlignment))
        {
            TextAnchor alignment;
            if (System.Enum.TryParse(command.childAlignment, true, out alignment))
            {
                layoutGroup.childAlignment = alignment;
            }
        }
        if (command.cellSize != null && command.cellSize.Length == 2)
        {
            layoutGroup.cellSize = new Vector2(command.cellSize[0], command.cellSize[1]);
        }
        if (!string.IsNullOrEmpty(command.startCorner))
        {
            GridLayoutGroup.Corner corner;
            if (System.Enum.TryParse(command.startCorner, true, out corner))
            {
                layoutGroup.startCorner = corner;
            }
        }
        if (!string.IsNullOrEmpty(command.startAxis))
        {
            GridLayoutGroup.Axis axis;
            if (System.Enum.TryParse(command.startAxis, true, out axis))
            {
                layoutGroup.startAxis = axis;
            }
        }
        if (!string.IsNullOrEmpty(command.constraint))
        {
            GridLayoutGroup.Constraint constraint;
            if (System.Enum.TryParse(command.constraint, true, out constraint))
            {
                layoutGroup.constraint = constraint;
            }
        }
        layoutGroup.constraintCount = command.constraintCount;
        result.success = true;
        result.message = $"GameObject '{command.target}'에 GridLayoutGroup이 추가되었습니다.";
        Debug.Log(result.message);
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
        var gameObjectManager = new GameObjectManager();
        GameObject targetGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.target });
        if (targetGO == null)
        {
            result.success = false;
            result.message = $"GameObject '{command.target}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        ContentSizeFitter contentSizeFitter = targetGO.AddComponent<ContentSizeFitter>();
        if (!string.IsNullOrEmpty(command.horizontalFit))
        {
            ContentSizeFitter.FitMode fitMode;
            if (System.Enum.TryParse(command.horizontalFit, true, out fitMode))
            {
                contentSizeFitter.horizontalFit = fitMode;
            }
        }
        if (!string.IsNullOrEmpty(command.verticalFit))
        {
            ContentSizeFitter.FitMode fitMode;
            if (System.Enum.TryParse(command.verticalFit, true, out fitMode))
            {
                contentSizeFitter.verticalFit = fitMode;
            }
        }
        result.success = true;
        result.message = $"GameObject '{command.target}'에 ContentSizeFitter가 추가되었습니다.";
        Debug.Log(result.message);
        return result;
    }
    public CommandResult AddLayoutElement(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target))
        {
            result.success = false;
            result.message = "LayoutElement를 추가할 GameObject의 'target'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        var gameObjectManager = new GameObjectManager();
        GameObject targetGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.target });
        if (targetGO == null)
        {
            result.success = false;
            result.message = $"GameObject '{command.target}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        LayoutElement layoutElement = targetGO.AddComponent<LayoutElement>();
        layoutElement.minWidth = command.minWidth;
        layoutElement.minHeight = command.minHeight;
        layoutElement.preferredWidth = command.preferredWidth;
        layoutElement.preferredHeight = command.preferredHeight;
        layoutElement.flexibleWidth = command.flexibleWidth;
        layoutElement.flexibleHeight = command.flexibleHeight;
        layoutElement.layoutPriority = command.layoutPriority;
        layoutElement.ignoreLayout = command.ignoreLayout;
        result.success = true;
        result.message = $"GameObject '{command.target}'에 LayoutElement가 추가되었습니다.";
        Debug.Log(result.message);
        return result;
    }
    public CommandResult AddCanvasGroup(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target))
        {
            result.success = false;
            result.message = "CanvasGroup을 추가할 GameObject의 'target'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        var gameObjectManager = new GameObjectManager();
        GameObject targetGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.target });
        if (targetGO == null)
        {
            result.success = false;
            result.message = $"GameObject '{command.target}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        CanvasGroup canvasGroup = targetGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha = command.alpha;
        canvasGroup.interactable = command.interactable;
        canvasGroup.blocksRaycasts = command.blocksRaycasts;
        canvasGroup.ignoreParentGroups = command.ignoreParentGroups;
        result.success = true;
        result.message = $"GameObject '{command.target}'에 CanvasGroup이 추가되었습니다.";
        Debug.Log(result.message);
        return result;
    }
    public CommandResult AddMask(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target))
        {
            result.success = false;
            result.message = "Mask를 추가할 GameObject의 'target'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        var gameObjectManager = new GameObjectManager();
        GameObject targetGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.target });
        if (targetGO == null)
        {
            result.success = false;
            result.message = $"GameObject '{command.target}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        Mask mask = targetGO.AddComponent<Mask>();
        mask.showMaskGraphic = command.showMaskGraphic;
        result.success = true;
        result.message = $"GameObject '{command.target}'에 Mask가 추가되었습니다.";
        Debug.Log(result.message);
        return result;
    }
    public CommandResult AddRectMask2D(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target))
        {
            result.success = false;
            result.message = "RectMask2D를 추가할 GameObject의 'target'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        var gameObjectManager = new GameObjectManager();
        GameObject targetGO = gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.target });
        if (targetGO == null)
        {
            result.success = false;
            result.message = $"GameObject '{command.target}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        RectMask2D rectMask2D = targetGO.AddComponent<RectMask2D>();
        result.success = true;
        result.message = $"GameObject '{command.target}'에 RectMask2D가 추가되었습니다.";
        Debug.Log(result.message);
        return result;
    }
}
#endif