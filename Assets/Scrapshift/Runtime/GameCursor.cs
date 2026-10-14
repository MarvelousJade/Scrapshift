using UnityEngine;

namespace Scrapshift
{
    // The source game uses its small arrow in menus and a centered reticle for mouse aiming.
    public sealed class GameCursor : MonoBehaviour
    {
        enum CursorKind { Unset, Arrow, Hidden, Aim }

        ArenaGame game;
        Texture2D arrow;
        Texture2D aim;
        CursorKind applied;

        public void Initialize(ArenaGame owner)
        {
            game = owner;
            arrow = LoadCursor("CursorNormal");
            aim = LoadCursor("CursorManual");
            Apply();
        }

        static Texture2D LoadCursor(string resource)
        {
            var asset = Resources.Load<TextAsset>(resource);
            if (asset == null) { Debug.LogError("Missing cursor resource: " + resource); return null; }
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texture, asset.bytes, false))
            {
                Destroy(texture);
                Debug.LogError("Could not decode cursor resource: " + resource);
                return null;
            }
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }

        void LateUpdate() { Apply(); }

        void Apply()
        {
            if (game == null) return;
            bool playing = game.Rules.Phase == RunPhase.Playing;
            bool manualAim = playing && CombatInput.ManualAim(
                GameSettings.Flag("manualAim", false),
                GameSettings.Flag("aimPressed", false),
                Input.GetMouseButton(0));
            CursorKind wanted = manualAim ? CursorKind.Aim :
                playing && !GameSettings.Flag("mouseMove", false) && !GameSettings.Flag("aimPressed", false)
                    ? CursorKind.Hidden : CursorKind.Arrow;
            if (wanted == applied) return;
            applied = wanted;
            Cursor.visible = wanted != CursorKind.Hidden;
            switch (wanted)
            {
                case CursorKind.Aim:
                    Cursor.SetCursor(aim, new Vector2(35, 35), CursorMode.Auto);
                    break;
                case CursorKind.Arrow:
                    Cursor.SetCursor(arrow, new Vector2(3, 3), CursorMode.Auto);
                    break;
                default:
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    break;
            }
        }

        void OnDisable()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            Cursor.visible = true;
            applied = CursorKind.Unset;
        }

        void OnDestroy()
        {
            if (arrow != null) Destroy(arrow);
            if (aim != null) Destroy(aim);
        }
    }
}
