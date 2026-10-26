using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using static CustomExtension.ArrayExtensions;

public class MenuTypeHUD : BaseMenuType
{
    #region    Variables
        [Header("HUD GUI Settings")]
            [SerializeField] private Menu_ChatElement _chat;
            private Menu_ChatElement IO_Ctm_MCE_Chat;   
            public  Menu_ChatElement ChatElement => IO_Ctm_MCE_Chat;
            [SerializeField] private Aim _aim;
            private Aim R_Aim_Cursor;
            public Aim AimCursor => R_Aim_Cursor;
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Resume Base //
                    base.Awake();
                // Variables //
                    IO_Ctm_MCE_Chat = _chat;
                    R_Aim_Cursor = _aim;
            }
            #pragma warning disable CS1998
            public async void OnUpdate(float Flt_FixedDT)
            #pragma warning restore CS1998
            {
                R_Aim_Cursor.OnUpdate(Flt_FixedDT);
            }
        #endregion Unity Methods
        #region    Custom Methods
            #region    Menu Methods
            #endregion Menu Methods
            #region    Room Methods
                public void LeftRoom()
                {
                    IO_Ctm_MCE_Chat.LeftRoom();
                }
                public void JoinRoom()
                {
                    IO_Ctm_MCE_Chat.JoinRoom();
                }
                public void GameStart()
                {
                    IO_Ctm_MCE_Chat.GameStart();
                }
                public void GameEnd()
                {
                    IO_Ctm_MCE_Chat.GameEnd();
                }
            #endregion Room Methods
        #endregion Custom Methods
    #endregion Methods
}