using UnityEngine;
using TMPro;

// Texto de UI que se traduce solo y se actualiza al cambiar de idioma //
public class LocalizedTextUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _uiText;
    [SerializeField] private string _tableKey = GameLocalization.MAIN_TABLE;
    [SerializeField] private string _entryKey;

    // Suscripcion solo mientras el objeto esta activo: los menus se prenden y apagan todo el tiempo //
    private void OnEnable()
    {
        GameLocalization.Service.OnLanguageChanged += UpdateText;
        UpdateText();
    }

    private void OnDisable()
    {
        GameLocalization.Service.OnLanguageChanged -= UpdateText;
    }

    private void UpdateText()
    {
        if (_uiText == null || string.IsNullOrEmpty(_entryKey)) return;
        // Si la tabla todavia no cargo, se deja el texto que trae el prefab (nunca "[Clave]") //
        _uiText.text = GameLocalization.Service.GetLocalizedValue(_tableKey, _entryKey, _uiText.text);
    }
}
