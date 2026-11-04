#version 330 core

/*
    This shader adjusts each pixel of the Torus. It outputs a final
    colour which is applied to the specific pixel.

    The shader takes in the texture reference and a textureMix value, which
    is a mix of how much colour is applied in place of the texture on the same
    pixel.
*/
uniform sampler2D tex;              // texture to be rendered
uniform vec3 colour;
uniform bool useTexture;

in vec3 vNorm;                      // normals sent to frag
in vec2 vUV;                        // UVs sent to frag

out vec4 fragColour;                // pixel colour out

void main()
{
    if (useTexture)
    {
        fragColour = texture(tex, vUV) * vec4(colour, 1.0);
    }
    else
    {
        fragColour = vec4(colour, 1.0);
    }
}

/*
    References

    de Vries, J. (n.d.-a). Shaders. LearnOpenGL.
        https://learnopengl.com/Getting-started/Shaders

    de Vries, J. (n.d.-b). Textures. LearnOpenGL.
        https://learnopengl.com/Getting-started/Textures

    Khronos Group. (n.d.-a). mix [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/mix.xhtml

    Khronos Group. (n.d.-b). texture [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/texture.xhtmlrt
*/
