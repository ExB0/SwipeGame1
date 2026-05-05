using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapInteractionController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask _interactableLayers;

        void Update()
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Vector3 touchPos = Input.GetTouch(0).position;
                Ray ray = cam.ScreenPointToRay(touchPos);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    var clickable = hit.collider.GetComponent<IClickable>();
                    if (clickable != null)
                    {
                        clickable.OnClick();
                    }
                }
            }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("ПОПАЛ В: " + hit.collider.name);
                var clickable = hit.collider.GetComponent<IClickable>();
                if (clickable != null)
                {
                    Debug.Log("silno");
                    clickable.OnClick();
                }
            }
        }
    }

}
