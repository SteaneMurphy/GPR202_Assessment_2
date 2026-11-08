using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

/*
    Controls the RV model's behaviour: driving, steering and spinning the wheels.

    The model loader and ObjectNode stay generic. This controller finds the RV's nodes by
    name after loading and only changes their position and rotation, which ObjectNode
    then uses when rendering.

    UpdateInput is called every frame from the main window.
*/
public class RVController
{
    Dictionary<string, ObjectNode> wheels = new Dictionary<string, ObjectNode>();
    ObjectNode rootNode = null!;                                // the whole RV, moved when driving

    // wheels
    float rotationSpeed = 50f;                                  // spin, degrees per second
    float steerSpeed = 30f;                                     // degrees per second
    float steerAngle = 0f;                                      // current steer, + is left
    float maxSteerAngle = 30f;
    float wheelSkewY = -5.2f;                                   // model's built-in yaw (error within 3D model during modelling)

    /*
        Finds the RV's wheel nodes in the loaded tree and fixes their origins.
        Must run before root.BuildMeshes(), because it edits the vertex data.
    */
    public void Setup(ObjectNode root)
    {
        rootNode = root;

        foreach (ObjectNode node in root.childNodes)
        {
            switch(node.name)
            {
                case "Cylinder.005_wheel_front_right":
                    wheels["frontRight"] = node;
                    break;
                case "Cylinder.005_wheel_front_left":
                    wheels["frontLeft"] = node;
                    break;
                case "Cylinder.005_wheel_back_right":
                    wheels["backRight"] = node;
                    break;
                case "Cylinder.005_wheel_back_left":
                    wheels["backLeft"] = node;
                    break;
                default:
                    continue;                                   // not a wheel
            }

            FixWheelOrigin(node);
        }
    }

    /*
        The wheel's vertices are exported in RV space, so its origin is not at its centre
        and rotating it would swing it around the RV instead of spinning on its axle.
        This moves the vertices so the wheel's centre is its origin, straightens out the
        model's yaw, then places the node back where the wheel was.

        AI help with fixing the wheel origin
    */
    private void FixWheelOrigin(ObjectNode wheelRef)
    {
        List<float> v = wheelRef.part!.vertices;

        // find the wheel's bounds (8 floats per vertex: position, normal, uv)
        Vector3 min = new Vector3(v[0], v[1], v[2]);
        Vector3 max = min;

        for (int i = 0; i < v.Count; i += 8)
        {
            Vector3 position = new Vector3(v[i], v[i + 1], v[i + 2]);
            min = Vector3.ComponentMin(min, position);
            max = Vector3.ComponentMax(max, position);
        }

        Vector3 centre = (min + max) / 2;

        // undo the model's yaw so the axle lines up with local X
        Matrix4 straighten = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(-wheelSkewY));

        for (int i = 0; i < v.Count; i += 8)
        {
            // centre the wheel on its own origin, then straighten it
            Vector3 position = new Vector3(v[i], v[i + 1], v[i + 2]) - centre;
            Vector3 normal = new Vector3(v[i + 3], v[i + 4], v[i + 5]);

            position = Vector3.TransformPosition(position, straighten);
            normal = Vector3.TransformNormal(normal, straighten);

            v[i]     = position.X;
            v[i + 1] = position.Y;
            v[i + 2] = position.Z;
            v[i + 3] = normal.X;
            v[i + 4] = normal.Y;
            v[i + 5] = normal.Z;
        }

