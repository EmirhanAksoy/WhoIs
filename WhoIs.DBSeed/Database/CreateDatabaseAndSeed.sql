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
    FacePath VARCHAR(MAX) NOT NULL,
    FaceName VARCHAR(500) NOT NULL DEFAULT(''),
    IsActive BIT DEFAULT(1)
);


-- Create Image Table
CREATE TABLE Images (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UniqueId NVARCHAR(255) UNIQUE NOT NULL,
    ImagePath VARCHAR(MAX) NOT NULL,
    IsProcessed BIT DEFAULT(0),
    IsActive BIT DEFAULT(1)
);


-- Create Image-Face Mapping Table
CREATE TABLE ImageFaceMapping (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FaceId NVARCHAR(255),
    ImageId VARCHAR(MAX) NOT NULL
);