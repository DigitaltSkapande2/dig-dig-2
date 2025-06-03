using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError($"no singleton of type {typeof(T).Name} has been initialized");
                return null;
            }
            return instance;
        }

        private set
        {
            instance = value; 
        }
    }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this as T;
        DontDestroyOnLoad(gameObject); 
    }
}
