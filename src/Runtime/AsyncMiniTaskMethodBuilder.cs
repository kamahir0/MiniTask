using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Kamahir0.Threading.Tasks
{
    /// <summary>
    /// C#コンパイラがasync MiniTaskメソッドを状態機械に変換する際に使用するビルダークラス。
    /// 
    /// asyncメソッドの内部では、コンパイラが以下の処理を自動生成します：
    /// 1. このビルダーのインスタンスを作成
    /// 2. 状態機械を初期化してStart()を呼ぶ
    /// 3. awaitポイントでAwaitOnCompleted()を呼んで継続処理を登録
    /// 4. 完了時にSetResult()またはSetException()を呼ぶ
    /// 
    /// これにより、複雑な非同期処理が同期的なコードのように書けるようになります。
    /// </summary>
    public class AsyncMiniTaskMethodBuilder
    {
        // asyncメソッド全体の状態と結果を管理するSource
        // null遅延初期化により、Taskプロパティが参照されるまでインスタンス化を遅らせる
        private ManualResetMiniTaskSource _source;

        /// <summary>
        /// コンパイラが最初に呼ぶメソッド。
        /// ビルダーインスタンスを作成します。
        /// </summary>
        public static AsyncMiniTaskMethodBuilder Create() => new();

        /// <summary>
        /// asyncメソッドの実行を開始します。
        /// 
        /// stateMachine は、コンパイラが生成した状態機械オブジェクトです。
        /// MoveNext()を呼ぶことで、最初のawaitポイントまで（または完了まで）処理が進みます。
        /// 
        /// refパラメータを使用することで、値型の状態機械のコピーを避け、
        /// パフォーマンスを向上させています。
        /// </summary>
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        /// <summary>
        /// awaitポイントで呼ばれ、非同期処理の継続を登録します。
        /// 
        /// awaiterが未完了の場合、OnCompleted()により状態機械のMoveNext()が
        /// 後で呼ばれるように設定されます。これにより、awaitした処理が完了した時に
        /// asyncメソッドの続きが実行されます。
        /// 
        /// 一般的なINotifyCompletion実装向けのメソッドです。
        /// </summary>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        /// <summary>
        /// パフォーマンス重視のawait処理用メソッド。
        /// 
        /// ICriticalNotifyCompletionを実装するawaiterに対して使用されます。
        /// UnsafeOnCompleted()は、ExecutionContextの保存/復元を行わないため、
        /// OnCompleted()より高速ですが、セキュリティコンテキストの引き継ぎが行われません。
        /// 
        /// コンパイラは可能な限りこちらの方を優先的に使用します。
        /// </summary>
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }

        /// <summary>
        /// asyncメソッドが正常完了した時に呼ばれます。
        /// 
        /// これにより、このMiniTaskをawaitしている他のコードに
        /// 「処理が完了した」ことが通知されます。
        /// 戻り値なしのasyncメソッドなので、結果は設定せず完了状態のみ設定します。
        /// </summary>
        public void SetResult()
        {
            _source?.TrySetResult();
        }

        /// <summary>
        /// asyncメソッド内で例外が発生した時に呼ばれます。
        /// 
        /// 例外情報を保存し、このMiniTaskをawaitしているコードで
        /// await時に同じ例外が再スローされるようにします。
        /// これによって、try-catch文での例外処理が正常に機能します。
        /// </summary>
        public void SetException(Exception exception)
        {
            _source?.TrySetException(exception);
        }

        /// <summary>
        /// デバッグ時にのみ呼ばれる、状態機械設定用メソッド。
        /// 
        /// 通常の実行時は何もしません。デバッガが状態機械の詳細情報を
        /// 取得する際に使用される可能性があります。
        /// 
        /// パフォーマンス上の理由から、boxingされた状態機械は使用しません。
        /// </summary>
        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // ボックス化された状態機械は使用しない（パフォーマンス上の理由）
        }

        /// <summary>
        /// このビルダーが管理するMiniTaskを取得します。
        /// 
        /// 遅延初期化により、実際にTaskが必要になるまで
        /// ManualResetMiniTaskSourceの作成を遅らせています。
        /// 
        /// 戻り値が必要ない場合（fire-and-forget）や、
        /// 同期的に完了する場合にメモリ使用量を削減できます。
        /// </summary>
        public MiniTask Task
        {
            get
            {
                _source ??= new ManualResetMiniTaskSource();
                return new MiniTask(_source);
            }
        }
    }

    /// <summary>
    /// 結果値を返すasync MiniTask&lt;T&gt;メソッド用のビルダークラス。
    /// 
    /// 基本的な動作は非ジェネリック版と同じですが、
    /// SetResult(T result)メソッドで具体的な結果値を受け取り、
    /// MiniTask&lt;T&gt;を返す点が異なります。
    /// 
    /// 例: async MiniTask&lt;string&gt; GetDataAsync() のようなメソッド用
    /// </summary>
    public class AsyncMiniTaskMethodBuilder<T>
    {
        // 結果値を持つバージョンのSource
        private ManualResetMiniTaskSource<T> _source;

        /// <summary>
        /// ジェネリック版ビルダーの作成。
        /// </summary>
        public static AsyncMiniTaskMethodBuilder<T> Create() => new();

        /// <summary>
        /// 状態機械の実行開始。非ジェネリック版と同じです。
        /// </summary>
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        /// <summary>
        /// 通常のawait継続処理。非ジェネリック版と同じです。
        /// </summary>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        /// <summary>
        /// 高速なawait継続処理。非ジェネリック版と同じです。
        /// </summary>
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }

        /// <summary>
        /// 結果値付きで正常完了します。
        /// 
        /// このメソッドが呼ばれると、awaitしているコードで
        /// await GetDataAsync() のような式から具体的な値が取得できるようになります。
        /// </summary>
        public void SetResult(T result)
        {
            _source?.TrySetResult(result);
        }

        /// <summary>
        /// 例外での完了処理。非ジェネリック版と同じです。
        /// </summary>
        public void SetException(Exception exception)
        {
            _source?.TrySetException(exception);
        }

        /// <summary>
        /// 状態機械設定（通常は使用されません）。
        /// </summary>
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // 実装不要
        }

        /// <summary>
        /// 結果を返すMiniTaskを取得します。
        /// 
        /// 遅延初期化により、必要になるまでSourceの作成を遅らせています。
        /// </summary>
        public MiniTask<T> Task
        {
            get
            {
                _source ??= new ManualResetMiniTaskSource<T>();
                return new MiniTask<T>(_source);
            }
        }
    }
}