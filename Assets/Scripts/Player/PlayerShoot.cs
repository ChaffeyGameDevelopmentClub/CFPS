using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    enum FiringMode {Single, Burst, FullAuto};

    [SerializeField] private FiringMode fireMode = FiringMode.FullAuto;
    [SerializeField] private float shotDelay = .5f;
    [SerializeField] private float burstDelay = .1f;
    [SerializeField] private int magSize = 30;
    [SerializeField] private int damage = 30;

    private float shotTimer = 0;

    [SerializeField] Camera playerCam;


    // Update is called once per frame
    void Update()
    {
        shotTimer -= Time.deltaTime;
        switch (fireMode)
        {
            case FiringMode.FullAuto:
                if (shotTimer <= 0 && Input.GetButton("Fire1"))
                {
                    Fire();
                    shotTimer = shotDelay;
                }
                break;
            case FiringMode.Single:
                if (shotTimer <= 0 && Input.GetButtonDown("Fire1"))
                {
                    Fire();
                    shotTimer = shotDelay;
                }
                break;
            case FiringMode.Burst:
                if (shotTimer <= 0 & Input.GetButtonDown("Fire1"))
                {
                    shotTimer = burstDelay * 3 + shotDelay;
                    StartCoroutine(Burst());
                }
                break;
        }
    }
    private void Fire()
    {
        RaycastHit hit;
        Debug.DrawRay(playerCam.transform.position, playerCam.transform.TransformDirection(Vector3.forward), Color.red, 1f, false);
        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.TransformDirection(Vector3.forward), out hit))
        {
            EnemyHealth enemy = hit.transform.GetComponent<EnemyHealth>();
            if(enemy)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
    IEnumerator Burst()
    {
        for(int i = 0; i < 3; i++)
        {
            Fire();
            yield return new WaitForSeconds(burstDelay);
        }
    }
}