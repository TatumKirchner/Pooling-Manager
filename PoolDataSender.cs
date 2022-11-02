using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Place on an object to set up pools for your objects.
 */

namespace Utilities.PoolManager
{
    public class PoolDataSender : MonoBehaviour
    {
        [SerializeField, Tooltip("Set up what items you need pooled")]
        private List<PooledObjectManager.PoolStruct> _poolData = new List<PooledObjectManager.PoolStruct>();

        private void Start()
        {
            PooledObjectManager.Instance.InitalizePools(_poolData);
        }
    }
}
