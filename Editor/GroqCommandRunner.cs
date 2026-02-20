using UnityEditor;
using UnityEngine;
using SweetHome.Editor.Models; // GroqUnityCommand를 위해 필요
using Newtonsoft.Json; // JSON 역직렬화를 위해 필요
using System.Threading.Tasks; // For async/await

public class GroqCommandRunner : EditorWindow
{
    private string userPrompt = "새로운 큐브를 씬에 생성해줘.";
    private string jsonCommandInput = ""; // New field for direct JSON input

    [MenuItem("Tools/Groq Command Runner")]
    public static void ShowWindow()
    {
        GetWindow<GroqCommandRunner>("Groq Command Runner");
    }

    private void OnGUI()
    {
        GUILayout.Label("AI 명령 실행 (CLI Agent)", EditorStyles.boldLabel);

        // Original user prompt input (can be used to generate JSON via CLI agent)
        GUILayout.Label("자연어 프롬프트 (CLI Agent에게 전달)", EditorStyles.miniLabel);
        userPrompt = EditorGUILayout.TextArea(userPrompt, GUILayout.Height(50));
        EditorGUILayout.Space();

        // New JSON command input
        GUILayout.Label("JSON 명령 직접 입력", EditorStyles.miniLabel);
        jsonCommandInput = EditorGUILayout.TextArea(jsonCommandInput, GUILayout.Height(150));

        if (GUILayout.Button("Execute JSON Command"))
        {
            ExecuteJsonCommand(); // 비동기 메서드를 호출하고 결과를 기다리지 않음
        }
    }

    private void ExecuteJsonCommand()
    {
        if (string.IsNullOrEmpty(jsonCommandInput))
        {
            Debug.LogWarning("실행할 JSON 명령이 비어 있습니다.");
            return;
        }

        Debug.Log("JSON 명령 실행 시작...");
        UnityCommandProcessor processor = new UnityCommandProcessor();
        CommandResult finalResult = new CommandResult { success = true, message = "JSON command(s) executed." };

        try
        {
            // AI가 단일 명령 또는 명령 배열을 반환할 수 있으므로 유연하게 처리
            if (jsonCommandInput.Trim().StartsWith("[")) // JSON 배열인 경우
            {
                GroqUnityCommand[] commands = JsonConvert.DeserializeObject<GroqUnityCommand[]>(jsonCommandInput);
                if (commands == null || commands.Length == 0)
                {
                    finalResult.success = false;
                    finalResult.message = "유효한 Unity 명령 배열이 없습니다.";
                    Debug.LogError(finalResult.message);
                    return;
                }

                foreach (var command in commands)
                {
                    Debug.Log($"명령 실행: {command.commandType}");
                    CommandResult result = processor.ProcessCommand(command);
                    if (!result.success)
                    {
                        finalResult = result; // Store the first failure
                        Debug.LogError($"명령 '{command.commandType}' 실패: {result.message}");
                        break; // Stop on first error for batch
                    }
                }
            }
            else // 단일 JSON 객체인 경우
            {
                GroqUnityCommand command = JsonConvert.DeserializeObject<GroqUnityCommand>(jsonCommandInput);
                if (command == null)
                {
                    finalResult.success = false;
                    finalResult.message = "유효한 Unity 명령으로 역직렬화할 수 없습니다.";
                    Debug.LogError(finalResult.message);
                    return;
                }

                Debug.Log($"명령 실행: {command.commandType}");
                finalResult = processor.ProcessCommand(command);
            }
        }
        catch (System.Exception e)
        {
            finalResult.success = false;
            finalResult.message = $"JSON 명령 처리 중 오류 발생: {e.Message}\n내용: {jsonCommandInput}";
            Debug.LogError(finalResult.message);
        }

        if (finalResult.success)
        {
            Debug.Log($"JSON 명령 성공: {finalResult.message}");
            if (!string.IsNullOrEmpty(finalResult.output))
            {
                Debug.Log($"출력: {finalResult.output}");
            }
        }
        else
        {
            Debug.LogError($"JSON 명령 실패: {finalResult.message}");
        }
    }
}

