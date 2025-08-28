CREATE TABLE OutboxMessages (
    Id UUID NOT NULL PRIMARY KEY,
    ExchangeName TEXT NOT NULL,
    RoutingKey TEXT NOT NULL,
    Payload JSONB NOT NULL,
    Status INT NOT NULL,
    UpdatedAt TIMESTAMP NULL,
    CreatedAt TIMESTAMP NOT NULL,
);