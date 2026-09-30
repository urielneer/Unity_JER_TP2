using UnityEngine;

public class MenuTypeError : BaseMenuType
{
    #region    Variables
        [Header("Error GUI Settings")]
            [SerializeField] private BaseMenuLabel _error;
            private BaseMenuLabel Ctm_BML_Error;    
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Resume Base //
                    base.Awake();
                // Variables //
                    Ctm_BML_Error = _error;
            }
        #endregion Unity Methods
        #region    Override Methods
            public void PrintError(string Str_Message)
            {
                Ctm_BML_Error.Overwrite(Str_Message);
            }
        #endregion OverrideMethods
    #endregion Methods
}
