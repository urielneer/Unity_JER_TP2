// Textos de las pantallas de conexion - Traducidos con la tabla principal (respaldo en espanol) //
// Son propiedades (no const) para que siempre devuelvan el idioma actual //
public static class ConnectionStrings
{
    public static string LostTitle        => GameLocalization.Get("Conexion_PerdidaTitulo",       "CONEXIÓN PERDIDA");
    public static string Reconnecting     => GameLocalization.Get("Conexion_Reconectando",        "Reconectando...");
    public static string LostBodyFormat   => GameLocalization.Get("Conexion_ReconectandoIntento", "Reconectando... (intento {0} de {1})");
    public static string OpponentTitle    => GameLocalization.Get("Conexion_RivalTitulo",         "RIVAL DESCONECTADO");
    public static string OpponentBodyFmt  => GameLocalization.Get("Conexion_RivalEsperando",      "Esperando su reconexión: {0} s");
    public static string ReconnectFailed  => GameLocalization.Get("Conexion_FalloReconexion",     "No se pudo reconectar a la partida.");
    public static string ConnectionLost   => GameLocalization.Get("Conexion_Perdida",             "Se perdió la conexión con el servidor ({0}).");
    public static string WinByAbandon     => GameLocalization.Get("Resultado_GanasteAbandono",    "Tu rival abandonó la partida.");
    public static string LoseByAbandon    => GameLocalization.Get("Resultado_PerdisteAbandono",   "Abandonaste la partida.");
    public static string BackToMenuFormat => GameLocalization.Get("Resultado_VolviendoMenu",      "Volviendo al menú en {0}...");
}
