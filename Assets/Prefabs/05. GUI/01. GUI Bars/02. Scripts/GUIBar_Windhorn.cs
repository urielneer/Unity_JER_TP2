using UnityEngine;

public class GUIBar_Windhorn : GUIBar
{
    #region    Variables
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                base.Awake();
            }
        #endregion Unity Methods
        #region    Override Methods
            #pragma warning disable CS1998
            public async override void OnStartUp()
            #pragma warning restore CS1998
            {
                I_Flt_Max = MasterManager.Instance.CharacterManager.MaxHealth;
                base.OnStartUp();
                // Variables //
            }
            #pragma warning disable CS1998
            public async override void OnUpdate(float Flt_DT)
            #pragma warning restore CS1998
            {
                base.OnUpdate(Flt_DT);
                // Variables //
            }
        #endregion Override Methods
        #region    Custom Methods
            #region    Bar
                public override void Increase()
                {
                    base.Increase();
                    R_Flt_Current += MasterManager.Instance.CharacterManager.WindhornRegen;
                }
                public override void Reduce()
                {
                    base.Reduce();
                    R_Flt_Current -= I_Flt_Max;
                }
            #endregion Bar
            #region    Animation
                public override void AnimOver()
                {
                    base.AnimOver();
                }
                public void WindhornEmptyOver()
                {

                }
            #endregion Animation
        #endregion Custom Methods
    #endregion Methods
}
