#if UNITY_IOS && UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public static class AddTrackingUsagePlist
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS) return;
    
        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
    
        PlistElementDict rootDict = plist.root;
    
        const string key = "NSUserTrackingUsageDescription";
        // string value = "Ứng dụng sử dụng IDFA để cá nhân hóa quảng cáo và thống kê hiệu quả.";
        string value = "This app uses IDFA to deliver personalized ads and analyze advertising performance.";
        rootDict.SetString(key, value);
        File.WriteAllText(plistPath, plist.WriteToString());
    }
}
#endif