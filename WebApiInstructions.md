# Exercise #1

## Goal

In this exercise we will create a backed Web API that will be controlled by our Spring Cloud Gateway implementation.

## Expected Results

At the conclusion of this exercise we will have created a Web API based microservice backed by an in-memory persistent store.  We will be able to hit the 4 API endpoints and should be able to verify the API is producing the expected results.

1. Create a directory for our new API with the following command:  `mkdir product-api`

2. Navigate to the newly created directory using the following command: `cd product-api`

3. Use the Dotnet CLI to scaffold a basic Web API application with the following command: `dotnet new webapi`.  This will create a new application with name product-api.  **Note the project will take the name of the folder that the command is run from unless given a specific name**

4. Now utilize the Dotnet CLI to add nuget packages to your project.

    ```powershell

    dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 3.0.0

    dotnet add package Microsoft.EntityFrameworkCore.Design --version 3.0.0
    ```

5. Create a file named `Product.cs` that will serve as the entity class that represents our store's catalog of products.  The class should have four fields: Id (long), Category (string), Name (string) and Inventory (int).  When complete the class should have the following definition:

    ```c#
    namespace product_api
    {
        public class Product
        {
            public long Id { get; set; }
            public string Category { get; set; }
            public string Name { get; set; }
            public int Inventory { get; set; }
        }
    }
    ```

6. Create a file named `ProductContext.cs` that will serve as our context class that will be utilized by Entity Framework to store our in memory objects.  The class should extend DbContext, define 2 constructors, one without parameters and another which takes DbContextOptions and creates a DbSet of Products.  Finally the class will override the OnModelCreating method.  In this method we will set up data to be seeded when we later create and execute our database migrations.

    ```c#
    using Microsoft.EntityFrameworkCore;

    namespace product_api
    {
        public class ProductContext : DbContext
        {
            public ProductContext(DbContextOptions options) : base(options)
            {
            }

            protected ProductContext()
            {
            }

            public DbSet<Product> Products { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                var products = new []
                {
                    new {Id = 1L, Category = "Books", Inventory = 5, Name="The Ultimate Guide To Budget Travel"},
                    new {Id = 2L, Category = "Sports", Inventory = 4, Name="Upper Deck Baseball Set"},
                    new {Id = 3L, Category = "Groceries", Inventory = 2, Name="Gatorade"},
                    new {Id = 4L, Category = "Electronics", Inventory = 50, Name="Google Pixel 3"},
                    new {Id = 5L, Category = "Home and Garden", Inventory = 20, Name="Kitchenette Stand Mixer"}
                };

                modelBuilder.Entity<Product>().HasData(products);
            }
        }
    }
    ```

7. Once the ProductContext class has been created, we will create an interface to aid with dependency management among other things.  When completed the interface should have the following definition:

    ```c#
    using Microsoft.EntityFrameworkCore;

    namespace product_api
    {
        public interface IProductContext
        {
            DbSet<Product> Products { get; set; }
        }
    }
    ```

8. Navigate to the Startup class and edit the file as follows:

   1. set the following using statements:

        ```c#
        using Microsoft.EntityFrameworkCore;
        ```

   2. In the ConfigureServices method use the following code snippet to add the database context class to the service container.  

        ```c#
        services.AddDbContext<ProductContext>(options =>
        {
            options.UseSqlite("DataSource=:memory:");
        }, ServiceLifetime.Singleton);
        ```

   3. Remove the line `app.UseHttpsRedirection();` from the Configure method.

9. Create a file called `EnsureMigration.cs`.  We will utilize this class when we set up our middleware pipeline to run the Entity Framework Migrations and initially seed our database.  The class should have the following definition.

    ```c#
    using System.Data.Common;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    namespace product_api
    {
        public static class EnsureMigrations
        {
            static DbConnection _connection;
            public static IHost EnsureMigrationOfContext<T>(this IHost webHost) where T : DbContext
            {
                using (var scope = webHost.Services.CreateScope())
                using (var serviceScope = scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var productContext = serviceScope.ServiceProvider.GetService<T>();
                    _connection = productContext.Database.GetDbConnection();
                    _connection.Open();
                    productContext.Database.Migrate();
                }

                return webHost;
            }
        }
    }
    ```

10. Navigate to the Program.cs.  We will utilize the EnsureMigration class to apply our database migrations once the application has started up.  To do this we have the modify our Main method after we build our Web Host.  When complete the Main Method should look like the following snippet:

    ```c#
    public static void Main(string[] args)
    {
        CreateHostBuilder(args)
            .Build()
            .EnsureMigrationOfContext<ProductContext>()
            .Run();
    }
    ```

11. We must now create the actual migrations themselves.  To do this we run the command `dotnet ef migrations add InitialCreation` which will create a folder named Migrations in our solution.  This folder will hold the initial migration files that create our database based on the definition of our Product context and entity class.  **Note if using .NET Core SDK 3.0, you must first install the dotnet ef cli tool which is no longer included in the .NET Core SDK**

12. In the controllers folder create a new class and name it `ProductsController.cs` and then paste the following contents into the file:

    ```c#
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;

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
                var connection = _context.Database.GetDbConnection();
                Console.WriteLine($"Retrieving product catalog from {connection.DataSource}/{connection.Database}");
                return Json(_context.Products.ToList());
            }

            [HttpPost]
            public IActionResult Post()
            {
                var connection = _context.Database.GetDbConnection();
                Console.WriteLine($"Creating new product");
            }
        }
    }
    ```

13. In the root directory navigate to the appsettings.json file and add an entry for spring like the below snippet.

    ```json
    "spring": {
      "application": {
        "name": "product-api"
      }
    }
    ```
