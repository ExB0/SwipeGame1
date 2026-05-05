using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundScaler : MonoBehaviour
{
    [SerializeField] SpriteRenderer _spriteRenderer;
    private void Start()
    {
        FitToCamera();
    }

    private void FitToCamera()
    {
        Camera cam = Camera.main;

        float distance = Mathf.Abs(transform.position.z - cam.transform.position.z);

        float height = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * cam.aspect;

        float scaleX = width / _spriteRenderer.bounds.size.x;
        float scaleY = height / _spriteRenderer.bounds.size.y;

        float scale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
