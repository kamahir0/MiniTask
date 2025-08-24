using System.Threading;

namespace Kamahir0.Threading.Tasks
{
    partial struct MiniTask
    {
        /// <summary>
        /// 指定した時間(ミリ秒)だけ待機する MiniTask を返します
        /// </summary>
        public static MiniTask Delay(int milliseconds)
        {
            var source = new DelayMiniTaskSource(milliseconds);
            return new MiniTask(source);
        }
    }

    /// <summary>
    /// Delay のロジックを実装した MiniTaskSource
    /// </summary>
    internal sealed class DelayMiniTaskSource : MiniTaskSourceBase, IMiniTaskSource
    {
        private readonly SynchronizationContext _capturedContext;
        private Timer _timer;

        public DelayMiniTaskSource(int milliseconds)
        {
            // 完了時に元のコンテキストに復帰して継続処理を行うために SynchronizationContext をキャプチャ
            _capturedContext = SynchronizationContext.Current;

            // 指定時間後に一度だけコールバックを実行するタイマーを作成
            _timer = new Timer(state =>
            {
                var self = (DelayMiniTaskSource)state;
                var capturedContext = self._capturedContext;

                // タイマーのコールバックはスレッドプールで実行される
                if (capturedContext != null)
                {
                    // 元のコンテキスト(UIスレッドなど)があれば、そこで完了処理を行う
                    capturedContext.Post(_ => TrySetResult(), null);
                }
                else
                {
                    // コンテキストがなければ、スレッドプールのスレッドでそのまま完了させる
                    TrySetResult();
                }

                // 一度実行されたらタイマーは不要なので破棄する
                self._timer?.Dispose();
                self._timer = null;

            }, this, milliseconds, Timeout.Infinite);
        }
    }
}