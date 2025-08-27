CREATE TABLE Customers (
    Id UUID NOT NULL PRIMARY KEY,
    Name TEXT NOT NULL,
    Email TEXT NOT NULL,
    Address TEXT NOT NULL,
    State TEXT NOT NULL,
    ZipCode TEXT NOT NULL,
    Country TEXT NOT NULL,
    DateOfBirth DATE,
    CreatedAt TIMESTAMP NOT NULL,
    
    CONSTRAINT Email_Unique UNIQUE(Email)
);

ALTER TABLE Orders ADD CONSTRAINT FK_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id);