#nullable enable

namespace Edanoue.EH.Scene.Tests
{

    // このテストスイートでのみ使用されるシーンクラス
    public class TestScene_A : SceneBase
    {
        protected override string[] SceneNameList => new string[] {
                TestSceneInfo.A1_name,
                TestSceneInfo.A2_name,
            };
    }

    public class TestScene_B : SceneBase
    {
        protected override string[] SceneNameList => new string[] {
                TestSceneInfo.B1_name,
                TestSceneInfo.B2_name,
            };
    }

    public class TestScene_ForUnload : SceneBase
    {
        protected override string[] SceneNameList => new string[] { };
    }

    internal static class TestSceneInfo
    {
        // テスト用のシーンの情報
        internal const string A1_name = "__DO_NOT_BUILD__TEST_SCENE A-1";
        internal const string A2_name = "__DO_NOT_BUILD__TEST_SCENE A-2";
        internal const string B1_name = "__DO_NOT_BUILD__TEST_SCENE B-1";
        internal const string B2_name = "__DO_NOT_BUILD__TEST_SCENE B-2";
        internal const string A1_path = "Assets/EH/Runtime/Scripts/Scene/Tests/Runtime/TestScenes/" + A1_name + ".unity";
        internal const string A2_path = "Assets/EH/Runtime/Scripts/Scene/Tests/Runtime/TestScenes/" + A2_name + ".unity";
        internal const string B1_path = "Assets/EH/Runtime/Scripts/Scene/Tests/Runtime/TestScenes/" + B1_name + ".unity";
        internal const string B2_path = "Assets/EH/Runtime/Scripts/Scene/Tests/Runtime/TestScenes/" + B2_name + ".unity";
    }
}
