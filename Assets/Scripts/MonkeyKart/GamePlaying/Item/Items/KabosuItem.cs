using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class KabosuItem : MonoBehaviour
{
    Vector3 velocity;
    float rotationSpeed = 200f;

    public void Init(Vector3 velocity)
    {
        this.velocity = velocity;
    }

    void Update()
    {
        transform.position += velocity * Time.deltaTime;
        transform.Rotate(rotationSpeed * Time.deltaTime, 0, 0);
    }
}
