using System;
using System.IO;
using UnityEditor;

namespace TA.AssetValidation
{
    public static class AssetNamingFixer
    {
        public static bool Fix(AssetValidationResult result, AssetValidationRuleConfig config)
        {
            if (result == null || config == null)
            {
                return false;
            }

            if (result.issueType != AssetIssueType.Naming)
            {
                return false;
            }

            string desiredPrefix = GetDesiredPrefix(result, config);
            if (string.IsNullOrWhiteSpace(desiredPrefix))
            {
                return false;
            }

            string assetPath = result.assetPath;
            string currentName = Path.GetFileNameWithoutExtension(assetPath);

            if (string.IsNullOrWhiteSpace(currentName))
            {
                return false;
            }

            if (currentName.StartsWith(desiredPrefix, StringComparison.Ordinal))
            {
                return false;
            }

            string targetName = desiredPrefix + currentName;
            string uniqueTargetName = GenerateUniqueAssetName(assetPath, targetName);

            string renameError = AssetDatabase.RenameAsset(assetPath, uniqueTargetName);
            if (!string.IsNullOrEmpty(renameError))
            {
                UnityEngine.Debug.LogError($"Rename asset failed: {assetPath} -> {uniqueTargetName}. Error: {renameError}");
                return false;
            }

            AssetDatabase.SaveAssets();
            return true;
        }

        private static string GetDesiredPrefix(AssetValidationResult result, AssetValidationRuleConfig config)
        {
            switch (result.assetType)
            {
                case "Texture":
                    return config.texturePrefix;

                case "Model":
                    if (!string.IsNullOrWhiteSpace(config.defaultModelPrefixForAutoFix))
                    {
                        return config.defaultModelPrefixForAutoFix;
                    }

                    if (config.modelPrefixes != null && config.modelPrefixes.Length > 0)
                    {
                        return config.modelPrefixes[0];
                    }

                    return "SM_";

                default:
                    return string.Empty;
            }
        }

        private static string GenerateUniqueAssetName(string assetPath, string targetName)
        {
            string directory = Path.GetDirectoryName(assetPath);
            string extension = Path.GetExtension(assetPath);

            if (string.IsNullOrEmpty(directory))
            {
                return targetName;
            }

            string candidateName = targetName;
            string candidatePath = ToUnityPath(Path.Combine(directory, candidateName + extension));

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(candidatePath) == null)
            {
                return candidateName;
            }

            for (int i = 1; i < 1000; i++)
            {
                candidateName = $"{targetName}_{i:000}";
                candidatePath = ToUnityPath(Path.Combine(directory, candidateName + extension));

                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(candidatePath) == null)
                {
                    return candidateName;
                }
            }

            return targetName + "_Renamed";
        }

        private static string ToUnityPath(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
