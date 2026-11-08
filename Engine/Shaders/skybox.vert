#version 330 core

layout (location = 0) in vec3 aPos;

uniform mat4 view;
uniform mat4 projection;

out vec3 vDir;

void main()
{
    vDir = aPos;                                        // cube position = direction to look up in the cubemap
    vec4 pos = vec4(aPos, 1.0) * view * projection;     // same order as shader.vert
    gl_Position = pos.xyww;                             // z = w
}