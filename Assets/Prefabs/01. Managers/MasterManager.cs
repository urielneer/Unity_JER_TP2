using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

#region    ...
#endregion ...

public class MasterManager : BaseManager<MasterManager>
{
    #region    Variables
        #region    Sub-Managers
            // MAP //
                [SerializeField] private MapManager _mapManager;
                private MapManager I_Ctm_MpMgr_MapManager;
                public MapManager MapManager => I_Ctm_MpMgr_MapManager;
            // CHARACTER //
                [SerializeField] private CharacterManager _charManager;
                private CharacterManager I_Ctm_CMgr_CharManager;
                public CharacterManager CharacterManager => I_Ctm_CMgr_CharManager;
            // GAME //
                [SerializeField] private GameManager _gameManager;
                private GameManager I_Ctm_GMgr_GameManager;
                public GameManager GameManager => I_Ctm_GMgr_GameManager;
            // BULLET //
                [SerializeField] private BulletManager _bulletManager;
                private BulletManager I_Ctm_BMgr_BulletManager;
                public BulletManager BulletManager => I_Ctm_BMgr_BulletManager;
            // NETWORK //
                [SerializeField] private NetworkManager _networkManager;
                private NetworkManager I_Ctm_NMgr_NetworkManager;
                public NetworkManager NetworkManager => I_Ctm_NMgr_NetworkManager;
            // MENU //
                [SerializeField] private MenuManager _menuManager;
                private MenuManager I_Ctm_MnMgr_MenuManager;
                public MenuManager MenuManager => I_Ctm_MnMgr_MenuManager;
            // INPUT //
                [SerializeField] private CustomInputManager _inputManager;
                private CustomInputManager I_Ctm_IMgr_InputManager;
                public CustomInputManager InputManager => I_Ctm_IMgr_InputManager;
        #endregion Sub-Managers
        #region    Flags
            TaskCompletionSource<bool> R_Bool_StartupAwait; 
            bool R_Bool_StartupCompleted = false;
        #endregion Flags
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            public override void OnEnable()
            {
                // Resume Base //
                    base.OnEnable();
                // Scene Manager Subscribe //
                    SceneManager.sceneLoaded += OnSceneLoaded;
            }
            public override void OnDisable()
            {
                // Resume Base //
                    base.OnDisable();
                // Scene Manager Subscribe //
                    SceneManager.sceneLoaded -= OnSceneLoaded;
            }
            protected override void Awake()
            {
                // "Fuse" Managers //
                    if (_instance != null && _instance != this)
                    { 
                        if (_mapManager     != null) _instance._mapManager =           (MapManager) UpdateSubManager(_instance._mapManager, _mapManager, _mapManager.gameObject);
                        if (_charManager    != null) _instance._charManager =    (CharacterManager) UpdateSubManager(_instance._charManager, _charManager, _charManager.gameObject);
                        if (_gameManager    != null) _instance._gameManager =         (GameManager) UpdateSubManager(_instance._gameManager, _gameManager, _gameManager.gameObject);
                        if (_menuManager    != null) _instance._menuManager =         (MenuManager) UpdateSubManager(_instance._menuManager, _menuManager, _menuManager.gameObject);
                        if (_bulletManager  != null) _instance._bulletManager =     (BulletManager) UpdateSubManager(_instance._bulletManager, _bulletManager, _bulletManager.gameObject);
                        if (_networkManager != null) _instance._networkManager =   (NetworkManager) UpdateSubManager(_instance._networkManager, _networkManager, _networkManager.gameObject);
                        if (_inputManager   != null) _instance._inputManager = (CustomInputManager) UpdateSubManager(_instance._inputManager, _inputManager, _inputManager.gameObject);
                    }
                // Resume Base //
                    base.Awake();
                // Variables //
                    I_Ctm_MpMgr_MapManager = _mapManager;         
                    I_Ctm_CMgr_CharManager = _charManager;       
                    I_Ctm_GMgr_GameManager = _gameManager;       
                    I_Ctm_MnMgr_MenuManager = _menuManager;       
                    I_Ctm_BMgr_BulletManager = _bulletManager;   
                    I_Ctm_NMgr_NetworkManager = _networkManager;
                    I_Ctm_IMgr_InputManager = _inputManager;
            }
            private void Start()
            {
                //OnStartUp();
            }
            private void Update()
            {
                OnUpdate(Time.fixedDeltaTime);
            }
            private void FixedUpdate()
            {
                OnFixedUpdate(Time.fixedDeltaTime);
            }
        #endregion Unity Methods
        #region    Override Methods
            public async override void OnStartUp()
            {
                // Flag //
                    R_Bool_StartupCompleted = false;
                    R_Bool_StartupAwait = new TaskCompletionSource<bool>(); 
                // Resume Base //
                    base.OnStartUp();
                // Start Up Sub-Managers //
                    // Start //
                        if (I_Ctm_IMgr_InputManager   != null) I_Ctm_IMgr_InputManager.OnStartUp();   // Set Key Actions //
                        if (I_Ctm_MnMgr_MenuManager   != null) I_Ctm_MnMgr_MenuManager.OnStartUp();   // Set Menu Up //
                        if (I_Ctm_NMgr_NetworkManager != null) I_Ctm_NMgr_NetworkManager.OnStartUp(); // Set Nework status On (If It's Off) //
//-//                        if (I_Ctm_MpMgr_MapManager    != null) I_Ctm_MpMgr_MapManager.OnStartUp();    // Create/Load Map //
                    // Await - Resume Startup //
//-//                         R_Bool_StartupCompleted = await R_Bool_StartupAwait.Task;
                    // Resume //
//-//                        if (I_Ctm_GMgr_GameManager    != null) I_Ctm_GMgr_GameManager.OnStartUp();    // Assign Spawnpoints //
//-//                        if (I_Ctm_CMgr_CharManager    != null) I_Ctm_CMgr_CharManager.OnStartUp();    // Spawn Players //
                        // Game Manager Communication // 
                            MasterManager.Instance.GameManager.GameStart();
//-//                        if (I_Ctm_BMgr_BulletManager  != null) I_Ctm_BMgr_BulletManager.OnStartUp();  // Setup Bullet Manager //
                // Flag //
                    R_Bool_StartupCompleted = true;
            }
            public override void OnUpdate(float Flt_DT)
            {
                // Flag Check //
                    if (!R_Bool_StartupCompleted) return;
                // Resume Base //
                    base.OnUpdate(Flt_DT);
                // Start Up Sub-Managers //
                    if (I_Ctm_MnMgr_MenuManager   != null) I_Ctm_MnMgr_MenuManager.OnUpdate(Flt_DT);       
                    // 1) Manage in-game GUI //
                    if (I_Ctm_CMgr_CharManager    != null) I_Ctm_CMgr_CharManager.OnUpdate(Flt_DT);   // 1) Player Controls //
//-//                    if (I_Ctm_BMgr_BulletManager  != null) I_Ctm_BMgr_BulletManager.OnUpdate(Flt_FixedDT); // 2) Bullet "Flight" //
            }
            public override void OnFixedUpdate(float Flt_FixedDT)
            {
                // Flag Check //
                    if (!R_Bool_StartupCompleted) return;
                // Resume Base //
                    base.OnFixedUpdate(Flt_FixedDT);
                // Start Up Sub-Managers //
                    if (I_Ctm_MnMgr_MenuManager   != null) I_Ctm_MnMgr_MenuManager.OnFixedUpdate(Flt_FixedDT);  // 1) Manage Menus //
//-//                    if (I_Ctm_CMgr_CharManager    != null) I_Ctm_CMgr_CharManager.OnFixedUpdate(Flt_FixedDT);   // 3) Player Controls //
                    if (I_Ctm_GMgr_GameManager    != null) I_Ctm_GMgr_GameManager.OnFixedUpdate(Flt_FixedDT);   // 4) Assign Spawnpoints & Respawn //
//-//                    if (I_Ctm_BMgr_BulletManager  != null) I_Ctm_BMgr_BulletManager.OnFixedUpdate(Flt_FixedDT); // 5) Bullet "Flight" //
            }
        #endregion Override Methods
        #region    Scene Subscription Methods
            #pragma warning disable CS1998
            private async void OnSceneLoaded(Scene Scn_Scene, LoadSceneMode LSM_Mode)
            #pragma warning restore CS1998
            {
                OnStartUp();
            }
        #endregion Scene Subscription Methods
        #region    Custom Methods
            public void StartUpProceed()
            {
                R_Bool_StartupAwait?.TrySetResult(true);
            }
            private object UpdateSubManager(object Obj_Instance, object Obj_This, GameObject GObj_This)
            {
                switch (Obj_Instance)
                {
                    case null:
                        GObj_This.transform.SetParent(_instance.gameObject.transform);
                        return Obj_This;
                    default:
                        return Obj_Instance;
                }
            }
            private GameObject ClearSubManager(GameObject GObj_This) 
            {
                Destroy(GObj_This);
                return null;
            }
        #endregion Custom Methods
    #endregion Methods
}
