using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseMenuLabel : BaseMenuElement
{
    #region    Variables
        [Header("Base Label Settings")]
            [SerializeField] bool _isSubtitle = false;
            bool bool_IsSubtitle;
            [SerializeField] private Color[] _fontColor = {new Color(), new Color()};
            private Color[] I_Clr_A1_Font;
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
            public override void Overwrite(string Str_Input)
            { 
                this.gameObject.GetComponent<TMP_Text>().text = Str_Input;
            }
            public override void RedrawElement()
            {
                // Resume Base //
                    base.RedrawElement();
                // Variables //
                    bool_IsSubtitle = _isSubtitle;
                    I_Clr_A1_Font = _fontColor;
                // Redraw //
                    SetColor(I_Clr_A1_Font[0], I_Clr_A1_Font[1]);
            }
            public override void SetColor(Color Clr_ColorA, Color Clr_ColorB, Color Clr_ColorC = new Color()) // A: Subtitle, B: Body //
            {
                this.gameObject.GetComponent<TMP_Text>().color = (bool_IsSubtitle) ?
                                                                    Clr_ColorA:
                                                                    Clr_ColorB;
            }
        #endregion Override Methods
    #endregion Methods
}
