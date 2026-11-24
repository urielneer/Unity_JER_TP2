using UnityEngine;
using UnityEngine.UI;

// Barra del HUD (vida / Windhorn) //
// - El valor se muestra con el fillAmount del "GUI Bar Filler": el Animator anima sprite, color y posicion, nunca el relleno, //
//   asi que el valor real y las animaciones de feedback conviven sin pisarse. //
// - Los parametros del Animator son opcionales: si el controller no los tiene, simplemente no se disparan. //
public class GUIBar : MonoBehaviour
{
    #region    Constants
        private const string FILLER_NAME = "GUI Bar Filler";
        // Parametros de los controllers "GUI Health Bars" y "GUI Windhorn Bars" //
        private static readonly int Hash_IsDamaged       = Animator.StringToHash("IsDamaged");
        private static readonly int Hash_IsHealing       = Animator.StringToHash("IsHealing");
        private static readonly int Hash_IsDead          = Animator.StringToHash("IsDead");
        private static readonly int Hash_WasWindhornUsed = Animator.StringToHash("WasWindhornUsed");
    #endregion Constants
    #region    Variables
        [Header(" Bar Settings")]
            [Tooltip("Imagen que se rellena. Si queda vacio se busca el hijo 'GUI Bar Filler'")]
            [SerializeField] private Image _filler;
            [Tooltip("Si queda vacio se usa el Animator del mismo objeto")]
            [SerializeField] private Animator _animator;
            [SerializeField] private Image.FillMethod _fillMethod = Image.FillMethod.Horizontal;
        private Image I_Img_Filler;
        private Animator I_Anim_Bar;
        private bool R_Bool_IsInitialized;
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            private void Awake()
            {
                Initialize();
            }
        #endregion Unity Methods
        #region    API
            // Valor 0..1 //
            public void SetValue(float Flt_Normalized)
            {
                Initialize();
                if (I_Img_Filler == null) return;
                I_Img_Filler.fillAmount = Mathf.Clamp01(Flt_Normalized);
            }
            public void PlayDamage()         { SetAnimatorBool(Hash_IsDamaged, true); }
            public void PlayHeal()           { SetAnimatorBool(Hash_IsHealing, true); }
            public void SetDead(bool Bool_IsDead) { SetAnimatorBool(Hash_IsDead, Bool_IsDead); }
            public void PlayWindhornUsed()   { SetAnimatorBool(Hash_WasWindhornUsed, true); }
        #endregion API
        #region    Animation Events (llamados desde los clips)
            public void AnimOver()
            {
                SetAnimatorBool(Hash_IsDamaged, false);
                SetAnimatorBool(Hash_IsHealing, false);
            }
            public void WindhornEmptyOver()
            {
                SetAnimatorBool(Hash_WasWindhornUsed, false);
            }
        #endregion Animation Events (llamados desde los clips)
        #region    Internals
            private void Initialize()
            {
                if (R_Bool_IsInitialized) return;
                R_Bool_IsInitialized = true;
                // Referencias //
                    I_Anim_Bar = _animator != null ? _animator : GetComponent<Animator>();
                    I_Img_Filler = _filler != null ? _filler : FindFiller();
                    if (I_Img_Filler == null)
                    {
                        Debug.LogWarning("[GUIBar] '" + name + "' no tiene '" + FILLER_NAME + "': la barra no va a mostrar el valor", this);
                        return;
                    }
                // La imagen del prefab es "Simple": se pasa a "Filled" para poder mostrar el porcentaje //
                    I_Img_Filler.type = Image.Type.Filled;
                    I_Img_Filler.fillMethod = _fillMethod;
                    I_Img_Filler.fillOrigin = 0;
            }
            private Image FindFiller()
            {
                foreach (Image Img_Child in GetComponentsInChildren<Image>(true))
                {
                    if (Img_Child.name == FILLER_NAME) return Img_Child;
                }
                return null;
            }
            private void SetAnimatorBool(int Int_Hash, bool Bool_Value)
            {
                Initialize();
                // Sin controller asignado (o sin ese parametro) no se hace nada: no tira warnings por frame //
                if (I_Anim_Bar == null || I_Anim_Bar.runtimeAnimatorController == null) return;
                if (!HasParameter(Int_Hash)) return;
                I_Anim_Bar.SetBool(Int_Hash, Bool_Value);
            }
            private bool HasParameter(int Int_Hash)
            {
                AnimatorControllerParameter[] A1_Parameters = I_Anim_Bar.parameters;
                for (int Int_Index = 0; Int_Index < A1_Parameters.Length; Int_Index++)
                {
                    if (A1_Parameters[Int_Index].nameHash == Int_Hash) return true;
                }
                return false;
            }
        #endregion Internals
    #endregion Methods
}
