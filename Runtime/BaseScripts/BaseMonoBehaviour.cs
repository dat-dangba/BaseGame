using Teo.AutoReference;
using UnityEditor;
using UnityEngine;

namespace DBD.BaseGame
{
    public abstract class BaseMonoBehaviour : MonoBehaviour
    {
        protected virtual void Reset()
        {
            AutoReference.Sync(this);
        }

        [OnAfterSync]
        protected virtual void OnAfterSyncAttribute()
        {
            LoadComponents();
        }

        protected virtual void LoadComponents()
        {
        }

        protected virtual void Awake()
        {
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void Start()
        {
        }

        protected virtual void Update()
        {
        }

        protected virtual void FixedUpdate()
        {
        }

#if UNITY_EDITOR
        protected T LoadAssetAtPath<T>(string assetPath) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }
#endif
    }
}