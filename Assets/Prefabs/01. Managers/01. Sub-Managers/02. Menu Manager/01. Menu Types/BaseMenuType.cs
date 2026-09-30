using Photon.Pun;
using UnityEngine;

public class BaseMenuType : MonoBehaviourPunCallbacks
{
    #region    Variables
        #region    Manager-To-Manager Data
            private string O_Str_MenuName = "";
            public string MenuName => O_Str_MenuName;

            private bool O_Bool_IsOpen = false;
            public bool IsOpen => O_Bool_IsOpen;
        #endregion Manager-To-Manager Data
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected virtual void Awake()
            {
                // Variables //
                    O_Str_MenuName = this.gameObject.name;
            }
        #endregion Unity Methods
        #region    Custom Methods
            public void Open()
            {
                O_Bool_IsOpen = true;
                gameObject.SetActive(true);
            }
            public void Close()
            {
                O_Bool_IsOpen = false;
                gameObject.SetActive(false);
            }
        #endregion Custom Methods
    #endregion Methods
}
