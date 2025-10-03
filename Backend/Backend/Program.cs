using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Order Management API",
        Description = "Una API simple para gestionar órdenes construida con .NET Minimal APIs. que permite crear, leer, actualizar y eliminar órdenes.",
    });
});

var orders = new List<OrderRecord>();
builder.Services.AddSingleton(orders);

builder.Services.AddCors(options =>
{
    options.AddPolicy("VueAppPolicy",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{

}

app.UseHttpsRedirection();
app.UseCors("VueAppPolicy");

IResult ValidateOrder(OrderDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Client))
    {
        return Results.BadRequest(new { error = "Please provide a client name" });
    }

    if (dto.Total <= 0)
    {
        return Results.BadRequest(new { error = "The total must be greater than 0" });
    }

    return Results.Ok();
}

app.MapGet("/orders", (List<OrderRecord> orders) => Results.Ok(orders))
    .WithName("GetAllOrders")
    .WithTags("Orders")
    .Produces<List<OrderRecord>>(StatusCodes.Status200OK)
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Obtiene todas las órdenes.",
        Description = "Devuelve una lista de todas las órdenes actualmente en memoria."
    });

app.MapGet("/orders/{id}", (Guid id, List<OrderRecord> orders) =>
    orders.FirstOrDefault(o => o.Id == id) is OrderRecord order
        ? Results.Ok(order)
        : Results.NotFound())
    .WithName("GetOrderById")
    .WithTags("Orders")
    .Produces<OrderRecord>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Obtiene una orden por su ID.",
        Description = "Busca una orden específica usando su identificador único (GUID)."
    });

app.MapPost("/orders", (OrderDto dto, List<OrderRecord> orders) =>
{
    var validationResult = ValidateOrder(dto);
    if (validationResult is BadRequest<object>) return validationResult;

    var newOrder = new OrderRecord(
        Id: Guid.NewGuid(),
        Client: dto.Client,
        Date: dto.Date,
        Total: dto.Total
    );
    orders.Add(newOrder);
    return Results.Created($"/orders/{newOrder.Id}", newOrder);
})
.WithName("CreateOrder")
.WithTags("Orders")
.Produces<OrderRecord>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.WithOpenApi(operation => new(operation)
{
    Summary = "Crea una nueva orden.",
    Description = "Recibe los datos de una orden, la valida y la almacena en memoria. Los datos que recibe son el cliente, la fecha y el total."
});


app.MapPut("orders/{id}", (Guid id, OrderDto dto, List<OrderRecord> orders) =>
{
    var validationResult = ValidateOrder(dto);
    if (validationResult is BadRequest<object>) return validationResult;

    var existingOrder = orders.FirstOrDefault(o => o.Id == id);
    if (existingOrder == null) return Results.NotFound();

    var updatedOrder = existingOrder with
    {
        Client = dto.Client,
        Date = dto.Date,
        Total = dto.Total
    };

    orders.Remove(existingOrder);
    orders.Add(updatedOrder);
    return Results.Ok(updatedOrder);
})
.WithName("UpdateOrder")
.WithTags("Orders")
.Produces<OrderRecord>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound)
.WithOpenApi(operation => new(operation)
{
    Summary = "Actualiza una orden existente.",
    Description = "Busca una orden por su ID y actualiza sus datos con la información proporcionada."
});

app.MapDelete("/orders/{id}", (Guid id, List<OrderRecord> orders) =>
{
    var orderDelete = orders.FirstOrDefault(o => o.Id == id);
    if (orderDelete == null) return Results.NotFound();

    orders.Remove(orderDelete);
    return Results.NoContent();
})
.WithName("DeleteOrder")
.WithTags("Orders")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound)
.WithOpenApi(operation => new(operation)
{
    Summary = "Elimina una orden.",
    Description = "Busca y elimina una orden de la memoria usando su ID."
});


app.Run();

public record OrderRecord(Guid Id, string Client, DateTime Date, decimal Total);
