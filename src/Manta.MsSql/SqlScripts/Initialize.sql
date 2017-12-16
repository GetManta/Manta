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

-- For idempotency checking per stream
CREATE UNIQUE NONCLUSTERED INDEX [IX_Streams_MessageId_Name] ON [dbo].[Streams]
(
    [MessageId] ASC,
    [Name] ASC
);

CREATE UNIQUE NONCLUSTERED INDEX [IX_Streams_InternalId] ON [dbo].[Streams]
(
    [InternalId] ASC
);

CREATE TABLE [dbo].[StreamsStats](
    [InternalId] [int] NOT NULL,
    [MaxMessagePosition] [bigint] NOT NULL DEFAULT(0),
    [CountOfAllMessages] [bigint] NOT NULL DEFAULT(0),
    CONSTRAINT [PK_StreamsStats] PRIMARY KEY CLUSTERED
    (
        [InternalId] ASC
    )
);

INSERT INTO [dbo].[StreamsStats]([InternalId],[MaxMessagePosition],[CountOfAllMessages])VALUES(1,0,0);
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

    IF EXISTS(SELECT TOP 1 1 FROM [Streams] s WITH(READPAST,ROWLOCK) WHERE s.[Name]=@StreamName AND s.[MessageId]=@MessageId) BEGIN
        RETURN; -- idempotency checking
    END

    INSERT INTO [Streams]([Name],[MessageVersion],[MessageId],[CorrelationId],[ContractId],[Payload])
    SELECT TOP 1
        @StreamName,
        IsNull(MAX(s.[MessageVersion]), 0) + 1,
        @MessageId,
        @CorrelationId,
        @ContractId,
        @Payload
    FROM
        [Streams] s WITH(READPAST,ROWLOCK)
    WHERE
        s.[Name] = @StreamName
END;
GO

CREATE PROCEDURE [dbo].[mantaAppendExpectedVersion]
(
    @StreamName VARCHAR(512),
    @CorrelationId UNIQUEIDENTIFIER,
    @ContractId INT,
    @MessageId UNIQUEIDENTIFIER,
    @MessageVersion INT,
    @Payload VARBINARY(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS(SELECT TOP 1 1 FROM [Streams] s WITH(READPAST,ROWLOCK) WHERE s.[Name]=@StreamName AND s.[MessageId]=@MessageId) BEGIN
        RETURN; -- idempotency checking
    END

    INSERT INTO [Streams]([Name],[MessageVersion],[MessageId],[CorrelationId],[ContractId],[Payload])
    SELECT TOP 1
        s.[Name],
        @MessageVersion,
        @MessageId,
        @CorrelationId,
        @ContractId,
        @Payload
    FROM
        [Streams] s WITH(READPAST,ROWLOCK)
    WHERE
        s.[Name] = @StreamName
        AND s.[MessageVersion] = (@MessageVersion - 1)

    IF @@ROWCOUNT = 0 BEGIN
        RAISERROR('WrongExpectedVersion',16,1);
    END
END;
GO

CREATE PROCEDURE [dbo].[mantaAppendNoStream]
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

    IF EXISTS(SELECT TOP 1 1 FROM [Streams] s WITH(READPAST,ROWLOCK) WHERE s.[Name]=@StreamName AND s.[MessageId]=@MessageId) BEGIN
        RETURN; -- idempotency checking
    END

    INSERT INTO [Streams]([Name],[MessageVersion],[MessageId],[CorrelationId],[ContractId],[Payload])
    VALUES(@StreamName,1,@MessageId,@CorrelationId,@ContractId,@Payload)
END;
GO

CREATE PROCEDURE [dbo].[mantaReadStreamForward]
(
    @StreamName VARCHAR(512),
    @FromVersion INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.[MessageId],
        s.[MessageVersion],
        s.[ContractId],
        s.[Payload]
    FROM
        [Streams] s WITH(READPAST,ROWLOCK)
    WHERE
        s.[Name] = @StreamName
        AND s.[MessageVersion] >= @FromVersion
    ORDER BY
        s.[MessageVersion] ASC
END;
GO

CREATE PROCEDURE [dbo].[mantaReadMessage]
(
    @StreamName VARCHAR(512),
    @MessageVersion INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        s.[MessageId],
        s.[MessageVersion],
        s.[ContractId],
        s.[Payload]
    FROM
        [Streams] s WITH(READPAST,ROWLOCK)
    WHERE
        s.[Name] = @StreamName
        AND s.[MessageVersion] = @MessageVersion
END;
GO

CREATE PROCEDURE [dbo].[mantaLinearizeStreams]
(
    @BatchSize INT
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @MessagePosition BIGINT

    SELECT @MessagePosition = IsNull(MAX(s.[MessagePosition]), 0) FROM [Streams] s WITH (READPAST,ROWLOCK) WHERE s.[MessagePosition] > 0

    UPDATE dest SET
        [MessagePosition] = @MessagePosition,
        @MessagePosition = @MessagePosition + 1
    FROM
        [Streams] dest
        INNER JOIN (SELECT TOP(@BatchSize) s.[InternalId] FROM [Streams] s WITH (READPAST,ROWLOCK) WHERE s.[MessagePosition] IS NULL ORDER BY s.[InternalId] ASC) src ON src.[InternalId] = dest.[InternalId]

    SELECT @MessagePosition = IsNull(MAX(s.[MessagePosition]), 0) FROM [Streams] s WITH (READPAST,ROWLOCK) WHERE s.[MessagePosition] > 0

    -- Update stats
    UPDATE StreamsStats SET
        MaxMessagePosition = @MessagePosition,
        CountOfAllMessages = (SELECT SUM(st.row_count) FROM sys.dm_db_partition_stats st WHERE object_name(object_id) = 'Streams' AND (index_id < 2))

    SELECT TOP 1 CAST(1 AS BIT) FROM [Streams] s WITH (READPAST,ROWLOCK) WHERE s.[MessagePosition] IS NULL ORDER BY s.[InternalId] ASC
END;
GO

CREATE PROCEDURE [dbo].[mantaReadHeadMessagePosition]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP(1) [MaxMessagePosition] FROM [StreamsStats] WITH (NOLOCK) WHERE [InternalId] = 1
END;
GO

CREATE PROCEDURE [dbo].[mantaHardDeleteStream]
(
    @StreamName VARCHAR(512),
    @ExpectedVersion INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [Streams] WHERE [Name] = @StreamName AND
        (SELECT TOP(1) [MessageVersion] FROM [Streams] WHERE [Name] = @StreamName ORDER BY [MessageVersion] DESC) = @ExpectedVersion

    IF @@ROWCOUNT = 0 BEGIN
        RAISERROR('WrongExpectedVersion',16,1);
    END
END;
GO