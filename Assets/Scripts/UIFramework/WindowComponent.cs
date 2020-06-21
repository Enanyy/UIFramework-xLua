using UnityEngine;
using System.Collections;
public class WindowComponent : MonoBehaviour
{
    public T GetComponent<T>(string path) where T : Component
    {
        if (string.IsNullOrEmpty(path))
        {
            TryGetComponent(out T component);
            return component;
        }
        else
        {
            Transform child = transform.Find(path);
            if (child)
            {
                child.TryGetComponent(out T component);
                return component;
            }
        }
        return null;
    }
}