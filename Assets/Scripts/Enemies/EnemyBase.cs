using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamageable, IAlliable
{
    public static int enemyCount = 0;
    private IAlliable.Alliance alliance = IAlliable.Alliance.Neutral;
    [SerializeField] private float maxHealth = 100;
    private float currentHealth;
    private Vector3 targetCoordinates = Vector3.zero;

    public void Awake()
    {
        enemyCount++;
    }

    public virtual void setTargetCoordinates(Vector3 targetCoordinates) {
        this.targetCoordinates = targetCoordinates;
    }
    public abstract void onDeath();

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            onDeath(); 
        }
    }

    public void setAlliance(IAlliable.Alliance alliance)
    {
        this.alliance = alliance;
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private enum CPUState
    {
        Evading, //Evade attacker
        Attacking, //Attack non-allied target
        Relocating //Find path to a position
    };
}
