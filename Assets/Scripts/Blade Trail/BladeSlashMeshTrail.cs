using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class BladeSlashMeshTrail : MonoBehaviour
{
    [Header("Blade Points")]
    [SerializeField] private Transform bladeBottom;
    [SerializeField] private Transform bladeTip;

    [Header("Trail Shape")]
    [SerializeField] private float trailLifetime = 0.18f;
    [SerializeField] private float minDistanceBetweenSegments = 0.03f;
    [SerializeField] private float widthMultiplier = 1.0f;

    [Header("Visual")]
    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color endColor = new Color(1f, 1f, 1f, 0f);

    private Mesh mesh;
    private Material materialInstance;

    private readonly List<SlashSegment> segments = new List<SlashSegment>();

    private Vector3 lastTipPosition;
    private bool hasLastPosition;
    private bool emitting;

    private struct SlashSegment
    {
        public Vector3 bottom;
        public Vector3 tip;
        public float time;
    }

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "Blade Slash Mesh Trail";

        GetComponent<MeshFilter>().mesh = mesh;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null && meshRenderer.sharedMaterial != null)
        {
            materialInstance = new Material(meshRenderer.sharedMaterial);
            meshRenderer.material = materialInstance;
            materialInstance.color = startColor;
        }
    }

    private void LateUpdate()
    {
        if (emitting)
        {
            TryAddSegment();
        }

        RemoveOldSegments();
        RebuildMesh();
    }

    public void StartTrail()
    {
        emitting = true;
        hasLastPosition = false;
        segments.Clear();

        if (mesh != null)
            mesh.Clear();

        UpdateMaterialColor();
    }

    public void StopTrail()
    {
        emitting = false;
    }

    public void ClearTrail()
    {
        segments.Clear();
        hasLastPosition = false;

        if (mesh != null)
            mesh.Clear();
    }

    private void TryAddSegment()
    {
        if (bladeBottom == null || bladeTip == null)
            return;

        Vector3 currentTip = bladeTip.position;

        if (!hasLastPosition)
        {
            AddSegment();
            lastTipPosition = currentTip;
            hasLastPosition = true;
            return;
        }

        float distance = Vector3.Distance(currentTip, lastTipPosition);

        if (distance >= minDistanceBetweenSegments)
        {
            AddSegment();
            lastTipPosition = currentTip;
        }
    }

    private void AddSegment()
    {
        Vector3 bottom = bladeBottom.position;
        Vector3 tip = bladeTip.position;

        Vector3 center = (bottom + tip) * 0.5f;

        bottom = Vector3.Lerp(center, bottom, widthMultiplier);
        tip = Vector3.Lerp(center, tip, widthMultiplier);

        segments.Add(new SlashSegment
        {
            bottom = bottom,
            tip = tip,
            time = Time.time
        });
    }

    private void RemoveOldSegments()
    {
        float oldestAllowedTime = Time.time - trailLifetime;

        for (int i = segments.Count - 1; i >= 0; i--)
        {
            if (segments[i].time < oldestAllowedTime)
            {
                segments.RemoveAt(i);
            }
        }
    }

    private void RebuildMesh()
    {
        if (mesh == null)
            return;

        UpdateMaterialColor();

        mesh.Clear();

        if (segments.Count < 2)
            return;

        int vertexCount = segments.Count * 2;

        Vector3[] vertices = new Vector3[vertexCount];
        Color[] colors = new Color[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];

        // 12 indices per quad because we render both sides.
        int triangleCount = (segments.Count - 1) * 12;
        int[] triangles = new int[triangleCount];

        for (int i = 0; i < segments.Count; i++)
        {
            SlashSegment segment = segments[i];

            float age = Time.time - segment.time;
            float t = Mathf.Clamp01(age / trailLifetime);

            Color color = Color.Lerp(startColor, endColor, t);

            int bottomIndex = i * 2;
            int tipIndex = bottomIndex + 1;

            vertices[bottomIndex] = transform.InverseTransformPoint(segment.bottom);
            vertices[tipIndex] = transform.InverseTransformPoint(segment.tip);

            colors[bottomIndex] = color;
            colors[tipIndex] = color;

            float uvY = i / (float)(segments.Count - 1);

            uvs[bottomIndex] = new Vector2(0f, uvY);
            uvs[tipIndex] = new Vector2(1f, uvY);
        }

        int triangleIndex = 0;

        for (int i = 0; i < segments.Count - 1; i++)
        {
            int currentBottom = i * 2;
            int currentTip = currentBottom + 1;

            int nextBottom = currentBottom + 2;
            int nextTip = currentBottom + 3;

            // Front side.
            triangles[triangleIndex++] = currentBottom;
            triangles[triangleIndex++] = currentTip;
            triangles[triangleIndex++] = nextTip;

            triangles[triangleIndex++] = currentBottom;
            triangles[triangleIndex++] = nextTip;
            triangles[triangleIndex++] = nextBottom;

            // Back side, reversed winding order.
            triangles[triangleIndex++] = currentBottom;
            triangles[triangleIndex++] = nextTip;
            triangles[triangleIndex++] = currentTip;

            triangles[triangleIndex++] = currentBottom;
            triangles[triangleIndex++] = nextBottom;
            triangles[triangleIndex++] = nextTip;
        }

        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
    }

    private void UpdateMaterialColor()
    {
        if (materialInstance != null)
        {
            materialInstance.color = startColor;
        }
    }
}