using OrderApi.CircuitBreakerService.Enums;
using OrderApi.Interfaces;

namespace OrderApi.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IRestaurantService restaurantService;

        public OrderService(IRestaurantService restaurantService)
        {
            this.restaurantService = restaurantService;
        }

        public async Task<int> CreateOrder(Order order)
        {
            try
            {
                bool isOrderAccepted = await restaurantService.CreateRestaurantOrder(order);

                if (isOrderAccepted)
                {
                    return 200;
                }
                else
                {
                    var circuitState = restaurantService.GetCircuitBreakerState();

                    if (circuitState != CircuitBreakerState.Closed)
                    {
                        return 503;
                    }
                    return 500;
                }

            }
            catch (Exception ex)
            {
                return 500;
            }
        }
    }
}
