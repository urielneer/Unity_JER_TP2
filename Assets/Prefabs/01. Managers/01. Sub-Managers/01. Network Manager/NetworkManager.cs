using UnityEngine;
using System.Collections.Generic;
using static CustomExtension.ArrayExtensions;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

#region    ...
#endregion ...

public class NetworkManager : BaseManager<NetworkManager>
{
    #region    Variables
        #region    Menu
            
        #endregion Menu
        #region    Manager-To-Manager Data
            private bool O_Bool_IsStatusOn = false;
            public bool IsStatusOn => O_Bool_IsStatusOn;
        #endregion Manager-To-Manager Data
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Singleton Operations //
                    base.Awake();
                // Variables //
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
                        PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 2000;
                        O_Bool_IsStatusOn = true;
                        Debug.Log("[Network Manager] Status: 'On'");
                }
            }
        #endregion Override Methods
        #region    PUN Methods
            #region    Title Room
                public override void OnConnectedToMaster()
                {
                    // PUN //
                        PhotonNetwork.JoinLobby();
                        PhotonNetwork.AutomaticallySyncScene = true;
                    // Menu Manager Communication - Change to Loading //
                        MasterManager.Instance.MenuManager.OpenMenu("TitleMenu");
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
                    // Menu Manager Communication //
                        // Variables //
                            MenuTypeWaitingRoom Ctm_MTWR_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("WaitingRoom")).gameObject.GetComponent<MenuTypeWaitingRoom>();
                        // Update Room Name //
                            Ctm_MTWR_Reference.SetUpRoom();
                            Ctm_MTWR_Reference.JoinRoom();
                    Debug.Log("[Network Manager] Status: 'Joined room'");
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
            #region    Waiting Room
                public override void OnPlayerEnteredRoom(Player PUNPl_NewPlayer)
                {
                    Debug.Log("[Network Manager] Status: 'Player Entered Room'");
                }
                public override void OnPlayerLeftRoom(Player PUNPl_OtherPlayer) // ANYONE LEAVES THE ROOM //
                {
                    // Variables //
                        MenuTypeWaitingRoom Ctm_MTWR_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("WaitingRoom")).gameObject.GetComponent<MenuTypeWaitingRoom>();
                    // Menu Manager Communication //
                        // Update Room //
                            Ctm_MTWR_Reference.SetUpRoom();
                    // Si se fue el Vigilante, elegir otro y reiniciar //
                        ReassignVigilantIfNeeded(PUNPl_OtherPlayer);
                    Debug.Log("[Network Manager] Status: 'Player Left Room'");
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
                    // Publicar nuevo Vigilante //
                        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "VigilantActor", PUNPl_New.ActorNumber } });
                        Debug.Log("[Network Manager] Vigilante se fue. Nuevo Vigilante: Actor " + PUNPl_New.ActorNumber + ". Reiniciando partida.");
                    // Reiniciar la ronda para todos //
                        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 1)
                            PhotonNetwork.LoadLevel(1);
                }
                public override void OnLeftRoom() // CURRENT PLAYER LEAVES THE ROOM //
                {
                    Debug.Log("[Network Manager] Status: 'Left Room'");
                    // Volver al menu principal //
                        MasterManager.Instance.GameManager.ShowCursor();
                        ReturnToTitle();
                }
            #endregion Waiting Room
            #region    Error Room
                public override void OnCreateRoomFailed(short Shrt_ReturnCode, string Str_Message)
                {
                    // Menu Manager Communication - Change to Loading //
                        MasterManager.Instance.MenuManager.OpenMenu("ErrorMenu"); 
                        (MasterManager.Instance.MenuManager.GetMenuReference("CreateRoomMenu")).gameObject.GetComponent<MenuTypeError>().PrintError(Str_Message);
                    Debug.Log("[Network Manager] Status: 'ERROR FOUND!: "+Str_Message+"'");
                }
            #endregion Error Room
        #endregion PUN Methods
        #region    Custom Methods
            public void StartGame()
            {
                // PUN //
                    PhotonNetwork.LoadLevel(1);
                // Lock Room Mid Game //
                    PhotonNetwork.CurrentRoom.IsOpen = false;
            }
            public void EndGame()
            {
                // PUN //
                    PhotonNetwork.LoadLevel(0);
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
                // Salir - El regreso al menu lo hace OnLeftRoom() //
                    if (PhotonNetwork.InRoom) PhotonNetwork.LeaveRoom();
                    else                      ReturnToTitle();
            }
            private void ReturnToTitle()
            {
                // Sacar la pantalla de resultado si quedo abierta //
                    GameResultScreen.Hide();
                // Volver a la escena de menu //
                    if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != 0)
                        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                // Menu Manager Communication //
                    MasterManager.Instance.MenuManager.OpenMenu("TitleMenu");
            }
            public void JoinRoom(RoomInfo PUNRI_Input)
            {
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
                        MaxPlayers = Ctm_MTCR_Reference.RoomSize,
                        IsVisible = true,
                        IsOpen = true,
                        EmptyRoomTtl = 0,
                        CleanupCacheOnLeave = true
                    };
                // Error Exit //
                    if (string.IsNullOrEmpty(Ctm_MTCR_Reference.RoomName))
                    {
                        return;
                    }
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
                // Network anager - Leave //
                    PhotonNetwork.LeaveRoom();
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
