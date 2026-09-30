using Photon.Realtime;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MenuListContainerRoom : BaseMenuListContainer
{
    #region    Variables
        [Header("Room List Container Settings")]
            protected static List<RoomInfo> I_Lst_PUNRI_RoomList = null;
            [SerializeField] MenuListElementRoom _prefab;
            MenuListElementRoom Ctm_MLER_Prefab;
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Resume Base //
                    base.Awake();
                // Variables //
                    Ctm_MLER_Prefab = _prefab;
            }
            protected override void OnValidate()
            {
                // Resume Base //
                    base.OnValidate();
                // Variables //
            }
        #endregion Unity Methods
        #region    Override Methods
            protected override void SetListVariables(List<object> Lst_Obj_Input)
            {
                I_Lst_Obj_List = Lst_Obj_Input;
                I_Lst_PUNRI_RoomList = Lst_Obj_Input.OfType<RoomInfo>().ToList();
            }
            public override void SetList(List<object> Lst_Obj_Input = null)
            {
                SetListVariables(Lst_Obj_Input);
                base.SetList(Lst_Obj_Input);
            }
            public override BaseMenuListElement SetUpElement(object Obj_Element) 
            { 
                // Variables //
                    MenuListElementRoom Ctm_MLER_Temp = Instantiate(Ctm_MLER_Prefab, this.gameObject.transform).GetComponent<MenuListElementRoom>();
                    RoomInfo PUNRI_Temp = (RoomInfo) Obj_Element;
                // Add //
                    Ctm_MLER_Temp.SetUp(MasterContainer.gameObject, PUNRI_Temp);
                    return Ctm_MLER_Temp;
            }
        #endregion Override Methods
    #endregion Methods
}
