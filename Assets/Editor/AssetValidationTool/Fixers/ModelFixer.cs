using UnityEditor;
using UnityEngine;

namespace TA.AssetValidation
{
    public static class ModelFixer
    {
        public static bool Fix(AssetValidationResult result, AssetValidationRuleConfig config)
        {
            ModelImporter importer = AssetImporter.GetAtPath(result.assetPath) as ModelImporter;
            if (importer == null)
            {
                return false;
            }

            bool changed = false;

            switch (result.issueType)
            {
                case AssetIssueType.ModelScale:
                    if (!Mathf.Approximately(importer.globalScale, 1f))
                    {
                        importer.globalScale = 1f;
                        changed = true;
                    }
                    break;

                case AssetIssueType.ReadWrite:
                    if (importer.isReadable)
                    {
                        importer.isReadable = false;
                        changed = true;
                    }
                    break;
            }

            if (!changed)
            {
                return false;
            }

            importer.SaveAndReimport();
            return true;
        }
    }
}
