extends Node3D

const ZOOM_STEPS = 5
const MIN_ZOOM = 1
var first_person = false

@onready var playerCamera := $PlayerCamera

func move(event: InputEventMouseMotion, mouse_sensitivity: float) -> void:
	rotation.y -= deg_to_rad(event.relative.x * mouse_sensitivity)
	rotation.x -= deg_to_rad(event.relative.y * mouse_sensitivity)
	rotation.x = clamp(rotation.x, deg_to_rad(-80), deg_to_rad(80))
	
func zoom_in() -> void:
	if (playerCamera.spring_length > MIN_ZOOM):
		playerCamera.spring_length -= ZOOM_STEPS
		playerCamera.spring_length = max(playerCamera.spring_length, MIN_ZOOM)
	else:
		playerCamera.spring_length -= MIN_ZOOM
		playerCamera.spring_length = max(playerCamera.spring_length, 0)
		first_person = true

func zoom_out() -> void:
	if first_person:
		playerCamera.spring_length += MIN_ZOOM
		first_person = false
	else:
		playerCamera.spring_length += ZOOM_STEPS
		playerCamera.spring_length = min(playerCamera.spring_length, 400)

func _process(_delta: float) -> void:
	global_position = $"..".global_position + Vector3.UP * 4.5
