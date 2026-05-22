IF DB_ID('AutomationTest') IS NULL
BEGIN
    CREATE DATABASE AutomationTest;
END
GO

USE AutomationTest;
GO

IF OBJECT_ID('dbo.ExecucaoAutomacao', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExecucaoAutomacao
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        AcaoExecutada NVARCHAR(200) NOT NULL,
        ValorCapturado NVARCHAR(MAX) NULL,
        Sucesso BIT NOT NULL,
        DataExecucao DATETIME2 NOT NULL
    );
END
GO
