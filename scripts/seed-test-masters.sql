/*
  Тестові майстри для BusinessProcessesAutomationDb
  База (за замовчуванням): BusinessProcessesAutomationDb на localhost

  Що створює:
    - 4 майстри (профілі, налаштування запису, робочі години, послуги)
    - MasterTelegrams зі slug для клієнтських посилань ?start=<slug>

  ПЕРЕД ЗАПУСКОМ (обовʼязково для панелі майстра в Telegram):
    1. Дізнайтесь свій Telegram User ID (@userinfobot або лог бота).
    2. Замініть @YourTelegramUserId нижче на своє число.
    3. Розкоментуйте блок UPDATE в кінці файлу для майстра, під яким хочете входити.

  Клієнтські посилання (підставте @YourBotUsername):
    https://t.me/YourBotUsername?start=anna_beauty
    https://t.me/YourBotUsername?start=olena_nails
    https://t.me/YourBotUsername?start=maria_hair
    https://t.me/YourBotUsername?start=ivan_barber

  Скрипт ідемпотентний: повторний запуск не дублює slug, якщо записи вже є.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

USE [BusinessProcessesAutomationDb];
GO

DECLARE @YourTelegramUserId BIGINT = NULL;  -- напр. 123456789 — для UPDATE в кінці

BEGIN TRANSACTION;

-- ---------------------------------------------------------------------------
-- 1. Майстри (тільки якщо ще немає такого Username / slug)
-- ---------------------------------------------------------------------------
DECLARE @Masters TABLE (
    Slug           NVARCHAR(50)  NOT NULL PRIMARY KEY,
    FirstName      NVARCHAR(100) NOT NULL,
    LastName       NVARCHAR(100) NOT NULL,
    Email          NVARCHAR(256) NOT NULL,
    Phone          NVARCHAR(20)  NOT NULL,
    TimeZone       NVARCHAR(64)  NOT NULL,
    TelegramUserId BIGINT        NOT NULL,
    TelegramUser   NVARCHAR(64)  NOT NULL
);

INSERT INTO @Masters (Slug, FirstName, LastName, Email, Phone, TimeZone, TelegramUserId, TelegramUser)
VALUES
    (N'anna_beauty',  N'Анна',   N'Коваленко', N'anna.beauty@test.local',   N'+380501111101', N'Europe/Kyiv', 9100000001, N'anna_beauty_dev'),
    (N'olena_nails',  N'Олена',  N'Шевченко',  N'olena.nails@test.local',   N'+380501111102', N'Europe/Kyiv', 9100000002, N'olena_nails_dev'),
    (N'maria_hair',   N'Марія',  N'Бондар',    N'maria.hair@test.local',    N'+380501111103', N'Europe/Kyiv', 9100000003, N'maria_hair_dev'),
    (N'ivan_barber',  N'Іван',   N'Мельник',   N'ivan.barber@test.local',   N'+380501111104', N'Europe/Kyiv', 9100000004, N'ivan_barber_dev');

INSERT INTO dbo.Masters (FirstName, LastName, Username, Email, PhoneNumber, TimeZone, IsActive, IsDeleted)
SELECT m.FirstName, m.LastName, m.Slug, m.Email, m.Phone, m.TimeZone, 1, 0
FROM @Masters m
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Masters x
    WHERE x.Username = m.Slug AND x.IsDeleted = 0
);

-- ---------------------------------------------------------------------------
-- 2. Налаштування запису
-- ---------------------------------------------------------------------------
INSERT INTO dbo.MasterAppointmentSettings (
    MasterId,
    MinBookingNoticeMinutes,
    MaxBookingDaysAhead,
    CancellationPolicyHours,
    BufferBetweenClientsMinutes,
    FreeSlotIntervalMinutes,
    MaxRescheduleCount,
    IsDeleted)
SELECT
    mas.Id,
    60,   -- мін. за notice до запису
    30,   -- макс. днів наперед
    24,
    10,   -- буфер між клієнтами
    v.IntervalMinutes,
    1,
    0
FROM dbo.Masters mas
INNER JOIN @Masters m ON mas.Username = m.Slug AND mas.IsDeleted = 0
INNER JOIN (VALUES
    (N'anna_beauty',  5),
    (N'olena_nails', 10),
    (N'maria_hair',  15),
    (N'ivan_barber', 30)
) AS v(Username, IntervalMinutes) ON mas.Username = v.Username
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.MasterAppointmentSettings s
    WHERE s.MasterId = mas.Id AND s.IsDeleted = 0
);

-- ---------------------------------------------------------------------------
-- 3. Telegram (slug для ?start=)
-- ---------------------------------------------------------------------------
INSERT INTO dbo.MasterTelegrams (MasterId, TelegramUserId, TelegramUsername, BotStartParameter, IsDeleted)
SELECT mas.Id, m.TelegramUserId, m.TelegramUser, m.Slug, 0
FROM dbo.Masters mas
INNER JOIN @Masters m ON mas.Username = m.Slug AND mas.IsDeleted = 0
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.MasterTelegrams t
    WHERE t.BotStartParameter = m.Slug AND t.IsDeleted = 0
);

-- ---------------------------------------------------------------------------
-- 4. Робочі години: Пн–Пт 10:00–19:00, Сб 10:00–15:00 (Нд — вихідний)
-- ---------------------------------------------------------------------------
DECLARE @Days TABLE (DayName NVARCHAR(16) NOT NULL, WorkStart TIME NOT NULL, WorkEnd TIME NOT NULL);

INSERT INTO @Days VALUES
    (N'Monday',    '10:00', '19:00'),
    (N'Tuesday',   '10:00', '19:00'),
    (N'Wednesday', '10:00', '19:00'),
    (N'Thursday',  '10:00', '19:00'),
    (N'Friday',    '10:00', '19:00'),
    (N'Saturday',  '10:00', '15:00');

INSERT INTO dbo.WorkingHoursPerDays (MasterId, DayOfWeek, WorkStartTime, WorkEndTime, IsDeleted)
SELECT mas.Id, d.DayName, d.WorkStart, d.WorkEnd, 0
FROM dbo.Masters mas
INNER JOIN @Masters m ON mas.Username = m.Slug AND mas.IsDeleted = 0
CROSS JOIN @Days d
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.WorkingHoursPerDays w
    WHERE w.MasterId = mas.Id AND w.DayOfWeek = d.DayName AND w.IsDeleted = 0
);

-- ---------------------------------------------------------------------------
-- 5. Послуги (2 на майстра; Prepayment <= Price)
-- ---------------------------------------------------------------------------
DECLARE @Services TABLE (
    Slug        NVARCHAR(50)  NOT NULL,
    ServiceName NVARCHAR(200) NOT NULL,
    Duration    INT           NOT NULL,
    Price       DECIMAL(18,2) NOT NULL,
    Prepayment  DECIMAL(18,2) NOT NULL,
    SortOrder   INT           NOT NULL
);

INSERT INTO @Services VALUES
    (N'anna_beauty', N'Манікюр класичний', 60,  450.00, 100.00, 1),
    (N'anna_beauty', N'Педикюр',           75,  550.00, 150.00, 2),
    (N'olena_nails', N'Нарощування',      120, 1200.00, 300.00, 1),
    (N'olena_nails', N'Корекція',          90,  800.00, 200.00, 2),
    (N'maria_hair',  N'Стрижка жіноча',    60,  600.00, 0.00,   1),
    (N'maria_hair',  N'Фарбування',       150, 1800.00, 400.00, 2),
    (N'ivan_barber', N'Стрижка чоловіча', 45,  350.00, 0.00,   1),
    (N'ivan_barber', N'Борода + стрижка',  60,  500.00, 100.00, 2);

INSERT INTO dbo.Services (
    MasterId, ServiceName, DurationInMinutes, Price, Prepayment,
    PreparationBeforeInMinutes, PreparationAfterInMinutes, IsDeleted)
SELECT mas.Id, s.ServiceName, s.Duration, s.Price, s.Prepayment, 0, 0, 0
FROM @Services s
INNER JOIN dbo.Masters mas ON mas.Username = s.Slug AND mas.IsDeleted = 0
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Services x
    WHERE x.MasterId = mas.Id AND x.ServiceName = s.ServiceName AND x.IsDeleted = 0
);

COMMIT TRANSACTION;
GO

-- ---------------------------------------------------------------------------
-- 6. Привʼязка ВАШОГО Telegram до одного з майстрів (панель /register не потрібен)
--    Розкоментуйте один рядок і підставте свій Telegram User ID.
-- ---------------------------------------------------------------------------
/*
DECLARE @YourTelegramUserId BIGINT = 123456789;

UPDATE dbo.MasterTelegrams
SET TelegramUserId = @YourTelegramUserId,
    TelegramUsername = N'your_tg_username',
    UpdatedAt = GETUTCDATE()
WHERE BotStartParameter = N'anna_beauty' AND IsDeleted = 0;
*/

