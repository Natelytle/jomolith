extends CharacterBody3D
@onready var camera: Node3D = $"PlayerCameraPivot"
@onready var hitbox: CollisionShape3D = $Hitbox

# movement
const MAX_SPEED = 16
const GROUND_ACCELERATION = 50
const AIR_ACCELERATION = 15
const JUMP_VELOCITY = 50
const COYOTE_TIME = 0.2

var currentSpeed = 0
var currentMovementVector = Vector3.ZERO
var timeSinceLastOnFloor = 0

# mouse
var mouse_locked = false
var shift_lock = false
@export var mouse_sensitivity: float = 0.4

func _input(event: InputEvent) -> void:
	if event.is_action("zoom_in") and event.is_pressed():
		camera.zoom_in()
		mouse_locked = shift_lock || camera.first_person

	if event.is_action("zoom_out") and event.is_pressed():
		camera.zoom_out()
		mouse_locked = shift_lock || camera.first_person
	
	if event.is_action("shift_lock") and event.is_pressed():
		shift_lock = !shift_lock
		mouse_locked = shift_lock || camera.first_person

	if mouse_locked or Input.is_action_pressed("right_click"):
		Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
		camera.move(event, mouse_sensitivity)
	else:
		Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)

func _process(_delta: float) -> void:
	# Handle turning with and without shift-lock
	if mouse_locked:
		hitbox.rotation.y = camera.rotation.y


func _physics_process(delta: float) -> void:
	if not is_on_floor():
		timeSinceLastOnFloor += delta
		velocity += get_gravity() * delta
	else:
		timeSinceLastOnFloor = 0

	if Input.is_action_pressed("jump") and timeSinceLastOnFloor <= COYOTE_TIME:
		velocity.y = JUMP_VELOCITY
		timeSinceLastOnFloor += COYOTE_TIME

	# Get the input direction and handle the movement/deceleration.
	var input_dir := Input.get_vector("left", "right", "forward", "backward")
	
	var direction = (transform.basis.rotated(Vector3.UP, camera.rotation.y) * Vector3(input_dir.x, 0, input_dir.y)).normalized()

	if is_on_floor():
		var blend = 1 - pow(0.5, GROUND_ACCELERATION * delta)
		currentMovementVector = lerp(currentMovementVector, direction, blend)
	else:
		var blend = 1 - pow(0.5, AIR_ACCELERATION * delta)
		currentMovementVector = lerp(currentMovementVector, direction, blend)

	velocity.x = currentMovementVector.x * MAX_SPEED
	velocity.z = currentMovementVector.z * MAX_SPEED

	if not mouse_locked and direction.length() > 0:
		hitbox.rotation.y = lerp_angle(hitbox.rotation.y, atan2(-direction.x, -direction.z), 10 * delta)

	move_and_slide()
