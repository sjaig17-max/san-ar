using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using System.IO;
using SAN_AR.AR;
using SAN_AR.Dragon;
using SAN_AR.UI;

namespace SAN_AR.Editor
{
    public static class SANProjectBuilder
    {
        [MenuItem("SAN AR Dragon/1. Build Entire Project (Scenes, Prefabs, Setup)")]
        public static void BuildEntireProject()
        {
            Debug.Log("[SANProjectBuilder] Starting Automated Setup for SAN AR Dragon...");

            ConfigureAndroidProjectSettings();
            Material dragonMat = CreateDragonMaterial();
            GameObject dragonPrefab = BuildDragonPrefab(dragonMat);
            XRReferenceImageLibrary imageLibrary = BuildReferenceImageLibrary();
            BuildMainMenuScene();
            BuildARGameplayScene(dragonPrefab, imageLibrary);
            UpdateBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[SANProjectBuilder] === SAN AR Dragon Build Setup Completed Successfully! ===");
        }

        [MenuItem("SAN AR Dragon/2. Configure Android Build Settings")]
        public static void ConfigureAndroidProjectSettings()
        {
            PlayerSettings.productName = "SAN AR Dragon";
            PlayerSettings.companyName = "SAN Games";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.san.ardragon");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel28;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            Debug.Log("[SANProjectBuilder] Configured Android Player Settings: com.san.ardragon");
        }

        private static Material CreateDragonMaterial()
        {
            string matPath = "Assets/SAN_AR/Materials/DragonMaterial.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                mat.color = new Color(0.85f, 0.15f, 0.15f, 1f); // Crimson Red
                mat.SetFloat("_Smoothness", 0.6f);
                mat.SetFloat("_Metallic", 0.4f);
                AssetDatabase.CreateAsset(mat, matPath);
                Debug.Log("[SANProjectBuilder] Created DragonMaterial at " + matPath);
            }
            return mat;
        }

        private static GameObject BuildDragonPrefab(Material dragonMat)
        {
            string prefabPath = "Assets/SAN_AR/Prefabs/DragonPrefab.prefab";
            GameObject dragonRoot = new GameObject("DragonPrefab");

            // Attach dragon movement & animation controller
            DragonMovement movement = dragonRoot.AddComponent<DragonMovement>();
            DragonAnimationController animCtrl = dragonRoot.AddComponent<DragonAnimationController>();

            // Child container for procedural animations
            GameObject bodyContainer = new GameObject("BodyContainer");
            bodyContainer.transform.SetParent(dragonRoot.transform, false);

            // Check if DragonModel.obj exists
            string modelPath = "Assets/SAN_AR/Models/DragonModel.obj";
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

            if (modelAsset != null)
            {
                GameObject visual = Object.Instantiate(modelAsset, bodyContainer.transform);
                visual.name = "DragonVisual";
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = Vector3.one;

                Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers)
                {
                    r.sharedMaterial = dragonMat;
                }
            }
            else
            {
                // Fallback procedural visual representation if mesh is pending import
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = "DragonBodyMesh";
                cube.transform.SetParent(bodyContainer.transform, false);
                cube.transform.localScale = new Vector3(0.4f, 0.3f, 0.7f);
                cube.GetComponent<Renderer>().sharedMaterial = dragonMat;
            }

