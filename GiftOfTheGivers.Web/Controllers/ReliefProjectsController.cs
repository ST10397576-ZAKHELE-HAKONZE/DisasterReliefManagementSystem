using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Web.Controllers
{
    public class ReliefProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReliefProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ReliefProjects
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ReliefProjects.Include(r => r.Coordinator);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ReliefProjects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reliefProject = await _context.ReliefProjects
                .Include(r => r.Coordinator)
                .FirstOrDefaultAsync(m => m.ProjectID == id);
            if (reliefProject == null)
            {
                return NotFound();
            }

            return View(reliefProject);
        }

        // GET: ReliefProjects/Create
        public IActionResult Create()
        {
            // FIX: Change "Id" to "Email" to display a friendly user name
            ViewData["CoordinatorId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: ReliefProjects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectID,Title,Location,Description,Status,StartDate,EndDate,CoordinatorId")] ReliefProject reliefProject)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reliefProject);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CoordinatorId"] = new SelectList(_context.Users, "Id", "Email", reliefProject.CoordinatorId);
            return View(reliefProject);
        }

        // GET: ReliefProjects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reliefProject = await _context.ReliefProjects.FindAsync(id);
            if (reliefProject == null)
            {
                return NotFound();
            }
            ViewData["CoordinatorId"] = new SelectList(_context.Users, "Id", "Email", reliefProject.CoordinatorId);
            return View(reliefProject);
        }

        // POST: ReliefProjects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProjectID,Title,Location,Description,Status,StartDate,EndDate,CoordinatorId")] ReliefProject reliefProject)
        {
            if (id != reliefProject.ProjectID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reliefProject);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReliefProjectExists(reliefProject.ProjectID))
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
            ViewData["CoordinatorId"] = new SelectList(_context.Users, "Id", "Id", reliefProject.CoordinatorId);
            return View(reliefProject);
        }

        // GET: ReliefProjects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reliefProject = await _context.ReliefProjects
                .Include(r => r.Coordinator)
                .FirstOrDefaultAsync(m => m.ProjectID == id);
            if (reliefProject == null)
            {
                return NotFound();
            }

            return View(reliefProject);
        }

        // POST: ReliefProjects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reliefProject = await _context.ReliefProjects.FindAsync(id);
            if (reliefProject != null)
            {
                _context.ReliefProjects.Remove(reliefProject);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReliefProjectExists(int id)
        {
            return _context.ReliefProjects.Any(e => e.ProjectID == id);
        }
    }
}
