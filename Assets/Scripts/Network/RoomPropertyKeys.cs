// Claves compartidas de las Room Custom Properties //
// Un unico lugar para las claves evita typos entre managers y deja listo el terreno //
// para el ciclo de etapas (Stage / StageStart) y los Power-Ups con CAS //
public static class RoomPropertyKeys
{
    #region    Conexion
        public const string PausedActor   = "Pause_Actor";    // int    - ActorNumber del jugador desconectado (0 = sin pausa) //
        public const string PauseDeadline = "Pause_Deadline"; // double - PhotonNetwork.Time en el que vence la espera //
    #endregion Conexion
    #region    Partida
        public const string MatchWinner    = "Match_Winner";  // int    - ActorNumber del ganador (0 = partida en curso) //
        public const string MatchEndReason = "Match_Reason";  // int    - (int) MatchEndReason //
    #endregion Partida
}

// Motivo de fin de partida - Se manda como int por red //
public enum MatchEndReason
{
    None    = 0,
    Abandon = 1,
    KnockOut = 2,
}