        // move the node back to the axle and re-apply the yaw.
        // rotation order is X then Y, so the wheel spins on its straight axle before being turned back
        wheelRef.position = centre;
        wheelRef.rotation.Y = wheelSkewY;
    }

    /*
        Checks the RV's keys each frame and calls the matching action.
        R spins the wheels on the spot, Left/Right steer, Up/Down drive forward and reverse.
    */
    public void UpdateInput(KeyboardState keyboardState, float deltaTime)
    {
        if (keyboardState.IsKeyDown(Keys.R)) RotateWheels((float)deltaTime);
        if (keyboardState.IsKeyDown(Keys.Left)) SteerWheels(Keys.Left, (float)deltaTime);
        if (keyboardState.IsKeyDown(Keys.Right)) SteerWheels(Keys.Right, (float)deltaTime);
        if (keyboardState.IsKeyDown(Keys.Up)) MoveRV(Keys.Up, (float)deltaTime);
        if (keyboardState.IsKeyDown(Keys.Down)) MoveRV(Keys.Down, (float)deltaTime);
    }

    // Spins all four wheels around their axle (local X).
    private void RotateWheels(float deltaTime)
    {
        foreach(ObjectNode wheel in wheels.Values)
        {
            wheel.rotation.X += rotationSpeed * deltaTime;
        }
    }

    /*
        Turns the front wheels left or right, clamped to the max steer angle.
        The angle is added on top of the model's built-in yaw so the wheels stay lined up.
        steerAngle is also used by MoveRV to turn the RV while driving.
    */
    private void SteerWheels(Keys key, float deltaTime)
    {
        

        switch (key)
        {
            case Keys.Left:
            steerAngle += steerSpeed * deltaTime;
                break;
            case Keys.Right:
            steerAngle -= steerSpeed * deltaTime;
                break;
            default:
                break;
        }

        steerAngle = MathHelper.Clamp(steerAngle, -maxSteerAngle, maxSteerAngle);

        wheels["frontRight"].rotation.Y = wheelSkewY + steerAngle;
        wheels["frontLeft"].rotation.Y = wheelSkewY + steerAngle;
    }

    /*
        Drives the whole RV by moving its root node. Every child (body, wheels) follows,
        and the wheels keep their own spin and steer on top.
    */
    private void MoveRV(Keys key, float deltaTime)
    {
        float moveSpeed = 5f;                                       // units per second
        float direction = 0f;                                       // +1 forward, -1 reverse

        switch(key)
        {
            case Keys.Up:
                direction = 1f;
                break;
            case Keys.Down:
                direction = -1f;
                break;
            default:
                break;
        }

        rootNode.rotation.Y += steerAngle * direction * deltaTime;

        float yaw = MathHelper.DegreesToRadians(rootNode.rotation.Y);
        Vector3 forward = new Vector3(MathF.Sin(yaw), 0f, MathF.Cos(yaw));

        rootNode.position += forward * direction * moveSpeed * deltaTime;           // move the RV node by direction

        RotateWheels(direction * deltaTime);                                        // spin the wheels when moving (-direction is reverse)
    }
}

/*
    References

    Anthropic. (2026). Claude [Large language model]. https://claude.ai

    de Vries, J. (n.d.). Transformations. LearnOpenGL.
        https://learnopengl.com/Getting-started/Transformations

    Jonathan, S. (2021, September). Scene graph. LearnOpenGL.
        https://learnopengl.com/Guest-Articles/2021/Scene/Scene-Graph

    Monster, M. (2003). Car physics for games.
        https://asawicki.info/Mirror/Car%20Physics%20for%20Games/Car%20Physics%20for%20Games.html

    OpenTK. (n.d.-a). KeyboardState [API reference]. OpenTK.
        https://opentk.net/api/OpenTK.Windowing.GraphicsLibraryFramework.KeyboardState.html

    OpenTK. (n.d.-b). Matrix4 [API reference]. OpenTK.
        https://opentk.net/api/OpenTK.Mathematics.Matrix4.html

    OpenTK. (n.d.-c). Vector3 [API reference]. OpenTK.
        https://opentk.net/api/OpenTK.Mathematics.Vector3.html

    VectorMan. (2020, July 26). Breaking Bad 1986 Fleetwood Bounder RV [3D model]. Sketchfab.
        https://sketchfab.com/3d-models/breaking-bad-1986-fleetwood-bounder-rv-28006272fe514cb68e3a0ac2125734aa

    Weisstein, E. W. (n.d.). Polar coordinates. MathWorld--A Wolfram Web Resource.
        https://mathworld.wolfram.com/PolarCoordinates.html
*/
