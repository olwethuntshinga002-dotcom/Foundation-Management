using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationManagement.Controllers
{
    [Authorize(Roles = "Donor")]
    public class DonorController : Controller
    {
        // Donor home / dashboard
        public IActionResult Index()
        {
            // TODO: load these from the database for the logged-in donor
            ViewBag.PledgeCount   = 0;
            ViewBag.FoodCount     = 0;
            ViewBag.ClothingCount = 0;
            ViewBag.NgoCount      = 0;
            return View();
        }

        // The pages below use a placeholder view until each one is built.
        // When you build a real page, add Views/Donor/<ActionName>.cshtml
        // and change `return View("DonorPlaceholder")` to `return View()`.

        public IActionResult Profile()
        {
            ViewBag.PageTitle = "My Profile";
            return View("DonorPlaceholder");
        }

        public IActionResult NgoInventory()
        {
            ViewBag.PageTitle = "NGOs Inventory";
            return View("DonorPlaceholder");
        }

        public IActionResult Pledges()
        {
            ViewBag.PageTitle = "Pledges";
            return View("DonorPlaceholder");
        }

        public IActionResult MyDonations()
        {
            ViewBag.PageTitle = "My Donations";
            return View("DonorPlaceholder");
        }

        public IActionResult DonatableItems()
        {
            ViewBag.PageTitle = "Donatable Items";
            return View("DonorPlaceholder");
        }
    }
}