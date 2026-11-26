using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using static CustomExtension.ArrayExtensions;
using System;

public class LiveOpsManager : BaseManager<CharacterManager>
{
    #region    Variables
        [Header(" Health Settings")]
            // Max Amount of Health bar //
                [SerializeField] private float _maxHealth;
                private float IO_Flt_MaxHealth;
                public float MaxHealth => IO_Flt_MaxHealth;
            // Rate in which the Health bar increases //
                [SerializeField] private float _healthRegen;
                private float IO_Flt_HealthRegen;
                public float HealthRegen => IO_Flt_HealthRegen;
        [Space(10)]
        [Header(" Damage Settings")]
            // Damage Amount //
                [SerializeField] private float _damage;
                private float IO_Flt_Damage;
                public float Damage => IO_Flt_Damage;
        [Space(10)]
        [Header(" WindHorn Settings")]
            // Max Amount of Windhorn bar //
                [SerializeField] private float _maxWindhorn;
                private float IO_Flt_MaxWindhorn;
                public float MaxWindhorn => IO_Flt_MaxWindhorn;
            // Rate in which the Windhorn bar increases //
                [SerializeField] private float _windhornRegen;
                private float IO_Flt_WindhornRegen;
                public float WindhornRegen => IO_Flt_WindhornRegen;
            // Rate in which the Windhorn bar decreases //
                [SerializeField] private float _windhornSpoil;
                private float IO_Flt_WindhornSpoil;
                public float WindhornSpoil => IO_Flt_WindhornSpoil;
            // How long the game waits until Windhorn bar startes to decrease //
                [SerializeField] private float _WindhornDelay;
                private float IO_Flt_WindhornDelay;
                public float WindhornDelay => IO_Flt_WindhornDelay;
        [Space(10)]
        [Header(" PowerUp Settings")]
            // Range of posible values of PowerUp speed //
                [SerializeField] private float[] _powerUpSpeedLimits;
                private float[] IO_FltA1_PowerUpSpeedLimits;
                public float[] PowerUpSpeedLimits => IO_FltA1_PowerUpSpeedLimits;
            // Range of posible values of PowerUp lifespan //
                [SerializeField] private float[] _powerUpLifeLimits;
                private float[] IO_FltA1_PowerUpLifeLimits;
                public float[] PowerUpLifeLimits => IO_FltA1_PowerUpLifeLimits;
        [Space(10)]
        [Header(" Animation Settings")]
            // Speed of Every Character Animation //
                [SerializeField] private float[] _animationSpeed;
                private float[] IO_FltA1_AnimationSpeed;
                public float[] AnimationSpeed => IO_FltA1_AnimationSpeed;
        [Space(10)]
        [Header(" Game Settings")]
            // Wait time until shootout starts //
                [SerializeField] private float _relocationWait;
                private float IO_Flt_RelocationWait;
                public float RelocationWait => IO_Flt_RelocationWait;
            // Wait time until relocation starts //
                [SerializeField] private float _shootoutWait;
                private float IO_Flt_ShootoutWait;
                public float ShootoutWait => IO_Flt_ShootoutWait;
    #endregion Variables
    #region    Methods
    #region    Unity Methods
            protected override void Awake()
            {
                // Singleton Operations //
                    base.Awake();
                    OnStartUp();
            }
        #endregion Unity Methods
        #region    Override Methods
            #pragma warning disable CS1998
            public async override void OnStartUp()
            #pragma warning restore CS1998
            {
                // Resume Base //
                    base.OnStartUp();
                // Variables //
                    IO_Flt_MaxHealth = _maxHealth;
                    IO_Flt_HealthRegen = _healthRegen;
                    IO_Flt_Damage = _damage;
                    IO_Flt_MaxWindhorn = _maxWindhorn;
                    IO_Flt_WindhornRegen = _windhornRegen;
                    IO_Flt_WindhornSpoil = _windhornSpoil;
                    IO_Flt_WindhornDelay = _WindhornDelay;
                    IO_FltA1_PowerUpSpeedLimits = _powerUpSpeedLimits;
                    IO_FltA1_PowerUpLifeLimits = _powerUpLifeLimits;
                    IO_FltA1_AnimationSpeed = _animationSpeed;
                    IO_Flt_RelocationWait = _relocationWait;
                    IO_Flt_ShootoutWait = _shootoutWait;
            }
            #pragma warning disable CS1998
            public async override void OnUpdate(float Flt_DT)
            #pragma warning restore CS1998
            {
                // Resume Base //
                    base.OnUpdate(Flt_DT);
                // Variables //
            }
        #endregion Override Methods
        #region    Custom Methods
        #endregion Custom Methods
    #endregion Methods
}
