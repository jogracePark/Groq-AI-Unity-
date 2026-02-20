using SweetHome.Editor.Models;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using Newtonsoft.Json; // Required for serializing the callback command
using System;

#if UNITY_EDITOR
public class EventManager
{
    private readonly GameObjectManager _gameObjectManager = new GameObjectManager();

    /// <summary>
    /// Adds a self-contained callback to a UI event, typically a Button's onClick.
    /// The callback itself is another GroqUnityCommand, which is executed at runtime.
    /// </summary>
    public CommandResult AddEventCallback(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target) || command.callbackCommand == null)
        {
            result.success = false;
            result.message = "이벤트 콜백을 추가하려면 'target'과 'callbackCommand'가 필요합니다.";
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

        // For now, we only support Button onClick. This could be expanded.
        Button button = targetGO.GetComponent<Button>();
        if (button == null)
        {
            result.success = false;
            result.message = $"GameObject '{command.target}'에 Button 컴포넌트가 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        // Add or get the handler component
        EventCallbackHandler handler = targetGO.GetComponent<EventCallbackHandler>();
        if (handler == null)
        {
            handler = targetGO.AddComponent<EventCallbackHandler>();
        }

        // Serialize the callback command and store it in the handler
        try
        {
            handler.commandJson = JsonConvert.SerializeObject(command.callbackCommand, Formatting.Indented);
        }
        catch (Exception e)
        {
            result.success = false;
            result.message = $"콜백 명령을 JSON으로 직렬화하는 데 실패했습니다: {e.Message}";
            Debug.LogError(result.message);
            return result;
        }

        // Add a persistent listener from button.onClick to handler.ExecuteCallback
        UnityEventTools.AddPersistentListener(button.onClick, handler.ExecuteCallback);
        EditorUtility.SetDirty(button);
        EditorUtility.SetDirty(handler);

        result.success = true;
        result.message = $"'{targetGO.name}'의 onClick 이벤트에 콜백이 성공적으로 추가되었습니다.";
        Debug.Log(result.message);
        return result;
    }

    [Obsolete("AddClickListener is deprecated. Please use AddEventCallback instead for more robust event handling.")]
    public CommandResult AddClickListener(GroqUnityCommand command)
    {
        return new CommandResult
        {
            success = false,
            commandType = command.commandType,
            message = "AddClickListener는 더 이상 사용되지 않습니다. 더 강력한 이벤트 처리를 위해 'AddEventCallback'을 사용하세요."
        };
    }
}
#endif