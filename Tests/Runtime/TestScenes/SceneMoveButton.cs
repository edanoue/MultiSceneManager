using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

using Edanoue.EH.Scene;

namespace Edanoue.EH.Scene.Tests
{
    public class SceneMoveButton : MonoBehaviour
    {
        internal class TestScene_A : SceneBase
        {
            protected override string[] SceneNameList => new string[] {
                TestSceneInfo.A1_name,
                TestSceneInfo.A2_name,
            };
        }
        internal class TestScene_B : SceneBase
        {
            protected override string[] SceneNameList => new string[] {
                TestSceneInfo.B1_name,
                TestSceneInfo.B2_name,
            };
        }

        void OnGUI()
        {
            if (GUI.Button(new Rect(10, 40, 100, 20), "Load Scene A"))
            {
                Utility.MoveTo<TestScene_A>();
            }

            if (GUI.Button(new Rect(10, 70, 100, 20), "Load Scene B"))
            {
                Utility.MoveTo<TestScene_B>();
            }
        }

        IEnumerator LoadScene(string name)
        {
            var a = SceneManager.LoadSceneAsync(name);
            while (!a.isDone)
            {
                yield return null;
            }
        }
    }
}
