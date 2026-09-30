using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseMenuInputField : BaseMenuElement
{
    #region    Variables
        [Header("Base Input Settings")]
            [SerializeField] private Color _backgroundColor;
            private Color I_Clr_Background;
            [SerializeField] private Color[] _fontColor = {new Color(), new Color()};
            private Color[] I_Clr_A1_Font;
        #region    Manager-To-Manager Data
            protected TMP_InputField O_IFld_Input;
            public TMP_InputField InputField => O_IFld_Input;
        #endregion Manager-To-Manager Data
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Resume Base //
                    base.Awake();
                // Variables //
                    O_IFld_Input = this.gameObject.GetComponent<TMP_InputField>();
                    O_IFld_Input.contentType = TMP_InputField.ContentType.Standard;
                    if (MasterContainer == null) RedrawElement();
                // Listeners //    
                    //AddPlayerControllerListener();
            }
            protected override void OnValidate()
            {
                // Resume Base //
                    base.OnValidate();
                // Variables //
                    if (MasterContainer == null) RedrawElement();
            }
            protected override void OnEnabled()
            {
                // Resume Base //
                    base.OnEnabled();
            }
            protected override void OnDisabled()
            {
                // Resume Base //
                    base.OnDisabled();
            }
        #endregion Unity Methods
        #region    Override Methods
            public override void Overwrite(string Str_Input)
            {
                base.Overwrite(Str_Input);
                O_IFld_Input.text = Str_Input;
            }
            public override void RedrawElement()
            {
                // Resume Base //
                    base.RedrawElement();
                // Variables //
                    I_Clr_Background = _backgroundColor;
                    I_Clr_A1_Font = _fontColor;
                // Redraw //
                    SetColor(I_Clr_Background, I_Clr_A1_Font[0], I_Clr_A1_Font[1]);
            }
            public override void SetColor(Color Clr_ColorA, Color Clr_ColorB, Color Clr_ColorC) // A: Background, B: Placeholder, C: Text //
            {
                this.gameObject.GetComponent<Image>().color = Clr_ColorA;
                this.gameObject.GetComponent<TMP_InputField>().placeholder.color = Clr_ColorB;
                this.gameObject.GetComponent<TMP_InputField>().textComponent.color = Clr_ColorC;
            }
        #endregion Override Methods
    #endregion Methods
}
