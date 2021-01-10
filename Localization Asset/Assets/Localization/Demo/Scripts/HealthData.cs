using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthData : MonoBehaviour
{
    public float currentHealth = default;

    void Start()
    {
        currentHealth = 100;

        StartCoroutine(ReduceHealth());
    }

    IEnumerator ReduceHealth()
    {
        while(enabled)
        {
            currentHealth -= 1;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
