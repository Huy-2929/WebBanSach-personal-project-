-- =============================================
-- DATABASE WEBSITE BÁN SÁCH
-- =============================================

IF DB_ID('WebBanSach') IS NULL
BEGIN
    CREATE DATABASE WebBanSach;
END
GO

USE WebBanSach;
GO


-- =============================================
-- XÓA TABLE CŨ NẾU ĐÃ TỒN TẠI
-- =============================================

IF OBJECT_ID('OrderDetails', 'U') IS NOT NULL
    DROP TABLE OrderDetails;
GO

IF OBJECT_ID('Orders', 'U') IS NOT NULL
    DROP TABLE Orders;
GO

IF OBJECT_ID('Books', 'U') IS NOT NULL
    DROP TABLE Books;
GO

IF OBJECT_ID('Publishers', 'U') IS NOT NULL
    DROP TABLE Publishers;
GO

IF OBJECT_ID('Authors', 'U') IS NOT NULL
    DROP TABLE Authors;
GO

IF OBJECT_ID('Categories', 'U') IS NOT NULL
    DROP TABLE Categories;
GO

IF OBJECT_ID('Users', 'U') IS NOT NULL
    DROP TABLE Users;
GO


-- =============================================
-- 1. USERS
-- =============================================

CREATE TABLE Users
(
    UserId INT IDENTITY(1,1) PRIMARY KEY,

    FullName NVARCHAR(100) NOT NULL,

    Email VARCHAR(150) NOT NULL,

    PasswordHash VARCHAR(500) NOT NULL,

    Phone VARCHAR(20),

    Address NVARCHAR(255),

    Role VARCHAR(20) NOT NULL DEFAULT 'Customer',

    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT UQ_Users_Email
        UNIQUE (Email),

    CONSTRAINT CK_Users_Role
        CHECK (Role IN ('Customer', 'Admin'))
);
GO


-- =============================================
-- 2. CATEGORIES
-- =============================================

CREATE TABLE Categories
(
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,

    CategoryName NVARCHAR(100) NOT NULL,

    Description NVARCHAR(500),

    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT UQ_Categories_CategoryName
        UNIQUE (CategoryName)
);
GO


-- =============================================
-- 3. AUTHORS
-- =============================================

