#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aCol;
layout (location = 2) in vec2 aUV;
out vec3 vCol;
out vec2 vUV;
uniform mat4 mvp;

void main()
{
    gl_Position = vec4(aPos, 1.0) * mvp;
    vCol = aCol;
    vUV = aUV;
}