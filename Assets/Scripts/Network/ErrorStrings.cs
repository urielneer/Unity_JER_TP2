using Photon.Realtime;

// Mensajes de la ventana de error - Traducidos y pensados para el jugador //
// Photon devuelve codigos (ErrorCode) y mensajes tecnicos en ingles: aca se traducen a algo entendible //
public static class ErrorStrings
{
    #region    Textos
        public static string EmptyRoomName      => GameLocalization.Get("Error_NombreSalaVacio", "Escribe un nombre para la sala.");
        public static string RoomAlreadyExists  => GameLocalization.Get("Error_SalaExiste",      "Ya existe una sala con ese nombre.");
        public static string RoomFull           => GameLocalization.Get("Error_SalaLlena",       "La sala está llena.");
        public static string RoomClosed         => GameLocalization.Get("Error_SalaCerrada",     "La partida en esa sala ya empezó.");
        public static string RoomNotFound       => GameLocalization.Get("Error_SalaNoExiste",    "La sala ya no existe.");
        public static string CreateRoomFormat   => GameLocalization.Get("Error_CrearSala",       "No se pudo crear la sala ({0}).");
        public static string JoinRoomFormat     => GameLocalization.Get("Error_UnirseSala",      "No se pudo entrar a la sala ({0}).");
        public static string CannotConnectFormat=> GameLocalization.Get("Error_SinConexion",     "No se pudo conectar al servidor ({0}). Reintentando...");
    #endregion Textos
    #region    Codigos de Photon
        public static string ForCreateRoom(short Shrt_Code)
        {
            switch (Shrt_Code)
            {
                case ErrorCode.GameIdAlreadyExists: return RoomAlreadyExists;
                default:                            return string.Format(CreateRoomFormat, Shrt_Code);
            }
        }
        public static string ForJoinRoom(short Shrt_Code)
        {
            switch (Shrt_Code)
            {
                case ErrorCode.GameFull:         return RoomFull;
                case ErrorCode.GameClosed:       return RoomClosed;
                case ErrorCode.GameDoesNotExist: return RoomNotFound;
                default:                         return string.Format(JoinRoomFormat, Shrt_Code);
            }
        }
    #endregion Codigos de Photon
}
