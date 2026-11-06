using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class RVController
{
    Dictionary<string, ObjectNode> wheels = new Dictionary<string, ObjectNode>();
    float rotationSpeed = 50f;
    float steerSpeed = 30f;
    float steerAngle = 0f;
    float maxSteerAngle = 30f;
    float wheelSkewY = -5.2f;   // model's built-in yaw, flip sign if the spin looks worse

    public void SetWheelReference(ObjectNode wheelRef, MaterialPart material)
    {
        switch(material.name)
        {
            case "Cylinder.005_wheel_front_right":
                wheels["frontRight"] = wheelRef;
                break;
            case "Cylinder.005_wheel_front_left":
                wheels["frontLeft"] = wheelRef;
                break;
            case "Cylinder.005_wheel_back_right":
                wheels["backRight"] = wheelRef;
                break;
            case "Cylinder.005_wheel_back_left":
                wheels["backLeft"] = wheelRef;
                break;
            default:
                break;
        }

        FixWheelOrigin(wheelRef, material);
    }

    // AI help with fixing the wheel origin
    public void FixWheelOrigin(ObjectNode wheelRef, MaterialPart material)
    {
        List<float> v = material.vertices;

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
}
