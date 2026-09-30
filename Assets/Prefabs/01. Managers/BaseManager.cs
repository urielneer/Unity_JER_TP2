using Photon.Pun;
using UnityEngine;
using static CustomExtension.ArrayExtensions;

// This generic class allows the proper reusability of the singletons //
public class BaseManager<GenericManager> : MonoBehaviourPunCallbacks where GenericManager : MonoBehaviourPunCallbacks
{
    #region    Variables
        #region    Singleton
            protected static GenericManager _instance;
            public static GenericManager Instance => _instance;
        #endregion Singleton
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected virtual void Awake()
            { 
                // Is there no other instance of this singleton present? //
                    if (_instance == null)
                    {
                        _instance = this as GenericManager;

                        if (transform.parent == null)
                        {
                            DontDestroyOnLoad(gameObject);
                        }
                    }
                // Is there an instance of this singleton present? //
                    else if (_instance != this) 
                    {
                        Destroy(gameObject);
                    }
            }
        #endregion Unity Methods
        #region    Singleton Methods
            public virtual GenericManager GetInstance()
            {
                return _instance;
            }
        #endregion Singleton Methods
        #region    Custom Methods
            public virtual void OnStartUp()
            {
            }
            public virtual void OnFixedUpdate(float Flt_FixedDT)
            {
            }
            public virtual void OnUpdate(float Flt_FixedDT)
            {
            }
        #endregion Custom Methods
    #endregion Methods
}
