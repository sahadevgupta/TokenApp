namespace TokenApp.Models
{
    public enum FailureKind
    {
        None,
        /// <summary>No connectivity — nothing was sent.</summary>
        Offline,
        /// <summary>Request failed in transit (timeout, DNS, connection reset). Safe to retry.</summary>
        Network,
        /// <summary>401 — token missing/expired.</summary>
        Unauthorized,
        /// <summary>403 — insufficient privilege.</summary>
        Forbidden,
        /// <summary>422 — validation / business rule. Retrying the same request will not help.</summary>
        Validation,
        /// <summary>5xx. Retry later.</summary>
        Server,
        Unknown
    }

    public class ServiceResult
    {
        public bool IsSuccess { get; init; }
        public string? Message { get; init; }
        public FailureKind Failure { get; init; }

        /// <summary>True when retrying the same request later could succeed.</summary>
        public bool IsTransient => Failure is FailureKind.Offline or FailureKind.Network or FailureKind.Server;

        public static ServiceResult Ok(string? message = null) => new() { IsSuccess = true, Message = message };
        public static ServiceResult Fail(FailureKind kind, string? message) => new() { Failure = kind, Message = message };
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; init; }

        public static ServiceResult<T> Ok(T data, string? message = null) => new() { IsSuccess = true, Data = data, Message = message };
        public new static ServiceResult<T> Fail(FailureKind kind, string? message) => new() { Failure = kind, Message = message };
    }
}
