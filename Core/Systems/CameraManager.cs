using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using SpawnHouses.Core.Geometry;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SpawnHouses.Core.Systems;

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
    private static float _currentZoom;
    private static float _vanillaZoom;

    private static Vector2 _previousScreenPos;
    private static Vector2 _currentVelocity; // px/sec
    private static Vector2 _startVelocity; // captured at BeginSegment
    private static Vector2 _cutVelocity;

    private static float _previousZoom;
    private static float _currentZoomVelocity; // zoom units/sec
    private static float _startZoomVelocity;

    private static readonly Stopwatch Clock = Stopwatch.StartNew(); // per-segment, restarted in BeginSegment
    private static readonly Stopwatch FrameClock = Stopwatch.StartNew(); // never restarted — for per-tick delta only
    private static double _lastFrameTime;
    public static bool IsTransitioning => _state is CamState.TransitioningIn or CamState.TransitioningOut;

    public override void PostUpdateEverything() {
        if (Main.dedServ) return;
        
        float deltaTime = GetDeltaTime();

        if (_state == CamState.Inactive) {
            _currentZoom = Main.GameViewMatrix.Zoom.X;
            _currentScreenPos = Main.screenPosition;
            UpdateVelocityTracking(deltaTime);
            return;
        }

        if (_state == CamState.Locked) {
            Main.screenPosition = _targetScreenPos;
            _currentScreenPos = _targetScreenPos;
            _currentZoom = _targetZoom;
            UpdateVelocityTracking(deltaTime);
            return;
        }

        if (_state == CamState.TransitioningOut) _targetScreenPos = GetVanillaTranslation();

        float t = MathHelper.Clamp((float)Clock.Elapsed.TotalSeconds / _duration, 0f, 1f);

        if (_useTeleportCut) {
            float halfDuration = _duration * 0.5f;
            if (t < 0.5f) {
                float localT = MathHelper.Clamp((float)Clock.Elapsed.TotalSeconds / halfDuration, 0f, 1f);
                _currentScreenPos = HermitePosition(_startScreenPos, _startVelocity, _exitScreenPos, _cutVelocity, localT, halfDuration);
            }
            else {
                float localT = MathHelper.Clamp(((float)Clock.Elapsed.TotalSeconds - halfDuration) / halfDuration, 0f, 1f);
                _currentScreenPos = HermitePosition(_entryScreenPos, _cutVelocity, _targetScreenPos, Vector2.Zero, localT, halfDuration);
            }
        }
        else {
            _currentScreenPos = HermitePosition(_startScreenPos, _startVelocity, _targetScreenPos, Vector2.Zero, t, _duration);
        }

        // zoom stays on the full, un-split curve — only position teleports, so zoom keeps easing smoothly straight through
        _currentZoom = HermiteScalar(_startZoom, _startZoomVelocity, _targetZoom, 0f, t, _duration);

        UpdateVelocityTracking(deltaTime);

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

    private static Vector2 HermitePosition(Vector2 p0, Vector2 v0, Vector2 p1, Vector2 v1, float t, float duration) {
        float t2 = t * t, t3 = t2 * t;
        float h00 = 2f * t3 - 3f * t2 + 1f;
        float h10 = t3 - 2f * t2 + t;
        float h01 = -2f * t3 + 3f * t2;
        float h11 = t3 - t2;
        return h00 * p0 + h10 * (v0 * duration) + h01 * p1 + h11 * (v1 * duration);
    }

    private static float HermiteScalar(float p0, float v0, float p1, float v1, float t, float duration) {
        float t2 = t * t, t3 = t2 * t;
        float h00 = 2f * t3 - 3f * t2 + 1f;
        float h10 = t3 - 2f * t2 + t;
        float h01 = -2f * t3 + 3f * t2;
        float h11 = t3 - t2;
        return h00 * p0 + h10 * (v0 * duration) + h01 * p1 + h11 * (v1 * duration);
    }

    private static void UpdateVelocityTracking(float deltaTime) {
        if (deltaTime > 0f) {
            _currentVelocity = (_currentScreenPos - _previousScreenPos) / deltaTime;
            _currentZoomVelocity = (_currentZoom - _previousZoom) / deltaTime;
        }

        _previousScreenPos = _currentScreenPos;
        _previousZoom = _currentZoom;
    }

    private static float GetDeltaTime() {
        double now = FrameClock.Elapsed.TotalSeconds;
        float delta = (float)(now - _lastFrameTime);
        _lastFrameTime = now;
        return MathF.Min(delta, 0.0003f);
    }

    private static Vector2 GetVanillaTranslation() {
        Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
        return Main.LocalPlayer.Center - screenSize / 2f + new Vector2(0, 9);
    }

    private static void BeginSegment(Vector2 from, Vector2 to, float fromZoom, float toZoom) {
        _startScreenPos = from;
        _startVelocity = _currentVelocity;
        _targetScreenPos = to;
        _startZoom = fromZoom;
        _startZoomVelocity = _currentZoomVelocity;
        _targetZoom = toZoom;

        float totalDist = Vector2.Distance(from, to);
        float distTiles = totalDist / 16f;
        float excess = MathF.Max(0f, distTiles - 50f);
        _duration = 0.6f + 0.05f * MathF.Pow(excess, 0.47f);
        Clock.Restart();
        
        // prevent overshoot
        if (_currentVelocity.Length() > totalDist / _duration * 1.5f)
            _startVelocity = Vector2.Normalize(_currentVelocity) * totalDist / _duration * 0.55f;
        float maxZoomVelocity = MathF.Abs(toZoom - fromZoom) / _duration * 1.5f;
        _startZoomVelocity = MathHelper.Clamp(_startZoomVelocity, -maxZoomVelocity, maxZoomVelocity);

        float speed = 1400f + 45f * MathF.Pow(distTiles, 1f);
        speed = Math.Min(speed, 7000f);
        float naturalTravel = speed * _duration;

        _useTeleportCut = naturalTravel < totalDist;

        if (_useTeleportCut) {
            Vector2 dir = totalDist > 0.0001f ? Vector2.Normalize(to - from) : Vector2.Zero;
            float segmentTime = _duration * 0.5f;
            float travel = Math.Min(speed * segmentTime, totalDist / 2f);
            _exitScreenPos = from + dir * travel;
            _entryScreenPos = to - dir * travel;
            _cutVelocity = dir * speed;
        }
    }

    public static void TransitionToPosition(Vector2 worldPos, float zoom) {
        Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
        BeginSegment(Main.screenPosition, worldPos - screenSize / 2f, _currentZoom, zoom);
        _state = CamState.TransitioningIn;
    }

    public static void TransitionToTileRect(TileBox rect) {
        Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
        Rectangle worldRect = rect.Scale(16);
        float zoomX = screenSize.X / worldRect.Width;
        float zoomY = screenSize.Y / worldRect.Height;
        float zoom = Math.Min(zoomX, zoomY);
        Vector2 rectCenterWorld = new(worldRect.X + worldRect.Width / 2f, worldRect.Y + worldRect.Height / 2f);
        TransitionToPosition(rectCenterWorld, zoom);
    }

    public static void TransitionToStructure(StructureRoot structure, int paddingTiles = -1) {
        int padding = paddingTiles == -1 ? 3 + (int)Math.Sqrt(Math.Max(structure.Tilemap.BoundingBox.Width, structure.Tilemap.BoundingBox.Height)) : paddingTiles;
        TransitionToTileRect(structure.Tilemap.BoundingBox.Inflate(padding));
    }

    public static void ReleaseCamera() {
        if (_state is CamState.Inactive) return;
        BeginSegment(Main.screenPosition, GetVanillaTranslation(), _currentZoom, _vanillaZoom);
        _state = CamState.TransitioningOut;
    }
}