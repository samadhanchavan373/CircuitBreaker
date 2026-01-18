namespace OrderApi.CircuitBreakerService.Enums
{
    public enum CircuitBreakerState
    {
        Closed,    // Normal operation
        Open,      // Circuit is open, requests fail fast
        HalfOpen   // Testing if service is back up
    }
}