CREATE TABLE Authors
(
    AuthorId INT IDENTITY(1,1) PRIMARY KEY,

    AuthorName NVARCHAR(150) NOT NULL,

    Biography NVARCHAR(MAX),

    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO


-- =============================================
-- 4. PUBLISHERS
-- =============================================

CREATE TABLE Publishers
(
    PublisherId INT IDENTITY(1,1) PRIMARY KEY,

    PublisherName NVARCHAR(150) NOT NULL,

    Address NVARCHAR(255),

    Phone VARCHAR(20),

    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT UQ_Publishers_PublisherName
        UNIQUE (PublisherName)
);
GO


-- =============================================
-- 5. BOOKS
-- =============================================

CREATE TABLE Books
(
    BookId INT IDENTITY(1,1) PRIMARY KEY,

    BookName NVARCHAR(250) NOT NULL,

    ISBN VARCHAR(20),

    Price DECIMAL(18,2) NOT NULL,

    Quantity INT NOT NULL DEFAULT 0,

    Description NVARCHAR(MAX),

    ImageUrl VARCHAR(500),

    PublishedYear INT,

    CategoryId INT NOT NULL,

    AuthorId INT NOT NULL,

    PublisherId INT NOT NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    UpdatedAt DATETIME2 NULL,


    CONSTRAINT CK_Books_Price
        CHECK (Price >= 0),

    CONSTRAINT CK_Books_Quantity
        CHECK (Quantity >= 0),

    CONSTRAINT CK_Books_PublishedYear
        CHECK (
            PublishedYear IS NULL
            OR PublishedYear >= 0
        ),

    CONSTRAINT UQ_Books_ISBN
        UNIQUE (ISBN),

    CONSTRAINT FK_Books_Categories
        FOREIGN KEY (CategoryId)
        REFERENCES Categories(CategoryId),

    CONSTRAINT FK_Books_Authors
        FOREIGN KEY (AuthorId)
        REFERENCES Authors(AuthorId),

    CONSTRAINT FK_Books_Publishers
        FOREIGN KEY (PublisherId)
        REFERENCES Publishers(PublisherId)
);
GO


-- =============================================
-- 6. ORDERS
-- =============================================

CREATE TABLE Orders
(
    OrderId INT IDENTITY(1,1) PRIMARY KEY,

    UserId INT NOT NULL,

    OrderDate DATETIME2 NOT NULL DEFAULT GETDATE(),

    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,

    Status VARCHAR(30) NOT NULL DEFAULT 'Pending',

    ShippingName NVARCHAR(100) NOT NULL,

    ShippingPhone VARCHAR(20) NOT NULL,

    ShippingAddress NVARCHAR(255) NOT NULL,

    Note NVARCHAR(500),


    CONSTRAINT CK_Orders_TotalAmount
        CHECK (TotalAmount >= 0),

    CONSTRAINT CK_Orders_Status
        CHECK
        (
            Status IN
            (
                'Pending',
                'Confirmed',
                'Shipping',
                'Completed',
                'Cancelled'
            )
        ),

    CONSTRAINT FK_Orders_Users
        FOREIGN KEY (UserId)
        REFERENCES Users(UserId)
);
GO


-- =============================================
-- 7. ORDER DETAILS
-- =============================================

CREATE TABLE OrderDetails
(
    OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,

    OrderId INT NOT NULL,

    BookId INT NOT NULL,

    Quantity INT NOT NULL,

    UnitPrice DECIMAL(18,2) NOT NULL,


    CONSTRAINT CK_OrderDetails_Quantity
        CHECK (Quantity > 0),

    CONSTRAINT CK_OrderDetails_UnitPrice
        CHECK (UnitPrice >= 0),

    CONSTRAINT FK_OrderDetails_Orders
        FOREIGN KEY (OrderId)
        REFERENCES Orders(OrderId),

    CONSTRAINT FK_OrderDetails_Books
        FOREIGN KEY (BookId)
        REFERENCES Books(BookId)
);
GO


-- =============================================
-- DỮ LIỆU MẪU
-- =============================================


-- USERS

INSERT INTO Users
(
    FullName,
    Email,
    PasswordHash,
    Phone,
    Address,
    Role
)
VALUES
(
    N'Quản trị viên',
    'admin@gmail.com',
    'TEMP_PASSWORD_HASH',
    '0900000000',
    N'TP. Hồ Chí Minh',
    'Admin'
),
(
    N'Nguyễn Văn A',
    'user@gmail.com',
    'TEMP_PASSWORD_HASH',
    '0911111111',
    N'TP. Hồ Chí Minh',
    'Customer'
);
GO


-- CATEGORIES

INSERT INTO Categories
(
    CategoryName,
    Description
)
VALUES
(
    N'Văn học',
    N'Sách văn học Việt Nam và nước ngoài'
),
(
    N'Kinh tế',
    N'Sách kinh doanh, tài chính và đầu tư'
),
(
    N'Công nghệ',
    N'Sách công nghệ thông tin và lập trình'
),
(
    N'Kỹ năng sống',
    N'Sách phát triển bản thân và kỹ năng sống'
),
(
    N'Tiểu thuyết',
    N'Các loại tiểu thuyết'
);
GO


-- AUTHORS

INSERT INTO Authors
(
    AuthorName,
    Biography
)
VALUES
(
    N'Nguyễn Nhật Ánh',
    N'Tác giả nổi tiếng của Việt Nam.'
),
(
    N'Dale Carnegie',
    N'Tác giả nổi tiếng với các sách về giao tiếp và phát triển bản thân.'
),
(
    N'Robert Kiyosaki',
    N'Tác giả nổi tiếng với các sách về tài chính cá nhân.'
),
(
    N'J.K. Rowling',
    N'Tác giả của bộ truyện Harry Potter.'
);
GO


-- PUBLISHERS

INSERT INTO Publishers
(
    PublisherName,
    Address,
    Phone
)
VALUES
(
    N'Nhà xuất bản Trẻ',
    N'TP. Hồ Chí Minh',
    '0281234567'
),
(
    N'Nhà xuất bản Kim Đồng',
    N'Hà Nội',
    '0241234567'
),
(
    N'Nhà xuất bản Giáo dục Việt Nam',
    N'Hà Nội',
    '0249876543'
);
GO


-- BOOKS

INSERT INTO Books
(
    BookName,
    ISBN,
    Price,
    Quantity,
    Description,
    ImageUrl,
    PublishedYear,
    CategoryId,
    AuthorId,
    PublisherId
)
VALUES
(
    N'Mắt Biếc',
    '9786041234567',
    85000,
    20,
    N'Tiểu thuyết nổi tiếng của Nguyễn Nhật Ánh.',
    NULL,
    2019,
    1,
    1,
    1
),
(
    N'Đắc Nhân Tâm',
    '9786049876543',
    75000,
    30,
    N'Cuốn sách nổi tiếng về nghệ thuật giao tiếp.',
    NULL,
    2020,
    4,
    2,
    1
),
(
    N'Cha Giàu Cha Nghèo',
    '9786045555555',
    120000,
    15,
    N'Sách về tư duy tài chính cá nhân.',
    NULL,
    2021,
    2,
    3,
    1
),
(
    N'Harry Potter và Hòn Đá Phù Thủy',
    '9786046666666',
    150000,
    25,
    N'Phần đầu tiên của series Harry Potter.',
    NULL,
    2018,
    5,
    4,
    2
);
GO


-- =============================================
-- KIỂM TRA
-- =============================================

SELECT * FROM Users;

SELECT * FROM Categories;

SELECT * FROM Authors;

SELECT * FROM Publishers;

SELECT * FROM Books;

SELECT * FROM Orders;

SELECT * FROM OrderDetails;
GO