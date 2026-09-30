using Photon.Realtime;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class BaseMenuListContainer :  BaseMenuElement
{

    #region    Variables
            protected static BaseMenuListElement Ctm_LsE_Prefab;
            protected static List<object> I_Lst_Obj_List = new List<object>{};
        #region    Manager-To-Manager Data
            protected static BaseMenuListElement[] O_Ctm_BMLE_A1_List = new BaseMenuListElement[] { };
            public static BaseMenuListElement[] List => O_Ctm_BMLE_A1_List;
        #endregion Manager-To-Manager Data
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Resume Base //
                    base.Awake();
                // Variables //
            }
            protected override void OnValidate()
            {
                // Resume Base //
                    base.OnValidate();
                // Variables //
            }
        #endregion Unity Methods
        #region    Override Methods
            protected virtual void SetListVariables(List<object> Lst_Obj_Input) {}
            public virtual void SetList(List<object> Lst_Obj_Input = null)
            {
                CleanList();
                I_Lst_Obj_List = Lst_Obj_Input;
                UpdateList();
            }
            public virtual void UpdateList()
            {
                foreach (object Obj_Element in I_Lst_Obj_List)
                {
                    AddListItem(Obj_Element);
                }
            }
            public virtual BaseMenuListElement SetUpElement(object Obj_Element) { return null; }
            public virtual void AddListItem(object Obj_Element)
            {
                // Variables //
                    BaseMenuListElement Ctm_BMLE_Temp = SetUpElement(Obj_Element);
                // Add //
                    CustomExtension.ArrayExtensions.AddNewToArray(ref O_Ctm_BMLE_A1_List, Ctm_BMLE_Temp);
            }
            public virtual void CleanList()
            {
                // If Empty //
                    if (this.gameObject.transform.childCount == 0) return;
                // Empty Contents //
                    foreach (Transform Trns_Temp in this.transform)
                    { 
                        if (Trns_Temp.TryGetComponent<BaseMenuListElement>(out BaseMenuListElement Ctm_BMLE_DeleteMe)) 
                        {
                            MasterContainer.RemoveElement(Ctm_BMLE_DeleteMe);
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
