using OrderApi.CircuitBreakerService.Enums;
using System.Collections.Generic;

namespace OrderApi.CircuitBreakerService
{
    public class CircuitBreakerProps
    {
        public  int _failureThreshold;
        public  TimeSpan _timeout;
        public int _failureCount;
        public DateTime _lastFailureTime;
        public CircuitBreakerState _state;

        public CircuitBreakerState State => _state;
        public int FailureCount => _failureCount;
        public DateTime? LastFailureTime => _lastFailureTime == DateTime.MinValue ? null : _lastFailureTime;
    }

    public class CircuitBreaker
    {
        public Dictionary<string, CircuitBreakerProps> keyValuePairs { get; private set; }

        private readonly object _lock = new object();


        public CircuitBreaker(Dictionary<string, CircuitBreakerProps> keyValuePairs )
        {
            this.keyValuePairs = keyValuePairs;
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, string serviceName)
        {
            if (!CanExecute(serviceName))
            {
                throw new CircuitBreakerOpenException($"Circuit breaker is OPEN. {serviceName} service is currently unavailable.");
            }

            try
            {
                var result = await operation();
                OnSuccess(serviceName);
                return result;
            }
            catch (Exception ex)
            {
                OnFailure(ex, serviceName);
                throw;
            }
        }

        private bool CanExecute(string serviceName)
        {
            lock (_lock)
            {
                CircuitBreakerProps CircuitBreakerProps = keyValuePairs[serviceName];
                if (CircuitBreakerProps._state == CircuitBreakerState.Open)
                {
                    // Check if timeout period has passed
                    if (DateTime.UtcNow - CircuitBreakerProps._lastFailureTime >= CircuitBreakerProps._timeout)
                    {
                        CircuitBreakerProps._state = CircuitBreakerState.HalfOpen;
                        Console.WriteLine("[Circuit Breaker] State changed to HALF-OPEN - Testing service availability");
                        return true;
                    }
                    return false;
                }
                return true;
            }
        }

        private void OnSuccess(string serviceName)
        {
            lock (_lock)
            {
                CircuitBreakerProps CircuitBreakerProps = keyValuePairs[serviceName];

                CircuitBreakerProps._failureCount = 0;
                if (CircuitBreakerProps._state == CircuitBreakerState.HalfOpen)
                {
                    CircuitBreakerProps._state = CircuitBreakerState.Closed;
                    Console.WriteLine("[Circuit Breaker] State changed to CLOSED - Service is healthy");
                }
            }
        }

        private void OnFailure(Exception ex, string serviceName)
        {
            lock (_lock)
            {
                CircuitBreakerProps CircuitBreakerProps = keyValuePairs[serviceName];

                CircuitBreakerProps._failureCount++;
                CircuitBreakerProps._lastFailureTime = DateTime.UtcNow;

                Console.WriteLine($"[Circuit Breaker] Failure #{CircuitBreakerProps._failureCount}: {ex.Message}");

                if (CircuitBreakerProps._failureCount >= CircuitBreakerProps._failureThreshold && CircuitBreakerProps._state == CircuitBreakerState.Closed)
                {
                    CircuitBreakerProps._state = CircuitBreakerState.Open;
                    Console.WriteLine($"[Circuit Breaker] OPENED after {CircuitBreakerProps._failureCount} consecutive failures. Will retry after {CircuitBreakerProps._timeout.TotalMinutes} minutes.");
                }
                else if (CircuitBreakerProps._state == CircuitBreakerState.HalfOpen)
                {
                    CircuitBreakerProps._state = CircuitBreakerState.Open;
                    Console.WriteLine("[Circuit Breaker] Back to OPEN state - Service still unavailable");
                }
            }
        }
    }
}
