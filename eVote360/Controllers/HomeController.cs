using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Voting");
        }
    }
}
