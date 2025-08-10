extends Node3D

const ZOOM_STEPS = 5
const MIN_ZOOM = 1
var first_person = false

@onready var springArm: SpringArm3D = $"PlayerCamera"
@onready var camera: Camera3D = $"Camera3D"

func move(event: InputEventMouseMotion, mouse_sensitivity: float) -> void:
	rotation.y -= deg_to_rad(event.relative.x * mouse_sensitivity)
	rotation.x -= deg_to_rad(event.relative.y * mouse_sensitivity)
	rotation.x = clamp(rotation.x, deg_to_rad(-80), deg_to_rad(80))
	
func zoom_in() -> void:
	if (springArm.spring_length > MIN_ZOOM):
		springArm.spring_length -= ZOOM_STEPS
		springArm.spring_length = max(springArm.spring_length, MIN_ZOOM)
	else:
		springArm.spring_length -= MIN_ZOOM
		springArm.spring_length = max(springArm.spring_length, 0)
		first_person = true

func zoom_out() -> void:
	if first_person:
		springArm.spring_length += MIN_ZOOM
		first_person = false
	else:
		springArm.spring_length += ZOOM_STEPS
		springArm.spring_length = min(springArm.spring_length, 400)

func set_horizontal_offset(amount: float):
	camera.h_offset = amount


func _process(_delta: float) -> void:
	global_position = $"../PlayerCenter/CameraAnchor".global_position
