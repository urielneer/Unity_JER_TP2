using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #region    Animation
        public void ImpactOver()
        {
            // Has Hit Player ? //
                if (true)
                {
                }
            // Self Destruct //
                Destroy(this.gameObject);
        }
    #endregion Animation
}
