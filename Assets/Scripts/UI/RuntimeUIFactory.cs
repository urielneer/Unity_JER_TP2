using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Helpers para armar UI por codigo (pantallas que deben existir en cualquier escena) //
// Misma tecnica que GameResultScreen: TMP si hay fuente, Text clasico como respaldo //
public static class RuntimeUIFactory
{
    #region    Label
        // Envoltorio de texto - Oculta si por debajo hay TMP o Text clasico //
        public sealed class Label
        {
            public TMP_Text Tmp;
            public Text Txt;
            public RectTransform Rect => Tmp != null ? Tmp.rectTransform : Txt.rectTransform;
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
    #endregion Label
    #region    Builders
        public static Canvas CreateOverlayCanvas(Transform Tfm_Parent, int Int_SortingOrder)
        {
            GameObject GObj_Canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            GObj_Canvas.transform.SetParent(Tfm_Parent, false);
            Canvas Cnv_Root = GObj_Canvas.GetComponent<Canvas>();
            Cnv_Root.renderMode = RenderMode.ScreenSpaceOverlay;
            Cnv_Root.sortingOrder = Int_SortingOrder;
            CanvasScaler Ctm_CS_Scaler = GObj_Canvas.GetComponent<CanvasScaler>();
            Ctm_CS_Scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            Ctm_CS_Scaler.referenceResolution = new Vector2(1920f, 1080f);
            Ctm_CS_Scaler.matchWidthOrHeight = 0.5f;
            return Cnv_Root;
        }
        public static Image CreateImage(Transform Tfm_Parent, string Str_Name, Color Clr_Color)
        {
            GameObject GObj_New = new GameObject(Str_Name, typeof(RectTransform), typeof(Image));
            GObj_New.transform.SetParent(Tfm_Parent, false);
            Image Img_New = GObj_New.GetComponent<Image>();
            Img_New.color = Clr_Color;
            Img_New.raycastTarget = false;
            return Img_New;
        }
        public static Label CreateLabel(Transform Tfm_Parent, string Str_Name, int Int_Size, bool Bool_Bold)
        {
            Label Ctm_Lbl_New = new Label();
            GameObject GObj_New = new GameObject(Str_Name, typeof(RectTransform));
            GObj_New.transform.SetParent(Tfm_Parent, false);
            TMP_FontAsset Ctm_TFA_Font = ResolveTMPFont();
            // Camino preferido - TMP //
                if (Ctm_TFA_Font != null)
                {
                    TextMeshProUGUI Ctm_TMP_New = GObj_New.AddComponent<TextMeshProUGUI>();
                    Ctm_TMP_New.font = Ctm_TFA_Font;
                    Ctm_TMP_New.fontSize = Int_Size;
                    Ctm_TMP_New.fontStyle = Bool_Bold ? FontStyles.Bold : FontStyles.Normal;
                    Ctm_TMP_New.alignment = TextAlignmentOptions.Center;
                    Ctm_TMP_New.raycastTarget = false;
                    Ctm_Lbl_New.Tmp = Ctm_TMP_New;
                    return Ctm_Lbl_New;
                }
            // Respaldo - Text clasico //
                Text Ctm_Txt_New = GObj_New.AddComponent<Text>();
                Ctm_Txt_New.font = ResolveLegacyFont();
                Ctm_Txt_New.fontSize = Int_Size;
                Ctm_Txt_New.fontStyle = Bool_Bold ? FontStyle.Bold : FontStyle.Normal;
                Ctm_Txt_New.alignment = TextAnchor.MiddleCenter;
                Ctm_Txt_New.horizontalOverflow = HorizontalWrapMode.Wrap;
                Ctm_Txt_New.verticalOverflow = VerticalWrapMode.Overflow;
                Ctm_Txt_New.raycastTarget = false;
                Ctm_Lbl_New.Txt = Ctm_Txt_New;
                return Ctm_Lbl_New;
        }
        public static Button CreateButton(Transform Tfm_Parent, string Str_Name, Vector2 Vec2_Size, Color Clr_Background, int Int_FontSize, out Label Ctm_Lbl_Text)
        {
            // Fondo clickeable //
                Image Img_Back = CreateImage(Tfm_Parent, Str_Name, Clr_Background);
                Img_Back.raycastTarget = true;
                Img_Back.rectTransform.sizeDelta = Vec2_Size;
                Button Btn_New = Img_Back.gameObject.AddComponent<Button>();
                Btn_New.targetGraphic = Img_Back;
            // Texto //
                Ctm_Lbl_Text = CreateLabel(Img_Back.transform, "Text", Int_FontSize, true);
                Stretch(Ctm_Lbl_Text.Rect);
            return Btn_New;
        }
    #endregion Builders
    #region    Layout
        public static void Stretch(RectTransform Rct_Input)
        {
            Rct_Input.anchorMin = Vector2.zero;
            Rct_Input.anchorMax = Vector2.one;
            Rct_Input.offsetMin = Vector2.zero;
            Rct_Input.offsetMax = Vector2.zero;
        }
        public static void HorizontalBand(RectTransform Rct_Input, float Flt_PosY, float Flt_Height, float Flt_SidePadding = 0f)
        {
            Rct_Input.anchorMin = new Vector2(0f, 0.5f);
            Rct_Input.anchorMax = new Vector2(1f, 0.5f);
            Rct_Input.pivot = new Vector2(0.5f, 0.5f);
            Rct_Input.anchoredPosition = new Vector2(0f, Flt_PosY);
            Rct_Input.sizeDelta = new Vector2(-Flt_SidePadding * 2f, Flt_Height);
        }
    #endregion Layout
    #region    Fonts
        private static TMP_FontAsset ResolveTMPFont()
        {
            try
            {
                if (TMP_Settings.defaultFontAsset != null) return TMP_Settings.defaultFontAsset;
            }
            catch { /* TMP sin configurar - Se usa el respaldo */ }
            return Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }
        private static Font ResolveLegacyFont()
        {
            Font Fnt_Result = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (Fnt_Result == null) Fnt_Result = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return Fnt_Result;
        }
    #endregion Fonts
}
