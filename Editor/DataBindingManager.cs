using SweetHome.Editor.Models;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection; // For reflection

#if UNITY_EDITOR
public class DataBindingManager
{
    private readonly GameObjectManager _gameObjectManager = new GameObjectManager();

    public CommandResult BindProperty(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target))
        {
            result.success = false;
            result.message = "데이터 바인딩을 설정할 UI GameObject의 'target'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        if (string.IsNullOrEmpty(command.uiComponentType) || string.IsNullOrEmpty(command.uiProperty))
        {
            result.success = false;
            result.message = "바인딩할 UI 컴포넌트 타입 ('uiComponentType')과 속성 ('uiProperty')이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        if (string.IsNullOrEmpty(command.dataSourceGameObject) || string.IsNullOrEmpty(command.dataSourceComponentType) || string.IsNullOrEmpty(command.dataSourceProperty))
        {
            result.success = false;
            result.message = "데이터 소스 GameObject ('dataSourceGameObject'), 컴포넌트 타입 ('dataSourceComponentType'), 속성 ('dataSourceProperty')이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        GameObject targetUIGO = _gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.target });
        if (targetUIGO == null)
        {
            result.success = false;
            result.message = $"UI GameObject '{command.target}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        GameObject dataSourceGO = _gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.dataSourceGameObject });
        if (dataSourceGO == null)
        {
            result.success = false;
            result.message = $"데이터 소스 GameObject '{command.dataSourceGameObject}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        // Generate or find the UIDataBinder script
        string scriptName = "UIDataBinder";
        string scriptPath = $"Assets/Scripts/Generated/{scriptName}.cs"; // Assuming a Generated folder for scripts

        // Ensure the directory exists
        string directory = Path.GetDirectoryName(scriptPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Check if script already exists, if not, create it
        if (!File.Exists(scriptPath))
        {
            string scriptContent = GenerateUIDataBinderScript(scriptName);
            File.WriteAllText(scriptPath, scriptContent);
            AssetDatabase.Refresh(); // Compile the new script
        }

        // Add UIDataBinder component to the target UI GameObject
        // Use AddComponent(Type) as UIDataBinder might not be compiled yet when this Editor script is compiled.
        // Need to use the assembly qualified name for Type.GetType to find the runtime script.
        System.Type binderType = System.Type.GetType(scriptName + ", Assembly-CSharp");
        if (binderType == null)
        {
            result.success = false;
            result.message = $"UIDataBinder 스크립트 타입 '{scriptName}'을 찾을 수 없습니다. 스크립트가 컴파일되었는지 확인하세요.";
            Debug.LogError(result.message);
            return result;
        }
        Component addedComponent = targetUIGO.AddComponent(binderType);
        if (addedComponent == null)
        {
            result.success = false;
            result.message = $"UIDataBinder 컴포넌트를 '{command.target}'에 추가하지 못했습니다.";
            Debug.LogError(result.message);
            return result;
        }

        // Set binder properties using reflection to avoid compile-time type dependency
        binderType.GetField("dataSourceGameObject").SetValue(addedComponent, dataSourceGO);
        binderType.GetField("dataSourceComponentType").SetValue(addedComponent, command.dataSourceComponentType);
        binderType.GetField("dataSourceProperty").SetValue(addedComponent, command.dataSourceProperty);
        binderType.GetField("targetUIComponentType").SetValue(addedComponent, command.uiComponentType);
        binderType.GetField("targetUIProperty").SetValue(addedComponent, command.uiProperty);

        EditorUtility.SetDirty(targetUIGO);
        result.success = true;
        result.message = $"'{command.target}'에 데이터 바인딩이 설정되었습니다. 소스: '{command.dataSourceGameObject}.{command.dataSourceComponentType}.{command.dataSourceProperty}', 대상: '{command.target}.{command.uiComponentType}.{command.uiProperty}'.";
        Debug.Log(result.message);
        return result;
    }

    private string GenerateUIDataBinderScript(string scriptName)
    {
        return $@"
using UnityEngine;
using System;
using System.Reflection;
using TMPro; // For TextMeshProUGUI

public class {scriptName} : MonoBehaviour
{{
    public GameObject dataSourceGameObject;
    public string dataSourceComponentType;
    public string dataSourceProperty;

    public string targetUIComponentType;
    public string targetUIProperty;

    private Component _dataSourceComponent;
    private PropertyInfo _dataSourcePropertyInfo;
    private FieldInfo _dataSourceFieldInfo;

    private Component _targetUIComponent;
    private PropertyInfo _targetUIPropertyInfo;
    private FieldInfo _targetUIFieldInfo;

    void Start()
    {{
        InitializeBinder();
    }}

    void InitializeBinder()
    {{
        if (dataSourceGameObject == null)
        {{
            Debug.LogError(""Data Source GameObject is not assigned for "" + gameObject.name);
            enabled = false;
            return;
        }}

        Type dsComponentType = Type.GetType(dataSourceComponentType);
        if (dsComponentType == null)
        {{
            Debug.LogError($""Data Source Component Type '{{dataSourceComponentType}}' not found for {{gameObject.name}}"");
            enabled = false;
            return;
        }}
        _dataSourceComponent = dataSourceGameObject.GetComponent(dsComponentType);
        if (_dataSourceComponent == null)
        {{
            Debug.LogError($""Data Source Component '{{dataSourceComponentType}}' not found on '{{dataSourceGameObject.name}}' for {{gameObject.name}}"");
            enabled = false;
            return;
        }}

        _dataSourcePropertyInfo = dsComponentType.GetProperty(dataSourceProperty);
        _dataSourceFieldInfo = dsComponentType.GetField(dataSourceProperty);
        if (_dataSourcePropertyInfo == null && _dataSourceFieldInfo == null)
        {{
            Debug.LogError($""Data Source Property/Field '{{dataSourceProperty}}' not found on '{{dataSourceComponentType}}' for {{gameObject.name}}"");
            enabled = false;
            return;
        }}

        Type targetComponentType = Type.GetType(targetUIComponentType);
        if (targetComponentType == null)
        {{
            Debug.LogError($""Target UI Component Type '{{targetUIComponentType}}' not found for {{gameObject.name}}"");
            enabled = false;
            return;
        }}
        _targetUIComponent = GetComponent(targetComponentType);
        if (_targetUIComponent == null)
        {{
            Debug.LogError($""Target UI Component '{{targetUIComponentType}}' not found on '{{gameObject.name}}'"");
            enabled = false;
            return;
        }}

        _targetUIPropertyInfo = targetComponentType.GetProperty(targetUIProperty);
        _targetUIFieldInfo = targetComponentType.GetField(targetUIProperty);
        if (_targetUIPropertyInfo == null && _targetUIFieldInfo == null)
        {{
            Debug.LogError($""Target UI Property/Field '{{targetUIProperty}}' not found on '{{targetUIComponentType}}' for {{gameObject.name}}"");
            enabled = false;
            return;
        }}
    }}

    void Update()
    {{
        if (!enabled) return;

        object sourceValue = null;
        if (_dataSourcePropertyInfo != null)
        {{
            sourceValue = _dataSourcePropertyInfo.GetValue(_dataSourceComponent);
        }}
        else if (_dataSourceFieldInfo != null)
        {{
            sourceValue = _dataSourceFieldInfo.GetValue(_dataSourceComponent);
        }}

        if (sourceValue != null)
        {{
            if (_targetUIPropertyInfo != null)
            {{
                _targetUIPropertyInfo.SetValue(_targetUIComponent, sourceValue);
            }}
            else if (_targetUIFieldInfo != null)
            {{
                _targetUIFieldInfo.SetValue(_targetUIComponent, sourceValue);
            }}
        }}
    }}
}}
";
    }
}
#endif