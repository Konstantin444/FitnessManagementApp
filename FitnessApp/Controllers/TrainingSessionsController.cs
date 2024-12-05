using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessApp.Data;
using FitnessApp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessApp.Controllers
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
            var trainingSessions = await _context.TrainingSessions
                .Include(t => t.Reservations)
                .Include(t => t.Trainer) // Include Trainer
                .ToListAsync();

            if (User.IsInRole("Member"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var personalRequests = await _context.PersonalTrainingRequests
                    .Where(r => r.MemberId == userId)
                    .Include(r => r.Trainer)
                    .ToListAsync();

                ViewBag.PersonalTrainingRequests = personalRequests;
            }

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
                .Include(t => t.Trainer) // Include Trainer
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
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "Name");
            return View();
        }

        // POST: TrainingSessions/Create (Admin Only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SessionId,Name,TrainerId,SessionDateTime,MaxParticipants")] TrainingSession trainingSession)
        {
            // Check if the date is in the past
            if (trainingSession.SessionDateTime.Date < DateTime.Now.Date)
            {
                ModelState.AddModelError("SessionDateTime", "The session date cannot be in the past.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(trainingSession);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "Name", trainingSession.TrainerId);
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

            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "Name", trainingSession.TrainerId); // Populate trainers for dropdown
            return View(trainingSession);
        }

        // POST: TrainingSessions/Edit/5 (Admin Only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SessionId,Name,TrainerId,SessionDateTime,MaxParticipants")] TrainingSession trainingSession)
        {
            if (id != trainingSession.SessionId)
            {
                return NotFound();
            }

            // Check if the date is in the past
            if (trainingSession.SessionDateTime.Date < DateTime.Now.Date)
            {
                ModelState.AddModelError("SessionDateTime", "The session date cannot be in the past.");
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
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "Name", trainingSession.TrainerId);
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

            var trainingSession = await _context.TrainingSessions
                .Include(t => t.Trainer) // Include Trainer for confirmation
                .FirstOrDefaultAsync(m => m.SessionId == id);

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
            var trainingSession = await _context.TrainingSessions
                .Include(t => t.Reservations)
                .FirstOrDefaultAsync(t => t.SessionId == id);

            if (trainingSession == null)
            {
                return NotFound();
            }

            _context.Reservations.RemoveRange(trainingSession.Reservations); // Remove reservations
            _context.TrainingSessions.Remove(trainingSession); // Remove the session
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Reserve(int id)
        {
            var trainingSession = await _context.TrainingSessions
                .Include(t => t.Reservations)
                .FirstOrDefaultAsync(t => t.SessionId == id);

            if (trainingSession == null)
            {
                return NotFound("The selected training session does not exist.");
            }

            if (trainingSession.MaxParticipants <= trainingSession.Reservations.Count)
            {
                return BadRequest("No spots available for this session.");
            }

            var userId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.UserId == userId && r.TrainingSessionId == id);

            if (existingReservation != null)
            {
                return BadRequest("You have already reserved a spot for this session.");
            }

            var reservation = new Reservation
            {
                UserId = userId,
                TrainingSessionId = id,
                ReservationDateTime = DateTime.Now
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Leave(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.UserId == userId && r.TrainingSessionId == id);

            if (reservation == null)
            {
                return BadRequest("You are not registered for this session.");
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id });
        }

        // GET: Request Personal Training
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> RequestTraining()
        {
            var trainers = await _context.Trainers.ToListAsync();
            return View(trainers);
        }

        // POST: Request Personal Training
        [HttpPost]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> RequestTraining(int trainerId, DateTime date)
        {
            var request = new PersonalTrainingRequest
            {
                TrainerId = trainerId,
                Date = date,
                Status = "Pending",
                MemberId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            _context.PersonalTrainingRequests.Add(request);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyRequests");
        }

        // GET: View Personal Training Requests
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> MyRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var requests = await _context.PersonalTrainingRequests
                .Where(r => r.MemberId == userId)
                .Include(r => r.Trainer)
                .ToListAsync();

            return View(requests);
        }

        private bool TrainingSessionExists(int id)
        {
            return _context.TrainingSessions.Any(e => e.SessionId == id);
        }



        [HttpGet]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> CreateTrainingRequest()
        {
            // Get a list of all trainers
            var trainers = await _context.Trainers.ToListAsync();

            // Pass the list of trainers to the view
            ViewBag.Trainers = new SelectList(trainers, "Id", "Name"); // "Id" is the value, "Name" is what is displayed

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRequestStatus(int id, string status)
        {
            var request = await _context.PersonalTrainingRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            request.Status = status;
            _context.Update(request);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageRequests));
        }


        [Authorize(Roles = "Admin")]
        public IActionResult CreateTrainer()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTrainer([Bind("Name,Specialization")] Trainer trainer)
        {
            if (ModelState.IsValid)
            {
                _context.Trainers.Add(trainer);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index"); // Redirect to a list of trainers or training sessions
            }
            return View(trainer);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageRequests()
        {
            var pendingRequests = await _context.PersonalTrainingRequests
                .Include(r => r.Member)
                .Include(r => r.Trainer)
                .Where(r => r.Status == "Pending")
                .ToListAsync();

            return View(pendingRequests);
        }

        [Authorize(Roles = "Admin")]
        public class AdminController : Controller
        {
            private readonly ApplicationDbContext _context;

            public AdminController(ApplicationDbContext context)
            {
                _context = context;
            }

            // Most popular training sessions
            public async Task<IActionResult> PopularTrainingSessions()
            {
                var popularSessions = await _context.TrainingSessions
                    .Include(t => t.Reservations)
                    .OrderByDescending(t => t.Reservations.Count)
                    .Take(10) // Top 10
                    .ToListAsync();

                return View(popularSessions);
            }

            // Total Members
            public async Task<IActionResult> TotalMembers()
            {
                var totalMembers = await _context.Users.CountAsync();
                ViewBag.TotalMembers = totalMembers;
                return View();
            }
        }
    }
}
