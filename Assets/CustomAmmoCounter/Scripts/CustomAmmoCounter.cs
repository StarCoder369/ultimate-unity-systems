using UnityEngine;
using UnityEngine.UI;

public class ArcBulletIndicator : Graphic
{
    [Header("Please ignore the above color/material values. They have been inherited from Graphic, and are difficult to hide without a custom editor script.")]
    [Header("Ammo")]
    // So bullet count is total bullets that will be visible
    public int bulletCount = 5;
    // Current ammo is current bullets, which should be less or equal to bullet count.
    public int currentAmmo = 3;

    [Header("Arc Settings")]
    // Basically it's how large the curve is and how far away from the center
    public float radius = 100f;
    // How long the arc is, 180 is semi-circle, 90 is one fourth of a circle, and so on
    // Doesn't support 360 degrees very well
    public float arcAngle = 90f;
    public float startAngle = 0f;
    public float gap = 5f;
    public int arcResolution = 20;
    public int capResolution = 8;

    [Header("Filled Bullet")]
    public Color filledColor = Color.white;
    [Range(0f, 1f)] public float filledAlpha = 1f;
    public float filledThickness = 20f;

    [Header("Empty Bullet")]
    public Color emptyColor = Color.gray;
    [Range(0f, 1f)] public float emptyAlpha = 0.4f;
    public float emptyThickness = 20f;

    [Header("Editor")]
    // If false, it will just not show in the editor.
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