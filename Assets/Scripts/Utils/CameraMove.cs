using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Interactable;

[RequireComponent(typeof(Camera))]
public class CameraMove : MonoBehaviour
{
    public float speed = 0.1f;

    private Vector3 myStartPos;
    private Vector3 startDragMousePos;
    private Vector3 lastPos;
    private DragType dragType;

    private Camera cam;

    void Awake() {
        cam = GetComponent<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        dragType = DragType.NONE;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(2)) {
            dragType = DragType.MIDDLE;
            startDragMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            myStartPos = transform.position;
        }
        else if (Input.GetMouseButtonUp(2)) {
            dragType = DragType.NONE;
        }

        if (dragType == DragType.MIDDLE) {
            Vector3 mousePos = Input.mousePosition;
            if (lastPos != mousePos) {
                lastPos = mousePos;
                mousePos = Camera.main.ScreenToWorldPoint(mousePos);
                gameObject.transform.position = myStartPos - (speed*(mousePos - startDragMousePos));
            }
        }
    }
}
