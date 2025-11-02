using System.Reflection;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Extensions;

public static class ApplicationExtensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        #region Catalog db context

        builder.Services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString(CatalogDbContext.DefaultConnectionStringName)));

        #endregion

        #region Add mass transit

        builder.Services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        #endregion

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    }
}