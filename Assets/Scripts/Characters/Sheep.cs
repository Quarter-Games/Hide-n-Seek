using Unity.VisualScripting;
using UnityEngine;

public class Sheep : Character, ILinkedListable
{
    const float thrashold = 0.5f;
    float Timer = 1;
    [SerializeField] float TimeToChangeDirection = 1f;
    private Vector2 direction;
    [SerializeField] SpriteRenderer spriteRenderer;
    bool isStopped = false;

    int selfID = 0;

    public System.Action<int> CountChanged { get; set; }
    public ILinkedListable Next { get; set; }
    [SerializeField] Character _next;
    public ILinkedListable Previous { get; set; }
    [SerializeField] Character _previous;

    private Coroutine coroutine;
    private void Awake()
    {
        if (_next is ILinkedListable linked)
        {
            Next = linked;
        }
        if (_previous is ILinkedListable linked2)
        {
            Previous = linked2;
        }
    }

    private void FixedUpdate()
    {
        if (IPausable.IsPaused) return;
        if (Previous != null)
        {
            speed = (Previous as Character).speed;
            var Root = (this as ILinkedListable).GetFirst() as Character;
            if (isStopped && coroutine == null)
            {
                if (Vector2.Distance(transform.position, Root.transform.position) <= thrashold + 0.1f) return;
                int count = (Root as ILinkedListable).Count();
                Quaternion rotation = Quaternion.Euler(0, 0, 360 / count * selfID);
                var dir = (Root.transform.position + rotation * (Vector2.one * thrashold));
                coroutine = StartCoroutine(MoveTo(dir));
            }
            var distance = Vector2.Distance((Previous as Character).transform.position, transform.position);
            if (distance > thrashold / 2)
            {
                if (isStopped && distance < thrashold * 2) return;
                Move((Previous as Character).transform.position - transform.position);
                isStopped = false;
                spriteRenderer.color = Color.white;
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                    coroutine = null;
                }
            }
            else if ((Previous is Sheep sheep && sheep.isStopped) || Previous is Player)
            {
                isStopped = true;
                spriteRenderer.color = Color.green;
                direction = transform.position;
                selfID = (this as ILinkedListable).Count();
            }
        }
        else
        {
            Timer += Time.fixedDeltaTime;
            if (Timer >= TimeToChangeDirection)
            {
                speed = InitialSpeed;
                direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                Timer = 0;
            }
            Move(direction);
        }
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<Player>(out var player))
        {
            if (Previous != null) return;
            (this as ILinkedListable).AddAfter((player as ILinkedListable).GetLast());
        }
        else if (collision.collider.TryGetComponent<Wolf>(out var wolf))
        {
            if (isStopped) return;
            if (Next != null) (Next as Sheep).OnCollisionEnter2D(collision);
            (this as ILinkedListable).RemoveAllAfter();
            speed = InitialSpeed * 2;
            Timer = -1f;
            direction = transform.position - wolf.transform.position;
        }
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (isStopped) return;
        if (collision.TryGetComponent<Wolf>(out var wolf))
        {
            if (Next != null) (Next as Sheep).OnTriggerEnter2D(collision);
            (this as ILinkedListable).RemoveAllAfter();
            speed = InitialSpeed * 2;
            Timer = -1f;
            direction = transform.position - wolf.transform.position;

        }
    }
}
