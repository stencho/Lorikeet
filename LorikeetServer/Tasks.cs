using System.Runtime.CompilerServices;

namespace LorikeetServer;

public class Tasks {
    static int _task_count = 0;
    public static int TaskCount => _task_count;

    static void IncrementTaskCount() => Interlocked.Increment(ref _task_count);
    static void DecrementTaskCount() => Interlocked.Decrement(ref _task_count);

    internal static CancellationTokenSource cancellation_token_source = new CancellationTokenSource();
    internal static CancellationToken cancellation_token => cancellation_token_source.Token;

    public static void reset_cancellation_token() {
        cancellation_token_source = new CancellationTokenSource();
    }

    public static Task StartTask(Action action, [CallerFilePath] string callerfilename = "", [CallerMemberName] string membername = "") {
        Guid task_guid = Guid.NewGuid();

        return Task.Run(() => {
            IncrementTaskCount();
            try {
                action.Invoke();
            } finally {
                DecrementTaskCount();
            }
        }, cancellation_token).ContinueWith(t => {
            if (t.IsFaulted) {
                Logging.Error($"Task failed: ");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    public static Task StartTask(Action action, CancellationToken cancellation_token, [CallerFilePath] string callerfilename = "", [CallerMemberName] string membername = "") {
        Guid task_guid = Guid.NewGuid();

        return Task.Run(() => {
            IncrementTaskCount();
            try {
                action.Invoke();
            } finally {
                DecrementTaskCount();
            }
        }, cancellation_token).ContinueWith(t => {
            if (t.IsFaulted) {
                Logging.Error($"Task failed: ");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
    
}