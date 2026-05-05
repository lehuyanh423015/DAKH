using UnityEngine;

/// <summary>
/// BackgroundMarkerGenerator – Simple placeholder world anchor system (Phase 18).
/// </summary>
public class BackgroundMarkerGenerator : MonoBehaviour
{
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private int markerCount = 9;
    [SerializeField] private float spacing = 1.5f;
    [SerializeField] private float yPosition = -3.0f;
    [SerializeField] private Vector2 markerScale = new Vector2(0.05f, 6.0f);
    [SerializeField] private Color markerColor = new Color(1f, 1f, 1f, 0.25f);
    [SerializeField] private string markerNamePrefix = "BG_Marker_";

    [Tooltip("Assign Unity's default square sprite, or any other simple placeholder sprite.")]
    [SerializeField] private Sprite markerSprite;

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateMarkers();
        }
    }

    private void GenerateMarkers()
    {
        if (markerSprite == null)
        {
            Debug.LogWarning("BackgroundMarkerGenerator: markerSprite is null. Cannot generate markers.");
            return;
        }

        int halfCount = markerCount / 2;

        for (int i = 0; i < markerCount; i++)
        {
            float xPos = (i - halfCount) * spacing;
            
            GameObject marker = new GameObject($"{markerNamePrefix}{i}");
            marker.transform.SetParent(transform);
            marker.transform.position = new Vector3(xPos, yPosition, 0f);
            marker.transform.localScale = new Vector3(markerScale.x, markerScale.y, 1f);

            SpriteRenderer sr = marker.AddComponent<SpriteRenderer>();
            sr.sprite = markerSprite;
            sr.color = markerColor;
            sr.sortingOrder = -100; // Place behind gameplay
        }
    }
}
