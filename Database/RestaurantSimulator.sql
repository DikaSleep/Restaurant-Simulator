/*
    RestaurantSimulator
    SQL Server database script
    Includes: tables, primary keys, foreign keys, constraints, indexes, and sample data.

    Run this script in SQL Server Management Studio (SSMS).
    If the database already exists, the CREATE DATABASE statement will fail.
*/

IF DB_ID(N'RestaurantSimulator') IS NULL
BEGIN
    CREATE DATABASE RestaurantSimulator;
END
GO

USE RestaurantSimulator;
GO

/* Optional: remove existing tables in dependency order before rebuilding.
   Uncomment this section only if you want to RESET all data and tables.
*/
/*
DROP TABLE IF EXISTS dbo.Payments;
DROP TABLE IF EXISTS dbo.OrderDetails;
DROP TABLE IF EXISTS dbo.Orders;
DROP TABLE IF EXISTS dbo.Menus;
DROP TABLE IF EXISTS dbo.Categories;
DROP TABLE IF EXISTS dbo.RestaurantTables;
DROP TABLE IF EXISTS dbo.Employees;
DROP TABLE IF EXISTS dbo.Users;
GO
*/

/* 1. Users: accounts used by JWT authentication */
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        UserID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Users PRIMARY KEY,
        FullName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(150) NOT NULL
            CONSTRAINT UQ_Users_Email UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        Role NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Users_Role DEFAULT N'Customer',
        CreatedAt DATETIME2(0) NOT NULL
            CONSTRAINT DF_Users_CreatedAt DEFAULT SYSDATETIME(),
        CONSTRAINT CK_Users_Role
            CHECK (Role IN (N'Admin', N'Customer'))
    );
END
GO

