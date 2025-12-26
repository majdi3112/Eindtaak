-- =====================================================
-- ClientSimulator Database Creation Script
-- =====================================================

USE master;
GO

-- Create database
CREATE DATABASE ClientSimulatorDB;
GO

USE ClientSimulatorDB;
GO

-- =====================================================
-- 1. LAND TABLE
-- =====================================================
CREATE TABLE Land (
    LandId INT IDENTITY(1,1) PRIMARY KEY,
    Naam NVARCHAR(100) NOT NULL UNIQUE
);
GO

-- =====================================================
-- 2. VOORNAAM TABLE
-- =====================================================
CREATE TABLE Voornaam (
    VoornaamId INT IDENTITY(1,1) PRIMARY KEY,
    Naam NVARCHAR(250) NOT NULL,
    Geslacht CHAR(1) NOT NULL CHECK (Geslacht IN ('M', 'F')),
    Frequentie INT NOT NULL DEFAULT 1,
    LandId INT NOT NULL,

    FOREIGN KEY (LandId) REFERENCES Land(LandId) ON DELETE CASCADE,

    -- Ensure unique combination of name, gender, and country
    CONSTRAINT UK_Voornaam_NaamGeslachtLand UNIQUE (Naam, Geslacht, LandId)
);
GO

-- =====================================================
-- 3. ACHTERNAAM TABLE
-- =====================================================
CREATE TABLE Achternaam (
    AchternaamId INT IDENTITY(1,1) PRIMARY KEY,
    Naam NVARCHAR(250) NOT NULL,
    Frequentie INT NOT NULL DEFAULT 1,
    LandId INT NOT NULL,

    FOREIGN KEY (LandId) REFERENCES Land(LandId) ON DELETE CASCADE,

    -- Ensure unique combination of name and country
    CONSTRAINT UK_Achternaam_NaamLand UNIQUE (Naam, LandId)
);
GO

-- =====================================================
-- 4. GEMEENTE TABLE
-- =====================================================
CREATE TABLE Gemeente (
    GemeenteId INT IDENTITY(1,1) PRIMARY KEY,
    Naam NVARCHAR(250) NOT NULL,
    LandId INT NOT NULL,

    FOREIGN KEY (LandId) REFERENCES Land(LandId) ON DELETE CASCADE,

    -- Ensure unique combination of municipality name and country
    CONSTRAINT UK_Gemeente_NaamLand UNIQUE (Naam, LandId)
);
GO

-- =====================================================
-- 5. STRAAT TABLE
-- =====================================================
CREATE TABLE Straat (
    StraatId INT IDENTITY(1,1) PRIMARY KEY,
    GemeenteId INT NOT NULL,
    Naam NVARCHAR(250) NOT NULL,
    HighwayType NVARCHAR(50) NOT NULL,

    FOREIGN KEY (GemeenteId) REFERENCES Gemeente(GemeenteId) ON DELETE CASCADE,

    -- Ensure unique combination of street name and municipality
    CONSTRAINT UK_Straat_NaamGemeente UNIQUE (Naam, GemeenteId)
);
GO

-- =====================================================
-- 6. PERSOON TABLE (for simulation results)
-- =====================================================
CREATE TABLE Persoon (
    PersoonId INT IDENTITY(1,1) PRIMARY KEY,
    Voornaam NVARCHAR(50) NOT NULL,
    Achternaam NVARCHAR(50) NOT NULL,
    Geslacht CHAR(1) NOT NULL CHECK (Geslacht IN ('M', 'F')),
    Straat NVARCHAR(100) NOT NULL,
    Gemeente NVARCHAR(100) NOT NULL,
    Land NVARCHAR(100) NOT NULL,
    Leeftijd INT NOT NULL,
    Huisnummer NVARCHAR(10) NOT NULL,
    Opdrachtgever NVARCHAR(100) NOT NULL,
    GeboorteDatum DATE NOT NULL,
    HuidigeLeeftijd INT NOT NULL,
    SimulatieDatum DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- =====================================================
-- Note: Countries will be inserted automatically by the upload process
-- =====================================================