using OpenTK.Mathematics;

public class ObjectNode
{
    public string name;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
    public List<ObjectNode> childNodes;
    public Mesh? mesh;
    public string materialPath;
    public Vector3 colour;
    
    public ObjectNode(string _name)
    {
        name = _name;
        scale = Vector3.One;
        position = new Vector3(0f, 0f, 0f);
        rotation = new Vector3(0f, 0f, 0f);
        childNodes = new List<ObjectNode>();
    }

    public void SetParams(string _materialPath, Vector3 _colour, Mesh _mesh)
    {
        mesh = _mesh;
        materialPath = _materialPath;
        colour = _colour;
    }

    public Matrix4 CalculateLocalMatrix()
    {
        Matrix4 s = Matrix4.CreateScale(scale);
        Matrix4 r = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotation.X)) * 
        Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotation.Y)) * 
        Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation.Z));
        Matrix4 t = Matrix4.CreateTranslation(position);

        return s * r * t;
    }

    public void RenderNode(Dictionary<string, Texture> textures, ShaderPipeline _current, Matrix4 parentWorldSpace)
    {
        Matrix4 localToWorldSpace = CalculateLocalMatrix() * parentWorldSpace;

        if (mesh != null)
        {
            if (materialPath != "")
            {
                textures[materialPath].UseTexture();
                _current.SetBool("useTexture", true);
            }
            else
            {
                _current.SetBool("useTexture", false);
            }

            _current.SetVector3("colour", colour);
            _current.SetMatrix4("model", localToWorldSpace);
            mesh.DrawMesh();
        }

        foreach (ObjectNode node in childNodes)
        {
            node.RenderNode(textures, _current, localToWorldSpace);
        }
    }

    public void Cleanup()
    {
        if (mesh != null)
        {
            mesh.Cleanup();
        }

        foreach (ObjectNode node in childNodes)
        {
            node.Cleanup();
        }
    }
}