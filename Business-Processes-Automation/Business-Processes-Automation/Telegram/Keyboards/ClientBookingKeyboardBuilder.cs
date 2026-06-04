using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;
using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot.Types.ReplyMarkups;

namespace Business_Processes_Automation.Telegram.Keyboards;

public static class ClientBookingKeyboardBuilder
{
    private const int SlotButtonsPerRow = 3;
    private const int DateButtonsPerRow = 2;
    private const int MaxButtonLength = 64;

    public static ReplyKeyboardMarkup BuildPeriodMenu() =>
        new([
            [
                new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonViewTomorrow),
                new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonViewThreeDays)
            ],
            [
                new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonViewWeek),
                new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonViewMonth)
            ],
            [new KeyboardButton(TelegramBotTexts.Menu.ButtonBackToMenu)]
        ])
        {
            ResizeKeyboard = true
        };

    public static ReplyKeyboardMarkup BuildServiceMenu(IReadOnlyList<Service> services)
    {
        var rows = services
            .Select(service => new[] { new KeyboardButton(FormatServiceLabel(service)) })
            .ToList();

        rows.Add([new KeyboardButton(TelegramBotTexts.Menu.ButtonBackToMenu)]);
        return CreateKeyboard(rows);
    }

    public static ReplyKeyboardMarkup BuildDateMenu(IReadOnlyList<DateOnly> dates)
    {
        var rows = Chunk(
                dates.Select(FormatDateLabel).Select(label => new KeyboardButton(label)),
                DateButtonsPerRow)
            .Select(chunk => chunk.ToArray())
            .ToList();

        rows.Add([new KeyboardButton(TelegramBotTexts.Menu.ButtonBackToMenu)]);
        return CreateKeyboard(rows);
    }

    public static ReplyKeyboardMarkup BuildSlotMenu(IReadOnlyList<TimeOnly> slotTimes, bool showBackToDates)
    {
        var rows = Chunk(
                slotTimes.Select(FormatSlotLabel).Select(label => new KeyboardButton(label)),
                SlotButtonsPerRow)
            .Select(chunk => chunk.ToArray())
            .ToList();

        if (showBackToDates)
        {
            rows.Add([new KeyboardButton(TelegramBotTexts.ClientBooking.ButtonBackToDates)]);
        }

        rows.Add([new KeyboardButton(TelegramBotTexts.Menu.ButtonBackToMenu)]);
        return CreateKeyboard(rows);
    }

    public static ReplyKeyboardMarkup BuildConfirmation() =>
        new([
            [new KeyboardButton(TelegramBotTexts.ClientBooking.ButtonConfirm)],
            [new KeyboardButton(TelegramBotTexts.ClientBooking.ButtonCancel)],
            [new KeyboardButton(TelegramBotTexts.Menu.ButtonBackToMenu)]
        ])
        {
            ResizeKeyboard = true
        };

    public static string FormatServiceLabel(Service service)
    {
        var label = $"{service.ServiceName} ({service.DurationInMinutes} хв)";
        return label.Length <= MaxButtonLength ? label : label[..(MaxButtonLength - 1)] + "…";
    }

    public static string FormatDateLabel(DateOnly date)
    {
        var weekday = (Weekday)(int)date.DayOfWeek;
        return $"{date:dd.MM} ({FormatDayShort(weekday)})";
    }

    public static string FormatSlotLabel(TimeOnly time) => time.ToString("HH:mm");

    public static Service? TryMatchService(string text, IReadOnlyList<Service> services) =>
        services.FirstOrDefault(service => FormatServiceLabel(service) == text);

    public static DateOnly? TryMatchDate(string text, IReadOnlyList<DateOnly> dates)
    {
        foreach (var date in dates)
        {
            if (FormatDateLabel(date) == text)
            {
                return date;
            }
        }

        return null;
    }

    public static TimeOnly? TryMatchSlotTime(string text, IReadOnlyList<TimeOnly> slotTimes)
    {
        foreach (var time in slotTimes)
        {
            if (FormatSlotLabel(time) == text)
            {
                return time;
            }
        }

        return null;
    }

    public static bool IsViewPeriodButton(string text) =>
        text is TelegramBotTexts.MasterSchedule.ButtonViewTomorrow
            or TelegramBotTexts.MasterSchedule.ButtonViewThreeDays
            or TelegramBotTexts.MasterSchedule.ButtonViewWeek
            or TelegramBotTexts.MasterSchedule.ButtonViewMonth;

    public static bool IsConfirm(string text) =>
        text == TelegramBotTexts.ClientBooking.ButtonConfirm;

    public static bool IsCancel(string text) =>
        text == TelegramBotTexts.ClientBooking.ButtonCancel;

    public static bool IsBackToDates(string text) =>
        text == TelegramBotTexts.ClientBooking.ButtonBackToDates;

    private static ReplyKeyboardMarkup CreateKeyboard(IEnumerable<KeyboardButton[]> rows) =>
        new(rows)
        {
            ResizeKeyboard = true
        };

    private static IEnumerable<IReadOnlyList<T>> Chunk<T>(IEnumerable<T> source, int size)
    {
        var chunk = new List<T>(size);
        foreach (var item in source)
        {
            chunk.Add(item);
            if (chunk.Count == size)
            {
                yield return chunk;
                chunk = new List<T>(size);
            }
        }

        if (chunk.Count > 0)
        {
            yield return chunk;
        }
    }

    private static string FormatDayShort(Weekday day) => day switch
    {
        Weekday.Monday => "Пн",
        Weekday.Tuesday => "Вт",
        Weekday.Wednesday => "Ср",
        Weekday.Thursday => "Чт",
        Weekday.Friday => "Пт",
        Weekday.Saturday => "Сб",
        Weekday.Sunday => "Нд",
        _ => day.ToString()
    };
}
