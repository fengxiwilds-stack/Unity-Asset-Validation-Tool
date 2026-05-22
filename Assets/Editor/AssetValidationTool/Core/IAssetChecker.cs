using System.Collections.Generic;
using UnityEngine;

namespace TA.AssetValidation
{
    public interface IAssetChecker
    {
        string CheckerName { get; }

        bool CanCheck(string assetPath, Object asset);

        void Check(
            string assetPath,
            Object asset,
            AssetValidationRuleConfig config,
            List<AssetValidationResult> results);
    }
}
