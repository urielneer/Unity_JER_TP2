using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using static CustomExtension.ArrayExtensions;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

#region    ...
#endregion ...

public class NetworkManager : BaseManager<NetworkManager>
{
    #region    Constants
        public const int   MENU_SCENE_INDEX   = 0;
        public const int   MATCH_SCENE_INDEX  = 1;
        public const byte  ROOM_MAX_PLAYERS   = 2;      // GDD: duelos 1v1 //
        public const int   PLAYER_TTL_MS      = 30000;  // GDD: 30 s para volver antes de perder por abandono //
        public const float PLAYER_TTL_SECONDS = PLAYER_TTL_MS / 1000f;
        private const string ERROR_MENU_NAME  = "ErrorMenu";
    #endregion Constants
    #region    Variables
        #region    Connection Settings
            [Header(" Connection Settings")]
                [Tooltip("Milisegundos sin respuesta del servidor antes de considerar la conexion perdida")]
                [SerializeField] private int _disconnectTimeoutMs = 5000;
                [Tooltip("Intentos x espera deben cubrir lo que tarda el servidor en detectar el corte (~10 s)")]
                [SerializeField] private int _maxReconnectAttempts = 8;
                [Tooltip("Segundos entre intentos de reconexion")]
                [SerializeField] private float _reconnectRetryDelay = 1.5f;
                [Tooltip("Segundos entre intentos de conectar al servidor de Photon cuando no hay conexion (mientras se muestra el error)")]
                [SerializeField] private float _connectRetryDelay = 5f;
        #endregion Connection Settings
        #region    Reconnection
            private ReconnectionHandler R_Ctm_RH_Reconnection;
            public INetworkReconnectionService Reconnection => R_Ctm_RH_Reconnection;
            private Coroutine R_Crtn_Reconnect;
            private Coroutine R_Crtn_ConnectRetry;
            private bool R_Bool_HasReachedMaster; // Distingue "no se pudo conectar" de "se perdio la conexion" //
        #endregion Reconnection
        #region    Manager-To-Manager Data
            private bool O_Bool_IsStatusOn = false;
            public bool IsStatusOn => O_Bool_IsStatusOn;
            public static bool IsInMatchScene => SceneManager.GetActiveScene().buildIndex == MATCH_SCENE_INDEX;
        #endregion Manager-To-Manager Data
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Singleton Operations //
                    base.Awake();
                    if (_instance != this) return;
                // Variables //
                    R_Ctm_RH_Reconnection = new ReconnectionHandler(_maxReconnectAttempts, _reconnectRetryDelay);
                    R_Ctm_RH_Reconnection.OnReconnectionAttempt += HandleReconnectionAttempt;
                    R_Ctm_RH_Reconnection.OnReconnectionSuccess += HandleReconnectionSuccess;
                    R_Ctm_RH_Reconnection.OnReconnectionFailed  += HandleReconnectionFailed;
            }
            private void OnDestroy()
            {
                if (R_Ctm_RH_Reconnection == null) return;
                R_Ctm_RH_Reconnection.OnReconnectionAttempt -= HandleReconnectionAttempt;
                R_Ctm_RH_Reconnection.OnReconnectionSuccess -= HandleReconnectionSuccess;
                R_Ctm_RH_Reconnection.OnReconnectionFailed  -= HandleReconnectionFailed;
            }
        #endregion Unity Methods
        #region    Override Methods
            #pragma warning disable CS1998
            public async override void OnStartUp()
            #pragma warning restore CS1998
            {
                if (!IsStatusOn)
                {
                    // Resume Base //
                        base.OnStartUp();
                    // Log On //
                        MasterManager.Instance.MenuManager.OpenMenu("LoadingMenu");
                        PhotonNetwork.OfflineMode = false;
                        PhotonNetwork.ConnectUsingSettings();
                        PhotonNetwork.SendRate = 30;
                        PhotonNetwork.SerializationRate = 30;
                        PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = _disconnectTimeoutMs;
                        O_Bool_IsStatusOn = true;
                        Debug.Log("[Network Manager] Status: 'On'");
                }
            }
        #endregion Override Methods
        #region    PUN Methods
            #region    Connection
                public override void OnDisconnected(DisconnectCause E_Cause)
                {
                    Debug.Log("[Network Manager] Status: 'Disconnected' (" + E_Cause + ")");
                    // Corte recuperable estando en una sala: reconexion seamless //
                        if (R_Ctm_RH_Reconnection.ShouldReconnect(E_Cause))
                        {
                            if (!R_Ctm_RH_Reconnection.IsReconnecting)
                            {
                                ConnectionStatusOverlay.Show(ConnectionStrings.LostTitle, ConnectionStrings.Reconnecting);
                                if (IsInMatchScene) MasterManager.Instance.InputManager.DisableInputPlayerActions();
                            }
                            ScheduleReconnectAttempt();
                            return;
                        }
                    // Corte no recuperable en medio de una reconexion //
                        if (R_Ctm_RH_Reconnection.IsReconnecting)
                        {
                            R_Ctm_RH_Reconnection.Abort();
                            return;
                        }
                    // Desconexion pedida por el juego (cerrar la app, etc.) //
                        bool Bool_HadConnection = R_Bool_HasReachedMaster;
                        R_Bool_HasReachedMaster = false;
                        O_Bool_IsStatusOn = false;
                        if (E_Cause == DisconnectCause.DisconnectByClientLogic || E_Cause == DisconnectCause.ApplicationQuit) return;
                    // Cualquier otro corte (menu, lobby, o no se pudo conectar al abrir el juego): error + reintento //
                        GoToTitle(string.Format(Bool_HadConnection ? ConnectionStrings.ConnectionLost : ErrorStrings.CannotConnectFormat, E_Cause));
                }
            #endregion Connection
            #region    Title Room
                public override void OnConnectedToMaster()
                {
                    // Reconexion en curso: reingresar a la sala sin pasar por el menu //
                        if (R_Ctm_RH_Reconnection.HasPendingRejoin)
                        {
                            ScheduleRejoinFromMaster();
                            return;
                        }
                    // PUN //
                        R_Bool_HasReachedMaster = true;
                        StopConnectRetry();
                        PhotonNetwork.JoinLobby();
                        PhotonNetwork.AutomaticallySyncScene = true;
                    // Menu Manager Communication - Si el jugador esta leyendo un error, no se lo saca: el boton del ErrorMenu lo lleva al menu //
                        if (!IsErrorMenuOpen()) MasterManager.Instance.MenuManager.OpenMenu("TitleMenu");
                    Debug.Log("[Network Manager] Status: 'Joined Master'");
                }
                public override void OnJoinedLobby()
                {
                    Debug.Log("[Network Manager] Status: 'Joined Lobby'");
                }
            #endregion Title Room
            #region    Find Room
                public override void OnJoinedRoom()
                {
                    // Reconexion - Si volvimos en medio de la partida, no se toca el menu (seamless) //
                        bool Bool_WasReconnecting = R_Ctm_RH_Reconnection.IsReconnecting;
                        R_Ctm_RH_Reconnection.NotifyJoinedRoom();
                        if (Bool_WasReconnecting && IsInMatchScene) return;
                    // Menu Manager Communication //
                        // Variables //
                            MenuTypeWaitingRoom Ctm_MTWR_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("WaitingRoom")).gameObject.GetComponent<MenuTypeWaitingRoom>();
                        // Update Room Name //
                            Ctm_MTWR_Reference.SetUpRoom();
                            Ctm_MTWR_Reference.JoinRoom();
                    Debug.Log("[Network Manager] Status: 'Joined room'");
                }
                public override void OnJoinRoomFailed(short Shrt_ReturnCode, string Str_Message)
                {
                    bool Bool_WasReconnecting = R_Ctm_RH_Reconnection.IsReconnecting;
                    R_Ctm_RH_Reconnection.NotifyJoinRoomFailed(Shrt_ReturnCode, Str_Message);
                    if (!Bool_WasReconnecting) ShowError(ErrorStrings.ForJoinRoom(Shrt_ReturnCode));
                    Debug.Log("[Network Manager] Status: 'Join Room Failed' (" + Shrt_ReturnCode + "): " + Str_Message);
                }
                public override void OnRoomListUpdate(List<RoomInfo> Lst_PUNRI_RoomList)
                { 
                    // Menu Manager Communication - Update List //
                        MenuTypeFindRoom Ctm_MTFR_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("FindRoomMenu")).gameObject.GetComponent<MenuTypeFindRoom>();                 
                        Ctm_MTFR_Reference.ClearRoomList();
                        Ctm_MTFR_Reference.UpdateList(Lst_PUNRI_RoomList);
                    Debug.Log("[Network Manager] Status: 'Updated List'");
                }
            #endregion Find Room
            #region    Waiting Room / Match
                public override void OnPlayerEnteredRoom(Player PUNPl_NewPlayer)
                {
                    // Game Manager Communication - Un jugador volvio a la partida //
                        if (IsInMatchScene && PUNPl_NewPlayer.HasRejoined)
                            MasterManager.Instance.GameManager.OnPlayerReturned(PUNPl_NewPlayer);
                    Debug.Log("[Network Manager] Status: 'Player Entered Room' (Actor " + PUNPl_NewPlayer.ActorNumber + ", rejoin: " + PUNPl_NewPlayer.HasRejoined + ")");
                }
                public override void OnPlayerLeftRoom(Player PUNPl_OtherPlayer) // ANYONE LEAVES THE ROOM //
                {
                    // En partida: inactivo (puede volver) o ido de verdad (abandono) //
                        if (IsInMatchScene)
                        {
                            if (PUNPl_OtherPlayer.IsInactive) MasterManager.Instance.GameManager.OnPlayerInactive(PUNPl_OtherPlayer);
                            else                              MasterManager.Instance.GameManager.OnPlayerGone(PUNPl_OtherPlayer);
                            Debug.Log("[Network Manager] Status: 'Player Left Match' (Actor " + PUNPl_OtherPlayer.ActorNumber + ", inactivo: " + PUNPl_OtherPlayer.IsInactive + ")");
                            return;
                        }
                    // En la sala de espera //
                        MenuTypeWaitingRoom Ctm_MTWR_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("WaitingRoom")).gameObject.GetComponent<MenuTypeWaitingRoom>();
                        Ctm_MTWR_Reference.SetUpRoom();
                        ReassignVigilantIfNeeded(PUNPl_OtherPlayer);
                    Debug.Log("[Network Manager] Status: 'Player Left Room'");
                }
                public override void OnMasterClientSwitched(Player PUNPl_NewMaster)
                {
                    // GDD: si se cae el Master, el que queda retoma la autoridad leyendo el estado de la Room //
                        if (IsInMatchScene) MasterManager.Instance.GameManager.OnMasterSwitched();
                    Debug.Log("[Network Manager] Status: 'Master Switched' (Actor " + PUNPl_NewMaster.ActorNumber + ")");
                }
                private void ReassignVigilantIfNeeded(Player PUNPl_Gone)
                {
                    // Solo el Master decide //
                        if (!PhotonNetwork.IsMasterClient) return;
                        if (PhotonNetwork.CurrentRoom == null) return;
                    // Era el Vigilante el que se fue ? //
                        if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("VigilantActor", out object Obj_Actor)) return;
                        if ((int) Obj_Actor != PUNPl_Gone.ActorNumber) return;
                    // Elegir reemplazo //
                        Player PUNPl_New = null;
                        foreach (Player PUNPl_Candidate in PhotonNetwork.PlayerList)
                        {
                            if (PUNPl_Candidate.ActorNumber == PUNPl_Gone.ActorNumber) continue;
                            PUNPl_New = PUNPl_Candidate;
                            break;
                        }
                        if (PUNPl_New == null) return;
                    // Publicar nuevo Vigilante - Ya NO se reinicia la partida: eso rompia la migracion de Master //
                        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "VigilantActor", PUNPl_New.ActorNumber } });
                        Debug.Log("[Network Manager] Vigilante se fue. Nuevo Vigilante: Actor " + PUNPl_New.ActorNumber);
                }
                public override void OnLeftRoom() // CURRENT PLAYER LEAVES THE ROOM //
                {
                    // PUN llama a OnLeftRoom ANTES que a OnDisconnected cuando se corta la conexion //
                    // Si no fue una salida pedida, se espera a OnDisconnected para reconectar (no se vuelve al menu) //
                        if (R_Ctm_RH_Reconnection.IsUnexpectedRoomExit)
                        {
                            Debug.Log("[Network Manager] Status: 'Left Room' por desconexion - Esperando reconexion");
                            return;
                        }
                    Debug.Log("[Network Manager] Status: 'Left Room'");
                    // Volver al menu principal //
                        MasterManager.Instance.GameManager.ShowCursor();
                        ReturnToTitle();
                }
            #endregion Waiting Room / Match
            #region    Error Room
                public override void OnCreateRoomFailed(short Shrt_ReturnCode, string Str_Message)
                {
                    ShowError(ErrorStrings.ForCreateRoom(Shrt_ReturnCode));
                    Debug.Log("[Network Manager] Status: 'ERROR FOUND!: "+Str_Message+"'");
                }
            #endregion Error Room
        #endregion PUN Methods
        #region    Reconnection Methods
            private void ScheduleReconnectAttempt()
            {
                if (R_Crtn_Reconnect != null) StopCoroutine(R_Crtn_Reconnect);
                R_Crtn_Reconnect = StartCoroutine(ReconnectAttemptRoutine());
            }
            private IEnumerator ReconnectAttemptRoutine()
            {
                // Espera corta: deja que el peer termine de cerrarse antes de reintentar //
                    yield return new WaitForSecondsRealtime(R_Ctm_RH_Reconnection.RetryDelay);
                    R_Crtn_Reconnect = null;
                // Si Photon no acepto el pedido, se agenda otro (el handler corta al llegar al maximo) //
                    if (!R_Ctm_RH_Reconnection.TryNextAttempt() && R_Ctm_RH_Reconnection.IsReconnecting)
                        ScheduleReconnectAttempt();
            }
            private void ScheduleRejoinFromMaster()
            {
                if (R_Crtn_Reconnect != null) StopCoroutine(R_Crtn_Reconnect);
                R_Crtn_Reconnect = StartCoroutine(RejoinFromMasterRoutine());
            }
            private IEnumerator RejoinFromMasterRoutine()
            {
                // Espera para darle tiempo al servidor a marcarnos como inactivos (evita otro "ActiveJoiner") //
                    yield return new WaitForSecondsRealtime(R_Ctm_RH_Reconnection.RetryDelay);
                    R_Crtn_Reconnect = null;
                    R_Ctm_RH_Reconnection.TryConsumePendingRejoin();
            }
            private void HandleReconnectionAttempt(int Int_Attempt, int Int_Max)
            {
                ConnectionStatusOverlay.Show(ConnectionStrings.LostTitle, string.Format(ConnectionStrings.LostBodyFormat, Int_Attempt, Int_Max));
            }
            private void HandleReconnectionSuccess()
            {
                ConnectionStatusOverlay.Hide();
                if (IsInMatchScene) MasterManager.Instance.GameManager.OnLocalPlayerRejoined();
            }
            private void HandleReconnectionFailed()
            {
                if (R_Crtn_Reconnect != null) { StopCoroutine(R_Crtn_Reconnect); R_Crtn_Reconnect = null; }
                GoToTitle(ConnectionStrings.ReconnectFailed);
            }
        #endregion Reconnection Methods
        #region    Custom Methods
            public void StartGame()
            {
                // Estado de partida limpio (pausa / ganador) antes de cargar el nivel //
                    PhotonNetwork.CurrentRoom.SetCustomProperties(MatchConnectionMonitor.BuildResetProperties());
                // PUN //
                    PhotonNetwork.LoadLevel(MATCH_SCENE_INDEX);
                // Lock Room Mid Game //
                    PhotonNetwork.CurrentRoom.IsOpen = false;
            }
            public void EndGame()
            {
                // PUN //
                    PhotonNetwork.LoadLevel(MENU_SCENE_INDEX);
                // Lock Room Mid Game //
                    PhotonNetwork.CurrentRoom.IsOpen = true;
            }
            public void LeaveGame()
            {
                Debug.Log("[Network Manager] Status: 'Leaving Game'");
                // Devolver el control del mouse antes de volver al menu //
                    MasterManager.Instance.GameManager.ShowCursor();
                    MasterManager.Instance.InputManager.DisableInputPlayerActions();
                // Limpiar HUD / Scoreboard / lista de la sala - Tiene que ser ANTES de salir, usa un RPC //
                    if (PhotonNetwork.InRoom) MasterManager.Instance.GameManager.LeftRoom();
                // Salir - becomeInactive = false: con PlayerTtl, sino el lugar queda reservado 30 s //
                    R_Ctm_RH_Reconnection.MarkIntentionalLeave();
                    if (PhotonNetwork.InRoom) PhotonNetwork.LeaveRoom(false);
                    else                      ReturnToTitle();
            }
            private void ReturnToTitle()
            {
                // Sacar las pantallas superpuestas si quedaron abiertas //
                    GameResultScreen.Hide();
                    ConnectionStatusOverlay.Hide();
                // Volver a la escena de menu //
                    if (SceneManager.GetActiveScene().buildIndex != MENU_SCENE_INDEX)
                        SceneManager.LoadScene(MENU_SCENE_INDEX);
                // Menu Manager Communication //
                    MasterManager.Instance.MenuManager.OpenMenu("TitleMenu");
            }
            // Vuelve al titulo mostrando un error. Si no hay conexion, el error queda visible y se reintenta conectar cada tanto //
            private void GoToTitle(string Str_Error)
            {
                // Estado local //
                    MasterManager.Instance.GameManager.ShowCursor();
                    MasterManager.Instance.InputManager.DisableInputPlayerActions();
                // IsStatusOn queda en true: el OnStartUp de la escena de menu no tiene que reconectar por su cuenta //
                    O_Bool_IsStatusOn = true;
                    ReturnToTitle();
                // Seguimos conectados al Master: alcanza con volver al menu //
                    if (PhotonNetwork.IsConnectedAndReady)
                    {
                        if (!PhotonNetwork.InLobby) PhotonNetwork.JoinLobby();
                        ShowError(Str_Error);
                        return;
                    }
                // Sin conexion: error visible + reintento con espera (antes era un bucle inmediato de reconexion) //
                    StopConnectRetry();
                    R_Crtn_ConnectRetry = StartCoroutine(ConnectRetryRoutine(Str_Error));
            }
            private IEnumerator ConnectRetryRoutine(string Str_Error)
            {
                // Esperar a que la escena de menu este cargada para que el error no quede tapado //
                    while (SceneManager.GetActiveScene().buildIndex != MENU_SCENE_INDEX) yield return null;
                    yield return null;
                    ShowError(Str_Error);
                // Reintentar hasta llegar al Master (OnConnectedToMaster corta esta rutina) //
                    WaitForSecondsRealtime Ctm_WFS_Delay = new WaitForSecondsRealtime(_connectRetryDelay);
                    while (!PhotonNetwork.IsConnected)
                    {
                        yield return Ctm_WFS_Delay;
                        if (PhotonNetwork.IsConnected) break;
                        Debug.Log("[Network Manager] Status: 'Retrying Connection'");
                        PhotonNetwork.ConnectUsingSettings();
                        PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = _disconnectTimeoutMs;
                    }
                    R_Crtn_ConnectRetry = null;
            }
            private void StopConnectRetry()
            {
                if (R_Crtn_ConnectRetry == null) return;
                StopCoroutine(R_Crtn_ConnectRetry);
                R_Crtn_ConnectRetry = null;
            }
            private bool IsErrorMenuOpen()
            {
                BaseMenuType Ctm_BMT_Current = MasterManager.Instance.MenuManager.CurrentMenu;
                return Ctm_BMT_Current != null && Ctm_BMT_Current.MenuName == ERROR_MENU_NAME;
            }
            // Acciones de sala sin conexion: en vez de quedar trabado en "Cargando", se explica el problema //
            private bool EnsureConnectedForRoomAction()
            {
                if (PhotonNetwork.IsConnectedAndReady) return true;
                ShowError(string.Format(ErrorStrings.CannotConnectFormat, PhotonNetwork.NetworkClientState));
                return false;
            }
            private void ShowError(string Str_Message)
            {
                MasterManager.Instance.MenuManager.OpenMenu(ERROR_MENU_NAME);
                BaseMenuType Ctm_BMT_Error = MasterManager.Instance.MenuManager.GetMenuReference(ERROR_MENU_NAME);
                if (Ctm_BMT_Error != null) Ctm_BMT_Error.GetComponent<MenuTypeError>().PrintError(Str_Message);
            }
            public void JoinRoom(RoomInfo PUNRI_Input)
            {
                // Error Exit //
                    if (!EnsureConnectedForRoomAction()) return;
                // Variables //
                    MenuTypeFindRoom Ctm_MTFR_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("FindRoomMenu")).gameObject.GetComponent<MenuTypeFindRoom>();
                    MenuTypeWaitingRoom Ctm_MTWR_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("WaitingRoom")).gameObject.GetComponent<MenuTypeWaitingRoom>();
                // Menu Manager - Clear List //
                    Ctm_MTFR_Reference.ClearRoomList();
                // Join //
                    PhotonNetwork.JoinRoom(PUNRI_Input.Name);
                // Menu Manager Communication //
                    // Change to Loading //
                        MasterManager.Instance.MenuManager.OpenMenu("LoadingMenu");
                // Game Manager Communication // 
                    MasterManager.Instance.GameManager.LeftRoom();
                Debug.Log("[Network Manager] Status: 'Joining Room'");
            }
            public void CreateRoom()
            {
                // Variables //
                    MenuTypeCreateRoom Ctm_MTCR_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("CreateRoomMenu")).gameObject.GetComponent<MenuTypeCreateRoom>();
                    MenuTypeWaitingRoom Ctm_MTWR_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("WaitingRoom")).gameObject.GetComponent<MenuTypeWaitingRoom>();
                    Ctm_MTCR_Reference.UpdateRoomData();
                    RoomOptions PUNRO_Settings = new RoomOptions
                    {
                        MaxPlayers = ROOM_MAX_PLAYERS,        // GDD: siempre 2 (el campo RoomSize del menu ya no aplica) //
                        IsVisible = true,
                        IsOpen = true,
                        PlayerTtl = PLAYER_TTL_MS,            // Seamless reconnection: el jugador queda "inactivo" y puede volver //
                        EmptyRoomTtl = PLAYER_TTL_MS,         // Si se caen todos, la sala sobrevive para que puedan volver //
                        CleanupCacheOnLeave = true,
                        CustomRoomProperties = MatchConnectionMonitor.BuildResetProperties()
                    };
                // Error Exit - Antes salia en silencio: ahora se avisa al jugador //
                    if (string.IsNullOrWhiteSpace(Ctm_MTCR_Reference.RoomName))
                    {
                        ShowError(ErrorStrings.EmptyRoomName);
                        return;
                    }
                    if (!EnsureConnectedForRoomAction()) return;
                // Map Manager Communication - Reset Bounds //
                    MasterManager.Instance.MapManager.SetBounds(Ctm_MTCR_Reference.MapWidth, Ctm_MTCR_Reference.MapHeight);
                // Create Room //
                    PhotonNetwork.CreateRoom(Ctm_MTCR_Reference.RoomName, PUNRO_Settings); 
                    Ctm_MTWR_Reference.SetUpRoom();
                // Menu Manager Communication - Change to Loading //
                    MasterManager.Instance.MenuManager.OpenMenu("LoadingMenu");
                Debug.Log("[Network Manager] Status: 'Creating Room'");
            }
            public void LeaveRoom()
            {
                // Variables //
                    MenuTypeFindRoom Ctm_MTFR_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("FindRoomMenu")).gameObject.GetComponent<MenuTypeFindRoom>();
                    MenuTypeWaitingRoom Ctm_MTWR_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("WaitingRoom")).gameObject.GetComponent<MenuTypeWaitingRoom>();
                // Game Manager Communication // 
                    // Leave Room //
                        MasterManager.Instance.GameManager.LeftRoom();
                // Menu Manager Communication // 
                    // Update GUI //
                        Ctm_MTWR_Reference.SetUpRoom();
                    // Change to Loading //
                        MasterManager.Instance.MenuManager.OpenMenu("LoadingMenu");
                // Menu Manager Communication // 
                    // Remove Room //
                        Ctm_MTFR_Reference.ClearRoomList();
                        Ctm_MTWR_Reference.ClearPlayerList();
                // Network Manager - Leave (becomeInactive = false: salida definitiva) //
                    R_Ctm_RH_Reconnection.MarkIntentionalLeave();
                    PhotonNetwork.LeaveRoom(false);
                // Menu Manager Communication // 
                    // Clear All //
                        Ctm_MTFR_Reference.RemoveRoomFromList();
                        Ctm_MTFR_Reference.ClearRoomList();
                        Ctm_MTWR_Reference.ClearPlayerList();
                Debug.Log("[Network Manager] Status: 'Leaving Room'");
            }
        #endregion Custom Methods
    #endregion Methods
}