/* 2. Employees */
IF OBJECT_ID(N'dbo.Employees', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employees (
        EmployeeID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Employees PRIMARY KEY,
        FullName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(150) NOT NULL
            CONSTRAINT UQ_Employees_Email UNIQUE,
        PhoneNumber NVARCHAR(20) NULL,
        Position NVARCHAR(50) NOT NULL,
        Salary DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_Employees_Salary DEFAULT (0),
        HireDate DATE NOT NULL
            CONSTRAINT DF_Employees_HireDate DEFAULT (CONVERT(date, GETDATE())),
        IsActive BIT NOT NULL
            CONSTRAINT DF_Employees_IsActive DEFAULT (1),
        CONSTRAINT CK_Employees_Salary CHECK (Salary >= 0)
    );
END
GO

/* 3. RestaurantTables */
IF OBJECT_ID(N'dbo.RestaurantTables', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RestaurantTables (
        TableID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_RestaurantTables PRIMARY KEY,
        TableNumber NVARCHAR(10) NOT NULL
            CONSTRAINT UQ_RestaurantTables_TableNumber UNIQUE,
        Capacity INT NOT NULL,
        Status NVARCHAR(20) NOT NULL
            CONSTRAINT DF_RestaurantTables_Status DEFAULT N'Available',
        CONSTRAINT CK_RestaurantTables_Capacity CHECK (Capacity > 0),
        CONSTRAINT CK_RestaurantTables_Status
            CHECK (Status IN (N'Available', N'Occupied', N'Reserved'))
    );
END
GO

/* 4. Categories */
IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories (
        CategoryID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Categories PRIMARY KEY,
        CategoryName NVARCHAR(100) NOT NULL
            CONSTRAINT UQ_Categories_CategoryName UNIQUE,
        Description NVARCHAR(255) NULL
    );
END
GO

/* 5. Menus */
IF OBJECT_ID(N'dbo.Menus', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Menus (
        MenuID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Menus PRIMARY KEY,
        CategoryID INT NOT NULL,
        MenuName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(255) NULL,
        Price DECIMAL(18,2) NOT NULL,
        Stock INT NOT NULL
            CONSTRAINT DF_Menus_Stock DEFAULT (0),
        ImageUrl NVARCHAR(255) NULL,
        IsAvailable BIT NOT NULL
            CONSTRAINT DF_Menus_IsAvailable DEFAULT (1),
        CONSTRAINT FK_Menus_Categories
            FOREIGN KEY (CategoryID) REFERENCES dbo.Categories(CategoryID),
        CONSTRAINT CK_Menus_Price CHECK (Price >= 0),
        CONSTRAINT CK_Menus_Stock CHECK (Stock >= 0)
    );
END
GO

/* 6. Orders */
IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders (
        OrderID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Orders PRIMARY KEY,
        UserID INT NOT NULL,
        EmployeeID INT NULL,
        TableID INT NOT NULL,
        OrderDate DATETIME2(0) NOT NULL
            CONSTRAINT DF_Orders_OrderDate DEFAULT SYSDATETIME(),
        Status NVARCHAR(30) NOT NULL
            CONSTRAINT DF_Orders_Status DEFAULT N'Pending',
        TotalAmount DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_Orders_TotalAmount DEFAULT (0),
        Notes NVARCHAR(255) NULL,
        CONSTRAINT FK_Orders_Users
            FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
        CONSTRAINT FK_Orders_Employees
            FOREIGN KEY (EmployeeID) REFERENCES dbo.Employees(EmployeeID),
        CONSTRAINT FK_Orders_RestaurantTables
            FOREIGN KEY (TableID) REFERENCES dbo.RestaurantTables(TableID),
        CONSTRAINT CK_Orders_Status
            CHECK (Status IN (N'Pending', N'Preparing', N'Ready',
                              N'Completed', N'Cancelled')),
        CONSTRAINT CK_Orders_TotalAmount CHECK (TotalAmount >= 0)
    );
END
GO

/* 7. OrderDetails */
IF OBJECT_ID(N'dbo.OrderDetails', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderDetails (
        OrderDetailID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_OrderDetails PRIMARY KEY,
        OrderID INT NOT NULL,
        MenuID INT NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        Subtotal AS (Quantity * UnitPrice) PERSISTED,
        CONSTRAINT FK_OrderDetails_Orders
            FOREIGN KEY (OrderID) REFERENCES dbo.Orders(OrderID),
        CONSTRAINT FK_OrderDetails_Menus
            FOREIGN KEY (MenuID) REFERENCES dbo.Menus(MenuID),
        CONSTRAINT CK_OrderDetails_Quantity CHECK (Quantity > 0),
        CONSTRAINT CK_OrderDetails_UnitPrice CHECK (UnitPrice >= 0)
    );
END
GO

/* 8. Payments */
IF OBJECT_ID(N'dbo.Payments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payments (
        PaymentID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Payments PRIMARY KEY,
        OrderID INT NOT NULL
            CONSTRAINT UQ_Payments_OrderID UNIQUE,
        PaymentMethod NVARCHAR(30) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        PaymentDate DATETIME2(0) NOT NULL
            CONSTRAINT DF_Payments_PaymentDate DEFAULT SYSDATETIME(),
        Status NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Payments_Status DEFAULT N'Pending',
        ReferenceNumber NVARCHAR(100) NULL,
        CONSTRAINT FK_Payments_Orders
            FOREIGN KEY (OrderID) REFERENCES dbo.Orders(OrderID),
        CONSTRAINT CK_Payments_Method
            CHECK (PaymentMethod IN (N'Cash', N'DebitCard',
                                     N'CreditCard', N'EWallet')),
        CONSTRAINT CK_Payments_Status
            CHECK (Status IN (N'Pending', N'Paid', N'Failed', N'Refunded')),
        CONSTRAINT CK_Payments_Amount CHECK (Amount >= 0)
    );
END
GO

/* Helpful indexes for common foreign-key lookups */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Menus_CategoryID' AND object_id = OBJECT_ID(N'dbo.Menus'))
    CREATE INDEX IX_Menus_CategoryID ON dbo.Menus(CategoryID);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Orders_UserID' AND object_id = OBJECT_ID(N'dbo.Orders'))
    CREATE INDEX IX_Orders_UserID ON dbo.Orders(UserID);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Orders_EmployeeID' AND object_id = OBJECT_ID(N'dbo.Orders'))
    CREATE INDEX IX_Orders_EmployeeID ON dbo.Orders(EmployeeID);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Orders_TableID' AND object_id = OBJECT_ID(N'dbo.Orders'))
    CREATE INDEX IX_Orders_TableID ON dbo.Orders(TableID);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OrderDetails_OrderID' AND object_id = OBJECT_ID(N'dbo.OrderDetails'))
    CREATE INDEX IX_OrderDetails_OrderID ON dbo.OrderDetails(OrderID);
GO

/* SAMPLE DATA
   PasswordHash is intentionally a placeholder.
   Register users through your ASP.NET Core API and store a real password hash.
   Do not use the placeholder as a login password.
*/
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'admin@restaurant.com')
    INSERT INTO dbo.Users (FullName, Email, PasswordHash, Role)
    VALUES (N'Administrator', N'admin@restaurant.com', N'REPLACE_WITH_REAL_PASSWORD_HASH', N'Admin');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'andika@example.com')
    INSERT INTO dbo.Users (FullName, Email, PasswordHash, Role)
    VALUES (N'Andika Pratama', N'andika@example.com', N'REPLACE_WITH_REAL_PASSWORD_HASH', N'Customer');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'budi@example.com')
    INSERT INTO dbo.Users (FullName, Email, PasswordHash, Role)
    VALUES (N'Budi Santoso', N'budi@example.com', N'REPLACE_WITH_REAL_PASSWORD_HASH', N'Customer');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE Email = N'rizky@restaurant.com')
    INSERT INTO dbo.Employees (FullName, Email, PhoneNumber, Position, Salary)
    VALUES (N'Rizky Ramadhan', N'rizky@restaurant.com', N'081234567801', N'Manager', 5000000);

IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE Email = N'siti@restaurant.com')
    INSERT INTO dbo.Employees (FullName, Email, PhoneNumber, Position, Salary)
    VALUES (N'Siti Amelia', N'siti@restaurant.com', N'081234567802', N'Waiter', 3000000);

IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE Email = N'doni@restaurant.com')
    INSERT INTO dbo.Employees (FullName, Email, PhoneNumber, Position, Salary)
    VALUES (N'Doni Saputra', N'doni@restaurant.com', N'081234567803', N'Chef', 4000000);

IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE Email = N'rina@restaurant.com')
    INSERT INTO dbo.Employees (FullName, Email, PhoneNumber, Position, Salary)
    VALUES (N'Rina Putri', N'rina@restaurant.com', N'081234567804', N'Cashier', 3200000);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.RestaurantTables WHERE TableNumber = N'A1')
    INSERT INTO dbo.RestaurantTables (TableNumber, Capacity) VALUES (N'A1', 2);
IF NOT EXISTS (SELECT 1 FROM dbo.RestaurantTables WHERE TableNumber = N'A2')
    INSERT INTO dbo.RestaurantTables (TableNumber, Capacity) VALUES (N'A2', 2);
IF NOT EXISTS (SELECT 1 FROM dbo.RestaurantTables WHERE TableNumber = N'B1')
    INSERT INTO dbo.RestaurantTables (TableNumber, Capacity) VALUES (N'B1', 4);
IF NOT EXISTS (SELECT 1 FROM dbo.RestaurantTables WHERE TableNumber = N'B2')
    INSERT INTO dbo.RestaurantTables (TableNumber, Capacity) VALUES (N'B2', 4);
IF NOT EXISTS (SELECT 1 FROM dbo.RestaurantTables WHERE TableNumber = N'C1')
    INSERT INTO dbo.RestaurantTables (TableNumber, Capacity) VALUES (N'C1', 6);
IF NOT EXISTS (SELECT 1 FROM dbo.RestaurantTables WHERE TableNumber = N'C2')
    INSERT INTO dbo.RestaurantTables (TableNumber, Capacity) VALUES (N'C2', 6);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryName = N'Main Course')
    INSERT INTO dbo.Categories (CategoryName, Description) VALUES (N'Main Course', N'Makanan utama');
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryName = N'Appetizer')
    INSERT INTO dbo.Categories (CategoryName, Description) VALUES (N'Appetizer', N'Makanan pembuka');
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryName = N'Dessert')
    INSERT INTO dbo.Categories (CategoryName, Description) VALUES (N'Dessert', N'Makanan penutup');
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryName = N'Beverage')
    INSERT INTO dbo.Categories (CategoryName, Description) VALUES (N'Beverage', N'Minuman');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Menus WHERE MenuName = N'Fried Rice')
    INSERT INTO dbo.Menus (CategoryID, MenuName, Description, Price, Stock, ImageUrl)
    SELECT CategoryID, N'Fried Rice', N'Special fried rice', 25000, 50, N'fried-rice.jpg'
    FROM dbo.Categories WHERE CategoryName = N'Main Course';

