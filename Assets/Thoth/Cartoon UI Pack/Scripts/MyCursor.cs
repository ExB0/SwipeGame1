using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCursor : MonoBehaviour {

    public Texture2D myCursor;

    public Vector2 hotSpot = Vector2.zero;

    void Start ()
    {

        Cursor.SetCursor(myCursor, hotSpot, CursorMode.Auto);
        Cursor.visible = true;

    }
	
	void Update () {
		
	}
}
