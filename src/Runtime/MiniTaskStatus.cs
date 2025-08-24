namespace Kamahir0.Threading.Tasks
{
    /// <summary>
    /// MiniTaskの完了状態を表す列挙型。
    /// </summary>
    public enum MiniTaskStatus
    {
        /// <summary>実行中</summary>
        Pending,
        // <summary>正常に完了</summary>
        Succeeded,
        /// <summary>例外で完了</summary>
        Faulted,
        /// <summary>キャンセルされた</summary>
        Canceled
    }
}