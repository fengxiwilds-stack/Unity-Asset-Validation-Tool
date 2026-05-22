using UnityEditor;
using UnityEngine;

namespace TA.AssetValidation
{
    public static class AssetValidationConfigCreator
    {
        private const string RootFolder = "Assets/Editor/AssetValidationTool";
        private const string ConfigFolder = RootFolder + "/Config";
        private const string DefaultConfigPath = ConfigFolder + "/DefaultAssetValidationRuleConfig.asset";

        [MenuItem("TA Tools/Asset Validation/Create Default Rule Config")]
        public static AssetValidationRuleConfig CreateDefaultConfigByMenu()
        {
            AssetValidationRuleConfig config = GetOrCreateDefaultConfig();
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
            return config;
        }

        public static AssetValidationRuleConfig GetOrCreateDefaultConfig()
        {
            EnsureFolder("Assets", "Editor");
            EnsureFolder("Assets/Editor", "AssetValidationTool");
            EnsureFolder(RootFolder, "Config");

            AssetValidationRuleConfig config =
                AssetDatabase.LoadAssetAtPath<AssetValidationRuleConfig>(DefaultConfigPath);

            if (config != null)
            {
                return config;
            }

            config = ScriptableObject.CreateInstance<AssetValidationRuleConfig>();
            AssetDatabase.CreateAsset(config, DefaultConfigPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return config;
        }

        private static void EnsureFolder(string parent, string child)
        {
            string fullPath = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
