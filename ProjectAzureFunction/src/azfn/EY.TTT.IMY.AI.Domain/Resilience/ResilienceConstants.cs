using System.Net;

namespace EY.TTT.IMY.AI.Domain.Resilience
{
    public class ResilienceConstants
    {
        public const string RETRIES = "Retries";
        public const string TIMEOUT = "TimeoutSeconds";
        public const string RETRY_SQL_EXCEPTIONS = "RetrySqlExceptions";
        public const string CIRCUIT_BREAKER_ALLOWED_EXCEPTIONS = "CircuitBreakerAllowedExceptions";
        public const string CIRCUIT_BREAKER_BREAK_DURATION = "CircuitBreakerBreakDuration";
        public const string RETRY_EXPONENTIAL_BASE = "RetryExponentialBase";

        //**** Config Section Values (for the app.settings file) ****
        public const string DATABASE_RELIABILITY_SECTION = "DatabaseReliabilityConfig";
        public const string API_RELIABILITY_SECTION = "APIReliabilityConfig";

        //**** Policy Types  ****
        public const string RETRY_POLICY = "Retry";
        public const string TIMEOUT_POLICY = "Timeout";
        public const string BREAKER_POLICY = "Breaker";

        //**** DB Transient Faults ****
        public static readonly int[] SqlTransientExceptions =
        {
            40613,	//DatabaseNotCurrentlyAvailable
			40197,	//ErrorProcessingRequest,
			40501,	//ServiceCurrentlyBusy
			49918,	//NotEnoughResources
			40549,	//SessionTerminatedLongTransaction
			40550	//SessionTerminatedToManyLocks
		};

        public static readonly HttpStatusCode[] httpStatusCodesWorthRetrying = {
                HttpStatusCode.NotFound, // 404
                HttpStatusCode.MethodNotAllowed//405
            };
    }
}
