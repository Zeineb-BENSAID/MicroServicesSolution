using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Services
{
    public static class Extensions
    {
        public static string ToUpperProductName(this ProductServices pservice, Guid id)
        {
            if(pservice.GetByIdAsync(id)!=null)
                return pservice.GetByIdAsync(id).Result.Name.ToUpper();
            return "Not Found !";
        }
    }
}
