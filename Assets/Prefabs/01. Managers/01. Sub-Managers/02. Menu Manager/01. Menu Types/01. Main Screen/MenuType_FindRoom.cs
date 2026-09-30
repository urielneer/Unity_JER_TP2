using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CustomExtension.ArrayExtensions;

public class MenuTypeFindRoom : BaseMenuType
{
    #region    Variables
        [Header("Find Room GUI Settings")]
            [SerializeField] MenuListContainerRoom _roomList;
            MenuListContainerRoom I_Ctm_MRLC_Rooms;
            [SerializeField] GameObject _masterContainer;
            GameObject I_GObj_Master;
        #region    Manager-To-Manager Data
            private List<RoomInfo> O_Lst_PUNRI_ActiveRooms;
            public List<RoomInfo> ActiveRooms => O_Lst_PUNRI_ActiveRooms;
        #endregion Manager-To-Manager Data
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Resume Base //
                    base.Awake();
                // Variables //
                    I_Ctm_MRLC_Rooms = _roomList;
                    I_GObj_Master = _masterContainer;
            }
        #endregion Unity Methods
        #region    Custom Methods
            public void RemoveRoomFromList(RoomInfo PUNRI_Room = null)
            {
                // Variables //
                    // Current Room is the default //
                        if (PUNRI_Room == null) PUNRI_Room = PhotonNetwork.CurrentRoom;
                // Remove //
                    RoomInfo[] PUNRI_A1_ActiveRooms = O_Lst_PUNRI_ActiveRooms.ToArray();
                    RemoveSpecificFromArray(ref PUNRI_A1_ActiveRooms, PUNRI_Room);
                    O_Lst_PUNRI_ActiveRooms = PUNRI_A1_ActiveRooms.OfType<RoomInfo>().ToList();
                // Uppdate //
                    ClearRoomList();
                    UpdateList(O_Lst_PUNRI_ActiveRooms);
            }
            public void UpdateList(List<RoomInfo> Lst_PUNRI_RoomList)
            {
                // Variables //
                    O_Lst_PUNRI_ActiveRooms = Lst_PUNRI_RoomList;
                // Update //
                    I_Ctm_MRLC_Rooms.SetMasterContainer(I_GObj_Master);
                    I_Ctm_MRLC_Rooms.SetList(Lst_PUNRI_RoomList.Cast<object>().ToList());
            }
            public void ClearRoomList()
            {
                I_Ctm_MRLC_Rooms.CleanList();
            }
        #endregion Custom Methods
    #endregion Methods
}

