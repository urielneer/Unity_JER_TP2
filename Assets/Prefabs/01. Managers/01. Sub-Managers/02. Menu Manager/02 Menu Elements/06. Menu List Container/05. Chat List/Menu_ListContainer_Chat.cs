using Photon.Realtime;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MenuListContainerChat : BaseMenuListContainer
{
    #region    Variables
        [Header("Chat List Container Settings")]
            [SerializeField] MenuListElementChat _prefab;
            MenuListElementChat Ctm_MLEP_Prefab;
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
                    MenuListElementChat Ctm_MLEP_Temp = Instantiate(Ctm_MLEP_Prefab, this.gameObject.transform);
                    string Str_Temp = (string) Obj_Element;
                // Add //
                    Ctm_MLEP_Temp.SetUp(null, Str_Temp);
                    return Ctm_MLEP_Temp;
            }
            public override void CleanList()
            {
                // If Empty //
                    if (this.gameObject.transform.childCount == 0) return;
                // Empty Contents //
                    foreach (Transform Trns_Temp in this.transform)
                    { 
                        if (Trns_Temp.TryGetComponent<BaseMenuListElement>(out BaseMenuListElement Ctm_BMLE_DeleteMe)) 
                        {
                            Destroy(Trns_Temp.gameObject);
                        }
                        else Debug.Log("ERROR: Non Menu List Element Child found");
                    }
                    O_Ctm_BMLE_A1_List = new BaseMenuListElement[]{}; 
                    I_Lst_Obj_List = new List<object> { };
            }
        #endregion Override Methods
    #endregion Methods
}
