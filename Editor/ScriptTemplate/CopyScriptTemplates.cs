using UnityEditor;
using System.IO;

namespace DBD.BaseGame.Editor
{
    [InitializeOnLoad]
    public static class CopyScriptTemplates
    {
        private const string InstalledKey = "DBD_BaseAds_ScriptTemplatesCopied";

        static CopyScriptTemplates()
        {
            EditorApplication.delayCall += Copy;
        }

        private static void Copy()
        {
            string sourcePath = "Packages/com.datdb.basegame/ScriptTemplates~";

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

        [MenuItem("Tools/ScriptTemplate/Generate")]
        private static void CreateFitter(MenuCommand command)
        {
            Copy();
        }
    }
}