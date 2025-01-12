using UnityEngine;
using System.Collections;

public class Svyaznoy : MonoBehaviour
{
    [SerializeField] private Uzelok connectedUzelok;
    [SerializeField] private GameObject greenRopePrefab;
    [SerializeField] private GameObject redRopePrefab;
    [SerializeField] private float ropeThickness = 0.1f;

    private LineRenderer lineRenderer;
    private GameObject currentRopePrefab;
    private bool isInitialCheckDone = false;

    public static event System.Action OnAllRopesGreen;

    void Start()
    {
        InitializeRope();
        Invoke(nameof(EnableInitialCheck), 1f);
    }

    public void EnableInitialCheck()
    {
        isInitialCheckDone = true;
    }

    private void InitializeRope()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.startWidth = ropeThickness;
        lineRenderer.endWidth = ropeThickness;
        lineRenderer.sortingLayerName = "Ropes";
        lineRenderer.sortingOrder = 0;
        lineRenderer.useWorldSpace = true;

        currentRopePrefab = greenRopePrefab;
    }

    void LateUpdate()
    {
        if (connectedUzelok == null)
        {
            if (lineRenderer != null)
                lineRenderer.positionCount = 0;
            return;
        }

        Vector3 startPos = transform.position;
        Vector3 endPos = connectedUzelok.transform.position;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] { startPos, endPos });

        bool isIntersecting = CheckIntersections(startPos, endPos);
        UpdateRopeColor(isIntersecting);
    }

    private void UpdateRopeColor(bool isIntersecting)
    {
        currentRopePrefab = isIntersecting ? redRopePrefab : greenRopePrefab;

        if (currentRopePrefab != null && currentRopePrefab.GetComponent<SpriteRenderer>() != null && currentRopePrefab.GetComponent<SpriteRenderer>().sprite != null)
        {
            float textureWidth = currentRopePrefab.GetComponent<SpriteRenderer>().sprite.rect.width;
            float pixelsPerUnit = currentRopePrefab.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;
            lineRenderer.material.mainTexture = currentRopePrefab.GetComponent<SpriteRenderer>().sprite.texture;
            lineRenderer.material.mainTextureScale = new Vector2(Vector3.Distance(transform.position, connectedUzelok.transform.position) / (textureWidth / pixelsPerUnit), 1);

            float angle = Mathf.Atan2(connectedUzelok.transform.position.y - transform.position.y, connectedUzelok.transform.position.x - transform.position.x) * Mathf.Rad2Deg;
            float textureOffset = angle / 360f;
            lineRenderer.material.mainTextureOffset = new Vector2(textureOffset, 0);
        }

        CheckAllRopesGreen();
    }

    private void CheckAllRopesGreen()
    {
        if (!isInitialCheckDone) return;

        Svyaznoy[] allRopes = FindObjectsByType<Svyaznoy>(FindObjectsSortMode.None);

        foreach (var rope in allRopes)
        {
            if (rope.IsRed())
            {
                return;
            }
        }

        StartCoroutine(DelayedWinEvent());
    }

    private IEnumerator DelayedWinEvent()
    {
        yield return new WaitForSeconds(0.5f);
        OnAllRopesGreen?.Invoke();
    }

    public bool IsRed()
    {
        return currentRopePrefab == redRopePrefab;
    }

    private bool CheckIntersections(Vector3 startPos, Vector3 endPos)
    {
        Svyaznoy[] allRopes = FindObjectsByType<Svyaznoy>(FindObjectsSortMode.None);

        foreach (var rope in allRopes)
        {
            if (rope != this && rope.connectedUzelok != null)
            {
                Vector3 otherStartPos = rope.transform.position;
                Vector3 otherEndPos = rope.connectedUzelok.transform.position;

                if (AreLinesIntersecting(startPos, endPos, otherStartPos, otherEndPos) &&
                    !IsNearNode(startPos, endPos, otherStartPos, otherEndPos))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool AreLinesIntersecting(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        Vector2 a = p1;
        Vector2 b = p2;
        Vector2 c = p3;
        Vector2 d = p4;

        float orientation1 = GetOrientation(a, b, c);
        float orientation2 = GetOrientation(a, b, d);
        float orientation3 = GetOrientation(c, d, a);
        float orientation4 = GetOrientation(c, d, b);

        if (orientation1 != orientation2 && orientation3 != orientation4)
            return true;

        if (orientation1 == 0 && IsPointOnSegment(a, c, b)) return true;
        if (orientation2 == 0 && IsPointOnSegment(a, d, b)) return true;
        if (orientation3 == 0 && IsPointOnSegment(c, a, d)) return true;
        if (orientation4 == 0 && IsPointOnSegment(c, b, d)) return true;

        return false;
    }

    private float GetOrientation(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float val = (p2.y - p1.y) * (p3.x - p2.x) - (p2.x - p1.x) * (p3.y - p2.y);
        if (val == 0) return 0;
        return (val > 0) ? 1 : 2;
    }

    private bool IsPointOnSegment(Vector2 p, Vector2 q, Vector2 r)
    {
        if (q.x <= Mathf.Max(p.x, r.x) && q.x >= Mathf.Min(p.x, r.x) &&
            q.y <= Mathf.Max(p.y, r.y) && q.y >= Mathf.Min(p.y, r.y))
            return true;
        return false;
    }

    private bool IsNearNode(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2)
    {
        float nodeRadius = 0.5f;

        if (Vector3.Distance(start1, start2) < nodeRadius || Vector3.Distance(start1, end2) < nodeRadius ||
            Vector3.Distance(end1, start2) < nodeRadius || Vector3.Distance(end1, end2) < nodeRadius)
        {
            return true;
        }

        return false;
    }

    public void ResetToInitialState()
    {
        currentRopePrefab = greenRopePrefab;
        UpdateRopeColor(false);
    }
}