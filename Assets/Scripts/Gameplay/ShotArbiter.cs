using System;
using System.Collections.Generic;

// Impacto reportado al Master //
public readonly struct ShotReport
{
    public readonly int ShooterActor;
    public readonly int VictimActor;
    public readonly double ShotTime; // PhotonNetwork.Time del momento del disparo (ya validado por el Master) //

    public ShotReport(int Int_Shooter, int Int_Victim, double Dbl_ShotTime)
    {
        ShooterActor = Int_Shooter;
        VictimActor = Int_Victim;
        ShotTime = Dbl_ShotTime;
    }
}

// Motivo por el que un impacto no cuenta //
public enum ShotAnnulReason
{
    HitFirst = 0, // El tirador ya habia recibido un impacto con timestamp anterior //
    ExactTie = 1, // GDD: "Empate exacto de timestamps: nadie recibe dano" //
}

// Lag compensation (GDD) - Solo lo usa el MasterClient //
// "Gana quien disparo primero de verdad, no quien tiene mejor ping": //
// se comparan timestamps de servidor (no el orden de llegada) y se espera una ventana corta antes de resolver, //
// para que el disparo de un jugador con mas latencia llegue a tiempo de competir. //
public sealed class ShotArbiter
{
    #region    Variables
        private readonly double I_Dbl_Window;
        private readonly List<ShotReport> R_Lst_Pending = new List<ShotReport>(4);
        private double R_Dbl_ResolveAt = double.MaxValue;
        private static readonly Comparison<ShotReport> Cmp_ByTime = (Ctm_SR_A, Ctm_SR_B) => Ctm_SR_A.ShotTime.CompareTo(Ctm_SR_B.ShotTime);
    #endregion Variables
    #region    Events
        public event Action<ShotReport> OnShotConfirmed;
        public event Action<ShotReport, ShotAnnulReason> OnShotAnnulled;
    #endregion Events
    #region    Properties
        public int PendingCount => R_Lst_Pending.Count;
    #endregion Properties
    #region    Methods
        public ShotArbiter(float Flt_WindowSeconds)
        {
            I_Dbl_Window = Math.Max(0d, Flt_WindowSeconds);
        }
        public void Report(ShotReport Ctm_SR_Report, double Dbl_Now)
        {
            R_Lst_Pending.Add(Ctm_SR_Report);
            // La ventana se mide desde el disparo, no desde la llegada: un impacto que llega tarde no extiende la espera //
            // pero siempre se espera al menos un tick para juntar reportes que llegan en el mismo frame //
            double Dbl_Candidate = Math.Max(Ctm_SR_Report.ShotTime + I_Dbl_Window, Dbl_Now);
            if (Dbl_Candidate < R_Dbl_ResolveAt) R_Dbl_ResolveAt = Dbl_Candidate;
        }
        public void Tick(double Dbl_Now)
        {
            if (R_Lst_Pending.Count == 0 || Dbl_Now < R_Dbl_ResolveAt) return;
            Resolve();
        }
        // Al cambiar de Master o terminar la partida, los pendientes se descartan //
        public void Clear()
        {
            R_Lst_Pending.Clear();
            R_Dbl_ResolveAt = double.MaxValue;
        }
        private void Resolve()
        {
            R_Lst_Pending.Sort(Cmp_ByTime);
            ShotReport Ctm_SR_First = R_Lst_Pending[0];
            // Empate exacto entre tiradores distintos: nadie recibe dano //
                bool Bool_IsTie = R_Lst_Pending.Count > 1
                                  && R_Lst_Pending[1].ShotTime == Ctm_SR_First.ShotTime
                                  && R_Lst_Pending[1].ShooterActor != Ctm_SR_First.ShooterActor;
                if (Bool_IsTie)
                {
                    for (int Int_Index = 0; Int_Index < R_Lst_Pending.Count; Int_Index++)
                        OnShotAnnulled?.Invoke(R_Lst_Pending[Int_Index], ShotAnnulReason.ExactTie);
                    Clear();
                    return;
                }
            // Gana el primero. Los disparos de quien recibio ese impacto se anulan (disparo despues de ser alcanzado) //
                OnShotConfirmed?.Invoke(Ctm_SR_First);
                for (int Int_Index = 1; Int_Index < R_Lst_Pending.Count; Int_Index++)
                {
                    ShotReport Ctm_SR_Other = R_Lst_Pending[Int_Index];
                    if (Ctm_SR_Other.ShooterActor == Ctm_SR_First.VictimActor) OnShotAnnulled?.Invoke(Ctm_SR_Other, ShotAnnulReason.HitFirst);
                    else                                                         OnShotConfirmed?.Invoke(Ctm_SR_Other);
                }
                Clear();
        }
    #endregion Methods
}
