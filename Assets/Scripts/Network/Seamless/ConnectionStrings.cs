// Textos de las pantallas de conexion //
// Centralizados para que el paso de Localization reemplace un solo archivo //
public static class ConnectionStrings
{
    public const string LostTitle        = "CONEXION PERDIDA";
    public const string Reconnecting     = "Reconectando...";
    public const string LostBodyFormat   = "Reconectando... (intento {0} de {1})";
    public const string OpponentTitle    = "RIVAL DESCONECTADO";
    public const string OpponentBodyFmt  = "Esperando su reconexion: {0} s";
    public const string ReconnectFailed  = "No se pudo reconectar a la partida.";
    public const string ConnectionLost   = "Se perdio la conexion con el servidor ({0}).";
    public const string WinByAbandon     = "Tu rival abandono la partida.";
    public const string LoseByAbandon    = "Abandonaste la partida.";
    public const string BackToMenuFormat = "Volviendo al menu en {0}...";
}
