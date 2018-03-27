using System;
using System.Collections.Generic;
using UnityEngine;

namespace Osblow
{
    public class PrefabPool
    {
        private Dictionary<string, GameObject> m_loadedPrefabs = new Dictionary<string, GameObject>();


        public GameObject GetPrefab(string path)
        {
            if (m_loadedPrefabs.ContainsKey(path))
            {
                return m_loadedPrefabs[path];
            }

            return LoadPrefab(path);
        }

        private GameObject LoadPrefab(string path)
        {
            GameObject prefab = Resources.Load(path) as GameObject;
            if (!prefab)
            {
                throw new UnassignedReferenceException();
                //return null;
            }

            // 一个古老问题，后续版本待测
            // 只Load不Instantiate依然会引起第一次实例化卡顿
            GameObject obj = GameObject.Instantiate(prefab);
            obj.SetActive(false);
            m_loadedPrefabs[path] = obj;

            return obj;
        }
    }
}
