using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GiftOfTheGivers.Web.Models;

namespace GiftOfTheGivers.Web.Controllers
{
    public class DonationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DonationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Donations
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Donations.Include(d => d.Donor).Include(d => d.RecordedByUser);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Donations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donation = await _context.Donations
                .Include(d => d.Donor)
                .Include(d => d.RecordedByUser)
                .Include(d => d.ReliefProject)
                .FirstOrDefaultAsync(m => m.DonationID == id);
            if (donation == null)
            {
                return NotFound();
            }

            return View(donation);
        }

        // GET: Donations/Create
        public IActionResult Create()
        {
            ViewData["DonorID"] = new SelectList(_context.Donors, "DonorID", "LastName");

            // FIX: Change "Id" to "Email" to display a friendly user name
            ViewData["RecordedByUserId"] = new SelectList(_context.Users, "Id", "Email");

            ViewData["ReliefProjectProjectID"] = new SelectList(_context.ReliefProjects, "ProjectID", "Title");
            return View();
        }

        // POST: Donations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create([Bind("DonationID,Type,Amount,Description,DateReceived,Status,DonorID,RecordedByUserId,ReliefProjectProjectID")] Donation donation)
        {
            // FIX: Clear Model State errors for Navigation Properties
            // These objects are null on submission and causing validation failure.
            ModelState.Remove("Donor");
            ModelState.Remove("RecordedByUser");
            ModelState.Remove("ReliefProject");

            // Now check if model state is valid
            if (ModelState.IsValid)
            {
                _context.Add(donation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Fallback if model state is invalid
            ViewData["DonorID"] = new SelectList(_context.Donors, "DonorID", "LastName", donation.DonorID);
            // FIX: Change "Id" to "Email" here too (on failure reload)
            ViewData["RecordedByUserId"] = new SelectList(_context.Users, "Id", "Email", donation.RecordedByUserId);
            ViewData["ReliefProjectProjectID"] = new SelectList(_context.ReliefProjects, "ProjectID", "Title", donation.ReliefProjectProjectID);
            return View(donation);
        }

        // GET: Donations/Edit/5
        // THIS WAS MISSING AND LIKELY CAUSED THE FAILURE
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donation = await _context.Donations.FindAsync(id);

            // Critical check: if the donation doesn't exist (e.g., ID 3 was deleted)
            if (donation == null)
            {
                return NotFound();
            }

            // Populate the ViewData lists for the dropdowns, setting the current value
            ViewData["DonorID"] = new SelectList(_context.Donors, "DonorID", "LastName", donation.DonorID);
            // FIX: Change "Id" to "Email" for display text
            ViewData["RecordedByUserId"] = new SelectList(_context.Users, "Id", "Email", donation.RecordedByUserId);
            ViewData["ReliefProjectProjectID"] = new SelectList(_context.ReliefProjects, "ProjectID", "Title", donation.ReliefProjectProjectID);

            return View(donation);
        }


        // POST: Donations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DonationID,Type,Amount,Description,DateReceived,Status,DonorID,RecordedByUserId,ReliefProjectProjectID")] Donation donation)
        {
            if (id != donation.DonationID)
            {
                return NotFound();
            }

            // Added removal of navigation properties for validation stability
            ModelState.Remove("Donor");
            ModelState.Remove("RecordedByUser");
            ModelState.Remove("ReliefProject");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(donation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DonationExists(donation.DonationID))
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

            // Fallback for POST when validation fails
            ViewData["DonorID"] = new SelectList(_context.Donors, "DonorID", "LastName", donation.DonorID);
            ViewData["RecordedByUserId"] = new SelectList(_context.Users, "Id", "Email", donation.RecordedByUserId);
            ViewData["ReliefProjectProjectID"] = new SelectList(_context.ReliefProjects, "ProjectID", "Title", donation.ReliefProjectProjectID);
            return View(donation);
        }

        // GET: Donations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donation = await _context.Donations
                .Include(d => d.Donor)
                .Include(d => d.RecordedByUser)
                .Include(d => d.ReliefProject)
                .FirstOrDefaultAsync(m => m.DonationID == id);
            if (donation == null)
            {
                return NotFound();
            }

            return View(donation);
        }

        // POST: Donations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var donation = await _context.Donations.FindAsync(id);

            if (donation == null)
            {
                // If it's already gone, just redirect back to the list.
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Donations.Remove(donation);
                await _context.SaveChangesAsync();

                // Use TempData for a success message upon redirection
                TempData["SuccessMessage"] = $"Donation recorded on {donation.DateReceived.ToShortDateString()} has been successfully deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                // Handle unexpected database errors (like a rare foreign key constraint if something else references Donation)
                TempData["ErrorMessage"] = "An error occurred while attempting to delete the donation. It may be referenced by other records.";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        private bool DonationExists(int id)
        {
            return _context.Donations.Any(e => e.DonationID == id);
        }
    }
}
