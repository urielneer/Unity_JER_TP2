using System;
using UnityEngine;

// Parametros de balance del duelo - Se editan en el Inspector del Character Manager //
// GDD: "Vida maxima y dano por impacto" se van a poder pisar desde Live Ops sin hacer un build //
[Serializable]
public class DuelBalance
{
    [Header(" Vida")]
        [Min(1)]  public int MaxHealth    = 100;
        [Min(0)]  public int DamagePerHit = 35;  // 3 impactos para ganar //
        [Min(0)]  public int HealAmount   = 35;  // Power-Up de Salud (nunca supera la vida maxima) //
    [Header(" Tambor")]
        [Min(1)]  public int MaxBullets   = 6;   // GDD: "el tambor tiene 6 balas" //
        [Min(0)]  public int StartBullets = 6;
        [Min(1)]  public int BulletsPerReload = 1; // GDD: "recargar una bala por vez" //
    [Header(" Windhorn")]
        [Tooltip("Segundos para cargar la barra de la Windhorn de 0 a 1")]
        [Min(0.1f)] public float HornChargeSeconds = 15f;
        [Tooltip("Cada cuanto se publica la carga por red (0.1 = cada 10%)")]
        [Range(0.01f, 1f)] public float HornPublishStep = 0.1f;

    // Copia independiente: Live Ops modifica una copia, no el asset/prefab //
    public DuelBalance Clone() => (DuelBalance) MemberwiseClone();
}
