using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform target;
    [SerializeField] private float projectileMoveSpeed = 1;

    public Transform firePoint;

    private float shootTimer;
    [SerializeField] private float shootRate;

    void Update()
    {
        // shootTimer -= Time.deltaTime;

        // if(shootTimer <= 0)
        // {
        //     shootTimer = shootRate;
        //     Shoot();
        // }
    }
    
    public void Shoot()
    {
        Projectile projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation).GetComponent<Projectile>();
        //projectile.InitializeProjectile(target, projectileMoveSpeed);
    }
}
