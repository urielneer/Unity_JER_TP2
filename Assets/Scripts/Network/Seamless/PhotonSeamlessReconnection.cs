using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

// 1. Interfaz para desacoplar el servicio de red (Interface Segregation / Dependency Inversion)
public interface INetworkReconnectionService
{
    event Action OnReconnectionSuccess;
    event Action OnReconnectionFailed;
    void AttemptReconnect();
}

// 2. Implementación concreta para Photon
public class PhotonSeamlessReconnection : MonoBehaviourPunCallbacks, INetworkReconnectionService
{
    public event Action OnReconnectionSuccess;
    event Action INetworkReconnectionService.OnReconnectionSuccess { add => OnReconnectionSuccess += value; remove => OnReconnectionSuccess -= value; }

    public event Action OnReconnectionFailed;
    event Action INetworkReconnectionService.OnReconnectionFailed { add => OnReconnectionFailed += value; remove => OnReconnectionFailed -= value; }

    private bool _isManualDisconnect = false;
    private string _cachedRoomName;

    public void SetTargetRoom(string roomName)
    {
        _cachedRoomName = roomName;
    }

    public void MarkAsManualDisconnect()
    {
        _isManualDisconnect = true;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (_isManualDisconnect) return;

        Debug.LogWarning($"[Reconnection] Desconexión detectada: {cause}. Intentando reconexión fluida...");
        AttemptReconnect();
    }

    public void AttemptReconnect()
    {
        if (!PhotonNetwork.ReconnectAndRejoin())
        {
            Debug.Log("[Reconnection] ReconnectedAndJoin falló. Reconectando al Master Server...");
            PhotonNetwork.Reconnect();
        }
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        if (!string.IsNullOrEmpty(_cachedRoomName))
        {
            Debug.Log("[Reconnection] Reconectado al Master. Reingresando a la sala...");
            PhotonNetwork.RejoinRoom(_cachedRoomName);
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("[Reconnection] ¡Reconexión a la sala exitosa!");
        OnReconnectionSuccess?.Invoke();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.LogError($"[Reconnection] Falló el reingreso a la sala: {message}");
        OnReconnectionFailed?.Invoke();
    }
}