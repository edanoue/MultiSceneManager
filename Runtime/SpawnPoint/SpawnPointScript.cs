#nullable enable

using UnityEngine;

namespace Edanoue.EH.Scene
{
    public class SpawnPointScript : MonoBehaviour, ISpawnPoint
    {
        /// <summary>
        /// This target point specify name
        /// </summary>
        [SerializeField]
        [Tooltip("Specify unique name in Scene")]
        private string m_name = "Default";

        #region Begin ISpawnTarget implements

        string ISpawnPoint.Name => m_name;
        Vector3 ISpawnPoint.Position => transform.position;
        Quaternion ISpawnPoint.Rotation => transform.rotation;

        #endregion // End ISpawnTarget implements

#if UNITY_EDITOR
        /// <summary>
        /// Draw Gizmo on Editor
        /// </summary>
        void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "target.png", true);
        }
#endif
    }
}
