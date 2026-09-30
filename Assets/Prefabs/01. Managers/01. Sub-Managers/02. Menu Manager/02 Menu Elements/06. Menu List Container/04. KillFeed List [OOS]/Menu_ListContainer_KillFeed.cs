using Photon.Realtime;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MenuListContainerKillFeed : BaseMenuListContainer
{
    #region    Variables
        [Header("Player List Container Settings")]
            [SerializeField] MenuListElementKillFeed _prefab;
            MenuListElementKillFeed Ctm_MLEP_Prefab;
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Resume Base //
                    base.Awake();
                // Variables //
                    Ctm_MLEP_Prefab = _prefab;
            }
            protected override void OnValidate()
            {
                // Resume Base //
                    base.OnValidate();
                // Variables //
            }
        #endregion Unity Methods
        #region    Custom Methods
            // Game Manager //
                public void LeftRoom()
                {
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
        #endregion Custom Methods
        #region    Override Methods
            protected override void SetListVariables(List<object> Lst_Obj_Input)
            {
                I_Lst_Obj_List = Lst_Obj_Input;
            }
            public override void SetList(List<object> Lst_Obj_Input = null)
            {
                SetListVariables(Lst_Obj_Input);
                base.SetList(Lst_Obj_Input);
            }
            public override BaseMenuListElement SetUpElement(object Obj_Element) 
            {
                // Variables //
                    MenuListElementKillFeed Ctm_MLEP_Temp = Instantiate(Ctm_MLEP_Prefab, this.gameObject.transform);
                    string Str_Temp = (string) Obj_Element;
                // Add //
                    Ctm_MLEP_Temp.SetUp(null, Str_Temp);
                    return Ctm_MLEP_Temp;
            }
        #endregion Override Methods
    #endregion Methods
}
