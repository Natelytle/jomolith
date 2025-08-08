shader_type spatial;

uniform sampler2D texture_albedo : source_color;
uniform float texture_scale : hint_range(0.1, 10.0) = 1.0;
uniform vec3 mesh_size = vec3(1.0, 1.0, 1.0);
uniform bool auto_center = true;

varying vec3 world_position;
varying vec3 world_normal;

void vertex() {
    world_position = VERTEX;
    world_normal = NORMAL;
}

void fragment() {
    // Calculate offset to center the texture on the mesh
    vec3 offset = auto_center ? mesh_size * 0.5 : vec3(0.0);

    // Apply offset and scale to vertex position
    vec3 vertex_pos = (world_position + offset) * texture_scale;

    // Use the world_normal from vertex shader (which is in object space)
    vec3 normal = normalize(world_normal);
    vec3 adjusted_normal = abs(normal);

    // Normalize weights so they sum to 1
    vec3 weights = adjusted_normal / (adjusted_normal.x + adjusted_normal.y + adjusted_normal.z);

    // Sample texture on each axis with proper offsetting
    vec2 uv_x = vertex_pos.zy;
    vec2 uv_y = vertex_pos.xz;
    vec2 uv_z = vertex_pos.xy;

    vec3 color_x = texture(texture_albedo, uv_x).rgb * weights.x;
    vec3 color_y = texture(texture_albedo, uv_y).rgb * weights.y;
    vec3 color_z = texture(texture_albedo, uv_z).rgb * weights.z;

    // No need to divide by 3 since weights sum to 1
    ALBEDO = color_x + color_y + color_z;
}