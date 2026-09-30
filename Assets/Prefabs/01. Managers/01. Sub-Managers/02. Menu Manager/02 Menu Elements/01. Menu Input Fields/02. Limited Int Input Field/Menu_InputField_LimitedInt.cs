using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MenuInputFieldLimitedInt : BaseMenuInputField
{
    #region    Variables  
        [Space(10)]  
        [Header("Limited Int Input Settings")]
            [SerializeField] private int _minValue;
            private int I_Int_MinValue;
            public int Min => I_Int_MinValue;
            [SerializeField] private int _maxValue;
            private int I_Int_MaxValue;
            public int Max => I_Int_MaxValue;
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
                    I_Int_MinValue = _minValue;
                    I_Int_MaxValue = _maxValue;
                    O_IFld_Input = this.gameObject.GetComponent<TMP_InputField>();
                    O_IFld_Input.contentType = TMP_InputField.ContentType.IntegerNumber;
                // Listeners //
                    AddClampListener();
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
                    AddClampListener();
            }
            protected override void OnDisabled()
            {
                // Resume Base //
                    base.OnDisabled();
                // Listeners //    
                    RemoveClampListener();
            }
        #endregion Unity Methods
        #region    Listener Methods
            #region    Caller Methods
                // Clamp //
                    private void AddClampListener()
                    {
                        O_IFld_Input.onEndEdit.RemoveListener(OnClampValue);
                        O_IFld_Input.onEndEdit.AddListener(OnClampValue);
                    }
                    private void RemoveClampListener()
                    {
                        O_IFld_Input.onEndEdit.RemoveListener(OnClampValue);
                    }
            #endregion Caller Methods
            #region    Callee Methods
                private void OnClampValue(string Str_Input)
                {
                    if (string.IsNullOrEmpty(Str_Input))
                    {           
                        O_IFld_Input.text = I_Int_MinValue.ToString();
                        return;
                    }
                    if (int.TryParse(Str_Input, out int Int_Parsed))
                    {
                        O_IFld_Input.text = Mathf.Clamp(Int_Parsed, I_Int_MinValue, I_Int_MaxValue).ToString();
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
