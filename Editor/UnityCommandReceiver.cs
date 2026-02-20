#if UNITY_EDITOR
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using SweetHome.Editor.Models;
using static UnityEngine.Rendering.DebugUI;

// --- LLM INTERFACE INSTRUCTIONS ---
// This Unity Editor extension exposes an HTTP endpoint to receive and execute commands.
// External agents (like an LLM or CLI tool) can send commands to Unity via this interface.
//
// Endpoint: http://localhost:8080/executeCommand/
// Method: POST
// Content-Type: application/json
//
// Request Body:
// The request body must be a JSON string representing a single GroqUnityCommand object
// or a JSON array of GroqUnityCommand objects for batch execution.
//
// CRITICAL INSTRUCTION:

// You MUST strictly adhere to the GroqUnityCommand JSON schema.

// - You MUST read the schema before generating any command.
// - You MUST ONLY use fields defined in the schema.
// - You MUST NOT invent new fields or omit required fields.
// - You MUST match the exact data types defined in the schema.
// - You MUST output valid JSON only.

// Any deviation from the schema will cause the command to fail.

// If you are unsure, DO NOT guess. Refer to the schema.
// can be found at: https://raw.githubusercontent.com/jogracePark/Groq-AI-Unity-/refs/heads/main/unity_command_schemas.json
//
// Example curl command for a single command:
// curl -X POST -H "Content-Type: application/json" -d '{ "commandType": "CreateGameObject", "name": "MyCube", "primitiveType": "Cube" }' http://localhost:8080/executeCommand/
//
// Example curl command for multiple commands (batch):
// curl -X POST -H "Content-Type: application/json" -d '[ { "commandType": "CreateGameObject", "name": "Sphere1", "primitiveType": "Sphere" }, { "commandType": "ModifyGameObject", "target": "Sphere1", "position": [0, 5, 0] } ]' http://localhost:8080/executeCommand/
//
// Response:
// The server will respond with a JSON object containing a CommandResult, indicating
// success or failure and any relevant messages.
// Example: {"success": true, "message": "GameObject created: MyCube"}
// --- END LLM INTERFACE INSTRUCTIONS ---

namespace SweetHome.Editor

{
    [InitializeOnLoad]
    public static class UnityCommandReceiver
    {
        private static HttpListener listener;
        private static Thread listenerThread;
        private static CancellationTokenSource cancellationTokenSource;
        private const string prefix = "http://localhost:8080/executeCommand/";

        static UnityCommandReceiver()
        {
            EditorApplication.delayCall += Initialize;
            EditorApplication.quitting += StopListener;
        }

        private static void Initialize()
        {
            StartListener();
        }

        private static void StartListener()
        {
            if (listener != null && listener.IsListening)
            {
                Debug.Log("UnityCommandReceiver: Listener is already running.");
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();
            listener = new HttpListener();
            listener.Prefixes.Add(prefix);

            try
            {
                listener.Start();
                Debug.Log($"UnityCommandReceiver: Started listening on {prefix}");

                listenerThread = new Thread(async () => await HandleRequests(cancellationTokenSource.Token));
                listenerThread.IsBackground = true;
                listenerThread.Start();
            }
            catch (Exception e)
            {
                Debug.LogError($"UnityCommandReceiver: Failed to start listener: {e.Message}");
            }
        }

        private static void StopListener()
        {
            if (listener != null && listener.IsListening)
            {
                cancellationTokenSource.Cancel();
                listener.Stop();
                listener.Close();
                Debug.Log("UnityCommandReceiver: Stopped listener.");
            }
            if (listenerThread != null && listenerThread.IsAlive)
            {
                listenerThread.Join(); // Wait for the thread to finish
            }
        }

        private static async Task HandleRequests(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    if (cancellationToken.IsCancellationRequested) break;

                    _ = ProcessRequest(context); // Process request without awaiting to not block the listener thread
                }
                catch (HttpListenerException ex)
                {
                    // This exception is common when the listener is stopped
                    if (ex.ErrorCode == 995) // ERROR_OPERATION_ABORTED
                    {
                        Debug.Log("UnityCommandReceiver: Listener was stopped.");
                    }
                    else
                    {
                        Debug.LogError($"UnityCommandReceiver: HttpListenerException: {ex.Message}");
                    }
                    break;
                }
                catch (ObjectDisposedException)
                {
                    // Listener was disposed, exit loop
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogError($"UnityCommandReceiver: Error in HandleRequests loop: {e.Message}");
                }
            }
        }

        private static async Task ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string responseString = "";
            CommandResult commandResult = new CommandResult { success = false, message = "Unknown error." };
            string requestBody = string.Empty; // Moved declaration here

            try
            {
                if (request.HttpMethod == "POST")
                {
                    using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        requestBody = await reader.ReadToEndAsync();
                    }

                    Debug.Log($"UnityCommandReceiver: Received command request: {requestBody}");

                    UnityCommandProcessor processor = new UnityCommandProcessor();

                    // AI가 단일 명령 또는 명령 배열을 반환할 수 있으므로 유연하게 처리
                    if (requestBody.Trim().StartsWith("[")) // JSON 배열인 경우
                    {
                        GroqUnityCommand[] commands = JsonConvert.DeserializeObject<GroqUnityCommand[]>(requestBody);
                        if (commands == null || commands.Length == 0)
                        {
                            commandResult.message = "유효한 Unity 명령 배열이 없습니다.";
                        }
                        else
                        {
                            var tcs = new TaskCompletionSource<CommandResult>();
                            EditorApplication.delayCall += () =>
                            {
                                CommandResult lastResult = new CommandResult { success = true, message = "Batch execution completed." };
                                foreach (var command in commands)
                                {
                                    Debug.Log($"UnityCommandReceiver: Executing command: {command.commandType}");
                                    lastResult = processor.ProcessCommand(command);
                                    if (!lastResult.success)
                                    {
                                        commandResult = lastResult; // Store the first failure
                                        break; // Stop on first error for batch
                                    }
                                }
                                tcs.SetResult(lastResult);
                            };
                            commandResult = await tcs.Task;
                        }
                    }
                    else // 단일 JSON 객체인 경우
                    {
                        GroqUnityCommand command = JsonConvert.DeserializeObject<GroqUnityCommand>(requestBody);
                        if (command == null)
                        {
                            commandResult.message = "유효한 Unity 명령으로 역직렬화할 수 없습니다.";
                        }
                        else
                        {
                            var tcs = new TaskCompletionSource<CommandResult>();
                            EditorApplication.delayCall += () =>
                            {
                                Debug.Log($"UnityCommandReceiver: Executing command: {command.commandType}");
                                tcs.SetResult(processor.ProcessCommand(command));
                            };
                            commandResult = await tcs.Task;
                        }
                    }
                }
                else
                {
                    commandResult.message = "Only POST requests are supported.";
                }
            }
            catch (JsonSerializationException e)
            {
                commandResult.message = $"JSON 역직렬화 오류: {e.Message}. 수신된 JSON: {requestBody}";
                Debug.LogError(commandResult.message);
            }
            catch (Exception e)
            {
                commandResult.message = $"명령 처리 중 오류 발생: {e.Message}";
                Debug.LogError(commandResult.message);
            }

            responseString = JsonConvert.SerializeObject(commandResult);
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.ContentType = "application/json";
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
}
#endif
