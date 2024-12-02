using FitnessApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace FitnessApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ManageTrainingRequests()
        {
            var requests = await _context.PersonalTrainingRequests
                .Include(r => r.Member)
                .Include(r => r.Trainer)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRequestStatus(int id, string status)
        {
            var request = await _context.PersonalTrainingRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            request.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction("ManageTrainingRequests");
        }
    }

}
