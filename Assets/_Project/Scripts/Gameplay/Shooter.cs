using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    public class Shooter : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform target;
        [SerializeField] private float projectileMoveSpeed = 1;

        public Transform firePoint;
        [SerializeField] private float shootRate;

        private float _shootTimer;

        // private void Update()
        // {
        //     // shootTimer -= Time.deltaTime;
        //
        //     // if(shootTimer <= 0)
        //     // {
        //     //     shootTimer = shootRate;
        //     //     Shoot();
        //     // }
        // }

        public void Shoot()
        {
            var projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation)
                .GetComponent<Projectile>();
            //projectile.InitializeProjectile(target, projectileMoveSpeed);
        }
    }
}