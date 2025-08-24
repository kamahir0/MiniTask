using System;

namespace Kamahir0.Threading.Tasks
{
    partial struct MiniTask
    {
        /// <summary>
        /// 指定された例外で即座に失敗する MiniTask を返します
        /// </summary>
        public static MiniTask FromException(Exception exception)
        {
            return new MiniTask(new FaultedMiniTaskSource(exception));
        }

        /// <summary>
        /// 指定された例外で即座に失敗する MiniTask を返します
        /// </summary>
        public static MiniTask<T> FromException<T>(Exception exception)
        {
            return new MiniTask<T>(new FaultedMiniTaskSource<T>(exception));
        }
    }

    /// <summary>
    /// 例外で即座に失敗する MiniTaskSource
    /// </summary>
    internal class FaultedMiniTaskSource : IMiniTaskSource
    {
        private readonly Exception _exception;

        public FaultedMiniTaskSource(Exception exception)
        {
            _exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }

        public MiniTaskStatus GetStatus() => MiniTaskStatus.Faulted;
        public void OnCompleted(Action continuation) => continuation?.Invoke();
        public void GetResult() => throw _exception;
    }

    /// <summary>
    /// 例外で即座に失敗する MiniTaskSource
    /// </summary>
    internal class FaultedMiniTaskSource<T> : IMiniTaskSource<T>
    {
        private readonly Exception _exception;

        public FaultedMiniTaskSource(Exception exception)
        {
            _exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }

        public MiniTaskStatus GetStatus() => MiniTaskStatus.Faulted;
        public void OnCompleted(Action continuation) => continuation?.Invoke();
        public T GetResult() => throw _exception;
    }
}