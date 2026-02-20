using System;
using System.Collections.Generic;

namespace SweetHome.Editor.Models
{
    /// <summary>
    /// Represents metadata for a single command parameter.
    /// </summary>
    [Serializable]
    public class ParameterMetadata
    {
        public string name;
        public Type type; // The expected System.Type of the parameter

        public ParameterMetadata(string name, Type type)
        {
            this.name = name;
            this.type = type;
        }
    }

    /// <summary>
    /// Defines metadata for each AI command, implementing the "capability-based command registration / dynamic discernment" philosophy.
    /// </summary>
    [Serializable]
    public class CommandMetadata
    {
        public string commandType;
        public bool isDestructive;
        public bool isEditorOnly;
        public bool isBatchable;
        public string description;
        public List<ParameterMetadata> requiredParameters;
        public List<ParameterMetadata> optionalParameters;
        public string managerName;

        public CommandMetadata(
            string commandType,
            string description,
            bool isDestructive = false,
            bool isEditorOnly = true,
            bool isBatchable = true,
            List<ParameterMetadata> requiredParameters = null,
            List<ParameterMetadata> optionalParameters = null,
            string managerName = "Unknown"
        )
        {
            this.commandType = commandType;
            this.description = description;
            this.isDestructive = isDestructive;
            this.isEditorOnly = isEditorOnly;
            this.isBatchable = isBatchable;
            this.requiredParameters = requiredParameters ?? new List<ParameterMetadata>();
            this.optionalParameters = optionalParameters ?? new List<ParameterMetadata>();
            this.managerName = managerName;
        }
    }
}