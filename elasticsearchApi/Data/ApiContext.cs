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
            modelBuilder.Entity<PersonEntity>(x => {
                x.ToTable("Persons");
                x.Property<int>("Id").HasAnnotation("SqlServer:Identity", "1, 1").IsRequired();
                x.Property<string?>("IIN").IsRequired(false).HasColumnType("nvarchar(max)");
                x.Property<string?>("SIN").IsRequired(false).HasColumnType("nvarchar(max)");
                x.Property<string?>("Last_Name").IsRequired(false).HasColumnType("nvarchar(max)");
                x.Property<string?>("First_Name").IsRequired(false).HasColumnType("nvarchar(max)");
                x.Property<string?>("Middle_Name").IsRequired(false).HasColumnType("nvarchar(max)");
                x.Property<DateTime?>("Date_of_Birth").IsRequired(false).HasColumnType("date");
                x.Property<Guid?>("Sex").IsRequired(false).HasColumnType("uniqueidentifier");
                x.Property<Guid?>("PassportType").IsRequired(false).HasColumnType("uniqueidentifier");
                x.Property<string?>("PassportSeries").IsRequired(false).HasColumnType("nvarchar(50)");
                x.Property<string?>("PassportNo").IsRequired(false).HasColumnType("nvarchar(max)");
                x.Property<DateTime?>("Date_of_Issue").IsRequired(false).HasColumnType("date");
                x.Property<string?>("Issuing_Authority").IsRequired(false).HasColumnType("nvarchar(max)");
                x.Property<Guid?>("FamilyState").IsRequired(false).HasColumnType("uniqueidentifier");

                // DB Computed Fields
                x.Property<DateTime>("CreatedAt").IsRequired(true).HasColumnType("datetime").HasDefaultValueSql("getdate()");
                x.Property<DateTime>("ModifiedAt").IsRequired(true).HasColumnType("datetime").HasDefaultValueSql("getdate()");
                x.Property<bool>("deleted").IsRequired(true).HasColumnType("bit").HasDefaultValue(0);
            });
            modelBuilder.Entity<PassportEntity>(x =>
            {
                x.ToTable("Passports");
                x.HasNoKey();
                x.Property<int>("PersonId").IsRequired();
                x.Property<Guid?>("PassportType").IsRequired(false).HasColumnType("uniqueidentifier");
                x.Property<string?>("PassportSeries").IsRequired(false).HasColumnType("nvarchar(50)");
                x.Property<string?>("PassportNo").IsRequired(false).HasColumnType("nvarchar(50)");
                x.Property<DateTime?>("Date_of_Issue").IsRequired(false).HasColumnType("date");
                x.Property<string?>("Issuing_Authority").IsRequired(false).HasColumnType("nvarchar(50)");
                x.Property<Guid?>("Marital_Status").IsRequired(false).HasColumnType("uniqueidentifier");

                // DB Computed Fields
                x.Property<DateTime>("CreatedAt").IsRequired(true).HasColumnType("datetime").HasDefaultValueSql("getdate()");
            });
            modelBuilder.Entity<AddressEntity>(x => {
                x.ToTable("address");
                x.HasNoKey();
                x.Property<int>("regionNo").IsRequired();
                x.Property<string?>("regionName").IsRequired().HasColumnType("nvarchar(50)");
                x.Property<int>("districtNo").IsRequired();
                x.Property<string?>("districtName").IsRequired().HasColumnType("nvarchar(50)");
            });
            modelBuilder.Entity<User>(x => {
                x.ToTable("Users");
            });

            
        }
        #nullable disable
        public DbSet<PersonEntity> Persons { get; set; }
        public DbSet<User> Users { get; set; }
        #nullable enable
    }
}
