using UnityEngine;
using Photon.Pun;
using System.Threading.Tasks;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using static CustomExtension.ArrayExtensions;

#region    ...
#endregion ...

#region    "using" Script Clarification
    using Hashtable = ExitGames.Client.Photon.Hashtable;
#endregion "using" Script Clarification

public class MapManager : BaseManager<MapManager> 
{
    #region    Variables
        #region    Tiles
            [SerializeField] private Transform _mapContainer;
            private Transform I_Trfm_MapContainer;
            [SerializeField] private int _tileRadius;
            private int I_Int_TileRadius;
            [SerializeField] private FloorTile_R[] _rockTiles;
            private FloorTile_R[] I_Ctm_FPR_A1_RockTiles;
            [SerializeField] private FloorTile_W[] _wallTiles;
            private FloorTile_W[] I_Ctm_FPW_A1_WallTiles;
            [SerializeField] private string[] _pickupResourcePaths;
            private string[] I_Str_A1_PickupResourcePaths;
            [SerializeField] private float _pickupSpawnInterval = 15f;
            private float I_Flt_PickupSpawnInterval;
            [SerializeField] private int _maxActivePickups = 12;
            private int I_Int_MaxActivePickups;
            private Coroutine R_Crtn_PickupSpawner;
        #endregion Tiles
        #region    Map
            [SerializeField] private int _mapLengthX;
            private int I_Int_MapLengthX;
            [SerializeField] private int _mapLengthY;
            private int I_Int_MapLengthY;
            private int[,] R_Int_A2_MapData;
            private int[] R_Int_A1_WallIDs;
        #endregion Map
        #region    Manager-To-Manager Data
            private GameObject[] O_GObj_A1_SpawnPoint;
            public GameObject[] SpawnPoints => O_GObj_A1_SpawnPoint;
        #endregion Manager-To-Manager Data
        #region    Hashtables (Communication among instances)
            private const string MapTileRadius_KEY = "MapTileRadius";
            private const string MapAxisX_KEY = "MapAxisX";
            private const string MapAxisY_KEY = "MapAxisY";
            private const string MapData_KEY = "MapData";
            private const string MapWallIDs_KEY = "MapWallIDs";
        #endregion Hashtables (Communication among instances)
        #endregion Async Wait Variables
            TaskCompletionSource<bool> R_Bool_StartupCompleted; 
            TaskCompletionSource<bool> R_Bool_AllPlayersFinished; 
            TaskCompletionSource<bool> R_Bool_MasterStartupCompleted;
            int R_Int_PlayersRemaining;
            // Si el RPC llega antes de que se cree el TaskCompletionSource, se recuerda aca //
            bool R_Bool_MapDataArrived = false;
            bool R_Bool_MasterDoneArrived = false;
        #region    Async Wait Variables
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Singleton Operations //
                    base.Awake();
                // Variables //
                    I_Trfm_MapContainer = _mapContainer;
                    I_Int_TileRadius = _tileRadius;
                    I_Ctm_FPR_A1_RockTiles = _rockTiles;
                    I_Ctm_FPW_A1_WallTiles = _wallTiles;
                    I_Str_A1_PickupResourcePaths = _pickupResourcePaths;
                    I_Flt_PickupSpawnInterval = _pickupSpawnInterval;
                    I_Int_MaxActivePickups = _maxActivePickups;
                    SetBounds(_mapLengthX, _mapLengthY);
            }
        #endregion Unity Methods
        #region    Override Methods
            public async override void OnStartUp()
            { 
                // Resume Base //
                    base.OnStartUp();
                // Scene dependant Behaviour //
                    switch (SceneManager.GetActiveScene().buildIndex) 
                    {
                        case 0:
                            // Menu Manager Communication - Change to Loading //
                                MasterManager.Instance.MenuManager.OpenMenu("LoadingMenu");
                        break;
                        case 1: // Level //
                            // Reset de ronda - El Map Manager es DontDestroyOnLoad, estos datos sobreviven al LoadLevel //
                                // Si no se limpian, en la 2da ronda los avisos viejos dan por cargado un mapa que todavia no existe //
                                    R_Bool_MapDataArrived = false;
                                    R_Bool_MasterDoneArrived = false;
                                // El pool apunta a Pickupables de la ronda anterior, ya destruidos //
//-//                                    R_Ctm_Pkp_A1_Pool = new Pickupable[0];
                                // Cortar el spawner de la ronda anterior, si no se acumula uno por ronda //
                                    if (R_Crtn_PickupSpawner != null)
                                    {
                                        StopCoroutine(R_Crtn_PickupSpawner);
                                        R_Crtn_PickupSpawner = null;
                                    }
                            // Async Variables //
                                R_Bool_StartupCompleted = new TaskCompletionSource<bool>();
                                R_Bool_AllPlayersFinished = new TaskCompletionSource<bool>();
                                R_Bool_MasterStartupCompleted = new TaskCompletionSource<bool>();
                                R_Int_PlayersRemaining = PhotonNetwork.CurrentRoom.PlayerCount;
                            // Si los avisos llegaron antes de crear los Task, se resuelven ahora //
                                if (R_Bool_MapDataArrived)   R_Bool_StartupCompleted.TrySetResult(true);
                                if (R_Bool_MasterDoneArrived) R_Bool_MasterStartupCompleted.TrySetResult(true);
                            // Menu Manager Communication - Change to Loading //
                                MasterManager.Instance.MenuManager.OpenMenu("LoadingMenu");
                            // Set Bounds //
                                SetBounds(_mapLengthX, _mapLengthY);
                            // MapManager - Search Container //
//-//                                I_Trfm_MapContainer = (Object.FindFirstObjectByType<MapContainer>()).gameObject.transform;
                            // NetworkManager - PUN Network Code //
                                if (PhotonNetwork.IsMasterClient)
                                {
                                    // Master //
                                        // Remove Self From Countdown //
                                            ReduceCountdown();
                                        // MapManager - Generate Map Data //
                                            GenerateMap();
                                    // Pickups - Start Periodic Spawn //
//-//                                    R_Crtn_PickupSpawner = StartCoroutine(SpawnPickupsPeriodically());
                                    // MenuManager - Switch Screen //
                                    MasterManager.Instance.MenuManager.OpenMenu("HUD"); 
                                        // Async Await - Wait For "All Loads" //
                                            // Wait //
                                                bool Bool_Success = await R_Bool_AllPlayersFinished.Task;
                                            // Inform Completion //
                                                MasterWaitCompleted();
                                        // Proceed //
                                                MasterManager.Instance.StartUpProceed();

                                    Debug.Log("[Map Manager] Status: Master Generated Map");
                                }
                                else
                                { 
                                    // Clients //
                                        // Async Await - Wait For "Load" //
                                            // Wait //
                                                bool Bool_Success1 = await R_Bool_StartupCompleted.Task;
                                            // Inform Completion //
                                                ReduceClientLoadCounter();
                                        // Async Await - Wait For Master //
                                            // Wait //
                                                bool Bool_Success2 = await R_Bool_MasterStartupCompleted.Task;
                                            // Inform Completion //
                                                // ReduceClientLoadCounter();
                                        // Proceed //
                                                MasterManager.Instance.StartUpProceed();

                                    Debug.Log("[Map Manager] Status: Client Loaded Map");
                                }
                        break;
                    }
            }
        #endregion Override Methods
        #region    Custom Methods
            #region    Regular Methods
                public void SetBounds(int Int_X, int Int_Y)
                {
                    _mapLengthX = ( Int_X < 4 ) ? 4: Int_X;
                    _mapLengthY = ( Int_Y < 4 ) ? 4: Int_Y;
                    I_Int_MapLengthX = _mapLengthX;
                    I_Int_MapLengthY = _mapLengthY;
                    R_Int_A2_MapData = new int[I_Int_MapLengthX, I_Int_MapLengthY];

                    O_GObj_A1_SpawnPoint = new GameObject[0]; // Spawnpoints are located on each Map Border Tile
                }
                public void GenerateMap()
                {
                    #region    Store Border Value 
                        #region    Variables
                            R_Int_A1_WallIDs = new int[0];
                            bool[,] Bool_A2_IsMapBorder = new bool[I_Int_MapLengthX, I_Int_MapLengthY];
                        #endregion Variables
                        #region    Process
                            for (int Int_IndexY = 0; Int_IndexY < I_Int_MapLengthY; Int_IndexY++)
                            {
                                for (int Int_IndexX = 0; Int_IndexX < I_Int_MapLengthX; Int_IndexX++)
                                {
                                    // Is Border? //
                                        if (
                                            (Int_IndexX == 0 || Int_IndexX == I_Int_MapLengthX - 1) || 
                                            (Int_IndexY == 0 || Int_IndexY == I_Int_MapLengthY - 1)
                                           )
                                        {
                                            Bool_A2_IsMapBorder[Int_IndexX, Int_IndexY] = true;
                                        }
                                    // Is Filler? //
                                        else
                                        {
                                            Bool_A2_IsMapBorder[Int_IndexX, Int_IndexY] = false;
                                        }
                                }
                            }
                        #endregion Process
                    #endregion Store Border Value 
                    #region    Spawn Map
                        #region    Variables
                            Vector3 Vec3_TilePos = new Vector3(0, 0, 0);
                            int Int_TileIndex = 0;
                        #endregion Variables
                        #region    Process
                            for (int Int_IndexY = 0; Int_IndexY < I_Int_MapLengthY; Int_IndexY++)
                            {
                                for (int Int_IndexX = 0; Int_IndexX < I_Int_MapLengthX; Int_IndexX++)
                                {
                                    #region    Spawn Tiles
                                        // Is Border? //
                                            if (
                                                Int_IndexX == 0                    || // Top Row          //
                                                Int_IndexX == I_Int_MapLengthX - 1 || // Bottom Row       //
                                                Int_IndexY == 0                    || // Leftmost Column  //
                                                Int_IndexY == I_Int_MapLengthY - 1    // Rightmost Column //
                                               )
                                            {
                                                // Get Index //
                                                    #pragma warning disable CS8846
                                                        R_Int_A2_MapData[Int_IndexX, Int_IndexY] =
                                                        (Int_IndexX, Int_IndexY, (I_Int_MapLengthX-1), (I_Int_MapLengthY-1)) switch 
                                                        {
                                                            // Is Corner? //
                                                                var (Xi, Yi,  _,  _) when (Xi == 0  && Yi == 0 ) => 0,                               // Top Left Corner     //
                                                                var (Xi, Yi, Xm,  _) when (Xi == Xm && Yi == 0 ) => 1,                               // Top Right Corner    //
                                                                var (Xi, Yi, Xm, Ym) when (Xi == Xm && Yi == Ym) => 2,                               // Bottom Right Corner //
                                                                var (Xi, Yi,  _, Ym) when (Xi == 0  && Yi == Ym) => 3,                               // Bottom Left Corner  //
                                                            // Is Side? //
                                                                var (Xi, Yi, Xm,  _) when (((0 < Xi) && (Xi < Xm)) && Yi == 0                ) => 4, // Top Side    //
                                                                var (Xi, Yi, Xm, Ym) when (Xi == Xm                && ((0 < Yi) && (Yi < Ym))) => 5, // Right Side  //
                                                                var (Xi, Yi, Xm, Ym) when (((0 < Xi) && (Xi < Xm)) && Yi == Ym               ) => 6, // Bottom Side //
                                                                var (Xi, Yi,  _, Ym) when (Xi == 0                 && ((0 < Yi) && (Yi < Ym))) => 7  // Left Side   //
                                                        };
                                                    #pragma warning restore CS8846
                                                // Spawn Tile //
                                                    string Str_TilePath = "01. Procedural Gen/01. Tiles/02. Walls/Variants/";
                                                    string[] Str_A1_WallTile = new string[] {
                                                                                                Str_TilePath + "FloorTile_WC0_",
                                                                                                Str_TilePath + "FloorTile_WC1_",
                                                                                                Str_TilePath + "FloorTile_WC2_", 
                                                                                                Str_TilePath + "FloorTile_WC3_",
                                                                                                Str_TilePath + "FloorTile_WS0_",
                                                                                                Str_TilePath + "FloorTile_WS1_",
                                                                                                Str_TilePath + "FloorTile_WS2_", 
                                                                                                Str_TilePath + "FloorTile_WS3_",
                                                                                            };
                                                    FloorTile_W Ctm_FTW_Tile = (PhotonNetwork.Instantiate(Str_A1_WallTile[R_Int_A2_MapData[Int_IndexX, Int_IndexY]], Vec3_TilePos, new Quaternion (0,0,0,0))).GetComponent<FloorTile_W>();
                                                    Ctm_FTW_Tile.gameObject.transform.SetParent(I_Trfm_MapContainer);
                                                    Ctm_FTW_Tile.gameObject.name = Ctm_FTW_Tile.gameObject.name[..14] + Int_TileIndex;

                                                    AddNewToArray(ref R_Int_A1_WallIDs, Ctm_FTW_Tile.GetComponent<PhotonView>().ViewID);
//-//                                                    AddNewToArray(ref O_GObj_A1_SpawnPoint, Ctm_FTW_Tile.SpawnPoint);
                                            }
                                        // Is Filler? //
                                            else
                                            {
                                                // Get Index //
                                                    do
                                                    {
                                                        R_Int_A2_MapData[Int_IndexX, Int_IndexY] = Random.Range(1, I_Ctm_FPR_A1_RockTiles.Length);
                                                    }
                                                    while 
                                                    (
                                                        #region    Is Different Than Adjacent?
                                                            // Left Tile //
                                                            (
                                                                (R_Int_A2_MapData[Int_IndexX - 1, Int_IndexY] == R_Int_A2_MapData[Int_IndexX, Int_IndexY]) &&
                                                                (!Bool_A2_IsMapBorder[Int_IndexX - 1, Int_IndexY])
                                                            ) ||
                                                            // Upper Left Tile //
                                                            (
                                                                (R_Int_A2_MapData[Int_IndexX - 1, Int_IndexY - 1] == R_Int_A2_MapData[Int_IndexX, Int_IndexY]) &&
                                                                (!Bool_A2_IsMapBorder[Int_IndexX - 1, Int_IndexY - 1])
                                                            ) ||
                                                            // Upper Tile //
                                                            (
                                                                (R_Int_A2_MapData[Int_IndexX, Int_IndexY - 1] == R_Int_A2_MapData[Int_IndexX, Int_IndexY]) &&
                                                                (!Bool_A2_IsMapBorder[Int_IndexX, Int_IndexY - 1])
                                                            ) ||
                                                            // Upper Right Tile //
                                                            (
                                                                (R_Int_A2_MapData[Int_IndexX + 1, Int_IndexY - 1] == R_Int_A2_MapData[Int_IndexX, Int_IndexY]) &&
                                                                (!Bool_A2_IsMapBorder[Int_IndexX + 1, Int_IndexY - 1])
                                                            )
                                                        #endregion 
                                                    );
                                                        // Spawn Tile //
                                                            string Str_TilePath = "01. Procedural Gen/01. Tiles/01. Rocks/Variants/";
                                                            string[] Str_A1_ObstacleTile = new string[] {
                                                                                                            Str_TilePath + "FloorTile_R00_",
                                                                                                            Str_TilePath + "FloorTile_R01_",
                                                                                                            Str_TilePath + "FloorTile_R02_",
                                                                                                            Str_TilePath + "FloorTile_R03_",
                                                                                                            Str_TilePath + "FloorTile_R04_",
                                                                                                            Str_TilePath + "FloorTile_R05_",
                                                                                                            Str_TilePath + "FloorTile_R06_",
                                                                                                            Str_TilePath + "FloorTile_R07_",
                                                                                                            Str_TilePath + "FloorTile_R08_",
                                                                                                            Str_TilePath + "FloorTile_R09_",
                                                                                                            Str_TilePath + "FloorTile_R10_",
                                                                                                            Str_TilePath + "FloorTile_R11_",
                                                                                                            Str_TilePath + "FloorTile_R12_",
                                                                                                            Str_TilePath + "FloorTile_R13_"
                                                                                                        };
                                                    FloorTile_R Ctm_FTR_Tile = (PhotonNetwork.Instantiate(Str_A1_ObstacleTile[R_Int_A2_MapData[Int_IndexX, Int_IndexY]], Vec3_TilePos, new Quaternion (0,0,0,0))).GetComponent<FloorTile_R>();
                                                    Ctm_FTR_Tile.gameObject.transform.SetParent(I_Trfm_MapContainer);
                                                    Ctm_FTR_Tile.gameObject.name = Ctm_FTR_Tile.gameObject.name[..14] + Int_TileIndex;
                                            }
                                        // Next Tile Index //
                                            Int_TileIndex++;
                                    #endregion Spawn Tiles
                                    #region    Change Next "X Axis" Location
                                        Vec3_TilePos = new Vector3(Vec3_TilePos.x + (I_Int_TileRadius*2), 0, Vec3_TilePos.z);
                                    #endregion
                                }
                                #region    Reset Next "X Axis" Location & Change Next "Y Axis" Location
                                    Vec3_TilePos = new Vector3(0, 0, Vec3_TilePos.z - (I_Int_TileRadius*2));
                                #endregion 
                            }
                        #endregion Process
                    #endregion Spawn Map
                    #region    Synchronize
                        Flatten2DArray(R_Int_A2_MapData, I_Int_MapLengthX, I_Int_MapLengthY, out int[] Int_A1_TempMapData);
                        Hashtable Hsh_MapKeys = new Hashtable
                        {
                            { MapTileRadius_KEY, I_Int_TileRadius     },
                            { MapAxisX_KEY     , I_Int_MapLengthX     },
                            { MapAxisY_KEY     , I_Int_MapLengthY     },
                            { MapData_KEY      , Int_A1_TempMapData   },
                            { MapWallIDs_KEY   , R_Int_A1_WallIDs     },
                        }; 
                        UpdateAllMapManagers(Hsh_MapKeys);
                    #endregion Synchronize
                }
                public void LoadMap()
                {
                    #region    Spawn Map
                        // Si algun tile todavia no llego, se saltea en vez de cortar el LoadMap a la mitad //
                        // Cortarlo dejaba el flag sin setear y el cliente se quedaba cargando para siempre //
                        int Int_Missing = 0;
                        foreach (int Int_ID in R_Int_A1_WallIDs)
                        {
                            PhotonView PV_Tile = PhotonView.Find(Int_ID);
                            if (PV_Tile == null) { Int_Missing++; continue; }
                            FloorTile_W Ctm_FTW_Tile = PV_Tile.GetComponent<FloorTile_W>();
//-//                            if (Ctm_FTW_Tile == null || Ctm_FTW_Tile.SpawnPoint == null) { Int_Missing++; continue; }
//-//                            AddNewToArray(ref O_GObj_A1_SpawnPoint, Ctm_FTW_Tile.SpawnPoint);
                        }
                        if (Int_Missing > 0)
                            Debug.LogWarning("[Map Manager] " + Int_Missing + " tiles todavia no llegaron al cargar el mapa");
                        R_Bool_MapDataArrived = true;
                        R_Bool_StartupCompleted?.TrySetResult(true);
                    #endregion Spawn Map
                }
                public void ReduceCountdown() 
                {
                    R_Int_PlayersRemaining--;
                    if (R_Int_PlayersRemaining == 0) { R_Bool_AllPlayersFinished?.TrySetResult(true); }
                }
                public void MatserStartUpCompleted()
                {
                    R_Bool_MasterDoneArrived = true;
                    R_Bool_MasterStartupCompleted?.TrySetResult(true);
                }
            #endregion Regular Methods
            #region    Coroutines Methods
            #endregion Coroutines Methods
            #region    PUN         
                #region    Hastable
                    // Map //
                        public void ApplyMapManagerProperties(Hashtable Hsh_Input)
                        {
                            I_Int_TileRadius =          (int) Hsh_Input[MapTileRadius_KEY];
                            R_Int_A2_MapData = new int[I_Int_MapLengthX, I_Int_MapLengthY];
                            SetBounds((int)Hsh_Input[MapAxisX_KEY], (int)Hsh_Input[MapAxisY_KEY]);
                            UnFlatten1DArray((int[]) Hsh_Input[MapData_KEY], I_Int_MapLengthX, I_Int_MapLengthY, out int[,] Int_A2_TempMapData);
                            R_Int_A2_MapData = Int_A2_TempMapData;
                            R_Int_A1_WallIDs  = (int[])Hsh_Input[MapWallIDs_KEY];
                        }
                        public void ReduceClientLoadCounter()
                        {
                            GetComponent<PhotonView>().RPC(nameof(RPC_ReduceClientLoadCounter), RpcTarget.MasterClient); 
                        }
                        public void MasterWaitCompleted()
                        {
                            GetComponent<PhotonView>().RPC(nameof(RPC_MasterWaitCompleted), RpcTarget.All); 
                        }
                        public void UpdateAllMapManagers(Hashtable Hsh_MapKeys)
                        {
                            GetComponent<PhotonView>().RPC(nameof(RPC_ForceMapUpdateToClients), RpcTarget.All, Hsh_MapKeys); 
                        }
                        private bool AreMapManagerHashtables(Hashtable Hsh_Input)
                        {
                            if (
                                Hsh_Input.ContainsKey(MapTileRadius_KEY) &&
                                Hsh_Input.ContainsKey(MapAxisX_KEY)      &&
                                Hsh_Input.ContainsKey(MapAxisY_KEY)      &&
                                Hsh_Input.ContainsKey(MapData_KEY)       &&
                                Hsh_Input.ContainsKey(MapWallIDs_KEY)
                                )
                            { 
                                return true; 
                            }
                            else
                            { 
                                UnityEngine.Debug.LogError("\"MapContiner.cs\"'s \"AreMapManagerHashtables()\" returned FALSE, review Hastables");
                                return false; 
                            }
                        }
                #endregion Hastable  
                #region    RPC
                    #region    Hashtables
                        // Map //
                            [PunRPC]
                            private void RPC_ForceMapUpdateToClients(Hashtable Hsh_Properties)
                            {
                                if (!PhotonNetwork.IsMasterClient)
                                {
                                    if (AreMapManagerHashtables(Hsh_Properties))
                                    {
                                        // Import new properties //
                                            MasterManager.Instance.MapManager.ApplyMapManagerProperties(Hsh_Properties);                           
                                        // Load Map //
                                            MasterManager.Instance.MapManager.LoadMap();
                                        // MenuManager - Switch Screen //
                                            MasterManager.Instance.MenuManager.OpenMenu("HUD"); 
                                    }
                                }
                            }
                    #endregion Hashtables 
                    #region    Await
                            [PunRPC]
                            private void RPC_ReduceClientLoadCounter()
                            {
                                if (PhotonNetwork.IsMasterClient)
                                {
                                     MasterManager.Instance.MapManager.ReduceCountdown();     
                                }
                            }
                            [PunRPC]
                            private void RPC_MasterWaitCompleted()
                            {
                                if (!PhotonNetwork.IsMasterClient)
                                {
                                      MasterManager.Instance.MapManager.MatserStartUpCompleted();     
                                }
                            }
                    #endregion Await 
                #endregion RPC
            #endregion PUN
        #endregion Custom Methods
            
    #endregion Methods
}
