CREATE TABLE [dbo].[Streams](
    [InternalId] [bigint] IDENTITY(1,1) NOT NULL,
    [Name] [varchar](512) COLLATE Latin1_General_BIN2 NOT NULL,
    [MessageVersion] [int] NOT NULL,
    [MessageId] [uniqueidentifier] NOT NULL,
    [CorrelationId] [uniqueidentifier] NOT NULL,
    [ContractId] [int] NOT NULL,
    [Timestamp] [datetime2](3) NOT NULL DEFAULT(getutcdate()),
    [Payload] [varbinary](MAX) NOT NULL,
    [MessagePosition] [bigint] NULL,
    CONSTRAINT [PK_Streams] PRIMARY KEY CLUSTERED
    (
        [Name] ASC,
        [MessageVersion] ASC
    )
);

CREATE NONCLUSTERED INDEX [IX_Streams_MessagePosition_InternalId] ON [dbo].[Streams]
(
    [MessagePosition] ASC,
    [InternalId] ASC
)
WHERE ([MessagePosition] IS NOT NULL);

CREATE NONCLUSTERED INDEX [IX_Streams_ContractId] ON [dbo].[Streams]
(
    [ContractId] ASC
)
WHERE ([MessagePosition] IS NOT NULL);

CREATE UNIQUE NONCLUSTERED INDEX [IX_Streams_InternalId] ON [dbo].[Streams]
(
    [InternalId] ASC
);
GO

CREATE PROCEDURE [dbo].[mantaAppendAnyVersion]
(
    @StreamName VARCHAR(512),
    @CorrelationId UNIQUEIDENTIFIER,
    @ContractId INT,
    @MessageId UNIQUEIDENTIFIER,
    @Payload VARBINARY(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [Streams]([Name],[MessageVersion],[MessageId],[CorrelationId],[ContractId],[Payload])
    SELECT TOP 1
        @StreamName,
        IsNull(MAX([MessageVersion]), 0) + 1,
        @MessageId,
        @CorrelationId,
        @ContractId,
        @Payload
    FROM
        [Streams]
    WHERE
        [Name] = @StreamName
END;
GO