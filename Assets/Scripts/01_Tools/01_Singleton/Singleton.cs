using UnityEngine;

public class Singleton<T> where T : class, new()
{
    protected static T _instance;

    public static T Instance
    {
        get
        {
            _instance ??= new T();
            return _instance;
        }
    }
}

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();
                if (_instance == null)
                {
                    GameObject obj = new(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                }
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            return;
        }

        if (_instance != this as T)
            Destroy(gameObject);
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this as T)
            _instance = null;
    }
}
