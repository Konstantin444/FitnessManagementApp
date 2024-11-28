using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessApp.Data;
using FitnessApp.Models;

namespace FitnessApp
{
    public class TrainingSessionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainingSessionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TrainingSessions
        public async Task<IActionResult> Index()
        {
            var trainingSessions = await _context.TrainingSessions.Include(t => t.Reservations).ToListAsync();
            return View(trainingSessions);
        }

        // GET: TrainingSessions/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainingSession = await _context.TrainingSessions
                .Include(t => t.Reservations)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.SessionId == id);

            if (trainingSession == null)
            {
                return NotFound();
            }

            return View(trainingSession);
        }

        // GET: TrainingSessions/Create (Admin Only)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: TrainingSessions/Create (Admin Only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SessionId,Name,Trainer,SessionDateTime,MaxParticipants")] TrainingSession trainingSession)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trainingSession);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(trainingSession);
        }

        // GET: TrainingSessions/Edit/5 (Admin Only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainingSession = await _context.TrainingSessions.FindAsync(id);
            if (trainingSession == null)
            {
                return NotFound();
            }
            return View(trainingSession);
        }

        // POST: TrainingSessions/Edit/5 (Admin Only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SessionId,Name,Trainer,SessionDateTime,MaxParticipants")] TrainingSession trainingSession)
        {
            if (id != trainingSession.SessionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainingSession);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainingSessionExists(trainingSession.SessionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(trainingSession);
        }

        // GET: TrainingSessions/Delete/5 (Admin Only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainingSession = await _context.TrainingSessions.FirstOrDefaultAsync(m => m.SessionId == id);
            if (trainingSession == null)
            {
                return NotFound();
            }

            return View(trainingSession);
        }

        // POST: TrainingSessions/Delete/5 (Admin Only)
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainingSession = await _context.TrainingSessions.FindAsync(id);
            if (trainingSession != null)
            {
                _context.TrainingSessions.Remove(trainingSession);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: TrainingSessions/Reserve/5 (Member Only)
        [HttpPost]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Reserve(int id)
        {
            var trainingSession = await _context.TrainingSessions
                .Include(t => t.Reservations)
                .FirstOrDefaultAsync(t => t.SessionId == id);

            if (trainingSession == null)
            {
                return NotFound();
            }

            // Check if there are available slots
            if (trainingSession.MaxParticipants <= trainingSession.Reservations.Count)
            {
                ModelState.AddModelError("", "No spots available for this session.");
                return RedirectToAction(nameof(Index));
            }

            // Check if the user has already reserved a spot
            var userId = User.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.UserId == userId && r.TrainingSessionId == id);

            if (existingReservation != null)
            {
                ModelState.AddModelError("", "You have already reserved a spot for this session.");
                return RedirectToAction(nameof(Index));
            }

            // Create a new reservation
            var reservation = new Reservation
            {
                UserId = userId,
                TrainingSessionId = id,
                ReservationDateTime = DateTime.Now
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool TrainingSessionExists(int id)
        {
            return _context.TrainingSessions.Any(e => e.SessionId == id);
        }
    }
}
