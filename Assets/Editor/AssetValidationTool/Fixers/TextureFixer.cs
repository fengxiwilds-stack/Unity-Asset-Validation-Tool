using UnityEditor;

namespace TA.AssetValidation
{
    public static class TextureFixer
    {
        public static bool Fix(AssetValidationResult result, AssetValidationRuleConfig config)
        {
            TextureImporter importer = AssetImporter.GetAtPath(result.assetPath) as TextureImporter;
            if (importer == null)
            {
                return false;
            }

            bool changed = false;

            switch (result.issueType)
            {
                case AssetIssueType.TextureSize:
                    if (importer.maxTextureSize > config.maxTextureSize)
                    {
                        importer.maxTextureSize = config.maxTextureSize;
                        changed = true;
                    }
                    break;

                case AssetIssueType.TextureType:
                    if (importer.textureType != TextureImporterType.NormalMap)
                    {
                        importer.textureType = TextureImporterType.NormalMap;
                        changed = true;
                    }

                    if (importer.sRGBTexture)
                    {
                        importer.sRGBTexture = false;
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
