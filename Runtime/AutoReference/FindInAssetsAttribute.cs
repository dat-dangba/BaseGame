// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teo.AutoReference.Internals.Collections;
using Teo.AutoReference.System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Teo.AutoReference {
    /// <summary>
    /// Gets a reference to an asset.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class FindInAssetsAttribute : AutoReferenceAttribute {
        private readonly string[] _searchInFoldersParams;
        private string _searchString;

        public FindInAssetsAttribute() {
            _searchInFoldersParams = Array.Empty<string>();
        }

        public FindInAssetsAttribute(params string[] searchInFolders) {
            _searchInFoldersParams = searchInFolders;
        }

        /// <summary>
        /// Include these folders for searching.
        /// </summary>
        public string[] SearchInFolders { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Find an asset with at least one of these labels. This is combined with <see cref="Label"/>.
        /// </summary>
        public string[] Labels { get; set; } = Array.Empty<string>();

        /// Find an asset with a specific label. This is combined with <see cref="Labels"/>.
        public string Label { get; set; } = "";

        /// Find an asset that belongs in a specific AssetBundle.
        public string Bundle { get; set; } = "";

        private static string Format(char prefix, string param) {
            return string.IsNullOrWhiteSpace(param) ? string.Empty : $"{prefix}:{param}";
        }

        private static string Format(char prefix, IEnumerable<string> param) {
            return string.Join(" ", param.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => Format(prefix, p)));
        }

        protected override ValidationResult OnInitialize() {
            if (!string.IsNullOrWhiteSpace(Label)) {
                Labels = Labels.PrependArray(Label);
            }

            Label = null;

            if (Labels.Length > 1) {
                Labels = Labels.Distinct().ToArray();
            }

            SearchInFolders = _searchInFoldersParams.Concat(SearchInFolders).ToArray();

            _searchString = $"{Format('t', Type.Name)} {Format('l', Labels)} {Format('b', Bundle)}";

            return ValidationResult.Ok;
        }

        protected override IEnumerable<Object> GetObjects() {
#if UNITY_EDITOR
            var guids = AssetDatabase.FindAssets(_searchString, SearchInFolders);

            return guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(assetPath => AssetDatabase.LoadAssetAtPath(assetPath, Type))
                .Where(asset => asset != null);
#else
            return Array.Empty<Object>();
#endif
        }

        protected override IEnumerable<Object> ValidateObjects(IEnumerable<Object> objects) {
            return objects.Where(Validate);
        }

        private bool Validate(Object value) {
#if UNITY_EDITOR
            if (Labels.Length > 0) {
                if (!AssetDatabase.GetLabels(value).Intersect(Labels).Any()) {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(Bundle)) {
                var assetPath = AssetDatabase.GetAssetPath(value);
                var importer = AssetImporter.GetAtPath(assetPath);

                if (importer == null) {
                    return false;
                }

                if (Bundle != importer.assetBundleName) {
                    return false;
                }
            }

            if (SearchInFolders.Length > 0) {
                var comparisonType = Application.platform is RuntimePlatform.WindowsEditor
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;

                var assetPath = AssetDatabase.GetAssetPath(value);
                return SearchInFolders.Any(folder => assetPath.StartsWith(folder, comparisonType));
            }

#endif
            return true;
        }
    }
}
