using UnityEngine;
using System.Collections;

public class Svyaznoy : MonoBehaviour
{
    public Uzelok connectedUzelok; // Узел, к которому привязана верёвка
    public GameObject greenRopePrefab; // Префаб для зелёной верёвки
    public GameObject redRopePrefab; // Префаб для красной верёвки
    public float ropeThickness = 0.1f; // Толщина верёвки

    private LineRenderer lineRenderer; // Компонент LineRenderer для отрисовки верёвки
    private GameObject currentRopePrefab; // Текущий префаб верёвки (зелёный или красный)
    private bool isInitialCheckDone = false; // Флаг для отслеживания начальной проверки

    // Событие, которое вызывается, когда все верёвки зелёные
    public static event System.Action OnAllRopesGreen;

    void Start()
    {
        InitializeRope();
        Invoke(nameof(EnableInitialCheck), 1f); // Задержка 1 секунда перед первой проверкой
    }

    // Метод для разрешения проверки состояния верёвок
    public void EnableInitialCheck()
    {
        isInitialCheckDone = true; // Разрешаем проверку состояния верёвок
    }

    private void InitializeRope()
    {
        // Инициализация LineRenderer
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

        // Устанавливаем начальный префаб верёвки (зелёный)
        currentRopePrefab = greenRopePrefab;
    }

    void LateUpdate()
    {
        // Если целевой узел не задан, очищаем LineRenderer
        if (connectedUzelok == null)
        {
            if (lineRenderer != null)
                lineRenderer.positionCount = 0;
            return;
        }

        // Получаем начальную и конечную позиции верёвки
        Vector3 startPos = transform.position;
        Vector3 endPos = connectedUzelok.transform.position;

        // Устанавливаем позиции для LineRenderer
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] { startPos, endPos });

        // Проверяем пересечения с другими верёвками
        bool isIntersecting = CheckIntersections(startPos, endPos);

        // Меняем цвет верёвки в зависимости от пересечения
        UpdateRopeColor(isIntersecting);
    }

    private void UpdateRopeColor(bool isIntersecting)
    {
        // Выбираем префаб в зависимости от пересечения
        currentRopePrefab = isIntersecting ? redRopePrefab : greenRopePrefab;

        // Устанавливаем текстуру верёвки
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
        else
        {
            Debug.LogWarning("Rope prefab or its SpriteRenderer/Sprite is missing!");
        }

        // Проверяем состояние всех верёвок после изменения цвета
        CheckAllRopesGreen();
    }

    private void CheckAllRopesGreen()
    {
        if (!isInitialCheckDone) return; // Игнорируем проверку до завершения начальной задержки

        Svyaznoy[] allRopes = FindObjectsByType<Svyaznoy>(FindObjectsSortMode.None);

        foreach (var rope in allRopes)
        {
            if (rope.IsRed()) // Если хотя бы одна верёвка красная, победа не наступила
            {
                return;
            }
        }

        // Задержка перед вызовом события победы
        StartCoroutine(DelayedWinEvent());
    }

    private IEnumerator DelayedWinEvent()
    {
        yield return new WaitForSeconds(0.5f); // Задержка 0.5 секунды

        // Если все верёвки зелёные, вызываем событие победы
        Debug.Log("Все верёвки зелёные! Вызываем событие победы.");
        OnAllRopesGreen?.Invoke();
    }

    // Метод для проверки, красная ли верёвка
    public bool IsRed()
    {
        return currentRopePrefab == redRopePrefab;
    }

    // Метод для проверки пересечения двух отрезков
    private bool CheckIntersections(Vector3 startPos, Vector3 endPos)
    {
        Svyaznoy[] allRopes = FindObjectsByType<Svyaznoy>(FindObjectsSortMode.None);

        foreach (var rope in allRopes)
        {
            if (rope != this && rope.connectedUzelok != null)
            {
                Vector3 otherStartPos = rope.transform.position;
                Vector3 otherEndPos = rope.connectedUzelok.transform.position;

                // Проверяем пересечение двух отрезков
                if (AreLinesIntersecting(startPos, endPos, otherStartPos, otherEndPos) &&
                    !IsNearNode(startPos, endPos, otherStartPos, otherEndPos))
                {
                    return true; // Пересечение обнаружено
                }
            }
        }

        return false; // Пересечений нет
    }

    // Метод для проверки пересечения двух отрезков
    private bool AreLinesIntersecting(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        // Преобразуем Vector3 в Vector2 для упрощения вычислений
        Vector2 a = p1;
        Vector2 b = p2;
        Vector2 c = p3;
        Vector2 d = p4;

        // Вычисляем ориентации
        float orientation1 = GetOrientation(a, b, c);
        float orientation2 = GetOrientation(a, b, d);
        float orientation3 = GetOrientation(c, d, a);
        float orientation4 = GetOrientation(c, d, b);

        // Общий случай пересечения
        if (orientation1 != orientation2 && orientation3 != orientation4)
            return true;

        // Специальные случаи (отрезки касаются друг друга)
        if (orientation1 == 0 && IsPointOnSegment(a, c, b)) return true;
        if (orientation2 == 0 && IsPointOnSegment(a, d, b)) return true;
        if (orientation3 == 0 && IsPointOnSegment(c, a, d)) return true;
        if (orientation4 == 0 && IsPointOnSegment(c, b, d)) return true;

        return false;
    }

    // Метод для вычисления ориентации трёх точек
    private float GetOrientation(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float val = (p2.y - p1.y) * (p3.x - p2.x) - (p2.x - p1.x) * (p3.y - p2.y);
        if (val == 0) return 0; // Коллинеарны
        return (val > 0) ? 1 : 2; // По часовой или против часовой стрелки
    }

    // Метод для проверки, лежит ли точка на отрезке
    private bool IsPointOnSegment(Vector2 p, Vector2 q, Vector2 r)
    {
        if (q.x <= Mathf.Max(p.x, r.x) && q.x >= Mathf.Min(p.x, r.x) &&
            q.y <= Mathf.Max(p.y, r.y) && q.y >= Mathf.Min(p.y, r.y))
            return true;
        return false;
    }

    // Метод для проверки, находится ли пересечение вблизи узлов
    private bool IsNearNode(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2)
    {
        float nodeRadius = 0.5f; // Радиус коллайдера узла (настройте под ваш проект)

        // Проверяем, находится ли пересечение вблизи начального или конечного узла первой верёвки
        if (Vector3.Distance(start1, start2) < nodeRadius || Vector3.Distance(start1, end2) < nodeRadius ||
            Vector3.Distance(end1, start2) < nodeRadius || Vector3.Distance(end1, end2) < nodeRadius)
        {
            return true;
        }

        return false;
    }

    // Метод для сброса верёвки в начальное состояние
    public void ResetToInitialState()
    {
        // Сбрасываем цвет верёвки на зелёный
        currentRopePrefab = greenRopePrefab;
        UpdateRopeColor(false); // Обновляем цвет верёвки
    }
}