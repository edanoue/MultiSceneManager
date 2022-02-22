#nullable enable

namespace Edanoue.EH.Scene
{
    /// <summary>
    /// シーンロード時のオプションの構造体
    /// </summary>
    public readonly struct SceneLoadOption
    {
        /// <summary>
        /// シーンの重複ロードを許可する
        /// </summary>
        public readonly bool IsAllowDuplicateLoad;

        public SceneLoadOption(bool isAllowDuplicateLoad)
        {
            IsAllowDuplicateLoad = isAllowDuplicateLoad;
        }
    }
}
