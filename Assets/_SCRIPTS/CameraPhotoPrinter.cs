using UnityEngine;

/// <summary>
/// Attach this script to any Camera. Press Space to take a "photo" —
/// it renders what the camera sees onto a RenderTexture, copies it to a
/// Texture2D, then applies that texture to an existing billboard
/// GameObject in the scene.
/// </summary>
public class CameraPhotoPrinter : MonoBehaviour
{
    [Header("Photo Settings")]
    [Tooltip("Resolution width of the captured photo (4:3 aspect ratio).")]
    public int photoWidth = 1024;

    [Tooltip("Resolution height of the captured photo (4:3 aspect ratio).")]
    public int photoHeight = 768;

    [Header("Billboard")]
    [Tooltip("The existing billboard GameObject in the scene to display the photo on.")]
    public GameObject billboard;

    [Header("Optional")]
    [Tooltip("A material to clone for the billboard. If left empty, the billboard's existing material is used and its texture is replaced.")]
    public Material baseMaterial;

    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null)
        {
            Debug.LogError("CameraPhotoPrinter: No Camera component found on this GameObject!");
            enabled = false;
            return;
        }

        if (billboard == null)
        {
            Debug.LogError("CameraPhotoPrinter: No billboard GameObject assigned!");
            enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakePhoto();
        }
    }

    /// <summary>
    /// Captures the camera view and applies it to the billboard.
    /// </summary>
    public void TakePhoto()
    {
        Texture2D photo = CaptureCamera();
        ApplyPhotoToBillboard(photo);
    }

    /// <summary>
    /// Renders the camera to a temporary RenderTexture and reads it back into a Texture2D.
    /// </summary>
    private Texture2D CaptureCamera()
    {
        // Create a temporary RenderTexture
        RenderTexture rt = new RenderTexture(photoWidth, photoHeight, 24);

        // Remember the camera's original target
        RenderTexture previousTarget = _cam.targetTexture;

        // Render the camera into our RT
        _cam.targetTexture = rt;
        _cam.Render();
        _cam.targetTexture = previousTarget;

        // Read pixels from the RT into a Texture2D
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D photo = new Texture2D(photoWidth, photoHeight, TextureFormat.RGB24, false);
        photo.ReadPixels(new Rect(0, 0, photoWidth, photoHeight), 0, 0);
        photo.Apply();

        // Clean up
        RenderTexture.active = previousActive;
        rt.Release();
        Destroy(rt);

        return photo;
    }

    /// <summary>
    /// Applies the captured photo texture to the billboard GameObject's material.
    /// </summary>
    private void ApplyPhotoToBillboard(Texture2D photo)
    {
        Renderer renderer = billboard.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("CameraPhotoPrinter: Billboard has no Renderer component!");
            return;
        }

        if (baseMaterial != null)
        {
            // Clone the provided base material and assign the photo
            Material mat = new Material(baseMaterial);
            mat.mainTexture = photo;
            renderer.material = mat;
        }
        else
        {
            // Replace the texture on the billboard's existing material
            renderer.material.mainTexture = photo;
        }

        Debug.Log("Photo taken and applied to billboard!");
    }
}