using OpenTK.Mathematics;

/*
    An orbit camera that circles around a target point.
    Its position is stored as a distance and two angles (yaw and pitch) rather than
    x, y, z, which makes orbiting and zooming simple. The view matrix is rebuilt
    from these values each frame.
*/
public class Camera
{
    float yaw;
    float pitch;
    float sensitivity;

    // movement
    float moveSpeed;
    float moveSpeedDefault;
    float moveSpeedMultiplier = 3f;
    Vector3 forwardDirection;
    Vector3 rightDirection;
    Vector3 currentPosition;

    /*
        Sets the starting position, mouse sensitivity, zoom speed and zoom limits.
        The target defaults to (0, 0, 0), where the object is.
    */
    public Camera(float _sensitivity, float _moveSpeed, Vector3 _position)
    {
        currentPosition = _position;
        sensitivity = _sensitivity;
        moveSpeed = _moveSpeed;
        moveSpeedDefault = _moveSpeed;
        UpdateDirection(0f, 0f);
    }

    public void UpdateDirection(float dx, float dy)
    {
        yaw += dx * sensitivity;                                    // calculate yaw from mouse delta X
        pitch -= dy * sensitivity;                                  // calculate pitch from mouse delta Y
        pitch = MathHelper.Clamp(pitch, -89f, 89f);                 // clamp pitch (-89 - 89)

        float yawRad = MathHelper.DegreesToRadians(yaw);            // convert yaw/pitch to degrees
        float pitchRad = MathHelper.DegreesToRadians(pitch);

        forwardDirection = Vector3.Normalize(new Vector3(           // determine current forward and right directions (direction camera is looking)
            MathF.Cos(yawRad) * MathF.Cos(pitchRad),
            MathF.Sin(pitchRad),
            MathF.Sin(yawRad) * MathF.Cos(pitchRad)));

        rightDirection = Vector3.Normalize(Vector3.Cross(forwardDirection, Vector3.UnitY));
    }

    public void UpdateVectors(float _forwardDirection, float _rightDirection, float _upDirection, double deltaTime)
    {
        Vector3 direction = new Vector3(                                        // build single vector from forward, right and up vectors given from input and from camera direction
            forwardDirection * _forwardDirection + 
            rightDirection * _rightDirection + 
            Vector3.UnitY * _upDirection);

        currentPosition += direction * moveSpeed * (float)deltaTime;            // update the new position with the new direction vector
    }

    public Matrix4 UpdateViewMatrix()
    {
        return Matrix4.LookAt(currentPosition, currentPosition + forwardDirection, Vector3.UnitY);
    }

    public void UpdateMoveSpeed(bool increasedMoveSpeed)
    {
        moveSpeed = increasedMoveSpeed ? moveSpeedDefault * moveSpeedMultiplier : moveSpeedDefault;
    }

    public Vector3 GetPosition()
    {
        return currentPosition;
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
