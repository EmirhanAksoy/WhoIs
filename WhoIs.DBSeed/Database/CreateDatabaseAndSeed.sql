USE master;
GO

-- Check if the database exists, drop if exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'WhoIs')
    DROP DATABASE WhoIs;
GO

-- Create the database
CREATE DATABASE WhoIs;
GO

-- Switch to the new database
USE WhoIs;
GO

-- Create Faces table
CREATE TABLE Faces (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UniqueId NVARCHAR(255) UNIQUE NOT NULL,
    Encoding VARCHAR(MAX) NOT NULL
);