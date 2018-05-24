CREATE TABLE [dbo].[MantaCheckpoints](
    [ProjectorName] [varchar](128) COLLATE Latin1_General_BIN2 NOT NULL,
    [ProjectionName] [varchar](128) COLLATE Latin1_General_BIN2 NOT NULL,
    [Position] [bigint] NOT NULL DEFAULT(0),
    [LastPositionUpdatedAtUtc] [datetime2](3) NOT NULL DEFAULT(getutcdate()),
    [DroppedAtUtc] [datetime2](3) NULL,
    CONSTRAINT [PK_MantaCheckpoints] PRIMARY KEY CLUSTERED
    (
        [ProjectorName] ASC,
        [ProjectionName] ASC
    )
);

EXEC sys.sp_addextendedproperty
    @name = 'Version', @VALUE = N'1.0.0',
    @level0type = 'SCHEMA', @level0name = 'dbo',
    @level1type = 'Table', @level1name = 'MantaCheckpoints';
GO

CREATE PROCEDURE [dbo].[mantaFindCheckpoints]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        s.ProjectorName,
        s.ProjectionName,
        s.Position,
        s.LastPositionUpdatedAtUtc,
        s.DroppedAtUtc
    FROM
        [dbo].[MantaCheckpoints] s
END;
GO

CREATE PROCEDURE [dbo].[mantaAddCheckpoint]
(
    @ProjectorName VARCHAR(128),
    @ProjectionName VARCHAR(128)
)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [dbo].[MantaCheckpoints](ProjectorName, ProjectionName)
    VALUES(@ProjectorName, @ProjectionName)
END;
GO

CREATE PROCEDURE [dbo].[mantaDeleteCheckpoint]
(
    @ProjectorName VARCHAR(128),
    @ProjectionName VARCHAR(128)
)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM [dbo].[MantaCheckpoints]
    WHERE ProjectorName = @ProjectorName AND ProjectionName = @ProjectionName
END;
GO

CREATE PROCEDURE [dbo].[mantaUpdateCheckpoint]
(
    @ProjectorName VARCHAR(128),
    @ProjectionName VARCHAR(128),
    @Position BIGINT,
    @DroppedAtUtc DATETIME2
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[MantaCheckpoints] SET Position = @Position, DroppedAtUtc = @DroppedAtUtc, LastPositionUpdatedAtUtc = getutcdate()
    WHERE ProjectorName = @ProjectorName AND ProjectionName = @ProjectionName
END;
GO