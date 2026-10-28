using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using static CustomExtension.ArrayExtensions;

#region    "using" Script Clarification
    using Hashtable = ExitGames.Client.Photon.Hashtable;
#endregion "using" Script Clarification

public class BulletManager : BaseManager<BulletManager>
{ 
    #region    Variables
        [Header(" All Character Settings")]
            [SerializeField] float _speed;
            float I_Flt_Speed;
            [SerializeField] float _damage;
            float I_Flt_Damage;
        [Header(" Manager Settings")]
            [SerializeField] Bullet _prefab;
            Bullet I_Ctm_Bllt_Prefab;
            [SerializeField] private Transform _mapContainer;
            private Transform I_Trfm_MapContainer;
        #region    Hashtables (Communication among instances)
            private const string BulletOwnerID_KEY = "BulletOwnerID";
            private const string BulletOwnerName_KEY = "BulletOwnerName";
            private const string BulletPos_KEY = "BulletPos";
            private const string BulletQuat_KEY = "BulletQuat";
            private const string BulletDir_KEY = "BulletDir";
            private const string BulletType_KEY = "BulletType";
        #endregion Hashtables (Communication among instances)
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Singleton Operations //
                    base.Awake();
                // Variables //
                    I_Trfm_MapContainer = _mapContainer;
                    I_Flt_Speed = _speed;
                    I_Flt_Damage = _damage;
                    I_Ctm_Bllt_Prefab = _prefab;
            }
        #endregion Unity Methods
        #region    Override Methods
            public override void OnStartUp()
            {
                // Resume Base //
                    base.OnStartUp();
                // Scene dependant Behaviour //
                    switch (SceneManager.GetActiveScene().buildIndex) 
                    {
                        case 0:
                        break;
                        case 1: // Level //
                            // MapManager - Container //
                        break;
                    }
            }
            public override void OnUpdate(float Flt_DT)
            {
                // Resume Base //
                    base.OnUpdate(Flt_DT);
                // Scene dependant Behaviour //
                    switch (SceneManager.GetActiveScene().buildIndex) 
                    {
                        case 0:
                        break;
                        case 1: // Level //
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
                        break;
                    }
            }
        #endregion Override Methods
        #region    Custom Methods
            public void SpawnBullet(string Str_OwnerNickname, int Int_ID, Vector3 Vec3_Pos, Quaternion Quat_Rot, Vector3 Vec3_Dir, int Int_Type, float Flt_SpeedMult)
            {
Debug.Log(" "+Str_OwnerNickname+") ID: "+Int_ID+" Dir: "+Vec3_Dir+" Speed: "+I_Flt_Speed+" Damage "+I_Flt_Damage+" Type: "+Int_Type);
                #region    Spawn
                    // Spawn Bullet //
                        Bullet Ctm_Bllt_Own = (Instantiate(I_Ctm_Bllt_Prefab, Vec3_Pos, Quat_Rot)).GetComponent<Bullet>();
                    // Setup Bullets Dir & Speed //
//-//                        Ctm_Bllt_Own.SetData(Str_OwnerNickname, Int_ID, Vec3_Dir, I_Flt_Speed * Flt_SpeedMult, I_Flt_Damage, Int_Type);
                    // Start Up // 
//-//                        Ctm_Bllt_Own.OnStartUp();
                    // MapManager - Container //
                        Ctm_Bllt_Own.gameObject.transform.SetParent(I_Trfm_MapContainer, true);
                #endregion Spawn
            }
            public void SynchronizeBullet(BasePlayer Ctm_BP_Owner, Vector3 Vec3_Dir, int Int_Index = 0)
            {
                // Variables //
//-//                    Transform Tfm_SpawnSource = Ctm_BP_Owner.CanonAnchor;
//-//                    Vector3 Vec3_Pos = Tfm_SpawnSource.transform.position;
//-//                   Quaternion Quat_Rot = Tfm_SpawnSource.transform.rotation;
                // Synchronize //
                    Hashtable Hsh_MapKeys = new Hashtable
                    {
//-//                       { BulletOwnerName_KEY , Ctm_BP_Owner.OwnerNickname },
//-//                       { BulletOwnerID_KEY   , Ctm_BP_Owner.ID            },
//-//                       { BulletPos_KEY       , Vec3_Pos                   },
//-//                       { BulletQuat_KEY      , Quat_Rot                   },
                        { BulletDir_KEY       , Vec3_Dir                   },
                        { BulletType_KEY      , Int_Index                   },
                    }; 
                    UpdateAllBulletManagers(Hsh_MapKeys);
                    PhotonNetwork.SendAllOutgoingCommands();
            }
        #endregion Custom Methods
        #region    PUN         
            #region    Hastable
                // Bullet //
                    public void UpdateAllBulletManagers(Hashtable Hsh_Input)
                    {
                        GetComponent<PhotonView>().RPC(nameof(RPC_ForceBulletUpdateToClients), RpcTarget.All, Hsh_Input); 
                    }
                    private bool AreBulletDataHashtables(Hashtable Hsh_Input)
                    {
                        if ( 
                            Hsh_Input.ContainsKey(BulletOwnerID_KEY)   &&
                            Hsh_Input.ContainsKey(BulletOwnerName_KEY) &&
                            Hsh_Input.ContainsKey(BulletPos_KEY)       &&
                            Hsh_Input.ContainsKey(BulletQuat_KEY)      &&
                            Hsh_Input.ContainsKey(BulletDir_KEY)       &&
                            Hsh_Input.ContainsKey(BulletType_KEY)       
                           )
                        { 
                            return true; 
                        }
                        else
                        { 
                            UnityEngine.Debug.LogError("\"MapContiner.cs\"'s \"AreBulletDataHashtables()\" returned FALSE, review Hastables");
                            return false; 
                        }
                    }
            #endregion Hastable  
            #region    RPC
                #region    Hashtables
                    // Bullet //
                        [PunRPC]
                        private void RPC_ForceBulletUpdateToClients(Hashtable Hsh_Input)
                        {
                            if (AreBulletDataHashtables(Hsh_Input))
                            {                                
                                float Flt_Multiplier = (PhotonNetwork.IsMasterClient) ? 1 : 10;
                                // Force Spawn //
                                    MasterManager.Instance.BulletManager.SpawnBullet(
                                                                                     (string)Hsh_Input[BulletOwnerName_KEY], 
                                                                                     (int)Hsh_Input[BulletOwnerID_KEY],
                                                                                     (Vector3)Hsh_Input[BulletPos_KEY], 
                                                                                     (Quaternion)Hsh_Input[BulletQuat_KEY], 
                                                                                     (Vector3)Hsh_Input[BulletDir_KEY],
                                                                                     (int)Hsh_Input[BulletType_KEY],
                                                                                     Flt_Multiplier
                                                                                    );
                            }
                        }
                #endregion Hashtables 
            #endregion RPC
        #endregion PUN
    #endregion Methods
}
