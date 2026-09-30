using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class BaseMenuListElement : BaseMenuElement
{
    #region    Variables
        [Header("Base List Element Settings")]
            [SerializeField] Color[] _fontColor = {new Color(), new Color()};
            protected static Color[] I_Clr_A1_Font;
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Resume Base //
                    base.Awake();
                // Variables //
                    if (MasterContainer == null) RedrawElement();
            }
            protected override void OnValidate()
            {
                // Resume Base //
                    base.OnValidate();
                // Variables //
                    if (MasterContainer == null) RedrawElement();
            }
        #endregion Unity Methods
        #region    Override Methods
            public override void RedrawElement()
            {
                // Resume Base //
                    base.RedrawElement();
                // Variables //
                    I_Clr_A1_Font = _fontColor;
            }
            public override void SetColor(Color Clr_ColorA, Color Clr_ColorB, Color Clr_ColorC = new Color()) {}
        #endregion Override Methods
        #region    Custom Methods
            public virtual BaseMenuElement SetUp(GameObject GObj_Master, object Obj_Element)
            {   
                // Values //
                    SetMasterContainer(GObj_Master);
                    SetElementText(Obj_Element);
                // Appearance //
                    RedrawElement();
                return this;
            }
            public virtual void RemoveElement() {}
            protected virtual void SetElementText(object Obj_Element) {}
        #endregion Custom Methods
    #endregion Methods
}
