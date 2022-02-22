#nullable enable

using UnityEngine;
using System.Collections;

namespace Edanoue.EH.Scene
{
    /// <summary>
    /// This class allows us to start Coroutines from non-Monobehaviour scripts
    /// Create a GameObject it will use to launch the coroutine on
    /// </summary>
    public class CoroutineHandler : MonoBehaviour
    {
        private static CoroutineHandler? m_Instance;
        private static CoroutineHandler Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    // Create GameObject on DontDestroyOnLoad
                    var o = new GameObject("CoroutineHandler");
                    DontDestroyOnLoad(o);
                    // Attatch this component
                    m_Instance = o.AddComponent<CoroutineHandler>();
                }

                return m_Instance;
            }
        }

        private void OnDisable()
        {
            if (m_Instance != null)
            {
                Destroy(m_Instance.gameObject);
                m_Instance = null;
            }
        }

        /// <summary>
        /// Start Static Coroutine
        /// </summary>
        /// <param name="coroutine"></param>
        /// <returns></returns>
        public static Coroutine StartStaticCoroutine(IEnumerator coroutine)
        {
            return Instance.StartCoroutine(coroutine);
        }
    }

}
