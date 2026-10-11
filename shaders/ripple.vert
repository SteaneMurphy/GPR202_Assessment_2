#version 330 core

/*
    This shader takes in a 8-float vertex whereby the values are
    split into vertex position, colour and UV coordinates.

    It provides a matrix calculation on the position vertices using the
    pre-calculated "mvp" and outputs the colour/UVs to the frag shader.

    gl_Position results in clip space.

    ripplePos adjusts each vertex's y position using a sine wave against
    its x position and time, resulting in changing vertex positions.
*/

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aCol;
layout (location = 2) in vec2 aUV;
out vec3 vCol;
out vec2 vUV;
uniform mat4 mvp;
uniform float time;

void main()
{
    vec3 ripplePos = aPos;
    ripplePos.y += 0.12 * sin(ripplePos.x * 8.0 + time * 6.0);              // Wave height: 0.12, amount of bumps: 8.0, speed: 6.0 (values adjusted with manual testing)
    gl_Position = vec4(ripplePos, 1.0) * mvp;
    vCol = aCol;
    vUV = aUV;
}

/*
    References

    de Vries, J. (n.d.). Shaders. LearnOpenGL.
        https://learnopengl.com/Getting-started/Shaders

    Khronos Group. (n.d.-a). gl_Position [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/gl_Position.xhtml

    Khronos Group. (n.d.-b). sin [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/sin.xhtml
*/
