using SweetHome.Editor.Models;
using UnityEngine;

#if UNITY_EDITOR
public class UnityCommandProcessor
{
    // Core Managers for UI/UX Design
    private readonly GameObjectManager _gameObjectManager = new GameObjectManager();
    private readonly ComponentManager _componentManager = new ComponentManager();
    private readonly UIManager _uiManager = new UIManager();
    private readonly LayoutManager _layoutManager = new LayoutManager();
    private readonly EventManager _eventManager = new EventManager();
    
    // New/Refactored Managers for UI/UX Design
    private readonly AnimationManager _animationManager = new AnimationManager();
    private readonly ThemeManager _themeManager = new ThemeManager();

    // Supporting Managers
    private readonly AssetManager _assetManager = new AssetManager();
    private readonly SceneManager _sceneManager = new SceneManager();
    private readonly MaterialManager _materialManager = new MaterialManager();
    private readonly TextureManager _textureManager = new TextureManager();
    private readonly ScriptableObjectManager _scriptableObjectManager = new ScriptableObjectManager(); // Kept for ThemeManager
    private readonly BatchManager _batchManager = new BatchManager();
    private readonly EditorManager _editorManager = new EditorManager();
    private readonly PrefabManager _prefabManager = new PrefabManager();
    private readonly DataBindingManager _dataBindingManager = new DataBindingManager();

    // --- Deprecated or Lower Priority Managers ---
    // private readonly LightingManager _lightingManager = new LightingManager();
    // private readonly ScriptManager _scriptManager = new ScriptManager();
    // private readonly VFXManager _vfxManager = new VFXManager();
    // private readonly ShaderManager _shaderManager = new ShaderManager();


