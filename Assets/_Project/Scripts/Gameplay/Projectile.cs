using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    public class Projectile : MonoBehaviour
    {
        public float speed = 20f;
        public Rigidbody2D rb;

        private void Start()
        {
            rb.linearVelocity = transform.right * speed;
        }

        private void OnTriggerEnter2D(Collider2D hitInfo)
        {
            Debug.Log(hitInfo.name);
            //Destroy(gameObject);
        }
    }
}