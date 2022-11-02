using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 *  Pooled objects must derive from this class to enable error logging.
 */
namespace Utilities.PoolManager 
{
    public abstract class PooledObject : MonoBehaviour
    {
        [Tooltip("Show the StackTrace to know the source of the objects destruction (requires active debug mode on)")]
        public bool destroyErrorSourceOnly = false;

        [Tooltip("Show error messages in the console")]
        public bool activeDebugMode = false;

        private GameObject _poolOwner;
        public GameObject PoolOwner { get { return _poolOwner; } }

        private string _StackTrace;
        private bool _SceneHasBeenUnloaded = false;

        private void Awake()
        {
#if UNITY_EDITOR
            SceneManager.sceneUnloaded += OnSceneUnloaded;
#endif
        }

#if UNITY_EDITOR
        private void OnSceneUnloaded(Scene scene)
        {
            if (scene == gameObject.scene)
            {
                m_SceneHasBeenUnloaded = true;
            }
        }
#endif

        public virtual void InitPooledObject(GameObject poolOwner)
        {
            _poolOwner = poolOwner;
            gameObject.SetActive(false);
        }

        protected virtual void OnDisable()
        {
#if UNITY_EDITOR
            m_StackTrace = StackTraceUtility.ExtractStackTrace();
#endif
            PooledObjectManager.Instance.ReturnPooledObject(this, PoolOwner);
        }

        protected virtual void OnDestroy()
        {
#if UNITY_EDITOR
            if (!m_SceneHasBeenUnloaded)
            {
                if (activeDebugMode)
                {
                    Debug.LogError(GetDestroyErrorMessage());
                }

                SceneManager.sceneUnloaded -= OnSceneUnloaded;
            }
#endif
        }

#if UNITY_EDITOR
        private string GetDestroyErrorMessage()
        {
            string errorMessage = "(Ignore it if you were stopping Unity play mode or changing scenes)\nPooledObject should have never been destroyed. \n{0}";

            if (!destroyErrorSourceOnly)
            {
                errorMessage = string.Format(errorMessage, m_StackTrace);
            }
            else
            {
                errorMessage = string.Format(errorMessage, GetDestroyerSource());
            }

            return errorMessage;
        }

        private string GetDestroyerSource()
        {
            string[] traces = m_StackTrace.Split('\n');
            return traces[traces.Length - 2] + "\n";
        }
#endif
    }
}
