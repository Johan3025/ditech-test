using EnterpriseCQRS.Data.Entities;
using EnterpriseCQRS.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCQRS.Data
{
    public class CommittedCapacityContext : DbContext
    {
        public CommittedCapacityContext( DbContextOptions<CommittedCapacityContext> options) : base(options)
        {
        }

        public DbSet<Transaction> Transaction { get; set; }
        public DbSet<Rates> Rates { get; set; }
    }
}
