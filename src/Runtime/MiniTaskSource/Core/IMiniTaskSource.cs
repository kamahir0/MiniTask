using System;

namespace Kamahir0.Threading.Tasks
{
    /// <summary>
    /// MiniTaskの非同期操作のロジック本体を表すインターフェース。
    /// これを実装することで、DelayやWaitForUntilなどの具体的な処理をカプセル化します。
    /// </summary>
    public interface IMiniTaskSource
    {
        MiniTaskStatus GetStatus();
        void OnCompleted(Action continuation);
        void GetResult();
    }

    /// <summary>
    /// 結果を返すMiniTaskのロジック本体を表すインターフェース。
    /// </summary>
    public interface IMiniTaskSource<out T>
    {
        MiniTaskStatus GetStatus();
        void OnCompleted(Action continuation);
        T GetResult();
    }
}