IF NOT EXISTS (SELECT 1 FROM dbo.Menus WHERE MenuName = N'Chicken Steak')
    INSERT INTO dbo.Menus (CategoryID, MenuName, Description, Price, Stock, ImageUrl)
    SELECT CategoryID, N'Chicken Steak', N'Grilled chicken steak', 35000, 30, N'chicken-steak.jpg'
    FROM dbo.Categories WHERE CategoryName = N'Main Course';

IF NOT EXISTS (SELECT 1 FROM dbo.Menus WHERE MenuName = N'Beef Burger')
    INSERT INTO dbo.Menus (CategoryID, MenuName, Description, Price, Stock, ImageUrl)
    SELECT CategoryID, N'Beef Burger', N'Classic beef burger', 30000, 25, N'beef-burger.jpg'
    FROM dbo.Categories WHERE CategoryName = N'Main Course';

IF NOT EXISTS (SELECT 1 FROM dbo.Menus WHERE MenuName = N'French Fries')
    INSERT INTO dbo.Menus (CategoryID, MenuName, Description, Price, Stock, ImageUrl)
    SELECT CategoryID, N'French Fries', N'Crispy french fries', 15000, 40, N'fries.jpg'
    FROM dbo.Categories WHERE CategoryName = N'Appetizer';

IF NOT EXISTS (SELECT 1 FROM dbo.Menus WHERE MenuName = N'Chocolate Cake')
    INSERT INTO dbo.Menus (CategoryID, MenuName, Description, Price, Stock, ImageUrl)
    SELECT CategoryID, N'Chocolate Cake', N'Chocolate dessert cake', 20000, 20, N'cake.jpg'
    FROM dbo.Categories WHERE CategoryName = N'Dessert';

IF NOT EXISTS (SELECT 1 FROM dbo.Menus WHERE MenuName = N'Iced Tea')
    INSERT INTO dbo.Menus (CategoryID, MenuName, Description, Price, Stock, ImageUrl)
    SELECT CategoryID, N'Iced Tea', N'Fresh iced tea', 8000, 100, N'iced-tea.jpg'
    FROM dbo.Categories WHERE CategoryName = N'Beverage';

IF NOT EXISTS (SELECT 1 FROM dbo.Menus WHERE MenuName = N'Orange Juice')
    INSERT INTO dbo.Menus (CategoryID, MenuName, Description, Price, Stock, ImageUrl)
    SELECT CategoryID, N'Orange Juice', N'Fresh orange juice', 12000, 50, N'orange-juice.jpg'
    FROM dbo.Categories WHERE CategoryName = N'Beverage';
GO

/* Insert sample orders only when none exist for these sample customers. */
IF NOT EXISTS (
    SELECT 1 FROM dbo.Orders o
    INNER JOIN dbo.Users u ON u.UserID = o.UserID
    WHERE u.Email = N'andika@example.com' AND o.Notes = N'Sample order - less spicy'
)
BEGIN
    INSERT INTO dbo.Orders (UserID, EmployeeID, TableID, Status, TotalAmount, Notes)
    SELECT u.UserID, e.EmployeeID, t.TableID, N'Preparing', 58000, N'Sample order - less spicy'
    FROM dbo.Users u
    CROSS JOIN dbo.Employees e
    CROSS JOIN dbo.RestaurantTables t
    WHERE u.Email = N'andika@example.com'
      AND e.Email = N'siti@restaurant.com'
      AND t.TableNumber = N'A1';
END;

