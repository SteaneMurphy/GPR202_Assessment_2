using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Globalization;
using System.Drawing;

public class MaterialPart
{
    public string Name = "";
    public string Material = "";
    public Color colour;
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
                    currentMaterial.Name = splits[1];
                    materials.Add(currentMaterial);
                    break;
                case "usemtl":                                                                                                  // material for current object, should be in order
                    currentMaterial.Material = splits[1];
                    break;
                default:
                    
                    break;
            }
        }
        return materials.ToArray();
    }

    // private static Dictionary<string, MaterialPart> ParseMaterial(string path)
    // {
    //     MaterialPart currentMaterial = null!;
    //     Dictionary<string, MaterialPart> materialData = new Dictionary<string, MaterialPart>();

    //     foreach(string line in File.ReadLines(path))
    //     {
    //         string[] splits = line.Split(' ');

    //         switch(splits[0])
    //         {
    //             case "newmtl":
    //                 currentMaterial = new MaterialPart();
    //                 materialData[splits[1]] = currentMaterial;
    //                 break;
    //             case "Kd":
    //                 currentMaterial.colour = new Vector3(ParseFloat(splits[1]), ParseFloat(splits[2]), ParseFloat(splits[3]));
    //                 break;
    //             case "map_kd":
    //                 break;
    //         }
    //     }
    // }

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
}