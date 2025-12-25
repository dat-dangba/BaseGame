#if UNITY_IOS && UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using UnityEngine;

public static class InfoPlistMerger
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget != BuildTarget.iOS)
            return;

        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        // Lấy root Info.plist
        PlistElementDict rootDict = plist.root;

        // Đọc file InfoExtra.plist trong Plugins/iOS
        string extraPath = Path.Combine(Application.dataPath, "Plugins/iOS/Info.plist");
        if (File.Exists(extraPath))
        {
            PlistDocument extraPlist = new PlistDocument();
            extraPlist.ReadFromFile(extraPath);

            foreach (var kv in extraPlist.root.values)
            {
                // Merge key-value từ InfoExtra.plist vào Info.plist chính
                rootDict[kv.Key] = kv.Value;
            }
        }

        File.WriteAllText(plistPath, plist.WriteToString());
    }
}
#endif