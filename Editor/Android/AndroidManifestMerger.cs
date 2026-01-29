#if UNITY_EDITOR && UNITY_ANDROID

using System.IO;
using System.Xml;
using UnityEditor.Android;

namespace DBD.BaseGame.Editor
{
    public class AndroidManifestMerger : IPostGenerateGradleAndroidProject
    {
        private const string ANDROID_NS = "http://schemas.android.com/apk/res/android";
        public int callbackOrder { get; }

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string manifestPath;
            if (path.Contains("unityLibrary"))
            {
                var pathProject = path.Replace("unityLibrary", "");
                manifestPath = Path.Combine(pathProject, "launcher/src/main/AndroidManifest.xml");
            }
            else if (path.Contains("launcher"))
            {
                manifestPath = Path.Combine(path, "src/main/AndroidManifest.xml");
            }
            else
            {
                manifestPath = Path.Combine(path, "launcher/src/main/AndroidManifest.xml");
            }

            var xml = new XmlDocument();
            xml.Load(manifestPath);
            var manifest = xml.SelectSingleNode("/manifest");
            XmlElement application = manifest.SelectSingleNode("application") as XmlElement;
            application.SetAttribute("allowBackup", ANDROID_NS, "false");
            application.SetAttribute("isGame", ANDROID_NS, "true");
            xml.Save(manifestPath);
        }
    }
}
#endif