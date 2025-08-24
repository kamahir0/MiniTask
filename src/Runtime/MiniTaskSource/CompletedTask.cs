using System;

namespace Kamahir0.Threading.Tasks
{
    partial struct MiniTask
    {
        /// <summary>
        /// 即座に完了する MiniTask を返します
        /// </summary>
        public static readonly MiniTask CompletedTask = new(new CompletedMiniTaskSource());

        /// <summary>
        /// 指定の結果で即座に完了する MiniTask を返します
        /// </summary>
        public static MiniTask<T> FromResult<T>(T result) => new(new CompletedMiniTaskSource<T>(result));
    }

    /// <summary>
    /// 即座に完了する MiniTaskSource
    /// </summary>
    internal class CompletedMiniTaskSource : IMiniTaskSource
    {
        public MiniTaskStatus GetStatus() => MiniTaskStatus.Succeeded;
        public void OnCompleted(Action continuation) => continuation?.Invoke();
        public void GetResult() { }
    }

    /// <summary>
    /// 指定の結果で即座に完了する MiniTaskSource
    /// </summary>
    internal class CompletedMiniTaskSource<T> : IMiniTaskSource<T>
    {
        private readonly T _result;

        public CompletedMiniTaskSource(T result) => _result = result;

        public MiniTaskStatus GetStatus() => MiniTaskStatus.Succeeded;
        public void OnCompleted(Action continuation) => continuation?.Invoke();
        public T GetResult() => _result;
    }
}