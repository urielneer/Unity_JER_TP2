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
            [SerializeField] private Transform _bulletContainer;
            private Transform I_Trfm_BulletContainer;
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Singleton Operations //
                    base.Awake();
                // Variables //
                    I_Trfm_BulletContainer = _bulletContainer;
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
                            foreach (Transform Tfrm_Bullet in I_Trfm_BulletContainer.transform)
                            {
                                Tfrm_Bullet.GetComponent<Bullet>().OnStartUp();
                            }
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
                            foreach (Transform Tfrm_Bullet in I_Trfm_BulletContainer.transform)
                            {
                                Tfrm_Bullet.GetComponent<Bullet>().OnUpdate(Flt_DT);
                            }
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
            public void SpawnBullet(Vector3 Vec3_Pos)
            {
                #region    Spawn
                    // Spawn Bullet //
                        Bullet Ctm_Bllt_Own = (Instantiate(I_Ctm_Bllt_Prefab, Vec3_Pos, Quaternion.identity)).GetComponent<Bullet>();
                        Ctm_Bllt_Own.transform.localScale = new Vector3(1,1,1);
                    // MapManager - Container //
                        Ctm_Bllt_Own.gameObject.transform.SetParent(I_Trfm_BulletContainer, true);
                        Ctm_Bllt_Own.transform.localScale = new Vector3(1,1,1);
                #endregion Spawn
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
                            true
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
                                    //MasterManager.Instance.BulletManager.SpawnBullet();
                            }
                        }
                #endregion Hashtables 
            #endregion RPC
        #endregion PUN
    #endregion Methods
}
