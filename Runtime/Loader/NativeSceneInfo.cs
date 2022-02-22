#nullable enable

using System;
using System.Text.RegularExpressions;

namespace Edanoue.EH.Scene
{

    /// <summary>
    /// Loader 内部で使用する専用の構造体
    /// 初回アクセス時にBuildSettings に存在するシーンの一覧をキャッシュします
    /// </summary>
    internal readonly struct NativeSceneInfo : IEquatable<NativeSceneInfo>
    {
        internal readonly int BuildIndex;
        internal readonly string Name;
        internal readonly string Path;

        internal NativeSceneInfo(int buildIndex, string path) : this()
        {
            if (buildIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(buildIndex), "0以上を指定してください");
            }

            BuildIndex = buildIndex;
            Path = path;
            GetSceneNameFromPath(Path, out Name);
        }

        private const string PATTERN = @"^Assets/(.+/)*(.+)\.unity$";
        private static void GetSceneNameFromPath(string path, out string name)
        {
            // Unityのシーンのパスからシーン名を抽出
            // 正規表現を使用
            // e.g. Assets/Foo/Scenes/Main.unity => Main
            // 正規表現は以下のページで確認できる
            // https://www.debuggex.com/r/RWPupBj7SHCx3ZzW
            var options = RegexOptions.IgnoreCase;
            var reg = new Regex(PATTERN, options);
            var m = reg.Match(path);
            if (m.Success)
            {
                var group_name = m.Groups[2];
                name = group_name.ToString();
            }
            else
            {
                throw new ArgumentException($"SceneAssetのパスから名前が取得できませんでした. {path}", nameof(path));
            }
        }

        #region IEquatable implements

        public override bool Equals(object obj) => obj is NativeSceneInfo other && this.Equals(other);

        public bool Equals(NativeSceneInfo p) => Path == p.Path;

        public override int GetHashCode() => Path.GetHashCode();

        public static bool operator ==(NativeSceneInfo lhs, NativeSceneInfo rhs) => lhs.Equals(rhs);

        public static bool operator !=(NativeSceneInfo lhs, NativeSceneInfo rhs) => !(lhs == rhs);

        #endregion
    }
}
