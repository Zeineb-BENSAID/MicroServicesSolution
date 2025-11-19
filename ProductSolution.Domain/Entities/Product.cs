using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Stock { get; set; }
    //prop de navigation
    public virtual Category? Category { get; set; }
    public virtual ICollection<Provider> Providers { get; set; }
    #region exemples
    //public string? Description2 { get; set; }

    /*public Product(Guid id, string name, decimal price, string description)
    {
        Id = id;
        Name = name;
        Price = price;
        Description = description;
    }*/
    //prop + 2tab
    //public int MyProperty { get; set; }
    //propfull + 2tab
    /*private int myVar;

    public int MyProperty
    {
        get { return myVar; }
        set { myVar = value; }
    }*/
    //propg + 2tab
    //public int MyProperty1 { get; private set; }

    /*public Product()//ctor
    {
            
    }*/
#nullable enable

#nullable disable
    #endregion
}

