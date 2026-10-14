# SAN AR Dragon — Android Build and Testing Guide

This guide covers building the APK for **SAN AR Dragon** and deploying it to an Android phone running Google Play Services for AR (ARCore).

---

## 1. Prerequisites

1. **Unity Version:** Unity 6.3 LTS (`6000.3.25f1`).
2. **Unity Android Build Support Module:**
   - Android Build Support
   - Android SDK & NDK Tools
   - OpenJDK
3. **Android Device Requirements:**
   - An Android device supported by ARCore (check [Google ARCore Supported Devices](https://developers.google.com/ar/devices)).
   - Android OS: Android 9.0 (API 28) or higher.
   - Google Play Services for AR installed on the device.

---

## 2. Installing Android Build Support in Unity Hub (If Not Installed)

If your Unity 6.3 Editor does not yet have the Android target module installed:
1. Open **Unity Hub**.
2. Click on the **Installs** tab on the left.
3. Locate **Unity 6000.3.25f1**, click the gear icon (or three dots `...`), and select **Add modules**.
4. Check:
   - **Android Build Support**
   - **Android SDK & NDK Tools**
   - **OpenJDK**
5. Click **Install** and wait for completion.

---

## 3. Configuring and Building the APK in Unity

1. Open the project in Unity Editor:
   - Folder: `D:\SAN AR Dragon\SAN AR Dragon`
2. Ensure the scenes are in the Build Settings:
   - Open **File > Build Settings...**
   - Verify the Scenes in Build:
     - `0: Assets/SAN_AR/Scenes/MainMenu.unity`
     - `1: Assets/SAN_AR/Scenes/ARGameplay.unity`
3. Switch Platform to **Android**:
   - In **Build Settings**, select **Android** under Platform.
   - Click **Switch Platform** (if not already Android).
4. Verify Player Settings (**Edit > Project Settings > Player**):
   - **Company Name:** `SAN Games`
   - **Product Name:** `SAN AR Dragon`
   - **Package Name:** `com.san.ardragon`
   - **Minimum API Level:** Android 9.0 'Pie' (API level 28) or higher
   - **Scripting Backend:** IL2CPP
   - **Target Architectures:** ARM64 enabled
5. Build the APK:
   - In **Build Settings**, click **Build**.
   - Choose a destination folder and name your file (e.g., `SAN_AR_Dragon.apk`).
   - Click **Save** and wait for Unity to complete the build.

---

## 4. Deploying to Your Android Device

### Option A: Using USB Debugging (Build and Run)
1. On your phone:
   - Go to **Settings > About phone**.
   - Tap **Build number** 7 times to enable **Developer Options**.
   - Go to **Settings > Developer options** and enable **USB debugging**.
2. Connect your phone to your PC via a USB cable.
3. When prompted on your phone screen, select **Allow USB debugging**.
4. In Unity's **Build Settings**, select your connected device in **Run Device** and click **Build and Run**.

### Option B: Manual Sideloading (Transfer APK)
1. Copy the generated `SAN_AR_Dragon.apk` to your phone (via USB file transfer, Google Drive, or messaging).
2. On your phone, tap the APK in your file manager to install it.
3. If prompted, allow "Install unknown apps" from your file manager.

---

## 5. Testing the Gameplay

1. **Launch the Game:**
   - Tap the **SAN AR Dragon** app icon on your phone.
   - You will see the dark fantasy start screen with the title, gold/red accents, **START AR**, **INSTRUCTIONS**, and **EXIT** buttons.
2. **Grant Camera Permission:**
   - Tap **START AR**.
   - When Android prompts for Camera Permission, select **Allow** or **While using the app**.
3. **Display the Reference Image:**
   - Open the image file located at:
     `D:\SAN AR Dragon\SAN AR Dragon\Assets\SAN_AR\Images\ReferenceImages\DragonReference.png`
   - Display this image full screen on your laptop, monitor, or print it out on paper.
4. **Scan and Control the Dragon:**
   - Aim your phone's rear camera at the dragon reference image.
   - Within seconds, the tracking dot turns **Green**, the HUD banner announces:
     *“Dragon Found! Use the joystick to move.”*
   - The 3D Dragon appears right above the reference image.
   - Touch and drag the **on-screen joystick** at the bottom-left to steer the dragon around your environment in real-world space.
   - Press **RESET DRAGON** at the bottom-right to immediately call the dragon back to the reference image marker.
   - Press **MENU** at the top-right to return to the main menu.
