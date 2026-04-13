using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class RevealController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer BGRenderer;
    [SerializeField] private SpriteRenderer overlayRenderer;
    [SerializeField] private Material revealMaterial;


    [Header("Reveal Settings")]
    [SerializeField] private int maskResolution = 1024;
    [SerializeField] private float brushSize = 50f;
    [SerializeField] Color overlayColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private bool useSoftBrush = true;

    private Texture2D maskTexture;
    private Color32[] clearColors;
    private Color32[] maskPixels;
    private Camera mainCamera;
    private Material instanceMaterial;

    private int maskResolutionSquared;
    private float pixelSize;


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
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetReveal();
        }
    }


    private void InitializeMaskTexture()
    {
        // Unity 6: Dùng Color32 cho performance tốt hơn
        maskTexture = new Texture2D(
            maskResolution,
            maskResolution,
            TextureFormat.R8,
            false
        );

        maskTexture.filterMode = FilterMode.Bilinear;
        maskTexture.wrapMode = TextureWrapMode.Clamp;

        // Khởi tạo màu đen
        clearColors = new Color32[maskResolutionSquared];
        maskPixels = new Color32[maskResolutionSquared];

        for (int i = 0; i < maskResolutionSquared; i++)
        {
            clearColors[i] = new Color32(0, 0, 0, 255);
            maskPixels[i] = new Color32(0, 0, 0, 255);
        }

        maskTexture.SetPixels32(clearColors);
        maskTexture.Apply(false); // false = không tạo mipmaps
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
        bool shouldReveal = false;
        Vector2 inputPosition = Vector2.zero;

        // Mouse input (Editor/Desktop)
        if (Input.GetMouseButton(0))
        {
            shouldReveal = true;
            inputPosition = Input.mousePosition;
        }
        // Touch input (Mobile)
        else if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                shouldReveal = true;
                inputPosition = touch.position;
            }
        }

        if (shouldReveal)
        {
            Vector2 worldPos = mainCamera.ScreenToWorldPoint(inputPosition);
            RevealAtPosition(worldPos);
        }
    }

    private void RevealAtPosition(Vector2 worldPosition)
    {
        // Chuyển world position sang local
        Vector2 localPos = overlayRenderer.transform.InverseTransformPoint(worldPosition);

        // Sprite bounds
        Bounds spriteBounds = overlayRenderer.sprite.bounds;
        float spriteWidth = spriteBounds.size.x;
        float spriteHeight = spriteBounds.size.y;

        // Tính UV (0-1)
        float uvX = (localPos.x + spriteBounds.extents.x) / spriteWidth;
        float uvY = (localPos.y + spriteBounds.extents.y) / spriteHeight;

        // Kiểm tra bounds
        if (uvX < 0 || uvX > 1 || uvY < 0 || uvY > 1)
            return;

        // Pixel coordinates
        int pixelX = Mathf.Clamp(Mathf.RoundToInt(uvX * maskResolution), 0, maskResolution - 1);
        int pixelY = Mathf.Clamp(Mathf.RoundToInt(uvY * maskResolution), 0, maskResolution - 1);

        // Vẽ
        if (useSoftBrush)
        {
            DrawSoftCircle(pixelX, pixelY, brushSize);
        }
        else
        {
            DrawHardCircle(pixelX, pixelY, brushSize);
        }

        // Apply - Unity 6 tối ưu hơn với SetPixels32
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
                    // Soft edge
                    float falloff = 1f - Mathf.Clamp01((distance - radius + 5f) / 5f);
                    byte alpha = (byte)(falloff * 255f);

                    int index = y * maskResolution + x;

                    // Chỉ vẽ nếu giá trị mới lớn hơn
                    if (alpha > maskPixels[index].r)
                    {
                        maskPixels[index] = new Color32(alpha, alpha, alpha, 255);
                    }
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
        {
            Destroy(maskTexture);
        }

        if (instanceMaterial != null)
        {
            Destroy(instanceMaterial);
        }
    }
}
