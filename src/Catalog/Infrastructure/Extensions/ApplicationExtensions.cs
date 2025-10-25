using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Extensions;

public static class ApplicationExtensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        #region Catalog db context

        builder.Services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString(CatalogDbContext.DefaultConnectionStringName)));

        #endregion
        
        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    }
}