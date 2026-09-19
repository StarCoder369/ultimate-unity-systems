using UnityEngine;
using UnityEngine.UI;

public class ArcBulletIndicator : Graphic
{
    [Header("Please ignore the above color/ material values that have been inherited from Graphic.")]
    [Space(5)]

    [Header("Ammo")]
    [Tooltip("The total number of bullets that will be visible.")]
    public int bulletCount = 5;

    [Tooltip("How many bullets are currently available.")]
    public int currentAmmo = 3;

    [Header("Arc Settings")]
    [Tooltip("How far the arc sits from the center.")]
    public float radius = 100f;

    [Tooltip("How much of the circle the indicator covers.")]
    [Range(0f, 360f)]
    public float arcAngle = 90f;

    [Tooltip("The angle where the arc begins.")]
    public float startAngle = 0f;

    [Tooltip("The space between each bullet segment.")]
    public float gap = 5f;

    [Tooltip("Higher values make the arc smoother.")]
    [Range(1, 50)]
    public int arcResolution = 20;

    [Tooltip("Higher values make the rounded caps smoother.")]
    [Range(1, 30)]
    public int capResolution = 8;

    [Header("Filled Bullet")]
    [Tooltip("The color used by bullets that are currently available.")]
    public Color filledColor = Color.white;

    [Range(0f, 1f)]
    public float filledAlpha = 1f;

    [Tooltip("The thickness of bullets that are currently available.")]
    public float filledThickness = 20f;

    [Header("Empty Bullet")]
    [Tooltip("The color used by bullets that have already been used.")]
    public Color emptyColor = Color.gray;

    [Range(0f, 1f)]
    public float emptyAlpha = 0.4f;

    [Tooltip("The thickness of bullets that have already been used.")]
    public float emptyThickness = 20f;

    [Header("Editor")]
    [Tooltip("Turn this off if you don't want the indicator updating while editing.")]
    public bool updateInEditor = true;


    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

#if UNITY_EDITOR
        if (!Application.isPlaying && !updateInEditor)
        {
            return;
        }
#endif

        if (bulletCount <= 0)
        {
            return;
        }

        float segmentAngle = (arcAngle - gap * (bulletCount - 1)) / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float segmentStart = startAngle + i * (segmentAngle + gap);
            float segmentEnd = segmentStart + segmentAngle;

            bool filled = i < Mathf.Clamp(currentAmmo, 0, bulletCount);

            Color color = filled ? filledColor : emptyColor;
            color.a = filled ? filledAlpha : emptyAlpha;

            float thickness = filled ? filledThickness : emptyThickness;

