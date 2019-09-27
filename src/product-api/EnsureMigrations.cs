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