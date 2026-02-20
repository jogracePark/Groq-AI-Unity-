using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Text;
using SweetHome.Editor.Models;

#if UNITY_EDITOR
public class TextureManager
{
    public CommandResult CreateTexture(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.path))
        {
            result.success = false;
            result.message = "텍스처를 생성하려면 'path'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        string fullPath = command.path;
        string directory = Path.GetDirectoryName(fullPath);
        if (!AssetDatabase.IsValidFolder(directory))
        {
            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }

        Texture2D newTexture = null;
        int width = command.textureWidth > 0 ? command.textureWidth : 64;
        int height = command.textureHeight > 0 ? command.textureHeight : 64;

        if (!string.IsNullOrEmpty(command.textureImagePath))
        {
            if (File.Exists(command.textureImagePath))
            {
                byte[] fileData = File.ReadAllBytes(command.textureImagePath);
                newTexture = new Texture2D(2, 2);
                newTexture.LoadImage(fileData);
                newTexture.Apply();
            }
            else
            {
                result.success = false;
                result.message = $"텍스처 이미지 파일을 찾을 수 없습니다: {command.textureImagePath}";
                Debug.LogWarning(result.message);
                return result;
            }
        }
        else if (command.textureColor != null && command.textureColor.Length >= 3)
        {
            newTexture = new Texture2D(width, height);
            Color fillColor = new Color(command.textureColor[0], command.textureColor[1], command.textureColor[2], command.textureColor.Length == 4 ? command.textureColor[3] : 1f);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = fillColor;
            }
            newTexture.SetPixels(pixels);
            newTexture.Apply();
        }
        else if (!string.IsNullOrEmpty(command.texturePattern))
        {
            newTexture = CreatePatternTexture(width, height, command.texturePattern, command.texturePalette);
        }
        else
        {
            newTexture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }
            newTexture.SetPixels(pixels);
            newTexture.Apply();
        }

        if (newTexture != null)
        {
            File.WriteAllBytes(fullPath, newTexture.EncodeToPNG());
            AssetDatabase.Refresh();

            if (!string.IsNullOrEmpty(command.textureImportSettings))
            {
                TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
                if (importer != null)
                {
                    ApplyTextureImportSettings(importer, command.textureImportSettings);
                    importer.SaveAndReimport();
                }
            }
            result.success = true;
            result.message = $"텍스처 생성됨: {fullPath}";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = "텍스처 생성 실패.";
            Debug.LogError(result.message);
        }
        return result;
    }

    public CommandResult CreateSpriteTexture(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.path))
        {
            result.success = false;
            result.message = "스프라이트 텍스처를 생성하려면 'path'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        string fullPath = command.path;
        string directory = Path.GetDirectoryName(fullPath);
        if (!AssetDatabase.IsValidFolder(directory))
        {
            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }

        Texture2D newTexture = null;
        int width = command.textureWidth > 0 ? command.textureWidth : 64;
        int height = command.textureHeight > 0 ? command.textureHeight : 64;

        if (!string.IsNullOrEmpty(command.textureImagePath))
        {
            result.success = false;
            result.message = "이미지 파일에서 스프라이트 로드는 구현이 복잡합니다. (수동 작업 필요)";
            Debug.LogWarning(result.message);
            return result;
        }
        else if (command.textureColor != null && command.textureColor.Length >= 3)
        {
            newTexture = new Texture2D(width, height);
            Color fillColor = new Color(command.textureColor[0], command.textureColor[1], command.textureColor[2], command.textureColor.Length == 4 ? command.textureColor[3] : 1f);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = fillColor;
            }
            newTexture.SetPixels(pixels);
            newTexture.Apply();
        }
        else if (!string.IsNullOrEmpty(command.texturePattern))
        {
            newTexture = CreatePatternTexture(width, height, command.texturePattern, command.texturePalette);
        }
        else
        {
            newTexture = CreatePatternTexture(width, height, "checkerboard", null);
        }

        if (newTexture != null)
        {
            File.WriteAllBytes(fullPath, newTexture.EncodeToPNG());
            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = command.ppu > 0 ? command.ppu : 100;
                if (command.pivot != null)
                {
                    importer.spritePivot = command.pivot;
                }
                importer.SaveAndReimport();
            }
            result.success = true;
            result.message = $"스프라이트 텍스처 생성됨: {fullPath}";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = "스프라이트 텍스처 생성 실패.";
            Debug.LogError(result.message);
        }
        return result;
    }

    public CommandResult ModifyTexture(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.path))
        {
            result.success = false;
            result.message = "텍스처를 수정하려면 'path'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(command.path);
        if (texture == null)
        {
            result.success = false;
            result.message = $"텍스처를 찾을 수 없습니다: {command.path}";
            Debug.LogWarning(result.message);
            return result;
        }

        TextureImporter importer = AssetImporter.GetAtPath(command.path) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(command.path);
        }

        int x = command.x;
        int y = command.y;
        int width = command.textureWidth;
        int height = command.textureHeight;

        if (command.textureColor != null && command.textureColor.Length >= 3)
        {
            Color fillColor = new Color(command.textureColor[0], command.textureColor[1], command.textureColor[2], command.textureColor.Length == 4 ? command.textureColor[3] : 1f);
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    texture.SetPixel(x + i, y + j, fillColor);
                }
            }
        }
        else if (command.pixels != null && command.pixels.Length > 0)
        {
            texture.SetPixels(x, y, width, height, command.pixels);
        }
        else
        {
            result.success = false;
            result.message = "setPixels 명령에 유효한 'color' 또는 'pixels' 데이터가 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        texture.Apply();
        AssetDatabase.SaveAssets();
        result.success = true;
        result.message = $"텍스처 수정됨: {command.path}";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult DeleteTexture(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.path))
        {
            result.success = false;
            result.message = "텍스처를 삭제하려면 'path'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        if (AssetDatabase.DeleteAsset(command.path))
        {
            result.success = true;
            result.message = $"텍스처 삭제됨: {command.path}";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"텍스처를 삭제할 수 없습니다: {command.path}";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    private Texture2D CreatePatternTexture(int width, int height, string patternType, float[] palette)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] colors = new Color[2];
        if (palette != null && palette.Length >= 6)
        {
            colors[0] = new Color(palette[0], palette[1], palette[2], palette.Length >= 4 ? palette[3] : 1f);
            colors[1] = new Color(palette[4], palette[5], palette.Length >= 8 ? palette[6] : 1f, palette.Length >= 8 ? palette[7] : 1f);
        }
        else
        {
            colors[0] = Color.black;
            colors[1] = Color.white;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixelColor = Color.white;
                switch (patternType.ToLower())
                {
                    case "checkerboard":
                        pixelColor = ((x / 8 + y / 8) % 2 == 0) ? colors[0] : colors[1];
                        break;
                    case "stripes":
                    case "stripes_h":
                        pixelColor = (y / 8 % 2 == 0) ? colors[0] : colors[1];
                        break;
                    case "stripes_v":
                        pixelColor = (x / 8 % 2 == 0) ? colors[0] : colors[1];
                        break;
                    case "dots":
                        pixelColor = (Mathf.PerlinNoise((float)x / width * 10, (float)y / height * 10) > 0.5f) ? colors[0] : colors[1];
                        break;
                    default:
                        pixelColor = colors[0];
                        break;
                }
                texture.SetPixel(x, y, pixelColor);
            }
        }
        texture.Apply();
        return texture;
    }

    private void ApplyTextureImportSettings(TextureImporter importer, string importSettingsJson)
    {
        try
        {
            TextureImportSettings settings = JsonUtility.FromJson<TextureImportSettings>(importSettingsJson);

            if (settings.textureType.HasValue)
            {
                importer.textureType = (TextureImporterType)settings.textureType.Value;
            }
            if (settings.filterMode.HasValue)
            {
                importer.filterMode = (FilterMode)settings.filterMode.Value;
            }
            if (settings.wrapMode.HasValue)
            {
                importer.wrapMode = (TextureWrapMode)settings.wrapMode.Value;
            }
            if (settings.isReadable.HasValue)
            {
                importer.isReadable = settings.isReadable.Value;
            }
            if (settings.mipmapEnabled.HasValue)
            {
                importer.mipmapEnabled = settings.mipmapEnabled.Value;
            }
            if (settings.compressionQuality.HasValue)
            {
                importer.compressionQuality = settings.compressionQuality.Value;
            }
            if (settings.maxTextureSize.HasValue)
            {
                importer.maxTextureSize = settings.maxTextureSize.Value;
            }
            if (importer.textureType == TextureImporterType.Sprite)
            {
                if (settings.spritePixelsPerUnit.HasValue)
                {
                    importer.spritePixelsPerUnit = settings.spritePixelsPerUnit.Value;
                }
                if (settings.spritePivotX.HasValue && settings.spritePivotY.HasValue)
                {
                    importer.spritePivot = new Vector2(settings.spritePivotX.Value, settings.spritePivotY.Value);
                }
            }

            Debug.Log("TextureImporter 설정 적용됨.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TextureImporter 설정 적용 중 오류 발생: {e.Message}");
        }
    }

    [System.Serializable]
    private class TextureImportSettings
    {
        public TextureImporterType? textureType;
        public FilterMode? filterMode;
        public TextureWrapMode? wrapMode;
        public bool? isReadable;
        public bool? mipmapEnabled;
        public int? compressionQuality;
        public int? maxTextureSize;

        public float? spritePixelsPerUnit;
        public float? spritePivotX;
        public float? spritePivotY;
    }
}
#endif