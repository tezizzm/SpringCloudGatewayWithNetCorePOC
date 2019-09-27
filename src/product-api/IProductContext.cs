using Microsoft.EntityFrameworkCore;

namespace product_api
{
    public interface IProductContext
    {
        DbSet<Product> Products { get; set; }
    }
}