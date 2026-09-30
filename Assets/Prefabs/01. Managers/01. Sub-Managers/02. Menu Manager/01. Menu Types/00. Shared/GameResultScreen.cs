using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pantalla de resultado de la ronda //
// Se arma entera por codigo: no depende del prefab del ErrorMenu ni de nada del Inspector //
public class GameResultScreen : MonoBehaviour
{
    #region    Variables
        private static GameResultScreen R_Ctm_GRS_Instance;
        private Canvas R_Cnv_Root;
        private Image R_Img_Background;
        private Image R_Img_Banner;
        private Label R_Lbl_Title;
        private Label R_Lbl_Subtitle;
        private Label R_Lbl_Countdown;
        private Coroutine R_Crtn_Countdown;
        // Colores - Victoria en verde, Derrota en rojo. Asi las dos pantallas no se confunden //
        private static readonly Color Clr_WinBack     = new Color(0.03f, 0.13f, 0.08f, 0.94f);
        private static readonly Color Clr_WinBanner   = new Color(0.10f, 0.42f, 0.24f, 1.00f);
        private static readonly Color Clr_WinTitle    = new Color(0.54f, 0.95f, 0.66f, 1.00f);
        private static readonly Color Clr_LoseBack    = new Color(0.14f, 0.03f, 0.05f, 0.94f);
        private static readonly Color Clr_LoseBanner  = new Color(0.47f, 0.12f, 0.15f, 1.00f);
        private static readonly Color Clr_LoseTitle   = new Color(1.00f, 0.52f, 0.52f, 1.00f);
        private static readonly Color Clr_Body        = new Color(0.92f, 0.92f, 0.90f, 1.00f);
        private static readonly Color Clr_Faded       = new Color(0.72f, 0.72f, 0.70f, 1.00f);
        // Envoltorio de texto - Usa TMP si hay fuente disponible, si no cae al Text clasico //
        private class Label
        {
            public TMP_Text Tmp;
            public Text Txt;
            public void SetText(string Str_Input)
            {
                if (Tmp != null) Tmp.text = Str_Input;
                if (Txt != null) Txt.text = Str_Input;
            }
            public void SetColor(Color Clr_Input)
            {
                if (Tmp != null) Tmp.color = Clr_Input;
                if (Txt != null) Txt.color = Clr_Input;
            }
        }
    #endregion Variables
    #region    Methods
        #region    API
            public static void Show(bool Bool_IWon, string Str_Reason, float Flt_RestartSeconds)
            {
                GetInstance().Display(Bool_IWon, Str_Reason, Flt_RestartSeconds);
            }
            public static void Hide()
            {
                if (R_Ctm_GRS_Instance == null) return;
                R_Ctm_GRS_Instance.Conceal();
            }
            // Reinicio limpio en dos pasos: primero al menu, despues a la ronda nueva //
            // El segundo paso NO puede vivir en una corrutina: el cambio de escena la mata a la mitad //
            // y la partida se quedaba clavada en "Loading..." para siempre //
            public static void RequestCleanRestart(float Flt_Pause)
            {
                GameResultScreen Ctm_GRS_Screen = GetInstance();
                Ctm_GRS_Screen.R_Bool_RestartPending = true;
                Ctm_GRS_Screen.R_Flt_RestartPause = Flt_Pause;
                Debug.Log("[Game Result Screen] Reinicio limpio: volviendo al menu");
                PhotonNetwork.LoadLevel(0);
            }
        #endregion API
        #region    Scene Hook
            private bool R_Bool_RestartPending = false;
            private float R_Flt_RestartPause = 1.5f;
            private void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
            private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }
            private void OnSceneLoaded(Scene Scn_Loaded, LoadSceneMode E_Mode)
            {
                // La pantalla de resultado nunca debe sobrevivir a un cambio de escena //
                    Conceal();
                // Segundo paso del reinicio - Ya estamos en el menu, se arranca la ronda nueva //
                    if (!R_Bool_RestartPending) return;
                    if (Scn_Loaded.buildIndex != 0) return;
                    R_Bool_RestartPending = false;
                    StartCoroutine(FinishCleanRestartRoutine());
            }
            private IEnumerator FinishCleanRestartRoutine()
            {
                // Margen para que los demas clientes tambien lleguen al menu //
                    yield return new WaitForSecondsRealtime(R_Flt_RestartPause);
                    if (!PhotonNetwork.InRoom)
                    {
                        Debug.LogWarning("[Game Result Screen] Reinicio cancelado: ya no estamos en la sala");
                        yield break;
                    }
                    if (!PhotonNetwork.IsMasterClient) yield break;
                    Debug.Log("[Game Result Screen] Reinicio limpio: arrancando ronda nueva");
                    PhotonNetwork.LoadLevel(1);
            }
        #endregion Scene Hook
        #region    Instance
            private static GameResultScreen GetInstance()
            {
                if (R_Ctm_GRS_Instance != null) return R_Ctm_GRS_Instance;
                GameObject GObj_Root = new GameObject("GameResultScreen");
                DontDestroyOnLoad(GObj_Root);
                R_Ctm_GRS_Instance = GObj_Root.AddComponent<GameResultScreen>();
                R_Ctm_GRS_Instance.Build();
                return R_Ctm_GRS_Instance;
            }
        #endregion Instance
        #region    Display
            private void Display(bool Bool_IWon, string Str_Reason, float Flt_RestartSeconds)
            {
                // Colores segun resultado //
                    R_Img_Background.color = Bool_IWon ? Clr_WinBack   : Clr_LoseBack;
                    R_Img_Banner.color     = Bool_IWon ? Clr_WinBanner : Clr_LoseBanner;
                    R_Lbl_Title.SetColor(Bool_IWon ? Clr_WinTitle : Clr_LoseTitle);
                // Textos //
                    R_Lbl_Title.SetText(Bool_IWon ? "VICTORIA" : "DERROTA");
                    R_Lbl_Subtitle.SetText(Str_Reason);
                    R_Lbl_Subtitle.SetColor(Clr_Body);
                    R_Lbl_Countdown.SetColor(Clr_Faded);
                // Mostrar //
                    R_Cnv_Root.gameObject.SetActive(true);
                    if (R_Crtn_Countdown != null) StopCoroutine(R_Crtn_Countdown);
                    R_Crtn_Countdown = StartCoroutine(CountdownRoutine(Flt_RestartSeconds));
            }
            private void Conceal()
            {
                if (R_Crtn_Countdown != null) { StopCoroutine(R_Crtn_Countdown); R_Crtn_Countdown = null; }
                if (R_Cnv_Root != null) R_Cnv_Root.gameObject.SetActive(false);
            }
            private IEnumerator CountdownRoutine(float Flt_Seconds)
            {
                float Flt_Remaining = Flt_Seconds;
                while (Flt_Remaining > 0f)
                {
                    R_Lbl_Countdown.SetText("Nueva ronda en " + Mathf.CeilToInt(Flt_Remaining) + "...");
                    yield return new WaitForSecondsRealtime(0.25f);
                    Flt_Remaining -= 0.25f;
                }
                R_Lbl_Countdown.SetText("Reiniciando...");
                R_Crtn_Countdown = null;
            }
        #endregion Display
        #region    Build
            private void Build()
            {
                // Canvas //
                    GameObject GObj_Canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                    GObj_Canvas.transform.SetParent(this.transform, false);
                    R_Cnv_Root = GObj_Canvas.GetComponent<Canvas>();
                    R_Cnv_Root.renderMode = RenderMode.ScreenSpaceOverlay;
                    R_Cnv_Root.sortingOrder = 32000; // Siempre por encima del HUD //
                    CanvasScaler Ctm_CS_Scaler = GObj_Canvas.GetComponent<CanvasScaler>();
                    Ctm_CS_Scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    Ctm_CS_Scaler.referenceResolution = new Vector2(1920f, 1080f);
                    Ctm_CS_Scaler.matchWidthOrHeight = 0.5f;
                // Fondo //
                    R_Img_Background = CreateImage(GObj_Canvas.transform, "Background");
                    Stretch(R_Img_Background.rectTransform, Vector2.zero, Vector2.zero);
                // Franja del titulo //
                    R_Img_Banner = CreateImage(GObj_Canvas.transform, "Banner");
                    RectTransform Rct_Banner = R_Img_Banner.rectTransform;
                    Rct_Banner.anchorMin = new Vector2(0f, 0.5f);
                    Rct_Banner.anchorMax = new Vector2(1f, 0.5f);
                    Rct_Banner.pivot = new Vector2(0.5f, 0.5f);
                    Rct_Banner.anchoredPosition = new Vector2(0f, 90f);
                    Rct_Banner.sizeDelta = new Vector2(0f, 190f);
                // Titulo //
                    R_Lbl_Title = CreateLabel(GObj_Canvas.transform, "Title", 120, FontStyles.Bold, FontStyle.Bold);
                    RectTransform Rct_Title = GetRect(R_Lbl_Title);
                    Rct_Title.anchorMin = new Vector2(0f, 0.5f);
                    Rct_Title.anchorMax = new Vector2(1f, 0.5f);
                    Rct_Title.pivot = new Vector2(0.5f, 0.5f);
                    Rct_Title.anchoredPosition = new Vector2(0f, 90f);
                    Rct_Title.sizeDelta = new Vector2(-80f, 190f);
                // Subtitulo //
                    R_Lbl_Subtitle = CreateLabel(GObj_Canvas.transform, "Subtitle", 38, FontStyles.Normal, FontStyle.Normal);
                    RectTransform Rct_Sub = GetRect(R_Lbl_Subtitle);
                    Rct_Sub.anchorMin = new Vector2(0f, 0.5f);
                    Rct_Sub.anchorMax = new Vector2(1f, 0.5f);
                    Rct_Sub.pivot = new Vector2(0.5f, 0.5f);
                    Rct_Sub.anchoredPosition = new Vector2(0f, -70f);
                    Rct_Sub.sizeDelta = new Vector2(-240f, 120f);
                // Cuenta regresiva //
                    R_Lbl_Countdown = CreateLabel(GObj_Canvas.transform, "Countdown", 30, FontStyles.Normal, FontStyle.Normal);
                    RectTransform Rct_Cnt = GetRect(R_Lbl_Countdown);
                    Rct_Cnt.anchorMin = new Vector2(0f, 0f);
                    Rct_Cnt.anchorMax = new Vector2(1f, 0f);
                    Rct_Cnt.pivot = new Vector2(0.5f, 0f);
                    Rct_Cnt.anchoredPosition = new Vector2(0f, 90f);
                    Rct_Cnt.sizeDelta = new Vector2(-80f, 60f);
                // Arranca oculta //
                    R_Cnv_Root.gameObject.SetActive(false);
            }
            private static void Stretch(RectTransform Rct_Input, Vector2 Vec2_OffsetMin, Vector2 Vec2_OffsetMax)
            {
                Rct_Input.anchorMin = Vector2.zero;
                Rct_Input.anchorMax = Vector2.one;
                Rct_Input.offsetMin = Vec2_OffsetMin;
                Rct_Input.offsetMax = Vec2_OffsetMax;
            }
            private static Image CreateImage(Transform Tfm_Parent, string Str_Name)
            {
                GameObject GObj_New = new GameObject(Str_Name, typeof(RectTransform), typeof(Image));
                GObj_New.transform.SetParent(Tfm_Parent, false);
                Image Img_New = GObj_New.GetComponent<Image>();
                Img_New.raycastTarget = false;
                return Img_New;
            }
            private static RectTransform GetRect(Label Ctm_Lbl_Input)
            {
                if (Ctm_Lbl_Input.Tmp != null) return Ctm_Lbl_Input.Tmp.rectTransform;
                return Ctm_Lbl_Input.Txt.rectTransform;
            }
            private static Label CreateLabel(Transform Tfm_Parent, string Str_Name, int Int_Size, FontStyles E_TmpStyle, FontStyle E_LegacyStyle)
            {
                Label Ctm_Lbl_New = new Label();
                TMP_FontAsset Ctm_TFA_Font = ResolveTMPFont();
                GameObject GObj_New = new GameObject(Str_Name, typeof(RectTransform));
                GObj_New.transform.SetParent(Tfm_Parent, false);
                // Camino preferido - TMP //
                    if (Ctm_TFA_Font != null)
                    {
                        TextMeshProUGUI Ctm_TMP_New = GObj_New.AddComponent<TextMeshProUGUI>();
                        Ctm_TMP_New.font = Ctm_TFA_Font;
                        Ctm_TMP_New.fontSize = Int_Size;
                        Ctm_TMP_New.fontStyle = E_TmpStyle;
                        Ctm_TMP_New.alignment = TextAlignmentOptions.Center;
                        Ctm_TMP_New.raycastTarget = false;
                        Ctm_Lbl_New.Tmp = Ctm_TMP_New;
                        return Ctm_Lbl_New;
                    }
                // Respaldo - Text clasico con la fuente interna de Unity //
                    Text Ctm_Txt_New = GObj_New.AddComponent<Text>();
                    Ctm_Txt_New.font = ResolveLegacyFont();
                    Ctm_Txt_New.fontSize = Int_Size;
                    Ctm_Txt_New.fontStyle = E_LegacyStyle;
                    Ctm_Txt_New.alignment = TextAnchor.MiddleCenter;
                    Ctm_Txt_New.horizontalOverflow = HorizontalWrapMode.Wrap;
                    Ctm_Txt_New.verticalOverflow = VerticalWrapMode.Overflow;
                    Ctm_Txt_New.raycastTarget = false;
                    Ctm_Lbl_New.Txt = Ctm_Txt_New;
                    return Ctm_Lbl_New;
            }
            private static TMP_FontAsset ResolveTMPFont()
            {
                try
                {
                    if (TMP_Settings.defaultFontAsset != null) return TMP_Settings.defaultFontAsset;
                }
                catch { /* TMP sin configurar - Se usa el respaldo //*/ }
                return Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }
            private static Font ResolveLegacyFont()
            {
                Font Fnt_Result = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (Fnt_Result == null) Fnt_Result = Resources.GetBuiltinResource<Font>("Arial.ttf");
                return Fnt_Result;
            }
        #endregion Build
    #endregion Methods
}
