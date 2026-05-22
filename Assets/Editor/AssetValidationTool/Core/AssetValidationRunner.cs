using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TA.AssetValidation
{
    public class AssetValidationRunner
    {
        private readonly List<IAssetChecker> _checkers = new List<IAssetChecker>
        {
            new TextureChecker(),
            new ModelChecker(),
            new MaterialChecker(),
            new PrefabChecker()
        };

        public List<AssetValidationResult> ScanPaths(
            IEnumerable<string> inputPaths,
            AssetValidationRuleConfig config)
        {
            List<AssetValidationResult> results = new List<AssetValidationResult>();
            if (config == null)
            {
                Debug.LogError("Asset Validation config is null.");
                return results;
            }

            List<string> assetPaths = ExpandAssetPaths(inputPaths);

            try
            {
                AssetDatabase.StartAssetEditing();

                for (int i = 0; i < assetPaths.Count; i++)
                {
                    string assetPath = assetPaths[i];
                    Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                    if (asset == null)
                    {
                        continue;
                    }

                    int resultCountBefore = results.Count;

                    for (int j = 0; j < _checkers.Count; j++)
                    {
                        IAssetChecker checker = _checkers[j];
                        if (!checker.CanCheck(assetPath, asset))
                        {
                            continue;
                        }

                        checker.Check(assetPath, asset, config, results);
                    }

                    if (config.includePassedInfo && resultCountBefore == results.Count)
                    {
                        results.Add(new AssetValidationResult(
                            assetPath,
                            asset.name,
                            asset.GetType().Name,
                            ValidationSeverity.Info,
                            AssetIssueType.None,
                            "未发现问题。",
                            string.Empty,
                            false));
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            return results;
        }

        public List<AssetValidationResult> ScanSelection(AssetValidationRuleConfig config)
        {
            List<string> paths = new List<string>();
            string[] guids = Selection.assetGUIDs;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!string.IsNullOrEmpty(path))
                {
                    paths.Add(path);
                }
            }

            return ScanPaths(paths, config);
        }

        public List<AssetValidationResult> ScanWholeProject(AssetValidationRuleConfig config)
        {
            return ScanPaths(new[] { "Assets" }, config);
        }

        private static List<string> ExpandAssetPaths(IEnumerable<string> inputPaths)
        {
            HashSet<string> uniquePaths = new HashSet<string>();

            if (inputPaths == null)
            {
                return new List<string>();
            }

            foreach (string inputPath in inputPaths)
            {
                if (string.IsNullOrWhiteSpace(inputPath))
                {
                    continue;
                }

                string normalizedPath = inputPath.Replace("\\", "/");

                if (AssetDatabase.IsValidFolder(normalizedPath))
                {
                    string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { normalizedPath });
                    for (int i = 0; i < guids.Length; i++)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                        if (!string.IsNullOrEmpty(path) && !AssetDatabase.IsValidFolder(path))
                        {
                            uniquePaths.Add(path);
                        }
                    }
                }
                else
                {
                    uniquePaths.Add(normalizedPath);
                }
            }

            return new List<string>(uniquePaths);
        }
    }
}
