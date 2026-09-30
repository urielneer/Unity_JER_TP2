using TMPro;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

public class MenuTypeCreateRoom : BaseMenuType
{
    #region    Variables
        [Header("Create Room GUI Settings")]
            [SerializeField] BaseMenuInputField _roomName;
            BaseMenuInputField Ctm_BMIF_RoomName;
            [SerializeField] BaseMenuInputField _roomSize;
            BaseMenuInputField Ctm_BMIF_RoomSize;
            [SerializeField] BaseMenuInputField _mapWidth;
            BaseMenuInputField Ctm_BMIF_MapWidth;
            [SerializeField] BaseMenuInputField _mapHeight;
            BaseMenuInputField Ctm_BMIF_MapHeight;
        #region    Manager-To-Manager Data
            private string O_Str_Name;
            public string RoomName => O_Str_Name;
            private byte O_Byte_Size;
            public byte RoomSize => O_Byte_Size;
            private int O_Int_Width;
            public int MapWidth => O_Int_Width;
            private int O_Int_Height;
            public int MapHeight => O_Int_Height;
        #endregion Manager-To-Manager Data
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Resume Base //
                    base.Awake();
                // Variables //
                    Ctm_BMIF_RoomName = _roomName;
                    Ctm_BMIF_RoomSize = _roomSize;
                    Ctm_BMIF_MapWidth = _mapWidth;
                    Ctm_BMIF_MapHeight = _mapHeight;
            }
        #endregion Unity Methods
        #region    Custom Methods
            public void UpdateRoomData()
            {
                O_Str_Name = Ctm_BMIF_RoomName.GetComponent<TMP_InputField>().textComponent.text;
                string Str_Temp = Ctm_BMIF_RoomSize.GetComponent<TMP_InputField>().textComponent.text;
                Str_Temp = Regex.Replace(Str_Temp, @"[^\d-]", "").Trim();
                if (int.TryParse(Str_Temp, out int Int_Parsed1))
                {
                    O_Byte_Size = (byte) Int_Parsed1;
                }
                Str_Temp = Ctm_BMIF_MapWidth.GetComponent<TMP_InputField>().textComponent.text;
                Str_Temp = Regex.Replace(Str_Temp, @"[^\d-]", "").Trim();
                if (int.TryParse(Str_Temp, out int Int_Parsed2))
                {
                    O_Int_Width = Mathf.Clamp(Int_Parsed2, ((MenuInputFieldLimitedInt)Ctm_BMIF_MapWidth).Min, ((MenuInputFieldLimitedInt)Ctm_BMIF_MapWidth).Max);
                }
                else
                {
                    O_Int_Width = ((MenuInputFieldLimitedInt)Ctm_BMIF_MapWidth).Min;
                }
                Str_Temp = Ctm_BMIF_MapHeight.GetComponent<TMP_InputField>().textComponent.text;
                Str_Temp = Regex.Replace(Str_Temp, @"[^\d-]", "").Trim();
                if (int.TryParse(Str_Temp, out int Int_Parsed3))
                {
                    O_Int_Height = Mathf.Clamp(Int_Parsed3, ((MenuInputFieldLimitedInt)Ctm_BMIF_MapHeight).Min, ((MenuInputFieldLimitedInt)Ctm_BMIF_MapHeight).Max);
                }
                else
                {
                    O_Int_Height = ((MenuInputFieldLimitedInt)Ctm_BMIF_MapHeight).Min;
                }
            }
        #endregion Custom Methods
    #endregion Methods
}
