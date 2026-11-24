using System;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

public class UnityLocalizationService : ILocalizationService
{
    public event Action OnLanguageChanged;

    public UnityLocalizationService()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale newLocale)
    {
        OnLanguageChanged?.Invoke();
    }

    public string GetLocalizedValue(string tableKey, string entryKey)
    {
        try
        {
            // Búsqueda inmediata para evitar que devuelva corchetes en el arranque
            var localizedString = LocalizationSettings.StringDatabase.GetLocalizedString(tableKey, entryKey);

            if (!string.IsNullOrEmpty(localizedString))
            {
                return localizedString;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Localization] Error al buscar la clave {entryKey}: {ex.Message}");
        }

        return $"[{entryKey}]";
    }

    public void SetLanguage(string localeCode)
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        for (int i = 0; i < locales.Count; i++)
        {
            if (locales[i].Identifier.Code == localeCode)
            {
                LocalizationSettings.SelectedLocale = locales[i];
                Debug.Log($"[Localization] Idioma cambiado a: {localeCode}");
                return;
            }
        }
        Debug.LogWarning($"[Localization] No se encontró el idioma con código: {localeCode}");
    }
}