using elasticsearchApi.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Nest;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace elasticsearchApi.Data
{
    public class ApiContext : DbContext
    {
        public ApiContext(DbContextOptions<ApiContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>(x => {
                x.ToTable("Users");
            });
        }

#nullable disable
        public DbSet<Person> Persons { get; set; }
#nullable enable
    }
}
