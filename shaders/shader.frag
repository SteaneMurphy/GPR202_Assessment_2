#version 330 core

out vec4 fragColour;
in vec3 vCol;
in vec2 vUV;
uniform sampler2D tex;
uniform float textureMix;

void main()
{
    // fragColour = vec4(vUV, 0.0, 1.0);
    fragColour = mix(vec4(vCol, 1.0), texture(tex, vUV), textureMix);
}