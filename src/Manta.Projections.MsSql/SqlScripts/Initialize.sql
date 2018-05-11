CREATE TABLE [dbo].[StreamsProjectionCheckpoints](
    [ProjectorName] [varchar](128) COLLATE Latin1_General_BIN2 NOT NULL,
    [ProjectionName] [varchar](128) COLLATE Latin1_General_BIN2 NOT NULL,
    [Position] [bigint] NOT NULL DEFAULT(0),
    [DroppedAtUtc] [datetime2](3) NULL,
    CONSTRAINT [PK_StreamsProjectionCheckpoints] PRIMARY KEY CLUSTERED
    (
        [ProjectorName] ASC,
        [ProjectionName] ASC
    )
);
GO

CREATE PROCEDURE [dbo].[mantaFetchAllProjectionCheckpoints]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        s.ProjectorName,
        s.ProjectionName,
        s.Position,
        s.DroppedAtUtc
    FROM
        StreamsProjectionCheckpoints s
END;
GO

CREATE PROCEDURE [dbo].[mantaAddProjectionCheckpoint]
(
    @ProjectorName VARCHAR(128),
    @ProjectionName VARCHAR(128)
)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO StreamsProjectionCheckpoints(ProjectorName, ProjectionName)
    VALUES(@ProjectorName, @ProjectionName)
END;
GO

CREATE PROCEDURE [dbo].[mantaDeleteProjectionCheckpoint]
(
    @ProjectorName VARCHAR(128),
    @ProjectionName VARCHAR(128)
)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM StreamsProjectionCheckpoints
    WHERE ProjectorName = @ProjectorName AND ProjectionName = @ProjectionName
END;
GO

CREATE PROCEDURE [dbo].[mantaUpdateProjectionCheckpoint]
(
    @ProjectorName VARCHAR(128),
    @ProjectionName VARCHAR(128),
    @Position BIGINT,
    @DroppedAtUtc DATETIME2
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE StreamsProjectionCheckpoints SET Position = @Position, DroppedAtUtc = @DroppedAtUtc
    WHERE ProjectorName = @ProjectorName AND ProjectionName = @ProjectionName
END;
GO