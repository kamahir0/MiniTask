using System;
using System.Threading;

namespace Kamahir0.Threading.Tasks
{
    partial struct MiniTask
    {
        /// <summary>
        /// 指定した条件がtrueになるまで待機する MiniTask を返します。
        /// </summary>
        public static MiniTask WaitForUntil(Func<bool> predicate)
        {
            var source = new WaitForUntilMiniTaskSource(predicate);
            return new MiniTask(source);
        }
    }

    /// <summary>
    /// WaitForUntil のロジックを実装した MiniTaskSource
    /// </summary>
    internal sealed class WaitForUntilMiniTaskSource : MiniTaskSourceBase, IMiniTaskSource
    {
        private readonly Func<bool> _predicate;
        private readonly SynchronizationContext _context;

        public WaitForUntilMiniTaskSource(Func<bool> predicate)
        {
            _predicate = predicate;

            // 完了時に元のコンテキストに復帰して継続処理を行うために SynchronizationContext をキャプチャ
            _context = SynchronizationContext.Current ?? new SynchronizationContext();
            // 最初のチェックを開始
            _context.Post(CheckPredicate, null);
        }

        private void CheckPredicate(object state)
        {
            try
            {
                if (_predicate())
                {
                    TrySetResult();
                }
                else
                {
                    // 条件を満たしていなければ、再度チェックをスケジュールする
                    _context.Post(CheckPredicate, null);
                }
            }
            catch (Exception ex)
            {
                TrySetException(ex);
            }
        }
    }
}