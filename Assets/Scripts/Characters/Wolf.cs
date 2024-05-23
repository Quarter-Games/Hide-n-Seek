using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : Character
{
    [SerializeField] List<Transform> WalkingPattern;


    private void FixedUpdate()
    {
        if (Vector2.Distance(transform.position, WalkingPattern[0].position) > transform.localScale.x)
        {
            Move(WalkingPattern[0].position - transform.position);
            
        }
        else
        {
            RandomizeList();
        }
    }


    private void RandomizeList()
    {
        for (int i = 0; i < WalkingPattern.Count; i++)
        {
            var temp = WalkingPattern[i];
            var randomIndex = Random.Range(i, WalkingPattern.Count);
            WalkingPattern[i] = WalkingPattern[randomIndex];
            WalkingPattern[randomIndex] = temp;
        }
    }

}
