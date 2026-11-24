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
        [Header(" Match Connection Settings")]
            [Tooltip("Segundos que se muestra el resultado antes de volver al menu cuando el rival abandona")]
            [SerializeField] private float _returnToMenuDelay = 4f;
            private MatchConnectionMonitor R_Ctm_MCM_Monitor;
            private Coroutine R_Crtn_ReturnToMenu;
        [Header(" Lag Compensation")]
            [Tooltip("Segundos (tiempo de servidor) que el Master espera despues de un disparo antes de resolver. Tiene que cubrir la latencia de ida del jugador mas lento")]
            [SerializeField] private float _hitResolutionWindow = 0.25f;
            [Tooltip("Maximo de segundos que se acepta 'retroceder' un disparo. Evita que un timestamp viejo o manipulado gane siempre")]
            [SerializeField] private float _maxShotRewind = 1f;
            private const double FUTURE_SHOT_TOLERANCE = 0.05d; // Un disparo no puede ser posterior a su propio envio //
            private ShotArbiter R_Ctm_SA_Arbiter;
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
                // Match Connection - Solo la instancia que sobrevive (singleton) //
                    if (_instance != this) return;
                    R_Ctm_MCM_Monitor = new MatchConnectionMonitor(NetworkManager.PLAYER_TTL_SECONDS);
                    R_Ctm_MCM_Monitor.OnPauseStarted += HandlePauseStarted;
                    R_Ctm_MCM_Monitor.OnPauseEnded   += HandlePauseEnded;
                    R_Ctm_MCM_Monitor.OnMatchEnded   += HandleMatchEnded;
                // Lag Compensation - El arbitro solo trabaja en el Master, pero existe en todos (puede cambiar el Master) //
                    R_Ctm_SA_Arbiter = new ShotArbiter(_hitResolutionWindow);
                    R_Ctm_SA_Arbiter.OnShotConfirmed += HandleShotConfirmed;
                    R_Ctm_SA_Arbiter.OnShotAnnulled  += HandleShotAnnulled;
            }
            private void OnDestroy()
            {
                if (R_Ctm_SA_Arbiter != null)
                {
                    R_Ctm_SA_Arbiter.OnShotConfirmed -= HandleShotConfirmed;
                    R_Ctm_SA_Arbiter.OnShotAnnulled  -= HandleShotAnnulled;
                }
                if (R_Ctm_MCM_Monitor == null) return;
                R_Ctm_MCM_Monitor.OnPauseStarted -= HandlePauseStarted;
                R_Ctm_MCM_Monitor.OnPauseEnded   -= HandlePauseEnded;
                R_Ctm_MCM_Monitor.OnMatchEnded   -= HandleMatchEnded;
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
                            // Estado de conexion de la partida (pausa / ganador) //
                                R_Ctm_MCM_Monitor?.SyncFromRoom();
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
                            // Autoridad del Master: vencimiento de la espera por reconexion //
                                R_Ctm_MCM_Monitor?.Tick();
                            // Autoridad del Master: resolucion de impactos al cerrar la ventana //
                                if (PhotonNetwork.IsMasterClient) R_Ctm_SA_Arbiter?.Tick(PhotonNetwork.Time);
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
                            { /* */ }     
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
//                            CompareTimeframes();
//                        yield return new WaitForSeconds(1f);
//                            _image.color = Color.yellow;     
                    }
                #endregion Regular
                #region    RPC
                    [PunRPC]
                    private void RPC_AlertShootToMaster(Hashtable Hsh_Input)
                    {
//                        int Int_ID = (int) Hsh_Input[UserID_KEY];
//                        if (Int_ID == PhotonNetwork.LocalPlayer.ActorNumber) // MASTER //
//                        { if (Shots[0].Count == 0) Shots[0] = Hsh_Input; }
//                        else                                                 // CLIENT //
//                        { if (Shots[1].Count == 0) Shots[1] = Hsh_Input; }                            
                    }
                    [PunRPC]
                    private void RPC_AlertWinnerToAll(int Int_ID)
                    {
//                        O_Bool_CanShoot = false;
//                        if (PhotonNetwork.IsMasterClient)
//                            Shots = new Hashtable[]{new Hashtable(), new Hashtable()};
//                        switch (Int_ID)
//                        {
//                            case 0:
//                                _image.color = Color.white;  
//                            return;
//                            case 1:
//                                if (PhotonNetwork.IsMasterClient)
//                                    _image.color = Color.green;      
//                                else
//                                    _image.color = Color.red;      
//                            return;
//                            case 2:
//                                if (!PhotonNetwork.IsMasterClient)
//                                    _image.color = Color.green;      
//                                else
//                                    _image.color = Color.red; 
//                            return;
//                        }
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
                    Cursor.lockState = CursorLockMode.None;
                    //Cursor.lockState = CursorLockMode.Locked;
                }
            #endregion Networking
            #region    Match Connection
                #region    Network Manager Communication
                    public void OnPlayerInactive(Player PUNPl_Player)  { R_Ctm_MCM_Monitor.HandlePlayerInactive(PUNPl_Player); }
                    public void OnPlayerReturned(Player PUNPl_Player)  { R_Ctm_MCM_Monitor.HandlePlayerReturned(PUNPl_Player); }
                    public void OnPlayerGone(Player PUNPl_Player)      { R_Ctm_MCM_Monitor.HandlePlayerGone(PUNPl_Player); }
                    public void OnMasterSwitched()
                    {
                        // Los impactos pendientes del Master anterior se pierden: no se pueden validar sin sus reportes //
                            R_Ctm_SA_Arbiter.Clear();
                            R_Ctm_MCM_Monitor.HandleMasterSwitched();
                    }
                    public void OnLocalPlayerRejoined()
                    {
                        // El estado de combate vuelve a publicarse (vida / balas / Windhorn) por si algo quedo a medias //
                            if (MasterManager.Instance.CharacterManager.OwnPlayer is PlayerCharacter Ctm_PC_Own && Ctm_PC_Own.CombatState != null)
                                Ctm_PC_Own.CombatState.PublishAll();
                        // Se relee todo desde la Room: si mientras estabamos afuera termino la partida o hay pausa, se dispara el evento //
                            R_Ctm_MCM_Monitor.SyncFromRoom();
                            if (!R_Ctm_MCM_Monitor.HasEnded && !R_Ctm_MCM_Monitor.IsPaused)
                                MasterManager.Instance.InputManager.EnableInputPlayerActions();
                        Debug.Log("[Game Manager] Status: 'Local Player Rejoined'");
                    }
                #endregion Network Manager Communication
                #region    PUN
                    public override void OnRoomPropertiesUpdate(Hashtable Hsh_Changed)
                    {
                        if (!NetworkManager.IsInMatchScene || R_Ctm_MCM_Monitor == null) return;
                        R_Ctm_MCM_Monitor.ApplyRoomProperties(Hsh_Changed);
                    }
                #endregion PUN
                #region    Monitor Events
                    private void HandlePauseStarted(int Int_Actor, double Dbl_Deadline)
                    {
                        MasterManager.Instance.InputManager.DisableInputPlayerActions();
                        ConnectionStatusOverlay.ShowCountdown(ConnectionStrings.OpponentTitle, ConnectionStrings.OpponentBodyFmt, Dbl_Deadline);
                    }
                    private void HandlePauseEnded()
                    {
                        ConnectionStatusOverlay.Hide();
                        MasterManager.Instance.InputManager.EnableInputPlayerActions();
                    }
                    private void HandleMatchEnded(int Int_Winner, MatchEndReason E_Reason)
                    {
                        // Estado local //
                            ConnectionStatusOverlay.Hide();
                            MasterManager.Instance.InputManager.DisableInputPlayerActions();
                            R_Ctm_SA_Arbiter.Clear();
                            if (R_Crtn_ReturnToMenu != null) { StopCoroutine(R_Crtn_ReturnToMenu); R_Crtn_ReturnToMenu = null; }
                        // Pantalla de resultado //
                            bool Bool_IWon = Int_Winner == PhotonNetwork.LocalPlayer.ActorNumber;
                            switch (E_Reason)
                            {
                                // Abandono: el rival no vuelve, se sale de la sala al terminar la cuenta //
                                case MatchEndReason.Abandon:
                                    GameResultScreen.Show(Bool_IWon, Bool_IWon ? ConnectionStrings.WinByAbandon : ConnectionStrings.LoseByAbandon,
                                                          _returnToMenuDelay, ConnectionStrings.BackToMenuFormat);
                                    R_Crtn_ReturnToMenu = StartCoroutine(ReturnToMenuRoutine());
                                break;
                                // Duelo ganado: los dos siguen en la sala, el Master arranca una ronda nueva //
                                case MatchEndReason.KnockOut:
                                    GameResultScreen.Show(Bool_IWon, Bool_IWon ? ResultStrings.WinDuel : ResultStrings.LoseDuel, _returnToMenuDelay);
                                    if (PhotonNetwork.IsMasterClient) R_Crtn_ReturnToMenu = StartCoroutine(NewRoundRoutine());
                                break;
                                default:
                                    GameResultScreen.Show(Bool_IWon, string.Empty, _returnToMenuDelay);
                                break;
                            }
                    }
                    private System.Collections.IEnumerator NewRoundRoutine()
                    {
                        yield return new WaitForSecondsRealtime(_returnToMenuDelay);
                        R_Crtn_ReturnToMenu = null;
                        if (!NetworkManager.IsInMatchScene || !PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom) yield break;
                        // Estado de partida limpio ANTES de recargar: sino la ronda nueva arrancaria "terminada" //
                            PhotonNetwork.CurrentRoom.SetCustomProperties(MatchConnectionMonitor.BuildResetProperties());
                        // Reinicio limpio en dos pasos (menu -> nivel), ya implementado en GameResultScreen //
                            GameResultScreen.RequestCleanRestart(1.5f);
                    }
                    private System.Collections.IEnumerator ReturnToMenuRoutine()
                    {
                        yield return new WaitForSecondsRealtime(_returnToMenuDelay);
                        R_Crtn_ReturnToMenu = null;
                        // Si el jugador ya salio por su cuenta, no se hace nada //
                            if (!NetworkManager.IsInMatchScene) yield break;
                            MasterManager.Instance.NetworkManager.LeaveGame();
                    }
                #endregion Monitor Events
            #endregion Match Connection
            #region    Lag Compensation
                #region    Tirador
                    // Lo llama la bala al detectar que toco al rival. Dbl_ShotTime = PhotonNetwork.Time del disparo //
                    public void ReportHit(double Dbl_ShotTime)
                    {
                        // Sin red (probando la escena sola): se aplica directo //
                            if (!PhotonNetwork.InRoom)
                            {
                                ((EnemyCharacter) MasterManager.Instance.CharacterManager.EnemyPlayer).Damage();
                                return;
                            }
                        photonView.RPC(nameof(RPC_ReportHit), RpcTarget.MasterClient, Dbl_ShotTime);
                    }
                #endregion Tirador
                #region    Master
                    [PunRPC]
                    private void RPC_ReportHit(double Dbl_ShotTime, PhotonMessageInfo PUNMI_Info)
                    {
                        // Validacion //
                            if (!PhotonNetwork.IsMasterClient || !NetworkManager.IsInMatchScene) return;
                            if (R_Ctm_MCM_Monitor.HasEnded || R_Ctm_MCM_Monitor.IsPaused) return;
                            int Int_Shooter = PUNMI_Info.Sender.ActorNumber; // Se usa el emisor real, no un parametro del cliente //
                            int Int_Victim = FindOpponentOf(Int_Shooter);
                            if (Int_Victim == 0) return;
                        // Timestamp acotado: no puede ser posterior a su envio ni retroceder mas de _maxShotRewind //
                            double Dbl_Sent = PUNMI_Info.SentServerTime;
                            double Dbl_Clamped = System.Math.Max(Dbl_Sent - _maxShotRewind, System.Math.Min(Dbl_ShotTime, Dbl_Sent + FUTURE_SHOT_TOLERANCE));
                            if (Dbl_Clamped != Dbl_ShotTime)
                                Debug.LogWarning("[Lag Comp] Timestamp de Actor " + Int_Shooter + " fuera de rango (" + Dbl_ShotTime.ToString("F3") + "), se usa " + Dbl_Clamped.ToString("F3"));
                        // A la cola del arbitro //
                            R_Ctm_SA_Arbiter.Report(new ShotReport(Int_Shooter, Int_Victim, Dbl_Clamped), PhotonNetwork.Time);
                            Debug.Log("[Lag Comp] Impacto reportado por Actor " + Int_Shooter + " (disparo t=" + Dbl_Clamped.ToString("F3") +
                                      ", llego " + ((PhotonNetwork.Time - Dbl_Clamped) * 1000d).ToString("F0") + " ms despues)");
                    }
                    private void HandleShotConfirmed(ShotReport Ctm_SR_Shot)
                    {
                        // El Master decide el dano (asi un cambio de balance por Live Ops es consistente para los dos) //
                            int Int_Damage = MasterManager.Instance.CharacterManager.Balance.DamagePerHit;
                            photonView.RPC(nameof(RPC_HitConfirmed), RpcTarget.All, Ctm_SR_Shot.ShooterActor, Ctm_SR_Shot.VictimActor, Int_Damage);
                            Debug.Log("[Lag Comp] Impacto CONFIRMADO: Actor " + Ctm_SR_Shot.ShooterActor + " -> Actor " + Ctm_SR_Shot.VictimActor + " (t=" + Ctm_SR_Shot.ShotTime.ToString("F3") + ")");
                    }
                    private void HandleShotAnnulled(ShotReport Ctm_SR_Shot, ShotAnnulReason E_Reason)
                    {
                        Player PUNPl_Shooter = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.GetPlayer(Ctm_SR_Shot.ShooterActor) : null;
                        if (PUNPl_Shooter != null) photonView.RPC(nameof(RPC_HitAnnulled), PUNPl_Shooter, (int) E_Reason);
                        Debug.Log("[Lag Comp] Impacto ANULADO (" + E_Reason + "): Actor " + Ctm_SR_Shot.ShooterActor + " (t=" + Ctm_SR_Shot.ShotTime.ToString("F3") + ")");
                    }
                    private static int FindOpponentOf(int Int_Actor)
                    {
                        foreach (Player PUNPl_Player in PhotonNetwork.PlayerList)
                        {
                            if (PUNPl_Player.ActorNumber != Int_Actor) return PUNPl_Player.ActorNumber;
                        }
                        return 0;
                    }
                    // GDD: "Gana el jugador que deja la vida del rival en 0" - La vida la publica el dueno, el Master la observa //
                    public override void OnPlayerPropertiesUpdate(Player PUNPl_Target, Hashtable Hsh_Changed)
                    {
                        if (!NetworkManager.IsInMatchScene || !PhotonNetwork.IsMasterClient || R_Ctm_MCM_Monitor == null) return;
                        if (!Hsh_Changed.ContainsKey(PlayerPropertyKeys.Health)) return;
                        if (PlayerCombatState.TryReadHealth(PUNPl_Target, out int Int_Health) && Int_Health <= 0)
                            R_Ctm_MCM_Monitor.DeclareKnockOut(PUNPl_Target.ActorNumber);
                    }
                #endregion Master
                #region    Todos los clientes
                    [PunRPC]
                    private void RPC_HitConfirmed(int Int_Shooter, int Int_Victim, int Int_Damage)
                    {
                        int Int_Local = PhotonNetwork.LocalPlayer.ActorNumber;
                        // Victima: descuenta su propia vida (y la publica en sus Player Custom Properties) //
                            if (Int_Local == Int_Victim)
                                ((PlayerCharacter) MasterManager.Instance.CharacterManager.OwnPlayer).ReceiveDamage(Int_Damage);
                        // Tirador: animacion de dano en su copia del rival (que ademas sincroniza la animacion de la victima) //
                            if (Int_Local == Int_Shooter)
                                ((EnemyCharacter) MasterManager.Instance.CharacterManager.EnemyPlayer).Damage();
                    }
                    [PunRPC]
                    private void RPC_HitAnnulled(int Int_Reason)
                    {
                        Debug.Log("[Lag Comp] Tu impacto no conto: " + (ShotAnnulReason) Int_Reason);
                    }
                #endregion Todos los clientes
            #endregion Lag Compensation
            #region    PowerUp
            #endregion PowerUp
        #endregion Custom Methods
    #endregion Methods
}
