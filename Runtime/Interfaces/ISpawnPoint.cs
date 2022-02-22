#nullable enable

using UnityEngine;

namespace Edanoue.EH.Scene
{
    /// <summary>
    /// Scene ロード後のスポーン位置候補が実装すべきインタフェース
    /// </summary>
    public interface ISpawnPoint
    {
        /// <summary>
        /// シーン内で一意な名前を取得
        /// </summary>
        /// <note>
        /// 重複した場合, シーン内検索で最初に見つかったものが使用されます
        /// </note>
        /// <value></value>
        string Name { get; }

        /// <summary>
        /// ワールド座標においての位置を取得
        /// </summary>
        /// <value></value>
        Vector3 Position { get; }

        /// <summary>
        /// ワールド座標においての回転を取得
        /// </summary>
        /// <value></value>
        Quaternion Rotation { get; }
    }
}
