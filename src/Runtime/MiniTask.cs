using System;
using System.Runtime.CompilerServices;

namespace Kamahir0.Threading.Tasks
{
    /// <summary>
    /// シンプルな Task-like 構造体。
    /// 
    /// async/awaitを使用可能にするためには、以下の要件を満たす必要があります：
    /// 1. GetAwaiter()メソッドを持つ
    /// 2. AsyncMethodBuilder属性で専用のビルダーを指定する
    /// </summary>
    [AsyncMethodBuilder(typeof(AsyncMiniTaskMethodBuilder))]
    public readonly partial struct MiniTask
    {
        // 実際の非同期処理ロジックを持つオブジェクト
        // これにより、MiniTask自体は軽量なハンドルとして機能します
        private readonly IMiniTaskSource _source;

        public MiniTask(IMiniTaskSource source) => _source = source;

        /// <summary>
        /// C# コンパイラがawait式を処理する際に自動的に呼び出されるメソッド。
        /// このメソッドが返すAwaiterが、実際のawait処理を担当します。
        /// </summary>
        public Awaiter GetAwaiter() => new(_source);

        /// <summary>
        /// await可能にするためのAwaiter構造体。
        /// 
        /// Awaiterパターンの3つの必須要素：
        /// 1. IsCompleted プロパティ: 処理完了状態の確認
        /// 2. GetResult() メソッド: 結果の取得と例外の再スロー
        /// 3. OnCompleted() メソッド: 完了時のコールバック登録
        /// 
        /// INotifyCompletionインターフェースの実装により、
        /// コンパイラが生成する状態機械との連携が可能になります。
        /// </summary>
        public readonly struct Awaiter : INotifyCompletion
        {
            private readonly IMiniTaskSource _source;

            public Awaiter(IMiniTaskSource source) => _source = source;

            /// <summary>
            /// 非同期処理が既に完了しているかどうかを示します。
            /// 
            /// trueの場合：await時に同期的に結果を返す（スレッドの切り替えなし）
            /// falseの場合：OnCompleted()で継続処理を登録し、後で非同期に完了する
            /// 
            /// この最適化により、既に完了している処理に対する不要なコールバック処理を避けられます。
            /// </summary>
            public bool IsCompleted => _source.GetStatus() != MiniTaskStatus.Pending;

            /// <summary>
            /// 非同期処理の結果を取得します。
            /// 
            /// 正常完了の場合：何も返さない（void）
            /// 例外発生の場合：元の例外を再スローして呼び出し元に伝える
            /// 
            /// このメソッドは処理が完了してから呼ばれることが前提です。
            /// 未完了の状態で呼ばれた場合の動作は実装依存です。
            /// </summary>
            public void GetResult() => _source.GetResult();

            /// <summary>
            /// 非同期処理完了時に実行する継続処理（コールバック）を登録します。
            /// 
            /// このメソッドがasync/awaitの核心部分です：
            /// - 処理が未完了の場合、continuationが後で呼ばれるよう登録される
            /// - 処理完了時にcontinuationが実行され、awaitの後の処理が継続される
            /// - continuationは通常、コンパイラが生成した状態機械のMoveNext()メソッド
            /// 
            /// これにより、await後の処理が適切なタイミングで再開されます。
            /// </summary>
            public void OnCompleted(Action continuation) => _source.OnCompleted(continuation);
        }
    }

    /// <summary>
    /// 結果値を返すバージョンのMiniTask。
    /// 
    /// 非同期処理の完了時に特定の型の結果を返します。
    /// ジェネリック型パラメータTが戻り値の型を表します。
    /// </summary>
    [AsyncMethodBuilder(typeof(AsyncMiniTaskMethodBuilder<>))]
    public readonly struct MiniTask<T>
    {
        // 結果を返すバージョンのSource
        private readonly IMiniTaskSource<T> _source;

        public MiniTask(IMiniTaskSource<T> source) => _source = source;

        /// <summary>
        /// 結果を返すMiniTask用のGetAwaiter。
        /// 型付きのAwaiterを返します。
        /// </summary>
        public Awaiter GetAwaiter() => new(_source);

        /// <summary>
        /// 結果を返すMiniTask用のAwaiter。
        /// 
        /// 基本的な仕組みは非ジェネリック版と同じですが、
        /// GetResult()メソッドが具体的な型Tの値を返す点が異なります。
        /// </summary>
        public readonly struct Awaiter : INotifyCompletion
        {
            private readonly IMiniTaskSource<T> _source;

            public Awaiter(IMiniTaskSource<T> source) => _source = source;

            /// <summary>
            /// 処理完了状態の確認。非ジェネリック版と同じロジックです。
            /// </summary>
            public bool IsCompleted => _source.GetStatus() != MiniTaskStatus.Pending;

            /// <summary>
            /// 非同期処理の結果をT型として取得します。
            /// 
            /// 例: await SomeAsync() の結果がstring型の場合、
            /// このメソッドがそのstring値を返します。
            /// 例外が発生していた場合は、ここで再スローされます。
            /// </summary>
            public T GetResult() => _source.GetResult();

            /// <summary>
            /// 継続処理の登録。非ジェネリック版と同じロジックです。
            /// </summary>
            public void OnCompleted(Action continuation) => _source.OnCompleted(continuation);
        }
    }
}