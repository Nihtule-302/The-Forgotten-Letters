using _Project.Scripts.Core.StateMachine;
using UnityEngine;

public class EnergyBlastState : State
{
    [SerializeField] private GameObject projectilePrefab;
    public Transform firePoint;

    public override void Enter()
    {
        Shoot();
    }
    public override void Do()
    {
        isComplete = true;
    }
    public override void Exit(){}

    public void Shoot()
    {
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }
}
