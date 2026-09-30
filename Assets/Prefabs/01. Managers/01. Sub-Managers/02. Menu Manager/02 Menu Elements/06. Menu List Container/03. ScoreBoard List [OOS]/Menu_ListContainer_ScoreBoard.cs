using Photon.Realtime;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using static CustomExtension.ArrayExtensions;

public class MenuListContainerScoreBoard : BaseMenuListContainer
{
    #region    Variables
        #region    Input
            [Space(10)]
            [Header("Score-Board List Container Settings")]
                [SerializeField] MenuListElementScoreBoard _prefab;
                MenuListElementScoreBoard Ctm_MLESB_Prefab;
        #endregion Input
        #region    Manager-To-Manager Data
            #pragma warning disable CS0108
                public List<ScoreBoardElement> List => (I_Lst_Obj_List.OfType<ScoreBoardElement>().ToList()); 
            #pragma warning restore CS0108
        #endregion Manager-To-Manager Data
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Resume Base //
                    base.Awake();
                // Variables //
                    Ctm_MLESB_Prefab = _prefab;
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
                Rearrange();
            }
            public override BaseMenuListElement SetUpElement(object Obj_Element) 
            { 
                // Variables //
                    MenuListElementScoreBoard Ctm_MLEP_Temp = Instantiate(Ctm_MLESB_Prefab, this.gameObject.transform);
                    ScoreBoardElement CtmS_ScbE_Temp = ((ScoreBoardElement) Obj_Element);
                // Add //
                    Ctm_MLEP_Temp.SetUp(null, CtmS_ScbE_Temp);
                    return Ctm_MLEP_Temp;
            }
            public override void CleanList()
            {
                // If Empty //
                    if (this.gameObject.transform.childCount == 0) return;
                // Empty Contents //
                    foreach (Transform Trns_Temp in this.transform)
                    { 
                        if (Trns_Temp.TryGetComponent<MenuListElementScoreBoard>(out MenuListElementScoreBoard Ctm_MLSBE_DeleteMe)) 
                        {
                            Ctm_MLSBE_DeleteMe.RemoveElement();
                        }
                        else Debug.Log("ERROR: Non Menu List Element Child found");
                    }
                    O_Ctm_BMLE_A1_List = new BaseMenuListElement[]{}; 
                    I_Lst_Obj_List = new List<object> { };
            }
        #endregion Override Methods
        #region    Custom Methods
            public void Rearrange()
            {
                // If Empty //
                    if (this.gameObject.transform.childCount == 0) return;
                // Variables //
                    SortedDictionary<int, Transform> SDic_Elements = new SortedDictionary<int, Transform>();
                // Sort //  
                    foreach (Transform Trns_Temp in this.transform)
                    { 
                        Trns_Temp.TryGetComponent<MenuListElementScoreBoard>(out MenuListElementScoreBoard Ctm_MLSBE_Temp);
                        int Int_Value = Ctm_MLSBE_Temp.Values.Kills - Ctm_MLSBE_Temp.Values.Deaths;
                        SDic_Elements.Add(Int_Value, Trns_Temp);
                    }
                // Rearrange //
                    foreach (Transform Trns_Temp in SDic_Elements.Values)
                    {
                        Trns_Temp.SetAsFirstSibling();
                    }
            }
            public MenuListElementScoreBoard FindTroughName(string Str_Name)
            {
                foreach (Transform Trns_Temp in this.transform)
                { 
                    Trns_Temp.TryGetComponent<MenuListElementScoreBoard>(out MenuListElementScoreBoard Ctm_MLSBE_Temp);
                    if (Ctm_MLSBE_Temp.Values.Name == Str_Name) return Ctm_MLSBE_Temp;
                }
                return null;
            }
            public void IncreseKills(string Str_Name)
            {
                MenuListElementScoreBoard Ctm_MLEP_Temp = FindTroughName(Str_Name);
                Ctm_MLEP_Temp.IncreaseKills();
            }
            public void IncreseDeaths(string Str_Name)
            {
                MenuListElementScoreBoard Ctm_MLEP_Temp = FindTroughName(Str_Name);
                Ctm_MLEP_Temp.IncreaseDeaths();
            }
            public void UpdatePing(string Str_Name, int Int_Ping)
            {
                MenuListElementScoreBoard Ctm_MLEP_Temp = FindTroughName(Str_Name);
                Ctm_MLEP_Temp.UpdatePing(Int_Ping);
            }
        #endregion Rearrange Methods
    #endregion Methods
}
