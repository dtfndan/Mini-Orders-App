

using System;

public record Order(
    Guid Id,
    string Client,
    DateTime Date,
    decimal Total
);

public record OrderDto(
    string Client,
    DateTime Date,
    decimal Total
);
