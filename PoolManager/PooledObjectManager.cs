using System.Collections.Generic;
using UnityEngine;
using System;

namespace Utilities.PoolManager 
{
    public class PooledObjectManager : MonoBehaviour
    {
        [Serializable]
        public struct PoolStruct
        {
            public string poolName;
            public GameObject prefab;
            public int size;
            [Tooltip("If true, once the pool has used all of its GameObjects, re-use the first active one. Otherwise grow the pool.")]
            public bool limitReachUseActiveObject;
            [Tooltip("If true, parent all of the pooled objects under an empty GameObject.")]
            public bool autoParentObjects;
            [Tooltip("Only set if pooled objects should be parented to this GameObject. Will be ignored if Auto Parent is selected.")]
            public Transform manualSetParent;
        }

        private List<PoolStruct> _pools = new List<PoolStruct>();

        private Dictionary<GameObject, List<GameObject>> _poolsObjects = new Dictionary<GameObject, List<GameObject>>();
        private Dictionary<GameObject, List<GameObject>> _poolsActiveObjects = new Dictionary<GameObject, List<GameObject>>();

        private static PooledObjectManager _instance;
        public static PooledObjectManager Instance { get { return _instance; } }

        void Awake()
        {
            if (_instance != null)
                Destroy(gameObject);
            else
                _instance = this;

            // Comment out or delete this if you want to rebuild the pools for new levels.
            DontDestroyOnLoad(gameObject);
        }

        public void InitalizePools(List<PoolStruct> pools)
        {
            _pools = pools;
            ClearOlderPools();
            CreatePools();
        }

        private void ClearOlderPools()
        {
            _poolsObjects.Clear();
            _poolsActiveObjects.Clear();
        }

        private void CreatePools()
        {
            for (int i = 0; i < _pools.Count; i++)
            {
                PoolStruct pool = _pools[i];
                if (IsValidPoolStruct(pool))
                {
                    _poolsObjects.Add(pool.prefab, new List<GameObject>());
                    _poolsActiveObjects.Add(pool.prefab, new List<GameObject>());
                    GrowPool(_pools[i]);
                }
            }
        }

        private bool IsValidPoolStruct(PoolStruct pool)
        {
            return pool.prefab != null;
        }

        private void GrowPool(PoolStruct pool)
        {
            if (pool.autoParentObjects)
            {
                GameObject poolParent = new GameObject(pool.poolName + " Pool");
                for (int i = 0; i < pool.size; i++)
                {
                    CreatePooledObject(pool.prefab, poolParent.transform);
                }
            }
            else if (pool.manualSetParent != null)
            {
                for (int i = 0; i < pool.size; i++)
                {
                    CreatePooledObject(pool.prefab, pool.manualSetParent);
                }
            }
            else
            {
                for (int i = 0; i < pool.size; i++)
                {
                    CreatePooledObject(pool.prefab);
                }
            }
        }

        /// <summary>
        /// Creates a pooled object that is parented to the poolParent.
        /// </summary>
        /// <param name="poolPrefab">The prefab to create.</param>
        /// <param name="poolParent">The parent which the prefabs will be parented to.</param>
        private void CreatePooledObject(GameObject poolPrefab, Transform poolParent)
        {
            GameObject pooledGameObject = Instantiate(poolPrefab, poolParent.transform);
            
            if (!pooledGameObject.TryGetComponent(out PooledObject pooledScript))
            {
                Debug.LogErrorFormat("Invalid Prefab for PoolManager: The prefab {0} should be derived from PooledObject.", poolPrefab.name);
                Destroy(pooledGameObject);
            }
            else
            {
                pooledScript.InitPooledObject(poolPrefab);
                pooledGameObject.SetActive(false);
                _poolsObjects[poolPrefab].Add(pooledGameObject); ;
            }
        }

        private void CreatePooledObject(GameObject poolPrefab)
        {
            GameObject pooledGameObject = Instantiate(poolPrefab);
            
            if (!pooledGameObject.TryGetComponent(out PooledObject pooledScript))
            {
                Debug.LogErrorFormat("Invalid Prefab for PoolManager: The prefab {0} should be derived from PooledObject.", poolPrefab.name);
                Destroy(pooledGameObject);
            }
            else
            {
                pooledScript.InitPooledObject(poolPrefab);
                pooledGameObject.SetActive(false);
                _poolsObjects[poolPrefab].Add(pooledGameObject); ;
            }
        }

        /// <summary>
        /// Gets a component from a pooled object.
        /// </summary>
        /// <typeparam name="T">The component to get.</typeparam>
        /// <param name="prefab">The pooled prefab to get the component from.</param>
        /// <returns></returns>
        public T UseObjectFromPool<T>(GameObject prefab)
        {
            GameObject pooledObject = UseObjectFromPool(prefab);
            return pooledObject.GetComponent<T>();
        }

