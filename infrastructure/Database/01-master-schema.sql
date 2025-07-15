-- Crée la base maître au cas où MYSQL_DATABASE ne l'aurait pas déjà faite
CREATE DATABASE IF NOT EXISTS TimeStockDB;
USE TimeStockDB;

CREATE TABLE IF NOT EXISTS Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    AccountName      VARCHAR(255) NOT NULL UNIQUE,
    Name             VARCHAR(255) NOT NULL,
    FirstName        VARCHAR(255) NOT NULL,
    Email            VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash     VARCHAR(255) NOT NULL,
    DatabaseName     VARCHAR(255) NOT NULL,
    DatabasePassword VARCHAR(255) NOT NULL      -- NEW (mode B)
);
