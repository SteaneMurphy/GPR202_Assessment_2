using OpenTK.Mathematics;

/*
    A free-fly camera. The mouse turns it (yaw and pitch) and the keys move it
    relative to where it is looking. Its position is stored as x, y, z, and the
    view matrix is rebuilt from the position and look direction each frame.
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
        Sets the starting position, mouse sensitivity and move speed.
        Yaw and pitch start at 0, so the camera starts looking along +X.
    */
    public Camera(float _sensitivity, float _moveSpeed, Vector3 _position)
    {
        currentPosition = _position;
        sensitivity = _sensitivity;
        moveSpeed = _moveSpeed;
        moveSpeedDefault = _moveSpeed;
        UpdateDirection(0f, 0f);
    }

    /*
        Turns the camera from the mouse movement, then works out the forward direction
        from yaw and pitch (spherical coordinates). Pitch is clamped just short of 90
        so the camera can't flip over the top.
    */
    public void UpdateDirection(float dx, float dy)
    {
        yaw += dx * sensitivity;                                    // calculate yaw from mouse delta X
        pitch -= dy * sensitivity;                                  // calculate pitch from mouse delta Y
        pitch = MathHelper.Clamp(pitch, -89f, 89f);                 // clamp pitch (-89 - 89)

        float yawRad = MathHelper.DegreesToRadians(yaw);            // convert yaw/pitch to radians
        float pitchRad = MathHelper.DegreesToRadians(pitch);

        forwardDirection = Vector3.Normalize(new Vector3(           // determine current forward and right directions (direction camera is looking)
            MathF.Cos(yawRad) * MathF.Cos(pitchRad),
            MathF.Sin(pitchRad),
            MathF.Sin(yawRad) * MathF.Cos(pitchRad)));

        rightDirection = Vector3.Normalize(Vector3.Cross(forwardDirection, Vector3.UnitY));
    }

    /*
        Moves the camera from the key input (-1, 0 or 1 on each axis). Forward and right
        follow where the camera is looking, up is always straight up (world Y).
    */
    public void UpdateVectors(float _forwardDirection, float _rightDirection, float _upDirection, double deltaTime)
    {
        Vector3 direction = new Vector3(                                        // build single vector from forward, right and up vectors given from input and from camera direction
            forwardDirection * _forwardDirection + 
            rightDirection * _rightDirection + 
            Vector3.UnitY * _upDirection);

        currentPosition += direction * moveSpeed * (float)deltaTime;            // update the new position with the new direction vector
    }

    // Builds the view matrix looking from the camera's position along its forward direction.
    public Matrix4 UpdateViewMatrix()
    {
        return Matrix4.LookAt(currentPosition, currentPosition + forwardDirection, Vector3.UnitY);
    }

    // Multiplies the move speed while the sprint key (Left Shift) is held.
    public void UpdateMoveSpeed(bool increasedMoveSpeed)
    {
        moveSpeed = increasedMoveSpeed ? moveSpeedDefault * moveSpeedMultiplier : moveSpeedDefault;
    }

    // Used by the main window to send viewPos to the shader for specular lighting.
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
