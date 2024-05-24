using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : Character
{
    [SerializeField] List<Transform> WalkingPattern;
    [SerializeField] Animator animator;


    private void FixedUpdate()
    {
        if (animator.GetBool("isAttacking")) return;
        if (Vector2.Distance(transform.position, WalkingPattern[0].position) > transform.localScale.x)
        {
            Move(WalkingPattern[0].position - transform.position);

        }
        else
        {
            RandomizeList();
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<ILinkedListable>(out var listable))
        {
            StartCoroutine(Attack());

        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<ILinkedListable>(out var listable))
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        animator.SetBool("isAttacking", true);
        yield return new WaitForSeconds(1.5f);
        animator.SetBool("isAttacking", false);

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
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < WalkingPattern.Count - 1; i++)
        {
            for (int j = 0; j < WalkingPattern.Count; j++)
            {
                Gizmos.DrawLine(WalkingPattern[i].position, WalkingPattern[j].position);
            }
        }
    }
}
