using UnityEngine;

// Indicador de voz: "HABLANDO..." abajo (jugador local) y "RIVAL HABLANDO" arriba (camara fija: el rival esta enfrente) //
// Se arma por codigo con RuntimeUIFactory y sobrevive a los cambios de escena //
public class VoiceActivityIndicator : MonoBehaviour
{
    #region    Constants
        private const int SORTING_ORDER = 30000; // Arriba del HUD, debajo de ConnectionStatusOverlay //
        private static readonly Color Clr_Local  = new Color(0.55f, 0.95f, 0.60f, 1f);
        private static readonly Color Clr_Remote = new Color(1.00f, 0.80f, 0.45f, 1f);
        private static readonly Color Clr_Back   = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Vector2 Vec2_TagSize = new Vector2(380f, 56f);
        private static readonly Vector2 Vec2_Margin  = new Vector2(30f, 30f);
    #endregion Constants
    #region    Variables
        private static VoiceActivityIndicator R_Ctm_VAI_Instance;
        private Canvas R_Cnv_Root;
        private GameObject R_GObj_Local;
        private GameObject R_GObj_Remote;
        private RuntimeUIFactory.Label R_Lbl_Local;
        private RuntimeUIFactory.Label R_Lbl_Remote;
    #endregion Variables
    #region    Methods
        #region    API
            public static void SetState(bool Bool_LocalSpeaking, bool Bool_RemoteSpeaking)
            {
                // No se crea nada hasta que alguien hable por primera vez //
                if (R_Ctm_VAI_Instance == null && !Bool_LocalSpeaking && !Bool_RemoteSpeaking) return;
                GetInstance().Apply(Bool_LocalSpeaking, Bool_RemoteSpeaking);
            }
        #endregion API
        #region    Internals
            private void Apply(bool Bool_Local, bool Bool_Remote)
            {
                // Textos en el idioma actual (solo se llama cuando cambia el estado, no todos los frames) //
                    if (Bool_Local)  R_Lbl_Local.SetText(VoiceStrings.LocalSpeaking);
                    if (Bool_Remote) R_Lbl_Remote.SetText(VoiceStrings.RemoteSpeaking);
                R_GObj_Local.SetActive(Bool_Local);
                R_GObj_Remote.SetActive(Bool_Remote);
                R_Cnv_Root.gameObject.SetActive(Bool_Local || Bool_Remote);
            }
            private static VoiceActivityIndicator GetInstance()
            {
                if (R_Ctm_VAI_Instance != null) return R_Ctm_VAI_Instance;
                GameObject GObj_Root = new GameObject("VoiceActivityIndicator");
                DontDestroyOnLoad(GObj_Root);
                R_Ctm_VAI_Instance = GObj_Root.AddComponent<VoiceActivityIndicator>();
                R_Ctm_VAI_Instance.Build();
                return R_Ctm_VAI_Instance;
            }
            private void Build()
            {
                R_Cnv_Root = RuntimeUIFactory.CreateOverlayCanvas(this.transform, SORTING_ORDER);
                R_GObj_Local  = CreateTag("LocalSpeaking",  Clr_Local,  new Vector2(0f, 0f), out R_Lbl_Local);
                R_GObj_Remote = CreateTag("RemoteSpeaking", Clr_Remote, new Vector2(0f, 1f), out R_Lbl_Remote);
                R_Cnv_Root.gameObject.SetActive(false);
            }
            // Vec2_Corner: (0,0) abajo a la izquierda / (0,1) arriba a la izquierda //
            private GameObject CreateTag(string Str_Name, Color Clr_Text, Vector2 Vec2_Corner, out RuntimeUIFactory.Label Ctm_Lbl_Out)
            {
                // Fondo //
                    UnityEngine.UI.Image Img_Back = RuntimeUIFactory.CreateImage(R_Cnv_Root.transform, Str_Name, Clr_Back);
                    RectTransform Rct_Back = Img_Back.rectTransform;
                    Rct_Back.anchorMin = Vec2_Corner;
                    Rct_Back.anchorMax = Vec2_Corner;
                    Rct_Back.pivot = Vec2_Corner;
                    Rct_Back.sizeDelta = Vec2_TagSize;
                    Rct_Back.anchoredPosition = new Vector2(Vec2_Margin.x, Vec2_Corner.y > 0.5f ? -Vec2_Margin.y : Vec2_Margin.y);
                // Texto //
                    Ctm_Lbl_Out = RuntimeUIFactory.CreateLabel(Rct_Back, "Text", 32, true);
                    Ctm_Lbl_Out.SetColor(Clr_Text);
                    RuntimeUIFactory.Stretch(Ctm_Lbl_Out.Rect);
                    Img_Back.gameObject.SetActive(false);
                return Img_Back.gameObject;
            }
        #endregion Internals
    #endregion Methods
}
