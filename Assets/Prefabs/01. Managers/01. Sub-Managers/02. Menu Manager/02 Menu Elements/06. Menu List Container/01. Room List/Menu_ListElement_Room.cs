using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class MenuListElementRoom : BaseMenuListElement
{
    #region    Variables
        [Space(10)]
        [Header("Room List Element Settings")]
            [SerializeField] Color _backgroundColor;
            protected static Color I_Clr_Background;
            [SerializeField] BaseMenuButton _button;
            protected static BaseMenuButton I_BMB_Element;
            protected static RoomInfo PUNRI_Room_Value;
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Variables //
                    I_BMB_Element = _button;
                // Resume Base //
                    base.Awake();
            }
            protected override void OnValidate()
            {
                // Variables //
                    I_BMB_Element = _button;
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
                    RoomInfo PUNRI_RoomList = (RoomInfo) Obj_Element;
                    if (PUNRI_RoomList.PlayerCount == 0)
                    { 
                        this.gameObject.SetActive(false);
                        RemoveElement();
                    }
                    MasterContainer.AddNewElement(I_BMB_Element);
                // Network Manager - Runtime Dynamic "OnClick" //
                    I_BMB_Element.GetComponent<Button>().onClick.RemoveAllListeners();
                    I_BMB_Element.GetComponent<Button>().onClick.AddListener(JoinRoom);
                return this;
            }
            public override void SetMasterContainer(GameObject GObj_Master) 
            {
                // Variables //
                    MasterContainer = GObj_Master.GetComponent<ElementMasterContainer>();
                    I_BMB_Element = _button;
                // Set Master //
                    I_BMB_Element.SetMasterContainer(GObj_Master);
            }
            public override void RedrawElement()
            {
                // Variables //
                    I_BMB_Element = _button;
                    I_Clr_Background = _backgroundColor;
                // Resume Base //
                    base.RedrawElement();
                // Redraw //
                    I_BMB_Element.OverrideValues(I_Clr_Background,  I_Clr_A1_Font);
                    I_BMB_Element.RedrawElement();
                    I_BMB_Element.SetColor(I_Clr_Background, I_Clr_A1_Font[0], I_Clr_A1_Font[1]);
            }
            public override void Overwrite(string Str_Input)
            {
                I_BMB_Element.Overwrite(Str_Input);
            }
            public override void SetColor(Color Clr_ColorA, Color Clr_ColorB, Color Clr_ColorC = new Color()) // A: Background, B: Placeholder, C: Text //
            {
                I_BMB_Element.SetColor(Clr_ColorA, Clr_ColorB, Clr_ColorC);
            }
            public override void RemoveElement()
            {
                MasterContainer.RemoveElement(I_BMB_Element);
            }
            protected override void SetElementText(object Obj_Element)
            {
                RoomInfo PUNRI_RoomList = (RoomInfo) Obj_Element;
                Overwrite("("+(PUNRI_RoomList.PlayerCount).ToString("D2")+"/"+(PUNRI_RoomList.MaxPlayers).ToString("D2")+") \""+PUNRI_RoomList.Name+"\"");
                PUNRI_Room_Value = PUNRI_RoomList;
            }
        #endregion Override Methods
        #region    Custom Methods
            public virtual void JoinRoom()
            {
                MasterManager.Instance.NetworkManager.JoinRoom(PUNRI_Room_Value);
            }
        #endregion Custom Methods
    #endregion Methods
}
