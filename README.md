# CS464 Assignment 02 
# M.Abubakar
# BSCS25109
- **Video:** https://drive.google.com/file/d/1Ox3ziNRM2IPjxzRJRtLotByjQb44AWwY/view?usp=sharing
- **Input:** Legacy Input with Active Input Handling set to Both
- **Ground check:** Contact normal — `OnCollisionStay`, `normal.y > 0.5f`
- **Camera:** Exact smoothing `1 − e^(−sharpness × dt)` with look-ahead
- **Juice:** Squash on landing — `Ease.OutBack` curve, started by `OnCollisionEnter` after `airTime > 0.1s`, run by a coroutine on the `Body` child

---

## Tuning

| Setting | Value | Why this number |
|---|---|---|
| Speed | 5 m/s | Crosses a 1 m platform in 0.2 s; fast enough to feel momentum, slow enough to steer accurately |
| Jump height | 1.8 m | Clears the tallest ledge in the levels (1.5 m) with 0.3 m of margin |
| Time to apex | 0.61 s | Derived: v₀ = √(2 × 9.81 × 1.8) = 5.94 m/s; apex = 5.94 ÷ 9.81 = 0.61 s |
| Fall multiplier | 2.2× | Fall time = 0.61 ÷ √2.2 = 0.41 s; total jump = 1.02 s; at 5 m/s reach = 5.1 m — clears the widest gap |
| Coyote time | 0.10 s | At 5 m/s that is 0.5 m past the edge — enough for the run-off jump in Level04 |
| Jump buffer | 0.12 s | ~7 frames at 60 fps; covers a realistic early press of 60–120 ms before landing |
| Camera smoothing | sharpness = 5 | Lag = 1/k = 0.2 s; at 5 m/s the camera trails by ~1 m — visible but comfortable |
| Look-ahead | 0.35 s | At 5 m/s that is 1.75 m ahead, roughly cancelling the 1 m lag so the camera leads slightly |

---

## Four answers

### 1. The jump, in numbers

With `jumpHeight = 1.8 m` and `g = 9.81 m/s²`:

- **Launch speed:** v₀ = √(2 × 9.81 × 1.8) = **5.94 m/s**
- **Jump impulse:** mass (1 kg) × v₀ = **5.94 N·s**
- **Time to apex:** v₀ ÷ g = 5.94 ÷ 9.81 = **0.61 s**
- **Fall time:** 0.61 ÷ √2.2 = **0.41 s** (2.2× fall gravity)
- **Total air time:** 0.61 + 0.41 = **1.02 s**
- **Horizontal reach:** 5 m/s × 1.02 s = **5.1 m**

The widest gap in my levels is 3 m (Level03). At 5.1 m reach the jump clears it with 2.1 m to spare.

---

### 2. Update and FixedUpdate

**Update** runs once per rendered frame (variable rate — 60 fps, 144 fps, etc.). I use it to:
- Read `Input.GetAxis` and `Input.GetKeyDown` — `GetKeyDown` is true for exactly one frame, so it must be read here or it will be missed
- Stamp `lastGroundedTime` (coyote) and `lastPressedTime` (buffer)
- Build the `move` vector relative to the camera
- Track `airTime` for the juice trigger

**FixedUpdate** runs at a fixed 0.02 s timestep (50 Hz), independent of framerate. I use it to:
- Set `rb.linearVelocity` from `move` — physics expects a consistent timestep
- Apply the queued jump impulse once and clear the flag
- Add fall gravity with `ForceMode.Acceleration`

**What breaks if swapped:**
- `GetKeyDown` in FixedUpdate: at 144 fps, ~2 frames in 3 run without any FixedUpdate call, so roughly 67% of Space presses would be silently dropped.
- `rb.linearVelocity` in Update: the physics engine and render loop are out of sync, causing jitter and inconsistent movement speed across different framerates.

---

### 3. Vector3.Dot

In `OnCollisionStay`, I check `c.GetContact(0).normal.y > 0.5f` to decide if a surface is ground.

This is equivalent to:

```csharp
Vector3.Dot(c.GetContact(0).normal, Vector3.up) > 0.5f
```

`Vector3.Dot(normal, Vector3.up)` returns the cosine of the angle between the surface normal and straight-up:

- **Positive (close to 1.0):** surface faces upward → it is walkable ground
- **Zero (0.0):** surface is vertical → it is a wall, not ground
- **Negative:** surface faces downward → it is a ceiling

The threshold 0.5 means the surface must be within ~60° of horizontal to count as ground. This prevents the player from jumping while pressing against a steep slope or a wall.

---

### 4. The juice

- **Effect:** Squash on landing — the `Body` child mesh is scaled down on Y (compressed) and out on X/Z (widened to preserve volume), then springs back to rest
- **Curve:** `Ease.OutBack` — goes from 0 → overshoots ~1.10 → settles at 1.0. The overshoot reads as physical weight and bounce
- **Duration:** 0.25 seconds = **15 frames at 60 fps**
- **Trigger event:** `OnCollisionEnter` with `contact.normal.y > 0.5f` AND `airTime > 0.1s` — only fires on a real landing, not on tiny ground bumps
- **Why OutBack:** A linear or OutQuad curve would just ease in and stop. OutBack's overshoot makes the body spring back slightly, which reads as the player having mass and elasticity — it feels alive

Squash is applied to the `Body` child only, so the Capsule Collider on the parent never changes shape and movement stays physically correct.

---

## Levels

| Blockouts | What the controller must do there | What I changed for this jump |
|---|---|---|
| B01 | Walk forward through pink cylinder obstacles along a walled corridor, step over a small raised platform, then climb a ramp to the elevated goal — basic movement and a gentle height change | No change needed; default 1.8 m jump clears the ramp step comfortably |
| B02 | [Move through enclosed rooms and narrow doorways, steer around obstacles, and follow the route to the end.] | [No change needed; default 1.8 m jump clears the ramp step comfortably ] |
| B03 | [Jump between raised platforms above the hazard, control each landing, and follow the parkour route toward the house.] | [No change needed;  3m jump because of the parkour  ] |
| B04 | [Navigate connected rooms and side passages, make controlled turns through narrow spaces, and find the final exit.] | [No change needed; default 1.8 m jump  ] |
