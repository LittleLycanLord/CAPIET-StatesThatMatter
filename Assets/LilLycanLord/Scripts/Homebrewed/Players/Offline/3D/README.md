# Movement System Setup Documentation

## Overview
The GeneralPlayerMovementSetup component provides automatic configuration and component management for the modular 3D movement system.

## Features

### 🎮 Movement Recipe System
Pre-configured movement profiles for different game genres:
- **Platformer2D**: Precise, responsive side-scrolling movement
- **Platformer3D**: Smooth 3D platforming with character rotation
- **ActionFPS**: Responsive shooter movement with realistic physics
- **ExplorationFPS**: Relaxed, immersive exploration movement
- **IsometricShooter**: Quick, responsive overhead action movement
- **RacingVehicle**: Momentum-based movement with acceleration buildup
- **Custom**: Use your own inspector-defined settings

### 🔧 Automatic Component Setup
The system can automatically add all required movement components via:

#### Context Menu Options:
- **GameObject → LilLycanLord → Add Movement Setup**: Adds setup component to selected GameObject
- **GameObject → LilLycanLord → Complete Movement Player**: Creates a new GameObject with all components pre-configured

#### Inspector Buttons:
- **Add All Movement Components**: Automatically adds missing movement components
- **Validate Components**: Checks for missing required components
- **Remove All Movement Components**: Safely removes all movement components

### 📋 Required Components
The system automatically manages these components:
1. **CharacterController** - Unity's built-in character physics
2. **General3DMovementCore** - Foundation movement component
3. **General3DMovement** - Enhanced movement with sprinting and rotation
4. **General3DJumpSystem** - Advanced jumping mechanics
5. **GeneralPlayerInputHandler** - Input handling and events
6. **GeneralPlayerController** - Main controller orchestrating all systems
7. **General3DMovementAnimator** - Animation integration (optional, added if Animator present)

### 🚀 Quick Start
1. Right-click in Hierarchy → **GameObject → LilLycanLord → Complete Movement Player**
2. This creates a fully configured player with all components
3. The setup is configured to auto-apply the 3D Platformer recipe on Start
4. Customize settings in the inspector or change the recipe as needed

### ⚙️ Manual Setup
1. Add **GeneralPlayerMovementSetup** component to your GameObject
2. Click **"Add All Movement Components"** in the inspector
3. Choose your desired **Movement Recipe**
4. Enable **"Auto Apply On Start"** for automatic configuration
5. Customize settings if needed

### 🎯 Runtime Testing
- Set a **Runtime Test Recipe** in the inspector
- Press **F1** in play mode (Editor only) to test different recipes on the fly

### 🔍 Debugging
- Enable **"Show Debug Info"** for detailed configuration logging
- Use **"Validate Components"** to check for missing dependencies
- All operations include comprehensive console feedback

## Code Organization
All scripts follow the standardized ASCII box formatting with sections:
- **COMPONENTS**: Component references
- **DISPLAYS**: Public display properties  
- **FIELDS**: Serialized configuration fields
- **ATTRIBUTES**: Runtime attributes and properties
- **MONOBEHAVIOUR**: Unity lifecycle methods
- **NON-MONOBEHAVIOUR**: Private utility methods
- **VIRTUAL/OVERRIDDEN FUNCTIONS**: Public API methods
