namespace OsmToolkit.Geocoding
{
    /// <summary>
    /// Enforces a minimum interval between successive calls to <see cref="WaitForTurnAsync"/>, used to keep
    /// outgoing requests within an external service's rate limit (e.g. Nominatim's 1 request/second usage policy).
    /// </summary>
    internal sealed class RateGate
    {
        private readonly TimeSpan _minimumInterval;
        private readonly object _lock = new();
        private DateTimeOffset _nextAllowedRequestAtUtc = DateTimeOffset.MinValue;

        /// <summary>
        /// Test seam for the clock this gate measures elapsed time against. Defaults to the real system clock.
        /// Not intended for use by consumers; only visible within this assembly and to <c>OsmToolkitTests</c>.
        /// </summary>
        internal Func<DateTimeOffset> UtcNow { get; set; } = () => DateTimeOffset.UtcNow;

        /// <summary>
        /// Test seam for how a required wait is performed. Defaults to <see cref="Task.Delay(TimeSpan, CancellationToken)"/>.
        /// Not intended for use by consumers; only visible within this assembly and to <c>OsmToolkitTests</c>.
        /// </summary>
        internal Func<TimeSpan, CancellationToken, Task> Delay { get; set; } = Task.Delay;

        internal RateGate(TimeSpan minimumInterval)
        {
            _minimumInterval = minimumInterval;
        }

        /// <summary>
        /// Waits, if necessary, until at least the configured minimum interval has elapsed since the previously
        /// granted turn, then reserves the next turn. Concurrent callers are serialized so each one waits for its
        /// own slot rather than racing to observe the same "no wait needed" state.
        /// </summary>
        internal async Task WaitForTurnAsync(CancellationToken cancellationToken)
        {
            TimeSpan waitDuration;
            lock (_lock)
            {
                var now = UtcNow();
                waitDuration = _nextAllowedRequestAtUtc > now ? _nextAllowedRequestAtUtc - now : TimeSpan.Zero;
                _nextAllowedRequestAtUtc = (waitDuration > TimeSpan.Zero ? _nextAllowedRequestAtUtc : now) + _minimumInterval;
            }

            if (waitDuration > TimeSpan.Zero)
                await Delay(waitDuration, cancellationToken);
        }
    }
}
