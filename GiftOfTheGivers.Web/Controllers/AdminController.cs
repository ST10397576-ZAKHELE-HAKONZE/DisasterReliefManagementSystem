using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiftOfTheGivers.Web.Data;
using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Models.ViewModels; // <<< NEW REQUIRED USING STATEMENT
using System.Linq;
using System.Threading.Tasks;
using System; // Required for DateTime methods

namespace GiftOfTheGivers.Web.Controllers
{
    // CRITICAL SECURITY FIX: Only allow users with the "Administrator" role to access this controller.
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/
        public async Task<IActionResult> Index()
        {
            // Set ViewBag for success message from TempData
            if (TempData["SuccessMessage"] is string message)
            {
                ViewBag.SuccessMessage = message;
            }

            var allReports = await _context.IncidentReports
                .Include(i => i.ReportedByUser)
                .ToListAsync();

            return View("Index", allReports);
        }

        // GET: /Admin/Edit/5
        // REFRACTOR: Now prepares and returns the ViewModel
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var incidentReport = await _context.IncidentReports
                .Include(i => i.ReportedByUser)
                .FirstOrDefaultAsync(m => m.ReportID == id);

            if (incidentReport == null) return NotFound();

            // Map the full model to the light-weight ViewModel for the view
            var viewModel = new IncidentReportEditViewModel
            {
                ReportID = incidentReport.ReportID,
                Title = incidentReport.Title,
                Timestamp = incidentReport.Timestamp,
                ReporterEmail = incidentReport.ReportedByUser?.Email ?? "N/A", // Safe dereference
                Status = incidentReport.Status,
                Severity = incidentReport.Severity,
                Description = incidentReport.Description // Pre-fill current description
            };

            return View("Edit", viewModel);
        }

        // POST: /Admin/Edit/5
        // REFRACTOR: Now accepts the light-weight ViewModel on postback (FIXES THE SAVE ISSUE)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IncidentReportEditViewModel vm)
        {
            if (id != vm.ReportID) return NotFound();

            // Model state validation only checks the fields present in the ViewModel (Status, Severity, Description), 
            // allowing the save logic to execute correctly.
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Fetch the original record from the database
                    var originalReport = await _context.IncidentReports.FindAsync(id);

                    if (originalReport == null) return NotFound();

                    // 2. Apply updates ONLY to the fields submitted by the admin
                    originalReport.Status = vm.Status;
                    originalReport.Severity = vm.Severity;
                    originalReport.Description = vm.Description;

                    // 3. Save changes
                    _context.Update(originalReport);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Incident Report #{id} successfully updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.IncidentReports.Any(e => e.ReportID == vm.ReportID))
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
            
            // If validation fails (e.g., Description is empty), we need to re-fetch the read-only data
            // to re-render the view correctly.
            var incidentReport = await _context.IncidentReports
                .Include(i => i.ReportedByUser)
                .AsNoTracking() // Important since we already fetched it, but to be safe
                .FirstOrDefaultAsync(m => m.ReportID == id);

            // Re-populate the read-only fields for the View (Title, Timestamp, Email)
            if (incidentReport != null)
            {
                vm.Title = incidentReport.Title;
                vm.Timestamp = incidentReport.Timestamp;
                vm.ReporterEmail = incidentReport.ReportedByUser?.Email ?? "N/A";
            }
            else
            {
                // This path should ideally not be reached if the record exists
                return NotFound();
            }

            // Return the ViewModel back to the view with validation errors
            return View("Edit", vm);
        }

        // GET: Admin/Delete/5
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

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var incidentReport = await _context.IncidentReports.FindAsync(id);

            if (incidentReport != null)
            {
                _context.IncidentReports.Remove(incidentReport);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Incident Report #{id} successfully deleted.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
