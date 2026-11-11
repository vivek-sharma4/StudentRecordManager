using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentRecordManager.Models;

namespace StudentRecordManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [AllowAnonymous]
        [Route("Home/Error")]
        public IActionResult Error()
        {
            var requestId = HttpContext.TraceIdentifier;
            _logger.LogError("An unexpected application error occurred. RequestId: {RequestId}", requestId);
            return View(new ErrorViewModel
            {
                RequestId = requestId
            });
        }
    }
}