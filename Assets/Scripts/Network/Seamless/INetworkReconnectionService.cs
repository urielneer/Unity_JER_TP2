using System;

// Contrato del servicio de reconexion - El resto del juego depende de esto, no de Photon //
public interface INetworkReconnectionService
{
    event Action<int, int> OnReconnectionAttempt; // (intento actual, intentos maximos) //
    event Action OnReconnectionSuccess;
    event Action OnReconnectionFailed;

    bool IsReconnecting { get; }

    // Se llama ANTES de salir de una sala a proposito (boton Salir, fin de partida) //
    void MarkIntentionalLeave();
}
