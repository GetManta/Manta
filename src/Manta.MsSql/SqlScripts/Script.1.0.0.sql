CREATE TABLE [dbo].[MantaStreams](
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
    CONSTRAINT [PK_MantaStreams] PRIMARY KEY CLUSTERED
    (
        [Name] ASC,
        [MessageVersion] ASC
    )
);

EXEC sys.sp_addextendedproperty
    @name = 'Version', @VALUE = N'1.0.0',
    @level0type = 'SCHEMA', @level0name = 'dbo',
    @level1type = 'Table', @level1name = 'MantaStreams';

CREATE NONCLUSTERED INDEX [IX_MantaStreams_MessagePosition_InternalId] ON [dbo].[MantaStreams]
(
    [MessagePosition] ASC,
    [InternalId] ASC
);

-- For idempotency checking
CREATE UNIQUE NONCLUSTERED INDEX [IX_MantaStreams_MessageId] ON [dbo].[MantaStreams]
(
    [MessageId] ASC
);

CREATE UNIQUE NONCLUSTERED INDEX [IX_MantaStreams_InternalId] ON [dbo].[MantaStreams]
(
    [InternalId] ASC
);

CREATE TABLE [dbo].[MantaStreamsStats](
    [InternalId] [int] NOT NULL,
    [MaxMessagePosition] [bigint] NOT NULL DEFAULT(0),
    [CountOfAllMessages] [bigint] NOT NULL DEFAULT(0),
    CONSTRAINT [PK_MantaStreamsStats] PRIMARY KEY CLUSTERED
    (
        [InternalId] ASC
    )
);

INSERT INTO [dbo].[MantaStreamsStats]([InternalId],[MaxMessagePosition],[CountOfAllMessages])VALUES(1,0,0);
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

    INSERT INTO [dbo].[MantaStreams]([Name],[MessageVersion],[MessageId],[CorrelationId],[ContractName],[Payload],[MetadataPayload])
    SELECT TOP 1
        @StreamName,
        IsNull(MAX(s.[MessageVersion]), 0) + 1,
        @MessageId,
        @CorrelationId,
        @ContractName,
        @Payload,
        @MetadataPayload
    FROM
        [dbo].[MantaStreams] s WITH(ROWLOCK)
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

    IF EXISTS(SELECT TOP 1 1 FROM [dbo].[MantaStreams] s WITH(ROWLOCK) WHERE s.[MessageId]=@MessageId) BEGIN
        RETURN; -- idempotency checking
    END

    INSERT INTO [dbo].[MantaStreams]([Name],[MessageVersion],[MessageId],[CorrelationId],[ContractName],[Payload],[MetadataPayload])
    SELECT TOP 1
        s.[Name],
        @MessageVersion,
        @MessageId,
        @CorrelationId,
        @ContractName,
        @Payload,
        @MetadataPayload
    FROM
        [dbo].[MantaStreams] s WITH(ROWLOCK)
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
    INSERT INTO [dbo].[MantaStreams]([Name],[MessageVersion],[MessageId],[CorrelationId],[ContractName],[Payload],[MetadataPayload])
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
        [dbo].[MantaStreams] s
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
        [dbo].[MantaStreams] s WITH(ROWLOCK)
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

    SELECT @MessagePosition = IsNull(MAX(s.[MessagePosition]), 0) FROM [dbo].[MantaStreams] s WITH (READPAST,ROWLOCK) WHERE s.[MessagePosition] > 0

    UPDATE dest SET
        @MessagePosition = [MessagePosition] = @MessagePosition + 1
    FROM
        [dbo].[MantaStreams] dest WITH (INDEX ([IX_MantaStreams_InternalId]))
        INNER JOIN (
            SELECT TOP(@BatchSize)
                s.[InternalId]
            FROM
                [dbo].[MantaStreams] s WITH (NOLOCK) -- we can use NOLOCK because only one linearizer can update [MessagePosition]
            WHERE
                s.[MessagePosition] IS NULL
            ORDER BY
                s.[InternalId] ASC) src ON src.[InternalId] = dest.[InternalId]
    OPTION (MAXDOP 1) -- do not allow to parallerize, we need linear execution

    -- Update stats
    UPDATE [dbo].[MantaStreamsStats] SET
        MaxMessagePosition = @MessagePosition,
        CountOfAllMessages = (SELECT SUM(st.row_count) FROM sys.dm_db_partition_stats st WHERE OBJECT_NAME(object_id) = 'MantaStreams' AND (index_id < 2))

    SELECT TOP 1 CAST(1 AS BIT) FROM [dbo].[MantaStreams] s WITH (READPAST,ROWLOCK) WHERE s.[MessagePosition] IS NULL
END;
GO

CREATE PROCEDURE [dbo].[mantaReadHeadMessagePosition]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP(1) [MaxMessagePosition] FROM [dbo].[MantaStreamsStats] WITH (NOLOCK) WHERE [InternalId] = 1
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

    DELETE FROM
        [dbo].[MantaStreams]
    WHERE
        [Name] = @StreamName AND (
            SELECT TOP(1)
                [MessageVersion]
            FROM
                [dbo].[MantaStreams]
            WHERE
                [Name] = @StreamName
            ORDER BY
                [MessageVersion] DESC) = @ExpectedVersion

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

    DELETE s FROM
        [dbo].[MantaStreams] s
    WHERE
        s.[Name] = @StreamName AND
        s.[MessageVersion] <= @ToVersion AND (
            SELECT TOP(1)
                [MessageVersion]
            FROM
                [dbo].[MantaStreams]
            WHERE
                [Name] = s.[Name]
            ORDER BY [MessageVersion] DESC) = @ExpectedVersion

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

    IF(SELECT COUNT(1) FROM [dbo].[MantaStreams] s WHERE s.[Name] = @StreamName AND s.[Timestamp] <= @ToCreationDate) = 0
        RETURN;

    DELETE s FROM
        [dbo].[MantaStreams] s
    WHERE
        s.[Name] = @StreamName AND
        s.[Timestamp] <= @ToCreationDate AND (
            SELECT TOP(1)
                [MessageVersion]
            FROM
                [dbo].[MantaStreams]
            WHERE
                [Name] = s.[Name]
            ORDER BY [MessageVersion] DESC) = @ExpectedVersion

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
        [dbo].[MantaStreams] s WITH(READPAST,ROWLOCK)
    WHERE
        s.[MessagePosition] > @FromPosition
    ORDER BY
        s.[MessagePosition] ASC
END;
GO