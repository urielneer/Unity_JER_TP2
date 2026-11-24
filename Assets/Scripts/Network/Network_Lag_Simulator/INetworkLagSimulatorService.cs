public interface INetworkLagSimulatorService
{
    void SetSimulatedPing(int latencyMs, int jitterMs);
    void SetPacketLoss(float lossPercentage);
    void DisableSimulation();
}