# SAN AR Dragon — Project Documentation

**Game Name:** SAN AR Dragon  
**Platform:** Android (ARCore Compatible Devices)  
**Engine & Version:** Unity 6.3 LTS (`6000.3.25f1`)  
**Package Identifier:** `com.san.ardragon`  
**AR Framework:** Unity AR Foundation 6.5.0 + Google ARCore XR Plug-in 6.5.0  

---

## 1. Overview & Concept

**SAN AR Dragon** is a real-world Augmented Reality (AR) mobile game inspired by image-tracking AR gameplay. 
The player points their Android phone camera at a dragon reference picture displayed on a screen (laptop/monitor/tablet) or printed on paper. Upon recognition:
1. AR Foundation image tracking detects the dragon reference marker.
2. A 3D Dragon model spawns at the real-world position and orientation of the detected image.
3. The player navigates the dragon around the environment using a responsive on-screen mobile joystick positioned at the bottom-left of the display.
4. The dragon features smooth rotation towards its movement heading, organic procedural breathing and wing bobbing, stable AR world tracking, and an in-game HUD with real-time tracking indicators, status text, and a Reset Dragon button.

---

## 2. Project Architecture & Folder Structure

All custom assets and scripts are placed in `Assets/SAN_AR/`:

```
Assets/SAN_AR/
├── Animations/             # Procedural & clip controllers
├── Audio/                  # Audio clips and sound effects
├── Documentation/          # Project documentation (README.md, BUILD_ANDROID.md)
├── Editor/                 # Unity Editor setup scripts (SANProjectBuilder.cs)
├── Images/
│   ├── MenuBackground.png  # Dark fantasy main menu background
│   └── ReferenceImages/
│       ├── DragonReference.png       # High-contrast tracking reference image
│       └── DragonImageLibrary.asset  # AR Foundation XRReferenceImageLibrary
├── Materials/
│   └── DragonMaterial.mat  # Crimson red metallic dragon PBR material
├── Models/
│   └── DragonModel.obj     # Complete 3D dragon mesh (torso, wings, legs, head, horns, tail)
├── Prefabs/
│   └── DragonPrefab.prefab # Complete playable dragon prefab with movement and procedural animations
├── Scenes/
│   ├── MainMenu.unity      # Title screen with Start AR, Instructions modal, and Exit
│   └── ARGameplay.unity    # Real AR camera scene with AR Foundation rig and on-screen joystick
└── Scripts/
    ├── AR/
    │   └── ARImageTrackingController.cs # Manages AR image tracking events, single spawn, and anchor resets
    ├── Dragon/
    │   ├── DragonMovement.cs            # Responsive world-space movement & camera-relative heading
    │   └── DragonAnimationController.cs # Procedural breathing, wing flaps, and animator hooks
    └── UI/
        ├── DragonJoystick.cs            # On-screen touchscreen joystick (IDragHandler, IPointerDownHandler)
        ├── GameplayUIController.cs      # In-game HUD, tracking indicator dot, and reset triggers
        └── MainMenuController.cs        # Main menu scene navigation and instructions modal
```

---

## 3. How Image Tracking Works

1. **AR Tracked Image Manager:** Located on the `XR Origin` in `ARGameplay.unity`. It references `DragonImageLibrary.asset`.
2. **Reference Marker:** `DragonReference.png` is registered with a physical dimension of `0.2m x 0.2m` (20 cm), ideal for laptop screens and A4 paper printouts.
3. **Detection & Spawning:**
   - When the camera sees the reference image, `ARImageTrackingController.OnTrackedImagesChanged` is invoked.
   - It instantiates a single instance of `DragonPrefab.prefab` anchored to the image.
   - The status updates to: `“Dragon Found! Use the joystick to move.”`
   - If the player steers the dragon with the joystick, the dragon smoothly decouples from the marker's local space into the AR world frame, allowing free movement around your room without jumping or snapping back to the image.
   - Pressing **RESET DRAGON** instantly repositions the dragon back to the image anchor.

---

## 4. How to Assign or Replace the Reference Image

1. Add your desired dragon reference image into `Assets/SAN_AR/Images/ReferenceImages/`.
2. In the Unity Inspector, select the image:
   - Set **Texture Type** to `Default`.
   - Enable **Read/Write**.
   - Click **Apply**.
3. Select `DragonImageLibrary.asset` in `Assets/SAN_AR/Images/ReferenceImages/`.
4. Add a new entry or replace `DragonReference`, set the name, and specify physical size (e.g., `0.2` x `0.2` meters).
5. Ensure `ARImageTrackingController` in `ARGameplay.unity` has `Target Image Name` matching your library entry name (`DragonReference`).

---

## 5. How to Replace the 3D Dragon Model

1. Import your 3D model (`.fbx` or `.obj`) into `Assets/SAN_AR/Models/`.
2. Open `Assets/SAN_AR/Prefabs/DragonPrefab.prefab` in Prefab Mode.
3. Replace the mesh or child GameObject under `BodyContainer` with your new 3D dragon mesh.
4. If your model includes skeletal animations (Idle, Walk, Fly):
   - Attach an `Animator` component with your `Animator Controller`.
   - In `DragonAnimationController`, ensure `IsWalking` and `Speed` parameters match your animator.
5. Save the prefab.

---

## 6. Adjusting Dragon Scale and Movement Speed

Select `Assets/SAN_AR/Prefabs/DragonPrefab.prefab` or the spawned dragon:
- In `DragonMovement.cs`:
  - **Move Speed:** Adjusts walking/gliding speed (default: `1.2`).
  - **Rotation Speed:** Adjusts turn sharpness (default: `10`).
  - **Dragon Scale:** Adjusts overall physical size in AR (default: `0.25`).

---

## 7. Controls & User Interface

- **Start AR:** Opens the AR camera scene.
- **Instructions:** Displays how-to-play popup.
- **On-Screen Joystick (Bottom-Left):** Drag the red knob to move and steer the dragon relative to your camera view.
- **Tracking Indicator (Top-Left):**
  - **Green Dot:** Reference image actively tracked.
  - **Red Dot:** Reference image not currently visible or tracking limited.
- **Reset Dragon (Bottom-Right):** Repositions the dragon back onto the reference image plane.
- **Menu (Top-Right):** Returns to the title screen.
