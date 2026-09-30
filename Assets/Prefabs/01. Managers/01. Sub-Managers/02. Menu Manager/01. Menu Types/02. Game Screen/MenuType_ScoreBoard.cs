using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using static CustomExtension.ArrayExtensions;

public class MenuTypeScoreBoard : BaseMenuType
{
    #region    Variables
        [Header("Score-Board GUI Settings")]
            [SerializeField] private Menu_ScoreBoard _scoreBoard;
            private Menu_ScoreBoard IO_Ctm_MSB_ScoreBoard;  
            public Menu_ScoreBoard ScoreBoard => IO_Ctm_MSB_ScoreBoard;
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Resume Base //
                    base.Awake();
                // Variables //
                    IO_Ctm_MSB_ScoreBoard = _scoreBoard;
            }
        #endregion Unity Methods
        #region    Custom Methods
            public void LeftRoom()
            {
                IO_Ctm_MSB_ScoreBoard.LeftRoom(PhotonNetwork.NickName);
            }
            public void JoinRoom()
            {
                IO_Ctm_MSB_ScoreBoard.JoinRoom();
            }
            public void GameStart()
            {
                IO_Ctm_MSB_ScoreBoard.GameStart();
            }
            public void GameEnd()
            {
                IO_Ctm_MSB_ScoreBoard.GameEnd();
            }
        #endregion Custom Methods
    #endregion Methods
}