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

public class MonoSingleton<T> : MonoBehaviour where T : class, new()
{
    protected static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                var type = typeof(T);
                _instance = FindFirstObjectByType(type) as T;
                if (_instance == null)
                {
                    GameObject obj = new(type.Name);
                    _instance = obj.AddComponent(type) as T;
                }
            }
            return _instance;
        }
    }
}
