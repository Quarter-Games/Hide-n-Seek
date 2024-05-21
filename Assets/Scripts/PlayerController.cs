using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    public string PlayerName;
    public bool isThisPlayer;
    [SerializeField] float speed = 2f;
    private void Update()
    {
        if (!isThisPlayer) return;

        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");
        transform.position += new Vector3(x, y, 0) * Time.deltaTime * speed;
    }
}
