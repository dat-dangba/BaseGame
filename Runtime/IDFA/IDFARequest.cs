using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class IDFARequest : BaseMonoBehaviour
{
    private Action<string> OnReceiveIDFA;

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void RequestTrackingAndGetIDFA(string gameObjectName, string callbackMethod);
#endif

    public void RequestIDFA(Action<string> OnReceiveIDFA)
    {
        this.OnReceiveIDFA = OnReceiveIDFA;
#if UNITY_IOS && !UNITY_EDITOR
        RequestTrackingAndGetIDFA(gameObject.name, "ReceiveIDFA");
#endif
    }

    public void ReceiveIDFA(string idfa)
    {
        OnReceiveIDFA?.Invoke(idfa);
    }
}