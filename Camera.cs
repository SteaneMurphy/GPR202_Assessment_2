using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Camera
{
    Vector3 target;
    float distance;
    float yaw;
    float pitch;
    float sensitivity;
    float minDistance;
    float maxDistance;
    float zoomSpeed;

    public Camera(float _distance, float _yaw, float _pitch, float _sensitivity, float _zoomSpeed, float _minDistance, float _maxDistance)
    {
        SetPosition(_distance, _yaw, _pitch);
        sensitivity = _sensitivity;
        zoomSpeed = _zoomSpeed;
        minDistance = _minDistance;
        maxDistance = _maxDistance;
    }

    public void SetPosition(float _distance, float _yaw, float _pitch)
    {
        distance = _distance;
        yaw = _yaw;
        pitch = _pitch;
    }

    public void SetTarget(Vector3 _target)
    {
        target = _target;
    }

    public void RotateCamera(float mouseDeltaX, float mouseDeltaY)
    {
        yaw += mouseDeltaX * sensitivity;
        pitch += mouseDeltaY * sensitivity;
        pitch = MathHelper.Clamp(pitch, -89f, 89f);
    }

    public void ZoomCamera(float scrollDelta)
    {
        distance -= scrollDelta * zoomSpeed;
        distance = MathHelper.Clamp(distance, minDistance, maxDistance);
    }

    public Vector3 GetPosition()
    {
        float posX = (float)(distance * MathHelper.Cos(MathHelper.DegreesToRadians(pitch)) * MathHelper.Sin(MathHelper.DegreesToRadians(yaw)));
        float posY = (float)(distance * MathHelper.Sin(MathHelper.DegreesToRadians(pitch)));
        float posZ = (float)(distance * MathHelper.Cos(MathHelper.DegreesToRadians(pitch)) * MathHelper.Cos(MathHelper.DegreesToRadians(yaw)));

        return new Vector3(posX, posY, posZ) + target;
    }

    public Matrix4 CalculateViewMatrix()
    {
        return Matrix4.LookAt(GetPosition(), target, Vector3.UnitY);
    }
}