-- ---------------------------------------------------------------------------
-- Перевірка
-- ---------------------------------------------------------------------------
SELECT
    m.Id,
    m.FirstName + N' ' + m.LastName AS FullName,
    m.Username,
    m.TimeZone,
    t.BotStartParameter AS ClientStartSlug,
    t.TelegramUserId,
    N'https://t.me/<BOT>?start=' + t.BotStartParameter AS ClientLinkTemplate
FROM dbo.Masters m
INNER JOIN dbo.MasterTelegrams t ON t.MasterId = m.Id AND t.IsDeleted = 0
WHERE m.IsDeleted = 0
  AND m.Username IN (N'anna_beauty', N'olena_nails', N'maria_hair', N'ivan_barber')
ORDER BY m.Id;

SELECT m.Username, s.ServiceName, s.DurationInMinutes, s.Price, s.Prepayment
FROM dbo.Services s
INNER JOIN dbo.Masters m ON m.Id = s.MasterId
WHERE m.IsDeleted = 0 AND s.IsDeleted = 0
  AND m.Username IN (N'anna_beauty', N'olena_nails', N'maria_hair', N'ivan_barber')
ORDER BY m.Username, s.ServiceName;

SELECT m.Username, w.DayOfWeek, w.WorkStartTime, w.WorkEndTime
FROM dbo.WorkingHoursPerDays w
INNER JOIN dbo.Masters m ON m.Id = w.MasterId
WHERE m.IsDeleted = 0 AND w.IsDeleted = 0
  AND m.Username IN (N'anna_beauty', N'olena_nails', N'maria_hair', N'ivan_barber')
ORDER BY m.Username, w.DayOfWeek;
GO

/*
  Видалити тестових майстрів (обережно — каскадно зникнуть послуги, години, записи):

  BEGIN TRANSACTION;
  DELETE FROM dbo.MasterTelegrams WHERE BotStartParameter IN (N'anna_beauty', N'olena_nails', N'maria_hair', N'ivan_barber');
  DELETE FROM dbo.Masters WHERE Username IN (N'anna_beauty', N'olena_nails', N'maria_hair', N'ivan_barber');
  COMMIT TRANSACTION;
*/