    public CommandResult ProcessCommand(GroqUnityCommand command)
    {
        if (command == null || string.IsNullOrEmpty(command.commandType))
        {
            return new CommandResult { success = false, message = "Invalid command: commandType is missing." };
        }

        // Add validation here
        CommandResult validationResult = command.Validate();
        if (!validationResult.success)
        {
            return validationResult;
        }

        switch (command.commandType)
        {
            // --- High Priority UI/UX Commands ---

            // GameObjectManager Commands
            case "CreateGameObject":
                return _gameObjectManager.CreateGameObject(command);
            case "ModifyGameObject":
                return _gameObjectManager.ModifyGameObject(command);
            case "DeleteGameObject":
                return _gameObjectManager.DeleteGameObject(command);
            case "SetConditionalActive":
                return _gameObjectManager.SetConditionalActive(command);

            // ComponentManager Commands
            case "AddComponent":
                return _componentManager.AddComponentToGameObject(command);
            case "RemoveComponent":
                return _componentManager.RemoveComponentFromGameObject(command);
            case "ModifyComponentProperties":
                return _componentManager.ModifyComponentProperties(command);

            // UIManager Commands
            case "CreateCanvas":
                return _uiManager.CreateCanvas(command);
            case "CreateText":
                return _uiManager.CreateText(command);
            case "CreateButton":
                return _uiManager.CreateButton(command);
            case "CreateImage":
                return _uiManager.CreateImage(command);
            case "CreateToggle":
                return _uiManager.CreateToggle(command);
            case "CreateSlider":
                return _uiManager.CreateSlider(command);
            case "CreateScrollView":
                return _uiManager.CreateScrollView(command);
            case "CreateInputField":
                return _uiManager.CreateInputField(command);
            case "CreateDropdown":
                return _uiManager.CreateDropdown(command);
            case "SetRectTransformAnchors":
                return _uiManager.SetRectTransformAnchors(command);
            case "InstantiateUIPrefab":
                return _uiManager.InstantiateUIPrefab(command);
            case "CreatePanel":
                return _uiManager.CreatePanel(command);
            case "CreateRawImage":
                return _uiManager.CreateRawImage(command);
            case "AddVerticalLayoutGroup":
                return _uiManager.AddVerticalLayoutGroup(command);
            case "AddHorizontalLayoutGroup":
                return _uiManager.AddHorizontalLayoutGroup(command);
            case "AddGridLayoutGroup":
                return _uiManager.AddGridLayoutGroup(command);
            case "AddCanvasGroup":
                return _uiManager.AddCanvasGroup(command);
            case "AddMask":
                return _uiManager.AddMask(command);
            case "AddRectMask2D":
                return _uiManager.AddRectMask2D(command);
            case "AddLayoutElement":
                return _uiManager.AddLayoutElement(command);

            // LayoutManager Commands
            case "AddLayoutGroup":
                return _layoutManager.AddLayoutGroup(command);
            case "AddContentSizeFitter":
                return _layoutManager.AddContentSizeFitter(command);
            case "SetLayoutElementProperties":
                return _layoutManager.SetLayoutElementProperties(command);

            // EventManager Commands
            case "AddClickListener":
                return _eventManager.AddClickListener(command); // Deprecated but kept for now
            case "AddEventCallback":
                return _eventManager.AddEventCallback(command);

            // AnimationManager Commands
            case "AnimateUI":
                return _animationManager.AnimateUI(command);

            // ThemeManager Commands (replaces StyleManager)
            case "CreateTheme":
                return _themeManager.CreateTheme(command);
            case "ApplyThemeStyle":
                return _themeManager.ApplyThemeStyle(command);

            // DataBindingManager Commands
            case "BindProperty":
                return _dataBindingManager.BindProperty(command);

            // --- Supporting Commands ---

            // AssetManager Commands
            case "SearchAssets":
                return _assetManager.SearchAssets(command);
            case "GetAssetInfo":
                return _assetManager.GetAssetInfo(command);
            case "CreateAsset":
                return _assetManager.CreateAsset(command);
            case "DeleteAsset":
                return _assetManager.DeleteAsset(command);
            case "DuplicateAsset":
                return _assetManager.DuplicateAsset(command);
            case "MoveAsset":
                return _assetManager.MoveAsset(command);
            case "RenameAsset":
                return _assetManager.RenameAsset(command);
            case "ImportAsset":
                return _assetManager.ImportAsset(command);
            case "CreateFolder":
                return _assetManager.CreateFolder(command);

            // SceneManager Commands
            case "GetSceneHierarchy":
                return _sceneManager.GetSceneHierarchy(command);
            case "GetActiveSceneInfo":
                return _sceneManager.GetActiveSceneInfo(command);
            case "LoadScene":
                return _sceneManager.LoadScene(command);
            case "SaveScene":
                return _sceneManager.SaveScene(command);
            case "CreateScene":
                return _sceneManager.CreateScene(command);
            case "GetBuildSettingsScenes":
                return _sceneManager.GetBuildSettingsScenes(command);
            case "CaptureScreenshot":
                return _sceneManager.CaptureScreenshot(command);

            // Material & Texture Commands (useful for UI)
            case "GetMaterialInfo":
                return _materialManager.GetMaterialInfo(command);
            case "CreateMaterial":
                return _materialManager.CreateMaterial(command);
            case "SetMaterialColor":
                return _materialManager.SetMaterialColor(command);
            case "SetMaterialProperty":
                return _materialManager.SetMaterialProperty(command);
            case "AssignMaterialToRenderer":
                return _materialManager.AssignMaterialToRenderer(command);
            case "SetRendererColor":
                return _materialManager.SetRendererColor(command);
            case "CreateTexture":
                return _textureManager.CreateTexture(command);
            case "ModifyTexture":
                return _textureManager.ModifyTexture(command);

            // ScriptableObjectManager Commands (useful for Themes)
            case "CreateScriptableObject":
                return _scriptableObjectManager.CreateScriptableObject(command);
            case "ModifyScriptableObject":
                return _scriptableObjectManager.ModifyScriptableObject(command);

            // Batch & Editor Commands
            case "ExecuteBatch":
                return _batchManager.ExecuteBatch(command);
            case "PlayEditor":
                return _editorManager.PlayEditor(command);
            case "PauseEditor":
                return _editorManager.PauseEditor(command);
            case "StopEditor":
                return _editorManager.StopEditor(command);
            case "ExecuteMenuItem":
                return _editorManager.ExecuteMenuItem(command);
            // case "AddTag":
            //     return _editorManager.AddTag(command);
            // case "RemoveTag":
            //     return _editorManager.RemoveTag(command);
            // case "AddLayer":
            //     return _editorManager.AddLayer(command);

            // PrefabManager Commands
            case "CreatePrefab":
                return _prefabManager.CreatePrefab(command);
            case "InstantiatePrefab":
                return _prefabManager.InstantiatePrefab(command);

            // --- Deprecated or Lower Priority Commands ---
            /*
            case "CreateLight":
                return _lightingManager.CreateLight(command);
            case "CompileScripts":
                return _scriptManager.CompileScripts(command);
            case "CreateParticleSystem":
                return _vfxManager.CreateParticleSystem(command);
            case "FindShader":
                return _shaderManager.FindShader(command);
            case "GetShaderParameters":
                return _shaderManager.GetShaderParameters(command);
            */

            default:
                return new CommandResult { success = false, message = $"Unknown or disabled commandType: {command.commandType}" };
        }
    }
}
#endif