@tool
extends CSGBox3D

# This script automatically updates the shader's mesh_size uniform
# based on the CSGBox3D's size property

@export var auto_update_shader := true

func _ready():
	if auto_update_shader:
		update_shader_mesh_size()

func _on_size_changed():
	if auto_update_shader:
		update_shader_mesh_size()

func update_shader_mesh_size():
	# Get the material from the CSGBox3D
	if material and material is ShaderMaterial:
		material.set_shader_parameter("mesh_size", size)
		
		# print("Updated shader mesh_size to: ", size)
	# else:
	# 	print("No shader material found - make sure to set a ShaderMaterial in the material_override property")

# Call this function whenever you change the box size in code
func set_box_size(new_size: Vector3):
	size = new_size
	update_shader_mesh_size()

# Optional: Connect to property changes if you want real-time updates
func _notification(what):
	if what == NOTIFICATION_EDITOR_POST_SAVE or what == NOTIFICATION_TRANSFORM_CHANGED:
		if auto_update_shader:
			call_deferred("update_shader_mesh_size")