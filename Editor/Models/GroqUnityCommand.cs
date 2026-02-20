using System.Collections.Generic;
using UnityEngine; // For Vector2, Color, etc.
using TMPro; // For TextAlignmentOptions
using Newtonsoft.Json; // Added for JsonProperty attribute
using SweetHome.Editor.Models; // For custom converters
using System.Reflection; // Added for reflection
using System; // Added for Type

namespace SweetHome.Editor.Models
{
    [System.Serializable]
    public class GroqUnityCommand : Command
    {
        // Core Command Properties
        public string commandType;
        public string name;
        public string target;
        public string searchMethod;
        public string parent;
        public string primitiveType;
        public bool setActive = true;
        public string componentType;
        public Patch[] patches;
        public string assetPath;
        public string destinationPath;
        public string query;
        public string assetType;
        public bool includeDependencies;
        public string scenePath;
        public bool saveChanges;
        public string newName;
        public string renderMode;
        public string lightType;
        public float lightIntensity;
        public bool compileScripts;
        public string materialPath;
        public string materialTarget;
        public string materialSearchMethod;
        public string materialProperty;
        public string materialValue;
        public int slot;
        public string materialMode;
        public Dictionary<string, string> materialProperties;
        public string texturePath;
        public int width;
        public int height;
        public string textureFormat;
        public bool mipmapEnabled;
        public float duration;
        public string animationType;
        public float startLifetime;
        public float startSpeed;
        public string shader;
        public string typeName;
        public string folderPath;
        public string assetName;
        public bool overwrite;
        public string tagName;
        public string layerName;
        public string toolName;
        public string menuPath;
        public string customToolName;
        public string[] logTypes;
        public int count;
        public string filterText;
        public bool clearConsole;
        public string mode;
        public bool asyncMode;
        public int waitTimeout;
        public string jobId;
        public string parentName;
        public string ancestorName;
        public string requiredComponent;
        public int targetIndex;
        public string fontAssetPath;

        // UI Specific Properties
        [JsonConverter(typeof(FloatArrayToVector2Converter))]
        public Vector2? sizeDelta; // Nullable Vector2
        [JsonConverter(typeof(FloatArrayToVector2Converter))]
        public Vector2? anchoredPosition; // Nullable Vector2
        [JsonConverter(typeof(FloatArrayToVector2Converter))]
        public Vector2? uiPosition; // Nullable Vector2
        [JsonConverter(typeof(FloatArrayToVector2Converter))]
        public Vector2? anchorMin; // Nullable Vector2
        [JsonConverter(typeof(FloatArrayToVector2Converter))]
        public Vector2? anchorMax; // Nullable Vector2
        [JsonConverter(typeof(FloatArrayToVector2Converter))]
        public Vector2? pivot; // Nullable Vector2
        [JsonConverter(typeof(FloatArrayToVector3Converter))]
        public Vector3? position; // Nullable Vector3
        [JsonConverter(typeof(FloatArrayToVector3Converter))]
        public Vector3? rotation; // Nullable Vector3
        [JsonConverter(typeof(FloatArrayToVector3Converter))]
        public Vector3? scale; // Nullable Vector3
        [JsonConverter(typeof(FloatArrayToColorConverter))]
        public Color? lightColor; // Nullable Color
        [JsonConverter(typeof(FloatArrayToColorConverter))]
        public Color? materialColor; // Nullable Color
        [JsonConverter(typeof(FloatArrayToColorConverter))]
        public Color? textureColor; // Nullable Color
        [JsonConverter(typeof(FloatArrayToColorArrayConverter))]
        public Color[] pixels; // Array of Colors
        [JsonConverter(typeof(FloatArrayToColorArrayConverter))]
        public Color[] texturePalette; // Array of Colors

