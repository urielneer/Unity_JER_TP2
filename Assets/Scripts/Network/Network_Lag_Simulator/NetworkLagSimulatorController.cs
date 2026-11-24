using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;

public class NetworkLagSimulatorController : MonoBehaviourPunCallbacks, INetworkLagSimulatorService
{
    [Header("Configuración de Red Simulada")]
    [SerializeField] private bool _enableSimulation = false;
    [SerializeField] private int _simulatedPing = 150;
    [SerializeField] private int _simulatedJitter = 30;
    [SerializeField] private int _packetLossPercentage = 0;

    [Header("Debug - Corte de conexión (solo Editor / Development Build)")]
    [Tooltip("Corta la conexión para probar la reconexión seamless")]
    [SerializeField] private Key _simulateConnectionLossKey = Key.F9;

    private void Start()
    {
        ApplySimulationState();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard[_simulateConnectionLossKey].wasPressedThisFrame) return;
        if (!PhotonNetwork.IsConnected) return;

        // Pierde el 100% de los paquetes: Photon detecta un ClientTimeout como en un corte real //
        Debug.LogWarning("[LagSimulator] Simulando corte de conexión (" + _simulateConnectionLossKey + ")");
        PhotonNetwork.NetworkingClient.SimulateConnectionLoss(true);
    }
#endif

    // Al desconectarse se restaura la configuración elegida: sino la reconexión también perdería todos los paquetes //
    public override void OnDisconnected(DisconnectCause cause)
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
