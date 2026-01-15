using UnityEngine;

namespace DBD.BaseGame
{
    using UnityEditor;
    using System.IO;
    using UnityEngine;

    [InitializeOnLoad]
    public static class CopyScriptTemplates
    {
        private const string InstalledKey = "DBD_BaseAds_ScriptTemplatesCopied";

        static CopyScriptTemplates()
        {
            EditorApplication.delayCall += Install;
        }

        private static void Install()
        {
            string sourcePath = "Packages/com.datdb.basegame/ScriptTemplates";

            string targetPath = "Assets/ScriptTemplates";

            if (!Directory.Exists(sourcePath) || Directory.Exists(targetPath))
                return;

            Directory.CreateDirectory(targetPath);

            foreach (var file in Directory.GetFiles(sourcePath, "*.cs.txt"))
            {
                string dest = Path.Combine(targetPath, Path.GetFileName(file));

                if (!File.Exists(dest))
                {
                    File.Copy(file, dest);
                }
            }

            EditorPrefs.SetBool(InstalledKey, true);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "DBD Base Game",
                "Script Templates đã được cài đặt.\nVui lòng restart Unity Editor để sử dụng.",
                "OK");
        }
    }
}