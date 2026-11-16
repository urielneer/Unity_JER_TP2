using UnityEngine;

public interface IVoiceChatService
{
    void SetTransmitEnabled(bool isEnabled);
    bool IsTransmitting();
}
