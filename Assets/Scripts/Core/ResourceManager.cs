using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : IResourceManager
{
    private Dictionary<string, List<GameObject>> objectPools = new Dictionary<string, List<GameObject>>();

    public T CreatePrefabInstance<T, E>(E item) where E : Enum
    {
        var prefab = CreatePrefabInstance(item);
        var result = prefab.GetComponent<T>();

        return result;
    }

    public GameObject CreatePrefabInstance<E>(E item) where E : Enum
    {
        var path = string.Format("{0}/{1}", typeof(E).Name, item.ToString());
        var asset = Resources.Load<GameObject>(path);
        var result = GameObject.Instantiate(asset);

        return result;
    }

    public T GetAsset<T, E>(E item) where T : UnityEngine.Object where E : Enum
    {
        var path = string.Format("{0}/{1}", typeof(E).Name, item.ToString());
        var result = Resources.Load<T>(path);

        return result;
    }
}
