using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.SignalR;
using Microsoft.Azure.SignalR.Management;
using System.Threading.Tasks;
using System.Linq; // Needed for .All in IsValidUserId

namespace FoodDeliveryApp.SignalRHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NegotiateController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public NegotiateController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpPost]
        public async Task<IActionResult> Negotiate([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId) || !IsValidUserId(userId))
            {
                return BadRequest("Invalid or missing User ID");
            }

            if (userId != User.Identity?.Name)
            {
                return Unauthorized("User ID does not match authenticated user");
            }

            // Fix: Use GetClientEndpoint instead of GenerateClientEndpoint
            var hubName = "notifications";
            var endpoint = _serviceManager.GetClientEndpoint(hubName);
            var accessToken = _serviceManager.GenerateClientAccessToken(hubName, userId: userId);

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(accessToken))
            {
                return StatusCode(500, "Failed to negotiate SignalR connection");
            }

            return new JsonResult(new
            {
                url = endpoint,
                accessToken = accessToken
            });
        }

        private bool IsValidUserId(string userId)
        {
            // Ensure userId is alphanumeric and within length limits
            return !string.IsNullOrEmpty(userId) && userId.Length <= 50 && userId.All(char.IsLetterOrDigit);
        }
    }
}