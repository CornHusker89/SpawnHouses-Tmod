using System;
using Microsoft.Xna.Framework;
using SpawnHouses.Core.DataStructures;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SpawnHouses.Core;

public class CameraManager : ModSystem {
    private enum CamState {
        Inactive,
        TransitioningIn,
        Locked,
        TransitioningOut
    }

    private static CamState _state = CamState.Inactive;

    private static Vector2 _currentScreenPos;
    private static Vector2 _startScreenPos;
    private static Vector2 _exitScreenPos;
    private static Vector2 _entryScreenPos;
    private static Vector2 _targetScreenPos;
    private static bool _useTeleportCut;

    private static float _startZoom;
    private static float _targetZoom;
    private static float _duration;
    private static float _timer;
    private static float _currentZoom;
    private static float _vanillaZoom;

    private static float EaseInOutCubic(float t) => t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f;

    public override void PostUpdateEverything() {
        if (_state == CamState.Inactive) {
            _currentZoom = Main.GameViewMatrix.Zoom.X;
            _currentScreenPos = Main.screenPosition;
            return;
        }

        if (Main.dedServ) return;

        if (_state == CamState.Locked) {
            Main.screenPosition = _targetScreenPos;
            _currentZoom = _targetZoom;
            return;
        }

        if (_state == CamState.TransitioningOut) _targetScreenPos = GetVanillaTranslation();

        _timer += 1f / 60f;
        float t = MathHelper.Clamp(_timer / _duration, 0f, 1f);

        if (_useTeleportCut)
            _currentScreenPos = t < 0.5f ? Vector2.Lerp(_startScreenPos, _exitScreenPos, EaseInOutCubic(t)) : Vector2.Lerp(_entryScreenPos, _targetScreenPos, EaseInOutCubic(t));
        else
            _currentScreenPos = Vector2.Lerp(_startScreenPos, _targetScreenPos, EaseInOutCubic(t));

        _currentZoom = MathHelper.Lerp(_startZoom, _targetZoom, EaseInOutCubic(t));

        if (t >= 1f)
            _state = _state == CamState.TransitioningIn ? CamState.Locked : CamState.Inactive;
    }

    public override void ModifyScreenPosition() {
        if (_state == CamState.Inactive) return;
        Main.screenPosition = _currentScreenPos;
    }

    public override void ModifyTransformMatrix(ref SpriteViewMatrix transform) {
        if (_state == CamState.Inactive) return;
        _vanillaZoom = Main.GameViewMatrix.Zoom.X;
        transform.Zoom = new Vector2(_currentZoom);
    }

    private static Vector2 GetVanillaTranslation() {
        Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
        return Main.LocalPlayer.Center - screenSize / 2f + new Vector2(0, 9);
    }

    private static void BeginSegment(Vector2 from, Vector2 to, float fromZoom, float toZoom) {
        _startScreenPos = from;
        _targetScreenPos = to;
        _startZoom = fromZoom;
        _targetZoom = toZoom;

        float totalDist = Vector2.Distance(from, to);
        float distTiles = totalDist / 16f;
        float excess = MathF.Max(0f, distTiles - 50f);
        _duration = 0.8f + 0.059f * MathF.Pow(excess, 0.47f);
        _timer = 0f;

        float speed = 1400f + 45f * MathF.Pow(distTiles, 1f);
        speed = Math.Min(speed, 9000f);
        float naturalTravel = speed * _duration; // distance a single, uncut lerp would cover at this speed

        _useTeleportCut = naturalTravel < totalDist;

        if (_useTeleportCut) {
            Vector2 dir = Vector2.Normalize(to - from);
            float segmentTime = _duration * 0.5f;
            float travel = speed * segmentTime;

            travel = Math.Min(travel, totalDist / 2f);
            _exitScreenPos = from + dir * travel;
            _entryScreenPos = to - dir * travel;
        }
    }

    /// <summary>
    ///     centers cam on world (not tile) position
    /// </summary>
    /// <param name="worldPos"></param>
    /// <param name="zoom"></param>
    public static void TransitionToPosition(Vector2 worldPos, float zoom) {
        Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
        BeginSegment(Main.screenPosition, worldPos - screenSize / 2f, _currentZoom, zoom);
        _state = CamState.TransitioningIn;
    }

    /// <summary>
    ///     camera will always see entire box, but may see beyond on one axis of the box
    /// </summary>
    /// <param name="rect"></param>
    public static void TransitionToTileRect(TileBox rect) {
        Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
        Rectangle worldRect = rect.Scale(16);

        float zoomX = screenSize.X / worldRect.Width;
        float zoomY = screenSize.Y / worldRect.Height;
        float zoom = Math.Min(zoomX, zoomY);

        Vector2 rectCenterWorld = new(
            worldRect.X + worldRect.Width / 2f,
            worldRect.Y + worldRect.Height / 2f
        );
        TransitionToPosition(rectCenterWorld, zoom);
    }

    public static void TransitionToStructure(StructureRoot structure, int paddingTiles = 5) {
        TransitionToTileRect(structure.Tilemap.BoundingBox.Inflate(paddingTiles));
    }

    /// <summary>
    ///     returns camera back to normal position
    /// </summary>
    public static void ReleaseCamera() {
        if (_state is CamState.Inactive) return;
        BeginSegment(Main.screenPosition, GetVanillaTranslation(), _currentZoom, _vanillaZoom);
        _state = CamState.TransitioningOut;
    }
}