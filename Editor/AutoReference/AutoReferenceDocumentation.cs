using System.IO;
using UnityEditor;
using UnityEngine;

namespace Teo.AutoReference.Editor {
    internal static class AutoReferenceDocumentation {
        private const string RelativePath = "Documentation~/index.html";

        private static string _docPath;

        private static string DocPath {
            get {
                if (_docPath != null) {
                    return _docPath;
                }

                var packageInfo =
                    UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(AutoReference).Assembly);
                if (packageInfo != null) {
                    _docPath = Path.Combine(packageInfo.resolvedPath, RelativePath);
                }
                return _docPath;
            }
        }

        public static void Open() {
            var path = DocPath;
            if (path == null || !File.Exists(path)) {
                return;
            }
            EditorUtility.OpenWithDefaultApp(path);
        }
    }
}
