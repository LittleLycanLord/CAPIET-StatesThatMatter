# Modular 3D Movement System - Comprehensive Guide

## 📋 Table of Contents

1. [Quick Start](#quick-start)
2. [Movement Presets & Recipes](#movement-presets--recipes)
3. [Component Overview](#component-overview)
4. [Setup Instructions](#setup-instructions)
5. [Common Configuration Patterns](#common-configuration-patterns)
6. [Troubleshooting & FAQ](#troubleshooting--faq)
7. [Advanced Customization](#advanced-customization)

---

## 🚀 Quick Start

### Essential Setup (All Game Types)

1. **Add `GeneralPlayerController`** to your player GameObject
2. **Ensure you have a `Characte### **Component Method Reference**

### **GeneralPlayerController Public Methods:**

-   `EnableAllSystems()` - Enable all movement systems
-   `DisableAllSystems()` - Disable all movement systems
-   `GetMovementInfo()` - Get current movement state
-   `SetMovementSpeed(float)` - Set base movement speed
-   `SetSprintMultiplier(float)` - Set sprint speed multiplier
-   `SetJumpHeight(float)` - Set jump height
-   `Jump()` - Force a jump
-   `ResetPlayer()` - Reset all systems to default state** and `Animator` component
3. **Configure your `GeneralInputManager`** with the Unity Input System
4. **Set Layer Masks** for ground detection in `General3DMovementCore`
5. **Assign Animation Parameters** in `General3DMovementAnimator`

---

## 🎮 Movement Presets & Recipes

### 🏃‍♂️ **2D Platformer/Metroidvania Style**

_Classic side-scrolling precision platforming_

#### General3DMovementCore Settings:

```
Move Speed: 6-8
Run Speed: 10-12
Gravity: 25-30 (snappy feel)
Ground Check Distance: 0.2
```

#### General3DMovement Settings:

```
✅ Enable Movement: true
✅ Enable Sprinting: true
✅ Face Movement Direction: false (maintain facing direction)
Rotation Speed: 0.1 (minimal rotation)
Sprint Multiplier: 1.3-1.5
Sprint Buildup: 0.1 (instant sprint)
✅ Require Movement For Sprint: true
❌ Prevent Air Sprinting: false (allow air control)
✅ Enable Movement Smoothing: false (precise input)
```

#### General3DJumpSystem Settings:

```
✅ Enable Jumping: true
Max Jumps: 2-3 (double/triple jump)
Jump Height: 3-4
Jump Modifier: 1.0
✅ Enable Variable Jump Height: true
Low Jump Multiplier: 2.0 (responsive short hops)
✅ Enable Coyote Time: true
Coyote Time: 0.15
✅ Enable Jump Buffer: true
Jump Buffer Time: 0.1
Fall Multiplier: 2.5 (quick falls)
✅ Enable General Gravity: true
```

#### Special Notes:

-   Lock Z-axis movement in code if true 2D
-   Use orthographic camera
-   Consider adding wall-jump mechanics

---

### 🏔️ **3D Platforming**

_Mario/Crash Bandicoot style precise 3D platforming_

#### General3DMovementCore Settings:

```
Move Speed: 5-7
Run Speed: 8-11
Gravity: 20-25
Ground Check Distance: 0.3
```

#### General3DMovement Settings:

```
✅ Enable Movement: true
✅ Enable Sprinting: true
✅ Face Movement Direction: true
Rotation Speed: 8-12 (smooth character turning)
Sprint Multiplier: 1.4-1.6
Sprint Buildup: 0.3 (slight buildup for feel)
✅ Require Movement For Sprint: true
✅ Prevent Air Sprinting: true (realistic)
✅ Enable Movement Smoothing: true
Acceleration Time: 0.15
Deceleration Time: 0.1
```

#### General3DJumpSystem Settings:

```
✅ Enable Jumping: true
Max Jumps: 2
Jump Height: 3.5-4.5
Jump Modifier: 1.0
✅ Enable Variable Jump Height: true
Low Jump Multiplier: 1.8
✅ Enable Coyote Time: true
Coyote Time: 0.2
✅ Enable Jump Buffer: true
Jump Buffer Time: 0.15
Fall Multiplier: 2.0
✅ Enable General Gravity: true
```

#### Special Notes:

-   Use smooth camera follow
-   Add landing particles/animations
-   Consider momentum preservation on platforms

---

### 🔫 **3D Action FPS**

_Call of Duty/Valorant style responsive shooter movement_

#### General3DMovementCore Settings:

```
Move Speed: 4-6
Run Speed: 7-9
Gravity: 15-20
Ground Check Distance: 0.2
```

#### General3DMovement Settings:

```
✅ Enable Movement: true
✅ Enable Sprinting: true
❌ Face Movement Direction: false (camera controls facing)
Rotation Speed: 0.1 (minimal body rotation)
Sprint Multiplier: 1.5-1.8
Sprint Buildup: 0.2 (quick but not instant)
✅ Require Movement For Sprint: true
❌ Prevent Air Sprinting: false (strafe jumping)
✅ Enable Movement Smoothing: true
Acceleration Time: 0.05 (very responsive)
Deceleration Time: 0.05
```

#### General3DJumpSystem Settings:

```
✅ Enable Jumping: true
Max Jumps: 1 (realistic)
Jump Height: 2-2.5 (realistic height)
Jump Modifier: 1.0
✅ Enable Variable Jump Height: true
Low Jump Multiplier: 1.5
✅ Enable Coyote Time: true
Coyote Time: 0.1 (minimal)
✅ Enable Jump Buffer: true
Jump Buffer Time: 0.1
Fall Multiplier: 1.5 (realistic)
❌ Enable General Gravity: false
```

#### Special Notes:

-   Disable Y-axis mouse movement on General3DMovement
-   Use MouseLook script for camera
-   Add weapon bob/sway animations
-   Consider slide mechanics

---

### 🌲 **3D Chill/Exploration FPS**

_Firewatch/What Remains of Edith Finch style relaxed exploration_

#### General3DMovementCore Settings:

```
Move Speed: 3-4
Run Speed: 5-6
Gravity: 15-18
Ground Check Distance: 0.4 (forgiving)
```

#### General3DMovement Settings:

```
✅ Enable Movement: true
✅ Enable Sprinting: true
❌ Face Movement Direction: false
Rotation Speed: 0.1
Sprint Multiplier: 1.3-1.5 (gentle sprint)
Sprint Buildup: 0.5 (gradual acceleration)
❌ Require Movement For Sprint: false (can sprint in place)
✅ Prevent Air Sprinting: true
✅ Enable Movement Smoothing: true
Acceleration Time: 0.3 (smooth starts)
Deceleration Time: 0.4 (smooth stops)
```

#### General3DJumpSystem Settings:

```
✅ Enable Jumping: true
Max Jumps: 1
Jump Height: 1.5-2 (realistic/small hops)
Jump Modifier: 1.0
✅ Enable Variable Jump Height: true
Low Jump Multiplier: 1.3
✅ Enable Coyote Time: true
Coyote Time: 0.25 (forgiving)
✅ Enable Jump Buffer: true
Jump Buffer Time: 0.2
Fall Multiplier: 1.2 (gentle)
❌ Enable General Gravity: false
```

#### Special Notes:

-   Use head bob for immersion
-   Add footstep sound variations
-   Gentle camera sway
-   Consider interaction prompts

---

### 🎯 **3D Isometric Shooter**

_Diablo/Path of Exile style overhead action_

#### General3DMovementCore Settings:

```
Move Speed: 5-7
Run Speed: 8-10
Gravity: 20 (standard)
Ground Check Distance: 0.3
```

#### General3DMovement Settings:

```
✅ Enable Movement: true
✅ Enable Sprinting: true
✅ Face Movement Direction: true (toward movement)
Rotation Speed: 15-20 (quick turns)
Sprint Multiplier: 1.4-1.7
Sprint Buildup: 0.1 (quick response)
✅ Require Movement For Sprint: true
❌ Prevent Air Sprinting: false
✅ Enable Movement Smoothing: true
Acceleration Time: 0.1
Deceleration Time: 0.1
```

#### General3DJumpSystem Settings:

```
❌ Enable Jumping: false (usually no jumping in isometric)
```

_OR if you want jumping:_

```
✅ Enable Jumping: true
Max Jumps: 1
Jump Height: 1.5
Jump Modifier: 1.0
❌ Enable Variable Jump Height: false
✅ Enable Coyote Time: true
Coyote Time: 0.1
❌ Enable Jump Buffer: false
Fall Multiplier: 2.0
✅ Enable General Gravity: true
```

#### Special Notes:

-   Lock camera to isometric angle
-   Use click-to-move if desired
-   Add dodge roll mechanics
-   Consider weapon range indicators

---

### 🏎️ **Additional Style: Racing/Vehicle Feel**

_When you want momentum-based movement_

#### General3DMovementCore Settings:

```
Move Speed: 6-8
Run Speed: 12-15
Gravity: 18
Ground Check Distance: 0.3
```

#### General3DMovement Settings:

```
✅ Enable Movement: true
✅ Enable Sprinting: true
✅ Face Movement Direction: true
Rotation Speed: 5-8 (slower turning)
Sprint Multiplier: 1.8-2.2 (significant speed boost)
Sprint Buildup: 0.8-1.2 (momentum buildup)
❌ Require Movement For Sprint: false
❌ Prevent Air Sprinting: false
✅ Enable Movement Smoothing: true
Acceleration Time: 0.5-0.8 (momentum building)
Deceleration Time: 0.6-1.0 (sliding stops)
```

---

## 🧩 Component Overview

### **General3DMovementCore**

-   **Purpose**: Foundation movement with CharacterController
-   **Key Features**: Ground detection, gravity, basic movement
-   **When to Modify**: Changing fundamental physics feel

### **GeneralPlayerInputHandler**

-   **Purpose**: Input processing and event management
-   **Key Features**: Input caching, sprint modes, jump buffering
-   **When to Modify**: Adding new input types or changing input behavior

### **General3DMovement**

-   **Purpose**: Advanced movement behaviors and feel
-   **Key Features**: Sprinting, rotation, movement smoothing
-   **When to Modify**: Tweaking game feel and movement responsiveness

### **General3DJumpSystem**

-   **Purpose**: All jumping mechanics and air control
-   **Key Features**: Multi-jump, coyote time, variable jump height
-   **When to Modify**: Adding platforming elements or air mechanics

### **General3DMovementAnimator**

-   **Purpose**: Animation parameter management
-   **Key Features**: Smooth parameter transitions, movement state tracking
-   **When to Modify**: Adding new animations or changing blend trees

### **GeneralPlayerController**

-   **Purpose**: Coordinator and public API
-   **Key Features**: System setup, state monitoring, external interface
-   **When to Modify**: Adding new public methods or system coordination

---

## ⚙️ Setup Instructions

### 1. **Basic Setup**

```csharp
// 1. Add GeneralPlayerController to player GameObject
// 2. It will auto-add required components:
//    - General3DMovementCore
//    - GeneralPlayerInputHandler
//    - General3DMovement
//    - General3DJumpSystem
//    - General3DMovementAnimator
```

### 2. **Required Components** (Auto-added)

-   `CharacterController` - Unity's built-in character physics
-   `Animator` - For movement animations

### 3. **Input System Setup**

```csharp
// Ensure GeneralInputManager is properly configured:
// - WASD movement mapped
// - Sprint key mapped (Shift)
// - Jump key mapped (Space)
// - Mouse input for camera (if FPS)
```

### 4. **Animation Setup**

```csharp
// Create Animator Controller with parameters:
// - "Speed" (Float) - Movement speed
// - "IsMoving" (Bool) - Is character moving
// - "IsGrounded" (Bool) - Is character on ground
// - "IsSprinting" (Bool) - Is character sprinting
// - "IsJumping" (Bool) - Is character jumping
// - "IsFalling" (Bool) - Is character falling
// - "JumpTrigger" (Trigger) - Jump animation trigger
// - "LandTrigger" (Trigger) - Landing animation trigger
```

### 5. **Layer Setup**

```csharp
// Ground Layer Setup:
// 1. Create "Ground" layer
// 2. Assign to ground objects
// 3. Set Ground Mask in General3DMovementCore to "Ground" layer
```

---

## 🔧 Common Configuration Patterns

### **Responsive Platforming**

```
High Sprint Buildup + Low Movement Smoothing + High Jump Height
```

### **Realistic Movement**

```
Prevent Air Sprinting + Variable Jump Height + Moderate Gravity
```

### **Arcade Feel**

```
Low Sprint Buildup + Multiple Jumps + High Fall Multiplier
```

### **Cinematic Movement**

```
High Movement Smoothing + Gradual Sprint Buildup + Gentle Gravity
```

---

## 🆘 Troubleshooting & FAQ

### **❗ Common Setup Errors**

#### **"Character doesn't move"**

-   ✅ Check if `EnableMovement` is true
-   ✅ Verify `GeneralInputManager` is in scene and working
-   ✅ Ensure `CharacterController` is present and enabled
-   ✅ Check if `General3DMovementCore` Move Speed > 0

#### **"Character falls through ground"**

-   ✅ Verify Ground Layer is set correctly
-   ✅ Check Ground Mask in `General3DMovementCore`
-   ✅ Ensure ground objects have colliders
-   ✅ Verify `CharacterController` has proper size

#### **"Jumping doesn't work"**

-   ✅ Check if `EnableJumping` is true in `General3DJumpSystem`
-   ✅ Verify jump input is mapped in `GeneralInputManager`
-   ✅ Ensure character is grounded
-   ✅ Check if `MaxJumps` > 0

#### **"Sprint not working"**

-   ✅ Verify `EnableSprinting` is true
-   ✅ Check sprint input mapping
-   ✅ If `RequireMovementForSprint` is true, ensure character is moving
-   ✅ If `PreventAirSprinting` is true, ensure character is grounded

#### **"Character rotates weirdly"**

-   ✅ Check `FaceMovementDirection` setting
-   ✅ Adjust `RotationSpeed` (lower = slower rotation)
-   ✅ For FPS games, set `FaceMovementDirection` to false

#### **"Animations not playing"**

-   ✅ Verify Animator Controller is assigned
-   ✅ Check animation parameter names match system defaults
-   ✅ Ensure `General3DMovementAnimator` component is enabled
-   ✅ Use `SetParameterNames()` if using custom parameter names

### **⚡ Performance Issues**

#### **"System feels laggy"**

-   ✅ Reduce `RotationSpeed` if rotation is too fast
-   ✅ Increase `AccelerationTime` for smoother movement
-   ✅ Check if too many components are enabled unnecessarily

#### **"Input feels unresponsive"**

-   ✅ Reduce `AccelerationTime` and `DecelerationTime`
-   ✅ Disable `MovementSmoothing` for instant response
-   ✅ Reduce `SprintBuildup` time

### **🎮 Common Configuration Questions**

#### **Q: How do I make character face camera direction instead of movement?**

A: Set `FaceMovementDirection` to false in `General3DMovement`

#### **Q: How do I add double/triple jumping?**

A: Increase `MaxJumps` in `General3DJumpSystem` (2 = double jump, 3 = triple jump)

#### **Q: How do I make movement more realistic/arcade?**

A:

-   **Realistic**: Enable `PreventAirSprinting`, set `MaxJumps` to 1, use moderate `FallMultiplier`
-   **Arcade**: Disable `PreventAirSprinting`, increase `MaxJumps`, increase `FallMultiplier`

#### **Q: How do I disable jumping entirely?**

A: Set `EnableJumping` to false in `General3DJumpSystem`

#### **Q: How do I make sprint work like a toggle instead of hold?**

A: Use `SetSprintToggleMode(true)` on `PlayerInputHandler`

#### **Q: Character moves too fast/slow?**

A: Adjust `MoveSpeed` and `RunSpeed` in `General3DMovementCore`

#### **Q: How do I add custom input actions?**

A: Extend `GeneralPlayerInputHandler` or subscribe to existing events in your custom scripts

---

## 🎨 Advanced Customization

### **Adding Custom Movement States**

```csharp
// Subscribe to movement events
general3DMovement.OnStartedMoving += () => {
    // Custom logic when movement starts
};

general3DMovement.OnStartedSprinting += () => {
    // Custom logic when sprinting starts
};
```

### **Custom Jump Mechanics**

```csharp
// Access jump system directly
var jumpSystem = GetComponent<General3DJumpSystem>();
jumpSystem.OnJumpStarted += () => {
    // Custom jump effects
};
```

### **Extending the System**

```csharp
// Create custom movement modifier
public class CustomMovementModifier : MonoBehaviour {
    private General3DMovement movement;

    void Start() {
        movement = GetComponent<General3DMovement>();
        // Modify movement parameters based on game state
        movement.SetSprintMultiplier(2.0f);
    }
}
```

### **Integration with Other Systems**

```csharp
// Health system affecting movement
public class HealthMovementIntegration : MonoBehaviour {
    void OnHealthChanged(float health) {
        var movement = GetComponent<General3DMovement>();
        float speedMultiplier = health / 100f;
        movement.SetSprintMultiplier(1.0f + speedMultiplier);
    }
}
```

---

## 📝 Component Method Reference

### **Streamlined3DPlayerController Public Methods:**

-   `EnableAllSystems()` - Enable all movement systems
-   `DisableAllSystems()` - Disable all movement systems
-   `GetMovementInfo()` - Get current movement state
-   `SetMovementSpeed(float)` - Set base movement speed
-   `SetSprintMultiplier(float)` - Set sprint speed multiplier
-   `SetJumpHeight(float)` - Set jump height
-   `Jump()` - Force a jump
-   `ResetPlayer()` - Reset all systems to default state

### **General3DMovement Public Methods:**

-   `EnableMovement()` / `DisableMovement()` - Toggle movement
-   `SetSprintMultiplier(float)` - Adjust sprint speed
-   `SetRotationSpeed(float)` - Adjust turn speed
-   `SetCameraReference(Transform)` - Set camera for movement direction

### **General3DJumpSystem Public Methods:**

-   `SetJumpHeight(float)` - Adjust jump height
-   `SetMaxJumps(int)` - Set multi-jump count
-   `ResetJumps()` - Reset jump counter
-   `EnableJumping()` / `DisableJumping()` - Toggle jumping

---

## 🔗 Integration Notes

-   **Compatible with**: Unity's Input System, Cinemachine, Timeline
-   **Animation Requirements**: Animator Controller with movement parameters
-   **Physics Requirements**: CharacterController (Rigidbody not needed)
-   **Input Requirements**: GeneralInputManager or custom input system

---

_This modular system replaces the monolithic General3DPlayerMovement with better maintainability, performance, and flexibility. Each component can be individually configured and extended for your specific game needs._
