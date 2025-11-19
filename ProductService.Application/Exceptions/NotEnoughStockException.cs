using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Exceptions
{
    public class NotEnoughStockException:Exception
    {
        public NotEnoughStockException():base("Not enough stock available for the requested product.")
        {
        }
        public NotEnoughStockException(string message):base(message)
        {
        }
        public NotEnoughStockException(string message, Exception innerException):base(message,innerException)
        {
        }
    }
}
