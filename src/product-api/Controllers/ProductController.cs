using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace product_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : Controller
    {
        private readonly ProductContext _context;
        public ProductsController([FromServices] ProductContext context)
        {
            _context = context;
        }

        // GET api/products
        [HttpGet]
        public IActionResult Get()
        {
            return Json(_context.Products.ToList());
        }

        [HttpGet("{id}")]
        public IActionResult Get(long id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product is null)
                return NotFound();
            return Json(product);
        }

        [HttpGet("[Action]")]
        public IActionResult DefaultProduct()
        {
            return Json
            (
                new Product
                {
                    Category = "Default Category",
                    Inventory = 100,
                    Name = "Default Product"
                }
            );
        }

        [HttpGet("[Action]")]
        public IActionResult BlueInventory()
        {
            var inventory = _context.Products.Average(p => p.Inventory);
            return Json(new {Inventory = inventory});
        }

        [HttpGet("[Action]")]
        public IActionResult GreenInventory()
        {
            var inventory = _context.Products.Sum(p => p.Inventory);
            return Json(new {Inventory = inventory});
        }
        
        [HttpPost]
        public IActionResult Post()
        {
            long id = _context.Products.Count() + 1L;
            var product =_context.Products.Add(new Product {Id= id,Category= "Newly Introduced Products",Inventory=1,Name=$"New Product {id}"});
            _context.SaveChanges();
            return CreatedAtAction("Get", new {id = product.Entity.Id}, product.Entity);
        }
    }
}