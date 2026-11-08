-- Creates the database and its tables. Safe to re-run: tables are dropped and recreated.
IF DB_ID('TaskManagerDb') IS NULL
    CREATE DATABASE TaskManagerDb;
GO

USE TaskManagerDb;
GO

-- Tasks must be dropped first because it references Users.
IF OBJECT_ID('dbo.Tasks', 'U') IS NOT NULL
    DROP TABLE dbo.Tasks;
GO

IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users (
    Id        INT IDENTITY(1,1) NOT NULL,
    Name      NVARCHAR(100)     NOT NULL,
    Email     NVARCHAR(150)     NOT NULL,
    CreatedAt DATETIME2         NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Users PRIMARY KEY (Id),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);
GO

CREATE TABLE dbo.Tasks (
    Id             INT IDENTITY(1,1) NOT NULL,
    Title          NVARCHAR(200)     NOT NULL,
    Description    NVARCHAR(1000)    NULL,
    Status         NVARCHAR(20)      NOT NULL CONSTRAINT DF_Tasks_Status DEFAULT 'Pending',
    UserId         INT               NOT NULL,
    CreatedAt      DATETIME2         NOT NULL CONSTRAINT DF_Tasks_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt      DATETIME2         NOT NULL CONSTRAINT DF_Tasks_UpdatedAt DEFAULT SYSUTCDATETIME(),
    -- Flexible per-task attributes (priority, tags, ...) stored as JSON; ISJSON rejects malformed text.
    AdditionalData NVARCHAR(MAX)     NULL,
    CONSTRAINT PK_Tasks PRIMARY KEY (Id),
    CONSTRAINT CK_Tasks_Status CHECK (Status IN ('Pending', 'InProgress', 'Done')),
    CONSTRAINT CK_Tasks_AdditionalData_IsJson CHECK (AdditionalData IS NULL OR ISJSON(AdditionalData) = 1),
    CONSTRAINT FK_Tasks_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
);
GO

-- Serves the main query: a user's tasks filtered by status, newest first.
CREATE INDEX IX_Tasks_UserId_Status_CreatedAt
    ON dbo.Tasks (UserId, Status, CreatedAt DESC);
GO
