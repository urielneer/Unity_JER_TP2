using System;

public interface ILocalizationService
{
    event Action OnLanguageChanged;
    string GetLocalizedValue(string tableKey, string entryKey);
    void SetLanguage(string localeCode); 
}