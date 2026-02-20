using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq; // For JObject                                                                                                                                                                                            
using UnityEngine;
using System.Linq;

namespace SweetHome.Editor.Models
{
    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteValue($"[{value.x},{value.y}]");
        }

        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                string s = reader.Value.ToString();
                s = s.Trim('[', ']'); // Remove brackets                                                                                                                                                                              
                string[] components = s.Split(',');
                if (components.Length == 2 && float.TryParse(components[0], out float x) && float.TryParse(components[1], out float y))
                {
                    return new Vector2(x, y);
                }
            }
            // Fallback for direct array/object deserialization if it's not a string                                                                                                                                                  
            if (reader.TokenType == JsonToken.StartArray)
            {
                float[] values = serializer.Deserialize<float[]>(reader);
                if (values != null && values.Length == 2)
                {
                    return new Vector2(values[0], values[1]);
                }
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                // Handle {"x":0.5, "y":1.0} format                                                                                                                                                                                   
                JObject obj = JObject.Load(reader);
                return new Vector2(obj["x"].ToObject<float>(), obj["y"].ToObject<float>());
            }

            Debug.LogError($"Failed to parse Vector2 from: {reader.Value}");
            return Vector2.zero;
        }
    }

    public class FloatArrayConverter : JsonConverter<float[]>
    {
        public override void WriteJson(JsonWriter writer, float[] value, JsonSerializer serializer)
        {
            writer.WriteValue($"[{string.Join(",", value)}]");
        }

        public override float[] ReadJson(JsonReader reader, Type objectType, float[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                string s = reader.Value.ToString();
                s = s.Trim('[', ']'); // Remove brackets                                                                                                                                                                              
                string[] components = s.Split(',');
                return components.Select(float.Parse).ToArray();
            }
            // Fallback for direct array deserialization if it's not a string                                                                                                                                                         
            if (reader.TokenType == JsonToken.StartArray)
            {
                return serializer.Deserialize<float[]>(reader);
            }

            Debug.LogError($"Failed to parse float[] from: {reader.Value}");
            return new float[0];
        }
    }

    public class ColorConverter : JsonConverter<Color>
    {
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteValue($"[{value.r},{value.g},{value.b},{value.a}]");
        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                string s = reader.Value.ToString();
                s = s.Trim('[', ']'); // Remove brackets                                                                                                                                                                              
                string[] components = s.Split(',');
                if (components.Length >= 3 && float.TryParse(components[0], out float r) && float.TryParse(components[1], out float g) && float.TryParse(components[2], out float b))
                {
                    float a = 1f;
                    if (components.Length == 4 && float.TryParse(components[3], out float parsedA))
                    {
                        a = parsedA;
                    }
                    return new Color(r, g, b, a);
                }
            }
            // Fallback for direct array/object deserialization if it's not a string                                                                                                                                                  
            if (reader.TokenType == JsonToken.StartArray)
            {
                float[] values = serializer.Deserialize<float[]>(reader);
                if (values != null && values.Length >= 3)
                {
                    float a = (values.Length == 4) ? values[3] : 1f;
                    return new Color(values[0], values[1], values[2], a);
                }
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                // Handle {"r":0.5, "g":1.0, "b":0.5, "a":1.0} format                                                                                                                                                                 
                JObject obj = JObject.Load(reader);
                return new Color(obj["r"].ToObject<float>(), obj["g"].ToObject<float>(), obj["b"].ToObject<float>(), obj["a"]?.ToObject<float>() ?? 1f);
            }

            Debug.LogError($"Failed to parse Color from: {reader.Value}");
            return Color.white;
        }
    }
}