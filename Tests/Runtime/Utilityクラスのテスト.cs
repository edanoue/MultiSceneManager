#nullable enable

using Edanoue.EH.Scene;
using Edanoue.EH.Scene.Tests;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

// テスト実行の前後のビルド段階で行う処理を指定
[PrebuildSetup(typeof(PreBuildAndPostBuildProcess))]
[PostBuildCleanup(typeof(PreBuildAndPostBuildProcess))]
public class Utilityクラスのテスト
{
    /// <summary>
    /// ロード済みのシーンがIsLoadedでTrue返ってくる
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    [Category("Normal")]
    [Timeout(5000)] // 5s
    public IEnumerator ロード済みのシーンはIsLoadedでTrueが返ってくる()
    {
        // シーンA に移動する
        yield return Utility.MoveTo<TestScene_A>();

        // シーンA がロード済みである
        Assert.That(Utility.IsLoaded<TestScene_A>(), Is.True);

        // シーンA を全てアンロードしておく
        yield return Utility.MoveTo<TestScene_ForUnload>();
    }

    /// <summary>
    /// ロードしてないシーンがIsLoadedでFalse返ってくる
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    [Category("Normal")]
    [Timeout(5000)] // 5s
    public IEnumerator ロード済でないシーンはIsLoadedでFalseが返ってくる()
    {
        // シーンA に移動する
        yield return Utility.MoveTo<TestScene_A>();

        // シーンB はロード済みではない
        Assert.That(Utility.IsLoaded<TestScene_B>(), Is.False);

        // シーンA を全てアンロードしておく
        yield return Utility.MoveTo<TestScene_ForUnload>();
    }
}
