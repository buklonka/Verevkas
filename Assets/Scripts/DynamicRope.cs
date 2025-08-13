using UnityEngine;

using System.Collections.Generic;

public class DynamicRope : MonoBehaviour
{
    public static event System.Action OnAllRopesUncrossed;
    private static List<DynamicRope> allRopes = new List<DynamicRope>();

    [Header("Rope Settings")]
    public Transform startPoint;
    public Transform endPoint;
    public int segmentCount = 20;
    public float segmentLength = 0.1f; // The length of each segment
    public float ropeMass = 1f; // The total mass of the rope

    private GameObject ropeSegmentTemplate;
    private HashSet<DynamicRope> intersectingRopes = new HashSet<DynamicRope>();

    void OnEnable()
    {
        allRopes.Add(this);
    }

    void OnDisable()
    {
        allRopes.Remove(this);
    }

    void Start()
    {
        CreateSegmentTemplate();
        GenerateRope();
    }

    private void CreateSegmentTemplate()
    {
        ropeSegmentTemplate = new GameObject("RopeSegmentTemplate");

        SpriteRenderer sr = ropeSegmentTemplate.AddComponent<SpriteRenderer>();
        // Assuming Green.prefab is a sprite, let's try to load it.
        // This might need adjustment based on what Green.prefab actually is.
        GameObject greenPrefab = Resources.Load<GameObject>("Green");
        if (greenPrefab != null && greenPrefab.GetComponent<SpriteRenderer>() != null)
        {
            sr.sprite = greenPrefab.GetComponent<SpriteRenderer>().sprite;
        }
        sr.color = Color.green; // Fallback color

        // The size of the sprite should match the segment length
        // This part is tricky without knowing the sprite's original size.
        // We'll assume a simple box for now.
        ropeSegmentTemplate.transform.localScale = new Vector3(0.1f, segmentLength, 1f);

        Rigidbody2D rb = ropeSegmentTemplate.AddComponent<Rigidbody2D>();
        rb.mass = ropeMass / segmentCount;

        HingeJoint2D joint = ropeSegmentTemplate.AddComponent<HingeJoint2D>();
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = new Vector2(0, segmentLength / 2f);
        joint.connectedAnchor = new Vector2(0, -segmentLength / 2f);

        ropeSegmentTemplate.SetActive(false); // Keep the template inactive
    }

    private void GenerateRope()
    {
        if (startPoint == null || endPoint == null || ropeSegmentTemplate == null)
        {
            Debug.LogError("Rope points or template not set!");
            return;
        }

        Rigidbody2D previousSegmentRB = startPoint.GetComponent<Rigidbody2D>();
        if (previousSegmentRB == null)
        {
            Debug.LogError("Start point must have a Rigidbody2D component!");
            return;
        }

        for (int i = 0; i < segmentCount; i++)
        {
            GameObject segmentGO = Instantiate(ropeSegmentTemplate, transform);
            segmentGO.name = "RopeSegment_" + i;
            segmentGO.SetActive(true);

            segmentGO.transform.position = startPoint.position + Vector3.down * (segmentLength * (i + 1));

            HingeJoint2D joint = segmentGO.GetComponent<HingeJoint2D>();
            joint.connectedBody = previousSegmentRB;

            RopeSegment segment = segmentGO.AddComponent<RopeSegment>();
            segment.parentRope = this;

            previousSegmentRB = segmentGO.GetComponent<Rigidbody2D>();
        }

        // Attach the final segment to the end point
        HingeJoint2D endJoint = endPoint.gameObject.AddComponent<HingeJoint2D>();
        endJoint.connectedBody = previousSegmentRB;
        endJoint.autoConfigureConnectedAnchor = false;
        endJoint.connectedAnchor = Vector2.zero;
        endJoint.anchor = Vector2.zero;
    }

    public bool IsIntersecting => intersectingRopes.Count > 0;

    public void AddIntersection(DynamicRope otherRope)
    {
        if (intersectingRopes.Add(otherRope) && intersectingRopes.Count == 1)
        {
            SetRopeColor(Color.red);
        }
    }

    public void RemoveIntersection(DynamicRope otherRope)
    {
        if (intersectingRopes.Remove(otherRope) && intersectingRopes.Count == 0)
        {
            SetRopeColor(Color.green);
        }
    }

    private void SetRopeColor(Color color)
    {
        foreach (Transform segment in transform)
        {
            SpriteRenderer sr = segment.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = color;
            }
        }
    }

    public static void CheckForWinCondition()
    {
        bool allUncrossed = true;
        foreach (var rope in allRopes)
        {
            if (rope.IsIntersecting)
            {
                allUncrossed = false;
                break;
            }
        }

        if (allUncrossed && allRopes.Count > 0)
        {
            OnAllRopesUncrossed?.Invoke();
        }
    }
}
