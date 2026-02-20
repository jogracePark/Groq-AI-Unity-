using UnityEngine;

namespace SweetHome.Editor.Tools
{
    public static class McpLog
    {
        public static void Info(string message, bool always = true)
        {
            if (always)
            {
                Debug.Log($"[MCP Info] {message}");
            }
        }

        public static void Warn(string message, bool always = true)
            {
            if (always)
            {
                Debug.LogWarning($"[MCP Warn] {message}");
            }
        }

        public static void Error(string message, bool always = true)
        {
            if (always)
            {
                Debug.LogError($"[MCP Error] {message}");
            }
        }
    }
}
