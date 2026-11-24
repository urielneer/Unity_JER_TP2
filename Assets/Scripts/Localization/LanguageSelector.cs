using UnityEngine;
using UnityEngine.UI;

// Selector de idioma ES / EN (arriba a la derecha de la pantalla de titulo) //
// Se arma por codigo: no hay que tocar la escena ni el prefab del menu. Lo muestra/oculta el MenuManager //
public class LanguageSelector : MonoBehaviour
{
    #region    Constants
        private const int SORTING_ORDER = 100;                  // Arriba del menu, debajo de los overlays de conexion //
        private static readonly Vector2 Vec2_ButtonSize = new Vector2(110f, 64f);
        private static readonly Vector2 Vec2_Margin     = new Vector2(30f, 30f);
        private const float BUTTON_SPACING = 12f;
        private const int FONT_SIZE = 34;
        private static readonly Color Clr_Idle      = new Color(0.10f, 0.08f, 0.06f, 0.75f);
        private static readonly Color Clr_Selected  = new Color(0.62f, 0.40f, 0.16f, 1.00f);
        private static readonly Color Clr_TextIdle  = new Color(0.80f, 0.78f, 0.74f, 1.00f);
        private static readonly Color Clr_TextOn    = Color.white;
    #endregion Constants
    #region    Variables
        private static LanguageSelector R_Ctm_LS_Instance;
        private Canvas R_Cnv_Root;
        private LanguageButton[] R_A1_Buttons;
        // Un boton por idioma - Codigo + referencias para pintarlo //
        private struct LanguageButton
        {
            public string Code;
            public Image Background;
            public RuntimeUIFactory.Label Label;
        }
    #endregion Variables
    #region    Methods
        #region    API
            public static void SetVisible(bool Bool_Visible)
            {
                if (!Bool_Visible && R_Ctm_LS_Instance == null) return; // No se crea solo para ocultarlo //
                LanguageSelector Ctm_LS_Selector = GetInstance();
                Ctm_LS_Selector.R_Cnv_Root.gameObject.SetActive(Bool_Visible);
                if (Bool_Visible) Ctm_LS_Selector.RefreshSelection();
            }
        #endregion API
        #region    Unity Methods
            private void OnEnable()  { GameLocalization.Service.OnLanguageChanged += RefreshSelection; }
            private void OnDisable() { GameLocalization.Service.OnLanguageChanged -= RefreshSelection; }
        #endregion Unity Methods
        #region    Internals
            private static LanguageSelector GetInstance()
            {
                if (R_Ctm_LS_Instance != null) return R_Ctm_LS_Instance;
                GameObject GObj_Root = new GameObject("LanguageSelector");
                DontDestroyOnLoad(GObj_Root);
                R_Ctm_LS_Instance = GObj_Root.AddComponent<LanguageSelector>();
                R_Ctm_LS_Instance.Build();
                return R_Ctm_LS_Instance;
            }
            private void Build()
            {
                R_Cnv_Root = RuntimeUIFactory.CreateOverlayCanvas(this.transform, SORTING_ORDER);
                // De derecha a izquierda: EN queda en la esquina, ES a su izquierda //
                    string[] Str_A1_Codes = { GameLocalization.ENGLISH, GameLocalization.SPANISH };
                    R_A1_Buttons = new LanguageButton[Str_A1_Codes.Length];
                    for (int Int_Index = 0; Int_Index < Str_A1_Codes.Length; Int_Index++)
                        R_A1_Buttons[Int_Index] = CreateLanguageButton(Str_A1_Codes[Int_Index], Int_Index);
                R_Cnv_Root.gameObject.SetActive(false);
            }
            private LanguageButton CreateLanguageButton(string Str_Code, int Int_SlotFromRight)
            {
                // Boton //
                    Button Btn_New = RuntimeUIFactory.CreateButton(R_Cnv_Root.transform, "Lang_" + Str_Code, Vec2_ButtonSize, Clr_Idle, FONT_SIZE, out RuntimeUIFactory.Label Ctm_Lbl_Text);
                    Ctm_Lbl_Text.SetText(Str_Code.ToUpperInvariant());
                    Btn_New.onClick.AddListener(() => GameLocalization.Service.SetLanguage(Str_Code));
                // Ubicacion - Esquina superior derecha //
                    RectTransform Rct_Button = (RectTransform) Btn_New.transform;
                    Rct_Button.anchorMin = Vector2.one;
                    Rct_Button.anchorMax = Vector2.one;
                    Rct_Button.pivot = Vector2.one;
                    float Flt_OffsetX = Vec2_Margin.x + Int_SlotFromRight * (Vec2_ButtonSize.x + BUTTON_SPACING);
                    Rct_Button.anchoredPosition = new Vector2(-Flt_OffsetX, -Vec2_Margin.y);
                return new LanguageButton { Code = Str_Code, Background = (Image) Btn_New.targetGraphic, Label = Ctm_Lbl_Text };
            }
            private void RefreshSelection()
            {
                if (R_A1_Buttons == null) return;
                string Str_Current = GameLocalization.Service.CurrentLanguageCode;
                for (int Int_Index = 0; Int_Index < R_A1_Buttons.Length; Int_Index++)
                {
                    bool Bool_IsSelected = R_A1_Buttons[Int_Index].Code == Str_Current;
                    R_A1_Buttons[Int_Index].Background.color = Bool_IsSelected ? Clr_Selected : Clr_Idle;
                    R_A1_Buttons[Int_Index].Label.SetColor(Bool_IsSelected ? Clr_TextOn : Clr_TextIdle);
                }
            }
        #endregion Internals
    #endregion Methods
}
