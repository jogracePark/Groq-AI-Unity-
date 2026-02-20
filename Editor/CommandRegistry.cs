using System.Collections.Generic;
using UnityEngine; // For Debug.LogWarning

namespace SweetHome.Editor.Models
{
    public static class CommandRegistry
    {
        private static Dictionary<string, CommandMetadata> _metadata = new Dictionary<string, CommandMetadata>();

        public static void RegisterCommand(CommandMetadata metadata)
        {
            if (_metadata.ContainsKey(metadata.commandType))
            {
                Debug.LogWarning($"Command '{metadata.commandType}' is already registered. Overwriting.");
                _metadata[metadata.commandType] = metadata;
            }
            else
            {
                _metadata.Add(metadata.commandType, metadata);
            }
        }

        public static CommandMetadata GetMetadata(string commandType)
        {
            _metadata.TryGetValue(commandType, out var metadata);
            return metadata;
        }
    }
}
