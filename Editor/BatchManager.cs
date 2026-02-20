using System.Collections.Generic;
using System.Threading.Tasks;
using SweetHome.Editor.Models;
using UnityEngine;

#if UNITY_EDITOR
public class BatchManager
{
    public CommandResult ExecuteBatch(GroqUnityCommand command)
    {
        var result = new CommandResult { commandType = command.commandType, success = true, message = "Batch execution started." };
        if (command.batch == null || command.batch.Length == 0)
        {
            result.success = false;
            result.message = "Batch command list is empty.";
            Debug.LogWarning(result.message);
            return result;
        }

        Debug.Log($"Executing batch of {command.batch.Length} commands.");
        
        // Per the new architecture, the processor is instantiated directly.
        var commandProcessor = new UnityCommandProcessor();
        var results = new List<CommandResult>();

        foreach (var cmd in command.batch)
        {
            // The command processor is no longer async
            CommandResult singleResult = commandProcessor.ProcessCommand(cmd);
            results.Add(singleResult);
            if (!singleResult.success && command.breakOnError)
            {
                result.success = false;
                result.message = $"Batch execution halted due to error in command '{cmd.commandType}': {singleResult.message}";
                Debug.LogError(result.message);
                break; 
            }
        }

        if (result.success)
        {
            result.message = "Batch execution completed.";
        }
        
        // TODO: Decide on a format for returning multiple results. For now, just returning the final status.
        // result.output = results; 
        
        return result;
    }
}
#endif