        public string[] options; // For Dropdown
        public bool isOn = false; // For Toggle
        public float minValue = 0f; // For Slider
        public float maxValue = 1f; // For Slider
        public float value = 0f; // For Slider
        public bool wholeNumbers = false; // For Slider
        public string contentName; // For ScrollView
        public bool horizontal = false; // For ScrollView
        public bool vertical = true; // For ScrollView
        public string placeholderText; // For InputField
        public string contentType; // For InputField
        public int characterLimit = 0; // For InputField

        // Layout Properties
        public string layoutGroupType;
        public float spacing = 0f;
        [JsonConverter(typeof(FloatArrayToIntArrayConverter))]
        public int[] padding; // left, right, top, bottom
        [JsonConverter(typeof(FloatArrayToVector2Converter))]
        public Vector2? cellSize; // Nullable Vector2
        public string startCorner;
        public string startAxis;
        public string childAlignment;
        public string constraint;
        public int constraintCount = 0;
        public bool childControlWidth = false;
        public bool childControlHeight = false;
        public bool childForceExpandWidth = false;
        public bool childForceExpandHeight = false;
        public string horizontalFit;
        public string verticalFit;
        public float minWidth = 0f;
        public float minHeight = 0f;
        public float preferredWidth = 0f;
        public float preferredHeight = 0f;
        public float flexibleWidth = 0f;
        public float flexibleHeight = 0f;
        public int layoutPriority = 0;
        public bool ignoreLayout = false;
        public string anchorPreset;

        // CanvasGroup Properties
        public float alpha = 1f;
        public bool interactable = true;
        public bool blocksRaycasts = true;
        public bool ignoreParentGroups = false;

        // Mask Properties
        public bool showMaskGraphic = true;

        // BatchManager Properties
        public GroqUnityCommand[] batch;
        public bool breakOnError;

        // SceneManager Properties
        public int maxDepth = 0;
        public bool includeTransform = false;
        public string searchPattern;
        public string filterType;
        public string shaderName;
        public string path;
        public int buildIndex = -1;
        public string tag;
        public string layer;
        public string source;
        public string destination;
        public string screenshotFileName = "";
        public int screenshotSuperSize = 1;

        // TextureManager Properties
        public int textureWidth = 64;
        public int textureHeight = 64;
        public string textureImagePath;
        public string texturePattern;
        public string textureImportSettings;
        public float ppu = 100f;
        public bool setPixels = false;
        public int x = 0;
        public int y = 0;

        // EventManager Properties
        public string methodName;
        public string methodTargetGameObject;
        public GroqUnityCommand callbackCommand;

        // StyleManager Properties
        public string styleName;
        public string themeAssetPath;
        public string styleType;
        [JsonConverter(typeof(FloatArrayToColorConverter))]
        public Color? color; // Nullable Color
        public float? fontSize;
        public string alignment;
        [JsonConverter(typeof(FloatArrayToColorConverter))]
        public Color? backgroundColor; // Nullable Color
        public string backgroundSpritePath;

        // DataBindingManager Properties
        public string uiComponentType;
        public string uiProperty;
        public string dataSourceGameObject;
        public string dataSourceComponentType;
        public string dataSourceProperty;

        // Conditional Activation Properties
        public string conditionType;
        public float conditionValue;


