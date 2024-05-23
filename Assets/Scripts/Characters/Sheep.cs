using UnityEngine;

public class Sheep : Character
{
    const float thrashold = 0.25f;
    public Character Forward = null;
    public Sheep Backward = null;
    float Timer = 1;
    [SerializeField] float TimeToChangeDirection = 1f;
    private Vector2 direction;
    bool isStopped = false;

    public Player Root = null;
    int selfID = 0;

    private void FixedUpdate()
    {
        if (Forward != null)
        {
            speed = Forward.speed;
            if (isStopped && false)
            {
                if (Vector2.Distance(transform.position, Root.transform.position) <= thrashold + 0.1f) return;
                int count = Root.SheepCount;
                Quaternion rotation = Quaternion.Euler(0, 0, 360 / count * selfID);
                var dir = (Root.transform.position - Root.transform.position).normalized * thrashold;

                Move(transform.position - (dir + (rotation * dir)));
            }
            var distance = Vector2.Distance(Forward.transform.position, transform.position);
            if (distance > thrashold)
            {
                if (isStopped && distance < thrashold * 4) return;
                Move(Forward.transform.position - transform.position);
                isStopped = false;
            }
            else
            {
                isStopped = true;
                Root = GetForwardRecursive() as Player;
                if (Root != null)
                {
                    Root.GetSheepCount();
                    selfID = GetSheepCountRecursive(0);
                }
            }
        }
        else
        {
            Timer += Time.fixedDeltaTime;
            speed = InitialSpeed;
            if (Timer >= TimeToChangeDirection)
            {
                direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                Timer = 0;
            }
            Move(direction);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<Player>(out var player))
        {
            if (Forward != null) return;
            Forward = player.GetLastInList();
            speed = Forward.speed;
            if (player.RootSheep == null)
            {
                player.RootSheep = this;
            }
            if (Forward is Sheep sheep)
            {
                sheep.Backward = this;
            }
        }
        else if (collision.collider.TryGetComponent<Wolf>(out var wolf))
        {
            Unlink();
            speed = InitialSpeed*2;
            Timer = -1f;
            Move(wolf.transform.position - transform.position);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Wolf>(out var wolf))
        {
            Unlink();
            speed = InitialSpeed * 2;
            Timer = -1f;
            Move(wolf.transform.position - transform.position);
        }
    }
    public override Character GetLastInList()
    {
        if (Backward == null)
        {
            return this;
        }
        return Backward.GetLastInList();
    }
    public override void Unlink()
    {
        speed = InitialSpeed;
        if (Forward is Player player)
        {
            player.RootSheep = null;
        }
        else if (Forward is Sheep sheep)
        {
            sheep.Backward = null;
        }

        Forward = null;
        if (Backward != null)
        {
            Backward.Unlink();
            Backward = null;
        }
    }

    public Character GetForwardRecursive()
    {
        if (Forward is Player player)
        {
            return player;
        }
        else if (Forward is Sheep sheep)
        {
            return sheep.GetForwardRecursive();
        }
        else
        {
            return null;
        }
    }

    internal int GetSheepCountRecursive(int count)
    {
        count++;
        if (Backward != null)
        {
            count = Backward.GetSheepCountRecursive(count);
        }
        return count;
    }
}
