using UnityEngine;
using UnityEngine.UIElements;

public class GUIBar : MonoBehaviour
{
    #region    Variables   
        [Header(" Animator Settings")]
            [SerializeField] Animator _animator;
        [Space(10)]
        [Header(" Bar Settings")]
            [SerializeField] Slider _bar;
            protected Slider I_Slider_Bar;
            protected float I_Flt_Max;
            public float Max => I_Flt_Max;
            protected float R_Flt_Current;
            public float Current => R_Flt_Current;
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected virtual void Awake()
            {
                I_Slider_Bar = _bar;
            }
        #endregion Unity Methods
        #region    Override Methods
            #pragma warning disable CS1998
            public async virtual void OnStartUp()
            #pragma warning restore CS1998
            {
                // Variables //
                    R_Flt_Current = I_Flt_Max;
                    I_Slider_Bar.highValue = I_Flt_Max;
            }
            #pragma warning disable CS1998
            public async virtual void OnUpdate(float Flt_DT)
            #pragma warning restore CS1998
            {
                if(I_Slider_Bar.value != R_Flt_Current) 
                {
                    I_Slider_Bar.value = R_Flt_Current;
                }
            }
        #endregion Override Methods
        #region    Custom Methods
            #region    Bar
                public virtual void Increase()
                {
                    
                }
                public virtual void Reduce()
                {

                }
            #endregion Bar
            #region    Animation
                public virtual void AnimOver()
                {

                }
            #endregion Animation
        #endregion Custom Methods
    #endregion Methods
}
