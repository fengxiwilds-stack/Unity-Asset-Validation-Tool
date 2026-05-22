using System;

namespace TA.AssetValidation
{
    [Serializable]
    public class AssetValidationResult
    {
        public string assetPath;
        public string assetName;
        public string assetType;
        public ValidationSeverity severity;
        public AssetIssueType issueType;
        public string message;
        public string suggestedFix;
        public bool canAutoFix;

        public AssetValidationResult(
            string assetPath,
            string assetName,
            string assetType,
            ValidationSeverity severity,
            AssetIssueType issueType,
            string message,
            string suggestedFix,
            bool canAutoFix)
        {
            this.assetPath = assetPath;
            this.assetName = assetName;
            this.assetType = assetType;
            this.severity = severity;
            this.issueType = issueType;
            this.message = message;
            this.suggestedFix = suggestedFix;
            this.canAutoFix = canAutoFix;
        }
    }
}
