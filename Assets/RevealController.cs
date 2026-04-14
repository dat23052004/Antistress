using UnityEngine;

public class RevealController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer BGRenderer;
    [SerializeField] private SpriteRenderer overlayRenderer;
    [SerializeField] private Material revealMaterial;

    [Header("Reveal Settings")]
    [SerializeField] private int maskResolution = 1024;
    [SerializeField] private float brushSize = 50f;
    [SerializeField] private Color overlayColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private bool useSoftBrush = true;

    private Texture2D maskTexture;
    private Color32[] clearColors;
    private Color32[] maskPixels;
    private Camera mainCamera;
    private Material instanceMaterial;

    private int maskResolutionSquared;

    private void Start()
    {
        mainCamera = Camera.main;
        maskResolutionSquared = maskResolution * maskResolution;

        InitializeMaskTexture();
        SetupMaterial();
    }

    private void Update()
    {
        HandleInput();

        if (InputManager.ResetPressedThisFrame())
            ResetReveal();
    }

    private void InitializeMaskTexture()
    {
        maskTexture = new Texture2D(maskResolution, maskResolution, TextureFormat.R8, false);
        maskTexture.filterMode = FilterMode.Bilinear;
        maskTexture.wrapMode = TextureWrapMode.Clamp;

        clearColors = new Color32[maskResolutionSquared];
        maskPixels = new Color32[maskResolutionSquared];

        for (int i = 0; i < maskResolutionSquared; i++)
        {
            clearColors[i] = new Color32(0, 0, 0, 255);
            maskPixels[i] = new Color32(0, 0, 0, 255);
        }

        maskTexture.SetPixels32(clearColors);
        maskTexture.Apply(false);
    }

    private void SetupMaterial()
    {
        instanceMaterial = new Material(revealMaterial);
        instanceMaterial.SetTexture("_MaskTex", maskTexture);
        instanceMaterial.SetColor("_Color", overlayColor);
        overlayRenderer.material = instanceMaterial;
    }

    private void HandleInput()
    {
        if (!InputManager.TryGetPrimaryPointerHeld(out Vector2 inputPosition))
            return;

        Vector2 worldPos = mainCamera.ScreenToWorldPoint(inputPosition);
        RevealAtPosition(worldPos);
    }

    private void RevealAtPosition(Vector2 worldPosition)
    {
        Vector2 localPos = overlayRenderer.transform.InverseTransformPoint(worldPosition);

        Bounds spriteBounds = overlayRenderer.sprite.bounds;
        float spriteWidth = spriteBounds.size.x;
        float spriteHeight = spriteBounds.size.y;

        float uvX = (localPos.x + spriteBounds.extents.x) / spriteWidth;
        float uvY = (localPos.y + spriteBounds.extents.y) / spriteHeight;

        if (uvX < 0f || uvX > 1f || uvY < 0f || uvY > 1f)
            return;

        int pixelX = Mathf.Clamp(Mathf.RoundToInt(uvX * maskResolution), 0, maskResolution - 1);
        int pixelY = Mathf.Clamp(Mathf.RoundToInt(uvY * maskResolution), 0, maskResolution - 1);

        if (useSoftBrush)
            DrawSoftCircle(pixelX, pixelY, brushSize);
        else
            DrawHardCircle(pixelX, pixelY, brushSize);

        maskTexture.SetPixels32(maskPixels);
        maskTexture.Apply(false);
    }

    private void DrawSoftCircle(int centerX, int centerY, float radius)
    {
        int radiusInt = Mathf.CeilToInt(radius);
        int startX = Mathf.Max(0, centerX - radiusInt);
        int endX = Mathf.Min(maskResolution - 1, centerX + radiusInt);
        int startY = Mathf.Max(0, centerY - radiusInt);
        int endY = Mathf.Min(maskResolution - 1, centerY + radiusInt);

        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                float dx = x - centerX;
                float dy = y - centerY;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                if (distance <= radius)
                {
                    float falloff = 1f - Mathf.Clamp01((distance - radius + 5f) / 5f);
                    byte alpha = (byte)(falloff * 255f);
                    int index = y * maskResolution + x;

                    if (alpha > maskPixels[index].r)
                        maskPixels[index] = new Color32(alpha, alpha, alpha, 255);
                }
            }
        }
    }

    private void DrawHardCircle(int centerX, int centerY, float radius)
    {
        int radiusInt = Mathf.CeilToInt(radius);
        int radiusSquared = radiusInt * radiusInt;

        int startX = Mathf.Max(0, centerX - radiusInt);
        int endX = Mathf.Min(maskResolution - 1, centerX + radiusInt);
        int startY = Mathf.Max(0, centerY - radiusInt);
        int endY = Mathf.Min(maskResolution - 1, centerY + radiusInt);

        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                int dx = x - centerX;
                int dy = y - centerY;

                if (dx * dx + dy * dy <= radiusSquared)
                {
                    int index = y * maskResolution + x;
                    maskPixels[index] = new Color32(255, 255, 255, 255);
                }
            }
        }
    }

    public void ResetReveal()
    {
        System.Array.Copy(clearColors, maskPixels, maskResolutionSquared);
        maskTexture.SetPixels32(maskPixels);
        maskTexture.Apply(false);
    }

    private void OnDestroy()
    {
        if (maskTexture != null)
            Destroy(maskTexture);

        if (instanceMaterial != null)
            Destroy(instanceMaterial);
    }

}
