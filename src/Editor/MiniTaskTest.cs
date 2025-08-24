using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using Debug = UnityEngine.Debug;
using Kamahir0.Threading.Tasks;

namespace Kamahir0.Threading.Tasks.Test
{
    public class MiniTaskTest
    {
        [Test]
        public async Task RunAsync()
        {
            Debug.Log("--- MiniTask Demo Start ---");

            // 1. Delayのテスト
            await TestDelay();

            Debug.Log("----------------------------");

            // 2. WaitForUntilのテスト
            await TestWaitForUntil();

            Debug.Log("----------------------------");

            // 3. WhenAllのテスト
            await TestWhenAll();

            Debug.Log("----------------------------");

            // 4. 結果を返すasyncメソッドのテスト
            await TestReturnValue();

            Debug.Log("----------------------------");

            // 5. 例外処理のテスト
            await TestExceptionHandling();

            Debug.Log("--- MiniTask Demo End ---");
        }

        private static async MiniTask TestDelay()
        {
            Debug.Log("[TestDelay] 開始。1秒待ちます。");
            var sw = Stopwatch.StartNew();
            await MiniTask.Delay(1000);
            sw.Stop();
            Debug.Log($"[TestDelay] 完了。経過時間: {sw.ElapsedMilliseconds}ms");
        }

        private static async MiniTask TestWaitForUntil()
        {
            Debug.Log("[TestWaitForUntil] 開始。3秒後にtrueになる条件を待ちます。");
            var startTime = DateTime.UtcNow;
            await MiniTask.WaitForUntil(() => (DateTime.UtcNow - startTime).TotalSeconds >= 3);
            Debug.Log($"[TestWaitForUntil] 条件達成、完了しました。");
        }

        private static async MiniTask TestWhenAll()
        {
            Debug.Log("[TestWhenAll] 開始。1秒、1.5秒、0.5秒待つタスクを並行して実行します。");
            var sw = Stopwatch.StartNew();

            var task1 = MiniTask.Delay(1000);
            var task2 = MiniTask.Delay(1500);
            var task3 = MiniTask.Delay(500);

            await MiniTask.WhenAll(task1, task2, task3);

            sw.Stop();
            Debug.Log($"[TestWhenAll] すべてのタスクが完了。一番長いタスクは約1.5秒なので、それに近い時間になるはず。");
            Debug.Log($"[TestWhenAll] 経過時間: {sw.ElapsedMilliseconds}ms");
        }

        // MiniTask<T> を返す async メソッド
        private static async MiniTask<string> GetMessageAsync()
        {
            Debug.Log("[GetMessageAsync] 2秒かけてメッセージを取得します...");
            var sw = Stopwatch.StartNew();
            await MiniTask.Delay(2000);
            sw.Stop();
            Debug.Log($"[GetMessageAsync] メッセージ取得完了。経過時間: {sw.ElapsedMilliseconds}ms");

            // string を返す
            return $"Hello world! taken {sw.ElapsedMilliseconds}ms";
        }

        private static async MiniTask TestReturnValue()
        {
            Debug.Log("[TestReturnValue] 開始。");
            string message = await GetMessageAsync();
            Debug.Log($"[TestReturnValue] 受け取ったメッセージ: '{message}'");
        }

        private static async MiniTask TestExceptionHandling()
        {
            Debug.Log("[TestExceptionHandling] 開始。例外を投げるメソッドを呼び出します。");
            try
            {
                await MiniTask.FromException(new InvalidOperationException("これはテスト用の例外です。"));
                Debug.Log("[TestExceptionHandling] この行は表示されません。");
            }
            catch (InvalidOperationException ex)
            {
                Debug.Log($"[TestExceptionHandling] 期待通り例外をキャッチしました: {ex.Message}");
            }
        }
    }
}