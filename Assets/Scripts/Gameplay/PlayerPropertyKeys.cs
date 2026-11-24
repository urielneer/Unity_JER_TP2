// Claves de las Player Custom Properties (GDD: "Vida, balas y carga de Windhorn -> Dueno del personaje") //
// Cortas a proposito: viajan en cada actualizacion //
public static class PlayerPropertyKeys
{
    public const string Health     = "Hp";   // int   - Vida actual (0 = muerto) //
    public const string Bullets    = "Ammo"; // int   - Balas en el tambor //
    public const string HornCharge = "Horn"; // float - Carga de la Windhorn, 0..1 //
}
