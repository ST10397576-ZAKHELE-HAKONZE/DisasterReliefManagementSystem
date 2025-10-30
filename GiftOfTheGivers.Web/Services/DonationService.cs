using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GiftOfTheGivers.Web.Models;

namespace GiftOfTheGivers.Web.Services
{
    public class DonationService
    {
        private readonly ApplicationDbContext _context;

        public DonationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Donation>> GetRecentDonationsAsync(int count = 5)
        {
            return await _context.Donations
                .Include(d => d.Donor)
                .Include(d => d.ReliefProject)
                .Where(d => d.Status == DonationStatus.Received)
                .OrderByDescending(d => d.DateReceived)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Donation> GetDonationByIdAsync(int id)
        {
            return await _context.Donations
                .Include(d => d.Donor)
                .Include(d => d.ReliefProject)
                .FirstOrDefaultAsync(d => d.DonationID == id);
        }

        public async Task AddDonationAsync(Donation donation)
        {
            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDonationAsync(Donation donation)
        {
            _context.Donations.Update(donation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDonationAsync(int id)
        {
            var donation = await _context.Donations.FindAsync(id);
            if (donation != null)
            {
                _context.Donations.Remove(donation);
                await _context.SaveChangesAsync();
            }
        }
    }
}