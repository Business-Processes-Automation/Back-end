namespace Business_Processes_Automation.BLL.Localization;

public static class MasterTelegramLinkMessages
{
    public const string InvalidLinkCodeFormat =
        "Код має містити 8 латинських літер або цифр.";

    public const string InvalidOrExpiredLinkCode =
        "Невірний або прострочений код. Згенеруйте новий у веб-кабінеті.";

    public const string AccountAlreadyLinked =
        "Цей акаунт уже прив'язаний до Telegram.";

    public const string TelegramAlreadyLinkedToOtherAccount =
        "Ваш Telegram уже прив'язаний до іншого акаунта.";

    public const string InvalidBotLinkFormat =
        "Ідентифікатор: латинські літери, цифри та _, від 3 до 32 символів (наприклад, my_salon).";

    public const string BotLinkTaken =
        "Цей ідентифікатор уже зайнятий. Введіть інший.";

    public const string TelegramAlreadyLinkedToWebAccount =
        "Telegram уже прив'язано до цього акаунта.";
}
