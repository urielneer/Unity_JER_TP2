using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using static CustomExtension.ArrayExtensions;
using System;

public class AudioManager : BaseManager<CharacterManager>
{
    #region    Variables
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Singleton Operations //
                    base.Awake();
            }
        #endregion Unity Methods
        #region    Override Methods
            #pragma warning disable CS1998
            public async override void OnStartUp()
            #pragma warning restore CS1998
            {
                // Resume Base //
                    base.OnStartUp();
                // Variables //
            }
            #pragma warning disable CS1998
            public async override void OnUpdate(float Flt_DT)
            #pragma warning restore CS1998
            {
                // Resume Base //
                    base.OnUpdate(Flt_DT);
                // Variables //
            }
        #endregion Override Methods
        #region    Custom Methods
        #endregion Custom Methods
    #endregion Methods
}