IF NOT EXISTS (
    SELECT 1 FROM dbo.Orders o
    INNER JOIN dbo.Users u ON u.UserID = o.UserID
    WHERE u.Email = N'budi@example.com' AND o.Notes = N'Sample order - no additional notes'
)
BEGIN
    INSERT INTO dbo.Orders (UserID, EmployeeID, TableID, Status, TotalAmount, Notes)
    SELECT u.UserID, e.EmployeeID, t.TableID, N'Completed', 53000, N'Sample order - no additional notes'
    FROM dbo.Users u
    CROSS JOIN dbo.Employees e
    CROSS JOIN dbo.RestaurantTables t
    WHERE u.Email = N'budi@example.com'
      AND e.Email = N'siti@restaurant.com'
      AND t.TableNumber = N'B1';
END;
GO

/* Add sample order details only if the sample orders have no details. */
DECLARE @OrderAndika INT =
(
    SELECT TOP (1) o.OrderID
    FROM dbo.Orders o
    INNER JOIN dbo.Users u ON u.UserID = o.UserID
    WHERE u.Email = N'andika@example.com'
      AND o.Notes = N'Sample order - less spicy'
    ORDER BY o.OrderID
);

IF @OrderAndika IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.OrderDetails WHERE OrderID = @OrderAndika)
BEGIN
    INSERT INTO dbo.OrderDetails (OrderID, MenuID, Quantity, UnitPrice)
    SELECT @OrderAndika, MenuID, 2, 25000 FROM dbo.Menus WHERE MenuName = N'Fried Rice'
    UNION ALL
    SELECT @OrderAndika, MenuID, 1, 8000 FROM dbo.Menus WHERE MenuName = N'Iced Tea';
END;

DECLARE @OrderBudi INT =
(
    SELECT TOP (1) o.OrderID
    FROM dbo.Orders o
    INNER JOIN dbo.Users u ON u.UserID = o.UserID
    WHERE u.Email = N'budi@example.com'
      AND o.Notes = N'Sample order - no additional notes'
    ORDER BY o.OrderID
);

IF @OrderBudi IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.OrderDetails WHERE OrderID = @OrderBudi)
BEGIN
    INSERT INTO dbo.OrderDetails (OrderID, MenuID, Quantity, UnitPrice)
    SELECT @OrderBudi, MenuID, 1, 30000 FROM dbo.Menus WHERE MenuName = N'Beef Burger'
    UNION ALL
    SELECT @OrderBudi, MenuID, 1, 8000 FROM dbo.Menus WHERE MenuName = N'Iced Tea'
    UNION ALL
    SELECT @OrderBudi, MenuID, 1, 15000 FROM dbo.Menus WHERE MenuName = N'French Fries';
END;
GO

/* Recalculate sample order totals from their details. */
UPDATE o
SET TotalAmount = totals.CalculatedTotal
FROM dbo.Orders o
CROSS APPLY (
    SELECT SUM(od.Subtotal) AS CalculatedTotal
    FROM dbo.OrderDetails od
    WHERE od.OrderID = o.OrderID
) totals
WHERE totals.CalculatedTotal IS NOT NULL;
GO

/* Sample payment for Budi's completed order. */
IF NOT EXISTS (
    SELECT 1
    FROM dbo.Payments p
    INNER JOIN dbo.Orders o ON o.OrderID = p.OrderID
    WHERE o.Notes = N'Sample order - no additional notes'
)
BEGIN
    INSERT INTO dbo.Payments (OrderID, PaymentMethod, Amount, Status, ReferenceNumber)
    SELECT TOP (1) o.OrderID, N'Cash', o.TotalAmount, N'Paid', N'PAY-SAMPLE-001'
    FROM dbo.Orders o
    WHERE o.Notes = N'Sample order - no additional notes'
    ORDER BY o.OrderID;
END;
GO

/* Verification queries */
SELECT * FROM dbo.Users;
SELECT * FROM dbo.Employees;
SELECT * FROM dbo.RestaurantTables;
SELECT * FROM dbo.Categories;
SELECT * FROM dbo.Menus;
SELECT * FROM dbo.Orders;
SELECT * FROM dbo.OrderDetails;
SELECT * FROM dbo.Payments;
GO
