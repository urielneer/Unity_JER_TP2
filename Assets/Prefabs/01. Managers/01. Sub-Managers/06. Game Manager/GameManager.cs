using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using static CustomExtension.ArrayExtensions;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

#region    "using" Script Clarification
    using Hashtable = ExitGames.Client.Photon.Hashtable;
#endregion "using" Script Clarification

public class GameManager : BaseManager<GameManager>
{
    #region    Variables
        [Header(" Manager Settings")]
            [SerializeField] private float _respawnCooldown;
            float IO_Flt_RespawnCooldown;
            public float RespawnCooldown => IO_Flt_RespawnCooldown;
        #region    Dummy[Header("UI Settings")]
            [SerializeField] private TextMeshProUGUI _timerText;
            [SerializeField] private Image _image;

            [Header("Timer State")]
                [SerializeField] private float _elapsedTime = 0f;
                private int _currentSeconds = 0;

            [Header("Shooting State")]
                [SerializeField] private bool O_Bool_CanShoot = false;
            public bool Bool_CanShoot => O_Bool_CanShoot;
        #endregion Dummy
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Singleton Operations //
                    base.Awake();
                // Variables //
                    IO_Flt_RespawnCooldown = _respawnCooldown;
            }
        #endregion Unity Methods
        #region    Override Methods
            #pragma warning disable CS1998
            public async override void OnStartUp()
            #pragma warning restore CS1998
            {
                // Resume Base //
                    base.OnStartUp();
                // Scene dependant Behaviour //
                    switch (SceneManager.GetActiveScene().buildIndex) 
                    {
                        case 0:
                        break;
                        case 1: // Level //
                                // Spawn Player //
                            MasterManager.Instance.MenuManager.OpenMenu("HUD");
                            _elapsedTime = 0f;
                            _currentSeconds = 0;
                            O_Bool_CanShoot = false;
                            _image.color = Color.yellow;
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
                            MasterManager.Instance.InputManager.DisableInputPlayerActions();
                        break;
                        case 1: // Level //   
                            TimerUpdate();
                        break;
                    }
            }
        #endregion Override Methods
        #region    Custom Methods
            #region    Dummy
                #region    Regular
                    private void TimerUpdate()
                    {
                        // Accumulate frame delta time
                        _elapsedTime += Time.deltaTime;

                        // Convert to integer seconds
                        int newSeconds = Mathf.FloorToInt(_elapsedTime);

                        // Reset counter when reaching 100 seconds
                        if (newSeconds >= 100)
                        {
                            _elapsedTime = 0f;
                            newSeconds = 0;
                        }

                        // Only update logic and UI when a full second elapses
                        if (newSeconds != _currentSeconds)
                        {
                            _currentSeconds = newSeconds;
                            UpdateTimerLogic();
                        }
                    }
                    int Int_LastCurrent = 0;
                    private void UpdateTimerLogic()
                    {
                        // Format as 3 digits with leading zeros (e.g., 000, 003, 012)
                        if (_timerText != null)
                        {
                            _timerText.text = _currentSeconds.ToString("D3");
                        }

                        // Bool is true when seconds is a non-zero multiple of 3 (or include 0 depending on logic)
                        O_Bool_CanShoot = (_currentSeconds % 3 == 0);
                        if (_currentSeconds % 3 == 0)
                            { Int_LastCurrent = _currentSeconds; }
                        if (_currentSeconds == Int_LastCurrent + 1)
                            { CompareTimeframes(); }
                        if (_currentSeconds == Int_LastCurrent + 2)
                            { _image.color = Color.yellow; }     
                    }
                    string Time_KEY = "Time_KEY";
                    string Ticks_KEY = "Ticks_KEY";
                    string UserID_KEY = "UserID_KEY";
                    Hashtable[] Shots = {new Hashtable(), new Hashtable()}; // Master / Client //
                    private void CompareTimeframes()
                    { 
                        if (PhotonNetwork.IsMasterClient)
                        {
                            GetComponent<PhotonView>().RPC(nameof(RPC_AlertWinnerToAll),RpcTarget.All, 
                                                                                                        ((Shots[0].Count == 0) && (Shots[1].Count == 0)) ? 
                                                                                                            0 :                 // TIE //
                                                                                                            ((Shots[0].Count != 0) && (Shots[1].Count == 0)) ?
                                                                                                                1 :             // MASTER //
                                                                                                                ((Shots[0].Count == 0) && (Shots[1].Count != 0)) ?
                                                                                                                    2 :         // CLIENT //
                                                                                                                    ((double)Shots[0][Time_KEY] == (double)Shots[1][Time_KEY]) ?
                                                                                                                        0 :     // TIE //
                                                                                                                        ((double)Shots[0][Time_KEY] < (double)Shots[1][Time_KEY]) ?
                                                                                                                            1 : // MASTER //
                                                                                                                            2   // CLIENT //
                                                                                                     );
                        }
                    }
                    public void ShootDummy()
                    {
                        if (O_Bool_CanShoot)
                        {
                            Hashtable Hsh_Keys = new Hashtable
                            {
                                { UserID_KEY, PhotonNetwork.LocalPlayer.ActorNumber },
                                { Time_KEY  , (double) PhotonNetwork.Time                  },
                                { Ticks_KEY , (long) DateTime.UtcNow.Ticks                 }
                            }; 
                            GetComponent<PhotonView>().RPC(nameof(RPC_AlertShootToMaster),RpcTarget.MasterClient, Hsh_Keys);
                            _image.color = Color.blue;
                        }
                    }
    
                    private System.Collections.IEnumerator StartShootout()
                    {
                        yield return new WaitForSeconds(1f);
                            CompareTimeframes();
                        yield return new WaitForSeconds(1f);
                            _image.color = Color.yellow;     
                    }
                #endregion Regular
                #region    RPC
                    [PunRPC]
                    private void RPC_AlertShootToMaster(Hashtable Hsh_Input)
                    {
                        int Int_ID = (int) Hsh_Input[UserID_KEY];
                        if (Int_ID == PhotonNetwork.LocalPlayer.ActorNumber) // MASTER //
                        { if (Shots[0].Count == 0) Shots[0] = Hsh_Input; }
                        else                                                 // CLIENT //
                        { if (Shots[1].Count == 0) Shots[1] = Hsh_Input; }                            
                    }
                    [PunRPC]
                    private void RPC_AlertWinnerToAll(int Int_ID)
                    {
                        O_Bool_CanShoot = false;
                        if (PhotonNetwork.IsMasterClient)
                            Shots = new Hashtable[]{new Hashtable(), new Hashtable()};
                        switch (Int_ID)
                        {
                            case 0:
                                _image.color = Color.white;  
                            return;
                            case 1:
                                if (PhotonNetwork.IsMasterClient)
                                    _image.color = Color.green;      
                                else
                                    _image.color = Color.red;      
                            return;
                            case 2:
                                if (!PhotonNetwork.IsMasterClient)
                                    _image.color = Color.green;      
                                else
                                    _image.color = Color.red; 
                            return;
                        }
                    }
                #endregion RPC
            #endregion Dummy
            #region    Networking
                public void LeftRoom()
                {
                    // Variables //
                        MenuTypeHUD Ctm_MTH_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("HUD")).gameObject.GetComponent<MenuTypeHUD>();
                        MenuTypeScoreBoard Ctm_MTSB_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("Scoreboard")).gameObject.GetComponent<MenuTypeScoreBoard>();
                        MenuTypeWaitingRoom Ctm_MTWR_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("WaitingRoom")).gameObject.GetComponent<MenuTypeWaitingRoom>();
                    // Menu Manager Communication // 
                        Ctm_MTWR_Reference.LeftRoom(PhotonNetwork.LocalPlayer.NickName);
                        Ctm_MTH_Reference.LeftRoom();
                        Ctm_MTSB_Reference.LeftRoom();
                }
                public void JoinRoom()
                {
                    // Variables //
                        MenuTypeHUD Ctm_MTH_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("HUD")).gameObject.GetComponent<MenuTypeHUD>();
                        MenuTypeScoreBoard Ctm_MTSB_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("Scoreboard")).gameObject.GetComponent<MenuTypeScoreBoard>();
                        MenuTypeWaitingRoom Ctm_MTWR_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("WaitingRoom")).gameObject.GetComponent<MenuTypeWaitingRoom>();
                    // Menu Manager Communication // 
                        Ctm_MTWR_Reference.JoinRoom();
                        Ctm_MTH_Reference.JoinRoom();
                        Ctm_MTSB_Reference.JoinRoom();
                }
                public void GameStart()
                {
                    // Input Manager Communication // 
                        MasterManager.Instance.InputManager.OnStartUp();
                    // Variables //
                        MenuTypeHUD Ctm_MTH_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("HUD")).gameObject.GetComponent<MenuTypeHUD>();
                        MenuTypeScoreBoard Ctm_MTSB_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("Scoreboard")).gameObject.GetComponent<MenuTypeScoreBoard>();
                    // Menu Manager Communication // 
                        Ctm_MTH_Reference.GameStart();
                        Ctm_MTSB_Reference.GameStart();
                        OnStartUp();
                    MasterManager.Instance.InputManager.EnableInputPlayerActions();
                    Debug.Log("[Game Manager] Status: 'Game Started'");
                }
                public void GameEnd()
                {
                    // Variables //
                        MenuTypeHUD Ctm_MTH_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("HUD")).gameObject.GetComponent<MenuTypeHUD>();
                        MenuTypeScoreBoard Ctm_MTSB_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("Scoreboard")).gameObject.GetComponent<MenuTypeScoreBoard>();
                    // Menu Manager Communication // 
                        Ctm_MTH_Reference.GameEnd();
                        Ctm_MTSB_Reference.GameEnd();
                    MasterManager.Instance.InputManager.DisableInputPlayerActions();
                    Debug.Log("[Game Manager] Status: 'Game Over'");
                }
                public void ShowCursor()
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                public void HideCursor()
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            #endregion Networking
            #region    PowerUp
            #endregion PowerUp
        #endregion Custom Methods
    #endregion Methods
}
