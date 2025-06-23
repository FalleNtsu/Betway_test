--Tables
CREATE TABLE dbo.access
(
    APIKey uniqueidentifier NOT NULL PRIMARY KEY,
    Password varchar(30) NOT NULL
);

INSERT INTO dbo.access (APIKey, Password)
VALUES ('586214D2-4CBB-4914-99FC-7B27EDFBC8FA', 'OT_Assessment_Password');

CREATE TABLE users (
                accountId UNIQUEIDENTIFIER PRIMARY KEY,
                username NVARCHAR(100) NOT NULL,
                created_date DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                modified_date DATETIME2 NULL,
                modified_by_id UNIQUEIDENTIFIER NULL,
                deleted BIT NOT NULL DEFAULT 0
            )


CREATE TABLE bets (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    wagerId UNIQUEIDENTIFIER NOT NULL,
    game NVARCHAR(100) NOT NULL,
    provider NVARCHAR(100) NOT NULL,
    amount DECIMAL(18, 2) NOT NULL,
    createdDate DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    transactionId UNIQUEIDENTIFIER NOT NULL,
    accountId UNIQUEIDENTIFIER NOT NULL,

    CONSTRAINT FK_Wagers_Users FOREIGN KEY (accountId)
        REFERENCES users(accountId)
        ON DELETE CASCADE
);

CREATE INDEX IX_Wagers_AccountId ON bets(accountId);
CREATE INDEX IX_Wagers_WagerId ON bets(wagerId);

CREATE TABLE userStats (
    accountId UNIQUEIDENTIFIER PRIMARY KEY,
    totalAmountSpend DECIMAL(18, 2) NOT NULL DEFAULT 0,
    wagerCount INT NOT NULL DEFAULT 0,
    modified DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),

    CONSTRAINT FK_UserStats_Users FOREIGN KEY (accountId)
        REFERENCES users(accountId)
        ON DELETE CASCADE
);

CREATE INDEX IX_UserStats_TotalAmountSpend ON userStats(totalAmountSpend);

CREATE INDEX IX_UserStats_WagerCount ON userStats(wagerCount);


--Procedures

CREATE PROCEDURE [dbo].[ValidateAccess]
    @APIKey uniqueidentifier,
    @Password varchar(30)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM dbo.access
        WHERE APIKey = @APIKey
          AND RTRIM(LTRIM(Password)) = RTRIM(LTRIM(@Password))
    )
        SELECT 1 AS IsValid;
    ELSE
        SELECT 0 AS IsValid;
END

CREATE PROCEDURE CreateUser
    @accountId UNIQUEIDENTIFIER = NULL,
    @username NVARCHAR(100),
    @modified_date DATETIME2 = NULL,
    @deleted BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM users WHERE accountId = @accountId
    )
    BEGIN
        INSERT INTO users (
            accountId,
            username,
            created_date,
            deleted
        )
        VALUES (
            @accountId,
            @username,
            SYSUTCDATETIME(),
            0
        );
    END
END;


CREATE PROCEDURE CreateWager
    @wagerId UNIQUEIDENTIFIER = NULL,
    @game NVARCHAR(100),
    @provider NVARCHAR(100),
    @amount DECIMAL(18, 2),
    @createdDate DATETIMEOFFSET(7) = NULL,
    @transactionId UNIQUEIDENTIFIER,
    @accountId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    IF @wagerId IS NULL
        SET @wagerId = NEWID();

    IF @createdDate IS NULL
        SET @createdDate = SYSDATETIMEOFFSET();

    INSERT INTO bets (
        wagerId,
        game,
        provider,
        amount,
        createdDate,
        transactionId,
        accountId
    )
    VALUES (
        @wagerId,
        @game,
        @provider,
        @amount,
        @createdDate,
        @transactionId,
        @accountId
    );

END;

CREATE PROCEDURE [dbo].[CreateOrUpdateUserStat]
    @accountId UNIQUEIDENTIFIER,
    @amountToAdd DECIMAL(18, 2)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @currentDateTime DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

    MERGE userStats AS target
    USING (SELECT @accountId AS accountId) AS source
    ON target.accountId = source.accountId

    WHEN MATCHED THEN
        UPDATE SET
            totalAmountSpend = totalAmountSpend + @amountToAdd,
            wagerCount = wagerCount + 1,
            modified = @currentDateTime

    WHEN NOT MATCHED THEN
        INSERT (accountId, totalAmountSpend, wagerCount, modified)
        VALUES (@accountId, @amountToAdd, 1, @currentDateTime);

END;

CREATE PROCEDURE GetCasinoWagersForPlayer
    @AccountId UNIQUEIDENTIFIER,
    @Offset INT,
    @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Get total count
    SELECT COUNT(*) AS TotalCount
    FROM bets
    WHERE accountId = @AccountId;

    -- Get paginated records
    SELECT wagerId, game, provider, amount, createdDate
    FROM bets
    WHERE accountId = @AccountId
    ORDER BY createdDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END

CREATE PROCEDURE GetTopSpenders
    @Count INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Count)
        u.accountId,
        u.username,
        us.totalAmountSpend
    FROM userStats us
    INNER JOIN users u ON u.accountId = us.accountId
    WHERE u.deleted = 0
    ORDER BY us.totalAmountSpend DESC;
END