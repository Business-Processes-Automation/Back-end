/*
  Застосувати схему, якщо міграція ExpandDraftJsonAndSlotInterval пройшла порожньою.
  Безпечно запускати повторно (перевіряє наявність колонок).

  Після цього можна виконати update-masters-slot-interval.sql для різних значень у майстрів.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

USE [BusinessProcessesAutomationDb];
GO

-- 1. DraftJson → nvarchar(max)
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'TelegramUserSessions'
      AND COLUMN_NAME = 'DraftJson' AND CHARACTER_MAXIMUM_LENGTH = 1000)
BEGIN
    ALTER TABLE dbo.TelegramUserSessions ALTER COLUMN DraftJson NVARCHAR(MAX) NULL;
    PRINT 'DraftJson expanded to nvarchar(max).';
END
ELSE
    PRINT 'DraftJson already nvarchar(max) or column missing.';
GO

-- 2. FreeSlotIntervalMinutes
IF COL_LENGTH('dbo.MasterAppointmentSettings', 'FreeSlotIntervalMinutes') IS NULL
BEGIN
    ALTER TABLE dbo.MasterAppointmentSettings
        ADD FreeSlotIntervalMinutes INT NOT NULL
            CONSTRAINT DF_MasterAppointmentSettings_FreeSlotInterval DEFAULT (15);
    PRINT 'Added FreeSlotIntervalMinutes (default 15).';
END
ELSE
    PRINT 'FreeSlotIntervalMinutes already exists.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE name = 'CK_MasterAppointmentSettings_FreeSlotIntervalMinutes')
BEGIN
    ALTER TABLE dbo.MasterAppointmentSettings
        ADD CONSTRAINT CK_MasterAppointmentSettings_FreeSlotIntervalMinutes
        CHECK ([FreeSlotIntervalMinutes] >= 5 AND [FreeSlotIntervalMinutes] <= 120 AND [FreeSlotIntervalMinutes] % 5 = 0);
    PRINT 'Added check constraint.';
END
GO

-- Перевірка
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('TelegramUserSessions', 'MasterAppointmentSettings')
  AND COLUMN_NAME IN ('DraftJson', 'FreeSlotIntervalMinutes')
ORDER BY TABLE_NAME, COLUMN_NAME;

SELECT MigrationId FROM dbo.__EFMigrationsHistory ORDER BY MigrationId;
GO
