using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Configurations
{
    public class ProductConfigurations : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // many to many : renommer la table d'association
            builder.HasMany(p=>p.Providers).WithMany(prov=>prov.Products).
                UsingEntity(j=>j.ToTable("Providings")); // totable pour renommer 
            //builder.Property(p => p.Name.ToLower().StartsWith("a")).IsRequired();
           // builder.HasOne(p => p.Name).WithOne();
        }
    }
}
