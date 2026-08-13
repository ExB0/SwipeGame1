using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using InterFaces;

namespace Controller
{
    public class TapInteractionController : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
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
            if (_camera == null)
            {
                Debug.LogError("Camera is missing");
                return;
            }

            Ray ray = _camera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _interactableLayers))
            {
                var clickable = hit.collider.GetComponent<IClickable>();
                clickable?.OnClick();
            }
        }
    }
}
