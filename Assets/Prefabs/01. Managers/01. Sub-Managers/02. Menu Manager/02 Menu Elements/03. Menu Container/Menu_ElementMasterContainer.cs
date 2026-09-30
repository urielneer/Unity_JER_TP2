using TMPro;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#region    ...
#endregion ...

public class ElementMasterContainer : BaseMenuElement
{
    #region    Variables
        #region    Appearance
            [Header("Base Element Master Container Settings")]
                [SerializeField] private Color _containerBackgroundColor;
                private Color I_Clr_ContainerBackground;
                [SerializeField] private Color _textTitleFontColor;
                private Color I_Clr_TextTitleFont;
                [SerializeField] private Color _textSubTitleFontColor;
                private Color I_Clr_TextSubTitleFont;
                [SerializeField] private Color _textBodyFontColor;
                private Color I_Clr_TextBodyFont;
                [SerializeField] private Color _inputFieldBackgroundColor;
                private Color I_Clr_InputFieldBackground;
                [SerializeField] private Color[] _inputFieldFontColor = {new Color(), new Color()};
                private Color[] I_Clr_A1_InputFieldFont;
                [SerializeField] private Color _buttonBackground;
                private Color I_Clr_ButtonBackground;
                [SerializeField] private Color _buttonFont;
                private Color I_Clr_ButtonFont;
                [SerializeField] private string _titleName;
                private string I_Str_TitleName;
                [SerializeField] private GameObject _elementContainer;
                private GameObject I_GObj_ElementContainer;
                [SerializeField] private TMP_Text _title;
                private TMP_Text I_TTxt_Title;
                [SerializeField] private BaseMenuElement[] _elementList;
                private BaseMenuElement[] I_Ctm_BME_A1_ElementList;
        #endregion Appearance
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Resume Base //
                    base.Awake();
                // Redraw //
                    RedrawElement();
            }
            protected override void OnValidate()
            {
                // Resume Base //
                    base.OnValidate();
                // Redraw //
                    RedrawElement();
            }
        #endregion Unity Methods
        #region    Override Methods
            public override void RedrawElement()
            {
                // Resume Base //
                    base.RedrawElement();
                // Variables //
                    I_Clr_ContainerBackground = _containerBackgroundColor;
                    I_Clr_TextTitleFont = _textTitleFontColor;
                    I_Clr_TextSubTitleFont = _textSubTitleFontColor;
                    I_Clr_TextBodyFont = _textBodyFontColor;
                    I_Clr_InputFieldBackground = _inputFieldBackgroundColor;
                    I_Clr_A1_InputFieldFont = _inputFieldFontColor;
                    I_Clr_ButtonBackground = _buttonBackground;
                    I_Clr_ButtonFont = _buttonFont;
                    I_Str_TitleName = _titleName;
                    I_GObj_ElementContainer = _elementContainer;
                    I_TTxt_Title = _title;
                    I_Ctm_BME_A1_ElementList = _elementList;
                // Redraw //
                    this.gameObject.GetComponent<Image>().color = _containerBackgroundColor;
                    I_TTxt_Title.color = _textTitleFontColor;
                    I_TTxt_Title.text = I_Str_TitleName;
                    foreach (BaseMenuElement Ctm_BME_Element in I_Ctm_BME_A1_ElementList)
                    {
                        RepaintElement(Ctm_BME_Element);
                    }            
            }
        #endregion Override Methods
        #region    Custom Methods
            private void RepaintElement(BaseMenuElement Ctm_BME_Element)
            {
                switch (Ctm_BME_Element)
                {
                    case BaseMenuLabel Mlbl_Cast:
                        Mlbl_Cast.SetMasterContainer(this.gameObject);
                        Mlbl_Cast.SetColor(I_Clr_TextSubTitleFont, I_Clr_TextBodyFont);
                    break;
                    case BaseMenuInputField Ctm_BMIF_Cast:
                        Ctm_BMIF_Cast.SetMasterContainer(this.gameObject);
                        Ctm_BMIF_Cast.SetColor(I_Clr_InputFieldBackground, I_Clr_A1_InputFieldFont[0], I_Clr_A1_InputFieldFont[1]);
                    break;
                    case BaseMenuButton Ctm_BMB_Cast:
                        Ctm_BMB_Cast.SetMasterContainer(this.gameObject);
                        Ctm_BMB_Cast.SetColor(I_Clr_ButtonBackground, I_Clr_ButtonFont);
                    break;
                    case BaseMenuScrollbar Ctm_BMS_Cast:
                    break;
                }
            }
            public void AddNewElement(BaseMenuElement Ctm_BME_Element)
            { 
                CustomExtension.ArrayExtensions.AddNewToArray(ref I_Ctm_BME_A1_ElementList, Ctm_BME_Element);
                RepaintElement(Ctm_BME_Element);
            }
            public void RemoveElement(BaseMenuElement Ctm_BME_Remove)
            {
                CustomExtension.ArrayExtensions.RemoveSpecificFromArray(ref I_Ctm_BME_A1_ElementList, Ctm_BME_Remove);
            }
        #endregion Custom Methods
    #endregion Methods
}