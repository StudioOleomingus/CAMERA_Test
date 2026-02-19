using UnityEngine;

/// <summary>
/// Attach this script to any Camera. Press Space to take a "photo" —
/// it renders what the camera sees onto a RenderTexture, copies it to a
/// Texture2D, then spawns a Plane in front of the camera with that
/// texture applied like a printed photograph.
/// </summary>
public class CameraPhotoPrinter : MonoBehaviour
{
    [Header("Photo Settings")]
    [Tooltip("Resolution width of the captured photo.")]
    public int photoWidth = 1920;

    [Tooltip("Resolution height of the captured photo.")]
    public int photoHeight = 1080;

    [Header("Spawn Settings")]
    [Tooltip("How far in front of the camera the photo spawns.")]
    public float spawnDistance = 2f;

    [Tooltip("How far below the camera's forward line the photo drops (simulates falling/printing).")]
    public float spawnDropOffset = 0.5f;

    [Tooltip("Scale of the spawned photo plane (width). Height is auto-calculated from aspect ratio.")]
    public float photoWorldWidth = 0.4f;

    [Header("Physics")]
    [Tooltip("If true, the spawned photo will have a Rigidbody and fall with gravity.")]
    public bool usePhysics = true;

    [Header("Optional")]
    [Tooltip("A material to clone for each photo. If left empty, a default Unlit material is created.")]
    public Material baseMaterial;

    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null)
        {
            Debug.LogError("CameraPhotoPrinter: No Camera component found on this GameObject!");
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
    /// Captures the camera view and spawns a textured plane.
    /// </summary>
    public void TakePhoto()
    {
        Texture2D photo = CaptureCamera();
        SpawnPhotoPlane(photo);
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
    /// Creates a Plane primitive, applies the photo texture, and positions it in front of the camera.
    /// </summary>
    private void SpawnPhotoPlane(Texture2D photo)
    {
        // Create the plane
        GameObject photoPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        photoPlane.name = "Photo_" + System.DateTime.Now.ToString("HHmmss_fff");

        // Calculate aspect-correct scale
        // Unity's default Plane is 10x10 units, so we divide desired size by 10
        float aspect = (float)photoWidth / photoHeight;
        float scaleX = photoWorldWidth / 10f;
        float scaleZ = (photoWorldWidth / aspect) / 10f;
        photoPlane.transform.localScale = new Vector3(scaleX, 1f, scaleZ);

        // Position it in front of the camera, slightly below center
        Vector3 spawnPos = _cam.transform.position
                         + _cam.transform.forward * spawnDistance
                         + _cam.transform.right * 0.1f
                         - _cam.transform.up * spawnDropOffset;
        photoPlane.transform.position = spawnPos;

        // Orient the plane so its face points back toward the camera
        // Unity's default Plane faces up (+Y), so we rotate it to face the camera
        photoPlane.transform.rotation = Quaternion.LookRotation(_cam.transform.up, -_cam.transform.forward);

        // Create and assign material
        Material mat;
        if (baseMaterial != null)
        {
            mat = new Material(baseMaterial);
        }
        else
        {
            // Use Unlit so the photo isn't affected by scene lighting
            Shader unlitShader = Shader.Find("Unlit/Texture");
            if (unlitShader == null)
            {
                // Fallback if Unlit/Texture isn't included in the build
                unlitShader = Shader.Find("Standard");
            }
            mat = new Material(unlitShader);
        }
        mat.mainTexture = photo;

        // Make the material double-sided by disabling culling (Standard shader)
        mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

        Renderer renderer = photoPlane.GetComponent<Renderer>();
        renderer.material = mat;

        // Optionally add physics so the photo "falls" like a real print
        if (usePhysics)
        {
            Rigidbody rb = photoPlane.AddComponent<Rigidbody>();
            rb.mass = 0.05f;
            rb.linearDamping = 1f;
            rb.angularDamping = 1f;
        }

        Debug.Log($"Photo taken! Spawned '{photoPlane.name}'");
    }
}
