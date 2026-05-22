namespace TA.AssetValidation
{
    public static class AssetValidationAutoFixer
    {
        public static bool TryFix(AssetValidationResult result, AssetValidationRuleConfig config)
        {
            if (result == null || config == null || !result.canAutoFix)
            {
                return false;
            }

            switch (result.issueType)
            {
                case AssetIssueType.Naming:
                    return AssetNamingFixer.Fix(result, config);

                case AssetIssueType.TextureSize:
                case AssetIssueType.TextureType:
                    return TextureFixer.Fix(result, config);

                case AssetIssueType.ModelScale:
                case AssetIssueType.ReadWrite:
                    return ModelFixer.Fix(result, config);

                default:
                    return false;
            }
        }
    }
}
