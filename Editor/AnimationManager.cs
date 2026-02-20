#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using SweetHome.Editor.Models;

public class AnimationManager
{
    private readonly GameObjectManager _gameObjectManager = new GameObjectManager();

    public CommandResult AnimateUI(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.target) || string.IsNullOrEmpty(command.animationType))
        {
            result.success = false;
            result.message = "UI를 애니메이션하려면 'target'과 'animationType'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        GameObject targetGO = _gameObjectManager.FindGameObject(new GroqUnityCommand { target = command.target });
        if (targetGO == null)
        {
            result.success = false;
            result.message = $"애니메이션할 GameObject '{command.target}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        UIAnimator animator = targetGO.GetComponent<UIAnimator>();
        if (animator == null)
        {
            animator = targetGO.AddComponent<UIAnimator>();
        }

        float duration = command.duration > 0 ? command.duration : 0.5f;

        switch (command.animationType.ToLower())
        {
            case "fadein":
                animator.FadeIn(duration);
                break;
            case "fadeout":
                animator.FadeOut(duration);
                break;
            case "scaleup":
                animator.ScaleUp(duration);
                break;
            case "scaledown":
                animator.ScaleDown(duration);
                break;
            default:
                result.success = false;
                result.message = $"알 수 없는 애니메이션 유형: {command.animationType}";
                Debug.LogWarning(result.message);
                return result;
        }

        result.success = true;
        result.message = $"GameObject '{command.target}'에 '{command.animationType}' 애니메이션이 시작되었습니다.";
        Debug.Log(result.message);
        return result;
    }
}
#endif
