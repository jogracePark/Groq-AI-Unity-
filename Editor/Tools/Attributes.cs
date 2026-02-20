using System;

namespace SweetHome.Editor.Tools
{
    /// <summary>
    /// Attribute to mark a class as an MCP tool command handler.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class McpForUnityToolAttribute : Attribute
    {
        public string CommandName { get; }

        public McpForUnityToolAttribute(string commandName = null)
        {
            CommandName = commandName;
        }
    }

    /// <summary>
    /// Attribute to mark a class as an MCP resource command handler.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class McpForUnityResourceAttribute : Attribute
    {
        public string ResourceName { get; }

        public McpForUnityResourceAttribute(string resourceName = null)
        {
            ResourceName = resourceName;
        }
    }
}
