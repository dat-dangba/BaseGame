using UnityEngine;

namespace DBD.BaseGame
{
    public class SafeArea : BaseMonoBehaviour
    {
        #region Simulations

        /// <summary>
        /// Simulation device that uses safe area due to a physical notch or software home bar. For use in Editor only.
        /// </summary>
        public enum SimDevice
        {
            /// <summary>
            /// Don't use a simulated safe area - GUI will be full screen as normal.
            /// </summary>
            None,

            /// <summary>
            /// Simulate the iPhone X and Xs (identical safe areas).
            /// </summary>
            iPhoneX,

            /// <summary>
            /// Simulate the iPhone Xs Max and XR (identical safe areas).
            /// </summary>
            iPhoneXsMax,

            /// <summary>
            /// Simulate the Google Pixel 3 XL using landscape left.
            /// </summary>
            Pixel3XL_LSL,

            /// <summary>
            /// Simulate the Google Pixel 3 XL using landscape right.
            /// </summary>
            Pixel3XL_LSR
        }

        /// <summary>
        /// Simulation mode for use in editor only. This can be edited at runtime to toggle between different safe areas.
        /// </summary>
        public static SimDevice Sim = SimDevice.None;

        /// <summary>
        /// Normalised safe areas for iPhone X with Home indicator (ratios are identical to Xs, 11 Pro). Absolute values:
        ///  PortraitU x=0, y=102, w=1125, h=2202 on full extents w=1125, h=2436;
        ///  PortraitD x=0, y=102, w=1125, h=2202 on full extents w=1125, h=2436 (not supported, remains in Portrait Up);
        ///  LandscapeL x=132, y=63, w=2172, h=1062 on full extents w=2436, h=1125;
        ///  LandscapeR x=132, y=63, w=2172, h=1062 on full extents w=2436, h=1125.
        ///  Aspect Ratio: ~19.5:9.
        /// </summary>
        private Rect[] NSA_iPhoneX =
        {
            new(0f, 102f / 2436f, 1f, 2202f / 2436f), // Portrait
            new(132f / 2436f, 63f / 1125f, 2172f / 2436f, 1062f / 1125f) // Landscape
        };

        /// <summary>
        /// Normalised safe areas for iPhone Xs Max with Home indicator (ratios are identical to XR, 11, 11 Pro Max). Absolute values:
        ///  PortraitU x=0, y=102, w=1242, h=2454 on full extents w=1242, h=2688;
        ///  PortraitD x=0, y=102, w=1242, h=2454 on full extents w=1242, h=2688 (not supported, remains in Portrait Up);
        ///  LandscapeL x=132, y=63, w=2424, h=1179 on full extents w=2688, h=1242;
        ///  LandscapeR x=132, y=63, w=2424, h=1179 on full extents w=2688, h=1242.
        ///  Aspect Ratio: ~19.5:9.
        /// </summary>
        private Rect[] NSA_iPhoneXsMax =
        {
            new(0f, 102f / 2688f, 1f, 2454f / 2688f), // Portrait
            new(132f / 2688f, 63f / 1242f, 2424f / 2688f, 1179f / 1242f) // Landscape
        };

        /// <summary>
        /// Normalised safe areas for Pixel 3 XL using landscape left. Absolute values:
        ///  PortraitU x=0, y=0, w=1440, h=2789 on full extents w=1440, h=2960;
        ///  PortraitD x=0, y=0, w=1440, h=2789 on full extents w=1440, h=2960;
        ///  LandscapeL x=171, y=0, w=2789, h=1440 on full extents w=2960, h=1440;
        ///  LandscapeR x=0, y=0, w=2789, h=1440 on full extents w=2960, h=1440.
        ///  Aspect Ratio: 18.5:9.
        /// </summary>
        private Rect[] NSA_Pixel3XL_LSL =
        {
            new(0f, 0f, 1f, 2789f / 2960f), // Portrait
            new(0f, 0f, 2789f / 2960f, 1f) // Landscape
        };

        /// <summary>
        /// Normalised safe areas for Pixel 3 XL using landscape right. Absolute values and aspect ratio same as above.
        /// </summary>
        private Rect[] NSA_Pixel3XL_LSR =
        {
            new Rect(0f, 0f, 1f, 2789f / 2960f), // Portrait
            new Rect(171f / 2960f, 0f, 2789f / 2960f, 1f) // Landscape
        };

        #endregion

        private RectTransform panel;
        private Rect lastSafeArea = new(0, 0, 0, 0);
        private Vector2Int lastScreenSize = new(0, 0);
        private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

        [SerializeField]
        private bool conformX = true; // Conform to screen safe area on X-axis (default true, disable to ignore)

