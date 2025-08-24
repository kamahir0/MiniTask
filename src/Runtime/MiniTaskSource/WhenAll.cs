using System;
using System.Threading;
using ZLinq;

namespace Kamahir0.Threading.Tasks
{
    partial struct MiniTask
    {
        /// <summary>
        /// すべての MiniTask が完了するまで待機します
        /// </summary>
        public static MiniTask WhenAll(params MiniTask[] tasks)
        {
            var sources = tasks.AsValueEnumerable().Select(t => t._source).ToArray();
            var source = new WhenAllMiniTaskSource(sources);
            return new MiniTask(source);
        }
    }

    /// <summary>
    /// WhenAll のロジックを実装した MiniTaskSource。
    /// </summary>
    internal sealed class WhenAllMiniTaskSource : MiniTaskSourceBase, IMiniTaskSource
    {
        private int _completedCount;
        private readonly int _taskCount;

        public WhenAllMiniTaskSource(IMiniTaskSource[] sources)
        {
            _taskCount = sources.Length;
            if (_taskCount == 0)
            {
                TrySetResult();
                return;
            }

            foreach (var source in sources)
            {
                source.OnCompleted(() =>
                {
                    if (source.GetStatus() == MiniTaskStatus.Faulted)
                    {
                        // いずれか一つでも失敗したら、全体を失敗させる
                        try
                        {
                            source.GetResult();
                        }
                        catch (Exception ex)
                        {
                            TrySetException(ex);
                        }
                    }
                    else
                    {
                        // 完了したTaskの数を数える
                        if (Interlocked.Increment(ref _completedCount) == _taskCount)
                        {
                            TrySetResult();
                        }
                    }
                });
            }
        }
    }
}