        public override CommandResult Validate()
        {
            if (string.IsNullOrEmpty(this.commandType))
            {
                return new CommandResult { success = false, message = "Command validation failed: 'commandType' is missing." };
            }

            CommandMetadata metadata = CommandRegistry.GetMetadata(this.commandType);
            if (metadata == null)
            {
                return new CommandResult { success = false, message = $"Command validation failed: Unknown commandType '{this.commandType}'. No metadata found for validation." };
            }

            foreach (ParameterMetadata paramMetadata in metadata.requiredParameters)
            {
                object value = null;
                Type fieldOrPropertyType = null;

                // Try to get field value
                FieldInfo field = typeof(GroqUnityCommand).GetField(paramMetadata.name);
                if (field != null)
                {
                    value = field.GetValue(this);
                    fieldOrPropertyType = field.FieldType;
                }
                else
                {
                    // Try to get property value
                    PropertyInfo prop = typeof(GroqUnityCommand).GetProperty(paramMetadata.name);
                    if (prop != null)
                    {
                        value = prop.GetValue(this);
                        fieldOrPropertyType = prop.PropertyType;
                    }
                }

                if (value == null)
                {
                    // If it's a nullable value type (e.g., Vector2?), it will be null if not provided.
                    // If it's a reference type (e.g., string, array), it will be null if not provided.
                    return new CommandResult { success = false, message = $"Command validation failed for '{this.commandType}': Required parameter '{paramMetadata.name}' is missing (null)." };
                }

                // Special handling for default values of non-nullable value types
                // For example, if 'int count' is required, and JSON didn't provide it, it defaults to 0.
                // We need to decide if 0 is a valid "provided" value or if it means "missing".
                // For now, we'll assume if it's a non-nullable value type and its default, it's missing.
                // This might need refinement based on specific parameter semantics.
                if (fieldOrPropertyType != null && fieldOrPropertyType.IsValueType && Nullable.GetUnderlyingType(fieldOrPropertyType) == null)
                {
                    object defaultValue = Activator.CreateInstance(fieldOrPropertyType);
                    if (value.Equals(defaultValue) && !IsExplicitlySet(paramMetadata.name)) // Need a way to check if explicitly set
                    {
                        // This is a tricky part. If a value type is required and its default value is 0,
                        // and the user didn't provide it, it will be 0. How to distinguish from explicit 0?
                        // For now, we'll rely on the JSON deserialization to set non-default values.
                        // If a required non-nullable value type is 0/false and it's not explicitly set,
                        // it might be considered missing. This requires more advanced JSON parsing context.
                        // For simplicity, we'll assume if it's a non-nullable value type and it's 0/false,
                        // it's considered provided unless it's a string or array that is empty.
                    }
                }

                // Type-specific checks for emptiness
                if (paramMetadata.type == typeof(string))
                {
                    if (string.IsNullOrEmpty(value as string))
                    {
                        return new CommandResult { success = false, message = $"Command validation failed for '{this.commandType}': Required string parameter '{paramMetadata.name}' is empty." };
                    }
                }
                else if (paramMetadata.type == typeof(float[])) // This type is now converted to Vector2/3 or Color
                {
                    // This case should ideally not be hit if converters work correctly for Vector2/3/Color
                    // If it's a raw float array, check its length
                    if (value is float[] floatArray && floatArray.Length == 0)
                    {
                        return new CommandResult { success = false, message = $"Command validation failed for '{this.commandType}': Required float array parameter '{paramMetadata.name}' is empty." };
                    }
                }
                else if (paramMetadata.type == typeof(int[])) // For padding
                {
                    if (value is int[] intArray && intArray.Length == 0)
                    {
                        return new CommandResult { success = false, message = $"Command validation failed for '{this.commandType}': Required int array parameter '{paramMetadata.name}' is empty." };
                    }
                }
                else if (paramMetadata.type == typeof(GroqUnityCommand[]))
                {
                    if (value is GroqUnityCommand[] commandArray && commandArray.Length == 0)
                    {
                        return new CommandResult { success = false, message = $"Command validation failed for '{this.commandType}': Required command array parameter '{paramMetadata.name}' is empty." };
                    }
                }
                else if (paramMetadata.type == typeof(string[]))
                {
                    if (value is string[] stringArray && stringArray.Length == 0)
                    {
                        return new CommandResult { success = false, message = $"Command validation failed for '{this.commandType}': Required string array parameter '{paramMetadata.name}' is empty." };
                    }
                }
                // For nullable value types (Vector2?, Color?), if they are not null, they are considered provided.
                // For non-nullable value types (bool, int, float), they always have a value, so they are considered provided if they reach here.
            }
            return new CommandResult { success = true, message = "Command validated successfully." };
        }

