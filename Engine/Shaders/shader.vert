#version 330 core

/*
    This shader takes in an 8-float vertex: position, normal and UV.

    It moves the position into world space with the model matrix, then into
    clip space with view and projection (gl_Position). The world position,
    rotated normal and UV are passed to the frag shader for lighting and texturing.

    OpenTK matrices are row-major, so the vector goes on the left: v * model * view * projection.
*/

layout (location = 0) in vec3 aPos;             // vertices position (vector3)
layout (location = 1) in vec3 aNorm;            // vertices normal (vector3)
layout (location = 2) in vec2 aUV;              // vertices UV (vector2)

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 vNorm;                                 // normal vertices out to frag
out vec2 vUV;                                   // UV vertices out to frag
out vec3 vWorldPos;                             // where this pixel is on the mesh in world space

void main()
{
    vec4 worldPos = vec4(aPos, 1.0) * model;    // model -> world space (kept separate for lighting)
    gl_Position = worldPos * view * projection; // world -> clip space
    vNorm = aNorm * mat3(model);                // rotate the normal with the model (mat3 drops translation; fine while scale is uniform)
    vUV = aUV;                                  // assigns the UVs out
    vWorldPos = worldPos.xyz;
}

/*
    References

    de Vries, J. (n.d.-a). Basic lighting. LearnOpenGL.
        https://learnopengl.com/Lighting/Basic-Lighting

    de Vries, J. (n.d.-b). Coordinate systems. LearnOpenGL.
        https://learnopengl.com/Getting-started/Coordinate-Systems

    de Vries, J. (n.d.-c). Shaders. LearnOpenGL.
        https://learnopengl.com/Getting-started/Shaders

    Khronos Group. (n.d.). gl_Position [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/gl_Position.xhtml
*/
