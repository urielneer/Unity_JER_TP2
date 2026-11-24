using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

// Logica de reconexion "seamless" //
// Es una clase plana a proposito: NO recibe callbacks de PUN por su cuenta. //
// El NetworkManager es el unico MonoBehaviourPunCallbacks de conexion y le reenvia los eventos, //
// asi no hay dos scripts compitiendo por OnConnectedToMaster / OnJoinedRoom (bug de la version anterior) //
public sealed class ReconnectionHandler : INetworkReconnectionService
{
    #region    Variables
        #region    Settings
            private readonly int I_Int_MaxAttempts;
            private readonly float I_Flt_RetryDelay;
        #endregion Settings
        #region    Runtime
            private bool R_Bool_IsIntentionalLeave;       // El jugador salio a proposito: no se reconecta //
            private bool R_Bool_WasInRoom;                // Solo tiene sentido reconectar si estabamos en una sala //
            private bool R_Bool_IsReconnecting;
            private bool R_Bool_IsWaitingMasterForRejoin; // Camino alternativo: Reconnect() + RejoinRoom() //
            private bool R_Bool_RejoinNeedsNewAttempt;    // El rejoin pendiente viene de un "ActiveJoiner" //
            private int R_Int_Attempt;
            private string R_Str_LastRoomName;
        #endregion Runtime
        #region    Events
            public event Action<int, int> OnReconnectionAttempt;
            public event Action OnReconnectionSuccess;
            public event Action OnReconnectionFailed;
        #endregion Events
        #region    Properties
            public bool IsReconnecting => R_Bool_IsReconnecting;
            public bool HasPendingRejoin => R_Bool_IsWaitingMasterForRejoin;
            public float RetryDelay => I_Flt_RetryDelay;
            // Salida de sala que NO pidio el jugador => es una desconexion, no hay que volver al menu //
            public bool IsUnexpectedRoomExit => R_Bool_WasInRoom && !R_Bool_IsIntentionalLeave;
        #endregion Properties
    #endregion Variables
    #region    Methods
        #region    Constructor
            public ReconnectionHandler(int Int_MaxAttempts, float Flt_RetryDelay)
            {
                I_Int_MaxAttempts = Mathf.Max(1, Int_MaxAttempts);
                I_Flt_RetryDelay = Mathf.Max(0f, Flt_RetryDelay);
            }
        #endregion Constructor
        #region    Intentional Leave
            public void MarkIntentionalLeave()
            {
                R_Bool_IsIntentionalLeave = true;
                R_Bool_WasInRoom = false;
                Cancel();
            }
        #endregion Intentional Leave
        #region    Notifications (Las llama el NetworkManager)
            public void NotifyJoinedRoom()
            {
                // Variables //
                    R_Bool_WasInRoom = true;
                    R_Bool_IsIntentionalLeave = false;
                    R_Str_LastRoomName = PhotonNetwork.CurrentRoom.Name;
                // Era una reconexion ? //
                    if (!R_Bool_IsReconnecting) return;
                    Debug.Log("[Reconnection] Reingreso a la sala '" + R_Str_LastRoomName + "' exitoso en el intento " + R_Int_Attempt);
                    ResetRuntime();
                    OnReconnectionSuccess?.Invoke();
            }
            public bool ShouldReconnect(DisconnectCause E_Cause)
            {
                if (R_Bool_IsIntentionalLeave) return false;
                if (!R_Bool_WasInRoom && !R_Bool_IsReconnecting) return false;
                return IsRecoverable(E_Cause);
            }
            public void NotifyJoinRoomFailed(short Shrt_ReturnCode, string Str_Message)
            {
                if (!R_Bool_IsReconnecting) return;
                Debug.LogWarning("[Reconnection] El reingreso fallo (" + Shrt_ReturnCode + "): " + Str_Message);
                // El servidor todavia no detecto el corte y nos ve "activos": se reintenta desde el Master //
                // (al fallar un join en el Game Server, Photon vuelve solo al Master -> OnConnectedToMaster) //
                    if (Shrt_ReturnCode == ErrorCode.JoinFailedFoundActiveJoiner && R_Int_Attempt < I_Int_MaxAttempts)
                    {
                        R_Bool_IsWaitingMasterForRejoin = true;
                        R_Bool_RejoinNeedsNewAttempt = true;
                        return;
                    }
                // La sala ya no existe o el PlayerTtl vencio: no tiene sentido seguir intentando //
                    Fail();
            }
        #endregion Notifications (Las llama el NetworkManager)
        #region    Attempts
            // Devuelve true si quedo una conexion en curso (el resultado llega por OnJoinedRoom / OnDisconnected) //
            // Devuelve false si no arranco nada: o se agotaron los intentos (ya se disparo OnReconnectionFailed) //
            // o Photon no acepto el pedido (el NetworkManager agenda otro intento) //
            public bool TryNextAttempt()
            {
                // Limite de intentos - Evita el loop infinito OnDisconnected -> Reconnect -> OnDisconnected //
                    if (R_Int_Attempt >= I_Int_MaxAttempts)
                    {
                        Fail();
                        return false;
                    }
                    R_Bool_IsReconnecting = true;
                    R_Int_Attempt++;
                    OnReconnectionAttempt?.Invoke(R_Int_Attempt, I_Int_MaxAttempts);
                // Camino principal - Reconecta al Game Server y reingresa en un solo paso //
                    if (PhotonNetwork.ReconnectAndRejoin())
                    {
                        Debug.Log("[Reconnection] Intento " + R_Int_Attempt + ": ReconnectAndRejoin()");
                        return true;
                    }
                // Camino alternativo - Sin datos del Game Server: Master primero, RejoinRoom despues //
                    R_Bool_IsWaitingMasterForRejoin = !string.IsNullOrEmpty(R_Str_LastRoomName) && PhotonNetwork.Reconnect();
                    Debug.Log("[Reconnection] Intento " + R_Int_Attempt + ": Reconnect() al Master = " + R_Bool_IsWaitingMasterForRejoin);
                    return R_Bool_IsWaitingMasterForRejoin;
            }
            // El NetworkManager la llama en OnConnectedToMaster: si devuelve true, NO debe seguir el flujo normal del menu //
            public bool TryConsumePendingRejoin()
            {
                // Validacion //
                    if (!R_Bool_IsWaitingMasterForRejoin) return false;
                    R_Bool_IsWaitingMasterForRejoin = false;
                // Reintento por "ActiveJoiner": cuenta como un intento nuevo //
                    if (R_Bool_RejoinNeedsNewAttempt)
                    {
                        R_Bool_RejoinNeedsNewAttempt = false;
                        if (R_Int_Attempt >= I_Int_MaxAttempts) { Fail(); return true; }
                        R_Int_Attempt++;
                        OnReconnectionAttempt?.Invoke(R_Int_Attempt, I_Int_MaxAttempts);
                    }
                // Reingreso //
                    if (PhotonNetwork.RejoinRoom(R_Str_LastRoomName)) return true;
                    Fail();
                    return true;
            }
            // Corta el proceso en silencio (salida intencional) //
            public void Cancel()
            {
                ResetRuntime();
            }
            // Corta el proceso avisando el fallo (desconexion no recuperable en medio de la reconexion) //
            public void Abort()
            {
                if (R_Bool_IsReconnecting) Fail();
            }
            private void Fail()
            {
                Debug.LogWarning("[Reconnection] Reconexion fallida despues de " + R_Int_Attempt + " intento(s)");
                ResetRuntime();
                R_Bool_WasInRoom = false;
                OnReconnectionFailed?.Invoke();
            }
            private void ResetRuntime()
            {
                R_Bool_IsReconnecting = false;
                R_Bool_IsWaitingMasterForRejoin = false;
                R_Bool_RejoinNeedsNewAttempt = false;
                R_Int_Attempt = 0;
            }
        #endregion Attempts
        #region    Helpers
            // Solo se reintenta ante fallas de red; no ante errores de autenticacion, region, CCU, etc. //
            private static bool IsRecoverable(DisconnectCause E_Cause)
            {
                switch (E_Cause)
                {
                    case DisconnectCause.Exception:
                    case DisconnectCause.ExceptionOnConnect:
                    case DisconnectCause.DnsExceptionOnConnect:
                    case DisconnectCause.SendException:
                    case DisconnectCause.ReceiveException:
                    case DisconnectCause.ServerTimeout:
                    case DisconnectCause.ClientTimeout:
                    case DisconnectCause.DisconnectByServerReasonUnknown:
                        return true;
                    default:
                        return false;
                }
            }
        #endregion Helpers
    #endregion Methods
}
