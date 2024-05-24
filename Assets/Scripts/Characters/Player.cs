using System;
using UnityEngine;

public class Player : Character, ILinkedListable
{

    [SerializeField] bl_Joystick Joystick = null;
    public Action<int> CountChanged { get; set; }
    public ILinkedListable Next { get; set; }
    [SerializeField] Character _next;
    public ILinkedListable Previous { get; set; }
    public void Awake()
    {
        if (_next is ILinkedListable linked)
        {
            Next = linked;
        }
    }


    void FixedUpdate()
    {
        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");
        if (Joystick != null)
        {
            x = Joystick.Horizontal;
            y = Joystick.Vertical;
        }
        Move(new Vector2(x, y));
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Wolf>(out var wolf))
        {
            if (Next != null) (Next as Sheep).OnTriggerEnter2D(collision);
            (this as ILinkedListable).RemoveAllAfter();

        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<Wolf>(out var wolf))
        {
            if (Next != null) (Next as Sheep).OnCollisionEnter2D(collision);
            (this as ILinkedListable).RemoveAllAfter();

        }
    }
    public void Dispose()
    {
        Destroy(gameObject);
    }
}
