using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaseMenuButton : BaseMenuElement
{

    #region    Variables
        [Header("Base Button Settings")]
            [SerializeField] private Color _backgroundColor;
            private Color I_Clr_Background;
            [SerializeField] private Color[] _fontColor = {new Color(), new Color()};
            private Color[] I_Clr_A1_Font;
            [SerializeField] private TMP_Text _text;
            private TMP_Text I_TBtn_Text;
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Resume Base //
                    base.Awake();
                // Variables //
                    I_TBtn_Text = _text;
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
            public override void Overwrite(string Str_Input = "ERROR")
            {
                I_TBtn_Text = _text;
                I_TBtn_Text.text = Str_Input;
            }
            public override void RedrawElement()
            {
                // Resume Base //
                    base.RedrawElement();
                // Variables //
                    I_Clr_Background = _backgroundColor;
                    I_Clr_A1_Font = _fontColor;
                    I_TBtn_Text = _text;
                // Redraw //
                    SetColor(I_Clr_Background, I_Clr_A1_Font[0], I_Clr_A1_Font[1]);
            }
            public override void SetColor(Color Clr_ColorA, Color Clr_ColorB, Color Clr_ColorC = new Color()) // A: Background, B: Placeholder, C: Text //
            {
                this.gameObject.GetComponent<Image>().color = Clr_ColorA;
                I_TBtn_Text = _text;
                I_TBtn_Text.color = Clr_ColorB;
            }
            public void OverrideValues(Color Clr_ColorA, Color[] Clr_A1_ColorB)
            {
                    I_Clr_Background = Clr_ColorA;
                    I_Clr_A1_Font = Clr_A1_ColorB;
            }
        #endregion Override Methods
    #endregion Methods
}
