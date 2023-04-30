using Microsoft.EntityFrameworkCore;

namespace Api.Core.Entities
{
    public class GuidContext : DbContext
    {
        public DbSet<GuidMetadata> GuidMedata { get; set; }
        public GuidContext(DbContextOptions<GuidContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuidMetadata>().ToTable("GuidMetadata").HasKey("Guid");
        }


    }


    public static class GuidContextExtensions
    {
        /******************************************************************************/
        /*GetGuid                                                                     */
        /*Find a Guid in database                                                     */
        /*In case DbContext get closed                                                */
        /******************************************************************************/
        public static async Task<GuidMetadata?> GetGuid(this GuidContext context, Guid uniqueId)
        {
            return await context.GuidMedata.FindAsync(uniqueId);
        }
    }
}
