using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace RestaurantApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RestaurantController : ControllerBase
    {
        public static bool _isHealthy = true;

        private readonly ILogger<RestaurantController> _logger;

        public RestaurantController(ILogger<RestaurantController> logger)
        {
            _logger = logger;
        }

        [HttpPost(Name = "Order")]
        public ActionResult Order(Order order)
        {

            if (_isHealthy)
            {
                return this.StatusCode((int)HttpStatusCode.Accepted);
            }
            return this.StatusCode((int)HttpStatusCode.ServiceUnavailable);    
        }

        [HttpPost("SimulateServiceUnAvaliable")]
        public ActionResult ServiceUnAvaliable()
        {
            _isHealthy = false;

            return this.StatusCode((int)HttpStatusCode.OK);
        }

        [HttpPost("SimulateServiceAvaliable")]
        public ActionResult ServiceAvaliable()
        {
            _isHealthy = true;

            return this.StatusCode((int)HttpStatusCode.OK);
        }
    }
}
