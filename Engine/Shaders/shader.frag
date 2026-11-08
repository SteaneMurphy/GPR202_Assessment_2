#version 330 core

/*
    Works out the final colour of each pixel with Phong lighting from one
    directional light (the sun):

    - ambient: a fixed 0.2 so faces turned away from the sun aren't black
    - diffuse: brighter the more the surface faces the sun
    - specular: a highlight where the sun reflects towards the camera

    The base colour is the texture tinted by the material colour, or just the
    material colour if the part has no texture.
*/
uniform sampler2D tex;              // texture to be rendered
uniform vec3 colour;                // material colour (Kd from the .mtl)
uniform bool useTexture;            // false for parts with no texture
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
    float dotProduct = max(dot(unitNormal, unitLight), 0.0);       // diffuse, 0 when facing away

    vec3 viewVector = normalize(viewPos - vWorldPos);              // pixel -> camera
    vec3 reflectVector = reflect(-unitLight, unitNormal);           // sunlight bounced off the surface
    float specular = pow(max(dot(viewVector, reflectVector), 0.0), 32.0) * 0.3;     // shininess 32, strength 0.3

    vec3 base = useTexture ? texture(tex, vUV).rgb * colour : colour;
    fragColour = vec4(base * (0.2 + dotProduct) + specular, 1.0);                   // ambient + diffuse, then specular on top
}

/*
    References

    de Vries, J. (n.d.-a). Basic lighting. LearnOpenGL.
        https://learnopengl.com/Lighting/Basic-Lighting

    de Vries, J. (n.d.-b). Light casters. LearnOpenGL.
        https://learnopengl.com/Lighting/Light-casters

    de Vries, J. (n.d.-c). Shaders. LearnOpenGL.
        https://learnopengl.com/Getting-started/Shaders

    de Vries, J. (n.d.-d). Textures. LearnOpenGL.
        https://learnopengl.com/Getting-started/Textures

    Khronos Group. (n.d.-a). reflect [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/reflect.xhtml

    Khronos Group. (n.d.-b). texture [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/texture.xhtml
*/