        [SerializeField]
        private bool conformY = true; // Conform to screen safe area on Y-axis (default true, disable to ignore)

        [SerializeField]
        private bool top = true; // Conform to screen safe area on Y-axis top (default true, disable to ignore)

        [SerializeField]
        private bool bot = true; // Conform to screen safe area on Y-axis bot (default true, disable to ignore)

        [SerializeField]
        private bool logging; // Conform to screen safe area on Y-axis (default true, disable to ignore)

        protected override void Awake()
        {
            base.Awake();

            if (!TryGetComponent(out panel))
            {
                Debug.LogError("Cannot apply safe area - no RectTransform found on " + name);
                Destroy(gameObject);
            }

            Refresh();
        }

        protected override void Update()
        {
            base.Update();
            Refresh();
        }

        protected virtual void Refresh()
        {
            Rect safeArea = GetSafeArea();

            if (safeArea != lastSafeArea
                || Screen.width != lastScreenSize.x
                || Screen.height != lastScreenSize.y
                || Screen.orientation != lastOrientation)
            {
                // Fix for having auto-rotate off and manually forcing a screen orientation.
                // See https://forum.unity.com/threads/569236/#post-4473253 and https://forum.unity.com/threads/569236/page-2#post-5166467
                lastScreenSize.x = Screen.width;
                lastScreenSize.y = Screen.height;
                lastOrientation = Screen.orientation;

                ApplySafeArea(safeArea);
            }
        }

        private Rect GetSafeArea()
        {
            Rect safeArea = Screen.safeArea;

            if (Application.isEditor && Sim != SimDevice.None)
            {
                Rect nsa = new Rect(0, 0, Screen.width, Screen.height);

                switch (Sim)
                {
                    case SimDevice.iPhoneX:
                        if (Screen.height > Screen.width) // Portrait
                            nsa = NSA_iPhoneX[0];
                        else // Landscape
                            nsa = NSA_iPhoneX[1];
                        break;
                    case SimDevice.iPhoneXsMax:
                        if (Screen.height > Screen.width) // Portrait
                            nsa = NSA_iPhoneXsMax[0];
                        else // Landscape
                            nsa = NSA_iPhoneXsMax[1];
                        break;
                    case SimDevice.Pixel3XL_LSL:
                        if (Screen.height > Screen.width) // Portrait
                            nsa = NSA_Pixel3XL_LSL[0];
                        else // Landscape
                            nsa = NSA_Pixel3XL_LSL[1];
                        break;
                    case SimDevice.Pixel3XL_LSR:
                        if (Screen.height > Screen.width) // Portrait
                            nsa = NSA_Pixel3XL_LSR[0];
                        else // Landscape
                            nsa = NSA_Pixel3XL_LSR[1];
                        break;
                    default:
                        break;
                }

                safeArea = new Rect(Screen.width * nsa.x, Screen.height * nsa.y, Screen.width * nsa.width,
                    Screen.height * nsa.height);
            }

            return safeArea;
        }

        protected virtual void ApplySafeArea(Rect r)
        {
            lastSafeArea = r;

            // Ignore x-axis?
            if (!conformX)
            {
                r.x = 0;
                r.width = Screen.width;
            }

            // Ignore y-axis?
            if (!conformY)
            {
                r.y = 0;
                r.height = Screen.height;
            }

            // Check for invalid screen startup state on some Samsung devices (see below)
            if (Screen.width > 0 && Screen.height > 0)
            {
                // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
                Vector2 anchorMin = r.position;
                Vector2 anchorMax = r.position + r.size;
                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;

                // Fix for some Samsung devices (e.g. Note 10+, A71, S20) where Refresh gets called twice and the first time returns NaN anchor coordinates
                // See https://forum.unity.com/threads/569236/page-2#post-6199352
                if (anchorMin.x >= 0 && anchorMin.y >= 0 && anchorMax.x >= 0 && anchorMax.y >= 0)
                {
                    //Panel.anchorMin = anchorMin;
                    //Panel.anchorMax = anchorMax;
                    panel.anchorMin = new Vector2(anchorMin.x, bot ? anchorMin.y : 0);
                    panel.anchorMax = new Vector2(anchorMax.x, top ? anchorMax.y : 1);
                    ApplySafeArea(panel);
                }
            }

            if (logging)
            {
                Debug.LogFormat("New safe area applied to {0}: x={1}, y={2}, w={3}, h={4} on full extents w={5}, h={6}",
                    name, r.x, r.y, r.width, r.height, Screen.width, Screen.height);
            }
        }

        protected virtual void ApplySafeArea(RectTransform safeRectTransform)
        {
        }
    }
}