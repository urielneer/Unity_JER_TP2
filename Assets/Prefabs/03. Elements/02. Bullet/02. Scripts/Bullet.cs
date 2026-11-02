using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    #region    Variables
        [Header(" Bullet Details")]
            bool R_Bool_WassShotVerified = false; 
            [SerializeField] private float _radius;
            private float I_Flt_Radius;
            [SerializeField] private LayerMask _mask;
            private LayerMask I_LMsk_Target;
        [Space(10)]
        [Header(" Damage Settings")]
            bool R_Bool_ValidShot = true; 
            GameObject R_GObj_Target = null;
    #endregion Variables
    #region    Methods
        #region    Override Methods
            protected virtual void Awake()
            {
                OnStartUp();
            }
            #pragma warning disable CS1998
            public async void OnStartUp()
            #pragma warning restore CS1998
            {
                I_Flt_Radius = _radius;
                I_LMsk_Target = _mask;

                R_Bool_ValidShot = false; 
                R_Bool_WassShotVerified = false;
                R_GObj_Target = null;
            }
            #pragma warning disable CS1998
            public async void OnUpdate(float Flt_DT)
            #pragma warning restore CS1998
            {
                #region    Damage
                    // Has The Verification happen already //
                        if (R_Bool_WassShotVerified)
                            return;
                    // Temp Variables //
                        Vector2 Vec2_CheckPosition = new Vector2(this.transform.position.x, this.transform.position.y);
                    // Check Collisions //
                        Collider2D[] Col2D_A1_Hit = Physics2D.OverlapCircleAll
                        (
                            Vec2_CheckPosition,
                            I_Flt_Radius,
                            I_LMsk_Target
                        );
                    // Verifications //
                        // Verify That The Collision Is A PowerUp //
                            foreach (Collider2D Col2D_Hit in Col2D_A1_Hit)
                            {
                                if (Col2D_Hit.gameObject.layer == 9) 
                                {
                                    R_GObj_Target = Col2D_Hit.gameObject;
                                    R_Bool_ValidShot = true; 
                                    R_Bool_WassShotVerified = true;
                                    return;
                                }
                            }
                        // Verify That No Collision Is An Obstacle //
                            foreach (Collider2D Col2D_Hit in Col2D_A1_Hit)
                            {
                                if (Col2D_Hit.gameObject.layer == 7) 
                                {
                                    R_Bool_ValidShot = false;   
                                    R_Bool_WassShotVerified = true; 
                                    return;
                                }
                            }
                        // Verify That The Collision Is The Enemy //
                            foreach (Collider2D Col2D_Hit in Col2D_A1_Hit)
                            {
                                if (Col2D_Hit.gameObject.layer == 6) 
                                {
                                    R_GObj_Target = Col2D_Hit.gameObject;
                                    R_Bool_ValidShot = true; 
                                    R_Bool_WassShotVerified = true;
                                    return;
                                }
                            }
                #endregion Damage
            }
        #endregion Override Methods
        #region    Animation
            public void ImpactStart()
            {
                // Has Hit Player ? //
                    if (R_Bool_ValidShot)
                    {
                        if (R_GObj_Target.TryGetComponent<PowerUp>(out PowerUp Pup_Target))
                        {
Debug.Log("PowerUpShot");
                        }
                        else
                        if (R_GObj_Target.transform.parent.gameObject.TryGetComponent<EnemyCharacter>(out EnemyCharacter EChar_Target))
                        {
                            EChar_Target.Damage();
                        }
                    }
                    else
                    {
Debug.Log("ObstacleShot");
                    }
            }
            public void ImpactOver()
            {
                // Self Destruct //
/*?*/               ((PlayerCharacter)(MasterManager.Instance.CharacterManager.OwnPlayer)).ResetCanshootFlag();
                    Destroy(this.gameObject);
            }
        #endregion Animation
    #endregion Methods
}
