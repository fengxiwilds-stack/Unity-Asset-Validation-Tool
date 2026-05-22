using UnityEngine;

namespace TA.AssetValidation
{
    [CreateAssetMenu(
        fileName = "AssetValidationRuleConfig",
        menuName = "TA Tools/Asset Validation Rule Config")]
    public class AssetValidationRuleConfig : ScriptableObject
    {
        [Header("Naming Rules")]
        public string texturePrefix = "T_";
        public string[] modelPrefixes = { "SM_", "CH_" };
        public string defaultModelPrefixForAutoFix = "SM_";

        [Header("Path Rules")]
        public bool checkAllowedRootFolders = false;
        public string[] allowedRootFolders =
        {
            "Assets/Art",
            "Assets/Models",
            "Assets/Textures",
            "Assets/Prefabs"
        };

        [Header("Texture Rules")]
        public int maxTextureSize = 2048;
        public bool checkNormalMapByName = true;
        public string[] normalTextureKeywords =
        {
            "_N",
            "_Normal",
            "_NRM",
            "_NormalMap"
        };

        [Header("Model Rules")]
        public bool requireModelScaleFactorOne = true;
        public bool requireModelReadWriteDisabled = true;

        [Header("Prefab Rules")]
        public bool checkPrefabRootNameMatchesFileName = true;
        public bool checkPrefabMissingReferences = true;
        public bool checkPrefabEmptyMaterialSlots = true;

        [Header("Report Rules")]
        public bool includePassedInfo = false;
    }
}
