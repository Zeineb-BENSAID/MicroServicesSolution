using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Domain.Entities
{
    public class Provider
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        [RegularExpression(@"^(\+216)?[0-9]{8}$")]
        //[Phone] à titre d'exemple
        public string PhoneNumber { get; set; }
        //prop de navig
        public virtual ICollection<Product> Products { get; set; }
    }
}
