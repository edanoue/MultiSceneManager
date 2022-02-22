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
public class SpawnPoint
{
    #region テストケース

    /// <summary>
    /// TestScene A の SpawnPointを指定して 移動する
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    [Category("Normal")]
    [Timeout(5000)] // 5s
    public IEnumerator SpawnPointに移動する()
    {
        // シーンA に移動する
        // {0, 1, 0} にある Bathroom という地点に移動する
        // FIXME: 現状型がないです
        Vector3 spawnPosition = Vector3.zero;
        yield return Utility.MoveTo<TestScene_A>(
            spawnPointName: "Bathroom",
            setPositonCallback: (p, r) =>
            {
                // Bathroom への座標指定が行われる
                spawnPosition = p;
            }
        );

        // spawnPositon が 0, 1, 0 である
        Assert.That(spawnPosition, Is.EqualTo(new Vector3(0, 1, 0)));

        // シーンAを全てアンロードしておく
        yield return Utility.MoveTo<TestScene_ForUnload>();
    }

    /// <summary>
    /// TestScene A の SpawnPointを指定して 移動するがみつからないばあい
    /// 原点に移動する
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    [Category("Normal")]
    [Timeout(5000)] // 5s
    public IEnumerator 存在しないSpawnPointを指定すると原点にスポーンする()
    {
        // テストの結果比較用の座標を用意する
        // このあとの代入で変化することを期待するので, 最初にわざと変な値を入れておく
        Vector3 spawnPosition = new(0, 1, 0);

        yield return Utility.MoveTo<TestScene_A>(
            spawnPointName: "Missing",
            setPositonCallback: (p, _) =>
            {
                // 存在しない SpawnPoint が指定された場合は, (0, 0, 0) が代入される
                spawnPosition = p;
            }
        );

        // spawnPositon が 先頭で代入した (0, 1, 0) ではなく, (0, 0, 0) となっている
        Assert.That(spawnPosition, Is.EqualTo(new Vector3(0, 0, 0)));

        // シーンAを全てアンロードしておく
        yield return Utility.MoveTo<TestScene_ForUnload>();
    }

    #endregion
}
