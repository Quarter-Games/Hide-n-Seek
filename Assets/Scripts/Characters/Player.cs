using UnityEngine;

public class Player : Character
{

    [SerializeField] bl_Joystick Joystick = null;
    public Sheep RootSheep = null;
    public int SheepCount = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
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
    public override Character GetLastInList()
    {
        if (RootSheep == null)
        {
            return this;
        }
        return RootSheep.GetLastInList();
    }
    public int GetSheepCount()
    {
        if (RootSheep == null)
        {
            SheepCount = 0;
            return 0;
        }
        SheepCount = RootSheep.GetSheepCountRecursive(0);
        return SheepCount;
    }
}
