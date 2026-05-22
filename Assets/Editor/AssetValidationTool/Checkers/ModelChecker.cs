using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TA.AssetValidation
{
    public class ModelChecker : IAssetChecker
    {
        public string CheckerName => "Model Checker";

        private static readonly string[] ModelExtensions =
        {
            ".fbx",
            ".obj",
            ".dae",
            ".blend",
            ".3ds"
        };

        public bool CanCheck(string assetPath, Object asset)
        {
            return IsModelPath(assetPath) && AssetImporter.GetAtPath(assetPath) is ModelImporter;
        }

        public void Check(
            string assetPath,
            Object asset,
            AssetValidationRuleConfig config,
            List<AssetValidationResult> results)
        {
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
            {
                return;
            }

            string assetName = Path.GetFileNameWithoutExtension(assetPath);

            CheckPath(assetPath, assetName, config, results);
            CheckNaming(assetPath, assetName, config, results);
            CheckScaleFactor(assetPath, assetName, importer, config, results);
            CheckReadWrite(assetPath, assetName, importer, config, results);
            CheckEmptyMaterialSlots(assetPath, assetName, asset as GameObject, results);
        }

        private static bool IsModelPath(string assetPath)
        {
            string ext = Path.GetExtension(assetPath);
            for (int i = 0; i < ModelExtensions.Length; i++)
            {
                if (string.Equals(ext, ModelExtensions[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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
                "Model",
                ValidationSeverity.Warning,
                AssetIssueType.Path,
                "模型资源不在推荐目录中。",
                "移动到配置文件 allowedRootFolders 中指定的目录，例如 Assets/Models。",
                false));
        }

        private static void CheckNaming(
            string assetPath,
            string assetName,
            AssetValidationRuleConfig config,
            List<AssetValidationResult> results)
        {
            if (StartsWithAny(assetName, config.modelPrefixes))
            {
                return;
            }

            string prefixText = config.modelPrefixes == null
                ? "SM_ / CH_"
                : string.Join(" / ", config.modelPrefixes);

            results.Add(new AssetValidationResult(
                assetPath,
                assetName,
                "Model",
                ValidationSeverity.Warning,
                AssetIssueType.Naming,
                $"模型命名不符合规则：应以 {prefixText} 开头。",
                $"自动修复会使用默认模型前缀 {config.defaultModelPrefixForAutoFix}，也可手动改成 SM_ / CH_。",
                true));
        }

        private static void CheckScaleFactor(
            string assetPath,
            string assetName,
            ModelImporter importer,
            AssetValidationRuleConfig config,
            List<AssetValidationResult> results)
        {
            if (!config.requireModelScaleFactorOne)
            {
                return;
            }

            if (Mathf.Approximately(importer.globalScale, 1f))
            {
                return;
            }

            results.Add(new AssetValidationResult(
                assetPath,
                assetName,
                "Model",
                ValidationSeverity.Error,
                AssetIssueType.ModelScale,
                $"模型 Scale Factor 不是 1：当前 {importer.globalScale}。",
                "将 Model Importer 的 Scale Factor 设置为 1。",
                true));
        }

        private static void CheckReadWrite(
            string assetPath,
            string assetName,
            ModelImporter importer,
            AssetValidationRuleConfig config,
            List<AssetValidationResult> results)
        {
            if (!config.requireModelReadWriteDisabled)
            {
                return;
            }

            if (!importer.isReadable)
            {
                return;
            }

            results.Add(new AssetValidationResult(
                assetPath,
                assetName,
                "Model",
                ValidationSeverity.Warning,
                AssetIssueType.ReadWrite,
                "模型 Read/Write Enabled 被开启，可能造成不必要的内存开销。",
                "如果没有运行时读写 Mesh 的需求，关闭 Read/Write Enabled。",
                true));
        }

        private static void CheckEmptyMaterialSlots(
            string assetPath,
            string assetName,
            GameObject modelRoot,
            List<AssetValidationResult> results)
        {
            if (modelRoot == null)
            {
                return;
            }

            Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
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
                        "Model",
                        ValidationSeverity.Error,
                        AssetIssueType.MissingMaterial,
                        $"模型 Renderer 存在空材质槽：{renderer.name}，Slot {j}。",
                        "在 DCC 或 Unity 中补齐对应材质，避免渲染异常。",
                        false));
                }
            }
        }

        private static bool StartsWithAny(string value, string[] prefixes)
        {
            if (prefixes == null || prefixes.Length == 0)
            {
                return true;
            }

            for (int i = 0; i < prefixes.Length; i++)
            {
                string prefix = prefixes[i];
                if (string.IsNullOrWhiteSpace(prefix))
                {
                    continue;
                }

                if (value.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
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
