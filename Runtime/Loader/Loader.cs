#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Edanoue.EH.Scene
{
    // このファイルでは, Unity の SceneManager を取り扱うため
    // 同アセンブリの似たようなクラスと区別化を行うために, ここで明示的な using を行っています
    using NativeSceneManager = UnityEngine.SceneManagement.SceneManager;
    using NativeSceneUtility = UnityEngine.SceneManagement.SceneUtility;
    using NativeLoadSceneMode = UnityEngine.SceneManagement.LoadSceneMode;

    /// <summary>
    /// <para>UnityEngine.SceneManagement.SceneManager.LoadScene を少し拡張したSceneLoad 用の Static Class</para>
    /// </summary>
    /// <note>
    /// <para>オリジナルの機能に加えて, 以下のオプションを追加しています</para>
    /// <para>  重複したシーンの読み込みを許可しない</para>
    /// <para>  BuildSettings 内に存在しないシーンの読み込みを許可しない</para>
    /// </note>
    internal static class Loader
    {
        /// <summary>
        /// SceneName を元に非同期でSceneをロードする関数
        /// Unity の LoadSceneAsyncの ラッパー
        /// * Editor実行時であっても, BuildSettings に存在しないシーンのロードを防止する機能があります
        /// * 同一シーンの 重複ロードを防止する 機能があります
        /// </summary>
        /// <note>
        /// あくまで内部でUnityのSceneLoad関数を呼び出しているだけなので, 名前解決はそちらに委ねます
        /// 例えば同名シーンの場合は, パス指定を行わないと希望のシーンをロードすることができません
        /// </note>
        /// <param name="sceneName">Name of the Scene to load.</param>
        /// <param name="mode">If LoadSceneMode.Single then all current Scenes will be unloaded before loading.</param>
        /// <param name="option"></param>
        /// <returns>AsyncOperation Use the AsyncOperation to determine if the operation has completed.</returns>
        internal static IEnumerator LoadSceneAsync(
            string sceneName,
            LoadSceneMode mode = LoadSceneMode.Single,
            SceneLoadOption option = default
        )
        {

            // キャッシュ済みの Build Settings 内に指定されたシーンが存在するかどうかを確認する
            if (!IsSceneIncludedInBuildSettings(sceneName))
            {
                throw new ArgumentException($"Failed to find scene in Build Settings: {sceneName}. Please check your project Build Setings.", nameof(sceneName));
            }

            // 重複ロードを許可しないならば
            if (!option.IsAllowDuplicateLoad)
            {
                // すでにロード済みのシーンかどうか?
                if (IsSceneLoaded(sceneName))
                {
                    Debug.Log($"Already loaded scene: {sceneName}");
                    yield break;
                }
            }

            // 通常の Scene Manager を利用したシーンロード
            // AsyncOperation を作成する
            var asyncLoad = mode switch
            {
                LoadSceneMode.Single => NativeSceneManager.LoadSceneAsync(sceneName, NativeLoadSceneMode.Single),
                LoadSceneMode.Additive => NativeSceneManager.LoadSceneAsync(sceneName, NativeLoadSceneMode.Additive),
                _ => throw new ArgumentOutOfRangeException(nameof(mode)),
            };

            // Unity側でなんか問題が発生
            if (asyncLoad is null)
            {
                throw new InvalidProgramException($"Failed to load scene at UnityEngine.SceneManagement.LoadSceneMode. scene: {sceneName}");
            }

            // ロード完了しても勝手に表示しないようにする
            // このばあい, 実際のロードが完了したら progress が 0.9f となる
            asyncLoad.allowSceneActivation = false;

            // シーンロードのトークンを作成する
            var sceneLoadingToken = new SceneLoadingToken(asyncLoad);

            // トークンを使用したシーンローディングを行う
            while (!sceneLoadingToken.IsPreLoaded)
            {
                sceneLoadingToken.Update();
                yield return null;
            }

            // TODO: この時点で事前ロード後のロードを許可する
            sceneLoadingToken.Complete();
            while (!sceneLoadingToken.IsCompleted)
            {
                sceneLoadingToken.Update();
                yield return null;
            }

            // 完全にロードされた
        }

        internal static IEnumerator LoadSceneAddressableAsync(
           string sceneName,
           LoadSceneMode mode = LoadSceneMode.Single,
           SceneLoadOption option = default
       )
        {
            // 重複ロードを許可しないならば
            if (!option.IsAllowDuplicateLoad)
            {
                // すでにロード済みのシーンかどうか?
                if (IsSceneLoaded(sceneName))
                {
                    Debug.Log($"Already loaded scene: {sceneName}");
                    yield break;
                }
            }

            // Addressable を利用したシーンロード
            // 先に単に Addressable のアドレス解決を行う
            var asyncLoadHandle = mode switch
            {
                LoadSceneMode.Single => UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(sceneName, NativeLoadSceneMode.Single, false),
                LoadSceneMode.Additive => UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(sceneName, NativeLoadSceneMode.Additive, false),
                _ => throw new ArgumentOutOfRangeException(nameof(mode)),
            };

            // Addressabe のアドレス解決を待つ
            yield return asyncLoadHandle;

            // Addressable のアドレス解決が終了した段階で, 解決に成功したかどうかを確認する
            if (asyncLoadHandle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                // アドレス解決に失敗したのでエラー
                throw new InvalidProgramException($"Failed to resolve address: {sceneName}");
            }

            // 解決に成功していた場合, シーンの読み込みを開始する
            var sceneInstance = asyncLoadHandle.Result;
            var asyncLoad = sceneInstance.ActivateAsync();

            // Unity側でなんか問題が発生
            if (asyncLoad is null)
            {
                throw new InvalidProgramException($"Failed to load scene at UnityEngine.SceneManagement.LoadSceneMode. scene: {sceneName}");
            }

            // ロード完了しても勝手に表示しないようにする
            // このばあい, 実際のロードが完了したら progress が 0.9f となる
            asyncLoad.allowSceneActivation = false;

            // シーンロードのトークンを作成する
            var sceneLoadingToken = new SceneLoadingToken(asyncLoad);

            // トークンを使用したシーンローディングを行う
            while (!sceneLoadingToken.IsPreLoaded)
            {
                sceneLoadingToken.Update();
                yield return null;
            }

            // TODO: この時点で事前ロード後のロードを許可する
            sceneLoadingToken.Complete();
            while (!sceneLoadingToken.IsCompleted)
            {
                sceneLoadingToken.Update();
                yield return null;
            }

            // 完全にロードされた
        }

        /// <summary>
        /// シーンのアンロードを行います
        /// アンロード実行後に使用していないアセットの参照を開放する処理を実行します
        /// </summary>
        /// <note>
        /// シーンA の内部のコンテキストから シーンA のアンロードを行うとハングします
        /// </note>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        internal static IEnumerator UnloadSceneAsync(string sceneName)
        {
            // Unity Native の Unload を実行
            {
                var asyncLoad = NativeSceneManager.UnloadSceneAsync(sceneName);
                if (asyncLoad is null)
                {
                    throw new InvalidProgramException($"Failed to UnloadSceneAsync: {sceneName}");
                }
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
            }

            // Unload か完了したら毎回Resouce を開放する
            // https://scrapbox.io/edanoue/Unity_で_現在のシーンをアンロードしたらどうなるの
            {
                var asyncLoad = UnityEngine.Resources.UnloadUnusedAssets();
                if (asyncLoad is null)
                {
                    throw new InvalidProgramException($"Failed to UnloadUnusedAssets");
                }
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
            }
        }

        /// <summary>
        /// BuildSettings内にシーンが含まれているかどうか
        /// </summary>
        /// <note>
        /// 大文字小文字は無視されます
        /// </note>
        /// <param name="sceneName">検索するシーンの名前</param>
        /// <returns>見つかった場合true</returns>
        internal static bool IsSceneIncludedInBuildSettings(in string sceneName)
        {
            // 検索対象の sceneName を小文字(Lower case)にする
            var searchSceneNameLower = sceneName.ToLower();

            // BuildSettings 内に含まれているシーンの一覧を取得する
            GetSceneListInBuildSettings(out var sceneList);

            foreach (var sceneInfo in sceneList)
            {
                if (sceneInfo.Name.ToLower() == searchSceneNameLower)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 現在ロード済みのシーンかどうかを確認する
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        internal static bool IsSceneLoaded(in string sceneName)
        {
            // SceneManagerのGet系の関数はロード済みのシーンからしか取得できないため,
            // それを利用して ロード中かどうかを確認する
            return NativeSceneManager.GetSceneByName(sceneName).IsValid();
        }

        #region 内部処理用

        /// <summary>
        /// Build Settings に登録されているシーンをキャッシュする関数
        /// </summary>
        private static void GetSceneListInBuildSettings(out List<NativeSceneInfo> sceneList)
        {
            // Build Settings に含まれているシーンの総数を取得する
            int sceneCount = NativeSceneManager.sceneCountInBuildSettings;

            // Output用の配列を初期化
            sceneList = new List<NativeSceneInfo>();

            // Build Settings 内にある各シーンごとに
            for (int buildIndex = 0; buildIndex < sceneCount; buildIndex++)
            {
                // Build Settings に登録されている Sceneのパスを取得
                // Assets/Foo/Scenes/Main.unity
                // のような形式で取得できる
                string scenePath = NativeSceneUtility.GetScenePathByBuildIndex(buildIndex);

                // SceneInfo を作成
                var sceneInfo = new NativeSceneInfo(buildIndex, scenePath);

                // すでに登録済みならエラーをだす
                if (sceneList.Contains(sceneInfo))
                {
                    throw new InvalidProgramException($"同名のシーンが存在しています {scenePath}");
                }

                // Output 用の配列に追加する
                sceneList.Add(sceneInfo);
            }
        }

        #endregion
    }
}
