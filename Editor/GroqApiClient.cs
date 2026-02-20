using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using UnityEditor;
using System.Threading.Tasks;
using SweetHome.Editor.Models;
using Newtonsoft.Json; // For JsonConvert.DeserializeObject
using System.Linq; // For string.Join
using NUnit.Framework.Internal;

public class GroqApiClient
{
    private const string GroqApiUrlKey = "GroqApiUrl";
    private const string GroqApiKeyKey = "GroqApiKey";
    private const string SchemaUrl = "https://raw.githubusercontent.com/jogracePark/Groq-AI-Unity-/refs/heads/main/unity_command_schemas.json";

    private string GroqApiUrl => EditorPrefs.GetString(GroqApiUrlKey, "YOUR_Groq_API_ENDPOINT_HERE");
    private string GroqApiKey => EditorPrefs.GetString(GroqApiKeyKey, "YOUR_Groq_API_KEY_HERE");

    [System.Serializable]
    public class GroqRequest
    {
        public string model;
        public Message[] messages;
        public float temperature;
        // public ResponseFormat response_format; // Removed for JSON object response
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class ResponseFormat
    {
        public string type;
    }

    [System.Serializable]
    public class GroqResponse
    {
        public string id;
        public string model;
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice
    {
        public int index;
        public Message message;
        public string finish_reason;
    }

    public async Task<string> SendGroqRequest(string userPrompt, string systemPrompt = null)
    {
        var messages = new System.Collections.Generic.List<Message>();
        if (systemPrompt != null)
        {
            messages.Add(new Message { role = "system", content = systemPrompt });
        }
        messages.Add(new Message { role = "user", content = userPrompt });

        GroqRequest requestBody = new GroqRequest
        {
            model = "llama3-8b-8192", // Use a suitable Groq model
            messages = messages.ToArray(),
            temperature = 0.7f
            // response_format = new ResponseFormat { type = "json_object" } // Removed for JSON object response
        };

        string jsonRequestBody = JsonConvert.SerializeObject(requestBody); // Use JsonConvert for better JSON serialization

        using (UnityWebRequest webRequest = new UnityWebRequest(GroqApiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", "Bearer " + GroqApiKey);

            var operation = webRequest.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Groq API Error: " + webRequest.error);
                return "Error: " + webRequest.error;
            }
            else
            {
                string responseJson = webRequest.downloadHandler.text;
                Debug.Log("Groq API Response: " + responseJson);

                GroqResponse groqResponse = JsonConvert.DeserializeObject<GroqResponse>(responseJson); // Use JsonConvert
                if (groqResponse != null && groqResponse.choices != null && groqResponse.choices.Length > 0)
                {
                    return groqResponse.choices[0].message.content;
                }
                else
                {
                    return "Error: No valid response from Groq.";
                }
            }
        }
    }

    /// <summary>
    /// 사용자 프롬프트를 Groq API로 보내고, 받은 JSON 명령을 UnityCommandProcessor를 통해 실행합니다.
    /// </summary>
    /// <param name="userPrompt">AI에게 보낼 사용자 프롬프트입니다.</param>
    /// <returns>최종 명령 실행 결과를 반환합니다.</returns>
    public async Task<CommandResult> SendPromptAndExecuteCommand(string userPrompt)
    {
        Debug.Log($"AI에게 프롬프트 전송: {userPrompt}");

        string systemPrompt = $"당신은 Unity Editor를 제어하는 AI 어시스턴트입니다. 사용자의 요청을 GroqUnityCommand JSON 객체로 변환하여 반환해야 합니다. 상세한 명령 구조는 {SchemaUrl}을(를) 참조하세요. 응답은 반드시 GroqUnityCommand JSON 객체 형식이어야 합니다. 여러 명령이 필요한 경우 JSON 배열로 반환하세요. 추가 설명이나 대화는 JSON 외부에 주석으로 작성하세요.";

        string jsonResponse = await SendGroqRequest(userPrompt, systemPrompt);

        if (jsonResponse.StartsWith("Error:"))
        {
            return new CommandResult { success = false, message = jsonResponse };
        }

        try
        {
            // AI가 단일 명령 또는 명령 배열을 반환할 수 있으므로 유연하게 처리
            if (jsonResponse.Trim().StartsWith("[")) // JSON 배열인 경우
            {
                GroqUnityCommand[] commands = JsonConvert.DeserializeObject<GroqUnityCommand[]>(jsonResponse);
                if (commands == null || commands.Length == 0)
                {
                    return new CommandResult { success = false, message = "AI 응답에 유효한 Unity 명령 배열이 없습니다." };
                }

                CommandResult lastResult = new CommandResult { success = true, message = "Batch execution completed." };
                UnityCommandProcessor processor = new UnityCommandProcessor();

                foreach (var command in commands)
                {
                    Debug.Log($"AI로부터 명령 수신: {command.commandType}");
                    lastResult = processor.ProcessCommand(command);
                    if (!lastResult.success)
                    {
                        Debug.LogError($"명령 '{command.commandType}' 실패: {lastResult.message}");
                        return lastResult; // 첫 번째 실패에서 중단
                    }
                }
                return lastResult;
            }
            else // 단일 JSON 객체인 경우
            {
                GroqUnityCommand command = JsonConvert.DeserializeObject<GroqUnityCommand>(jsonResponse);
                if (command == null)
                {
                    return new CommandResult { success = false, message = "AI 응답을 유효한 Unity 명령으로 역직렬화할 수 없습니다." };
                }

                Debug.Log($"AI로부터 명령 수신: {command.commandType}");
                UnityCommandProcessor processor = new UnityCommandProcessor();
                CommandResult result = processor.ProcessCommand(command);
                return result;
            }
        }
        catch (System.Exception e)
        {
            return new CommandResult { success = false, message = $"AI 응답 처리 중 오류 발생: {e.Message}\n응답 내용: {jsonResponse}" };
        }
    }

