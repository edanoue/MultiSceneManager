#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Edanoue.EH.Scene
{

    /// <summary>
    /// Sceneのローディング状況のフラグ
    /// </summary>
    [Flags]
    public enum SceneLoadingPhase : byte
    {
        // 何もシーンロードをしていない
        DoNotAnything,

        // 次のシーンをロードしている
        NextSceneLoading,
        // 次のシーンのロード完了
        NextSceneLoaded,
        // 以前のシーンをアンロードしている
        PrevSceneUnLoading,
        // 以前のシーンのアンロード完了
        PrevSceneUnLoaded,
        // Pawn の SpawnPointへの移動を開始
        SpawnPointMoving,

        // Pawn の SpawnPointへの移動が完了
        SpawnPointMoved,
    }

    /// <summary>
    /// TsuguVRにおいて, Unityが提供するSceneManager の代わりに使用されるクラスです
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// SceneBase を継承している全ての Type を取得 します
        /// Application が参照している全てのアセンブリから検索を行います
        /// これは将来, 追加アップデートや MOD などでもここで取得が行えるということです
        /// とても重い関数かつ結果が更新されないので, 起動直後などでキャッシュをとってください
        /// </summary>
        /// <note>
        /// エディタ拡張でシーンの一覧を選択したりするなどの用途を想定しています
        /// </note>
        /// <param name="result">SceneBase を継承している Type の一覧</param>
        public static void GetAllSceneTypes(out List<Type> result)
        {
            // make result array
            result = new List<Type>();

            // get SceneBase type object
            var searchType = typeof(SceneBase);

            // このアプリケーションが参照している全てのアセンブリを取得する
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                // アセンブリに含まれる全ての Type の配列を取得
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    // SceneBase を継承しておりかつ abstract ではないならば
                    if (type.IsSubclassOf(searchType) && !type.IsAbstract)
                    {
                        // 結果に加える
                        result.Add(type);
                    }
                }
            }
        }

        /// <summary>
        /// 現在ロード中の SceneAsset (.unity) の name の一覧を取得
        /// name というのは assets/path/foo.unity のばあい foo になります
        /// </summary>
        /// <param name="loadedSceneNames"></param>
        public static void GetLoadedSceneNames(out List<string> loadedSceneNames)
        {
            // 出力する配列を初期化する
            loadedSceneNames = new List<string>();

            // Unity の SceneManager を利用して, 現在ロード済みシーンの総数を取得
            int countLoaded = UnityEngine.SceneManagement.SceneManager.sceneCount;

            for (int i = 0; i < countLoaded; i++)
            {
                // 現在ロード済みのシーン参照を取得する
                var loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);

                // シーンの名前を取得して結果に代入する
                loadedSceneNames.Add(loadedScene.name);
            }
        }

        /// <summary>
        /// 現在ロード中の Scene 構造体の一覧を取得
        /// </summary>
        /// <param name="loadedScenes"></param>
        public static void GetLoadedScenes(out List<UnityEngine.SceneManagement.Scene> loadedScenes)
        {
            // 出力する配列を初期化する
            loadedScenes = new List<UnityEngine.SceneManagement.Scene>();

            // Unity の SceneManager を利用して, 現在ロード済みシーンの総数を取得
            int countLoaded = UnityEngine.SceneManagement.SceneManager.sceneCount;

            for (int i = 0; i < countLoaded; i++)
            {
                // 現在ロード済みのシーン参照を取得する
                var loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);

                // シーンを結果に代入する
                loadedScenes.Add(loadedScene);
            }
        }

        /// <summary>
        /// シーンを移動します
        /// </summary>
        /// <note>
        /// この関数自体はすぐにReturn されて, 内部で Unity のコルーチンが実行されます
        /// </note>
        /// <param name="spawnPointName"></param>
        /// <param name="setPositonCallback"></param>
        /// <param name="loadOption"></param>
        /// <typeparam name="TScene"></typeparam>
        public static Coroutine MoveTo<TScene>(
            string spawnPointName = "Default",
            Action<Vector3, Quaternion>? setPositonCallback = null,
            SceneLoadOption loadOption = default
        )
            where TScene : SceneBase, new()
        {
            // 内部の関数でシーンロードのコルーチンを作成する
            var coroutine = MoveSceneToAsyncInternal<TScene>(spawnPointName, setPositonCallback, loadOption);

            // Static な コルーチン (DontOnDestroy上にあるGameObject) で ロードを実行
            return CoroutineHandler.StartStaticCoroutine(coroutine);
        }

        /// <summary>
        /// シーンを移動します
        /// </summary>
        /// <note>
        /// この関数自体はすぐにReturn されて, 内部で Unity のコルーチンが実行されます
        /// </note>
        /// <param name="spawnPointName"></param>
        /// <param name="setPositonCallback"></param>
        /// <param name="loadOption"></param>
        /// <typeparam name="TScene"></typeparam>
        public static Coroutine MoveTo(
            SceneBase sceneInfo,
            string spawnPointName = "Default",
            Action<Vector3, Quaternion>? setPositonCallback = null,
            SceneLoadOption loadOption = default
        )
        {
            // 内部の関数でシーンロードのコルーチンを作成する
            var coroutine = MoveSceneToAsyncInternal(sceneInfo, spawnPointName, setPositonCallback, loadOption);

            // Static な コルーチン (DontOnDestroy上にあるGameObject) で ロードを実行
            return CoroutineHandler.StartStaticCoroutine(coroutine);
        }

        /// <summary>
        /// シーンを移動します
        /// </summary>
        /// <param name="context">UnityのCoroutineを実行するonoBehaviourのContextを代入</param>
        /// <param name="spawnPointName">移動先のシーン内に含まれる SpawnPoint の名前, 見つからなかった場合原点に</param>
        /// <param name="setPositonCallback">ロード完了後に呼ばれる, 見つかったSpawnPointの位置が代入されるコールバック</param>
        /// <param name="loadOption">シーンロードに渡すオプション</param>
        /// <typeparam name="TScene"></typeparam>
        private static IEnumerator MoveSceneToAsyncInternal<TScene>(
            string spawnPointName,
            Action<Vector3, Quaternion>? setPositonCallback,
            SceneLoadOption loadOption
        )
            where TScene : SceneBase, new()
        {
            // シーンのインスタンスを作成
            var sceneInfo = new TScene();

            yield return MoveSceneToAsyncInternal(sceneInfo, spawnPointName, setPositonCallback, loadOption);
        }

        /// <summary>
        /// シーンを移動します
        /// </summary>
        /// <param name="sceneInfo">SceneBase 継承のシーン情報を保持しているインスタンス</param>
        /// <param name="spawnPointName">移動先のシーン内に含まれる SpawnPoint の名前, 見つからなかった場合原点に</param>
        /// <param name="setPositonCallback">ロード完了後に呼ばれる, 見つかったSpawnPointの位置が代入されるコールバック</param>
        /// <param name="loadOption">シーンロードに渡すオプション</param>
        /// <returns></returns>
        private static IEnumerator MoveSceneToAsyncInternal(
            SceneBase sceneInfo,
            string spawnPointName,
            Action<Vector3, Quaternion>? setPositonCallback,
            SceneLoadOption loadOption
        )
        {
            // 現在すでにロード処理が走っていれば
            if (IsLoadingScene)
            {
                // スキップする
                Debug.LogWarning("現在すでにローディング中です, シーンロードを中止します");
                yield break;
            }

            // 現在のシーンと同じシーンであれば
            if (sceneInfo.Equals(CurrentLoadingScene))
            {
                // ロード処理をスキップする
                Debug.LogWarning("現在すでにロード中のシーンです, シーンロードを中止します");
                yield break;
            }

            // --------------------
            // シーンロードを開始する
            // --------------------

            // Phase を設定する: 次のシーンをロード中
            LoadingPhase = SceneLoadingPhase.NextSceneLoading;

            // いちどローディング用のシーンを読み込む
            // 暗転などの演出
            // TODO: 未実装

            // ロードするシーンの名前のリストを取得
            var nextSceneNames = new List<string>(sceneInfo.SceneNameList);

            // このあとアンロードするシーンのリスト用の変数を作成
            var unloadingSceneNames = new List<string>();

            // はじめに, 現在ロード済みのシーンがあるかどうかを確認する
            // すでにロード済みのシーンはそのままにしておき, それ以外はアンロードを行う
            {
                GetLoadedSceneNames(out var loadedSceneNames);
                foreach (var loadedSceneName in loadedSceneNames)
                {
                    // 次のシーンリストに含まれていないならば
                    if (!nextSceneNames.Contains(loadedSceneName))
                    {
                        // 今回アンロードするシーンの一覧に入れる
                        unloadingSceneNames.Add(loadedSceneName);
                    }
                }
            }

            Debug.Log($"----- Start to move {sceneInfo.GetType().Name} -----");

            // 常に加算で(Additiveで)ローディングを行います
            {
                bool IsFirstScene = true;
                foreach (var loadingSceneName in nextSceneNames)
                {
                    Debug.Log($"Start to load {loadingSceneName} ...");

                    // シーンのロード と Active 化 (表示) を同時に行う
                    yield return Loader.LoadSceneAddressableAsync(loadingSceneName, LoadSceneMode.Additive, loadOption);

                    Debug.Log($"Complete to load {loadingSceneName}");

                    if (IsFirstScene)
                    {
                        // 最初のシーンはすでにロード済みである
                        // ロード済みのSceneの参照を貰う
                        var firstScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(loadingSceneName);

                        // 最初のシーンであれば, Active に設定をする
                        UnityEngine.SceneManagement.SceneManager.SetActiveScene(firstScene);

                        Debug.Log($"Set active scene: {loadingSceneName}");
                        IsFirstScene = false;
                    }
                }
            }

            // 全てのシーンのロードが完了した
            // Phase を設定する: すべてのシーンのロード完了
            LoadingPhase = SceneLoadingPhase.NextSceneLoaded;


            // -----------------------------
            // 現在読み込んでいるシーンのアンローディングを行う
            // -----------------------------
            // ロードの後にアンロードをする理由:
            // Unity は 一つもScene を読み込んでいない状態 にはできないため, このような順序を取る必要がある

            // Phase を設定する: いらないシーンのアンロード開始
            LoadingPhase = SceneLoadingPhase.PrevSceneUnLoading;

            // 加算ロードが終わったタイミングでアンロードを開始する
            {
                foreach (var unloadingSceneName in unloadingSceneNames)
                {
                    // 南: めちゃくちゃ良くないのですが, テストシーンの場合は絶対にアンロードしてはいけない(できない)
                    // ために無視するような設定を入れています
                    if (unloadingSceneName.StartsWith("InitTestScene"))
                    {
                        continue;
                    }

                    Debug.Log($"Start to unload {unloadingSceneName} ...");

                    // シーンのアンローディングを行う
                    yield return Loader.UnloadSceneAsync(unloadingSceneName);

                    Debug.Log($"Complete to unload {unloadingSceneName}");
                }
            }

            // 全てのシーンのアンローディングが完了した

            // Phase を設定する: すべてのシーンのアンローディングが完了した
            LoadingPhase = SceneLoadingPhase.PrevSceneUnLoaded;

            // ----------------------------------------------------------------------
            // 全てのシーンのロード・アンロードが完了したので LightProbe の更新を行う
            // ----------------------------------------------------------------------

            // TetrahedralizeAsync() は特に完了を待つことができないため同期版のメソッドを使っています
            // もし遷移エフェクトへのチラツキなどが発生した場合は非同期への変更を再考する余地があります
            LightProbes.Tetrahedralize();

            // -----------------------------
            // 最後に Spawn Point の検索を行う
            // -----------------------------

            // Phase を設定する: SpawnPoint への移動を開始
            LoadingPhase = SceneLoadingPhase.SpawnPointMoving;

            // SpawnPoint移動のコールバックが設定されている場合
            if (setPositonCallback is not null)
            {
                // コールバック先で例外が発生する可能性がある
                try
                {
                    FindSpawnPointAndMoveIt(spawnPointName, setPositonCallback);
                }
                catch (Exception e)
                {
                    // SpawnPoint への 移動に失敗したが
                    // シーンロード自体は成功しているのでスキップする
                    Debug.LogWarning(e);
                }
            }

            // Phase を設定する: SpawnPoint への移動が完了
            LoadingPhase = SceneLoadingPhase.SpawnPointMoved;


            // ローディング用のシーンをアンロードする
            // 暗転などの演出, プレイヤーに対してロード終わったよ? ボタン表示する とか
            // TODO: 未実装

            // 現在ロード中のシーンを更新しておく
            CurrentLoadingScene = sceneInfo;

            // すべての処理が完了した
            // Phase を設定する: なにもしていない
            LoadingPhase = SceneLoadingPhase.DoNotAnything;

            Debug.Log($"----- Completed to move {sceneInfo.GetType().Name} -----");

        }

        /// <summary>
        /// 現在のシーンをリロードします
        /// </summary>
        /// <param name="spawnPoint"></param>
        public static void RestartAsync(string spawnPoint = "Default")
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// シーンを構成する UnityScene がロード済かどうかを取得する
        /// </summary>
        /// <param name="TScene">ロードされているか確認したいシーンのクラス</param>
        /// <returns>ロードされている場合は true を返す</returns>
        public static bool IsLoaded<TScene>()
            where TScene : SceneBase, new()
        {
            // 引数に指定された SceneInfo のインスタンスを生成する
            var sceneInfo = new TScene();

            // ロード済の UnityScene の名前の一覧を取得する
            GetLoadedSceneNames(out var loadedSceneNames);

            // 引数に与えられた SceneInfo を起点に, 現在読み込み済みのシーンを検索する
            foreach (var desireSceneName in sceneInfo.SceneNameList)
            {
                // SceneInfo のシーンがひとつでも読み込まれていない場合は false を返す
                if (!loadedSceneNames.Contains(desireSceneName))
                {
                    return false;
                }
            }

            // SceneInfo のシーンがすべて読み込まれている場合は true を返す
            return true;
        }

        #region 公開プロパティ

        /// <summary>
        /// 現在のSceneLoadingの状態を取得する
        /// </summary>
        /// <value></value>
        public static SceneLoadingPhase LoadingPhase
        {
            get
            {
                lock (s_loadingPhaseLock)
                    return s_loadingPhase;
            }
            private set
            {
                bool isChanged = false;
                lock (s_loadingPhaseLock)
                {
                    if (s_loadingPhase != value)
                    {
                        s_loadingPhase = value;
                        isChanged = true;
                    }
                }
                // 値が更新されていた場合コールバックを呼ぶ
                // ロックの外に出して置かなければデッドロックとなる
                if (isChanged)
                {
                    OnChangedLoadingPhase();
                }
            }
        }

        /// <summary>
        /// 現在シーンロード中かどうか
        /// </summary>
        public static bool IsLoadingScene => LoadingPhase != SceneLoadingPhase.DoNotAnything;

        /// <summary>
        /// 現在読込中のシーン情報
        /// </summary>
        public static SceneBase? CurrentLoadingScene
        {
            get
            {
                lock (s_currentLoadingSceneLock)
                    return s_currentLoadingScene;
            }
            private set
            {
                lock (s_currentLoadingSceneLock)
                    s_currentLoadingScene = value;
            }
        }

        #endregion

        #region 公開イベント

        public static event Action? LoadingScene;
        public static event Action? LoadedScene;
        public static event Action<SceneLoadingPhase>? ChangedLoadingPhase;

        #endregion

        #region 内部処理用

        // SceneLoadingPhase 用のロック
        private static readonly object s_loadingPhaseLock = new object();

        // SceneLoadingPhase の内部用の参照
        private static SceneLoadingPhase s_loadingPhase = SceneLoadingPhase.DoNotAnything;

        // 現在読込中のシーンの情報
        private static SceneBase? s_currentLoadingScene;

        // 現在読込中のシーンの情報 用のロック
        private static readonly object s_currentLoadingSceneLock = new object();

        private static void OnChangedLoadingPhase()
        {
            // 次のシーンロード開始時 になったら, シーンロードのかいし
            if (LoadingPhase == SceneLoadingPhase.NextSceneLoading)
            {
                LoadingScene?.Invoke();
            }
            // なにもしてない になったら, シーンロードの終了
            else if (LoadingPhase == SceneLoadingPhase.DoNotAnything)
            {
                LoadedScene?.Invoke();
            }

            // 常にPhase変更の通知は行う
            ChangedLoadingPhase?.Invoke(LoadingPhase);
        }

        private static void FindSpawnPointAndMoveIt(string spawnPointName, Action<Vector3, Quaternion> callback)
        {
            // 目的のSpawnTargetがみつかったかどうか
            bool IsFoundSpawnTarget = false;

            // 現在ロードしているシーンの一覧を取得する
            GetLoadedScenes(out var loadedScenes);

            // 現在ロードしているシーンごとにループ処理を行う
            foreach (var scene in loadedScenes)
            {
                // シーンのルートに平たく置かれているGameObjectを全て取得する
                // あくまでRootObjectsなので, 孫などにはアクセスすることができない
                var rootObjects = new List<GameObject>();
                scene.GetRootGameObjects(rootObjects);

                // シーン内から, ISpawnTargetを継承しているGameObject を全て検索する
                foreach (var gameObject in rootObjects)
                {
                    // GameObject 自体が ISpawnPoint を持っているかどうか?
                    {
                        var spawnTarget = gameObject.GetComponent<ISpawnPoint>();
                        if (!(spawnTarget is null))
                        {
                            // 名前の一致する SpawnPoint が見つかった
                            if (spawnTarget.Name == spawnPointName)
                            {
                                callback(spawnTarget.Position, spawnTarget.Rotation);
                                IsFoundSpawnTarget = true;
                                break;
                            }
                        }
                    }

                    // 孫を探す
                    var childs = gameObject.GetComponentsInChildren<ISpawnPoint>();
                    if (!(childs is null))
                    {
                        foreach (var spawnTarget in childs)
                        {
                            // 名前の一致する SpawnPoint が見つかった
                            if (spawnTarget.Name == spawnPointName)
                            {
                                callback(spawnTarget.Position, spawnTarget.Rotation);
                                IsFoundSpawnTarget = true;
                                break;
                            }
                        }
                    }

                }
                if (IsFoundSpawnTarget) break;
            }

            // 指定されたSpawnPointが見つからなかったばあい
            if (!IsFoundSpawnTarget)
            {
                Debug.LogWarning($"指定されたSpawnPointが見つかりませんでした: {spawnPointName}");
                callback(Vector3.zero, Quaternion.identity);
            }
        }

        #endregion

    }
}
