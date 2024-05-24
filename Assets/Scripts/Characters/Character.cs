using System.Collections;
using UnityEngine;

abstract public class Character : MonoBehaviour, IPausable
{
    [SerializeField] public float speed = 5f;
    [SerializeField] protected float InitialSpeed = 5f;
    private void Awake()
    {
        speed = InitialSpeed;
    }
    public void Move(Vector2 direction)
    {
        if (IPausable.IsPaused) return;
        direction = direction.normalized;
        transform.position += new Vector3(direction.x, direction.y, 0) * speed*Time.fixedDeltaTime;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
    }
    public IEnumerator MoveTo(Vector2 target)
    {
        while (Vector2.Distance(transform.position, target) > 0.1f)
        {
            Move(target - (Vector2)transform.position);
            yield return new WaitForFixedUpdate();
        }
    }
}
