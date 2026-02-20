using System.Text.RegularExpressions;

namespace SweetHome.Editor.Tools
{
    public static class StringCaseUtility
    {
        public static string ToSnakeCase(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            return Regex.Replace(name, "([A-Z])([A-Z][a-z])|([a-z0-9])([A-Z])", "$1$3_$2$4").ToLower();
        }
    }
}
