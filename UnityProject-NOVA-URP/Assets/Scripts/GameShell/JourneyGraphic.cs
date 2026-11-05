using UnityEngine;
using UnityEngine.UI;

public enum JourneyShape { Disc, Ring, Panel }

public sealed class JourneyGraphic : MaskableGraphic {
    public JourneyShape shape;
    public float radius = 12, thickness = 2;
    protected override void OnPopulateMesh(VertexHelper helper) {
        helper.Clear();
        Rect bounds = rectTransform.rect;
        if (shape == JourneyShape.Disc || shape == JourneyShape.Ring) {
            const int count = 64;
            Vector2 center = bounds.center;
            float outer = Mathf.Min(bounds.width, bounds.height) * .5f;
            float inner = shape == JourneyShape.Disc ? 0 : Mathf.Max(0, outer - thickness);
            for (int i = 0; i < count; i++) {
                float a = i * Mathf.PI * 2 / count, b = (i + 1) * Mathf.PI * 2 / count;
                Vector2 u = new Vector2(Mathf.Cos(a), Mathf.Sin(a)), v = new Vector2(Mathf.Cos(b), Mathf.Sin(b));
                Quad(helper, center + u * outer, center + v * outer, center + v * inner, center + u * inner, color);
            }
            return;
        }
        float corner = Mathf.Min(radius, Mathf.Min(bounds.width, bounds.height) * .5f);
        helper.AddVert(bounds.center, color, Vector2.zero);
        for (int cornerIndex = 0; cornerIndex < 4; cornerIndex++) {
            Vector2 center = new Vector2(cornerIndex == 0 || cornerIndex == 3 ? bounds.xMax - corner : bounds.xMin + corner,
                cornerIndex < 2 ? bounds.yMax - corner : bounds.yMin + corner);
            for (int step = 0; step <= 8; step++) {
                float angle = (cornerIndex * 90 + step * 90f / 8) * Mathf.Deg2Rad;
                helper.AddVert(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * corner, color, Vector2.zero);
            }
        }
        for (int i = 1; i <= 36; i++) helper.AddTriangle(0, i, i == 36 ? 1 : i + 1);
    }
    public static void Quad(VertexHelper helper, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color tint) {
        int index = helper.currentVertCount;
        helper.AddVert(a, tint, Vector2.zero); helper.AddVert(b, tint, Vector2.zero);
        helper.AddVert(c, tint, Vector2.zero); helper.AddVert(d, tint, Vector2.zero);
        helper.AddTriangle(index, index + 1, index + 2); helper.AddTriangle(index, index + 2, index + 3);
    }
}

public sealed class JourneyRouteGraphic : MaskableGraphic {
    public Vector2[] points;
    protected override void OnPopulateMesh(VertexHelper helper) {
        helper.Clear();
        if (points == null) return;
        var bounds = rectTransform.rect;
        for (int segment = 1; segment < points.Length; segment++) {
            Vector2 a = new Vector2(points[segment - 1].x * bounds.width, -points[segment - 1].y * bounds.height);
            Vector2 b = new Vector2(points[segment].x * bounds.width, -points[segment].y * bounds.height);
            Vector2 c = new Vector2((a.x + b.x) / 2, a.y), d = new Vector2((a.x + b.x) / 2, b.y);
            Vector2 previous = a;
            for (int step = 1; step <= 32; step++) {
                float t = step / 32f, u = 1 - t;
                Vector2 next = u * u * u * a + 3 * u * u * t * c + 3 * u * t * t * d + t * t * t * b;
                Vector2 normal = new Vector2(-(next - previous).y, (next - previous).x).normalized * 1.4f;
                JourneyGraphic.Quad(helper, previous + normal, next + normal, next - normal, previous - normal, color);
                previous = next;
            }
        }
    }
}