    /// <summary>
    /// Unity 명령 실행 결과를 LLM에 보고하고, LLM으로부터 다음 명령을 받아 실행합니다.
    /// </summary>
    /// <param name="resultJson">이전 명령의 실행 결과 JSON입니다.</param>
    /// <returns>다음 명령의 실행 결과를 반환합니다 (없으면 성공 메시지).</returns>
    public async Task<CommandResult> ReportUnityCommandResultToLLM(string resultJson)
    {
        Debug.Log($"[Unity Command Result] {resultJson}");

        string systemPrompt = $"당신은 Unity Editor를 제어하는 AI 어시스턴트입니다. 이전 Unity 명령 실행 결과는 다음과 같습니다: {resultJson}. 이 결과를 바탕으로 다음 Unity 명령 (GroqUnityCommand JSON 객체 형식)을 반환해야 합니다. 상세한 명령 구조는 {SchemaUrl}을(를) 참조하세요. 더 이상 필요한 작업이 없으면 빈 JSON 배열: []을 반환하세요. 응답은 반드시 GroqUnityCommand JSON 객체 형식 또는 빈 JSON 배열이어야 합니다. 추가 설명이나 대화는 JSON 외부에 주석으로 작성하세요.";

        string jsonResponse = await SendGroqRequest("이전 명령 결과에 따라 다음 작업을 수행하세요.", systemPrompt);

        if (jsonResponse.StartsWith("Error:"))
        {
            return new CommandResult { success = false, message = "Groq API Error: " + jsonResponse };
        }

        try
        {
            if (jsonResponse.Trim() == "[]")
            {
                return new CommandResult { success = true, message = "AI: 더 이상 실행할 명령이 없습니다." };
            }

            if (jsonResponse.Trim().StartsWith("[")) // JSON 배열인 경우
            {
                GroqUnityCommand[] commands = JsonConvert.DeserializeObject<GroqUnityCommand[]>(jsonResponse);
                if (commands == null || commands.Length == 0)
                {
                    return new CommandResult { success = false, message = "AI 응답에 유효한 Unity 명령 배열이 없습니다." };
                }

                CommandResult lastResult = new CommandResult { success = true, message = "Batch execution completed." };
                UnityCommandProcessor processor = new UnityCommandProcessor();

                foreach (var command in commands)
                {
                    Debug.Log($"AI로부터 다음 명령 수신: {command.commandType}");
                    lastResult = processor.ProcessCommand(command);
                    if (!lastResult.success)
                    {
                        Debug.LogError($"명령 '{command.commandType}' 실패: {lastResult.message}");
                        return lastResult;
                    }
                }
                return lastResult;
            }
            else // 단일 JSON 객체인 경우
            {
                GroqUnityCommand nextCommand = JsonConvert.DeserializeObject<GroqUnityCommand>(jsonResponse);
                if (nextCommand == null)
                {
                    return new CommandResult { success = false, message = "AI 응답을 유효한 Unity 명령으로 역직렬화할 수 없습니다." };
                }

                Debug.Log($"AI로부터 다음 명령 수신: {nextCommand.commandType}");
                UnityCommandProcessor processor = new UnityCommandProcessor();
                CommandResult nextResult = processor.ProcessCommand(nextCommand);
                return nextResult;
            }
        }
        catch (System.Exception e)
        {
            return new CommandResult { success = false, message = $"AI 응답 처리 중 오류 발생: {e.Message}\n응답 내용: {jsonResponse}" };
        }
    }
}

    