            CreateArc(vh, segmentStart, segmentEnd, thickness, color);
            CreateCap(vh, segmentStart, thickness, color, true);
            CreateCap(vh, segmentEnd, thickness, color, false);
        }
    }


    private void CreateArc(VertexHelper vh, float start, float end, float thickness, Color color)
    {
        int startIndex = vh.currentVertCount;

        for (int i = 0; i <= arcResolution; i++)
        {
            float t = i / (float)arcResolution;
            float angle = Mathf.Lerp(start, end, t) * Mathf.Deg2Rad;

            Vector2 outer = new Vector2(Mathf.Cos(angle) * (radius + thickness / 2), Mathf.Sin(angle) * (radius + thickness / 2));
            Vector2 inner = new Vector2(Mathf.Cos(angle) * (radius - thickness / 2), Mathf.Sin(angle) * (radius - thickness / 2));

            vh.AddVert(outer, color, Vector2.zero);
            vh.AddVert(inner, color, Vector2.zero);

            if (i < arcResolution)
            {
                int current = startIndex + i * 2;

                vh.AddTriangle(current, current + 2, current + 1);
                vh.AddTriangle(current + 1, current + 2, current + 3);
            }
        }
    }


    private void CreateCap(VertexHelper vh, float angle, float thickness, Color color, bool startCap)
    {
        float radians = angle * Mathf.Deg2Rad;

        Vector2 center = new Vector2(Mathf.Cos(radians) * radius, Mathf.Sin(radians) * radius);

        int centerIndex = vh.currentVertCount;

        vh.AddVert(center, color, Vector2.zero);

        float direction = startCap ? -1f : 1f;

        for (int i = 0; i <= capResolution; i++)
        {
            float capAngle = radians + direction * Mathf.PI * (i / (float)capResolution);

            Vector2 point = center + new Vector2(Mathf.Cos(capAngle) * (thickness / 2), Mathf.Sin(capAngle) * (thickness / 2));

            vh.AddVert(point, color, Vector2.zero);

            if (i > 0)
            {
                vh.AddTriangle(centerIndex, centerIndex + i, centerIndex + i + 1);
            }
        }
    }


    public void SetAmmo(int ammo)
    {
        currentAmmo = ammo;
        SetVerticesDirty();
    }


    public void SetCapacity(int capacity)
    {
        bulletCount = Mathf.Max(capacity, 0);
        SetVerticesDirty();
    }


    public void SetStartingAngle(float angle)
    {
        startAngle = angle;
        SetVerticesDirty();
    }


    public void SetArcAngle(float angle)
    {
        arcAngle = angle;
        SetVerticesDirty();
    }


    public void SetGap(float gapToSet)
    {
        gap = gapToSet;
        SetVerticesDirty();
    }


    public void SetArcResolution(int resolution)
    {
        arcResolution = resolution;
        SetVerticesDirty();
    }


    public void SetCapResolution(int resolution)
    {
        capResolution = resolution;
        SetVerticesDirty();
    }


    public void SetRadius(float radiusToSet)
    {
        radius = radiusToSet;
        SetVerticesDirty();
    }


    public void SetFilledColorAlpha(float alpha)
    {
        filledAlpha = alpha;
        SetVerticesDirty();
    }


    public void SetEmptyColorAlpha(float alpha)
    {
        emptyAlpha = alpha;
        SetVerticesDirty();
    }


    public void SetFilledRed(float red)
    {
        Color color = filledColor;
        color.r = red;
        filledColor = color;
        SetVerticesDirty();
    }


    public void SetFilledGreen(float green)
    {
        Color color = filledColor;
        color.g = green;
        filledColor = color;
        SetVerticesDirty();
    }


    public void SetFilledBlue(float blue)
    {
        Color color = filledColor;
        color.b = blue;
        filledColor = color;
        SetVerticesDirty();
    }


    public void SetEmptyRed(float red)
    {
        Color color = emptyColor;
        color.r = red;
        emptyColor = color;
        SetVerticesDirty();
    }


    public void SetEmptyGreen(float green)
    {
        Color color = emptyColor;
        color.g = green;
        emptyColor = color;
        SetVerticesDirty();
    }


    public void SetEmptyBlue(float blue)
    {
        Color color = emptyColor;
        color.b = blue;
        emptyColor = color;
        SetVerticesDirty();
    }


    public void SetFilledThickness(float thickness)
    {
        filledThickness = thickness;
        SetVerticesDirty();
    }


    public void SetEmptyThickness(float thickness)
    {
        emptyThickness = thickness;
        SetVerticesDirty();
    }


#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        bulletCount = Mathf.Max(0, bulletCount);
        currentAmmo = Mathf.Clamp(currentAmmo, 0, bulletCount);

        radius = Mathf.Max(0f, radius);
        gap = Mathf.Max(0f, gap);

        arcResolution = Mathf.Max(1, arcResolution);
        capResolution = Mathf.Max(1, capResolution);

        filledThickness = Mathf.Max(0f, filledThickness);
        emptyThickness = Mathf.Max(0f, emptyThickness);

        if (updateInEditor)
        {
            SetVerticesDirty();
        }
    }
#endif
}