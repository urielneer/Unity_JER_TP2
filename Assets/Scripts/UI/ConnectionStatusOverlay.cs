using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Cartel de estado de conexion: "Reconectando..." / "Rival desconectado: 23 s" //
// Se arma por codigo y sobrevive a los cambios de escena (igual que GameResultScreen) //
// La cuenta regresiva se calcula contra PhotonNetwork.Time: ambos clientes ven el mismo numero //
public class ConnectionStatusOverlay : MonoBehaviour
{
    #region    Constants
        private const int SORTING_ORDER = 31000; // Arriba del HUD, debajo de GameResultScreen (32000) //
        private static readonly Color Clr_Background = new Color(0.05f, 0.04f, 0.03f, 0.82f);
        private static readonly Color Clr_Banner     = new Color(0.45f, 0.30f, 0.12f, 1.00f);
        private static readonly Color Clr_Title      = new Color(1.00f, 0.86f, 0.55f, 1.00f);
        private static readonly Color Clr_Body       = new Color(0.92f, 0.92f, 0.90f, 1.00f);
    #endregion Constants
    #region    Variables
        private static ConnectionStatusOverlay R_Ctm_CSO_Instance;
        private Canvas R_Cnv_Root;
        private RuntimeUIFactory.Label R_Lbl_Title;
        private RuntimeUIFactory.Label R_Lbl_Body;
        // Cuenta regresiva //
        private bool R_Bool_IsCountingDown;
        private double R_Dbl_Deadline;
        private string R_Str_BodyFormat;
        private int R_Int_LastShownSeconds = -1;
    #endregion Variables
    #region    Methods
        #region    API
            public static void Show(string Str_Title, string Str_Body)
            {
                ConnectionStatusOverlay Ctm_CSO_Overlay = GetInstance();
                Ctm_CSO_Overlay.R_Bool_IsCountingDown = false;
                Ctm_CSO_Overlay.Display(Str_Title, Str_Body);
            }
            // Str_BodyFormat recibe los segundos restantes en {0} //
            public static void ShowCountdown(string Str_Title, string Str_BodyFormat, double Dbl_ServerDeadline)
            {
                ConnectionStatusOverlay Ctm_CSO_Overlay = GetInstance();
                Ctm_CSO_Overlay.R_Bool_IsCountingDown = true;
                Ctm_CSO_Overlay.R_Dbl_Deadline = Dbl_ServerDeadline;
                Ctm_CSO_Overlay.R_Str_BodyFormat = Str_BodyFormat;
                Ctm_CSO_Overlay.R_Int_LastShownSeconds = -1;
                Ctm_CSO_Overlay.Display(Str_Title, string.Empty);
                Ctm_CSO_Overlay.RefreshCountdown();
            }
            public static void Hide()
            {
                if (R_Ctm_CSO_Instance == null) return;
                R_Ctm_CSO_Instance.R_Bool_IsCountingDown = false;
                R_Ctm_CSO_Instance.R_Cnv_Root.gameObject.SetActive(false);
            }
        #endregion API
        #region    Unity Methods
            private void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
            private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }
            private void Update()
            {
                if (R_Bool_IsCountingDown) RefreshCountdown();
            }
        #endregion Unity Methods
        #region    Internals
            private void OnSceneLoaded(Scene Scn_Loaded, LoadSceneMode E_Mode)
            {
                // Un cambio de escena siempre invalida el estado mostrado //
                Hide();
            }
            private void RefreshCountdown()
            {
                // Solo se reescribe el texto cuando cambia el segundo: sin allocations por frame //
                int Int_Seconds = Mathf.Max(0, Mathf.CeilToInt((float) (R_Dbl_Deadline - PhotonNetwork.Time)));
                if (Int_Seconds == R_Int_LastShownSeconds) return;
                R_Int_LastShownSeconds = Int_Seconds;
                R_Lbl_Body.SetText(string.Format(R_Str_BodyFormat, Int_Seconds));
            }
            private void Display(string Str_Title, string Str_Body)
            {
                R_Lbl_Title.SetText(Str_Title);
                R_Lbl_Body.SetText(Str_Body);
                R_Cnv_Root.gameObject.SetActive(true);
            }
            private static ConnectionStatusOverlay GetInstance()
            {
                if (R_Ctm_CSO_Instance != null) return R_Ctm_CSO_Instance;
                GameObject GObj_Root = new GameObject("ConnectionStatusOverlay");
                DontDestroyOnLoad(GObj_Root);
                R_Ctm_CSO_Instance = GObj_Root.AddComponent<ConnectionStatusOverlay>();
                R_Ctm_CSO_Instance.Build();
                return R_Ctm_CSO_Instance;
            }
            private void Build()
            {
                // Canvas //
                    R_Cnv_Root = RuntimeUIFactory.CreateOverlayCanvas(this.transform, SORTING_ORDER);
                // Fondo - Bloquea clicks al juego mientras esta visible //
                    Image Img_Background = RuntimeUIFactory.CreateImage(R_Cnv_Root.transform, "Background", Clr_Background);
                    Img_Background.raycastTarget = true;
                    RuntimeUIFactory.Stretch(Img_Background.rectTransform);
                // Franja //
                    Image Img_Banner = RuntimeUIFactory.CreateImage(R_Cnv_Root.transform, "Banner", Clr_Banner);
                    RuntimeUIFactory.HorizontalBand(Img_Banner.rectTransform, 40f, 150f);
                // Textos //
                    R_Lbl_Title = RuntimeUIFactory.CreateLabel(R_Cnv_Root.transform, "Title", 84, true);
                    R_Lbl_Title.SetColor(Clr_Title);
                    RuntimeUIFactory.HorizontalBand(R_Lbl_Title.Rect, 40f, 150f, 40f);
                    R_Lbl_Body = RuntimeUIFactory.CreateLabel(R_Cnv_Root.transform, "Body", 40, false);
                    R_Lbl_Body.SetColor(Clr_Body);
                    RuntimeUIFactory.HorizontalBand(R_Lbl_Body.Rect, -90f, 80f, 120f);
                // Arranca oculto //
                    R_Cnv_Root.gameObject.SetActive(false);
            }
        #endregion Internals
    #endregion Methods
}
