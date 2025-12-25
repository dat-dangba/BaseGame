using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<INSTANCE> : BaseMonoBehaviour where INSTANCE : MonoBehaviour
{
    [SerializeField] protected bool dontDestroyOnLoad = true;

    private static INSTANCE instance;

    public static INSTANCE Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = FindAnyObjectByType<INSTANCE>();
            if (instance != null) return instance;
            GameObject singleton = new(typeof(INSTANCE).Name);
            instance = singleton.AddComponent<INSTANCE>();
            DontDestroyOnLoad(singleton);

            return instance;
        }
    }

    protected override void Awake()
    {
        if (instance == null)
        {
            instance = this as INSTANCE;

            if (!dontDestroyOnLoad) return;

            Transform root = transform.root;
            if (root != transform)
            {
                DontDestroyOnLoad(root);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}