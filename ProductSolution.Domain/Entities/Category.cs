using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Domain.Entities
{
    public class Category
    {
        [Key]
        public string CategoryCode { get; set; }
        // default conventions de EF
        //public int Id { get; set; }
        //public int CategoryId { get; set; }    
        [StringLength(15,ErrorMessage ="The max lenght of this field is 15C")]
        [Column("CategoryName"),Required(ErrorMessage ="This field is required !!")]
        public string Name { get; set; }
        //prop de navig
        public virtual ICollection<Product>? Products{ get; set; }
    }
}
