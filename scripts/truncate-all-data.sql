/*
  Повне очищення ДАНИХ у BusinessProcessesAutomationDb.
  Схема таблиць і міграції (__EFMigrationsHistory) залишаються.

  УВАГА: безповоротно видаляє всіх майстрів, клієнтів, записи, сесії Telegram тощо.

  Запуск: SSMS / Azure Data Studio → виконати весь файл.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

USE [BusinessProcessesAutomationDb];
GO

BEGIN TRANSACTION;

-- Залежні від записів / послуг / клієнтів
DELETE FROM dbo.Payments;
DELETE FROM dbo.Appointments;

-- Контент і публікації
DELETE FROM dbo.AIContentGenerations;
DELETE FROM dbo.PostPublications;
DELETE FROM dbo.Posts;

-- Сповіщення
DELETE FROM dbo.ClientNotificationPreferences;
DELETE FROM dbo.MasterNotificationPreferences;

-- Інше, привʼязане до майстрів
DELETE FROM dbo.SocialAccounts;
DELETE FROM dbo.Expenses;
DELETE FROM dbo.TelegramUserSessions;

-- Послуги (FK на Masters — Restrict)
DELETE FROM dbo.Services;

-- Майстри (каскадно прибере години, відпустки, налаштування, MasterTelegrams, пости…)
DELETE FROM dbo.Masters;

-- Клієнти
DELETE FROM dbo.Clients;

-- Довідники (якщо ви їх наповнювали вручну)
DELETE FROM dbo.NotificationChannels;
DELETE FROM dbo.NotificationTypes;
DELETE FROM dbo.Platforms;
DELETE FROM dbo.ExpenseCategories;

COMMIT TRANSACTION;
GO

-- Перевірка: усі основні таблиці мають бути порожні
SELECT 'Masters' AS [Table], COUNT(*) AS Cnt FROM dbo.Masters
UNION ALL SELECT 'Clients', COUNT(*) FROM dbo.Clients
UNION ALL SELECT 'Services', COUNT(*) FROM dbo.Services
UNION ALL SELECT 'Appointments', COUNT(*) FROM dbo.Appointments
UNION ALL SELECT 'MasterTelegrams', COUNT(*) FROM dbo.MasterTelegrams
UNION ALL SELECT 'TelegramUserSessions', COUNT(*) FROM dbo.TelegramUserSessions;
GO
