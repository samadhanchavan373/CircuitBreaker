using Microsoft.AspNetCore.Mvc;
using OrderApi.CircuitBreakerService.Enums;
using OrderApi.Interfaces;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IOrderService orderService;

        public OrderController(ILogger<OrderController> logger, IOrderService orderService)
        {
            _logger = logger;
            this.orderService = orderService;
        }

        [HttpPost(Name = "Order")]
        public  async Task<ActionResult> Order(Order order)
        {
            int statusCode = await this.orderService.CreateOrder(order);

            if (statusCode == 200)
            {
                return this.Ok("Success");
            }
            else if (statusCode == 503)
            {
                return StatusCode(503, new
                {
                    Error = "Restaurant service is currently unavailable"
                });
            }
                return StatusCode(500, new
                {
                    Error = "Internal Error"
                });
            
        }
    }
}
