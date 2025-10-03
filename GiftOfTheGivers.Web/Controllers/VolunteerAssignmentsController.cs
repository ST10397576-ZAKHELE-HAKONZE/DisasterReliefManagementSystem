using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Data; // Assuming context is here

namespace GiftOfTheGivers.Web.Controllers
{
    public class VolunteerAssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VolunteerAssignmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: VolunteerAssignments
        public async Task<IActionResult> Index()
        {
            // Include navigation properties for display (User email and Project title)
            var assignments = _context.VolunteerAssignments
                .Include(v => v.User)
                .Include(v => v.Project)
                .OrderBy(v => v.Project.Title); // Sort by project title for organization

            return View(await assignments.ToListAsync());
        }

        // GET: VolunteerAssignments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.VolunteerAssignments
                .Include(v => v.User)
                .Include(v => v.Project)
                .FirstOrDefaultAsync(m => m.AssignmentID == id);

            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // GET: VolunteerAssignments/Create
        public IActionResult Create()
        {
            // FIX: Change SelectList to use "Email" as the display text instead of "Id"
            ViewData["UserId"] = new SelectList(_context.Users.OrderBy(u => u.Email), "Id", "Email");
            ViewData["ProjectID"] = new SelectList(_context.ReliefProjects.OrderBy(p => p.Title), "ProjectID", "Title");
            return View();
        }

        // POST: VolunteerAssignments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AssignmentID,Role,AssignedDate,Status,UserId,ProjectID")] VolunteerAssignment volunteerAssignment)
        {
            // Remove navigation properties from validation check as they will be null on POST
            ModelState.Remove("User");
            ModelState.Remove("Project");

            if (ModelState.IsValid)
            {
                // Ensure AssignedDate is set if not provided by the form
                if (volunteerAssignment.AssignedDate == default)
                {
                    volunteerAssignment.AssignedDate = System.DateTime.UtcNow;
                }

                _context.Add(volunteerAssignment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Fallback for invalid model state
            // FIX: Change SelectList to use "Email" as the display text instead of "Id"
            ViewData["UserId"] = new SelectList(_context.Users.OrderBy(u => u.Email), "Id", "Email", volunteerAssignment.UserId);
            ViewData["ProjectID"] = new SelectList(_context.ReliefProjects.OrderBy(p => p.Title), "ProjectID", "Title", volunteerAssignment.ProjectID);
            return View(volunteerAssignment);
        }

        // GET: VolunteerAssignments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var volunteerAssignment = await _context.VolunteerAssignments.FindAsync(id);
            if (volunteerAssignment == null)
            {
                return NotFound();
            }

            // Populate dropdowns, setting the currently selected value
            // FIX: Change SelectList to use "Email" as the display text instead of "Id"
            ViewData["UserId"] = new SelectList(_context.Users.OrderBy(u => u.Email), "Id", "Email", volunteerAssignment.UserId);
            ViewData["ProjectID"] = new SelectList(_context.ReliefProjects.OrderBy(p => p.Title), "ProjectID", "Title", volunteerAssignment.ProjectID);

            return View(volunteerAssignment);
        }

        // POST: VolunteerAssignments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AssignmentID,Role,AssignedDate,Status,UserId,ProjectID")] VolunteerAssignment volunteerAssignment)
        {
            if (id != volunteerAssignment.AssignmentID)
            {
                return NotFound();
            }

            // Remove navigation properties from validation check
            ModelState.Remove("User");
            ModelState.Remove("Project");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(volunteerAssignment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VolunteerAssignmentExists(volunteerAssignment.AssignmentID))
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

            // Fallback for invalid model state
            // FIX: Change SelectList to use "Email" as the display text instead of "Id"
            ViewData["UserId"] = new SelectList(_context.Users.OrderBy(u => u.Email), "Id", "Email", volunteerAssignment.UserId);
            ViewData["ProjectID"] = new SelectList(_context.ReliefProjects.OrderBy(p => p.Title), "ProjectID", "Title", volunteerAssignment.ProjectID);
            return View(volunteerAssignment);
        }

        // GET: VolunteerAssignments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.VolunteerAssignments
                .Include(v => v.User)
                .Include(v => v.Project)
                .FirstOrDefaultAsync(m => m.AssignmentID == id);

            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // POST: VolunteerAssignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var volunteerAssignment = await _context.VolunteerAssignments.FindAsync(id);
            if (volunteerAssignment != null)
            {
                _context.VolunteerAssignments.Remove(volunteerAssignment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VolunteerAssignmentExists(int id)
        {
            return _context.VolunteerAssignments.Any(e => e.AssignmentID == id);
        }
    }
}
