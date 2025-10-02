using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var orders = new List<Order>();
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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

app.MapGet("/orders", (List<Order> orders) => Results.Ok(orders));

app.MapGet("/orders/{id}", (Guid ApplicationId, List<Order> orders) =>
    orders.FirstOrDefault(o => o.Id == ApplicationId) is Order order
        ? Results.Ok(order)
        : Results.NotFound());

app.MapPost("/orders", (OrderDto dto, List<Order> orders) =>
{
    var validationResult = ValidateOrder(dto);
    if (validationResult is BadRequest) return validationResult;

    var newOrder = new Order(
        Id: Guid.NewGuid(),
        Client: dto.Client,
        Date: dto.Date,
        Total: dto.Total
    );
    orders.Add(newOrder);
    return Results.Created($"/orders/{newOrder.Id}", newOrder);
});

app.MapPut("orders/{id}", (Guid id, OrderDto dto, List<Order> orders) =>
{
    var validationResult = ValidateOrder(dto);
    if (validationResult is BadRequest) return validationResult;

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
});

app.MapDelete("/orders/{id}", (Guid id, List<Order> orders) =>
{
    var orderDelete = orders.FirstOrDefault(o => o.Id == id);
    if (orderDelete == null) return Results.NotFound();
    
    orders.Remove(orderDelete);
    return Results.NoContent();
});

app.Run();

