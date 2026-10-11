using UnityEngine;

namespace Scrapshift
{
    // Owned by the actor but outside its hierarchy: squash and dash rotation cannot shear the oval.
    [ExecuteAlways]
    public sealed class GroundShadow : MonoBehaviour
    {
        GameObject view;
        MeshRenderer rendererView;
        MaterialPropertyBlock tint;
        float width, offset;
        public Transform View => view != null ? view.transform : null;

        public void Initialize(Mesh mesh, Material material, float diameter, float groundOffset)
        {
            width = diameter; offset = groundOffset;
            view = new GameObject(name + " ground shadow");
            view.AddComponent<MeshFilter>().sharedMesh = mesh;
            rendererView = view.AddComponent<MeshRenderer>();
            rendererView.sharedMaterial = material; rendererView.sortingOrder = 1;
            rendererView.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rendererView.receiveShadows = false;
            tint = new MaterialPropertyBlock();
            Place(transform.position, 1);
            view.SetActive(isActiveAndEnabled);
        }

        public void Place(Vector2 anchor, float scale, float opacity = .392157f)
        {
            if (view == null) return;
            view.transform.SetPositionAndRotation(new Vector3(anchor.x, anchor.y + offset * scale, 0), Quaternion.identity);
            view.transform.localScale = new Vector3(width * scale, width * scale * .3f, 1);
            tint.SetColor("_Color", new Color(0, 0, 0, opacity));
            rendererView.SetPropertyBlock(tint);
        }
        void OnEnable() { if (view != null) view.SetActive(true); }
        void OnDisable() { if (view != null) view.SetActive(false); }
        void OnDestroy()
        {
            if (view == null) return;
            if (Application.isPlaying) Destroy(view); else DestroyImmediate(view);
        }
    }
}
