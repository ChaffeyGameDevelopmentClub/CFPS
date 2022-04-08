using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : EnemyBase
{
    public override void setTargetCoordinates(Vector3 targetCoordinates)
    {
        transform.position = targetCoordinates;
    }

    public override void onDeath()
    {
        Destroy(this.transform.gameObject);
    }
}
