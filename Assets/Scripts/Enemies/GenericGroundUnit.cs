using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericGroundUnit : EnemyBase
{
    [SerializeField] private float speed = 5;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float jumpMultiplier = 500;
    CapsuleCollider cc;
    Rigidbody rb;

    UnitState unitState = UnitState.Grounded;

    enum UnitState
    {
        Jumping,
        Airborne,
        Grounded
    }

    public override void onDeath()
    {
        Destroy(this.transform.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
