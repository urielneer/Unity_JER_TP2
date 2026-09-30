using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class MenuListElementPlayer : BaseMenuListElement
{
    #region    Variables
        [Space(10)]
        [Header("Room List Element Settings")]
            [SerializeField] BaseMenuLabel _label;
            protected static BaseMenuLabel I_BML_Element;
            [SerializeField] Button _button;
            protected static Button I_Btn_Button;
            public Button Button => I_Btn_Button;
            protected static string I_str_Value;
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Variables //
                    I_BML_Element = _label;
                    I_Btn_Button = _button;
                // Resume Base //
                    base.Awake();
            }
            protected override void OnValidate()
            {
                // Variables //
                    I_BML_Element = _label;
                // Resume Base //
                    base.OnValidate();
            }
        #endregion Unity Methods
        #region    Override Methods
            public override BaseMenuElement SetUp(GameObject GObj_Master, object Obj_Element)
            {   
                // Resume Base //
                    base.SetUp(GObj_Master, Obj_Element);
                // Appearance //
                    MasterContainer.AddNewElement(I_BML_Element);
                    Button.image.color = Color.white;
                return this;
            }
            public override void SetMasterContainer(GameObject GObj_Master) 
            {
                // Variables //
                    MasterContainer = GObj_Master.GetComponent<ElementMasterContainer>();
                    I_BML_Element = _label;
                // Set Master //
                    I_BML_Element.SetMasterContainer(GObj_Master);
            }
            public override void RedrawElement()
            {
                // Variables //
                    I_BML_Element = _label;
                // Resume Base //
                    base.RedrawElement();
                // Redraw //
                    I_BML_Element.RedrawElement();
                    I_BML_Element.SetColor(I_Clr_A1_Font[0], I_Clr_A1_Font[1]);
            }
            public override void Overwrite(string Str_Input)
            {
                I_BML_Element.Overwrite(Str_Input);
            }
            public override void SetColor(Color Clr_ColorA, Color Clr_ColorB, Color Clr_ColorC = new Color()) // A: Background, B: Placeholder, C: Text //
            {
                I_BML_Element.SetColor(Clr_ColorA, Clr_ColorB);
            }
            public override void RemoveElement()
            {
                MasterContainer.RemoveElement(I_BML_Element);
            }
            protected override void SetElementText(object Obj_Element)
            {
                Overwrite((string)Obj_Element);
                I_str_Value = (string)Obj_Element;
            }
        #endregion Override Methods
        #region    Custom Methods
            public void ButtonColor(bool Bool_IsVigilant)
            {
                if (!Bool_IsVigilant)
                    Button.image.color = Color.white;
                else
                    Button.image.color = Color.red;
            }
            public void ChangeVigilant()
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    // Force Update Other Client's Room Prefab //
                        (GameObject.Find("WaitingRoom")).GetComponent<MenuTypeWaitingRoom>().ChangeVigilant(transform.GetChild(0).GetComponent<TMP_Text>().text);
                }
            }
        #endregion Custom Methods
    #endregion Methods
}
