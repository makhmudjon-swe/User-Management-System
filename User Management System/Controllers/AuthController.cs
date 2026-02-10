using Microsoft.AspNetCore.Mvc;

namespace User_Management_System.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        [HttpGet("register")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
