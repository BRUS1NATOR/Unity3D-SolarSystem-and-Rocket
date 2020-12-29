using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class SunLight : MonoBehaviour
{
    public GameObject planet;

    // Update is called once per frame
    void Update()
    {
       Vector3 dir = this.transform.position - planet.transform.position;
        this.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }
}
