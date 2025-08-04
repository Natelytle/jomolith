extends CharacterBody3D
@onready var camera: Node3D = $"PlayerCameraPivot"
@onready var hitbox: CollisionShape3D = $Hitbox

# movement
const MAX_SPEED = 16.0
const GROUND_ACCELERATION = 50
const AIR_ACCELERATION = 15
const JUMP_VELOCITY = 50
var currentSpeed = 0
var currentMovementVector = Vector3.ZERO

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


func _process(delta: float) -> void:
	if mouse_locked:
		hitbox.rotation.y = camera.rotation.y

func _physics_process(delta: float) -> void:
	# Add the gravity.
	if not is_on_floor():
		velocity += get_gravity() * delta

	# Handle jump.
	if Input.is_action_pressed("jump") and is_on_floor():
		velocity.y = JUMP_VELOCITY

	# Get the input direction and handle the movement/deceleration.
	# As good practice, you should replace UI actions with custom gameplay actions.
	var input_dir := Input.get_vector("left", "right", "forward", "backward")
	
	var direction = (transform.basis.rotated(Vector3.UP, camera.rotation.y) * Vector3(input_dir.x, 0, input_dir.y)).normalized()

	# basis rotated by the camera
	if is_on_floor():
		var blend = 1 - pow(0.5, GROUND_ACCELERATION * delta)
		currentMovementVector = lerp(currentMovementVector, direction, blend)
	else:
		var blend = 1 - pow(0.5, AIR_ACCELERATION * delta)
		currentMovementVector = lerp(currentMovementVector, direction, blend)

	# if direction:
	velocity.x = currentMovementVector.x * MAX_SPEED
	velocity.z = currentMovementVector.z * MAX_SPEED
	#else:
		#velocity.x = move_toward(velocity.x, 0, SPEED)
		#velocity.z = move_toward(velocity.z, 0, SPEED)

	move_and_slide()
