using UnityEngine;

public class GUIBar : MonoBehaviour
{
    #region    Variables
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected virtual void Awake()
            {
            }
        #endregion Unity Methods
        #region    Override Methods
            #pragma warning disable CS1998
            public async virtual void OnStartUp()
            #pragma warning restore CS1998
            {
                // Variables //
            }
            #pragma warning disable CS1998
            public async virtual void OnUpdate(float Flt_DT)
            #pragma warning restore CS1998
            {
                // Variables //
            }
        #endregion Override Methods
        #region    Custom Methods
            #region    Animation
                public virtual void AnimOver()
                {

                }
            #endregion Animation
        #endregion Custom Methods
    #endregion Methods
}
