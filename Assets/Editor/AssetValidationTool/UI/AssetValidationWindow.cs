using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TA.AssetValidation
{
    public class AssetValidationWindow : EditorWindow
    {
        private enum ScanScope
        {
            SelectedAssets,
            WholeProject
        }

        private AssetValidationRuleConfig _config;
        private AssetValidationRunner _runner;
        private List<AssetValidationResult> _results = new List<AssetValidationResult>();
        private Vector2 _scrollPosition;
        private ScanScope _scanScope = ScanScope.SelectedAssets;
        private string _searchKeyword = string.Empty;
        private ValidationSeverity _severityFilter = ValidationSeverity.Warning;
        private bool _showInfo = false;

        [MenuItem("TA Tools/Asset Validation/Open Tool")]
        public static void Open()
        {
            AssetValidationWindow window = GetWindow<AssetValidationWindow>();
            window.titleContent = new GUIContent("Asset Validation");
            window.minSize = new Vector2(860f, 520f);
            window.Show();
        }

        private void OnEnable()
        {
            _runner = new AssetValidationRunner();
            _config = AssetValidationConfigCreator.GetOrCreateDefaultConfig();
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawSummary();
            DrawFilters();
            DrawResultList();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Asset Validation Tool", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                _config = (AssetValidationRuleConfig)EditorGUILayout.ObjectField(
                    "Rule Config",
                    _config,
                    typeof(AssetValidationRuleConfig),
                    false);

                using (new EditorGUILayout.HorizontalScope())
                {
                    _scanScope = (ScanScope)EditorGUILayout.EnumPopup("Scan Scope", _scanScope);

                    if (GUILayout.Button("Create / Locate Default Config", GUILayout.Width(210f)))
                    {
                        _config = AssetValidationConfigCreator.GetOrCreateDefaultConfig();
                        Selection.activeObject = _config;
                        EditorGUIUtility.PingObject(_config);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Scan", GUILayout.Height(28f)))
                    {
                        Scan();
                    }

                    EditorGUI.BeginDisabledGroup(_results == null || _results.Count == 0);

                    if (GUILayout.Button("Auto Fix All Fixable", GUILayout.Height(28f)))
                    {
                        AutoFixAll();
                    }

                    if (GUILayout.Button("Export CSV", GUILayout.Height(28f)))
                    {
                        ValidationReportExporter.ExportCsv(_results);
                    }

                    if (GUILayout.Button("Export JSON", GUILayout.Height(28f)))
                    {
                        ValidationReportExporter.ExportJson(_results);
                    }

                    EditorGUI.EndDisabledGroup();
                }
            }
        }

        private void DrawSummary()
        {
            int errorCount = CountBySeverity(ValidationSeverity.Error);
            int warningCount = CountBySeverity(ValidationSeverity.Warning);
            int infoCount = CountBySeverity(ValidationSeverity.Info);
            int fixableCount = CountFixable();

            using (new EditorGUILayout.HorizontalScope("box"))
            {
                DrawSummaryItem("Total", _results.Count.ToString());
                DrawSummaryItem("Errors", errorCount.ToString());
                DrawSummaryItem("Warnings", warningCount.ToString());
                DrawSummaryItem("Info", infoCount.ToString());
                DrawSummaryItem("Fixable", fixableCount.ToString());
            }
        }

        private void DrawSummaryItem(string label, string value)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(120f)))
            {
                EditorGUILayout.LabelField(label, EditorStyles.miniLabel);
                EditorGUILayout.LabelField(value, EditorStyles.boldLabel);
            }
        }

        private void DrawFilters()
        {
            using (new EditorGUILayout.HorizontalScope("box"))
            {
                _searchKeyword = EditorGUILayout.TextField("Search", _searchKeyword);
                _severityFilter = (ValidationSeverity)EditorGUILayout.EnumPopup("Min Severity", _severityFilter);
                _showInfo = EditorGUILayout.ToggleLeft("Show Info", _showInfo, GUILayout.Width(90f));
            }
        }

        private void DrawResultList()
        {
            if (_results == null || _results.Count == 0)
            {
                EditorGUILayout.HelpBox("暂无结果。选择资源后点击 Scan，或将 Scan Scope 切到 Whole Project。", MessageType.Info);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            for (int i = 0; i < _results.Count; i++)
            {
                AssetValidationResult result = _results[i];
                if (!ShouldShowResult(result))
                {
                    continue;
                }

                DrawResultItem(result);
            }

            EditorGUILayout.EndScrollView();
        }

        private bool ShouldShowResult(AssetValidationResult result)
        {
            if (result.severity == ValidationSeverity.Info && !_showInfo)
            {
                return false;
            }

            if (result.severity < _severityFilter && result.severity != ValidationSeverity.Info)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(_searchKeyword))
            {
                return true;
            }

            string keyword = _searchKeyword.ToLowerInvariant();
            return Contains(result.assetPath, keyword)
                   || Contains(result.assetName, keyword)
                   || Contains(result.assetType, keyword)
                   || Contains(result.message, keyword)
                   || Contains(result.suggestedFix, keyword)
                   || result.issueType.ToString().ToLowerInvariant().Contains(keyword)
                   || result.severity.ToString().ToLowerInvariant().Contains(keyword);
        }

        private static bool Contains(string source, string keyword)
        {
            return !string.IsNullOrEmpty(source) && source.ToLowerInvariant().Contains(keyword);
        }

        private void DrawResultItem(AssetValidationResult result)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUIStyle severityStyle = new GUIStyle(EditorStyles.boldLabel);
                    severityStyle.normal.textColor = GetSeverityColor(result.severity);

                    EditorGUILayout.LabelField(result.severity.ToString(), severityStyle, GUILayout.Width(70f));
                    EditorGUILayout.LabelField(result.issueType.ToString(), GUILayout.Width(130f));
                    EditorGUILayout.LabelField(result.assetType, GUILayout.Width(80f));
                    EditorGUILayout.LabelField(result.assetName, EditorStyles.boldLabel);

                    if (GUILayout.Button("Ping", GUILayout.Width(52f)))
                    {
                        PingAsset(result.assetPath);
                    }

                    EditorGUI.BeginDisabledGroup(!result.canAutoFix);
                    if (GUILayout.Button("Fix", GUILayout.Width(52f)))
                    {
                        AutoFixOne(result);
                    }
                    EditorGUI.EndDisabledGroup();
                }

                EditorGUILayout.LabelField("Path", result.assetPath);
                EditorGUILayout.LabelField("Message", result.message);

                if (!string.IsNullOrEmpty(result.suggestedFix))
                {
                    EditorGUILayout.LabelField("Suggested Fix", result.suggestedFix);
                }
            }
        }

        private Color GetSeverityColor(ValidationSeverity severity)
        {
            switch (severity)
            {
                case ValidationSeverity.Error:
                    return new Color(1f, 0.25f, 0.25f);
                case ValidationSeverity.Warning:
                    return new Color(1f, 0.68f, 0.15f);
                default:
                    return new Color(0.55f, 0.75f, 1f);
            }
        }

        private void Scan()
        {
            if (_config == null)
            {
                _config = AssetValidationConfigCreator.GetOrCreateDefaultConfig();
            }

            switch (_scanScope)
            {
                case ScanScope.SelectedAssets:
                    _results = _runner.ScanSelection(_config);
                    break;

                case ScanScope.WholeProject:
                    _results = _runner.ScanWholeProject(_config);
                    break;
            }

            Repaint();
        }

        private void AutoFixOne(AssetValidationResult result)
        {
            bool fixedResult = AssetValidationAutoFixer.TryFix(result, _config);
            if (!fixedResult)
            {
                EditorUtility.DisplayDialog("Auto Fix", "该问题未被修复。可能已经无需修复，或规则不支持自动修复。", "OK");
                return;
            }

            Scan();
        }

        private void AutoFixAll()
        {
            if (_results == null || _results.Count == 0)
            {
                return;
            }

            List<AssetValidationResult> snapshot = new List<AssetValidationResult>(_results);

            int fixedCount = 0;

            // 先修复导入设置，再修复命名。
            // 原因：命名修复会改变 assetPath，如果先重命名，后续同一资源的 Importer 修复可能找不到旧路径。
            fixedCount += FixResults(snapshot, false);
            AssetDatabase.Refresh();

            fixedCount += FixResults(snapshot, true);
            AssetDatabase.Refresh();

            Scan();
            EditorUtility.DisplayDialog("Auto Fix", $"已修复 {fixedCount} 个问题。", "OK");
        }

        private int FixResults(List<AssetValidationResult> snapshot, bool fixNamingIssues)
        {
            int fixedCount = 0;

            for (int i = 0; i < snapshot.Count; i++)
            {
                AssetValidationResult result = snapshot[i];
                if (!result.canAutoFix)
                {
                    continue;
                }

                bool isNamingIssue = result.issueType == AssetIssueType.Naming;
                if (isNamingIssue != fixNamingIssues)
                {
                    continue;
                }

                if (AssetValidationAutoFixer.TryFix(result, _config))
                {
                    fixedCount++;
                }
            }

            return fixedCount;
        }

        private void PingAsset(string assetPath)
        {
            Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (asset == null)
            {
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private int CountBySeverity(ValidationSeverity severity)
        {
            if (_results == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < _results.Count; i++)
            {
                if (_results[i].severity == severity)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountFixable()
        {
            if (_results == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < _results.Count; i++)
            {
                if (_results[i].canAutoFix)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