        /// <summary>
        /// Get a component from a pooled object.
        /// </summary>
        /// <typeparam name="T">The type of component to get</typeparam>
        /// <param name="prefab">The type of pooled object to get</param>
        /// <param name="component">The returned component</param>
        /// <returns></returns>
        public bool TryGetObjectFromPool<T>(GameObject prefab, out T component)
        {
            GameObject pooledObject = GetObjectFromPool(prefab);
            if (pooledObject.TryGetComponent(out T comp))
            {
                component = comp;
                return true;
            }
            else
            {
                component = comp;
                return false;
            }
        }

        /// <summary>
        /// Get a GameObject from the pool and set the position and rotation.
        /// </summary>
        /// <param name="prefab">The prefab to get from the pool.</param>
        /// <param name="position">The position to set the GameObject to.</param>
        /// <param name="rotation">The rotation to set the GameObject to.</param>
        /// <param name="useLocalPosition">If true, use the local position of the GameObject.</param>
        /// <returns>The pooled object of type prefab.</returns>
        public GameObject UseObjectFromPool(GameObject prefab, Vector3 position, Quaternion rotation, bool useLocalPosition = false)
        {
            GameObject pooledObject = GetObjectFromPool(prefab);

            if (pooledObject != null)
            {
                if (useLocalPosition)
                {
                    pooledObject.transform.localPosition = position;
                    pooledObject.transform.localRotation = rotation;
                    pooledObject.SetActive(true);
                }
                else
                {
                    pooledObject.transform.SetPositionAndRotation(position, rotation);
                    pooledObject.SetActive(true);
                }

                _poolsActiveObjects[prefab].Add(pooledObject);
            }

            return pooledObject;
        }

        private GameObject UseObjectFromPool(GameObject prefab)
        {
            GameObject pooledObject = GetObjectFromPool(prefab);

            if (pooledObject != null)
            {
                pooledObject.SetActive(true);

                _poolsActiveObjects[prefab].Add(pooledObject);
            }

            return pooledObject;
        }

        private GameObject GetObjectFromPool(GameObject prefab)
        {
            //List<GameObject> pooledObject = new List<GameObject>();
            _poolsObjects.TryGetValue(prefab, out List<GameObject> pooledObject);

            for (int i = 0; i < pooledObject.Count; i++)
            {
                if (!pooledObject[i].activeInHierarchy)
                {
                    return pooledObject[i];
                }
            }
            return NotEnoughPooledObject(prefab);
        }

        private GameObject NotEnoughPooledObject(GameObject prefab)
        {
            PoolStruct pool = GetPoolStruct(prefab);

            if (IsValidPoolStruct(pool))
            {
                if (pool.limitReachUseActiveObject)
                {
                    return UseFirstActivePooledObject(pool);
                }
                else
                {
                    return DynamicGrowPool(pool);
                }
            }

            return null;
        }

        private PoolStruct GetPoolStruct(GameObject poolPrefab)
        {
            for (int i = 0; i < _pools.Count; i++)
            {
                if (_pools[i].m_Prefab.Equals(poolPrefab))
                {
                    return _pools[i];
                }
            }

            return new PoolStruct() { prefab = null };
        }

        private GameObject UseFirstActivePooledObject(PoolStruct pool)
        {
            _poolsActiveObjects.TryGetValue(pool.prefab, out List<GameObject> activeObjects);

            if (activeObjects.Count != 0)
            {
                GameObject firstElement = activeObjects[0];
                activeObjects.RemoveAt(0);
                return firstElement;
            }

            return null;
        }

        private GameObject DynamicGrowPool(PoolStruct pool)
        {
#if UNITY_EDITOR
            Debug.LogWarningFormat("POOL TO SMALL: The pool {0} is not big enough, creating {1} more PooledObject. Increments the pool size.",
                pool.m_Prefab.name, pool.m_Size);
#endif

            GrowPool(pool);
            return GetObjectFromPool(pool.prefab);
        }

        /// <summary>
        /// Manually return a Pooled Object.
        /// </summary>
        /// <param name="pooledObject"></param>
        /// <param name="poolOwner"></param>
        public void ReturnPooledObject(PooledObject pooledObject, GameObject poolOwner)
        {
            if (_poolsActiveObjects.ContainsKey(pooledObject.PoolOwner))
            {
                List<GameObject> activeObjects = _poolsActiveObjects[poolOwner];

                if (activeObjects.Count != 0)
                {
                    activeObjects.Remove(pooledObject.gameObject);
                }
            }
        }
    }
}
