using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] public Rect Boundries;
    [SerializeField] Camera cam;
    [SerializeField]
    [Range(-1f, 1f)] float XOffset;
    [SerializeField]
    [Range(-1f, 1f)] float YOffset;

    [Tooltip("How fast does camera follows the player \n 0 - don't, 1 - immediatlly")]
    [Range(0, 1)]
    [SerializeField] float CameraSpeed;

    [SerializeField]
    [Tooltip("This is a size of camera for screen with 1:1 ratio")]
    [Min(0)] float Size = 10;
    public Transform Target;
    public static event Action<CameraScript> CameraCreated;
    private void OnValidate()
    {
        Awake();
    }
    private void Awake()
    {
        cam.orthographicSize = Size / cam.aspect;
    }
    private void Start()
    {
        CameraCreated?.Invoke(this);
    }
    void Update()
    {
        FollowTarget();
    }
    public void FollowTarget()
    {
        var x = Mathf.Min(Mathf.Max(Target.position.x + XOffset * cam.orthographicSize * cam.aspect, Boundries.xMin + (cam.orthographicSize * cam.aspect)), Boundries.xMax - (cam.orthographicSize * cam.aspect));
        var y = Mathf.Min(Mathf.Max(Target.transform.position.y + YOffset * cam.orthographicSize, Boundries.yMin + (cam.orthographicSize)), Boundries.yMax - (cam.orthographicSize));
        transform.position = Vector3.Lerp(transform.position, new Vector3(x, y, transform.position.z), CameraSpeed);
    }
    #region Gizmo
    private Vector3 LeftTopCorner() => new Vector3(Boundries.xMin, Boundries.yMin);
    private Vector3 RightTopCorner() => new Vector3(Boundries.xMax, Boundries.yMin);
    private Vector3 LeftBottomCorner() => new Vector3(Boundries.xMin, Boundries.yMax);
    private Vector3 RightBottomCorner() => new Vector3(Boundries.xMax, Boundries.yMax);
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        //Gizmos.DrawLine(LeftTopCorner(), RightTopCorner());
        //Gizmos.DrawLine(RightTopCorner(), RightBottomCorner());
        //Gizmos.DrawLine(RightBottomCorner(), LeftBottomCorner());
        //Gizmos.DrawLine(LeftBottomCorner(), LeftTopCorner());
    }
    #endregion
}
