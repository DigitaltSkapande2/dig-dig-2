using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class Singleton<T> : NetworkBehaviour where T : NetworkBehaviour
{
    private static T instance;
    [Header("Singleton Settings")]
    [SerializeField] private bool destroyOnLoad = true;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                T potentialFoundInstance = FindFirstObjectByType<T>();
                if (potentialFoundInstance == null)
                {
                    Debug.LogError($"No singleton of type {typeof(T).Name} has been initialized.");
                    return null;
                }
                else
                {
                    return potentialFoundInstance;
                }
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
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;
        Debug.Log("Set instance of " + gameObject.name);
        if (destroyOnLoad) {
            DontDestroyOnLoad(gameObject);
        }
    }
}
