#nullable enable

using NUnit.Framework;
using Edanoue.EH.Scene;

public class UtilityTest
{
    /// <summary>
    /// このテストスイート向けに作成されたテスト用のシーン
    /// </summary>
    public class UtilityTestScene_A : SceneBase
    {
        protected override string[] SceneNameList => new string[] {
                "__DO_NOT_BUILD__TEST_SCENE A-1",
                "__DO_NOT_BUILD__TEST_SCENE A-2",
            };
    }

    public class UtilityTestScene_B : SceneBase
    {
        protected override string[] SceneNameList => new string[] {
                "__DO_NOT_BUILD__TEST_SCENE B-1",
                "__DO_NOT_BUILD__TEST_SCENE B-2",
            };
    }

    [Test]
    [Category("Normal")]
    public void このアセンブリを含むシーン一覧が取得できる()
    {
        // 全てのシーンの型情報を取得する
        Utility.GetAllSceneTypes(out var scenes);
        {
            bool isFound = false;
            foreach (var scene in scenes)
            {
                // このテストケース内にあるシーンが取得できたかどうか
                if (scene == typeof(UtilityTestScene_A))
                {
                    isFound = true;
                    break;
                }
            }
            Assert.That(isFound, Is.EqualTo(true));
        }
    }
}
