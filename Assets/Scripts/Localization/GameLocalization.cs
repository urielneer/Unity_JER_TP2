// Punto de acceso unico a la localizacion //
// Unity Localization ya es global (LocalizationSettings), asi que un acceso estatico no agrega acoplamiento real. //
// SetService permite inyectar otra implementacion (tests, otro proveedor) sin tocar a los que la usan //
public static class GameLocalization
{
    #region    Constants
        public const string MAIN_TABLE = "TextosMenu";
        public const string SPANISH    = "es";
        public const string ENGLISH    = "en";
    #endregion Constants
    #region    Service
        private static ILocalizationService R_Ctm_ILS_Service;
        public static ILocalizationService Service => R_Ctm_ILS_Service ??= new UnityLocalizationService();
        public static void SetService(ILocalizationService Ctm_ILS_Service)
        {
            (R_Ctm_ILS_Service as System.IDisposable)?.Dispose();
            R_Ctm_ILS_Service = Ctm_ILS_Service;
        }
    #endregion Service
    #region    Shortcuts
        // Texto de la tabla principal con respaldo en espanol si la tabla todavia no cargo //
        public static string Get(string Str_Key, string Str_Fallback)
        {
            return Service.GetLocalizedValue(MAIN_TABLE, Str_Key, Str_Fallback);
        }
    #endregion Shortcuts
}
