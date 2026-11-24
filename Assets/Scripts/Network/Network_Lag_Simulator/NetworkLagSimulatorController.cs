using UnityEngine;
using Photon.Pun;

public class NetworkLagSimulatorController : MonoBehaviour, INetworkLagSimulatorService
{
    [Header("Configuración de Red Simulada")]
    [SerializeField] private bool _enableSimulation = false;
    [SerializeField] private int _simulatedPing = 150;
    [SerializeField] private int _simulatedJitter = 30;
    [SerializeField] private int _packetLossPercentage = 0;

    private void Start()
    {
        ApplySimulationState();
    }

    public void SetSimulatedPing(int latencyMs, int jitterMs)
    {
        _simulatedPing = latencyMs;
        _simulatedJitter = jitterMs;
        ApplySimulationState();
    }

    public void SetPacketLoss(float lossPercentage)
    {
        _packetLossPercentage = Mathf.RoundToInt(lossPercentage);
        ApplySimulationState();
    }

    public void DisableSimulation()
    {
        _enableSimulation = false;
        ApplySimulationState();
    }

    private void ApplySimulationState()
    {
        if (PhotonNetwork.NetworkingClient != null && PhotonNetwork.NetworkingClient.LoadBalancingPeer != null)
        {
            var peer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;

            // Activamos o desactivamos la simulación en el peer de bajo nivel de Photon
            peer.IsSimulationEnabled = _enableSimulation;

            if (_enableSimulation)
            {
                // Configuramos los valores artificiales de red para probar los duelos
                peer.NetworkSimulationSettings.IncomingLag = _simulatedPing;
                peer.NetworkSimulationSettings.OutgoingLag = _simulatedPing;
                peer.NetworkSimulationSettings.IncomingJitter = _simulatedJitter;
                peer.NetworkSimulationSettings.OutgoingJitter = _simulatedJitter;
                peer.NetworkSimulationSettings.IncomingLossPercentage = _packetLossPercentage;
                peer.NetworkSimulationSettings.OutgoingLossPercentage = _packetLossPercentage;

                Debug.Log($"[LagSimulator] Simulación ACTIVA -> Ping: {_simulatedPing}ms | Jitter: {_simulatedJitter}ms | Pérdida: {_packetLossPercentage}%");
            }
            else
            {
                Debug.Log("[LagSimulator] Simulación DESACTIVADA. Red limpia.");
            }
        }
    }
}