#nullable enable

using System.Collections.Generic;
using UnityEditor;
using UnityEngine.TestTools;

namespace Edanoue.EH.Scene.Tests
{
    /// <summary>
    /// このアセンブリのビルド前, ビルド後の処理を定義しているクラス
    /// UTF の IPrebuildSetup と IPostBuildCleanup を実装している
    /// テスト実行中のみ BuildSettings に テスト用のシーンを追加する
    /// </summary>
    public class PreBuildAndPostBuildProcess : IPrebuildSetup, IPostBuildCleanup
    {

        // BuildSettings に シーンを追加する内部用の関数
        private static void AddSceneToBuildSettings(string scenePath)
        {
            // 現在BuildSettings 内にあるシーンのリストを取得
            var editorBuildSettingsScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            // すでに同一のパスが含まれていたらリターンする
            foreach (var editorBuildSettingsScene in editorBuildSettingsScenes)
            {
                if (scenePath == editorBuildSettingsScene.path)
                {
                    return;
                }
            }
            // 今回作成する EditorBuildSettingsScene のオブジェクトを作成
            var scene = new EditorBuildSettingsScene(scenePath, true);
            // BuildSettings を保存する
            editorBuildSettingsScenes.Add(scene);
            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();

            // 保存しないでも有効化される
            // AssetDatabase.SaveAssets();
        }

        // BuildSettings からシーンを削除する内部用の関数
        private static void RemoveSceneFromBuildSettings(string scenePath)
        {
            // 現在BuildSettings 内にあるシーンのリストを取得
            var editorBuildSettingsScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            // 同一のパスが含まれていたら削除して保存する
            // 配列を回しながら削除を行うため, 後ろからループを回す
            var i = editorBuildSettingsScenes.Count - 1;
            for (; i >= 0; i--)
            {
                var editorBuildSettingsScene = editorBuildSettingsScenes[i];
                if (scenePath == editorBuildSettingsScene.path)
                {
                    editorBuildSettingsScenes.Remove(editorBuildSettingsScene);
                    // BuildSettings を保存する
                    EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
                    // AssetDatabase.SaveAssets();
                }
            }
        }

        // BuildSettings に テスト用のシーンをまとめて追加する内部用の関数
        private static void AddTestScenesToBuildSettings()
        {
            foreach (var path in new string[] { TestSceneInfo.A1_path, TestSceneInfo.A2_path, TestSceneInfo.B1_path, TestSceneInfo.B2_path })
            {
                AddSceneToBuildSettings(path);
            }
        }

        // BuildSettings から テスト用のシーンをまとめて削除する内部用の関数
        private static void RemoveTestScenesFromBuildSettings()
        {
            foreach (var path in new string[] { TestSceneInfo.A1_path, TestSceneInfo.A2_path, TestSceneInfo.B1_path, TestSceneInfo.B2_path })
            {
                RemoveSceneFromBuildSettings(path);
            }
        }

        // テスト用のシーンの情報


        // Implement this method to call actions automatically before the build process.
        void IPrebuildSetup.Setup()
        {
#if UNITY_EDITOR
            // テスト用のシーンはBuildSettings に含まれていないため
            // このテストケースで一旦追加を行ってしまう
            // IPrebuildSetup のインタフェース内で呼ぶことで, テスト実行前のビルド段階で一度だけ実行がされる
            AddTestScenesToBuildSettings();
#endif
        }

        void IPostBuildCleanup.Cleanup()
        {
#if UNITY_EDITOR
            // 追加した BuildSettingsを削除する
            RemoveTestScenesFromBuildSettings();
#endif
        }
    }

}
