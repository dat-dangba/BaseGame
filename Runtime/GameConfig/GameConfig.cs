using System;
using UnityEngine;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

public class GameConfig : Singleton<GameConfig>
{
    protected AndroidJavaObject metaData;

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern IntPtr GetInfoPlistValue(string key);
#endif

    public int GetConfig(string key, int defaultValue)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            metaData ??= GetMetaData();
            try
            {
                if (ConstainKey(key))
                {
                    return metaData.Call<int>("getInt", key);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"datdb - {e}");
            }
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            string value = GetPlistValue(key);
            if (value != null)
            {
                return int.Parse(value);
            }
        }

        return defaultValue;
    }

    public float GetConfig(string key, float defaultValue)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            metaData ??= GetMetaData();
            try
            {
                if (ConstainKey(key))
                {
                    return metaData.Call<float>("getFloat", key);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"datdb - {e}");
            }
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            string value = GetPlistValue(key);
            if (value != null)
            {
                return float.Parse(value);
            }
        }

        return defaultValue;
    }

    public string GetConfig(string key, string defaultValue)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            metaData ??= GetMetaData();
            try
            {
                if (ConstainKey(key))
                {
                    return metaData.Call<string>("getString", key);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"datdb - {e}");
            }
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            string value = GetPlistValue(key);
            if (value != null)
            {
                return value;
            }
        }

        return defaultValue;
    }

    public bool GetConfig(string key, bool defaultValue)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            metaData ??= GetMetaData();
            try
            {
                if (ConstainKey(key))
                {
                    return metaData.Call<bool>("getBoolean", key);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"datdb - {e}");
            }
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            string value = GetPlistValue(key);
            if (value != null)
            {
                return value != "0";
                //return int.Parse(value);
            }
            else
            {
                Debug.Log($"datdb - value null");
            }
        }

        return defaultValue;
    }

    private bool ConstainKey(string key)
    {
        return metaData.Call<bool>("containsKey", key);
    }

    private AndroidJavaObject GetMetaData()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                AndroidJavaClass unityPlayer = new("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                string packageName = currentActivity.Call<string>("getPackageName");
                AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
                AndroidJavaObject applicationInfo =
                    packageManager.Call<AndroidJavaObject>("getApplicationInfo", packageName, 128);
                return applicationInfo.Get<AndroidJavaObject>("metaData");
            }
            catch (Exception e)
            {
                Debug.LogError($"datdb - {e}");
                return null;
            }
        }

        return null;
    }

    private string GetPlistValue(string key)
    {
#if UNITY_IOS
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            IntPtr result = GetInfoPlistValue(key);
            if (result == IntPtr.Zero)
            {
                return null;
            }
            return Marshal.PtrToStringAnsi(result);
        }
        else
        {
            Debug.LogError("datdb - This method is only supported on iOS.");
        }
#endif
        return null;
    }
}