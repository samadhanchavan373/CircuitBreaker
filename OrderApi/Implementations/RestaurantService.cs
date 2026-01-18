using OrderApi.CircuitBreakerService;
using OrderApi.CircuitBreakerService.Enums;
using OrderApi.Interfaces;
using System.Text.Json;
using System.Text;

namespace OrderApi.Implementations
{
    public class RestaurantService : IRestaurantService
    {
        private readonly HttpClient _httpClient;
        private readonly CircuitBreaker _circuitBreaker;
        private readonly string _restaurantServiceBaseUrl;

        public RestaurantService(HttpClient httpClient, IConfiguration configuration, CircuitBreaker circuitBreaker)
        {
            _httpClient = httpClient;
            _restaurantServiceBaseUrl = configuration["RestaurantService:BaseUrl"];

            // Circuit breaker: 3 failures, 5 minutes timeout
            _circuitBreaker = circuitBreaker;
        }

        public async Task<bool > CreateRestaurantOrder(Order order)
        {
            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    Console.WriteLine($"[RestaurantService] Submitting order {order.Id} to restaurant service");

                    var json = JsonSerializer.Serialize(order);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync($"{_restaurantServiceBaseUrl}", content);

                    if (response.StatusCode == System.Net.HttpStatusCode.Accepted) // 202
                    {
                        Console.WriteLine($"[RestaurantService] Order {order.Id} accepted (202)");
                        return true;
                    }
                    else if ((int)response.StatusCode == 503) // Service Unavailable
                    {
                        Console.WriteLine($"[RestaurantService] Service unavailable (503) for order {order.Id}");
                        throw new HttpRequestException("Restaurant service returned 503 - Service Unavailable");
                    }
                    else
                    {
                        Console.WriteLine($"[RestaurantService] Unexpected response {response.StatusCode} for order {order.Id}");
                        throw new HttpRequestException($"Unexpected response from restaurant service: {response.StatusCode}");
                    }
                },
                nameof(RestaurantService));
            }
            catch (CircuitBreakerOpenException)
            {
                Console.WriteLine($"[RestaurantService] Order {order.Id} rejected - Circuit breaker is OPEN");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantService] Failed to submit order {order.Id}: {ex.Message}");
                return false;
            }

        }

        public CircuitBreakerState GetCircuitBreakerState() => _circuitBreaker.keyValuePairs[nameof(RestaurantService)].State;
    }
}
