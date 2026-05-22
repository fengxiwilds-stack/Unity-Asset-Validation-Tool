using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TA.AssetValidation
{
    public class PrefabChecker : IAssetChecker
    {
        public string CheckerName => "Prefab Checker";

        public bool CanCheck(string assetPath, Object asset)
        {
            return asset is GameObject
                   && string.Equals(Path.GetExtension(assetPath), ".prefab", StringComparison.OrdinalIgnoreCase);
        }

        public void Check(
            string assetPath,
            Object asset,
            AssetValidationRuleConfig config,
            List<AssetValidationResult> results)
        {
            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            CheckPath(assetPath, assetName, config, results);

            GameObject prefabRoot = null;

            try
            {
                prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
                if (prefabRoot == null)
                {
                    return;
                }

                if (config.checkPrefabRootNameMatchesFileName)
                {
                    CheckRootName(assetPath, assetName, prefabRoot, results);
                }

                if (config.checkPrefabEmptyMaterialSlots)
                {
                    CheckEmptyMaterialSlots(assetPath, assetName, prefabRoot, results);
                }

                if (config.checkPrefabMissingReferences)
                {
                    CheckMissingScripts(assetPath, assetName, prefabRoot, results);
                    CheckMissingObjectReferences(assetPath, assetName, prefabRoot, results);
                }
            }
            finally
            {
                if (prefabRoot != null)
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
        }

        private static void CheckPath(
            string assetPath,
            string assetName,
            AssetValidationRuleConfig config,
            List<AssetValidationResult> results)
        {
            if (!config.checkAllowedRootFolders)
            {
                return;
            }

            if (IsInAllowedFolder(assetPath, config.allowedRootFolders))
            {
                return;
            }

            results.Add(new AssetValidationResult(
                assetPath,
                assetName,
                "Prefab",
                ValidationSeverity.Warning,
                AssetIssueType.Path,
                "Prefab 资源不在推荐目录中。",
                "移动到配置文件 allowedRootFolders 中指定的目录，例如 Assets/Prefabs。",
                false));
        }

        private static void CheckRootName(
            string assetPath,
            string assetName,
            GameObject prefabRoot,
            List<AssetValidationResult> results)
        {
            if (prefabRoot.name == assetName)
            {
                return;
            }

            results.Add(new AssetValidationResult(
                assetPath,
                assetName,
                "Prefab",
                ValidationSeverity.Warning,
                AssetIssueType.PrefabStructure,
                $"Prefab 根节点名称与文件名不一致：Root = {prefabRoot.name}，File = {assetName}。",
                "建议让 Prefab 根节点名与文件名一致，方便搜索、日志定位和批处理。",
                false));
        }

        private static void CheckEmptyMaterialSlots(
            string assetPath,
            string assetName,
            GameObject prefabRoot,
            List<AssetValidationResult> results)
        {
            Renderer[] renderers = prefabRoot.GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                Material[] materials = renderer.sharedMaterials;

                for (int j = 0; j < materials.Length; j++)
                {
                    if (materials[j] != null)
                    {
                        continue;
                    }

                    results.Add(new AssetValidationResult(
                        assetPath,
                        assetName,
                        "Prefab",
                        ValidationSeverity.Error,
                        AssetIssueType.MissingMaterial,
                        $"Prefab Renderer 存在空材质槽：{GetHierarchyPath(renderer.transform)}，Slot {j}。",
                        "补齐材质槽，避免运行时显示异常或粉色材质问题。",
                        false));
                }
            }
        }

        private static void CheckMissingScripts(
            string assetPath,
            string assetName,
            GameObject prefabRoot,
            List<AssetValidationResult> results)
        {
            Transform[] transforms = prefabRoot.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < transforms.Length; i++)
            {
                GameObject go = transforms[i].gameObject;
                Component[] components = go.GetComponents<Component>();

                for (int j = 0; j < components.Length; j++)
                {
                    if (components[j] != null)
                    {
                        continue;
                    }

                    results.Add(new AssetValidationResult(
                        assetPath,
                        assetName,
                        "Prefab",
                        ValidationSeverity.Error,
                        AssetIssueType.MissingScript,
                        $"Prefab 存在 Missing Script：{GetHierarchyPath(go.transform)}。",
                        "删除丢失脚本组件，或恢复对应脚本文件。",
                        false));
                }
            }
        }

        private static void CheckMissingObjectReferences(
            string assetPath,
            string assetName,
            GameObject prefabRoot,
            List<AssetValidationResult> results)
        {
            Component[] components = prefabRoot.GetComponentsInChildren<Component>(true);

            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null)
                {
                    continue;
                }

                SerializedObject serializedObject = new SerializedObject(component);
                SerializedProperty property = serializedObject.GetIterator();

                bool enterChildren = true;
                while (property.NextVisible(enterChildren))
                {
                    enterChildren = false;

                    if (property.propertyType != SerializedPropertyType.ObjectReference)
                    {
                        continue;
                    }

                    bool referenceMissing =
                        property.objectReferenceValue == null &&
                        property.objectReferenceInstanceIDValue != 0;

                    if (!referenceMissing)
                    {
                        continue;
                    }

                    results.Add(new AssetValidationResult(
                        assetPath,
                        assetName,
                        "Prefab",
                        ValidationSeverity.Error,
                        AssetIssueType.MissingReference,
                        $"Prefab 存在 Missing Reference：{GetHierarchyPath(component.transform)} / {component.GetType().Name}.{property.displayName}。",
                        "重新指定引用对象，或清理无效字段。",
                        false));
                }
            }
        }

        private static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            string path = transform.name;
            Transform current = transform.parent;

            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        private static bool IsInAllowedFolder(string assetPath, string[] allowedFolders)
        {
            if (allowedFolders == null || allowedFolders.Length == 0)
            {
                return true;
            }

            for (int i = 0; i < allowedFolders.Length; i++)
            {
                string folder = allowedFolders[i];
                if (string.IsNullOrWhiteSpace(folder))
                {
                    continue;
                }

                string normalizedFolder = folder.Replace("\\", "/").TrimEnd('/');
                if (assetPath.StartsWith(normalizedFolder + "/", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
