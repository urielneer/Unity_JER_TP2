using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

// Implementacion con el paquete Unity Localization //
// Hay UNA sola instancia (ver GameLocalization): antes cada LocalizedTextUI creaba la suya y se suscribia //
// al evento estatico SelectedLocaleChanged sin desuscribirse nunca //
public sealed class UnityLocalizationService : ILocalizationService, IDisposable
{
    #region    Constants
        private const string LANGUAGE_PREF_KEY = "CowboyProject.Language";
    #endregion Constants
    #region    Events
        public event Action OnLanguageChanged;
    #endregion Events
    #region    Properties
        public string CurrentLanguageCode
        {
            get
            {
                if (!LocalizationSettings.InitializationOperation.IsDone) return null;
                Locale Lcl_Selected = LocalizationSettings.SelectedLocale;
                return Lcl_Selected != null ? Lcl_Selected.Identifier.Code : null;
            }
        }
    #endregion Properties
    #region    Methods
        #region    Constructor / Dispose
            public UnityLocalizationService()
            {
                LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
                // Idioma elegido por el jugador en una sesion anterior //
                    if (LocalizationSettings.InitializationOperation.IsDone) ApplySavedLanguage();
                    else LocalizationSettings.InitializationOperation.Completed += Op_Handle => ApplySavedLanguage();
            }
            public void Dispose()
            {
                LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            }
        #endregion Constructor / Dispose
        #region    API
            public string GetLocalizedValue(string tableKey, string entryKey)
            {
                return GetLocalizedValue(tableKey, entryKey, "[" + entryKey + "]");
            }
            public string GetLocalizedValue(string tableKey, string entryKey, string fallback)
            {
                try
                {
                    string Str_Localized = LocalizationSettings.StringDatabase.GetLocalizedString(tableKey, entryKey);
                    if (!string.IsNullOrEmpty(Str_Localized)) return Str_Localized;
                }
                catch (Exception Exc_Error)
                {
                    Debug.LogWarning("[Localization] Error al buscar la clave '" + entryKey + "': " + Exc_Error.Message);
                }
                return fallback;
            }
            public void SetLanguage(string localeCode)
            {
                Locale Lcl_Target = FindLocale(localeCode);
                if (Lcl_Target == null)
                {
                    Debug.LogWarning("[Localization] No se encontro el idioma con codigo: " + localeCode);
                    return;
                }
                PlayerPrefs.SetString(LANGUAGE_PREF_KEY, localeCode);
                PlayerPrefs.Save();
                if (LocalizationSettings.SelectedLocale == Lcl_Target) return;
                LocalizationSettings.SelectedLocale = Lcl_Target;
                Debug.Log("[Localization] Idioma cambiado a: " + localeCode);
            }
        #endregion API
        #region    Internals
            private void ApplySavedLanguage()
            {
                if (!PlayerPrefs.HasKey(LANGUAGE_PREF_KEY)) return;
                Locale Lcl_Saved = FindLocale(PlayerPrefs.GetString(LANGUAGE_PREF_KEY));
                if (Lcl_Saved != null && LocalizationSettings.SelectedLocale != Lcl_Saved)
                    LocalizationSettings.SelectedLocale = Lcl_Saved;
            }
            private static Locale FindLocale(string Str_Code)
            {
                if (string.IsNullOrEmpty(Str_Code)) return null;
                var Lst_Locales = LocalizationSettings.AvailableLocales.Locales;
                for (int Int_Index = 0; Int_Index < Lst_Locales.Count; Int_Index++)
                {
                    if (Lst_Locales[Int_Index].Identifier.Code == Str_Code) return Lst_Locales[Int_Index];
                }
                return null;
            }
            private void OnLocaleChanged(Locale Lcl_New)
            {
                OnLanguageChanged?.Invoke();
            }
        #endregion Internals
    #endregion Methods
}
