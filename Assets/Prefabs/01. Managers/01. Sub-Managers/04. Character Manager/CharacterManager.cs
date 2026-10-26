using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using static CustomExtension.ArrayExtensions;


#region    "using" Script Clarification
    using Hashtable = ExitGames.Client.Photon.Hashtable;
#endregion "using" Script Clarification
#region    Enums
#endregion Enums
public class CharacterManager : BaseManager<CharacterManager>
{
    #region    Variables
        [Header(" All Character Settings")]
            [SerializeField] float _maxHealth;
            float IO_Flt_MaxHealth;
            public float MaxHealth => IO_Flt_MaxHealth;
        [Header(" Manager Settings")]
            [SerializeField] private float _shootCooldown;
            float IO_Flt_ShootCooldown;
            public float ShootCooldown => IO_Flt_ShootCooldown;
            [SerializeField] BasePlayer _ownPlayer;
            BasePlayer O_Ctm_BP_OwnPlayer;
            public BasePlayer OwnPlayer => O_Ctm_BP_OwnPlayer; 
            BasePlayer[] O_Ctm_BP_A1_PlayerList;
        #region    Hashtables (Communication among instances)
            private const string PlayerName_KEY = "PlayerName";
            private const string PlayerMatIndex_KEY = "PlayerrMatIndex";
            private const string PlayerType_KEY = "PlayerType";
            private const string PlayerID_KEY = "PlayerID";
            private const string PlayerList_KEY = "PlayerList";
        #endregion Hashtables (Communication among instances)
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Singleton Operations //
                    base.Awake();
                // Variables //
                    IO_Flt_MaxHealth = _maxHealth;
                    IO_Flt_ShootCooldown = _shootCooldown;

