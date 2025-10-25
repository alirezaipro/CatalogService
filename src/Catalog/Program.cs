using Catalog.Endpoints;
using Catalog.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGroup("/api/v1/brands")
    .WithTags("Brands Api")
    .MapCatalogBrandEndpoints();

app.Run();