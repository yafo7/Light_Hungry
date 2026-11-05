using UnityEngine;
using UnityEngine.UI;

// Select a single convex comic panel using its original pixels, without redrawing the art.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class OpeningPanelGraphic : MaskableGraphic {
    public OpeningBeat beat;
    public override Texture mainTexture => beat != null && beat.image ? beat.image : Texture2D.whiteTexture;
    protected override void OnPopulateMesh(VertexHelper mesh) {
        mesh.Clear();
        if (beat == null || !beat.image || beat.polygon == null || beat.polygon.Length < 3 || beat.sourceSize.x <= 0 || beat.sourceSize.y <= 0) return;
        Vector2 min = beat.polygon[0], max = min;
        foreach (var point in beat.polygon) { min = Vector2.Min(min, point); max = Vector2.Max(max, point); }
        Vector2 size = max - min;
        if (size.x <= 0 || size.y <= 0) return;
        float scale = Mathf.Min(rectTransform.rect.width / size.x, rectTransform.rect.height / size.y);
        Vector2 middle = (min + max) * .5f;
        foreach (var point in beat.polygon) {
            var local = rectTransform.rect.center + new Vector2(point.x - middle.x, middle.y - point.y) * scale;
            // Coordinates belong to the original artwork, independent of importer resizing/compression.
            mesh.AddVert(local, color, new Vector2(point.x / beat.sourceSize.x, 1 - point.y / beat.sourceSize.y));
        }
        for (int i = 1; i + 1 < beat.polygon.Length; i++) mesh.AddTriangle(0, i, i + 1);
    }
}
