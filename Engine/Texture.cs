using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

/*
    This class loads an image file from disk and uploads it to the GPU as an
    OpenGL 2D texture, ready to be sampled by the fragment shader.
*/
public class Texture
{
    string path;
    ImageResult image = null!;                                                                    // stays null if the image fails to load
    int textureInstance;

    /*
        Stores the image path, then loads the image and creates the texture.
    */
    public Texture(string _path)
    {
        path = _path;
        LoadImage();
        CreateTexture();
    }

    /*
        Reads the image file and decodes it into raw RGBA pixel data using StbImageSharp.
        The image is flipped because OpenGL expects the first row to be the bottom.
    */
    private void LoadImage()
    {
        if(!File.Exists(path))
        {
            Console.WriteLine($"Error: Texture file not found: {path}");
            return;
        }

        try
        {
            StbImage.stbi_set_flip_vertically_on_load(1);                                       // OpenGL reads bottom-up
            using Stream stream = File.OpenRead(path);
            image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        }
        catch(Exception e)
        {
            Console.WriteLine($"Error: Could not load texture: {path}, {e.Message}");           // e.g. corrupt or unsupported file
        }
    }

    /*
        Creates the texture on the GPU and uploads the pixel data. Wrapping is set to
        repeat so UVs above 1 tile the image, and mipmaps are generated so the texture
        stays smooth at a distance.
    */
    private void CreateTexture()
    {
        if(image == null)                                                                       // nothing to upload if the image failed to load
        {
            return;
        }

        textureInstance = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureInstance);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }

    /*
        Binds this texture to texture unit 0, which the fragment shader's sampler reads
        from. Called before drawing.
    */
    public void UseTexture()
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, textureInstance);
    }

    /*
        Deletes the texture from the GPU. Called when the window closes.
    */
    public void Cleanup()
    {
        GL.DeleteTexture(textureInstance);
    }
}

/*
    References

    de Vries, J. (n.d.). Textures. LearnOpenGL.
        https://learnopengl.com/Getting-started/Textures

    Khronos Group. (n.d.-a). glActiveTexture [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glActiveTexture.xhtml

    Khronos Group. (n.d.-b). glBindTexture [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glBindTexture.xhtml

    Khronos Group. (n.d.-c). glDeleteTextures [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glDeleteTextures.xhtml

    Khronos Group. (n.d.-d). glGenerateMipmap [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glGenerateMipmap.xhtml

    Khronos Group. (n.d.-e). glGenTextures [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glGenTextures.xhtml

    Khronos Group. (n.d.-f). glTexImage2D [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glTexImage2D.xhtml

    Khronos Group. (n.d.-g). glTexParameter [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glTexParameter.xhtml

    StbSharp. (n.d.). StbImageSharp [Computer software]. GitHub.
        https://github.com/StbSharp/StbImageSharp
*/