        // Helper to check if a parameter was explicitly set (more robust check would involve custom JsonConverter or parsing raw JSON)
        // For now, this is a placeholder and might not be fully accurate for all scenarios, especially for default value types.
        private bool IsExplicitlySet(string parameterName)
        {
            // This is a simplification. A true check would involve inspecting the raw JSON payload
            // to see if the key was present, even if its value was default.
            // For now, we assume if a nullable type is null, it wasn't set.
            // For non-nullable types, it's hard to tell without custom deserialization logic.
            return true; // Assume it's set if it's not null (for nullable types) or has a non-default value (for some value types)
        }
    }

    [System.Serializable]
    public class Patch
    {
        [JsonProperty("property")]
        public string propertyName;
        public string value;
    }

    // Custom JsonConverters for robust deserialization
    public class FloatArrayToVector2Converter : JsonConverter<Vector2?>
    {
        public override Vector2? ReadJson(JsonReader reader, Type objectType, Vector2? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.StartArray)
            {
                List<float> floatList = new List<float>();
                reader.Read(); // Move to first value
                while (reader.TokenType != JsonToken.EndArray)
                {
                    if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
                    {
                        floatList.Add(Convert.ToSingle(reader.Value));
                    }
                    else
                    {
                        throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing Vector2.");
                    }
                    reader.Read();
                }

                if (floatList.Count == 2)
                {
                    return new Vector2(floatList[0], floatList[1]);
                }
                else
                {
                    throw new JsonSerializationException($"Expected 2 elements for Vector2, but got {floatList.Count}.");
                }
            }
            else if (reader.TokenType == JsonToken.String)
            {
                // Handle string format like "[1.0, 2.0]"
                string s = reader.Value.ToString();
                s = s.Trim('[', ']');
                string[] parts = s.Split(',');
                if (parts.Length == 2 && float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y))
                {
                    return new Vector2(x, y);
                }
                throw new JsonSerializationException($"Cannot convert string '{s}' to Vector2.");
            }

            throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing Vector2. Expected array or string.");
        }

        public override void WriteJson(JsonWriter writer, Vector2? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            writer.WriteStartArray();
            writer.WriteValue(value.Value.x);
            writer.WriteValue(value.Value.y);
            writer.WriteEndArray();
        }
    }

    public class FloatArrayToVector3Converter : JsonConverter<Vector3?>
    {
        public override Vector3? ReadJson(JsonReader reader, Type objectType, Vector3? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.StartArray)
            {
                List<float> floatList = new List<float>();
                reader.Read(); // Move to first value
                while (reader.TokenType != JsonToken.EndArray)
                {
                    if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
                    {
                        floatList.Add(Convert.ToSingle(reader.Value));
                    }
                    else
                    {
                        throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing Vector3.");
                    }
                    reader.Read();
                }

                if (floatList.Count == 3)
                {
                    return new Vector3(floatList[0], floatList[1], floatList[2]);
                }
                else
                {
                    throw new JsonSerializationException($"Expected 3 elements for Vector3, but got {floatList.Count}.");
                }
            }
            else if (reader.TokenType == JsonToken.String)
            {
                string s = reader.Value.ToString();
                s = s.Trim('[', ']');
                string[] parts = s.Split(',');
                if (parts.Length == 3 && float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y) && float.TryParse(parts[2], out float z))
                {
                    return new Vector3(x, y, z);
                }
                throw new JsonSerializationException($"Cannot convert string '{s}' to Vector3.");
            }

            throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing Vector3. Expected array or string.");
        }

        public override void WriteJson(JsonWriter writer, Vector3? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            writer.WriteStartArray();
            writer.WriteValue(value.Value.x);
            writer.WriteValue(value.Value.y);
            writer.WriteValue(value.Value.z);
            writer.WriteEndArray();
        }
    }

    public class FloatArrayToColorConverter : JsonConverter<Color?>
    {
        public override Color? ReadJson(JsonReader reader, Type objectType, Color? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.StartArray)
            {
                List<float> floatList = new List<float>();
                reader.Read(); // Move to first value
                while (reader.TokenType != JsonToken.EndArray)
                {
                    if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
                    {
                        floatList.Add(Convert.ToSingle(reader.Value));
                    }
                    else
                    {
                        throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing Color.");
                    }
                    reader.Read();
                }

                if (floatList.Count == 3) // RGB
                {
                    return new Color(floatList[0], floatList[1], floatList[2]);
                }
                else if (floatList.Count == 4) // RGBA
                {
                    return new Color(floatList[0], floatList[1], floatList[2], floatList[3]);
                }
                else
                {
                    throw new JsonSerializationException($"Expected 3 or 4 elements for Color, but got {floatList.Count}.");
                }
            }
            else if (reader.TokenType == JsonToken.String)
            {
                string s = reader.Value.ToString();
                s = s.Trim('[', ']');
                string[] parts = s.Split(',');
                if (parts.Length == 3 && float.TryParse(parts[0], out float r) && float.TryParse(parts[1], out float g) && float.TryParse(parts[2], out float b))
                {
                    return new Color(r, g, b);
                }
                else if (parts.Length == 4 && float.TryParse(parts[0], out float r2) && float.TryParse(parts[1], out float g2) && float.TryParse(parts[2], out float b2) && float.TryParse(parts[3], out float a2))
                {
                    return new Color(r2, g2, b2, a2);
                }
                throw new JsonSerializationException($"Cannot convert string '{s}' to Color.");
            }

            throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing Color. Expected array or string.");
        }

        public override void WriteJson(JsonWriter writer, Color? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            writer.WriteStartArray();
            writer.WriteValue(value.Value.r);
            writer.WriteValue(value.Value.g);
            writer.WriteValue(value.Value.b);
            writer.WriteValue(value.Value.a);
            writer.WriteEndArray();
        }
    }

    public class FloatArrayToColorArrayConverter : JsonConverter<Color[]>
    {
        public override Color[] ReadJson(JsonReader reader, Type objectType, Color[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.StartArray)
            {
                List<Color> colors = new List<Color>();
                while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                {
                    if (reader.TokenType == JsonToken.StartArray || reader.TokenType == JsonToken.String)
                    {
                        // Recursively use the single Color converter
                        Color? color = serializer.Deserialize<Color?>(reader);
                        if (color.HasValue)
                        {
                            colors.Add(color.Value);
                        }
                    }
                    else
                    {
                        throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing Color array.");
                    }
                }
                return colors.ToArray();
            }
            throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing Color array. Expected array.");
        }

        public override void WriteJson(JsonWriter writer, Color[] value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            writer.WriteStartArray();
            foreach (Color color in value)
            {
                serializer.Serialize(writer, color);
            }
            writer.WriteEndArray();
        }
    }

    public class FloatArrayToIntArrayConverter : JsonConverter<int[]>
    {
        public override int[] ReadJson(JsonReader reader, Type objectType, int[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.StartArray)
            {
                List<int> intList = new List<int>();
                reader.Read(); // Move to first value
                while (reader.TokenType != JsonToken.EndArray)
                {
                    if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float) // Allow float and convert to int
                    {
                        intList.Add(Convert.ToInt32(reader.Value));
                    }
                    else
                    {
                        throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing int array.");
                    }
                    reader.Read();
                }
                return intList.ToArray();
            }
            else if (reader.TokenType == JsonToken.String)
            {
                string s = reader.Value.ToString();
                s = s.Trim('[', ']');
                string[] parts = s.Split(',');
                List<int> intList = new List<int>();
                foreach (string part in parts)
                {
                    if (int.TryParse(part.Trim(), out int i))
                    {
                        intList.Add(i);
                    }
                    else
                    {
                        throw new JsonSerializationException($"Cannot convert string part '{part}' to int.");
                    }
                }
                return intList.ToArray();
            }
            throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing int array. Expected array or string.");
        }

        public override void WriteJson(JsonWriter writer, int[] value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            writer.WriteStartArray();
            foreach (int i in value)
            {
                writer.WriteValue(i);
            }
            writer.WriteEndArray();
        }
    }
}
