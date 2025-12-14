using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigDig2
{
    public class SingletonGameObject : MonoBehaviour
    {
        // Global registry of the first-seen component instance per concrete component Type.
        // This enforces at most one live instance of any MonoBehaviour-derived type when this helper
        // is active on the GameObject that contains that component.
        private static Dictionary<Type, MonoBehaviour> types = new Dictionary<Type, MonoBehaviour>();

        void Awake()
        {
            // Find all MonoBehaviour components on this GameObject (including inherited types)
            var comps = GetComponents<MonoBehaviour>();

            // Iterate over a copy to avoid issues if we destroy components during the loop.
            foreach (var comp in comps)
            {
                if (comp == null) continue;

                var t = comp.GetType();

                // If we already registered an instance for this concrete type, treat this as a duplicate
                if (types.TryGetValue(t, out var existing))
                {
                    if (existing != comp)
                    {
                        Debug.LogWarning($"[SingletonGameObject] Duplicate component of type {t.Name} found on GameObject '{gameObject.name}'. Destroying duplicate instance.");
                        // Destroy the duplicate component (not the whole GameObject)
                        Destroy(comp);
                    }
                }
                else
                {
                    // Register the first-seen instance of this component type
                    types[t] = comp;
                }
            }
        }

        void OnDestroy()
        {
            // Clean up registry entries that pointed to components on this GameObject
            // (Remove only entries whose instance equals a destroyed component)
            var keys = new List<Type>(types.Keys);
            foreach (var k in keys)
            {
                if (types[k] == null) // already destroyed elsewhere
                {
                    types.Remove(k);
                    continue;
                }

                var instance = types[k];
                // If the instance belongs to this GameObject, remove it so a new one can register later
                if (instance != null && instance.gameObject == gameObject)
                {
                    types.Remove(k);
                }
            }
        }
    }
}


