#nullable enable

namespace Edanoue.EH.Scene
{
    // このnamespaceを呼ぶ側 がわざわざ using UnityEngine.SceneManagement をしないで良いように,
    // 同名のEnumを宣言している
    public enum LoadSceneMode
    {
        Single,
        Additive,
    }
}
