using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.Interfaces;
using ProductService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Services
{
    public class ADProductService:GenericRepository<Product>,IADProductService
    {
        public ADProductService(ProductDbContext _context) :base(_context)
        {
                
        }
        //implémentation de toutes les méthodes autres que CRUD

    }
}
