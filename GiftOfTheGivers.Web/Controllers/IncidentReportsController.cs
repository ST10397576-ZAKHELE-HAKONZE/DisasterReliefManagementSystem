using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; // Required for accessing the current user ID
using GiftOfTheGivers.Web.Data; // Assuming ApplicationDbContext is here
using GiftOfTheGivers.Web.Models; // For IncidentReport and ApplicationUser
using GiftOfTheGivers.Web.Models.Enums; // For IncidentSeverity and IncidentStatus

namespace GiftOfTheGivers.Web.Controllers
{
    public class IncidentReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IncidentReportsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // CORRECTED: Helper method now returns nullable Task<ApplicationUser?>
        private Task<ApplicationUser?> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        //
        // GET: IncidentReports - Displays a list of reports
        //
        public async Task<IActionResult> Index()
        {
            // Check for null user and handle (e.g., redirect to login)
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                // If user is not logged in, redirect to login
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Fetch reports, including the user who reported it (Navigation Property)
            // The 'user' object is guaranteed non-null due to the preceding check.
            var incidentReports = _context.IncidentReports
                .Include(i => i.ReportedByUser)
                .Where(i => i.ReportedByUserId == user.Id) // Filter for only the current user's reports
                .OrderByDescending(i => i.Timestamp); // Show newest reports first

            ViewData["CurrentUserId"] = user.Id;

            return View(await incidentReports.ToListAsync());
        }

        //
        // GET: IncidentReports/Details/5
        //
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var incidentReport = await _context.IncidentReports
                .Include(i => i.ReportedByUser)
                .FirstOrDefaultAsync(m => m.ReportID == id);

            if (incidentReport == null)
            {
                return NotFound();
            }

            // Check if the current user has permission (optional, but good practice)
            var user = await GetCurrentUserAsync();
            if (user != null && incidentReport.ReportedByUserId != user.Id)
            {
                // return Forbid(); 
            }

            return View(incidentReport);
        }

        //
        // GET: IncidentReports/Create - Shows the incident creation form
        //
        public IActionResult Create()
        {
            return View();
        }

        //
        // POST: IncidentReports/Create - Processes the submitted form data
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Bind only the properties the user can set via the form: Title, Location, Description, Severity
        public async Task<IActionResult> Create([Bind("Title,Location,Description,Severity")] IncidentReport incidentReport)
        {
            // Check for null user and handle
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Manually set required, non-user-supplied properties
            incidentReport.ReportedByUserId = user.Id;
            incidentReport.Timestamp = System.DateTime.UtcNow;
            incidentReport.Status = IncidentStatus.Reported; // All new reports start as Reported

            // We must remove the navigation property and the FK from validation since they are null during post, or set automatically
            ModelState.Remove(nameof(incidentReport.ReportedByUser));
            ModelState.Remove(nameof(incidentReport.ReportedByUserId));
            ModelState.Remove(nameof(incidentReport.Status));

            if (ModelState.IsValid)
            {
                _context.Add(incidentReport);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(incidentReport);
        }

        //
        // GET: IncidentReports/Edit/5
        //
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var incidentReport = await _context.IncidentReports.FindAsync(id);
            if (incidentReport == null)
            {
                return NotFound();
            }

            var user = await GetCurrentUserAsync();
            if (user != null && incidentReport.ReportedByUserId != user.Id && !User.IsInRole("Admin"))
            {
                // return Forbid(); 
            }

            // Populate the dropdown for Status, using the Enums
            ViewData["Status"] = new SelectList(Enum.GetValues(typeof(IncidentStatus)));

            return View(incidentReport);
        }

        //
        // POST: IncidentReports/Edit/5
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReportID,Title,Location,Description,Severity,Status,ReportedByUserId,Timestamp")] IncidentReport incidentReport)
        {
            if (id != incidentReport.ReportID)
            {
                return NotFound();
            }

            // Prevent user from accidentally overwriting immutable fields
            ModelState.Remove(nameof(incidentReport.ReportedByUser));

            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure the Timestamp and ReportedByUserId are NOT overwritten by user input from the form
                    _context.Entry(incidentReport).Property(i => i.Timestamp).IsModified = false;
                    _context.Entry(incidentReport).Property(i => i.ReportedByUserId).IsModified = false;

                    _context.Update(incidentReport);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IncidentReportExists(incidentReport.ReportID))
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
            ViewData["Status"] = new SelectList(Enum.GetValues(typeof(IncidentStatus)), incidentReport.Status);
            return View(incidentReport);
        }

        //
        // GET: IncidentReports/Delete/5
        //
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var incidentReport = await _context.IncidentReports
                .Include(i => i.ReportedByUser)
                .FirstOrDefaultAsync(m => m.ReportID == id);

            if (incidentReport == null)
            {
                return NotFound();
            }

            return View(incidentReport);
        }

        //
        // POST: IncidentReports/Delete/5
        //
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var incidentReport = await _context.IncidentReports.FindAsync(id);
            if (incidentReport != null)
            {
                _context.IncidentReports.Remove(incidentReport);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IncidentReportExists(int id)
        {
            return _context.IncidentReports.Any(e => e.ReportID == id);
        }
    }
}
