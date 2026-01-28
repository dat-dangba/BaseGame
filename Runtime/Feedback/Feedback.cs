using UnityEngine;

namespace DBD.BaseGame
{
    public class Feedback
    {
        public static void SendMail(string emailFeedback)
        {
            string subject = EscapeString($"{Application.productName} Feedback {Application.version}");
            string mailto = "mailto:" + emailFeedback + "?subject=" + subject;
            Application.OpenURL(mailto);
        }

        private static string EscapeString(string s)
        {
            return UnityEngine.Networking.UnityWebRequest.EscapeURL(s).Replace("+", "%20");
        }
    }
}