using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using Polly.Wrap;

namespace EY.TTT.IMY.AI.Domain.Resilience
{
    public class ResiliencePolicy : IResiliencePolicy
    {
        private ILogger<ResiliencePolicy> _logger;
        /// <summary>Number of retries</summary>
        protected int? Retries;
        /// <summary>Number of retries before circuit break opens</summary>
        protected int? CircuitBreakAllowedExceptions;
        /// <summary>Duration of break before circuit break closes</summary>
        protected int? DurationOfBreak;
        /// <summary>Timeout period</summary>
        protected TimeSpan Timeout;
        /// <summary>
        /// The exponential base based on which the retry policy will work
        /// </summary>
        protected int RetryExponentialBase;

        internal AsyncPolicyWrap? PolicyWrapAsync;
        internal AsyncPolicy? RetryPolicyAsync;
        internal AsyncPolicy? CircuitBreakerPolicyAsync;
        internal AsyncPolicy? TimeoutPolicyAsync;

        /// <summary>
        /// Instantiates a new instance of <see cref="ResiliencePolicy"/>.
        /// </summary>
        /// <param name="configuration">An instance of <see cref="IConfiguration"/>.</param>
        /// <param name="configSectionName">The name of config section that contains the configuration for the policies.</param>
        public ResiliencePolicy(IConfiguration configuration, string configSectionName, ILogger<ResiliencePolicy> logger)
        {
            this._logger = logger;
            this.Setup(configuration.GetSection(configSectionName));
        }

        private void Setup(IConfiguration config)
        {
            this.Retries = config.GetValue<int>(ResilienceConstants.RETRIES);
            this.CircuitBreakAllowedExceptions = config.GetValue<int>(ResilienceConstants.CIRCUIT_BREAKER_ALLOWED_EXCEPTIONS);
            this.DurationOfBreak = config.GetValue<int>(ResilienceConstants.CIRCUIT_BREAKER_BREAK_DURATION);
            var timeoutSeconds = config.GetValue<int>(ResilienceConstants.TIMEOUT);
            this.Timeout = (timeoutSeconds == default || timeoutSeconds == 0)
                ? TimeSpan.MinValue
                : TimeSpan.FromSeconds(timeoutSeconds);
            this.RetryExponentialBase = config.GetValue<int>(ResilienceConstants.RETRY_EXPONENTIAL_BASE);
            this.PolicyWrapAsync = this.GeneratePolicyWrap();
        }
        private AsyncPolicyWrap GeneratePolicyWrap()
        {
            this.RetryPolicyAsync = this.GenerateRetryAsyncPolicy() ?? Policy.NoOpAsync();
            this.TimeoutPolicyAsync = this.GenerateTimeoutAsyncPolicy() ?? Policy.NoOpAsync();
            this.CircuitBreakerPolicyAsync = this.GenerateCircuitBreakerAsyncPolicy() ?? Policy.NoOpAsync();
            var wrap = TimeoutPolicyAsync.WrapAsync(RetryPolicyAsync.WrapAsync(CircuitBreakerPolicyAsync));
            return wrap;
        }

        /// <summary>
        /// Generates an async policy to handle retry functionality.
        /// </summary>
        /// <returns>A <see cref="Policy"/> for retry functionality.</returns>
        protected virtual AsyncPolicy GenerateRetryAsyncPolicy()
        {
            if (this.Retries == default)
            {
                return null;
            }

            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(this.Retries.Value,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(RetryExponentialBase, retryAttempt)),
                (ex, count) =>
                {
                    // TODO: Logging Here
                    _logger.LogDebug($"{ex.GetType().ToString()} exception thrown while automatically retrying. Retry count = {count} at {DateTime.UtcNow}");
                });
        }

        /// <summary>
        /// Generates an async policy to handle circuit breaker functionality.
        /// </summary>
        /// <returns>A <see cref="Policy"/> for circuit breaker functionality.</returns>
        /// <exception cref="BrokenCircuitException"></exception>
        protected virtual AsyncPolicy GenerateCircuitBreakerAsyncPolicy()
        {
            if (this.CircuitBreakAllowedExceptions == default || this.CircuitBreakAllowedExceptions == 0 || this.DurationOfBreak == default || this.DurationOfBreak == 0)
            {
                return null;
            }

            return Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(this.CircuitBreakAllowedExceptions.Value, TimeSpan.FromSeconds(this.DurationOfBreak.Value),
                    (ex, span) =>
                    {
                        // TODO: Logging for when circuit is opened.
                        _logger.LogDebug($"Fund of Fund Aggregator API: Action Failed! Circuit breaker is open, waiting {span}... at at {DateTime.UtcNow}");
                    },
                    () =>
                    {
                        // TODO: Logging for first execution after circuit is reset
                        _logger.LogDebug($"Fund of Fund Aggregator API: Circuit breaker reset....at {DateTime.UtcNow}");
                    });
        }

        /// <summary>
        /// Generates an async policy to handle circuit breaker functionality.
        /// </summary>
        /// <returns>A <see cref="Policy"/> for circuit breaker functionality.</returns>
        protected virtual AsyncPolicy GenerateTimeoutAsyncPolicy()
        {
            if (this.Timeout == TimeSpan.MinValue)
            {
                return null;
            }

            return Policy.TimeoutAsync(this.Timeout, TimeoutStrategy.Pessimistic);
        }

        /// <summary>
        /// Overrides timeout policy
        /// </summary>
        /// <param name="timeout">An instance of <see cref="TimeSpan"/> representing the timeout period.</param>
        public ResiliencePolicy SetTimeout(TimeSpan timeout)
        {
            this.Timeout = timeout;
            this.PolicyWrapAsync = this.GeneratePolicyWrap();

            return this;
        }

        /// <summary>
        /// Set the Retry count
        /// </summary>
        /// <param name="cnt"></param>
        /// <returns></returns>
        public ResiliencePolicy SetRetries(int cnt)
        {
            this.Retries = cnt;
            this.PolicyWrapAsync = this.GeneratePolicyWrap();

            return this;
        }

        /// <inheritdoc />
        public async Task ExecuteAsync(Func<Task> operation)
            => await this.PolicyWrapAsync.ExecuteAsync(operation.Invoke);

        /// <inheritdoc />
        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation)
            => this.PolicyWrapAsync.ExecuteAsync(operation);
    }
}
