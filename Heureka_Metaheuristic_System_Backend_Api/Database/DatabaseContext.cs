using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Heureka_Metaheuristic_System_Backend_Api.Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public bool Disposed { get; private set; }

        public override void Dispose()
        {
            Disposed = true;
            base.Dispose();
        }

        public override ValueTask DisposeAsync()
        {
            Disposed = true;

            return base.DisposeAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.ApplyConfigurationsFromAssembly(
                Assembly.GetExecutingAssembly()
            );
        }
    }
}
