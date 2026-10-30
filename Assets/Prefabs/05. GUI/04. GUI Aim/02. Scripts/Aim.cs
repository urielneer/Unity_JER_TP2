using UnityEngine;
using System.Collections.Generic;
using static CustomExtension.ArrayExtensions;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine.InputSystem;

#region    ...
#endregion ...

public class Aim : MonoBehaviour
{
    #region    Variables
        #region    Menu
            [Header(" Cursor Move")]
                [SerializeField] private RectTransform _cursor;
                private RectTransform IO_RTfm_Cursor;
                public RectTransform AimCursor => IO_RTfm_Cursor;

                [SerializeField] private Canvas _canvas;     
                private Canvas I_Cvs_Scene;
            [Space(10)]
            [Header(" BulletShoot")]
                [SerializeField] private GameObject _bulletContainer;     
                private GameObject I_GObj_BulletContainer;
                [SerializeField] private Bullet _bullet;     
                private Bullet I_Bllt_Prefab;
        #endregion Menu
        #region    Manager-To-Manager Data
        #endregion Manager-To-Manager Data
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            void Awake()
            {
                // Variables //
                    IO_RTfm_Cursor = _cursor;
                    I_Cvs_Scene = _canvas;
                    I_GObj_BulletContainer = _bulletContainer; 
                    I_Bllt_Prefab = _bullet;
            }
        #endregion Unity Methods
        #region    Override Methods
            #pragma warning disable CS1998
            public async void OnStartUp()
            #pragma warning restore CS1998
            {

            }
            #pragma warning disable CS1998
            public async void OnUpdate(float Flt_DT)
            #pragma warning restore CS1998
            {
                IO_RTfm_Cursor.anchoredPosition = GetAimPosition();
            }
        #endregion Override Methods
        #region    Custom Methods
            private Vector2 GetAimPosition()
            {
                // Error //
                    if (IO_RTfm_Cursor == null || I_Cvs_Scene == null) return Vector2.zero;
                // Get Position //
                    Vector2 Vec2_Position = (Mouse.current != null) ? Mouse.current.position.ReadValue() : Vector2.zero;
                // Convert //
                    RectTransformUtility.ScreenPointToLocalPointInRectangle
                    (
                        I_Cvs_Scene.transform as RectTransform,
                        Vec2_Position,
                        I_Cvs_Scene.renderMode == (RenderMode.ScreenSpaceOverlay) ? null : I_Cvs_Scene.worldCamera,
                        out Vector2 Vec2_LocalPoint
                    );
                // Return //
                    return Vec2_LocalPoint;
            }
            public void SpawnBullet()
            {
                Bullet Bllt_Spawned = Instantiate(I_Bllt_Prefab, (new Vector3(this.transform.position.x, this.transform.position.y, 5)), Quaternion.identity);
                Bllt_Spawned.transform.SetParent(I_GObj_BulletContainer.transform, worldPositionStays: true);
                Bllt_Spawned.transform.localScale = new Vector3(1,1,1);
            }
        #endregion Custom Methods
    #endregion Methods
}