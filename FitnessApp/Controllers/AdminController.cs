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

        // Get most popular training sessions
        var popularSessions = await _context.TrainingSessions
            .Include(s => s.Reservations)
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
        return View(trainer);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTrainer(int id, [Bind("Name")] Trainer trainer)
    {
        if (id != trainer.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _context.Update(trainer);
            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard");
        }
        return View(trainer);
    }

    // Delete Trainer
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTrainer(int id)
    {
        var trainer = await _context.Trainers.FindAsync(id);
        if (trainer == null)
        {
            return NotFound();
        }

        _context.Trainers.Remove(trainer);
        await _context.SaveChangesAsync();
        return RedirectToAction("Dashboard");
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


}
