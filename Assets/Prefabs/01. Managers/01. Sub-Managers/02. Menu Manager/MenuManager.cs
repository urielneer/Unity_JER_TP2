using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : BaseManager<CharacterManager>
{
    #region    Variables
        #region    Menu
            [SerializeField] private BaseMenuType[] _BaseMenuType;
            private BaseMenuType[] I_Ctm_Memt_A1_Menus;
            private BaseMenuType O_Ctm_Memt_Current;
            public BaseMenuType CurrentMenu => O_Ctm_Memt_Current;
        #endregion Menu
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Singleton Operations //
                    base.Awake();
                // Variables //
                    I_Ctm_Memt_A1_Menus = _BaseMenuType;
                // Botones que no estan cableados desde el Inspector //
                    WireRuntimeButtons();
            }
            // El boton "Leave Game" del PauseMenu tiene el OnClick vacio en el prefab //
            private void WireRuntimeButtons()
            {
                foreach (Button Btn_Element in GetComponentsInChildren<Button>(true))
                {
                    // Solo el de salir //
                        if (Btn_Element.gameObject.name != "Btn_LeaveGame") continue;
                    // Si ya lo cablearon a mano en el Inspector, no tocarlo //
                        if (Btn_Element.onClick.GetPersistentEventCount() > 0) continue;
                    // Conectar //
                        Btn_Element.onClick.AddListener(() =>
                        {
                            if (MasterManager.Instance == null) return;
                            if (MasterManager.Instance.NetworkManager == null) return;
                            MasterManager.Instance.NetworkManager.LeaveGame();
                        });
                        Debug.Log("[Menu Manager] Boton 'Leave Game' conectado por codigo");
                }
            }
        #endregion Unity Methods
        #region    Override Methods
            #pragma warning disable CS1998
            public async override void OnStartUp()
            #pragma warning restore CS1998
            {
                // Resume Base //
                    base.OnStartUp();
                // Any Scene Behaviour //
                    this.gameObject.GetComponent<Canvas>().worldCamera = Object.FindFirstObjectByType<Camera>();
                // Specific Scene Behaviour //
                    switch (SceneManager.GetActiveScene().buildIndex) 
                    {
                        case 2: // Level //
                            // Transfer Map Bounds //
                        break;
                    }
            }
            public override void OnFixedUpdate(float Flt_FixedDT)
            {
                // Resume Base //
                    base.OnFixedUpdate(Flt_FixedDT);
                // Screen Resiza //
                    
            }
        #endregion Override Methods
        #region    Custom Methods
            public BaseMenuType GetMenuReference(string Str_MenuName)
            {
                foreach (BaseMenuType Ctm_Memt_Menu in I_Ctm_Memt_A1_Menus)
                {
                    if (Ctm_Memt_Menu.MenuName == Str_MenuName) return Ctm_Memt_Menu;
                }
                return null;
            }
            public void OpenMenu(string Str_MenuName)
            {
                foreach (BaseMenuType Ctm_Memt_Menu in I_Ctm_Memt_A1_Menus)
                {
                    if (Ctm_Memt_Menu.MenuName == Str_MenuName) OpenMenu(Ctm_Memt_Menu);
                    else if (Ctm_Memt_Menu.IsOpen)              CloseMenu(Ctm_Memt_Menu);
                }
            }
            public void OpenMenu(BaseMenuType Ctm_MEmt_Menu)
            {

                foreach (BaseMenuType Ctm_Memt_MenuToClose in I_Ctm_Memt_A1_Menus)
                {
                    CloseMenu(Ctm_Memt_MenuToClose);
                }
                Ctm_MEmt_Menu.Open();
                O_Ctm_Memt_Current = Ctm_MEmt_Menu;
            }
            public void CloseMenu(BaseMenuType Ctm_MEmt_Menu)
            {
                Ctm_MEmt_Menu.Close();
            }
            public void QuitGame()
            {  
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                    return;
                #endif
                #pragma warning disable CS0162
                    Application.Quit();
                #pragma warning restore CS0162
            }
            public void SwitchScene(int Int_Index)
            {
                SceneManager.LoadScene(Int_Index);
            }
        #endregion Custom Methods
    #endregion Methods
}