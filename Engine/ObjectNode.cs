using OpenTK.Mathematics;

/*
    One node in the scene tree. Each node has its own position, rotation and scale
    relative to its parent, an optional mesh, and a list of child nodes.

    Moving a parent moves all of its children with it (e.g. moving the RV's root
    moves the body and wheels). Rendering, building and cleanup all start at the
    scene root and work down the tree.
*/
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

    /*
        Builds this node's matrix from its position, rotation (in degrees) and scale.
        OpenTK applies matrices left to right, so s * r * t scales first, then rotates,
        then translates. Translating last stops the movement itself being rotated.
    */
    private Matrix4 CalculateLocalMatrix()
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

    /*
        Draws this node, then its children. The node's local matrix is combined with
        its parent's world matrix, and the result is passed down so each child is
        placed relative to this node. Uses the part's texture if it has one,
        otherwise just its material colour.
    */
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

    // Frees this node's mesh from the GPU, then every child's. Called on the scene root from the main window.
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

/*
    References

    de Vries, J. (n.d.). Transformations. LearnOpenGL.
        https://learnopengl.com/Getting-started/Transformations

    Jonathan, S. (2021, September). Scene graph. LearnOpenGL.
        https://learnopengl.com/Guest-Articles/2021/Scene/Scene-Graph

    OpenTK. (n.d.). Matrix4 [API reference]. OpenTK.
        https://opentk.net/api/OpenTK.Mathematics.Matrix4.html
*/