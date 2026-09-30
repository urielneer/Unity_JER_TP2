using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using static CustomExtension.ArrayExtensions;
public class Menu_ScoreBoard : BaseMenuElement
{
    #region    Variables
        [Header("Scoreboard Settings")]
            [SerializeField] MenuListContainerScoreBoard _container;
            MenuListContainerScoreBoard I_Ctm_MLCSB_Container;
            private ScoreBoardElement[] I_LCtmS_ScbE_A1_List = new ScoreBoardElement[1];
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Resume Base //
                    base.Awake();
                // Variables //
                    I_Ctm_MLCSB_Container = _container;
            }
            protected override void OnValidate()
            {
                // Resume Base //
                    base.OnValidate();
            }
        #endregion Unity Methods
        #region    Custom Methods
            // Game Manager //
                public void LeftRoom(string Str_Name) 
                { 
                    // Update //
                        GetComponent<PhotonView>().RPC(nameof(RPC_RemoveSelfFromMaster),RpcTarget.MasterClient, Str_Name);
                }
                public void JoinRoom()
                {
                }
                public void GameStart()
                {
                }
                public void GameEnd()
                {
                }
            // Element //
                public void UpdateScoreboard()
                {
                    // Update Own Room Prefab //
                        I_Ctm_MLCSB_Container.SetList(I_LCtmS_ScbE_A1_List.OfType<object>().ToList());             
                }
                public void IncreasePlayerKills(string Str_Player)
                {
                    GetComponent<PhotonView>().RPC(nameof(RPC_NotifyKillToMaster),RpcTarget.MasterClient, Str_Player);
                }
                public void IncreasePlayerDeaths(string Str_Player)
                {
                    GetComponent<PhotonView>().RPC(nameof(RPC_NotifyKillToMaster),RpcTarget.MasterClient, Str_Player);
                }
                public void UpdatePlayerPing(ScoreBoardSetupData CtmS_SCSD_Data)
                { 
                    GetComponent<PhotonView>().RPC(nameof(RPC_NotifyKillToMaster),RpcTarget.MasterClient, ((object) CtmS_SCSD_Data));
                }
                private void GetObjectList(out object[] Obj_A1_Output)
                { 
                    // Default //
                        Obj_A1_Output = null;
                    // If Empty //
                        if (this.gameObject.transform.childCount == 0) return;
                    // Proceed //
                        Transform[] Trns_A1_List = new Transform[this.gameObject.transform.childCount];
                        int Int_Index = 0;
                        foreach (Transform Trns_Temp in this.transform)
                        {
                            Trns_A1_List[Int_Index] = Trns_Temp;
                            Int_Index++;
                        }
                    // Result //
                        Obj_A1_Output = new object[] { Trns_A1_List };
                }
        #endregion Custom Methods
        #region    PUN
            #region    RPC
                [PunRPC]
                private void RPC_NotifyKillToMaster(string Str_Player)
                {
                    // Update //
                        I_Ctm_MLCSB_Container.IncreseKills(Str_Player);
                        I_Ctm_MLCSB_Container.Rearrange();
                    // Force Update Other Client's Scoreboard Prefab //
                        GetObjectList(out object[] Obj_A1_List);
                        if (Obj_A1_List != null) return;
                        GetComponent<PhotonView>().RPC(nameof(RPC_ForceUpdateListToClients),RpcTarget.All, Obj_A1_List); // ¿Por que object? PORQUE NO PARABA DE DAR FAIL Y ME VOLVIO LOCO!!!1! <3 //
                }
                [PunRPC]
                private void RPC_NotifyDeathToMaster(string Str_Player)
                {
                    // Update //
                        I_Ctm_MLCSB_Container.IncreseDeaths(Str_Player);
                        I_Ctm_MLCSB_Container.Rearrange();
                    // Force Update Other Client's Scoreboard Prefab //
                        GetObjectList(out object[] Obj_A1_List);
                        if (Obj_A1_List != null) return;
                        GetComponent<PhotonView>().RPC(nameof(RPC_ForceUpdateListToClients), RpcTarget.All, Obj_A1_List); // ¿Por que object? PORQUE NO PARABA DE DAR FAIL Y ME VOLVIO LOCO!!!1! <3 //
                }
                [PunRPC]
                private void RPC_NotifyPingToMaster(object CtmS_SCSD_Data)
                {
                    // Update //
                        I_Ctm_MLCSB_Container.UpdatePing(((ScoreBoardSetupData)CtmS_SCSD_Data).Name, ((ScoreBoardSetupData)CtmS_SCSD_Data).Ping);
                        I_Ctm_MLCSB_Container.Rearrange();
                    // Force Update Other Client's Scoreboard Prefab //
                        GetObjectList(out object[] Obj_A1_List);
                        if (Obj_A1_List != null) return;
                        GetComponent<PhotonView>().RPC(nameof(RPC_ForceUpdateListToClients),RpcTarget.All, Obj_A1_List); // ¿Por que object? PORQUE NO PARABA DE DAR FAIL Y ME VOLVIO LOCO!!!1! <3 //
                }
                [PunRPC]
                private void RPC_RemoveSelfFromMaster(string Str_Player)
                {
                    // Remove //
                        (I_Ctm_MLCSB_Container.FindTroughName(Str_Player)).RemoveElement();
                        I_Ctm_MLCSB_Container.Rearrange();
                    // Force Update Other Client's Scoreboard Prefab //
                        GetObjectList(out object[] Obj_A1_List);
                        if (Obj_A1_List != null) return;
                        GetComponent<PhotonView>().RPC(nameof(RPC_ForceUpdateListToClients),RpcTarget.All, Obj_A1_List); // ¿Por que object? PORQUE NO PARABA DE DAR FAIL Y ME VOLVIO LOCO!!!1! <3 //
                }
                [PunRPC]
                private void RPC_ForceUpdateListToClients(Transform[] Trns_A1_List)
                {
                    // If Empty //
                        if (Trns_A1_List.Length == 0) return;
                    // Update //
                        ScoreBoardElement[] CtmS_ScbE_A1_Data = new ScoreBoardElement[0];
                        foreach (Transform Trns_Temp in this.transform)
                        { 
                            if (Trns_Temp.TryGetComponent<MenuListElementScoreBoard>(out MenuListElementScoreBoard Ctm_MLSBE_DeleteMe)) 
                            {
                                AddNewToArray(ref CtmS_ScbE_A1_Data, Ctm_MLSBE_DeleteMe.Values);
                            }
                        }
                        I_LCtmS_ScbE_A1_List = CtmS_ScbE_A1_Data;
                        UpdateScoreboard();
                }
            #endregion RPC
        #endregion PUN
    #endregion Methods
}
