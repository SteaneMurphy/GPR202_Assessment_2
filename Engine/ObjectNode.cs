using OpenTK.Mathematics;

public class ObjectNode
{
    public string name;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
    public List<ObjectNode> childNodes;
    public Mesh? mesh;
    public MaterialPart? part;                      // vertex data + material   

    public ObjectNode(string _name)
    {
        name = _name;
        scale = Vector3.One;
        position = new Vector3(0f, 0f, 0f);
        rotation = new Vector3(0f, 0f, 0f);
        childNodes = new List<ObjectNode>();
    }

    /*
        Adds one child node per loaded part. Nothing is uploaded yet, so controllers
        can still edit the vertex data (e.g. re-centre a wheel) before BuildMeshes.
    */
    public void AddParts(MaterialPart[] parts)
    {
        foreach (MaterialPart part in parts)
        {
            ObjectNode node = new ObjectNode(part.name);
            node.part = part;
            childNodes.Add(node);
        }
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

    /*
        Uploads this node's vertex data to the GPU, then does the same for every child.
        Called once, after any controller has finished editing the vertex data.
    */
    public void BuildMeshes()
    {
        if (part != null)
        {
            mesh = new Mesh(part.vertices.ToArray(), part.indices.ToArray());
            mesh.BuildMesh();
        }

        foreach (ObjectNode node in childNodes)
        {
            node.BuildMeshes();
        }
    }

    public void RenderNode(Dictionary<string, Texture> textures, ShaderPipeline _current, Matrix4 parentWorldSpace)
    {
        Matrix4 localToWorldSpace = CalculateLocalMatrix() * parentWorldSpace;

        if (mesh != null && part != null)
        {
            if (part.materialPath != "")
            {
                textures[part.materialPath].UseTexture();
                _current.SetBool("useTexture", true);
            }
            else
            {
                _current.SetBool("useTexture", false);
            }

            _current.SetVector3("colour", part.colour);
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