using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

#region    "using" Script Clarification
    using Hashtable = ExitGames.Client.Photon.Hashtable;
#endregion "using" Script Clarification

// Pausa por desconexion y victoria por abandono //
// Autoridad: SOLO el MasterClient escribe las Room Properties. Todos los clientes reaccionan a ellas. //
// Como el estado vive en la Room (y no en el Master), si el Master se cae, el nuevo Master lo retoma //
public sealed class MatchConnectionMonitor
{
    #region    Variables
        #region    Settings
            private readonly float I_Flt_AbandonTimeout;
        #endregion Settings
        #region    Runtime
            private int R_Int_PausedActor;
            private double R_Dbl_Deadline;
            private int R_Int_Winner;
        #endregion Runtime
        #region    Events
            public event Action<int, double> OnPauseStarted;                // (actor desconectado, deadline en PhotonNetwork.Time) //
            public event Action OnPauseEnded;
            public event Action<int, MatchEndReason> OnMatchEnded;          // (actor ganador, motivo) //
        #endregion Events
        #region    Properties
            public bool IsPaused => R_Int_PausedActor != 0 && R_Int_PausedActor != LocalActor;
            public bool HasEnded => R_Int_Winner != 0;
            private static int LocalActor => PhotonNetwork.LocalPlayer.ActorNumber;
        #endregion Properties
    #endregion Variables
    #region    Methods
        #region    Constructor
            public MatchConnectionMonitor(float Flt_AbandonTimeout)
            {
                I_Flt_AbandonTimeout = Flt_AbandonTimeout;
            }
        #endregion Constructor
        #region    Authority (Solo MasterClient)
            public static Hashtable BuildResetProperties()
            {
                return new Hashtable
                {
                    { RoomPropertyKeys.PausedActor,    0 },
                    { RoomPropertyKeys.PauseDeadline,  0d },
                    { RoomPropertyKeys.MatchWinner,    0 },
                    { RoomPropertyKeys.MatchEndReason, (int) MatchEndReason.None },
                };
            }
            public void HandlePlayerInactive(Player PUNPl_Player)
            {
                // Validacion //
                    if (!PhotonNetwork.IsMasterClient || HasEnded) return;
                    if (R_Int_PausedActor == PUNPl_Player.ActorNumber) return; // Ya esta pausado: se respeta el deadline original //
                // Publicar pausa //
                    double Dbl_Deadline = PhotonNetwork.Time + I_Flt_AbandonTimeout;
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
                    {
                        { RoomPropertyKeys.PausedActor,   PUNPl_Player.ActorNumber },
                        { RoomPropertyKeys.PauseDeadline, Dbl_Deadline },
                    });
                    Debug.Log("[Match Monitor] Actor " + PUNPl_Player.ActorNumber + " inactivo. Pausa hasta " + Dbl_Deadline.ToString("F1"));
            }
            public void HandlePlayerReturned(Player PUNPl_Player)
            {
                if (!PhotonNetwork.IsMasterClient || HasEnded) return;
                if (R_Int_PausedActor != PUNPl_Player.ActorNumber) return;
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
                {
                    { RoomPropertyKeys.PausedActor,   0 },
                    { RoomPropertyKeys.PauseDeadline, 0d },
                });
                Debug.Log("[Match Monitor] Actor " + PUNPl_Player.ActorNumber + " volvio. Se reanuda la partida");
            }
            public void HandlePlayerGone(Player PUNPl_Player)
            {
                // Se fue de verdad (salio con el boton o vencio el PlayerTtl) //
                if (!PhotonNetwork.IsMasterClient || HasEnded) return;
                DeclareAbandon(PUNPl_Player.ActorNumber);
            }
            public void HandleMasterSwitched()
            {
                // El nuevo Master retoma la autoridad leyendo el estado de la Room (GDD: "Se cae el Master") //
                if (!PhotonNetwork.IsMasterClient || HasEnded) return;
                foreach (Player PUNPl_Other in PhotonNetwork.PlayerListOthers)
                {
                    if (PUNPl_Other.IsInactive) HandlePlayerInactive(PUNPl_Other);
                }
            }
            public void Tick()
            {
                if (!PhotonNetwork.IsMasterClient || HasEnded || R_Int_PausedActor == 0) return;
                if (PhotonNetwork.Time < R_Dbl_Deadline) return;
                DeclareAbandon(R_Int_PausedActor);
            }
            private void DeclareAbandon(int Int_LoserActor)
            {
                // Ganador: el primer jugador activo que no sea el que abandono //
                    int Int_Winner = 0;
                    foreach (Player PUNPl_Player in PhotonNetwork.PlayerList)
                    {
                        if (PUNPl_Player.ActorNumber == Int_LoserActor || PUNPl_Player.IsInactive) continue;
                        Int_Winner = PUNPl_Player.ActorNumber;
                        break;
                    }
                    if (Int_Winner == 0) return;
                // CAS (expectedProperties): solo se escribe si todavia no hay ganador //
                // Si el Tick y el evento Leave llegan juntos, el segundo pedido lo rechaza el servidor //
                    PhotonNetwork.CurrentRoom.SetCustomProperties
                    (
                        new Hashtable
                        {
                            { RoomPropertyKeys.MatchWinner,    Int_Winner },
                            { RoomPropertyKeys.MatchEndReason, (int) MatchEndReason.Abandon },
                            { RoomPropertyKeys.PausedActor,    0 },
                            { RoomPropertyKeys.PauseDeadline,  0d },
                        },
                        new Hashtable { { RoomPropertyKeys.MatchWinner, 0 } }
                    );
                    Debug.Log("[Match Monitor] Victoria por abandono para el Actor " + Int_Winner);
            }
        #endregion Authority (Solo MasterClient)
        #region    Replication (Todos los clientes)
            // Lee el estado completo (al empezar la partida o al reconectarse) //
            public void SyncFromRoom()
            {
                R_Int_PausedActor = 0;
                R_Dbl_Deadline = 0d;
                R_Int_Winner = 0;
                if (PhotonNetwork.CurrentRoom == null) return;
                ApplyRoomProperties(PhotonNetwork.CurrentRoom.CustomProperties);
            }
            public void ApplyRoomProperties(Hashtable Hsh_Changed)
            {
                // Fin de partida - Tiene prioridad sobre la pausa //
                    if (Hsh_Changed.TryGetValue(RoomPropertyKeys.MatchWinner, out object Obj_Winner) && Obj_Winner is int Int_Winner)
                    {
                        bool Bool_JustEnded = Int_Winner != 0 && R_Int_Winner == 0;
                        R_Int_Winner = Int_Winner;
                        if (Bool_JustEnded)
                        {
                            MatchEndReason E_Reason = Hsh_Changed.TryGetValue(RoomPropertyKeys.MatchEndReason, out object Obj_Reason) && Obj_Reason is int Int_Reason
                                                      ? (MatchEndReason) Int_Reason
                                                      : MatchEndReason.None;
                            R_Int_PausedActor = 0;
                            OnMatchEnded?.Invoke(Int_Winner, E_Reason);
                            return;
                        }
                    }
                // Pausa //
                    if (Hsh_Changed.TryGetValue(RoomPropertyKeys.PauseDeadline, out object Obj_Deadline) && Obj_Deadline is double Dbl_Deadline)
                        R_Dbl_Deadline = Dbl_Deadline;
                    if (Hsh_Changed.TryGetValue(RoomPropertyKeys.PausedActor, out object Obj_Paused) && Obj_Paused is int Int_Paused)
                    {
                        bool Bool_WasPaused = IsPaused;
                        R_Int_PausedActor = Int_Paused;
                        if (IsPaused)                        OnPauseStarted?.Invoke(R_Int_PausedActor, R_Dbl_Deadline);
                        else if (Bool_WasPaused && !HasEnded) OnPauseEnded?.Invoke();
                    }
            }
        #endregion Replication (Todos los clientes)
    #endregion Methods
}
