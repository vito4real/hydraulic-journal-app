namespace HydraulicJournalApp.Services;

public class AccessGuardService
{
    private const string SecretWord = "о";

    public async Task<bool> EnsureWriteAccessAsync(Page page)
    {
        var enteredValue = await page.DisplayPromptAsync(
            "Доступ к изменению",
            "Введите кодовое слово:",
            accept: "OK",
            cancel: "Отмена",
            placeholder: "Кодовое слово",
            maxLength: 50,
            keyboard: Keyboard.Text);

        if (string.IsNullOrWhiteSpace(enteredValue))
            return false;

        if (string.Equals(enteredValue.Trim(), SecretWord, StringComparison.OrdinalIgnoreCase))
            return true;

        await page.DisplayAlert("Ошибка", "Неверное кодовое слово.", "OK");
        return false;
    }
}