# Retrieving Localized Text Manually
You can use `LocalizeUIText` to automate the localization by leveraging components but sometimes you may need to manually retrieve the localized value of a key. In such cases, you can use the `GetLocalizedValue(string key, params object[] variables)` of `Localization Manager`.

For static localized text, you can leave the variables empty and for dynamic localized text, you can provide the variables by separating them with a comma. For instance:

> Localization Manager.GetLocalizedValue("Warm Hello"); // This is static and will get "Hi friend, how are you doing?"<br /><br />
> Localization Manager.GetLocalizedValue("Cold Hello", playerName, playerDebt); // This is dynamic and will get "Hi {playerName}, you owe me {playerDebt} gold"