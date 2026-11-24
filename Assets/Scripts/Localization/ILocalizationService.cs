using System;

// Contrato de localizacion - El juego depende de esto, no del paquete Unity Localization //
public interface ILocalizationService
{
    event Action OnLanguageChanged;

    string CurrentLanguageCode { get; } // "es", "en" o null si todavia no termino de inicializar //

    string GetLocalizedValue(string tableKey, string entryKey);
    // Devuelve Str_Fallback si la tabla todavia no cargo o falta la clave (nunca muestra "[Clave]" al jugador) //
    string GetLocalizedValue(string tableKey, string entryKey, string fallback);

    void SetLanguage(string localeCode);
}
