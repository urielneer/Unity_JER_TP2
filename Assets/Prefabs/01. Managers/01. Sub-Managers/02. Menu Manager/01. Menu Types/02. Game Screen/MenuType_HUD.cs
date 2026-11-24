using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using static CustomExtension.ArrayExtensions;

public class MenuTypeHUD : BaseMenuType
{
    #region    Constants
        // Nombres de las instancias dentro de HUD.prefab - Solo se usan si no se asignan en el Inspector //
        private const string HEALTH_BAR_NAME   = "Health GUI";
        private const string WINDHORN_BAR_NAME = "Windhorn GUI";
    #endregion Constants
    #region    Variables
        [Header("HUD GUI Settings")]
            [SerializeField] private Menu_ChatElement _chat;
            private Menu_ChatElement IO_Ctm_MCE_Chat;   
            public  Menu_ChatElement ChatElement => IO_Ctm_MCE_Chat;
            [SerializeField] private Aim _aim;
            private Aim R_Aim_Cursor;
            public Aim AimCursor => R_Aim_Cursor;
        [Header("HUD Player Values")]
            [SerializeField] private GUIBar _healthBar;
            [SerializeField] private GUIBar _windhornBar;
            private GUIBar I_GUIB_Health;
            private GUIBar I_GUIB_Windhorn;
            private PlayerCombatState R_Ctm_PCS_Bound;
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Resume Base //
                    base.Awake();
                // Variables //
                    IO_Ctm_MCE_Chat = _chat;
                    R_Aim_Cursor = _aim;
                    I_GUIB_Health   = _healthBar   != null ? _healthBar   : FindBar(HEALTH_BAR_NAME);
                    I_GUIB_Windhorn = _windhornBar != null ? _windhornBar : FindBar(WINDHORN_BAR_NAME);
            }
            private void OnDestroy()
            {
                Unbind();
            }
            #pragma warning disable CS1998
            public async void OnUpdate(float Flt_DT)
            #pragma warning restore CS1998
            {
                R_Aim_Cursor.OnUpdate(Flt_DT);
            }
        #endregion Unity Methods
        #region    Custom Methods
            #region    Player Values (Observer del PlayerCombatState)
                public void Bind(PlayerCombatState Ctm_PCS_State)
                {
                    if (R_Ctm_PCS_Bound == Ctm_PCS_State) return;
                    Unbind();
                    R_Ctm_PCS_Bound = Ctm_PCS_State;
                    if (R_Ctm_PCS_Bound == null) return;
                    R_Ctm_PCS_Bound.OnHealthChanged     += OnHealthChanged;
                    R_Ctm_PCS_Bound.OnHornChargeChanged += OnHornChargeChanged;
                    R_Ctm_PCS_Bound.OnDamaged           += OnDamaged;
                    R_Ctm_PCS_Bound.OnHealed            += OnHealed;
                    R_Ctm_PCS_Bound.OnDied              += OnDied;
                    R_Ctm_PCS_Bound.OnHornUsed          += OnHornUsed;
                    // Estado inicial //
                        OnHealthChanged(R_Ctm_PCS_Bound.Health, R_Ctm_PCS_Bound.Balance.MaxHealth);
                        OnHornChargeChanged(R_Ctm_PCS_Bound.HornCharge);
                        if (I_GUIB_Health != null) I_GUIB_Health.SetDead(R_Ctm_PCS_Bound.IsDead);
                }
                public void Unbind()
                {
                    if (R_Ctm_PCS_Bound == null) return;
                    R_Ctm_PCS_Bound.OnHealthChanged     -= OnHealthChanged;
                    R_Ctm_PCS_Bound.OnHornChargeChanged -= OnHornChargeChanged;
                    R_Ctm_PCS_Bound.OnDamaged           -= OnDamaged;
                    R_Ctm_PCS_Bound.OnHealed            -= OnHealed;
                    R_Ctm_PCS_Bound.OnDied              -= OnDied;
                    R_Ctm_PCS_Bound.OnHornUsed          -= OnHornUsed;
                    R_Ctm_PCS_Bound = null;
                }
                private void OnHealthChanged(int Int_Health, int Int_Max)
                {
                    if (I_GUIB_Health != null) I_GUIB_Health.SetValue(Int_Max > 0 ? (float) Int_Health / Int_Max : 0f);
                }
                private void OnHornChargeChanged(float Flt_Charge)
                {
                    if (I_GUIB_Windhorn != null) I_GUIB_Windhorn.SetValue(Flt_Charge);
                }
                private void OnDamaged()  { if (I_GUIB_Health   != null) I_GUIB_Health.PlayDamage(); }
                private void OnHealed()   { if (I_GUIB_Health   != null) I_GUIB_Health.PlayHeal(); }
                private void OnDied()     { if (I_GUIB_Health   != null) I_GUIB_Health.SetDead(true); }
                private void OnHornUsed() { if (I_GUIB_Windhorn != null) I_GUIB_Windhorn.PlayWindhornUsed(); }
                private GUIBar FindBar(string Str_Name)
                {
                    foreach (GUIBar Ctm_GUIB_Bar in GetComponentsInChildren<GUIBar>(true))
                    {
                        if (Ctm_GUIB_Bar.name == Str_Name) return Ctm_GUIB_Bar;
                    }
                    Debug.LogWarning("[HUD] No se encontro la barra '" + Str_Name + "'. Asignala en el Inspector del HUD.", this);
                    return null;
                }
            #endregion Player Values (Observer del PlayerCombatState)
            #region    Room Methods
                public void LeftRoom()
                {
                    IO_Ctm_MCE_Chat.LeftRoom();
                }
                public void JoinRoom()
                {
                    IO_Ctm_MCE_Chat.JoinRoom();
                }
                public void GameStart()
                {
                    IO_Ctm_MCE_Chat.GameStart();
                }
                public void GameEnd()
                {
                    IO_Ctm_MCE_Chat.GameEnd();
                }
            #endregion Room Methods
        #endregion Custom Methods
    #endregion Methods
}
