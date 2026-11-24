using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

#region    "using" Script Clarification
    using Hashtable = ExitGames.Client.Photon.Hashtable;
#endregion "using" Script Clarification

// Estado de combate del jugador LOCAL: vida, balas y carga de la Windhorn //
// - Es la unica fuente de verdad local: el PlayerCharacter pregunta aca en vez de tener contadores sueltos. //
// - Autoridad: el dueno del personaje escribe sus Player Custom Properties (GDD). //
//   Las properties quedan en el servidor aunque el jugador quede inactivo: la reconexion no pierde el estado. //
// - El HUD y el GameManager se enteran por eventos (Observer), no consultando todos los frames. //
public sealed class PlayerCombatState
{
    #region    Variables
        private DuelBalance R_Ctm_DB_Balance;
        private int R_Int_Health;
        private int R_Int_Bullets;
        private float R_Flt_HornCharge;
        private float R_Flt_LastPublishedHorn = -1f;
    #endregion Variables
    #region    Events
        public event Action<int, int> OnHealthChanged;   // (vida actual, vida maxima) //
        public event Action<int, int> OnBulletsChanged;  // (balas, maximo) //
        public event Action<float> OnHornChargeChanged;  // 0..1 - Se dispara todos los frames mientras carga //
        public event Action OnDamaged;
        public event Action OnHealed;
        public event Action OnDied;
        public event Action OnHornUsed;
    #endregion Events
    #region    Properties
        public DuelBalance Balance  => R_Ctm_DB_Balance;
        public int  Health          => R_Int_Health;
        public int  Bullets         => R_Int_Bullets;
        public float HornCharge     => R_Flt_HornCharge;
        public bool IsDead          => R_Int_Health <= 0;
        public bool HasBullets      => R_Int_Bullets > 0;
        public bool IsCylinderFull  => R_Int_Bullets >= R_Ctm_DB_Balance.MaxBullets;
        public bool IsHornReady     => R_Flt_HornCharge >= 1f;
        public float HealthNormalized => (float) R_Int_Health / R_Ctm_DB_Balance.MaxHealth;
    #endregion Properties
    #region    Methods
        #region    Constructor
            public PlayerCombatState(DuelBalance Ctm_DB_Balance)
            {
                R_Ctm_DB_Balance = Ctm_DB_Balance ?? new DuelBalance();
            }
        #endregion Constructor
        #region    Lifecycle
            // Arranque de partida: vida llena, tambor inicial y Windhorn vacia //
            public void ResetForNewMatch(DuelBalance Ctm_DB_Balance = null)
            {
                if (Ctm_DB_Balance != null) R_Ctm_DB_Balance = Ctm_DB_Balance;
                R_Int_Health     = R_Ctm_DB_Balance.MaxHealth;
                R_Int_Bullets    = Mathf.Clamp(R_Ctm_DB_Balance.StartBullets, 0, R_Ctm_DB_Balance.MaxBullets);
                R_Flt_HornCharge = 0f;
                PublishAll();
                RaiseAll();
            }
            // Reconexion: vuelve a publicar todo por si algo quedo a medias //
            public void PublishAll()
            {
                R_Flt_LastPublishedHorn = R_Flt_HornCharge;
                Publish(new Hashtable
                {
                    { PlayerPropertyKeys.Health,     R_Int_Health },
                    { PlayerPropertyKeys.Bullets,    R_Int_Bullets },
                    { PlayerPropertyKeys.HornCharge, R_Flt_HornCharge },
                });
            }
            public void RaiseAll()
            {
                OnHealthChanged?.Invoke(R_Int_Health, R_Ctm_DB_Balance.MaxHealth);
                OnBulletsChanged?.Invoke(R_Int_Bullets, R_Ctm_DB_Balance.MaxBullets);
                OnHornChargeChanged?.Invoke(R_Flt_HornCharge);
            }
        #endregion Lifecycle
        #region    Health
            public void ApplyDamage(int Int_Amount)
            {
                if (IsDead || Int_Amount <= 0) return;
                SetHealth(R_Int_Health - Int_Amount);
                OnDamaged?.Invoke();
                if (IsDead) OnDied?.Invoke();
            }
            public void Heal(int Int_Amount)
            {
                if (IsDead || Int_Amount <= 0) return;
                SetHealth(R_Int_Health + Int_Amount); // SetHealth recorta al maximo (GDD: "sin superar el maximo") //
                OnHealed?.Invoke();
            }
            private void SetHealth(int Int_Value)
            {
                int Int_Clamped = Mathf.Clamp(Int_Value, 0, R_Ctm_DB_Balance.MaxHealth);
                if (Int_Clamped == R_Int_Health) return;
                R_Int_Health = Int_Clamped;
                Publish(new Hashtable { { PlayerPropertyKeys.Health, R_Int_Health } });
                OnHealthChanged?.Invoke(R_Int_Health, R_Ctm_DB_Balance.MaxHealth);
            }
        #endregion Health
        #region    Bullets
            public bool TryConsumeBullet()
            {
                if (!HasBullets) return false;
                SetBullets(R_Int_Bullets - 1);
                return true;
            }
            public void AddBullets(int Int_Amount)
            {
                if (Int_Amount <= 0) return;
                SetBullets(R_Int_Bullets + Int_Amount);
            }
            public void FillCylinder()
            {
                SetBullets(R_Ctm_DB_Balance.MaxBullets);
            }
            private void SetBullets(int Int_Value)
            {
                int Int_Clamped = Mathf.Clamp(Int_Value, 0, R_Ctm_DB_Balance.MaxBullets);
                if (Int_Clamped == R_Int_Bullets) return;
                R_Int_Bullets = Int_Clamped;
                Publish(new Hashtable { { PlayerPropertyKeys.Bullets, R_Int_Bullets } });
                OnBulletsChanged?.Invoke(R_Int_Bullets, R_Ctm_DB_Balance.MaxBullets);
            }
        #endregion Bullets
        #region    Windhorn
            // GDD: "barra que se carga con el tiempo" - La llama el PlayerCharacter en su OnUpdate //
            public void TickHorn(float Flt_DT)
            {
                if (IsDead || IsHornReady) return;
                R_Flt_HornCharge = Mathf.Min(1f, R_Flt_HornCharge + Flt_DT / R_Ctm_DB_Balance.HornChargeSeconds);
                OnHornChargeChanged?.Invoke(R_Flt_HornCharge);
                // Por red solo se manda cada "HornPublishStep" (y al llenarse): no se satura con un valor por frame //
                    if (IsHornReady || R_Flt_HornCharge - R_Flt_LastPublishedHorn >= R_Ctm_DB_Balance.HornPublishStep)
                        PublishHorn();
            }
            public bool TryConsumeHorn()
            {
                if (!IsHornReady || IsDead) return false;
                R_Flt_HornCharge = 0f;
                PublishHorn();
                OnHornChargeChanged?.Invoke(R_Flt_HornCharge);
                OnHornUsed?.Invoke();
                return true;
            }
            private void PublishHorn()
            {
                R_Flt_LastPublishedHorn = R_Flt_HornCharge;
                Publish(new Hashtable { { PlayerPropertyKeys.HornCharge, R_Flt_HornCharge } });
            }
        #endregion Windhorn
        #region    Network
            private static void Publish(Hashtable Hsh_Properties)
            {
                // Fuera de una sala no hay a quien avisar (ej: probando la escena sola) //
                if (!PhotonNetwork.InRoom) return;
                PhotonNetwork.LocalPlayer.SetCustomProperties(Hsh_Properties);
            }
            // Lectura del estado de CUALQUIER jugador (ej: el Master revisando si el rival murio) //
            public static bool TryReadHealth(Player PUNPl_Player, out int Int_Health)
            {
                Int_Health = 0;
                if (PUNPl_Player == null) return false;
                if (!PUNPl_Player.CustomProperties.TryGetValue(PlayerPropertyKeys.Health, out object Obj_Health) || !(Obj_Health is int)) return false;
                Int_Health = (int) Obj_Health;
                return true;
            }
        #endregion Network
    #endregion Methods
}
