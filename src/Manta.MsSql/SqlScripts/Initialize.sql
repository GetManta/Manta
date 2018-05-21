CREATE TABLE [dbo].[Streams](
    [InternalId] [bigint] IDENTITY(1,1) NOT NULL,
    [Name] [varchar](255) COLLATE Latin1_General_BIN2 NOT NULL,
    [MessageVersion] [int] NOT NULL,
    [MessageId] [uniqueidentifier] NOT NULL,
    [CorrelationId] [uniqueidentifier] NOT NULL,
    [ContractName] [varchar](128)  COLLATE Latin1_General_BIN2 NOT NULL,
    [Timestamp] [datetime2](3) NOT NULL DEFAULT(getutcdate()),
    [Payload] [varbinary](MAX) NOT NULL,
    [MetadataPayload] [varbinary](MAX) NULL,
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
);

-- For idempotency checking
CREATE UNIQUE NONCLUSTERED INDEX [IX_Streams_MessageId] ON [dbo].[Streams]
(
    [MessageId] ASC
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
    @StreamName VARCHAR(255),
    @CorrelationId UNIQUEIDENTIFIER,
    @ContractName VARCHAR(128),
    @MessageId UNIQUEIDENTIFIER,
    @Payload VARBINARY(MAX),
    @MetadataPayload VARBINARY(MAX) NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [Streams]([Name],[MessageVersion],[MessageId],[CorrelationId],[ContractName],[Payload],[MetadataPayload])
    SELECT TOP 1
        @StreamName,
        IsNull(MAX(s.[MessageVersion]), 0) + 1,
        @MessageId,
        @CorrelationId,
        @ContractName,
        @Payload,
        @MetadataPayload
    FROM
        [Streams] s WITH(READPAST,ROWLOCK)
    WHERE
        s.[Name] = @StreamName
END;
GO

CREATE PROCEDURE [dbo].[mantaAppendExpectedVersion]
(
    @StreamName VARCHAR(255),
    @CorrelationId UNIQUEIDENTIFIER,
    @ContractName VARCHAR(128),
    @MessageId UNIQUEIDENTIFIER,
    @MessageVersion INT,
    @Payload VARBINARY(MAX),
    @MetadataPayload VARBINARY(MAX) NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS(SELECT TOP 1 1 FROM [Streams] s WITH(READPAST,ROWLOCK) WHERE s.[MessageId]=@MessageId) BEGIN
        RETURN; -- idempotency checking
    END

    INSERT INTO [Streams]([Name],[MessageVersion],[MessageId],[CorrelationId],[ContractName],[Payload],[MetadataPayload])
    SELECT TOP 1
        s.[Name],
        @MessageVersion,
        @MessageId,
        @CorrelationId,
        @ContractName,
        @Payload,
        @MetadataPayload
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
    @StreamName VARCHAR(255),
    @CorrelationId UNIQUEIDENTIFIER,
    @ContractName VARCHAR(128),
    @MessageId UNIQUEIDENTIFIER,
    @Payload VARBINARY(MAX),
    @MetadataPayload VARBINARY(MAX) NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [Streams]([Name],[MessageVersion],[MessageId],[CorrelationId],[ContractName],[Payload],[MetadataPayload])
    VALUES(@StreamName,1,@MessageId,@CorrelationId,@ContractName,@Payload,@MetadataPayload)
END;
GO

CREATE PROCEDURE [dbo].[mantaReadStreamForward]
(
    @StreamName VARCHAR(255),
    @FromVersion INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.[MessageId],
        s.[MessageVersion],
        s.[ContractName],
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
    @StreamName VARCHAR(255),
    @MessageVersion INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        s.[MessageId],
        s.[MessageVersion],
        s.[ContractName],
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
        @MessagePosition = [MessagePosition] = @MessagePosition + 1
    FROM
        [Streams] dest WITH (INDEX ([IX_Streams_InternalId]))
        INNER JOIN (SELECT TOP(@BatchSize) s.[InternalId] FROM [Streams] s WITH (READPAST,ROWLOCK) WHERE s.[MessagePosition] IS NULL ORDER BY s.[InternalId] ASC) src ON src.[InternalId] = dest.[InternalId]
    OPTION (MAXDOP 1)

    -- Update stats
    UPDATE StreamsStats SET
        MaxMessagePosition = @MessagePosition,
        CountOfAllMessages = (SELECT SUM(st.row_count) FROM sys.dm_db_partition_stats st WHERE OBJECT_NAME(object_id) = 'Streams' AND (index_id < 2))

    SELECT TOP 1 CAST(1 AS BIT) FROM [Streams] s WITH (READPAST,ROWLOCK) WHERE s.[MessagePosition] IS NULL
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
    @StreamName VARCHAR(255),
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

CREATE PROCEDURE [dbo].[mantaTruncateStreamToVersion]
(
    @StreamName VARCHAR(255),
    @ExpectedVersion INT,
    @ToVersion INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE s FROM [Streams] s WHERE s.[Name] = @StreamName AND s.[MessageVersion] <= @ToVersion AND
        (SELECT TOP(1) [MessageVersion] FROM [Streams] WHERE [Name] = s.[Name] ORDER BY [MessageVersion] DESC) = @ExpectedVersion

    IF @@ROWCOUNT = 0 BEGIN
        RAISERROR('WrongExpectedVersion',16,1);
    END
END;
GO

CREATE PROCEDURE [dbo].[mantaTruncateStreamToCreationDate]
(
    @StreamName VARCHAR(255),
    @ExpectedVersion INT,
    @ToCreationDate datetime2(3)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF(SELECT COUNT(1) FROM [Streams] s WHERE s.[Name] = @StreamName AND s.[Timestamp] <= @ToCreationDate) = 0
        RETURN;

    DELETE s FROM [Streams] s WHERE s.[Name] = @StreamName AND s.[Timestamp] <= @ToCreationDate AND
        (SELECT TOP(1) [MessageVersion] FROM [Streams] WHERE [Name] = s.[Name] ORDER BY [MessageVersion] DESC) = @ExpectedVersion

    IF @@ROWCOUNT = 0 BEGIN
        RAISERROR('WrongExpectedVersion',16,1);
    END
END;
GO

CREATE PROCEDURE [dbo].[mantaReadAllStreamsForward]
(
    @Limit INT,
    @FromPosition BIGINT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP(@Limit)
        s.[Name] AS [StreamName],
        s.[ContractName],
        s.[CorrelationId],
        s.[Timestamp],
        s.[MessageId],
        s.[MessageVersion],
        s.[MessagePosition],
        s.[Payload] AS [MessagePayload],
        s.[MetadataPayload]
    FROM
        [Streams] s WITH(READPAST,ROWLOCK)
    WHERE
        s.[MessagePosition] > @FromPosition
    ORDER BY
        s.[MessagePosition] ASC
END;
GO