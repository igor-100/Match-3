using System;
using UnityEngine;

public interface IResourceManager
{
    T CreatePrefabInstance<T, E>(E item) where E : Enum;
    GameObject CreatePrefabInstance<E>(E item) where E : Enum;
    T GetAsset<T, E>(E item) where T : UnityEngine.Object where E : Enum;
}
