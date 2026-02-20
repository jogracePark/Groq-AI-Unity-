#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using SweetHome.Editor.Models;

public class MaterialManager
{
    public CommandResult GetMaterialInfo(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.materialPath))
        {
            result.success = false;
            result.message = "재질 정보를 가져오려면 'materialPath'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }
        Material material = AssetDatabase.LoadAssetAtPath<Material>(command.materialPath);
        if (material != null)
        {
            result.success = true;
            result.message = $"재질 정보: {command.materialPath}, 셰이더: {material.shader.name}";
            result.output = JsonUtility.ToJson(new { path = command.materialPath, shader = material.shader.name });
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"재질을 찾을 수 없습니다: {command.materialPath}";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    public CommandResult CreateMaterial(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.materialPath))
        {
            result.success = false;
            result.message = "재질을 생성하려면 'materialPath'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        string fullPath = command.materialPath;
        string directory = Path.GetDirectoryName(fullPath);
        if (!AssetDatabase.IsValidFolder(directory))
        {
            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }

        Shader targetShader = Shader.Find(command.shader ?? "Standard");
        if (targetShader == null)
        {
            result.success = false;
            result.message = $"셰이더 '{command.shader ?? "Standard"}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        Material newMaterial = new Material(targetShader);
        AssetDatabase.CreateAsset(newMaterial, fullPath);

        if (command.materialProperties != null && command.materialProperties.Count > 0)
        {
            foreach (var prop in command.materialProperties)
            {
                SetMaterialPropertyInternal(newMaterial, prop.Key, prop.Value);
            }
        }
        AssetDatabase.SaveAssets();
        result.success = true;
        result.message = $"재질 생성됨: {fullPath} (셰이더: {targetShader.name})";
        Debug.Log(result.message);
        return result;
    }

    public CommandResult SetMaterialColor(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.materialPath) || command.materialColor == null || command.materialColor.Length < 3)
        {
            result.success = false;
            result.message = "재질 색상을 설정하려면 'materialPath'와 'materialColor'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(command.materialPath);
        if (material != null)
        {
            string colorProperty = command.materialProperty ?? "_Color";
            Color newColor = new Color(command.materialColor[0], command.materialColor[1], command.materialColor[2], command.materialColor.Length == 4 ? command.materialColor[3] : 1f);
            
            SetMaterialPropertyInternal(material, colorProperty, $"{newColor.r},{newColor.g},{newColor.b},{newColor.a}");
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            result.success = true;
            result.message = $"재질 '{command.materialPath}'의 '{colorProperty}' 색상 설정됨: {newColor}";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"재질을 찾을 수 없습니다: {command.materialPath}";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    public CommandResult SetMaterialProperty(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.materialPath) || string.IsNullOrEmpty(command.materialProperty) || command.materialValue == null)
        {
            result.success = false;
            result.message = "재질 속성을 설정하려면 'materialPath', 'materialProperty', 'materialValue'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(command.materialPath);
        if (material != null)
        {
            SetMaterialPropertyInternal(material, command.materialProperty, command.materialValue);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            result.success = true;
            result.message = $"재질 '{command.materialPath}'의 '{command.materialProperty}' 속성 설정됨: {command.materialValue}";
            Debug.Log(result.message);
        }
        else
        {
            result.success = false;
            result.message = $"재질을 찾을 수 없습니다: {command.materialPath}";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    private void SetMaterialPropertyInternal(Material material, string propertyName, string valueString)
    {
        if (!material.HasProperty(propertyName))
        {
            Debug.LogWarning($"재질 '{material.name}'에 속성 '{propertyName}'이 없습니다.");
            return;
        }

        try
        {
            int propIndex = material.shader.FindPropertyIndex(propertyName);
            if (propIndex == -1)
            {
                Debug.LogWarning($"재질 '{material.name}'의 셰이더에 속성 '{propertyName}'이 없습니다.");
                return;
            }
            
            var propType = UnityEditor.ShaderUtil.GetPropertyType(material.shader, propIndex);

            switch(propType)
            {
                case ShaderUtil.ShaderPropertyType.Color:
                    if (ComponentManager.ConvertValue(valueString, typeof(Color)) is Color colorValue)
                        material.SetColor(propertyName, colorValue);
                    break;
                case ShaderUtil.ShaderPropertyType.Vector:
                     if (ComponentManager.ConvertValue(valueString, typeof(Vector4)) is Vector4 vectorValue)
                        material.SetVector(propertyName, vectorValue);
                    break;
                case ShaderUtil.ShaderPropertyType.Float:
                case ShaderUtil.ShaderPropertyType.Range:
                    if (ComponentManager.ConvertValue(valueString, typeof(float)) is float floatValue)
                        material.SetFloat(propertyName, floatValue);
                    break;
                case ShaderUtil.ShaderPropertyType.TexEnv:
                    if (AssetDatabase.LoadAssetAtPath<Texture>(valueString) is Texture textureAsset)
                        material.SetTexture(propertyName, textureAsset);
                    break;
                case ShaderUtil.ShaderPropertyType.Int:
                     if (ComponentManager.ConvertValue(valueString, typeof(int)) is int intValue)
                        material.SetInt(propertyName, intValue);
                    break;
                default:
                    Debug.LogWarning($"재질 '{material.name}'의 속성 '{propertyName}'에 '{valueString}' 값을 설정할 수 없습니다. 지원되지 않는 타입({propType})이거나 파싱 실패.");
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"재질 속성 '{propertyName}' 설정 중 오류 발생: {e.Message}");
        }
    }

    public CommandResult AssignMaterialToRenderer(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.materialPath) || string.IsNullOrEmpty(command.materialTarget))
        {
            result.success = false;
            result.message = "재질을 할당하려면 'materialPath'와 'materialTarget'이 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(command.materialPath);
        if (material == null)
        {
            result.success = false;
            result.message = $"재질을 찾을 수 없습니다: {command.materialPath}";
            Debug.LogWarning(result.message);
            return result;
        }

        var gameObjectManager = new GameObjectManager();
        var findCommand = new GroqUnityCommand { 
            target = command.materialTarget, 
            searchMethod = command.materialSearchMethod,
            parentName = command.parentName,
            ancestorName = command.ancestorName,
            requiredComponent = command.requiredComponent,
            targetIndex = command.targetIndex
        };
        GameObject targetGameObject = gameObjectManager.FindGameObject(findCommand);
        if (targetGameObject != null)
        {
            Renderer renderer = targetGameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                int slot = command.slot >= 0 ? command.slot : 0;
                if (slot < renderer.sharedMaterials.Length)
                {
                    Material[] materials = renderer.sharedMaterials;
                    materials[slot] = material;
                    renderer.sharedMaterials = materials;
                    result.success = true;
                    result.message = $"'{targetGameObject.name}'의 렌더러에 재질 '{command.materialPath}'가 할당됨 (슬롯: {slot}).";
                    Debug.Log(result.message);
                }
                else
                {
                    result.success = false;
                    result.message = $"렌더러에 유효하지 않은 재질 슬롯 인덱스: {slot}";
                    Debug.LogWarning(result.message);
                }
            }
            else
            {
                result.success = false;
                result.message = $"'{targetGameObject.name}'에서 렌더러 컴포넌트를 찾을 수 없습니다.";
                Debug.LogWarning(result.message);
            }
        }
        else
        {
            result.success = false;
            result.message = $"재질을 할당할 GameObject '{command.materialTarget}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
        }
        return result;
    }

    public CommandResult SetRendererColor(GroqUnityCommand command)
    {
        CommandResult result = new CommandResult { commandType = command.commandType, parameters = JsonUtility.ToJson(command) };
        if (string.IsNullOrEmpty(command.materialTarget) || command.materialColor == null || command.materialColor.Length < 3)
        {
            result.success = false;
            result.message = "렌더러 색상을 설정하려면 'materialTarget'과 'materialColor'가 필요합니다.";
            Debug.LogWarning(result.message);
            return result;
        }

        var gameObjectManager = new GameObjectManager();
        var findCommand = new GroqUnityCommand {
            target = command.materialTarget,
            searchMethod = command.materialSearchMethod,
            parentName = command.parentName,
            ancestorName = command.ancestorName,
            requiredComponent = command.requiredComponent,
            targetIndex = command.targetIndex
        };
        GameObject targetGameObject = gameObjectManager.FindGameObject(findCommand);
        if (targetGameObject != null)
        {
            Renderer renderer = targetGameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                string colorProperty = command.materialProperty ?? "_Color";
                Color newColor = new Color(command.materialColor[0], command.materialColor[1], command.materialColor[2], command.materialColor.Length == 4 ? command.materialColor[3] : 1f);

                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                if (renderer.HasPropertyBlock())
                {
                    renderer.GetPropertyBlock(propBlock);
                }
                
                propBlock.SetColor(colorProperty, newColor);
                renderer.SetPropertyBlock(propBlock);

                result.success = true;
                result.message = $"'{targetGameObject.name}'의 렌더러 '{colorProperty}' 색상 설정됨: {newColor}";
                Debug.Log(result.message);
            }
            else
            {
                result.success = false;
                result.message = $"'{targetGameObject.name}'에서 렌더러 컴포넌트를 찾을 수 없습니다.";
                Debug.LogWarning(result.message);
            }
        }
        else
        {
            result.success = false;
            result.message = $"색상을 설정할 GameObject '{command.materialTarget}'를 찾을 수 없습니다.";
            Debug.LogWarning(result.message);
        }
        return result;
    }
}
#endif