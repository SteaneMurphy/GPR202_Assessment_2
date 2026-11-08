using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

/*
    This class manages the cubemap texture for the skybox.

    It takes 6 image paths (one per face, in the order +X, -X, +Y, -Y, +Z, -Z), creates a single
    cubemap texture with GL.GenTexture and sets its filtering/wrapping, then loads each image and
    uploads it to its face with GL.TexImage2D.

    UseTextures binds the cubemap to texture unit 0 for drawing.
    Cleanup is called by the Skybox class.
*/
public class CubemapTexture
{
    private string[] paths;
    int textureInstance;

    public CubemapTexture(string[] _paths)
    {
        paths = _paths;
        CreateCubemap();
        LoadTextures();
    }

    private void LoadTextures()
    {
        for(int i = 0; i < paths.Length; i++)
        {
            ImageResult image;
            if(!File.Exists(paths[i]))
            {
                Console.WriteLine($"Error: Texture file not found: {paths[i]}");
                return;
            }
            try
            {
                StbImage.stbi_set_flip_vertically_on_load(0);                                       // No flip on skybox textures
                using Stream stream = File.OpenRead(paths[i]);
                image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                GL.TexImage2D(
                    TextureTarget.TextureCubeMapPositiveX + i,                                      // each face is consecutive
                    0, 
                    PixelInternalFormat.Rgba, 
                    image.Width, 
                    image.Height, 
                    0, 
                    PixelFormat.Rgba, 
                    PixelType.UnsignedByte, 
                    image.Data
                );
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error: Could not load texture: {paths[i]}, {e.Message}");           
            }   
        }
    }

    /*
        Generates the cubemap texture and binds it so the faces can be uploaded to it.

        Linear filtering blends neighbouring pixels so the sky looks smooth when shrunk (min) or stretched (mag).
        ClampToEdge on S, T and R (the cubemap's 3 axes) stops the edge pixels wrapping around,
        which would otherwise show visible seams where the faces meet.
    */
    private void CreateCubemap()
    {
        textureInstance = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureCubeMap, textureInstance);

        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
    }

    public void UseTextures()
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.TextureCubeMap, textureInstance);
    }

    public void Cleanup()
    {
        GL.DeleteTexture(textureInstance);
    }
}

/*
    References

    de Vries, J. (n.d.). Cubemaps. LearnOpenGL.
        https://learnopengl.com/Advanced-OpenGL/Cubemaps

    Khronos Group. (n.d.-a). glActiveTexture [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glActiveTexture.xhtml

    Khronos Group. (n.d.-b). glBindTexture [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glBindTexture.xhtml

    Khronos Group. (n.d.-c). glDeleteTextures [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glDeleteTextures.xhtml

    Khronos Group. (n.d.-d). glGenTextures [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glGenTextures.xhtml

    Khronos Group. (n.d.-e). glTexImage2D [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glTexImage2D.xhtml

    Khronos Group. (n.d.-f). glTexParameter [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glTexParameter.xhtml

    StbSharp. (n.d.). StbImageSharp [Computer software]. GitHub.
        https://github.com/StbSharp/StbImageSharp
*/
