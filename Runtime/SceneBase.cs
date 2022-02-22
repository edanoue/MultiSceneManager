#nullable enable

using System;

namespace Edanoue.EH.Scene
{
    /// <summary>
    /// TsuguVR における Scene を定義するベースクラス
    /// </summary>
    public abstract class SceneBase : IEquatable<SceneBase>
    {
        /// <summary>
        /// このシーンを構成する UnityScene の名前のリスト
        /// </summary>
        /// <note>
        /// シーンロード時に, 前から順番にロードが行われます
        /// </note>
        /// <value></value>
        internal protected abstract string[] SceneNameList { get; }

        #region IEquatable

        public override bool Equals(object obj) => this.Equals(obj as SceneBase);

        public bool Equals(SceneBase? p)
        {
            // キャストに失敗 あるいは null が渡された時
            if (p is null)
            {
                return false;
            }

            // 同じメモリアドレスのオブジェクトである
            if (Object.ReferenceEquals(this, p))
            {
                return true;
            }

            // 同じ型なら等しいとする
            if (this.GetType() == p.GetType())
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode() => GetType().GetHashCode();

        public static bool operator ==(SceneBase? lhs, SceneBase? rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(SceneBase? lhs, SceneBase? rhs) => !(lhs == rhs);

        #endregion

    }
}
