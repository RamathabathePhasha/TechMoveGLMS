IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Clients] (
    [ClientId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [Phone] nvarchar(max) NOT NULL,
    [Region] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Clients] PRIMARY KEY ([ClientId])
);
GO

CREATE TABLE [Contracts] (
    [ContractId] int NOT NULL IDENTITY,
    [ClientId] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [ServiceLevel] nvarchar(50) NOT NULL,
    [SignedAgreementPath] nvarchar(max) NULL,
    [SignedAgreementFileName] nvarchar(max) NULL,
    CONSTRAINT [PK_Contracts] PRIMARY KEY ([ContractId]),
    CONSTRAINT [FK_Contracts_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]) ON DELETE CASCADE
);
GO

CREATE TABLE [ServiceRequests] (
    [ServiceRequestId] int NOT NULL IDENTITY,
    [ContractId] int NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [AmountUSD] decimal(18,2) NOT NULL,
    [AmountZAR] decimal(18,2) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ExchangeRateUsed] decimal(10,4) NOT NULL,
    CONSTRAINT [PK_ServiceRequests] PRIMARY KEY ([ServiceRequestId]),
    CONSTRAINT [FK_ServiceRequests_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([ContractId]) ON DELETE CASCADE
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ClientId', N'Email', N'Name', N'Phone', N'Region') AND [object_id] = OBJECT_ID(N'[Clients]'))
    SET IDENTITY_INSERT [Clients] ON;
INSERT INTO [Clients] ([ClientId], [Email], [Name], [Phone], [Region])
VALUES (1, N'contact@abclogistics.co.za', N'ABC Logistics', N'0123456789', N'Gauteng'),
(2, N'info@fastship.co.za', N'FastShip SA', N'0112345678', N'Western Cape'),
(3, N'support@cargomasters.co.za', N'Cargo Masters', N'0317654321', N'KwaZulu-Natal');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ClientId', N'Email', N'Name', N'Phone', N'Region') AND [object_id] = OBJECT_ID(N'[Clients]'))
    SET IDENTITY_INSERT [Clients] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ContractId', N'ClientId', N'EndDate', N'ServiceLevel', N'SignedAgreementFileName', N'SignedAgreementPath', N'StartDate', N'Status') AND [object_id] = OBJECT_ID(N'[Contracts]'))
    SET IDENTITY_INSERT [Contracts] ON;
INSERT INTO [Contracts] ([ContractId], [ClientId], [EndDate], [ServiceLevel], [SignedAgreementFileName], [SignedAgreementPath], [StartDate], [Status])
VALUES (1, 1, '2026-01-01T00:00:00.0000000', N'Premium', NULL, NULL, '2025-01-01T00:00:00.0000000', N'Active'),
(2, 2, '2025-12-31T00:00:00.0000000', N'Standard', NULL, NULL, '2025-03-01T00:00:00.0000000', N'Draft'),
(3, 1, '2024-12-31T00:00:00.0000000', N'Basic', NULL, NULL, '2024-01-01T00:00:00.0000000', N'Expired');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ContractId', N'ClientId', N'EndDate', N'ServiceLevel', N'SignedAgreementFileName', N'SignedAgreementPath', N'StartDate', N'Status') AND [object_id] = OBJECT_ID(N'[Contracts]'))
    SET IDENTITY_INSERT [Contracts] OFF;
GO

CREATE INDEX [IX_Contracts_ClientId] ON [Contracts] ([ClientId]);
GO

CREATE INDEX [IX_ServiceRequests_ContractId] ON [ServiceRequests] ([ContractId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260512170353_InitialCreate', N'8.0.0');
GO

COMMIT;
GO

