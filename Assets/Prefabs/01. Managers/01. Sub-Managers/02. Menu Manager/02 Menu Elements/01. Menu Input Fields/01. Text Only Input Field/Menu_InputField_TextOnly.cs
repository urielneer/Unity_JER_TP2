using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class MenuInputFieldTextOnly : BaseMenuInputField
{
    #region    Variables
        #region    Manager-To-Manager Data
            //private TMP_InputField O_IFld_Input;
            //public TMP_InputField InputField => O_IFld_Input;
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
                    O_IFld_Input.contentType = TMP_InputField.ContentType.Alphanumeric;
                // Listeners //        
                    AddSanitizeListener();
            }
            protected override void OnValidate()
            {
                // Resume Base //
                    base.OnValidate();
            }
            protected override void OnEnabled()
            {
                // Resume Base //
                    base.OnEnabled();
                // Listeners //    
                    AddSanitizeListener();
            }
            protected override void OnDisabled()
            {
                // Resume Base //
                    base.OnDisabled();
                // Listeners //    
                    RemoveSanitizeListener();
            }
        #endregion Unity Methods
        #region    Listener Methods
            #region    Caller Methods
                // Sanitize //
                    private void AddSanitizeListener()
                    {
                        O_IFld_Input.onEndEdit.RemoveListener(OnSanitizeInput);
                        O_IFld_Input.onEndEdit.AddListener(OnSanitizeInput);
                    }
                    private void RemoveSanitizeListener()
                    {
                        O_IFld_Input.onEndEdit.RemoveListener(OnSanitizeInput);
                    }
            #endregion Caller Methods
            #region    Callee Methods
                private void OnSanitizeInput(string Str_Input)
                {
                    string Str_Sanitized = Regex.Replace(Str_Input, @"[^a-zA-Z\s]", "");
                    if (Str_Sanitized != Str_Input)
                    {
                        O_IFld_Input.text = Str_Sanitized;
                    }
                }
            #endregion Callee Methods
        #endregion Listener Methods
        #region    Override Methods
            public override void RedrawElement()
            {
                // Resume Base //
                    base.RedrawElement();
            }
            public override void SetColor(Color Clr_ColorA, Color Clr_ColorB, Color Clr_ColorC) // A: Background, B: Placeholder, C: Text //
            {
                // Resume Base //
                    base.SetColor(Clr_ColorA, Clr_ColorB, Clr_ColorC);
            }
        #endregion Override Methods
        #region    Custom Methods
        #endregion Custom Methods
    #endregion Methods
}
