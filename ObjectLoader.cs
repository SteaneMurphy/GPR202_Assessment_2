using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Globalization;
using System.Drawing;

public class MaterialPart
{
    public string name = "";
    public string material = "";
    public string materialPath = "";
    public Vector3 colour;
    public List<float> vertices = new List<float>();
    public List<uint> indices = new List<uint>();
}

/*
    Really bad logic here. If obj file lists split identifiers out of order, no materialpart obj exists to write into. Rewrite with defence in mind.
*/
public class ObjectLoader
{
    public static MaterialPart[] ParseObjectData(string path)
    {
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        List<MaterialPart> materials = new List<MaterialPart>();
        Dictionary<string, MaterialPart> materialData = new Dictionary<string, MaterialPart>();                              // filled from the .mtl when the mtllib line is read
        string folder = Path.GetDirectoryName(path)!;                                                                         // mtl and texture names are relative to the obj's folder

        MaterialPart currentMaterial = null!;                                                                                // empty container for material, updated per obj foundw

        foreach (string line in File.ReadLines(path))
        {
            string[] splits = line.Split(' ');

            switch(splits[0])
            {
                case "v":                                                                                                       // position
                    positions.Add(new Vector3(ParseFloat(splits[1]), ParseFloat(splits[2]), ParseFloat(splits[3])));            
                    break;
                case "vn":                                                                                                      // normals
                    normals.Add(new Vector3(ParseFloat(splits[1]), ParseFloat(splits[2]), ParseFloat(splits[3])));              
                    break;
                case "vt":                                                                                                      // uvs
                    uvs.Add(new Vector2(ParseFloat(splits[1]), ParseFloat(splits[2])));                                         
                    break;
                case "f":                                                                                                       // faces
                    for (int i = 1; i <= 3; i++)
                    {
                        string[] corner = splits[i].Split('/');                  // "position/uv/normal"
                        Vector3 p = positions[int.Parse(corner[0]) - 1];         // OBJ counts from 1
                        Vector2 t = uvs[int.Parse(corner[1]) - 1];
                        Vector3 n = normals[int.Parse(corner[2]) - 1];

                        currentMaterial.vertices.AddRange([p.X, p.Y, p.Z, n.X, n.Y, n.Z, t.X, t.Y]);                                            // creates new vertice (8-float)
                        currentMaterial.indices.Add((uint)currentMaterial.indices.Count);                                                       // creates new index for the vertice    
                    }
                    break;
                case "o":                                                                                                       // object (ie; tyre, axle, door, rim, etc)
                    currentMaterial = new MaterialPart();
                    currentMaterial.name = splits[1];
                    materials.Add(currentMaterial);
                    break;
                case "mtllib":                                                                                                  // material file, listed before any objects
                    materialData = ParseMaterial(Path.Combine(folder, splits[1]));
                    break;
                case "usemtl":                                                                                                  // material for current object, should be in order
                    currentMaterial.material = splits[1];
                    currentMaterial.colour = materialData[splits[1]].colour;                                                    // copy the look from the mtl entry
                    currentMaterial.materialPath = materialData[splits[1]].materialPath;
                    break;
                default:
                    
                    break;
            }
        }

        Vector3 offset = CalculateObjectBounds(positions);
        
        foreach(MaterialPart material in materials)
        {
            for(int i = 0; i < material.vertices.Count; i += 8)
            {
                material.vertices[i] -= offset.X;
                material.vertices[i+1] -= offset.Y;
                material.vertices[i+2] -= offset.Z;
            }
        }

        return materials.ToArray();
    }

    public static Dictionary<string, MaterialPart> ParseMaterial(string path)
    {
        MaterialPart currentMaterial = null!;
        Dictionary<string, MaterialPart> materialData = new Dictionary<string, MaterialPart>();
        string folder = Path.GetDirectoryName(path)!;                                               // texture names are relative to the mtl's folder

        foreach(string line in File.ReadLines(path))
        {
            string[] splits = line.Split(' ');

            switch(splits[0])
            {
                case "newmtl":
                    currentMaterial = new MaterialPart();
                    materialData[splits[1]] = currentMaterial;
                    break;
                case "Kd":
                    currentMaterial.colour = new Vector3(ParseFloat(splits[1]), ParseFloat(splits[2]), ParseFloat(splits[3]));
                    break;
                case "map_Kd":
                    currentMaterial.materialPath = Path.Combine(folder, splits[1]);                 // e.g. models/BreakingBadVan/BlackMetal_baseColor.png
                    break;
            }
        }

        return materialData;
    }

    private static float ParseFloat(string input)
    {
        if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
        {
            return result;
        }
        else
        {
            Console.WriteLine("ERROR: ParseFloat - could not parse float, likely empty string");
            return 0f;
        }
    }

    private static Vector3 CalculateObjectBounds(List<Vector3> positions)
    {
        Vector3 minValues = positions[0];
        Vector3 maxValues = positions[0];

        foreach(Vector3 position in positions)
        {
            if(position.X < minValues.X) minValues.X = position.X;
            if(position.Y < minValues.Y) minValues.Y = position.Y;
            if(position.Z < minValues.Z) minValues.Z = position.Z;
            if(position.X > maxValues.X) maxValues.X = position.X;
            if(position.Y > maxValues.Y) maxValues.Y = position.Y;
            if(position.Z > maxValues.Z) maxValues.Z = position.Z;
        }

        Vector3 offset = Vector3.Zero;

        offset.X = (minValues.X + maxValues.X) / 2;
        offset.Y = minValues.Y;
        offset.Z = (minValues.Z + maxValues.Z) / 2;

        return offset;
    }
}