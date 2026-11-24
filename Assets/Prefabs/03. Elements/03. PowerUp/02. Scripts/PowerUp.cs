using UnityEngine;

public class PowerUp : MonoBehaviour
{
    #region    Variables
        #region    Controls 
            [Header(" Character Settings")]
                // Input //
                    [SerializeField] private Animator _animator;
                    private Animator I_Anim_Character;
                // Input/Output //
                    [SerializeField] private int _powerUpID;
                    private int IO_Int_PowerUpID = 0;
                    public int PowerUpID => IO_Int_PowerUpID;
                    [SerializeField] private int _direction;
                    private int IO_Int_Direction = 0;
                    public int Direction => IO_Int_Direction;
        #endregion Controls 
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected void Awake()
            {
                OnStartUp();
            }
        #endregion Unity Methods
        #region    Override Methods
            #pragma warning disable CS1998
            public async void OnStartUp()
            #pragma warning restore CS1998
            {
                // Variables //
                    I_Anim_Character = _animator;
                    IO_Int_PowerUpID = _powerUpID;
                    IO_Int_Direction = _direction;
                    SetAppearance(IO_Int_PowerUpID, IO_Int_Direction);
            }
            #pragma warning disable CS1998
            public async void OnUpdate(float Flt_DT)
            #pragma warning restore CS1998
            {
                #region    Movement
                #endregion Movement
            }
        #endregion Override Methods
        #region    Spawn Methods
            public void SetAppearance(int Int_Index, int Int_Dir)
            {
                IO_Int_PowerUpID = Int_Index;
                IO_Int_Direction = Int_Dir;
                UpdateAnimator();
            }
        #endregion Spawn Methods
        #region    Animation Methods
            #region    Reusables
                public void UpdateAnimator()
                {
                    // Values //
                        I_Anim_Character.SetInteger("PowerUp"  , IO_Int_PowerUpID);
                        I_Anim_Character.SetInteger("Direction", IO_Int_Direction);
                    // Proceed //
                        I_Anim_Character.speed = 0f;
                        I_Anim_Character.Update(0f);
                        I_Anim_Character.speed = 1f;
                }
            #endregion Reusables
        #endregion Animation Methods
    #endregion Methods
}
