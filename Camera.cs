using OpenTK.Mathematics;

/*
    An orbit camera that circles around a target point.
    Its position is stored as a distance and two angles (yaw and pitch) rather than
    x, y, z, which makes orbiting and zooming simple. The view matrix is rebuilt
    from these values each frame.
*/
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

    /*
        Sets the starting position, mouse sensitivity, zoom speed and zoom limits.
        The target defaults to (0, 0, 0), where the object is.
    */
    public Camera(float _distance, float _yaw, float _pitch, float _sensitivity, float _zoomSpeed, float _minDistance, float _maxDistance)
    {
        SetPosition(_distance, _yaw, _pitch);
        sensitivity = _sensitivity;
        zoomSpeed = _zoomSpeed;
        minDistance = _minDistance;
        maxDistance = _maxDistance;
    }

    /*
        Moves the camera straight to a given distance and angle (in degrees).
    */
    public void SetPosition(float _distance, float _yaw, float _pitch)
    {
        distance = _distance;
        yaw = _yaw;
        pitch = _pitch;
    }

    /*
        Changes the point the camera looks at and orbits around.
    */
    public void SetTarget(Vector3 _target)
    {
        target = _target;
    }

    /*
        Orbits the camera using how far the mouse moved this frame.
        Pitch is clamped just under 90 degrees so the camera can't flip over the top,
        where LookAt would break.
    */
    public void RotateCamera(float mouseDeltaX, float mouseDeltaY)
    {
        yaw += mouseDeltaX * sensitivity;
        pitch += mouseDeltaY * sensitivity;
        pitch = MathHelper.Clamp(pitch, -89f, 89f);
    }

    /*
        Moves the camera closer or further away using the scroll wheel,
        kept between the min and max distance.
    */
    public void ZoomCamera(float scrollDelta)
    {
        distance -= scrollDelta * zoomSpeed;
        distance = MathHelper.Clamp(distance, minDistance, maxDistance);
    }

    /*
        Converts distance, yaw and pitch into an x, y, z position.
        cos(pitch) shrinks the circle as the camera looks further up or down,
        sin(pitch) sets the height, and yaw picks the point around the circle.
        The target is added so the camera orbits the target rather than the origin.
    */
    public Vector3 GetPosition()
    {
        float posX = (float)(distance * MathHelper.Cos(MathHelper.DegreesToRadians(pitch)) * MathHelper.Sin(MathHelper.DegreesToRadians(yaw)));
        float posY = (float)(distance * MathHelper.Sin(MathHelper.DegreesToRadians(pitch)));
        float posZ = (float)(distance * MathHelper.Cos(MathHelper.DegreesToRadians(pitch)) * MathHelper.Cos(MathHelper.DegreesToRadians(yaw)));

        return new Vector3(posX, posY, posZ) + target;
    }

    /*
        Builds the view matrix looking from the camera's position at the target,
        with Y as up.
    */
    public Matrix4 CalculateViewMatrix()
    {
        return Matrix4.LookAt(GetPosition(), target, Vector3.UnitY);
    }
}

/*
    References

    de Vries, J. (n.d.). Camera. LearnOpenGL.
        https://learnopengl.com/Getting-started/Camera

    OpenTK. (n.d.). Camera. OpenTK.
        https://opentk.net/learn/chapter1/9-camera.html

    Weisstein, E. W. (n.d.). Spherical coordinates. MathWorld--A Wolfram Web Resource.
        https://mathworld.wolfram.com/SphericalCoordinates.html
*/
