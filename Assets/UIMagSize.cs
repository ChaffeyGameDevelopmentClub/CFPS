using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMagSize : MonoBehaviour
{
    [SerializeField] PlayerShoot gun;
    Text magText;
    // Start is called before the first frame update
    void Start()
    {
        magText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        magText.text = gun.GetCurrentMag().ToString();
    }
}
