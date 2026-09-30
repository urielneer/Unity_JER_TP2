using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using static CustomExtension.ArrayExtensions;

public class MenuTypeWaitingRoom : BaseMenuType
{
    #region    Variables
        [Header("Waiting Room GUI Settings")]
            [SerializeField] private BaseMenuLabel _name;
            private BaseMenuLabel Ctm_BML_Name;    
            [SerializeField] BaseMenuButton _startButton;
            protected static BaseMenuButton I_BMB_StartButton;
            [SerializeField] MenuListContainerPlayer _playerList;
            MenuListContainerPlayer I_Ctm_MLCP_Players;
            [SerializeField] GameObject _masterContainer;
            GameObject I_GObj_Master;
            private string[] R_Str_A1_PlayerNames = new string[1];
            private int Int_VigilantNumber = 0;
            public int VigilantNumber => Int_VigilantNumber;
        #region    Manager-To-Manager Data
        #endregion Manager-To-Manager Data
        #region    Hashtables (Communication among instances)
            private const string PlayerCount_KEY = "PlayerCount";
            private const string NewPlayer_KEY = "NewPlayer";
        #endregion Hashtables (Communication among instances)
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Resume Base //
                    base.Awake();
                // Variables //
                    I_Ctm_MLCP_Players = _playerList;
                    I_GObj_Master = _masterContainer;
                    Ctm_BML_Name = _name;
                    I_BMB_StartButton = _startButton;
            }
        #endregion Unity Methods
        #region    Custom Methods
            #region   Room Management Methods
                public void SetUpRoom()
                {
                    I_BMB_StartButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
                }
                public void ClearPlayerList()
                {
                    I_Ctm_MLCP_Players.CleanList();
                }
            #endregion Room Management Methods
            #region    Update Methods
                private string ChangeRoomName(RoomInfo PUNPl_Room)
                {
                    // Variables //
                        MenuTypeWaitingRoom Ctm_MTWR_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("WaitingRoom")).gameObject.GetComponent<MenuTypeWaitingRoom>();
                        string Str_Temp = "Room:\""+PUNPl_Room.Name+"\"\n("+(R_Str_A1_PlayerNames.Length).ToString("D2")+"/"+(PUNPl_Room.MaxPlayers).ToString("D2")+")";
                    // Overwrite // 
                        Ctm_BML_Name.Overwrite(Str_Temp);
                    // Change to Waitimg //
                        MasterManager.Instance.MenuManager.OpenMenu("WaitingRoom");
                    return Str_Temp;
                }
                public void UpdateRoom(RoomInfo PUNRI_Room = null)
                {
                    // Variables //
                        // Current Room is the default //
                            if (PUNRI_Room == null) PUNRI_Room = PhotonNetwork.CurrentRoom;
                    // Update Own Room Prefab //
                        ChangeRoomName(PUNRI_Room);
                        I_Ctm_MLCP_Players.SetList(R_Str_A1_PlayerNames.OfType<object>().ToList());             
                }
            #endregion Update Methods
            #region    Network Manager Specific Methods
                public void LeftRoom(string Str_Name) 
                { 
                    // Update //
                        GetComponent<PhotonView>().RPC(nameof(RPC_RemoveSelfFromMaster),RpcTarget.MasterClient, Str_Name);
                }
                public void JoinRoom(RoomInfo PUNRI_Room = null)
                {
    string Str_TempName = "Player " + PhotonNetwork.LocalPlayer.UserId.Substring(0, 5); // DELETEMELATER //
    PhotonNetwork.LocalPlayer.NickName = Str_TempName;                                  // DELETEMELATER //
                    // Variables //
                        // Current Room is the default //
                            if (PUNRI_Room == null) PUNRI_Room = PhotonNetwork.CurrentRoom;
                        // Update //
                            Str_TempName = PhotonNetwork.LocalPlayer.NickName;
                    // Master Client's StartUp //
                        if (PhotonNetwork.IsMasterClient)
                        {
                            // Values //
                                R_Str_A1_PlayerNames = new string[] { Str_TempName };
                            // Master Container //
                                I_Ctm_MLCP_Players.SetMasterContainer(I_GObj_Master);
                            // Update //
                                ChangeVigilant(PhotonNetwork.LocalPlayer.NickName);
                        }
                    // Client's SendData //
                        else
                        {   
                            // Master Container //
                                I_Ctm_MLCP_Players.SetMasterContainer(I_GObj_Master);
                            // Update //
                                GetComponent<PhotonView>().RPC(nameof(RPC_AddSelfToMaster),RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.NickName);
                                UpdateRoom();
                        }
                }
            #endregion Network Manager Specific Methods
        #endregion Custom Methods
        #region    PUN
            public void ChangeVigilant(string Str_Name)
            {
                foreach (Player Plyr in PhotonNetwork.PlayerList)
                {
                    if (Plyr.NickName == Str_Name)
                    {
                        GetComponent<PhotonView>().RPC(nameof(RPC_ForceUpdateVigilantNumber),RpcTarget.All, Plyr.ActorNumber);
                        // Room Property - Fuente de verdad del rol (sobrevive cambio de escena y menus inactivos) //
                            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "VigilantActor", Plyr.ActorNumber } });
                    }
                }
                GetComponent<PhotonView>().RPC(nameof(RPC_ForceUpdateListToEveryone),RpcTarget.All, new object[] { R_Str_A1_PlayerNames });
            }
            #region    RPC
                [PunRPC]
                private void RPC_AddSelfToMaster(string Str_Player)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        // Update //
                            AddNewToArray(ref R_Str_A1_PlayerNames, Str_Player);
                            UpdateRoom();
                        // Force Update Other Client's Room Prefab //
                            GetComponent<PhotonView>().RPC(nameof(RPC_ForceUpdateVigilantNumber),RpcTarget.All, Int_VigilantNumber);
                            GetComponent<PhotonView>().RPC(nameof(RPC_ForceUpdateListToEveryone),RpcTarget.All, new object[] { R_Str_A1_PlayerNames }); // �Por que object? PORQUE NO PARABA DE DAR FAIL Y ME VOLVIO LOCO!!!1! <3 //
                    }
                }
                [PunRPC]
                private void RPC_RemoveSelfFromMaster(string Str_Player)
                {
                    // Update //
                        RemoveSpecificFromArray(ref R_Str_A1_PlayerNames, Str_Player);
                        UpdateRoom();
                    // Force Update Other Client's Room Prefab //
                        GetComponent<PhotonView>().RPC(nameof(RPC_ForceUpdateVigilantNumber),RpcTarget.All, Int_VigilantNumber);
                        GetComponent<PhotonView>().RPC(nameof(RPC_ForceUpdateVigilantNumber),RpcTarget.All, Int_VigilantNumber);
                        GetComponent<PhotonView>().RPC(nameof(RPC_ForceUpdateListToEveryone),RpcTarget.All, new object[] { R_Str_A1_PlayerNames }); // �Por que object? PORQUE NO PARABA DE DAR FAIL Y ME VOLVIO LOCO!!!1! <3 //
                }
                [PunRPC]
                private void RPC_ForceUpdateListToClients(string[] Str_A1_PlayerList)
                {
                    if (!PhotonNetwork.IsMasterClient)
                    {
                        R_Str_A1_PlayerNames = Str_A1_PlayerList;
                        SetUpRoom();
                        UpdateRoom();
                    }
                }
                [PunRPC]
                private void RPC_ForceUpdateListToEveryone(string[] Str_A1_PlayerList)
                {
                    R_Str_A1_PlayerNames = Str_A1_PlayerList;
                    UpdateRoom();
                    SetUpRoom();
                }
                [PunRPC]
                private void RPC_ForceUpdateVigilantNumber(int Int_Input)
                {
                    Int_VigilantNumber = Int_Input;
                    if (PhotonNetwork.LocalPlayer.ActorNumber != Int_Input)
                        MasterManager.Instance.CharacterManager.Changetype(PlayerType.Chaser);
                    else
                        MasterManager.Instance.CharacterManager.Changetype(PlayerType.Vigilant);
                }
            #endregion RPC
        #endregion PUN
    #endregion Methods
}