                    O_Ctm_BP_OwnPlayer = _ownPlayer;
            }
        #endregion Unity Methods
        #region    Override Methods
            #pragma warning disable CS1998
            public async override void OnStartUp()
            #pragma warning restore CS1998
            {
                // Resume Base //
                    base.OnStartUp();
                // Variables //
                    O_Ctm_BP_A1_PlayerList = new BasePlayer[0];
                // Scene dependant Behaviour //
                    switch (SceneManager.GetActiveScene().buildIndex) 
                    {
                        case 0:
                        break;
                        case 1: // Level //
                            // Variables //
                        break;
                    }
            }
            public override void OnUpdate(float Flt_FixedDT)
            {
                // Resume Base //
                    base.OnUpdate(Flt_FixedDT);
                // Scene dependant Behaviour //
                    switch (SceneManager.GetActiveScene().buildIndex) 
                    {
                        case 0:
                        break;
                        case 1: // Level //
                            if (O_Ctm_BP_A1_PlayerList.Length == 0) return;
//-//                            foreach (BasePlayer Ctm_BP_Player in O_Ctm_BP_A1_PlayerList) 
//-//                                Ctm_BP_Player.OnUpdate(Flt_FixedDT);
                        break;
                    }
            }
            public override void OnFixedUpdate(float Flt_FixedDT)
            {
                // Resume Base //
                    base.OnFixedUpdate(Flt_FixedDT);
                // Scene dependant Behaviour //
                    switch (SceneManager.GetActiveScene().buildIndex) 
                    {
                        case 0:
                        break;
                        case 1: // Level //
                            if (O_Ctm_BP_A1_PlayerList.Length == 0) return;
//-//                            foreach (BasePlayer Ctm_BP_Player in O_Ctm_BP_A1_PlayerList) 
//-//                                Ctm_BP_Player.OnFixedUpdate(Flt_FixedDT);
                break;
                    }
            }
        #endregion Override Methods
        #region    Custom Methods
            private MenuTypeHUD GetHUD()
            {
                BaseMenuType Ctm_BMT_HUD = MasterManager.Instance.MenuManager.GetMenuReference("HUD");
                if (Ctm_BMT_HUD == null) return null;
                return Ctm_BMT_HUD.gameObject.GetComponent<MenuTypeHUD>();
            }
            private System.Collections.IEnumerator ShowRoleRoutine()
            {
                // Esperar a que el HUD este abierto - Arranca con el LoadingMenu adelante //
                    float Flt_Timeout = 10f;
                    MenuTypeHUD Ctm_MTH_HUD = GetHUD();
                    while (Flt_Timeout > 0f && (Ctm_MTH_HUD == null || !Ctm_MTH_HUD.gameObject.activeInHierarchy))
                    {
                        Flt_Timeout -= Time.deltaTime;
                        yield return null;
                        Ctm_MTH_HUD = GetHUD();
                    }
                    if (Ctm_MTH_HUD == null) yield break;
                // Mostrar //
                    yield return new WaitForSeconds(2f);
            }
            public Hashtable AddToPlayerList(BasePlayer Ctm_BP_Client)
            {
                AddNewToArray(ref O_Ctm_BP_A1_PlayerList, Ctm_BP_Client);
                #region    Synchronize
                    int[] Int_A1_IDList = new int[0];
                    foreach (BasePlayer Ctm_BP_Element in O_Ctm_BP_A1_PlayerList) 
                        AddNewToArray(ref Int_A1_IDList, Ctm_BP_Element.GetComponent<PhotonView>().ViewID);
                    Hashtable Hsh_MapKeys = new Hashtable
                    {
                        { PlayerList_KEY, Int_A1_IDList },
                    }; 
                    return Hsh_MapKeys;
                #endregion Synchronize
            }
            public void OverwritePlayerList(BasePlayer[] Ctm_BP_A1_NewList)
            {
                O_Ctm_BP_A1_PlayerList = Ctm_BP_A1_NewList;
            }
        #endregion Custom Methods
        #region    PUN         
            #region    Hastable
                // Character //
                    private bool IsPlayerIDHashtable(Hashtable Hsh_Input)
                    {
                        if ( Hsh_Input.ContainsKey(PlayerID_KEY) )
                        { 
                            return true; 
                        }
                        else
                        { 
                            UnityEngine.Debug.LogError("\"MapContiner.cs\"'s \"IsPlayerIDHashtable()\" returned FALSE, review Hastables");
                            return false; 
                        }
                    }
                    private bool IsPlayerListHashtable(Hashtable Hsh_Input)
                    {
                        if ( Hsh_Input.ContainsKey(PlayerList_KEY) )
                        { 
                            return true; 
                        }
                        else
                        { 
                            UnityEngine.Debug.LogError("\"MapContiner.cs\"'s \"IsPlayerListHashtable()\" returned FALSE, review Hastables");
                            return false; 
                        }
                    }
            #endregion Hastable  
            #region    RPC
                #region    Regular
                    public void UpdateAllCharacterManagers(Hashtable Hsh_Input)
                    {
                        GetComponent<PhotonView>().RPC(nameof(RPC_SpawnCharacter), RpcTarget.All, Hsh_Input); 
                    }
                    public void ChangePlayerMaterial(BasePlayer Ctm_BP_Input = null, int Int_Input = -1)
                    {
                        if (Ctm_BP_Input == null)
                            Ctm_BP_Input = O_Ctm_BP_OwnPlayer;
                        Hashtable Hsh_MapKeys = new Hashtable
                        {
                            { PlayerID_KEY, Ctm_BP_Input.GetComponent<PhotonView>().ViewID },
                            { PlayerMatIndex_KEY, Int_Input }
                        }; 
                        GetComponent<PhotonView>().RPC(nameof(RPC_CharacterMaterial), RpcTarget.All, Hsh_MapKeys); 
                    }
                #endregion Regular
                #region    Hashtables
                    // Character //
                        [PunRPC]
                        private void RPC_CharacterMaterial(Hashtable Hsh_Input)
                        {
                            // Variables //
                                BasePlayer Ctm_BP_Client = (PhotonView.Find((int)Hsh_Input[PlayerID_KEY])).GetComponent<BasePlayer>();
                            // Set Up //
//-//                                Ctm_BP_Client.SetMaterial((int)Hsh_Input[PlayerMatIndex_KEY]); 
                        }
                        [PunRPC]
                        private void RPC_SpawnCharacter(Hashtable Hsh_Input)
                        {
                            // Variables //
                                BasePlayer Ctm_BP_Client = (PhotonView.Find((int)Hsh_Input[PlayerID_KEY])).GetComponent<BasePlayer>();
                            // Set Up //
//-//                                Ctm_BP_Client.SetData((string)Hsh_Input[PlayerName_KEY], (int)Hsh_Input[PlayerID_KEY], IO_Flt_MaxHealth, (PlayerType)Hsh_Input[PlayerType_KEY]); 
//-//                                Ctm_BP_Client.SetVisibility(true);
                                ChangePlayerMaterial(Ctm_BP_Client);
                                PhotonNetwork.SendAllOutgoingCommands();
                            // Force Update Other Client's Room Prefab //
                                GetComponent<PhotonView>().RPC(nameof(RPC_AddCharacterToMaster),RpcTarget.MasterClient, Hsh_Input);
                        }
                        [PunRPC]
                        private void RPC_AddCharacterToMaster(Hashtable Hsh_Input)
                        {
                            if (PhotonNetwork.IsMasterClient)
                            {
                                if (IsPlayerIDHashtable(Hsh_Input))
                                {
                                    // Variables //
                                        BasePlayer Ctm_BP_Client = (PhotonView.Find((int)Hsh_Input[PlayerID_KEY])).GetComponent<BasePlayer>();
                                    // Set Up //
                                        Hashtable Hsh_NewList = MasterManager.Instance.CharacterManager.AddToPlayerList(Ctm_BP_Client);
                                    // Force Update Other Client's Room Prefab //
                                        GetComponent<PhotonView>().RPC(nameof(RPC_ForceCharacterUpdateToClients),RpcTarget.All, Hsh_NewList);
                                }
                            }
                        }
                        [PunRPC]
                        private void RPC_ForceCharacterUpdateToClients(Hashtable Hsh_Input)
                        {
                            if (IsPlayerListHashtable(Hsh_Input))
                            {
                                if (!PhotonNetwork.IsMasterClient)
                                {
                                    // Variables //
                                        BasePlayer[] Ctm_BP_A1_NewList = new BasePlayer[0];
                                        foreach (int Int_ID in ((int[])Hsh_Input[PlayerList_KEY])) 
                                            AddNewToArray(ref Ctm_BP_A1_NewList, (PhotonView.Find(Int_ID)).GetComponent<BasePlayer>());
                                    // Force Update //
                                        MasterManager.Instance.CharacterManager.OverwritePlayerList(Ctm_BP_A1_NewList);
                                }
                            }
                        }
                #endregion Hashtables 
            #endregion RPC
        #endregion PUN
    #endregion Methods
}
