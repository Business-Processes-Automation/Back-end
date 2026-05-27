/*
  Оновлення FreeSlotIntervalMinutes для існуючих майстрів.
  База: BusinessProcessesAutomationDb

  Якщо міграція EF ще не застосована — скрипт додасть колонку сам.

  Тестові майстри (seed):
    anna_beauty  → 5 хв
    olena_nails  → 10 хв
    maria_hair   → 15 хв
    ivan_barber  → 30 хв

  Інші майстри — циклічно 5 / 10 / 15 / 30 за Id (щоб у кожного було своє значення).
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

USE [BusinessProcessesAutomationDb];
GO

IF COL_LENGTH('dbo.MasterAppointmentSettings', 'FreeSlotIntervalMinutes') IS NULL
BEGIN
    ALTER TABLE dbo.MasterAppointmentSettings
        ADD FreeSlotIntervalMinutes INT NOT NULL
            CONSTRAINT DF_MasterAppointmentSettings_FreeSlotInterval DEFAULT (15);

    IF NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE name = 'CK_MasterAppointmentSettings_FreeSlotIntervalMinutes')
    BEGIN
        ALTER TABLE dbo.MasterAppointmentSettings
            ADD CONSTRAINT CK_MasterAppointmentSettings_FreeSlotIntervalMinutes
            CHECK ([FreeSlotIntervalMinutes] >= 5 AND [FreeSlotIntervalMinutes] <= 120 AND [FreeSlotIntervalMinutes] % 5 = 0);
    END
END
GO

BEGIN TRANSACTION;

-- Відомі тестові slug
UPDATE s
SET s.FreeSlotIntervalMinutes = v.IntervalMinutes,
    s.UpdatedAt = GETUTCDATE()
FROM dbo.MasterAppointmentSettings s
INNER JOIN dbo.Masters m ON m.Id = s.MasterId AND m.IsDeleted = 0
INNER JOIN (VALUES
    (N'anna_beauty',  5),
    (N'olena_nails', 10),
    (N'maria_hair',  15),
    (N'ivan_barber', 30)
) AS v(Username, IntervalMinutes) ON m.Username = v.Username
WHERE s.IsDeleted = 0;

-- Усі інші активні майстри — різні кроки за Id
UPDATE s
SET s.FreeSlotIntervalMinutes = CASE (m.Id % 4)
        WHEN 0 THEN 5
        WHEN 1 THEN 10
        WHEN 2 THEN 15
        ELSE 30
    END,
    s.UpdatedAt = GETUTCDATE()
FROM dbo.MasterAppointmentSettings s
INNER JOIN dbo.Masters m ON m.Id = s.MasterId AND m.IsDeleted = 0
WHERE s.IsDeleted = 0
  AND m.Username NOT IN (N'anna_beauty', N'olena_nails', N'maria_hair', N'ivan_barber');

COMMIT TRANSACTION;
GO

SELECT
    m.Id,
    m.Username,
    m.FirstName + N' ' + m.LastName AS FullName,
    s.FreeSlotIntervalMinutes AS SlotIntervalMin,
    s.BufferBetweenClientsMinutes AS BufferMin
FROM dbo.Masters m
INNER JOIN dbo.MasterAppointmentSettings s ON s.MasterId = m.Id AND s.IsDeleted = 0
WHERE m.IsDeleted = 0
ORDER BY m.Id;
GO
