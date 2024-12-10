using FitnessApp.Data;
using FitnessApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Admin Dashboard
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Dashboard()
    {
        // Gather statistics
        var totalMembers = await _context.Users.CountAsync();
        var totalRequests = await _context.PersonalTrainingRequests.CountAsync();
        var totalVisits = await _context.Reservations.CountAsync();

        var popularSessions = await _context.TrainingSessions
            .Include(s => s.Reservations)
            .Include(s => s.Trainer)
            .OrderByDescending(s => s.Reservations.Count)
            .Take(3) // Top 3 sessions
            .ToListAsync();


        // Get all trainers
        var trainers = await _context.Trainers.ToListAsync();

        // Pass data to the view
        ViewBag.TotalMembers = totalMembers;
        ViewBag.TotalRequests = totalRequests;
        ViewBag.TotalVisits = totalVisits;
        ViewBag.PopularSessions = popularSessions;
        ViewBag.Trainers = trainers;

        return View();
    }
    // Edit Trainer
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditTrainer(int id)
    {
        var trainer = await _context.Trainers.FindAsync(id);
        if (trainer == null)
        {
            return NotFound();
        }
        return View(trainer); // Returns the EditTrainer view with the current trainer data
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTrainer(int id, [Bind("Id,Name,Specialization")] Trainer trainer)
    {
        if (id != trainer.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(trainer); // Updates the trainer in the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TrainerExists(trainer.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Dashboard"); // Redirects back to the dashboard after saving
        }
        return View(trainer); // Returns the form with validation errors if model is invalid
    }

    private bool TrainerExists(int id)
    {
        return _context.Trainers.Any(e => e.Id == id);
    }


    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTrainer(int id)
    {
        // Find the trainer by id
        var trainer = await _context.Trainers.FindAsync(id);

        if (trainer == null)
        {
            return NotFound();  // If no trainer is found, return 404
        }

        // Remove the trainer from the database
        _context.Trainers.Remove(trainer);
        await _context.SaveChangesAsync();  // Save the changes to the database

        return RedirectToAction("Dashboard");  // Redirect to Dashboard after deletion
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateTrainer()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTrainer([Bind("Name")] Trainer trainer)
    {
        if (ModelState.IsValid)
        {
            _context.Trainers.Add(trainer);
            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard");
        }
        return View(trainer);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ManageTrainingRequests()
    {
        var pendingRequests = await _context.PersonalTrainingRequests
            .Include(r => r.Member)
            .Include(r => r.Trainer)
            .Where(r => r.Status == "Pending")
            .ToListAsync();

        return View(pendingRequests);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRequestStatus(int id, string status)
    {
        // Check for valid status
        if (status != "Approved" && status != "Rejected")
        {
            return BadRequest("Invalid status value.");
        }

        // Find the training request by ID
        var request = await _context.PersonalTrainingRequests.FindAsync(id);
        if (request == null)
        {
            return NotFound(); // If request is not found, return 404
        }

        // Update the status of the training request
        request.Status = status;

        // Save changes to the database
        await _context.SaveChangesAsync();

        // Redirect back to the page where the admin can manage training requests
        return RedirectToAction("ManageTrainingRequests"); // Or you can use RedirectToAction("Dashboard") if needed
    }
}
