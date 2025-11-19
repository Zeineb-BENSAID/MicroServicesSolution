using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Data
{
    public class ProductDbContext:DbContext
    {
        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\mssqllocaldb;Initial Catalog=ProductDBTraining;Integrated Security=true");
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseLazyLoadingProxies();
        }*/
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
        //la liste des dbsets
        public DbSet<Product> Products { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Category> Categories { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProductConfigurations());
            modelBuilder.Entity<Provider>().Property(prov => prov.Email).IsRequired();
        }
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<DateTime>(p=>p.HaveColumnType("datetime2"));
            configurationBuilder.Properties<String>(p => p.HaveMaxLength(300));
        }
    }
}
