using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Features.PayoutService.Models;

namespace msih.p4g.Server.Common.Data
{
    public partial class ApplicationDbContext
    {
        public DbSet<Payout> Payouts { get; set; }
    }
}
