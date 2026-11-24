using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;

// Photon Lag Simulator (GDD: "Todos estos casos se prueban con el Photon Lag Simulator") //
// - Valores por Inspector: se aplican al iniciar Y en vivo al editarlos con el juego corriendo (OnValidate). //
// - Ventana en pantalla de PUN (PhotonLagSimulationGui) con RTT real y sliders: F8 la muestra / oculta. //
// - F9 simula un corte total para probar la reconexion seamless. //
// Las herramientas de debug solo existen en el Editor y en Development Builds. //
public class NetworkLagSimulatorController : MonoBehaviourPunCallbacks, INetworkLagSimulatorService
{
    [Header("Configuración de Red Simulada")]
    [SerializeField] private bool _enableSimulation = false;
    [SerializeField] private int _simulatedPing = 150;
    [SerializeField] private int _simulatedJitter = 30;
    [SerializeField] private int _packetLossPercentage = 0;

    [Header("Debug (solo Editor / Development Build)")]
    [Tooltip("Muestra / oculta la ventana de simulación de PUN (RTT, lag, jitter, pérdida)")]
    [SerializeField] private Key _toggleWindowKey = Key.F8;
    [Tooltip("Arrancar con la ventana visible")]
    [SerializeField] private bool _showWindowOnStart = false;
    [Tooltip("Corta la conexión para probar la reconexión seamless")]
    [SerializeField] private Key _simulateConnectionLossKey = Key.F9;

    private PhotonLagSimulationGui _window;

    private void Start()
    {
        ApplySimulationState();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Ventana de PUN creada por código: no hace falta tocar la escena ni el prefab //
        _window = gameObject.GetComponent<PhotonLagSimulationGui>();
        if (_window == null) _window = gameObject.AddComponent<PhotonLagSimulationGui>();
        _window.Visible = _showWindowOnStart;
#endif
    }

    // Cambios en el Inspector con el juego corriendo: se aplican al instante //
    private void OnValidate()
    {
        if (Application.isPlaying) ApplySimulationState();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (_window != null && keyboard[_toggleWindowKey].wasPressedThisFrame)
            _window.Visible = !_window.Visible;

        if (keyboard[_simulateConnectionLossKey].wasPressedThisFrame && PhotonNetwork.IsConnected)
        {
            // Pierde el 100% de los paquetes: Photon detecta un ClientTimeout como en un corte real //
            Debug.LogWarning("[LagSimulator] Simulando corte de conexión (" + _simulateConnectionLossKey + ")");
            PhotonNetwork.NetworkingClient.SimulateConnectionLoss(true);
        }
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
