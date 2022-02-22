#nullable enable

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Edanoue.EH.Scene;
using Edanoue.EH.Scene.Tests;

// テスト実行の前後のビルド段階で行う処理を指定
[PrebuildSetup(typeof(PreBuildAndPostBuildProcess))]
[PostBuildCleanup(typeof(PreBuildAndPostBuildProcess))]
public class マルチシーンロードのテスト
{

    #region テストケース

    /// <summary>
    /// TestScene A に移動する
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    [Category("Normal")]
    [Timeout(5000)] // 5s
    public IEnumerator TestSceneAに移動する()
    {
        // シーンA に移動する
        yield return Utility.MoveTo<TestScene_A>();

        // 移動後のシーンの名前を取得する
        {
            Utility.GetLoadedSceneNames(out var loadedSceneNames);

            // TestScene A の 全てのシーンがロードされている
            CollectionAssert.Contains(loadedSceneNames, TestSceneInfo.A1_name);
            CollectionAssert.Contains(loadedSceneNames, TestSceneInfo.A2_name);
        }

        // シーンAを全てアンロードしておく
        yield return Utility.MoveTo<TestScene_ForUnload>();
    }

    /// <summary>
    /// TestScene A に移動したあとに, TestSceneB に移動する
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    [Category("Normal")]
    [Timeout(5000)] // 5s
    public IEnumerator TestSceneAに移動したあとにTestSceneBに移動する()
    {
        // シーンA に移動する
        yield return Utility.MoveTo<TestScene_A>();

        // そのあと, シーンB に移動する
        yield return Utility.MoveTo<TestScene_B>();

        // 移動後のシーンの名前を取得する
        {
            Utility.GetLoadedSceneNames(out var loadedSceneNames);

            // TestScene B の 全てのシーンがロードされている
            CollectionAssert.Contains(loadedSceneNames, TestSceneInfo.B1_name);
            CollectionAssert.Contains(loadedSceneNames, TestSceneInfo.B2_name);

            // TestScene A の 全てのシーンは アンロードされている
            CollectionAssert.DoesNotContain(loadedSceneNames, TestSceneInfo.A1_name);
            CollectionAssert.DoesNotContain(loadedSceneNames, TestSceneInfo.A2_name);
        }

        // シーンBを全てアンロードしておく
        yield return Utility.MoveTo<TestScene_ForUnload>();
    }

    /// <summary>
    /// TestScene A に  2回連続で移動する
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    [Category("Normal")]
    [Timeout(5000)] // 5s
    public IEnumerator TestSceneAに2回連続で移動する()
    {
        // シーンA に移動する
        yield return Utility.MoveTo<TestScene_A>();

        // シーン内に適当にオブジェクトを作成する
        new GameObject("foobar");

        // シーンA にもう一度移動する(特にエラーなどが発生しない)
        yield return Utility.MoveTo<TestScene_A>();

        // なおこの場合, リロードではなく, 無視 という挙動になる
        // 先ほど作成したオブジェクトが存在しているかを確認する
        var go = GameObject.Find("foobar");
        Assert.IsNotNull(go);

        // 移動後のシーンの名前を取得する
        {
            // 現在ロード中のシーンをすべて読み込む
            Utility.GetLoadedSceneNames(out var loadedSceneNames);

            // TestScene A の 全てのシーンがロードされている
            CollectionAssert.Contains(loadedSceneNames, TestSceneInfo.A1_name);
            CollectionAssert.Contains(loadedSceneNames, TestSceneInfo.A2_name);
        }

        // シーンAを全てアンロードしておく
        yield return Utility.MoveTo<TestScene_ForUnload>();
    }

    #endregion
}
