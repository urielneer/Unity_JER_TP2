using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

#region    Struct
    public struct ScoreBoardSetupData
    {
        public string Name { get; set; }
        public int Ping { get; set; }
        public ScoreBoardSetupData(string Str_Name, int Int_Ping)
        {
            Name = Str_Name;
            Ping = Int_Ping;
        }
    }
    public struct ScoreBoardElement
    {
        public string Name { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Ping { get; set; }
        public ScoreBoardElement(string Str_Name, int Int_Ping)
        {
            Name = Str_Name;
            Kills = 0;
            Deaths = 0;
            Ping = Int_Ping;
        }
        public void IncreaseKills()
        {
            Kills++;
        }
        public void IncreaseDeaths()
        {
            Deaths++;
        }
        public void UpdatePing(int Int_Ping)
        {
            Ping = Int_Ping;
        }
        public void Overwrite(int Int_Deaths, int Int_Kills)
        {
            Deaths = Int_Deaths;
            Kills = Int_Kills;
        }
    }
#endregion Struct
public class MenuListElementScoreBoard : BaseMenuListElement
{
    #region    Variables
        #region    Input
            [Space(10)]
            [Header("Score-Board List Element Settings")]
                [SerializeField] BaseMenuLabel _name;
                protected static BaseMenuLabel I_BML_Name;
                [SerializeField] BaseMenuLabel _kills;
                protected static BaseMenuLabel I_BML_Kills;
                [SerializeField] BaseMenuLabel _deaths;
                protected static BaseMenuLabel I_BML_Deaths;
                [SerializeField] BaseMenuLabel _ping;
                protected static BaseMenuLabel I_BML_Ping;
                private bool I_Bool_DoesExist = false;
        #endregion Input
        #region    Manager-To-Manager Data
            private static ScoreBoardElement O_CtmS_ScbE_Values;
            public ScoreBoardElement Values => O_CtmS_ScbE_Values;
        #endregion Manager-To-Manager Data
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Variables //
                    SetVariables();
                // Resume Base //
                    base.Awake();
            }
            protected override void OnValidate()
            {
                // Variables //
                    SetVariables();
                // Resume Base //
                    base.OnValidate();
            }
        #endregion Unity Methods
        #region    Override Methods
            public virtual void Overwrite(string[] Str_A1_Input) 
            {
                I_BML_Name.Overwrite(Str_A1_Input[0]);
                I_BML_Kills.Overwrite(Str_A1_Input[1]);
                I_BML_Deaths.Overwrite(Str_A1_Input[2]);
                I_BML_Ping.Overwrite(Str_A1_Input[3]);
            }
            public override BaseMenuElement SetUp(GameObject GObj_Master, object Obj_Element)
            {   
                // Resume Base //
                    base.SetUp(GObj_Master, Obj_Element);
                // Set Up //
                    ScoreBoardElement CtmS_ScbE_Temp = ((ScoreBoardElement) Obj_Element);
                    SetVariables(CtmS_ScbE_Temp.Name, CtmS_ScbE_Temp.Ping);
                    if (I_Bool_DoesExist)
                    {
                        O_CtmS_ScbE_Values.Overwrite(CtmS_ScbE_Temp.Deaths, CtmS_ScbE_Temp.Kills);
                        SetElementText((object) O_CtmS_ScbE_Values);
                    }
                    I_Bool_DoesExist = true;
                return this;
            }
            public override void RemoveElement()
            {
                Destroy(this);
            }
            protected override void SetElementText(object Obj_Element)
            {
                ScoreBoardElement CtmS_ScbE_Temp = (ScoreBoardElement) Obj_Element;
                string[] Str_A1_Temp =
                {
                    CtmS_ScbE_Temp.Name,
                    (CtmS_ScbE_Temp.Kills).ToString("D3"),
                    (CtmS_ScbE_Temp.Deaths).ToString("D3"),
                    (CtmS_ScbE_Temp.Ping).ToString("D4")
                };
                Overwrite(Str_A1_Temp);
                O_CtmS_ScbE_Values = CtmS_ScbE_Temp;
            }
        #endregion Override Methods
        #region    Custom Methods
            private void SetVariables(string Stn_Name = "", int Int_Ping = 0)
            {
                I_BML_Name = _name;
                I_BML_Kills = _kills;
                I_BML_Deaths = _deaths;
                I_BML_Ping = _ping;
                SetValues(Stn_Name, Int_Ping);
            }
            public void SetValues(string Stn_Name = "", int Int_Ping = 0)
            {
                O_CtmS_ScbE_Values = new ScoreBoardElement(Stn_Name, Int_Ping);
                SetElementText((object) O_CtmS_ScbE_Values);
            }
            public void IncreaseKills()
            {
                O_CtmS_ScbE_Values.IncreaseKills();
                SetElementText((object) O_CtmS_ScbE_Values);
            }
            public void IncreaseDeaths()
            {
                O_CtmS_ScbE_Values.IncreaseDeaths();
                SetElementText((object) O_CtmS_ScbE_Values);
            }
            public void UpdatePing(int Int_Ping)
            {
                O_CtmS_ScbE_Values.UpdatePing(Int_Ping);
                SetElementText((object) O_CtmS_ScbE_Values);
            }
        #endregion Custom Methods
    #endregion Methods
}
