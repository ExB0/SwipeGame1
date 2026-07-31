using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapInteractionController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask _interactableLayers;

    private void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HandleClick(Input.GetTouch(0).position);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick(Input.mousePosition);
        }
    }

    private void HandleClick(Vector3 screenPosition)
    {
        if (cam == null)
        {
            Debug.LogError("Camera is missing");
            return;
        }

        Ray ray = cam.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _interactableLayers))
        {
            var clickable = hit.collider.GetComponent<IClickable>();
            clickable?.OnClick();
        }
    }
}
