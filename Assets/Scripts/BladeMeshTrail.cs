using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class BladeMeshTrail : MonoBehaviour
{
    [Header("Blade Points")]
    [SerializeField] private Transform bladeBasePoint;
    [SerializeField] private Transform bladeTipPoint;

    [Header("Trail Settings")]
    [SerializeField] private Material trailMaterial;
    [SerializeField] private float trailLifetime = 0.10f;
    [SerializeField] private float minMoveDistance = 0.06f;
    [SerializeField] private int maxSegments = 12;

    [Header("Safety")]
    [Tooltip("If the blade base or tip moves more than this in one frame, clear the trail instead of drawing a giant mesh.")]
    [SerializeField] private float maxPointMovePerFrame = 0.75f;

    private Mesh mesh;

    private readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<float> times = new List<float>();
    private readonly List<int> triangles = new List<int>();

    private Vector3 lastBasePosition;
    private Vector3 lastTipPosition;
    private bool hasLastPosition;

    private bool emitting;

    public bool Emitting
    {
        get { return emitting; }
        set
        {
            if (emitting == value)
                return;

            emitting = value;
            hasLastPosition = false;

            if (!emitting)
                ClearTrail();
        }
    }

    private void Awake()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        mesh = new Mesh();
        mesh.name = "Blade Mesh Trail";
        mesh.MarkDynamic();

        meshFilter.sharedMesh = mesh;

        if (trailMaterial != null)
            meshRenderer.material = trailMaterial;

        ClearTrail();
    }

    private void OnDisable()
    {
        ClearTrail();
    }

    private void LateUpdate()
    {
        if (!emitting)
            return;

        if (bladeBasePoint == null || bladeTipPoint == null)
            return;

        AddSegmentIfNeeded();
        RemoveOldSegments();
        BuildMesh();
    }

    private void AddSegmentIfNeeded()
    {
        Vector3 currentBase = bladeBasePoint.position;
        Vector3 currentTip = bladeTipPoint.position;

        if (!hasLastPosition)
        {
            lastBasePosition = currentBase;
            lastTipPosition = currentTip;
            hasLastPosition = true;
            return;
        }

        float baseDistance = Vector3.Distance(lastBasePosition, currentBase);
        float tipDistance = Vector3.Distance(lastTipPosition, currentTip);

        // IMPORTANT: this prevents Scene-view dragging / teleporting from creating a huge mesh.
        if (baseDistance > maxPointMovePerFrame || tipDistance > maxPointMovePerFrame)
        {
            ClearTrail();
            lastBasePosition = currentBase;
            lastTipPosition = currentTip;
            hasLastPosition = true;
            return;
        }

        if (baseDistance < minMoveDistance && tipDistance < minMoveDistance)
            return;

        vertices.Add(lastBasePosition);
        vertices.Add(lastTipPosition);
        vertices.Add(currentBase);
        vertices.Add(currentTip);

        times.Add(Time.time);

        lastBasePosition = currentBase;
        lastTipPosition = currentTip;

        while (times.Count > maxSegments)
            RemoveSegment(0);
    }

    private void RemoveOldSegments()
    {
        for (int i = times.Count - 1; i >= 0; i--)
        {
            if (Time.time - times[i] > trailLifetime)
                RemoveSegment(i);
        }
    }

    private void RemoveSegment(int segmentIndex)
    {
        int vertexIndex = segmentIndex * 4;

        if (vertexIndex + 4 <= vertices.Count)
            vertices.RemoveRange(vertexIndex, 4);

        if (segmentIndex >= 0 && segmentIndex < times.Count)
            times.RemoveAt(segmentIndex);
    }

    private void BuildMesh()
    {
        if (mesh == null)
            return;

        mesh.Clear();

        int segmentCount = times.Count;

        if (segmentCount <= 0)
            return;

        triangles.Clear();

        for (int i = 0; i < segmentCount; i++)
        {
            int index = i * 4;

            triangles.Add(index + 0);
            triangles.Add(index + 1);
            triangles.Add(index + 2);

            triangles.Add(index + 2);
            triangles.Add(index + 1);
            triangles.Add(index + 3);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
    }

    public void ClearTrail()
    {
        vertices.Clear();
        times.Clear();
        triangles.Clear();
        hasLastPosition = false;

        if (mesh != null)
            mesh.Clear();
    }
}