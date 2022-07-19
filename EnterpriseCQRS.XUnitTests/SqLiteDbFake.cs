using EnterpriseCQRS.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCQRS.XUnitTests
{
    public class SqLiteDbFake
    {
        private readonly DbContextOptions<CommittedCapacityContext> options;

        public SqLiteDbFake()
        {
            options = GetDbContextOptions;
        }

        public CommittedCapacityContext GetDbContext()
        {
            CommittedCapacityContext context = new CommittedCapacityContext(options);

            context.Database.EnsureCreated();

            return context;
        }

        private DbContextOptions<CommittedCapacityContext> GetDbContextOptions
        {
            get
            {
                SqliteConnection connection = new SqliteConnection("DataSource=:memory:");

                connection.Open();

                DbContextOptions<CommittedCapacityContext> options = new DbContextOptionsBuilder<CommittedCapacityContext>()
                    .UseSqlite(connection)
                    .Options;

                return options;
            }
        }
    }
}
