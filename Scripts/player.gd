extends CharacterBody3D
@onready var camera: Node3D = $"PlayerCameraPivot"
@onready var hitbox: CollisionShape3D = $"HitboxTorso"
@onready var hitboxLegs: Node3D = hitbox.get_node("HitboxLegs")

# movement
const MAX_SPEED = 16
const GROUND_ACCELERATION = 80
const AIR_ACCELERATION = 15
const JUMP_VELOCITY = 50
const COYOTE_TIME = 0.08

var currentSpeed = 0
var currentMovementVector = Vector3.ZERO
var coyoteTimeTimer = 0

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

	if event is InputEventMouseMotion:
		var mouseEvent = event as InputEventMouseMotion

		if Input.is_action_pressed("right_click"):
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
			camera.move(mouseEvent, mouse_sensitivity)
		elif mouse_locked:
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
			camera.move(mouseEvent, mouse_sensitivity)
		else:
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)

func _process(_delta: float) -> void:
	# Handle turning with and without shift-lock
	if mouse_locked:
		hitbox.rotation.y = camera.rotation.y


func _physics_process(delta: float) -> void:
	var averageLength = hitboxLegs.get_average_length()

	var isOnFloor = averageLength < 2.1

	if not isOnFloor:
		velocity += get_gravity() * delta
		coyoteTimeTimer += delta
	else:
		velocity.y = -get_gravity().y * (2 - averageLength) / 10
		coyoteTimeTimer = 0

	if Input.is_action_pressed("jump") and coyoteTimeTimer <= COYOTE_TIME:
		velocity.y = JUMP_VELOCITY
		coyoteTimeTimer += COYOTE_TIME

	# Get the input direction and handle the movement/deceleration.
	var input_dir := Input.get_vector("left", "right", "forward", "backward")
	
	var direction = (transform.basis.rotated(Vector3.UP, camera.rotation.y) * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	var targetMovementVector = direction * MAX_SPEED

	if isOnFloor:
		currentMovementVector = accelerate(currentMovementVector, targetMovementVector, GROUND_ACCELERATION, delta)
	else:
		currentMovementVector = accelerate(currentMovementVector, targetMovementVector, AIR_ACCELERATION, delta)

	velocity.x = currentMovementVector.x
	velocity.z = currentMovementVector.z

	if not mouse_locked and direction.length() > 0:
		hitbox.rotation.y = lerp_angle(hitbox.rotation.y, atan2(-direction.x, -direction.z), 10 * delta)

	move_and_slide()


func accelerate(current: Vector3, target: Vector3, accel: float, delta: float) -> Vector3:
	var factor = 1.0 - pow(0.2, 80.0 * delta)
	var scaledAccel = accel * (10.0 * delta)

	var lerp_z = lerp(current, target, factor)
	var accel_z = current + (target - current).normalized() * scaledAccel

	# Whichever one accelerates the player less is the chosen one
	if ((lerp_z - current).length() > (accel_z - current).length()):
		return accel_z
	else:
		return lerp_z