            // Save Prefab
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(dragonRoot, prefabPath);
            Object.DestroyImmediate(dragonRoot);
            Debug.Log("[SANProjectBuilder] Created Dragon Prefab at " + prefabPath);
            return savedPrefab;
        }

        private static XRReferenceImageLibrary BuildReferenceImageLibrary()
        {
            string libPath = "Assets/SAN_AR/Images/ReferenceImages/DragonImageLibrary.asset";
            XRReferenceImageLibrary lib = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(libPath);

            if (lib == null)
            {
                lib = ScriptableObject.CreateInstance<XRReferenceImageLibrary>();
                AssetDatabase.CreateAsset(lib, libPath);
            }

            string texturePath = "Assets/SAN_AR/Images/ReferenceImages/DragonReference.png";
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            EditorUtility.SetDirty(lib);
            AssetDatabase.SaveAssets();
            Debug.Log("[SANProjectBuilder] Reference Image Library saved at " + libPath);
            return lib;
        }

        private static void BuildMainMenuScene()
        {
            string scenePath = "Assets/SAN_AR/Scenes/MainMenu.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera & Light
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.05f, 0.1f);
            camObj.AddComponent<AudioListener>();

            // 2. EventSystem
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // 3. Canvas
            GameObject canvasObj = new GameObject("MainMenuCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();
            MainMenuController menuController = canvasObj.AddComponent<MainMenuController>();

            // 4. Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.06f, 0.12f, 1f);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            string bgTexPath = "Assets/SAN_AR/Images/MenuBackground.png";
            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgTexPath);
            if (bgSprite != null)
            {
                bgImg.sprite = bgSprite;
                bgImg.color = Color.white;
            }

            // 5. Title Text
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(canvasObj.transform, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "SAN AR DRAGON";
            titleText.fontSize = 72;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(1f, 0.85f, 0.2f); // Gold
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 480);
            titleRect.sizeDelta = new Vector2(900, 160);

            // Subtitle
            GameObject subObj = new GameObject("SubtitleText");
            subObj.transform.SetParent(canvasObj.transform, false);
            TextMeshProUGUI subText = subObj.AddComponent<TextMeshProUGUI>();
            subText.text = "IMAGE TRACKING AUGMENTED REALITY";
            subText.fontSize = 28;
            subText.alignment = TextAlignmentOptions.Center;
            subText.color = new Color(0.9f, 0.3f, 0.3f); // Crimson Accent
            RectTransform subRect = subObj.GetComponent<RectTransform>();
            subRect.anchoredPosition = new Vector2(0, 400);
            subRect.sizeDelta = new Vector2(900, 60);

            // 6. Buttons
            Button startBtn = CreateMenuButton(canvasObj.transform, "StartButton", "START AR", new Vector2(0, 20), new Color(0.75f, 0.15f, 0.15f));
            Button instBtn = CreateMenuButton(canvasObj.transform, "InstructionsButton", "INSTRUCTIONS", new Vector2(0, -140), new Color(0.2f, 0.2f, 0.25f));
            Button exitBtn = CreateMenuButton(canvasObj.transform, "ExitButton", "EXIT", new Vector2(0, -300), new Color(0.15f, 0.15f, 0.18f));

            // 7. Instructions Modal Panel
            GameObject instPanel = new GameObject("InstructionsPanel");
            instPanel.transform.SetParent(canvasObj.transform, false);
            Image panelImg = instPanel.AddComponent<Image>();
            panelImg.color = new Color(0.05f, 0.05f, 0.08f, 0.96f);
            RectTransform panelRect = instPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.15f);
            panelRect.anchorMax = new Vector2(0.9f, 0.85f);
            panelRect.sizeDelta = Vector2.zero;

            GameObject instTitleObj = new GameObject("PanelTitle");
            instTitleObj.transform.SetParent(instPanel.transform, false);
            TextMeshProUGUI pTitle = instTitleObj.AddComponent<TextMeshProUGUI>();
            pTitle.text = "HOW TO PLAY";
            pTitle.fontSize = 54;
            pTitle.alignment = TextAlignmentOptions.Center;
            pTitle.color = new Color(1f, 0.85f, 0.2f);
            RectTransform pTitleRect = instTitleObj.GetComponent<RectTransform>();
            pTitleRect.anchoredPosition = new Vector2(0, 380);
            pTitleRect.sizeDelta = new Vector2(700, 100);

            GameObject instBodyObj = new GameObject("PanelBody");
            instBodyObj.transform.SetParent(instPanel.transform, false);
            TextMeshProUGUI pBody = instBodyObj.AddComponent<TextMeshProUGUI>();
            pBody.text = "1. Press 'START AR' to open your camera.\n\n" +
                         "2. Open the dragon reference image on your laptop screen or print it on paper.\n\n" +
                         "3. Point your phone camera at the reference image.\n\n" +
                         "4. The 3D Dragon will spawn in real world space at the image location!\n\n" +
                         "5. Use the on-screen joystick on the bottom-left to move and turn your dragon.\n\n" +
                         "6. Press 'Reset Dragon' anytime to bring the dragon back to the image.";
            pBody.fontSize = 32;
            pBody.color = Color.white;
            RectTransform pBodyRect = instBodyObj.GetComponent<RectTransform>();
            pBodyRect.anchoredPosition = new Vector2(0, 40);
            pBodyRect.sizeDelta = new Vector2(720, 560);

            Button closeBtn = CreateMenuButton(instPanel.transform, "CloseButton", "CLOSE", new Vector2(0, -380), new Color(0.75f, 0.15f, 0.15f));

            // Wire serialized fields on MainMenuController via SerializedObject
            SerializedObject so = new SerializedObject(menuController);
            so.FindProperty("startARButton").objectReferenceValue = startBtn;
            so.FindProperty("instructionsButton").objectReferenceValue = instBtn;
            so.FindProperty("closeInstructionsButton").objectReferenceValue = closeBtn;
            so.FindProperty("exitButton").objectReferenceValue = exitBtn;
            so.FindProperty("instructionsPanel").objectReferenceValue = instPanel;
            so.FindProperty("arSceneName").stringValue = "ARGameplay";
            so.ApplyModifiedProperties();

            instPanel.SetActive(false);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log("[SANProjectBuilder] MainMenu Scene saved to " + scenePath);
        }

        private static Button CreateMenuButton(Transform parent, string name, string label, Vector2 pos, Color btnColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            Image img = btnObj.AddComponent<Image>();
            img.color = btnColor;
            Button btn = btnObj.AddComponent<Button>();
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(480, 110);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.text = label;
            txt.fontSize = 36;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
            RectTransform txtRect = textObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;

            return btn;
        }

        private static void BuildARGameplayScene(GameObject dragonPrefab, XRReferenceImageLibrary imageLibrary)
        {
            string scenePath = "Assets/SAN_AR/Scenes/ARGameplay.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. AR Session
            GameObject arSessionObj = new GameObject("AR Session");
            arSessionObj.AddComponent<ARSession>();
            arSessionObj.AddComponent<ARInputManager>();

            // 2. XR Origin (AR Rig)
            GameObject xrOriginObj = new GameObject("XR Origin");
            Unity.XR.CoreUtils.XROrigin origin = xrOriginObj.AddComponent<Unity.XR.CoreUtils.XROrigin>();
            ARTrackedImageManager imageManager = xrOriginObj.AddComponent<ARTrackedImageManager>();
            ARImageTrackingController trackingController = xrOriginObj.AddComponent<ARImageTrackingController>();

            if (imageLibrary != null)
            {
                imageManager.referenceLibrary = imageLibrary;
            }

            GameObject cameraOffsetObj = new GameObject("Camera Offset");
            cameraOffsetObj.transform.SetParent(xrOriginObj.transform, false);
            origin.CameraFloorOffsetObject = cameraOffsetObj;

            GameObject mainCamObj = new GameObject("Main Camera");
            mainCamObj.transform.SetParent(cameraOffsetObj.transform, false);
            mainCamObj.tag = "MainCamera";
            Camera arCam = mainCamObj.AddComponent<Camera>();
            arCam.clearFlags = CameraClearFlags.SolidColor;
            arCam.backgroundColor = Color.black;
            mainCamObj.AddComponent<ARCameraManager>();
            mainCamObj.AddComponent<ARCameraBackground>();
            mainCamObj.AddComponent<AudioListener>();
            origin.Camera = arCam;

            // Directional Light
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1.2f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 3. UI Canvas & EventSystem
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            GameObject canvasObj = new GameObject("GameplayCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();
            GameplayUIController gameplayUI = canvasObj.AddComponent<GameplayUIController>();

            // HUD Top Banner
            GameObject bannerObj = new GameObject("TopBanner");
            bannerObj.transform.SetParent(canvasObj.transform, false);
            Image bannerImg = bannerObj.AddComponent<Image>();
            bannerImg.color = new Color(0.08f, 0.08f, 0.12f, 0.85f);
            RectTransform bannerRect = bannerObj.GetComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0, 1);
            bannerRect.anchorMax = new Vector2(1, 1);
            bannerRect.pivot = new Vector2(0.5f, 1);
            bannerRect.anchoredPosition = Vector2.zero;
            bannerRect.sizeDelta = new Vector2(0, 220);

            // Title
            GameObject titleObj = new GameObject("GameTitle");
            titleObj.transform.SetParent(bannerObj.transform, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "SAN AR DRAGON";
            titleText.fontSize = 42;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(1f, 0.85f, 0.2f);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 50);
            titleRect.sizeDelta = new Vector2(600, 60);

            // Tracking Indicator Dot
            GameObject dotObj = new GameObject("TrackingDot");
            dotObj.transform.SetParent(bannerObj.transform, false);
            Image dotImg = dotObj.AddComponent<Image>();
            dotImg.color = new Color(0.9f, 0.3f, 0.2f);
            RectTransform dotRect = dotObj.GetComponent<RectTransform>();
            dotRect.anchoredPosition = new Vector2(-380, -20);
            dotRect.sizeDelta = new Vector2(30, 30);

            // Status Text
            GameObject statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(bannerObj.transform, false);
            TextMeshProUGUI statusTxt = statusObj.AddComponent<TextMeshProUGUI>();
            statusTxt.text = "Point your camera at the dragon image.";
            statusTxt.fontSize = 28;
            statusTxt.alignment = TextAlignmentOptions.Center;
            statusTxt.color = Color.white;
            RectTransform statusRect = statusObj.GetComponent<RectTransform>();
            statusRect.anchoredPosition = new Vector2(30, -20);
            statusRect.sizeDelta = new Vector2(760, 60);

            // Back to Menu Button (Top Right)
            GameObject menuBtnObj = new GameObject("MenuButton");
            menuBtnObj.transform.SetParent(bannerObj.transform, false);
            Image menuBtnImg = menuBtnObj.AddComponent<Image>();
            menuBtnImg.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);
            Button menuBtn = menuBtnObj.AddComponent<Button>();
            RectTransform menuBtnRect = menuBtnObj.GetComponent<RectTransform>();
            menuBtnRect.anchorMin = new Vector2(1, 1);
            menuBtnRect.anchorMax = new Vector2(1, 1);
            menuBtnRect.pivot = new Vector2(1, 1);
            menuBtnRect.anchoredPosition = new Vector2(-30, -30);
            menuBtnRect.sizeDelta = new Vector2(150, 75);

            GameObject menuBtnText = new GameObject("Text");
            menuBtnText.transform.SetParent(menuBtnObj.transform, false);
            TextMeshProUGUI menuTxt = menuBtnText.AddComponent<TextMeshProUGUI>();
            menuTxt.text = "MENU";
            menuTxt.fontSize = 26;
            menuTxt.alignment = TextAlignmentOptions.Center;
            menuTxt.color = Color.white;
            RectTransform mTxtRect = menuBtnText.GetComponent<RectTransform>();
            mTxtRect.anchorMin = Vector2.zero;
            mTxtRect.anchorMax = Vector2.one;
            mTxtRect.sizeDelta = Vector2.zero;

            // Reset Dragon Button (Bottom Right)
            GameObject resetBtnObj = new GameObject("ResetDragonButton");
            resetBtnObj.transform.SetParent(canvasObj.transform, false);
            Image resetBtnImg = resetBtnObj.AddComponent<Image>();
            resetBtnImg.color = new Color(0.75f, 0.15f, 0.15f, 0.9f);
            Button resetBtn = resetBtnObj.AddComponent<Button>();
            RectTransform resetBtnRect = resetBtnObj.GetComponent<RectTransform>();
            resetBtnRect.anchorMin = new Vector2(1, 0);
            resetBtnRect.anchorMax = new Vector2(1, 0);
            resetBtnRect.pivot = new Vector2(1, 0);
            resetBtnRect.anchoredPosition = new Vector2(-60, 60);
            resetBtnRect.sizeDelta = new Vector2(240, 90);

            GameObject resetBtnText = new GameObject("Text");
            resetBtnText.transform.SetParent(resetBtnObj.transform, false);
            TextMeshProUGUI resetTxt = resetBtnText.AddComponent<TextMeshProUGUI>();
            resetTxt.text = "RESET DRAGON";
            resetTxt.fontSize = 24;
            resetTxt.alignment = TextAlignmentOptions.Center;
            resetTxt.color = Color.white;
            RectTransform rTxtRect = resetBtnText.GetComponent<RectTransform>();
            rTxtRect.anchorMin = Vector2.zero;
            rTxtRect.anchorMax = Vector2.one;
            rTxtRect.sizeDelta = Vector2.zero;

            // 4. On-Screen Joystick (Bottom Left)
            GameObject joystickObj = new GameObject("DragonJoystick");
            joystickObj.transform.SetParent(canvasObj.transform, false);
            DragonJoystick joystickComp = joystickObj.AddComponent<DragonJoystick>();
            Image joyBgImg = joystickObj.AddComponent<Image>();
            joyBgImg.color = new Color(1f, 1f, 1f, 0.18f);
            RectTransform joyRect = joystickObj.GetComponent<RectTransform>();
            joyRect.anchorMin = new Vector2(0, 0);
            joyRect.anchorMax = new Vector2(0, 0);
            joyRect.pivot = new Vector2(0, 0);
            joyRect.anchoredPosition = new Vector2(70, 70);
            joyRect.sizeDelta = new Vector2(260, 260);

            GameObject joyHandleObj = new GameObject("Handle");
            joyHandleObj.transform.SetParent(joystickObj.transform, false);
            Image handleImg = joyHandleObj.AddComponent<Image>();
            handleImg.color = new Color(0.85f, 0.2f, 0.2f, 0.8f); // Red knob
            RectTransform handleRect = joyHandleObj.GetComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0.5f, 0.5f);
            handleRect.anchorMax = new Vector2(0.5f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.sizeDelta = new Vector2(110, 110);

            // Connect Joystick fields
            SerializedObject joySO = new SerializedObject(joystickComp);
            joySO.FindProperty("background").objectReferenceValue = joyRect;
            joySO.FindProperty("handle").objectReferenceValue = handleRect;
            joySO.FindProperty("handleRange").floatValue = 100f;
            joySO.ApplyModifiedProperties();

            // Connect GameplayUIController fields
            SerializedObject uiSO = new SerializedObject(gameplayUI);
            uiSO.FindProperty("statusText").objectReferenceValue = statusTxt;
            uiSO.FindProperty("trackingIndicatorDot").objectReferenceValue = dotImg;
            uiSO.FindProperty("resetButton").objectReferenceValue = resetBtn;
            uiSO.FindProperty("menuButton").objectReferenceValue = menuBtn;
            uiSO.FindProperty("trackingController").objectReferenceValue = trackingController;
            uiSO.FindProperty("mainMenuSceneName").stringValue = "MainMenu";
            uiSO.ApplyModifiedProperties();

            // Connect ARImageTrackingController fields
            SerializedObject tcSO = new SerializedObject(trackingController);
            tcSO.FindProperty("dragonPrefab").objectReferenceValue = dragonPrefab;
            tcSO.FindProperty("targetImageName").stringValue = "DragonReference";
            tcSO.FindProperty("gameplayUI").objectReferenceValue = gameplayUI;
            tcSO.FindProperty("joystick").objectReferenceValue = joystickComp;
            tcSO.FindProperty("arCamera").objectReferenceValue = arCam;
            tcSO.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log("[SANProjectBuilder] ARGameplay Scene saved to " + scenePath);
        }

        private static void UpdateBuildSettings()
        {
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/SAN_AR/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/SAN_AR/Scenes/ARGameplay.unity", true)
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log("[SANProjectBuilder] Updated EditorBuildSettings with MainMenu and ARGameplay scenes.");
        }
    }
}
