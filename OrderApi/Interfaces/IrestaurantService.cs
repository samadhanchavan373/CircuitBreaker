using OrderApi.CircuitBreakerService.Enums;

namespace OrderApi.Interfaces
{
    public interface IRestaurantService
    {
        Task<bool> CreateRestaurantOrder(Order order);
        CircuitBreakerState GetCircuitBreakerState();
    }
}
