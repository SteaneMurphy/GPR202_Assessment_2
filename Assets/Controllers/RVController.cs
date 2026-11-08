using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class RVController
{
    Dictionary<string, ObjectNode> wheels = new Dictionary<string, ObjectNode>();
    ObjectNode rootNode = null!;
    float rotationSpeed = 50f;
    float steerSpeed = 30f;
    float steerAngle = 0f;
    float maxSteerAngle = 30f;
    float wheelSkewY = -5.2f;   // model's built-in yaw, flip sign if the spin looks worse

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

    // AI help with fixing the wheel origin
    public void FixWheelOrigin(ObjectNode wheelRef)
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

    public void UpdateInput(KeyboardState keyboardState, float deltaTime)
    {
        if (keyboardState.IsKeyDown(Keys.R)) RotateWheels((float)deltaTime);
        if (keyboardState.IsKeyDown(Keys.Left)) SteerWheels(Keys.Left, (float)deltaTime);
        if (keyboardState.IsKeyDown(Keys.Right)) SteerWheels(Keys.Right, (float)deltaTime);
        if (keyboardState.IsKeyDown(Keys.Up)) MoveRV(Keys.Up, (float)deltaTime);
        if (keyboardState.IsKeyDown(Keys.Down)) MoveRV(Keys.Down, (float)deltaTime);
    }

    public void RotateWheels(float deltaTime)
    {
        foreach(ObjectNode wheel in wheels.Values)
        {
            wheel.rotation.X += rotationSpeed * deltaTime;
        }
    }

    public void SteerWheels(Keys key, float deltaTime)
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
    public void MoveRV(Keys key, float deltaTime)
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

        // turn by the steering angle, only while moving (full lock = 30 degrees per second)
        rootNode.rotation.Y += steerAngle * direction * deltaTime;

        // forward direction from the root's yaw (+Z when yaw is 0)
        float yaw = MathHelper.DegreesToRadians(rootNode.rotation.Y);
        Vector3 forward = new Vector3(MathF.Sin(yaw), 0f, MathF.Cos(yaw));

        rootNode.position += forward * direction * moveSpeed * deltaTime;

        // spin the wheels with the movement (negative time spins them backwards when reversing)
        RotateWheels(direction * deltaTime);
    }
}
