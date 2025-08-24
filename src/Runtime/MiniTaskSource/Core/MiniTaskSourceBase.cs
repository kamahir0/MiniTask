using System;

namespace Kamahir0.Threading.Tasks
{
    /// <summary>
    /// MiniTaskSourceの共通ロジックをまとめた抽象クラス。
    /// 継続処理の管理や状態遷移の基本ロジックを提供します。
    /// </summary>
    internal abstract class MiniTaskSourceBase
    {
        protected MiniTaskStatus _status = MiniTaskStatus.Pending;
        protected Action _continuation;
        protected Exception _exception;

        public MiniTaskStatus GetStatus() => _status;

        public void OnCompleted(Action continuation)
        {
            // 既に完了していたら即座に実行
            if (_status != MiniTaskStatus.Pending)
            {
                continuation?.Invoke();
            }
            else // 未完了なら後で実行するために保存
            {
                _continuation = continuation;
            }
        }

        public void TrySetResult()
        {
            if (_status != MiniTaskStatus.Pending) return;
            _status = MiniTaskStatus.Succeeded;
            // 完了したので、登録されていた継続処理を実行
            _continuation?.Invoke();
        }

        public void TrySetException(Exception ex)
        {
            if (_status != MiniTaskStatus.Pending) return;
            _status = MiniTaskStatus.Faulted;
            _exception = ex;
            _continuation?.Invoke();
        }

        public void GetResult()
        {
            if (_status == MiniTaskStatus.Faulted)
            {
                // 例外が発生していたら、ここでスローして呼び出し元に伝える
                throw _exception;
            }
        }
    }
}