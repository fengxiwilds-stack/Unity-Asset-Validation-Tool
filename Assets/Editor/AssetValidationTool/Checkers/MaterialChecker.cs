using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TA.AssetValidation
{
    public class MaterialChecker : IAssetChecker
    {
        public string CheckerName => "Material Checker";

        public bool CanCheck(string assetPath, Object asset)
        {
            return asset is Material;
        }

        public void Check(
            string assetPath,
            Object asset,
            AssetValidationRuleConfig config,
            List<AssetValidationResult> results)
        {
            Material material = asset as Material;
            if (material == null)
            {
                return;
            }

            string assetName = Path.GetFileNameWithoutExtension(assetPath);

            CheckPath(assetPath, assetName, config, results);
            CheckShader(assetPath, assetName, material, results);
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
                "Material",
                ValidationSeverity.Warning,
                AssetIssueType.Path,
                "材质资源不在推荐目录中。",
                "移动到配置文件 allowedRootFolders 中指定的目录。",
                false));
        }

        private static void CheckShader(
            string assetPath,
            string assetName,
            Material material,
            List<AssetValidationResult> results)
        {
            if (material.shader != null && material.shader.name != "Hidden/InternalErrorShader")
            {
                return;
            }

            results.Add(new AssetValidationResult(
                assetPath,
                assetName,
                "Material",
                ValidationSeverity.Error,
                AssetIssueType.ShaderMissing,
                "材质 Shader 缺失或处于错误 Shader。",
                "重新指定正确 Shader，或确认渲染管线是否切换导致材质失效。",
                false));
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
