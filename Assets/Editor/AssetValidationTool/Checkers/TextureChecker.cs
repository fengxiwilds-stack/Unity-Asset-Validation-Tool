using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TA.AssetValidation
{
    public class TextureChecker : IAssetChecker
    {
        public string CheckerName => "Texture Checker";

        public bool CanCheck(string assetPath, Object asset)
        {
            return asset is Texture2D && AssetImporter.GetAtPath(assetPath) is TextureImporter;
        }

        public void Check(
            string assetPath,
            Object asset,
            AssetValidationRuleConfig config,
            List<AssetValidationResult> results)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            string assetName = Path.GetFileNameWithoutExtension(assetPath);

            CheckPath(assetPath, assetName, config, results);
            CheckNaming(assetPath, assetName, config, results);
            CheckMaxSize(assetPath, assetName, importer, config, results);
            CheckNormalMapType(assetPath, assetName, importer, config, results);
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
                "Texture",
                ValidationSeverity.Warning,
                AssetIssueType.Path,
                "贴图资源不在推荐目录中。",
                "移动到配置文件 allowedRootFolders 中指定的目录，例如 Assets/Textures。",
                false));
        }

        private static void CheckNaming(
            string assetPath,
            string assetName,
            AssetValidationRuleConfig config,
            List<AssetValidationResult> results)
        {
            if (assetName.StartsWith(config.texturePrefix, StringComparison.Ordinal))
            {
                return;
            }

            results.Add(new AssetValidationResult(
                assetPath,
                assetName,
                "Texture",
                ValidationSeverity.Warning,
                AssetIssueType.Naming,
                $"贴图命名不符合规则：应以 {config.texturePrefix} 开头。",
                $"自动修复会将资源重命名为 {config.texturePrefix}{assetName}。",
                true));
        }

        private static void CheckMaxSize(
            string assetPath,
            string assetName,
            TextureImporter importer,
            AssetValidationRuleConfig config,
            List<AssetValidationResult> results)
        {
            if (importer.maxTextureSize <= config.maxTextureSize)
            {
                return;
            }

            results.Add(new AssetValidationResult(
                assetPath,
                assetName,
                "Texture",
                ValidationSeverity.Warning,
                AssetIssueType.TextureSize,
                $"贴图 Max Size 过大：当前 {importer.maxTextureSize}，规则上限 {config.maxTextureSize}。",
                $"将 Max Size 设置为 {config.maxTextureSize}。",
                true));
        }

        private static void CheckNormalMapType(
            string assetPath,
            string assetName,
            TextureImporter importer,
            AssetValidationRuleConfig config,
            List<AssetValidationResult> results)
        {
            if (!config.checkNormalMapByName)
            {
                return;
            }

            if (!LooksLikeNormalMap(assetName, config.normalTextureKeywords))
            {
                return;
            }

            if (importer.textureType == TextureImporterType.NormalMap)
            {
                return;
            }

            results.Add(new AssetValidationResult(
                assetPath,
                assetName,
                "Texture",
                ValidationSeverity.Error,
                AssetIssueType.TextureType,
                "疑似 Normal 贴图，但 Texture Type 不是 Normal Map。",
                "将 Texture Type 设置为 Normal Map，并关闭 sRGB。",
                true));
        }

        private static bool LooksLikeNormalMap(string assetName, string[] keywords)
        {
            if (keywords == null)
            {
                return false;
            }

            for (int i = 0; i < keywords.Length; i++)
            {
                string keyword = keywords[i];
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    continue;
                }

                if (assetName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
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
