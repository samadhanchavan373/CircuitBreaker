
using OrderApi.CircuitBreakerService;
using OrderApi.Implementations;
using OrderApi.Interfaces;

namespace OrderApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            var dict = new Dictionary<string, CircuitBreakerProps>();

            dict.Add(nameof(RestaurantService), new CircuitBreakerProps { _failureThreshold = 3, _timeout = TimeSpan.FromMinutes(3) });

            builder.Services.AddControllers();
            // Register CircuitBreaker as Singleton - CRITICAL for maintaining state across requests
            builder.Services.AddSingleton<CircuitBreaker>();

            builder.Services.AddHttpClient<RestaurantService>();
            builder.Services.AddHttpClient<IRestaurantService, RestaurantService>();

            builder.Services.AddSingleton<Dictionary<string, CircuitBreakerProps>>(provider => dict);

            // Register HttpClient
            builder.Services.AddScoped<IOrderService, OrderService>();


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
