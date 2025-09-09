namespace EY.TTT.IMY.AI.Domain.Resilience
{
    public interface IResiliencePolicy
    {
        /// <summary>
		/// Reliable async excution method for policy.
		/// </summary>
		Task ExecuteAsync(Func<Task> operation);

        /// <summary>
        /// Reliable asyncronous excution method for policy returning TResult.
        /// </summary>
        Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation);

        /// <summary>
        /// Set the timeout for retry
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        ResiliencePolicy SetTimeout(TimeSpan timeout);

        /// <summary>
        /// Set the Retry count
        /// </summary>
        /// <param name="cnt"></param>
        /// <returns></returns>
        ResiliencePolicy SetRetries(int cnt);
    }
}
