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
uniform vec3 lightDir;              // vector direction of directional scene light
uniform vec3 viewPos;               // camera view position

in vec3 vNorm;                      // normals sent to frag
in vec2 vUV;                        // UVs sent to frag
in vec3 vWorldPos;                  // pixel world position sent to frag

out vec4 fragColour;                // pixel colour out

void main()
{
    vec3 unitNormal = normalize(vNorm);
    vec3 unitLight = normalize(-lightDir);              // reversed sun -> object
    float dotProduct = max(dot(unitNormal, unitLight), 0.0);

    vec3 viewVector = normalize(viewPos - vWorldPos);              // pixel -> camera
    vec3 reflectVector = reflect(-unitLight, unitNormal);           // sunlight bounced off the surface
    float specular = pow(max(dot(viewVector, reflectVector), 0.0), 32.0) * 0.3;

    vec3 base = useTexture ? texture(tex, vUV).rgb * colour : colour;
    fragColour = vec4(base * (0.2 + dotProduct) + specular, 1.0);
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
