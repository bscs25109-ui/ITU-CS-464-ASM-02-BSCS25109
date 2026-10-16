# CS464 Assignment 02 · [Your name] · [Roll no]
- **Video:** [YouTube (unlisted) or Google Drive link: 30–90 s, all five levels]
- **Input:** Legacy Input with Active Input Handling set to Both. Movement and Space are read in Update.
- **Ground check:** Physics.CheckSphere at the Feet marker, checking only the Ground layer, with a wire-sphere gizmo.
- **Camera:** Exact exponential smoothing using 1 − exp(−sharpness × deltaTime) in LateUpdate, with velocity-based look-ahead.
- **Juice:** Landing squash after a successful jump, recovering through an Ease Out Quad curve over 0.2 seconds using a coroutine.
## Tuning — Part D2

| Setting | Value | Why this number |
|---|---|---|
| Speed | 5 m/s | Gives enough speed to cross the course while allowing adjustments on the narrow bridge and platforms. |
| Jump height | 1.8 m | Provides a theoretical same-height reach of about 5.07 m at full speed, exceeding Blockout 1's planned 2–3 m gap. Actual clearance must be checked in playtesting. |
| Time to apex | 0.606 s | Derived from jump height and gravity. Gives approximately six-tenths of a second to adjust movement before descending. |
| Fall multiplier | 2.2 | Shortens the theoretical descent from 0.606 s to 0.409 s, making the jump less floaty without changing the intended apex. |
| Coyote time | 0.1 s | At 5 m/s, allows a jump approximately 0.5 m beyond a platform edge, forgiving a slightly late press. |
| Jump buffer | 0.12 s | Remembers a press up to 120 ms before landing, helping consecutive jumps register. |
| Camera smoothing | Sharpness 10 | Closes about 90% of the distance to a stationary target in 0.23 s, reducing camera delay during movement. |
| Look-ahead | 0.15 s | At 5 m/s, shifts the desired camera position 0.75 m ahead to show more of the upcoming route. |

## Four answers — Part D2

### 1. The jump, in numbers

My jump height is 1.8 m, gravity is 9.81 m/s², and the player's Rigidbody mass is 1 kg.

- Launch speed: v₀ = √(2gh) = √(2 × 9.81 × 1.8) ≈ 5.94 m/s.
- Jump impulse: J = mass × v₀ = 1 × 5.94 ≈ 5.94 N·s.
- Time to apex: v₀ / g ≈ 0.606 s.
- Fall time with 2.2× gravity: √(2h / (g × 2.2)) ≈ 0.409 s.
- Total airborne time: 0.606 + 0.409 ≈ 1.015 s.
- Horizontal reach at 5 m/s: 5 × 1.015 ≈ 5.07 m.

The code calculates the impulse from jumpHeight rather than storing a fixed jumpForce. It resets vertical velocity before applying the impulse so a jump started while falling still has the intended launch speed.

These calculations assume full horizontal speed, equal launch and landing heights, and no intervening collisions. Unity's discrete physics steps can produce a slightly lower actual height and reach.

My widest measured gap is [___ m] in [level name]. During testing, the player [cleared it / required the following adjustment: ___].

### 2. Update and FixedUpdate

Update reads movement input and the Space key, builds camera-relative movement, clamps diagonals, checks grounding, and manages the coyote-time and jump-buffer timestamps. It queues a jump when both timing windows allow it.

FixedUpdate sets horizontal Rigidbody velocity while preserving vertical velocity. It applies each queued jump once, clears the queue, and adds extra gravity while falling.

Reading GetKeyDown only in FixedUpdate can miss a press because some rendered frames contain no physics step. Applying continuous physics forces in Update makes their application depend on the rendering frame rate. Rigidbody velocity is already measured in metres per second, so I do not multiply it by deltaTime.

### 3. Vector3.Dot

A possible use in Blockout 1 is checking whether the player is moving toward the goal. This is a proposed feature, not currently implemented.

Example line inside PlayerController, with a goal Transform reference:

    float alignment = Vector3.Dot(
        move.normalized,
        (goal.position - transform.position).normalized
    );

A positive result means movement has a component toward the goal. A negative result means movement is away from the goal. Zero means perpendicular movement, or no movement when the movement vector is zero.

This could support a wrong-direction indicator.

### 4. The juice

My effect is a landing squash triggered after an actual jump. Walking off an edge without jumping does not arm the effect.

At impact, Body becomes 1.4 times its original width and depth and 0.6 times its original height. A coroutine then restores its original scale using Ease Out Quad:

    float eased = 1f - (1f - t) * (1f - t);

The recovery lasts 0.2 seconds, approximately 12 frames at 60 FPS. This makes landing noticeable while keeping the recovery short. Ease Out Quad restores the shape quickly at first and slows near the end.

The coroutine sets the final scale exactly to the original scale. Only the visual Body changes; the Player's collider remains unchanged.

## Levels — Part D3

| Level | What the controller must do there | What I changed for this jump |
|---|---|---|
| Level01 / Blockout 1 | Navigate the obstacles, cross the narrow bridge, climb the ramp, and jump the final gap to reach the goal. | [After testing: state that no gap changes were needed, or give the actual changes and measurements.] |
| Level02 | Move through enclosed rooms and obstacles, using doorways and narrow openings to reach the goal. | [Test with the controller and record any changes, or state that none were needed.] |
| Level03 | Jump between parkour platforms and climb obstacles to reach the house landmark. | [Record any adjusted platform heights or gaps, or confirm none were needed after testing.] |
| Level04 | Navigate connected rooms, side paths, and narrow passages to reach the exit. | [Record any changes needed for the player to complete the route, or confirm none were needed.] |
