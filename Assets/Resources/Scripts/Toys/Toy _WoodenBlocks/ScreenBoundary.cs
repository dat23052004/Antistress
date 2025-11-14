using UnityEngine;

public class ScreenBoundary : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float thickness = 0.5f;

    private float currentAspect;

    private void Start()
    {
        if (cam == null)
            cam = Camera.main;

        CreateBoundaries();
    }

    private void Update()
    {
        if (Mathf.Abs(cam.aspect - currentAspect) > 0.01f)
        {
            currentAspect = cam.aspect;
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            CreateBoundaries();
        }
    }

    private void CreateBoundaries()
    {
        currentAspect = cam.aspect;
        float screenHeight = 2f * cam.orthographicSize;
        float screenWidth = screenHeight * cam.aspect;
        Vector3 camPos = cam.transform.position;

        CreateWall("Top", new Vector2(camPos.x, camPos.y + screenHeight / 2 + thickness / 2), new Vector2(screenWidth + thickness * 2, thickness));
        CreateWall("Bottom", new Vector2(camPos.x, camPos.y - screenHeight / 2 - thickness / 2), new Vector2(screenWidth + thickness * 2, thickness));
        CreateWall("Left", new Vector2(camPos.x - screenWidth / 2 - thickness / 2, camPos.y), new Vector2(thickness, screenHeight));
        CreateWall("Right", new Vector2(camPos.x + screenWidth / 2 + thickness / 2, camPos.y), new Vector2(thickness, screenHeight));
    }

    private void CreateWall(string name, Vector2 center, Vector2 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.parent = transform;
        wall.transform.position = center;
        var collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;
    }
}
