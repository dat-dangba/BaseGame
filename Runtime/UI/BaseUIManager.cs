using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public abstract class BaseUIManager<INSTANCE> : BaseMonoBehaviour where INSTANCE : BaseMonoBehaviour
{
    [SerializeField] private List<BaseUI> prefabs;

    private Dictionary<Type, BaseUI> uiPrefabs = new();

    private Dictionary<Type, BaseUI> uiLoaded = new();

    protected abstract Transform GetParent();

    protected abstract string GetFolderPrefabs();

    protected abstract void OnInitCompleted();

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadUIPrefabs();
    }

    private void LoadUIPrefabs()
    {
        prefabs = new List<BaseUI>();

#if UNITY_EDITOR
        string[] files = Directory.GetFiles(GetFolderPrefabs(), "*.prefab");

        foreach (string file in files)
        {
            BaseUI prefab = AssetDatabase.LoadAssetAtPath(file, typeof(BaseUI)) as BaseUI;
            if (prefab != null)
            {
                prefabs.Add(prefab);
            }
        }
#endif
    }

    #region Singleton

    private static INSTANCE instance;

    public static INSTANCE Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = FindFirstObjectByType<INSTANCE>();
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

    #endregion

    protected override void Start()
    {
        base.Start();
        CreateUIPrefabs();
        OnInitCompleted();
    }

    private void CreateUIPrefabs()
    {
        uiPrefabs = new Dictionary<Type, BaseUI>();
        foreach (var item in prefabs)
        {
            uiPrefabs[item.GetType()] = item;
        }
    }

    /// <summary>
    /// Hiển thị UI 
    /// </summary>
    /// <param name="OnPreShow"></param>
    /// <typeparam name="T"></typeparam>
    public virtual void Show<T>(Action<T> OnPreShow = null) where T : BaseUI
    {
        Show(typeof(T), baseUI => { OnPreShow?.Invoke((T)baseUI); });
    }

    /// <summary>
    /// Hiển thị UI 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="OnPreShow"></param>
    public virtual void Show(Type type, Action<BaseUI> OnPreShow = null)
    {
        BaseUI ui = GetUI(type);
        OnPreShow?.Invoke(ui);
        ui.Show();
        BringToFront(ui);
    }

    /// <summary>
    /// Show UI ngay lập tức (không anim)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public virtual void ShowImmediately<T>() where T : BaseUI
    {
        ShowImmediately(typeof(T));
    }

    /// <summary>
    /// Show UI ngay lập tức (không anim)
    /// </summary>
    /// <param name="type"></param>
    public virtual void ShowImmediately(Type type)
    {
        BaseUI ui = GetUI(type);
        ui.ShowImmediately();
        BringToFront(ui);
    }

    /// <summary>
    /// Đưa UI lên trên cùng 
    /// </summary>
    /// <param name="ui"></param>
    public virtual void BringToFront(BaseUI ui)
    {
        if (IsUIDisplayed(ui.GetType()))
        {
            ui.transform.SetSiblingIndex(GetParent().childCount - 1);
        }
    }

    /// <summary>
    /// Ẩn UI
    /// </summary>
    /// <param name="OnInvisible"></param>
    /// <typeparam name="T"></typeparam>
    public virtual void Hide<T>(Action OnInvisible = null) where T : BaseUI
    {
        Hide(typeof(T), OnInvisible);
    }

    /// <summary>
    /// Ẩn UI
    /// </summary>
    /// <param name="type"></param>
    /// <param name="OnInvisible"></param>
    public virtual void Hide(Type type, Action OnInvisible = null)
    {
        if (IsUIDisplayed(type))
        {
            uiLoaded[type].Hide(OnInvisible);
        }
    }

    /// <summary>
    /// Ẩn UI ngay lập tức (không anim)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public virtual void HideImmediately<T>() where T : BaseUI
    {
        HideImmediately(typeof(T));
    }

    /// <summary>
    /// Ẩn UI ngay lập tức (không anim)
    /// </summary>
    /// <param name="type"></param>
    public virtual void HideImmediately(Type type)
    {
        if (IsUIDisplayed(type))
        {
            uiLoaded[type].HideImmediately();
        }
    }

    /// <summary>
    /// Kiểm tra UI đã có trên scene chưa 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual bool IsUILoaded<T>() where T : BaseUI
    {
        return IsUILoaded(typeof(T));
    }

    /// <summary>
    /// Kiểm tra UI đã có trên scene chưa 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public virtual bool IsUILoaded(Type type)
    {
        return uiLoaded.ContainsKey(type) && uiLoaded[type] != null;
    }

    /// <summary>
    /// Kiểm tra UI có hiển thị hay không
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual bool IsUIDisplayed<T>() where T : BaseUI
    {
        return IsUIDisplayed(typeof(T));
    }

    /// <summary>
    /// Kiểm tra UI có hiển thị hay không
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public virtual bool IsUIDisplayed(Type type)
    {
        return IsUILoaded(type) && uiLoaded[type].gameObject.activeSelf;
    }

    /// <summary>
    /// Kiểm tra UI có hiển thị 1 mình hay không
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual bool IsUIOnlyDisplay<T>() where T : BaseUI
    {
        return IsUIOnlyDisplay(typeof(T));
    }


    /// <summary>
    /// Kiểm tra UI có hiển thị 1 mình hay không
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public virtual bool IsUIOnlyDisplay(Type type)
    {
        bool isOnlyDisplay = false;

        foreach (var item in uiLoaded)
        {
            if (item.Value != null && item.Value.gameObject.activeSelf)
            {
                if (item.Key == type)
                {
                    isOnlyDisplay = true;
                }
                else
                {
                    return false;
                }
            }
        }

        return isOnlyDisplay;
    }

    /// <summary>
    /// Kiểm tra UI có nằm trên cùng hay không 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual bool IsUIOnTop<T>() where T : BaseUI
    {
        return IsUIOnTop(typeof(T));
    }


    /// <summary>
    /// Kiểm tra UI có nằm trên cùng hay không 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public virtual bool IsUIOnTop(Type type)
    {
        Transform parent = GetParent();
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            if (parent.GetChild(i).gameObject.activeSelf)
            {
                if (parent.GetChild(i).TryGetComponent<BaseUI>(out var ui))
                {
                    return uiLoaded.ContainsKey(ui.GetType()) && ui.GetType() == type;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Get UI, nếu chưa show thì show 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual T GetUI<T>() where T : BaseUI
    {
        return GetUI(typeof(T)) as T;
    }

    /// <summary>
    /// Get UI, nếu chưa show thì show 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public virtual BaseUI GetUI(Type type)
    {
        if (IsUILoaded(type)) return uiLoaded[type];
        BaseUI prefab = GetUIPrefab(type);
        BaseUI ui = Instantiate(prefab, GetParent());
        ui.name = prefab.name;

        uiLoaded[type] = ui;

        return uiLoaded[type];
    }

    private T GetUIPrefab<T>() where T : BaseUI
    {
        return GetUIPrefab(typeof(T)) as T;
    }

    private BaseUI GetUIPrefab(Type type)
    {
        return uiPrefabs[type];
    }

    /// <summary>
    /// Ẩn toàn bộ UI 
    /// </summary>
    /// <param name="isImmediately"></param>
    public virtual void HideAll(bool isImmediately = true)
    {
        foreach (var item in uiLoaded)
        {
            if (item.Value != null && item.Value.gameObject.activeSelf)
            {
                if (isImmediately)
                {
                    item.Value.HideImmediately();
                }
                else
                {
                    item.Value.Hide();
                }
            }
        }
    }

    /// <summary>
    /// Ẩn toàn bộ UI ngoại trừ T 
    /// </summary>
    /// <param name="isImmediately"></param>
    /// <typeparam name="T"></typeparam>
    public virtual void HideAllIgnore<T>(bool isImmediately) where T : BaseUI
    {
        HideAllIgnore(typeof(T), isImmediately);
    }

    /// <summary>
    /// Ẩn toàn bộ UI ngoại trừ UI với Type = type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="isImmediately"></param>
    public virtual void HideAllIgnore(Type type, bool isImmediately)
    {
        foreach (var item in uiLoaded)
        {
            if (item.Value != null && item.Value.gameObject.activeSelf && item.Value.GetType() != type)
            {
                if (isImmediately)
                {
                    item.Value.HideImmediately();
                }
                else
                {
                    item.Value.Hide();
                }
            }
        }
    }
}