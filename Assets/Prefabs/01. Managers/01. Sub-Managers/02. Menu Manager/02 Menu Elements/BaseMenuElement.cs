using UnityEngine;
using UnityEngine.UI;
using CustomExtension;
public class BaseMenuElement : MonoBehaviour
{
    #region    Variables
        protected ElementMasterContainer MasterContainer = null;
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected virtual void Awake()
            {
                // Variables //
            }
            protected virtual void OnValidate()
            {
                // Variables //
            }
            protected virtual void OnEnabled()
            {
            }
            protected virtual void OnDisabled()
            {
            }
        #endregion Unity Methods
        #region    Override Methods
            public virtual void Overwrite(string Str_Input) {}
            public virtual void Overwrite(object Obj_Input) {}
            public virtual void RedrawElement() {}
            public virtual void SetMasterContainer(GameObject GObj_Master) 
            {
                if (GObj_Master != null)
                {
                    MasterContainer = GObj_Master.GetComponent<ElementMasterContainer>();
                }
            }
            public virtual void SetColor(Color Clr_ColorA, Color Clr_ColorB = new Color(), Color Clr_ColorC = new Color()) {}
        #endregion Override Methods
    #endregion Methods
}
