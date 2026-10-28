using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UIElements;
using static CustomExtension.ArrayExtensions;

public class BasePlayer : MonoBehaviourPunCallbacks
{
    #region    Variables
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected virtual void Awake()
            {
            }
            public virtual void OnUpdate(float Flt_DT)
            {
            }
        #endregion Unity Methods
    #endregion Methods    
}
