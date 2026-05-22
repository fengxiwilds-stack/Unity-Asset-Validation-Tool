using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TA.AssetValidation
{
    public static class ValidationReportExporter
    {
        [Serializable]
        private class ValidationReportWrapper
        {
            public string exportedAt;
            public int totalCount;
            public int errorCount;
            public int warningCount;
            public int infoCount;
            public List<AssetValidationResult> results;
        }

        public static void ExportCsv(List<AssetValidationResult> results)
        {
            if (results == null || results.Count == 0)
            {
                EditorUtility.DisplayDialog("Export CSV", "没有可导出的检查结果。", "OK");
                return;
            }

            string path = EditorUtility.SaveFilePanel(
                "Export Asset Validation CSV",
                Application.dataPath,
                "AssetValidationReport.csv",
                "csv");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Severity,IssueType,AssetType,AssetName,AssetPath,Message,SuggestedFix,CanAutoFix");

            for (int i = 0; i < results.Count; i++)
            {
                AssetValidationResult result = results[i];
                builder.Append(EscapeCsv(result.severity.ToString())).Append(',');
                builder.Append(EscapeCsv(result.issueType.ToString())).Append(',');
                builder.Append(EscapeCsv(result.assetType)).Append(',');
                builder.Append(EscapeCsv(result.assetName)).Append(',');
                builder.Append(EscapeCsv(result.assetPath)).Append(',');
                builder.Append(EscapeCsv(result.message)).Append(',');
                builder.Append(EscapeCsv(result.suggestedFix)).Append(',');
                builder.Append(EscapeCsv(result.canAutoFix.ToString()));
                builder.AppendLine();
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();
            EditorUtility.RevealInFinder(path);
        }

        public static void ExportJson(List<AssetValidationResult> results)
        {
            if (results == null || results.Count == 0)
            {
                EditorUtility.DisplayDialog("Export JSON", "没有可导出的检查结果。", "OK");
                return;
            }

            string path = EditorUtility.SaveFilePanel(
                "Export Asset Validation JSON",
                Application.dataPath,
                "AssetValidationReport.json",
                "json");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            ValidationReportWrapper wrapper = new ValidationReportWrapper
            {
                exportedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                totalCount = results.Count,
                errorCount = CountBySeverity(results, ValidationSeverity.Error),
                warningCount = CountBySeverity(results, ValidationSeverity.Warning),
                infoCount = CountBySeverity(results, ValidationSeverity.Info),
                results = results
            };

            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(path, json, Encoding.UTF8);
            AssetDatabase.Refresh();
            EditorUtility.RevealInFinder(path);
        }

        private static int CountBySeverity(List<AssetValidationResult> results, ValidationSeverity severity)
        {
            int count = 0;
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].severity == severity)
                {
                    count++;
                }
            }

            return count;
        }

        private static string EscapeCsv(string value)
        {
            if (value == null)
            {
                return "";
            }

            bool mustQuote = value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r");
            string escaped = value.Replace("\"", "\"\"");

            return mustQuote ? $"\"{escaped}\"" : escaped;
        }
    }
}
