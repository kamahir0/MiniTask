namespace Kamahir0.Threading.Tasks
{
    /// <summary>
    /// asyncメソッドビルダーが内部的に使用するSource。
    /// </summary>
    internal sealed class ManualResetMiniTaskSource : MiniTaskSourceBase, IMiniTaskSource
    {
        // 外部からTrySetResult/TrySetExceptionを呼ぶ
    }

    internal sealed class ManualResetMiniTaskSource<T> : MiniTaskSourceBase, IMiniTaskSource<T>
    {
        private T _result;

        public new T GetResult()
        {
            base.GetResult(); // for exception
            return _result;
        }

        public void TrySetResult(T result)
        {
            _result = result;
            base.TrySetResult();
        }
    }
}