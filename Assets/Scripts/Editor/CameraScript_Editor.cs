using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraScript))]
public class CameraScript_Editor : Editor
{
    private void OnSceneGUI()
    {
        CameraScript cameraScript = (CameraScript)target;
        Handles.color = Color.green;
        //Handles.RectangleHandleCap(0, cameraScript.Boundries.center, Quaternion.identity, 1, EventType.Repaint);
        Handles.DrawWireCube(cameraScript.Boundries.center, cameraScript.Boundries.size);

        //Draw handles to control camera rect using like in Unity Colliders
        Undo.RecordObject(cameraScript, "Changed Camera Boundries");
        var Right = Handles.FreeMoveHandle(cameraScript.Boundries.center + new Vector2(cameraScript.Boundries.size.x / 2, 0), 0.1f, Vector3.one, Handles.DotHandleCap);
        var Left = Handles.FreeMoveHandle(cameraScript.Boundries.center + new Vector2(-cameraScript.Boundries.size.x / 2, 0), 0.1f, Vector3.one, Handles.DotHandleCap);
        var Top = Handles.FreeMoveHandle(cameraScript.Boundries.center + new Vector2(0, cameraScript.Boundries.size.y / 2), 0.1f, Vector3.one, Handles.DotHandleCap);
        var Bottom = Handles.FreeMoveHandle(cameraScript.Boundries.center + new Vector2(0, -cameraScript.Boundries.size.y / 2), 0.1f, Vector3.one, Handles.DotHandleCap);
        Rect newRect = new Rect(Left.x, Bottom.y, (Right - Left).x, (Top - Bottom).y);
        cameraScript.Boundries = newRect;
        //Make this action Revertable
        if (GUI.changed)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(cameraScript.gameObject.scene);
        }
        //Also by ctrl+z

    }
}
