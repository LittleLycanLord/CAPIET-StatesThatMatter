using System.Text.RegularExpressions;
using LilLycanLord_Official;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace LilLycanLord_Official
{
    internal struct ColorData
    {
        public Color backgroundColor;
        public Color textColor;

        public ColorData(Color bgColor, Color txtColor)
        {
            backgroundColor = bgColor;
            textColor = txtColor;
        }
    }

    [UnityEditor.InitializeOnLoad]
    public class ColoredHierarchy
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        private static Vector2 offset = new Vector2(20, 1);
        private static readonly string commandPrefix = "/color.";

        // Regex pattern for hex color codes (#RRGGBB or #RGB)
        private static readonly Regex hexColorPattern = new Regex(
            @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$"
        );

        // Predefined color dictionary for better performance and maintainability
        private static readonly System.Collections.Generic.Dictionary<
            string,
            ColorData
        > predefinedColors = new System.Collections.Generic.Dictionary<string, ColorData>
        {
            {
                "Red",
                new ColorData(new Color(0.92f, 0.47f, 0.47f), new Color(0.61f, 0.31f, 0.31f))
            },
            {
                "Orange",
                new ColorData(new Color(0.94f, 0.67f, 0.49f), new Color(0.62f, 0.45f, 0.33f))
            },
            {
                "Yellow",
                new ColorData(new Color(1.00f, 0.90f, 0.65f), new Color(0.67f, 0.60f, 0.43f))
            },
            {
                "Green",
                new ColorData(new Color(0.55f, 0.85f, 0.52f), new Color(0.37f, 0.56f, 0.35f))
            },
            {
                "Blue",
                new ColorData(new Color(0.52f, 0.70f, 0.80f), new Color(0.35f, 0.47f, 0.53f))
            },
            {
                "Purple",
                new ColorData(new Color(0.51f, 0.51f, 0.77f), new Color(0.34f, 0.34f, 0.51f))
            },
            {
                "Violet",
                new ColorData(new Color(0.51f, 0.51f, 0.77f), new Color(0.34f, 0.34f, 0.51f))
            },
            { "Black", new ColorData(new Color(0.1f, 0.1f, 0.1f), new Color(0.9f, 0.9f, 0.9f)) },
            {
                "Grey",
                new ColorData(new Color(0.22f, 0.22f, 0.24f), new Color(0.16f, 0.18f, 0.18f))
            },
            {
                "Gray",
                new ColorData(new Color(0.22f, 0.22f, 0.24f), new Color(0.16f, 0.18f, 0.18f))
            },
            { "White", new ColorData(new Color(1.0f, 1.0f, 1.0f), new Color(0.1f, 0.1f, 0.1f)) },
            // Additional common colors
            {
                "Pink",
                new ColorData(new Color(0.93f, 0.51f, 0.93f), new Color(0.62f, 0.34f, 0.62f))
            },
            {
                "Cyan",
                new ColorData(new Color(0.25f, 0.88f, 0.82f), new Color(0.17f, 0.58f, 0.55f))
            },
            {
                "Magenta",
                new ColorData(new Color(0.80f, 0.20f, 0.60f), new Color(0.53f, 0.13f, 0.40f))
            },
            {
                "Lime",
                new ColorData(new Color(0.75f, 1.00f, 0.00f), new Color(0.50f, 0.67f, 0.00f))
            },
            {
                "Indigo",
                new ColorData(new Color(0.29f, 0.00f, 0.51f), new Color(0.19f, 0.00f, 0.34f))
            },
            {
                "Teal",
                new ColorData(new Color(0.00f, 0.50f, 0.50f), new Color(0.00f, 0.33f, 0.33f))
            },
            {
                "Brown",
                new ColorData(new Color(0.65f, 0.40f, 0.20f), new Color(0.43f, 0.27f, 0.13f))
            },
            {
                "Maroon",
                new ColorData(new Color(0.50f, 0.00f, 0.00f), new Color(0.33f, 0.00f, 0.00f))
            },
            {
                "Navy",
                new ColorData(new Color(0.00f, 0.00f, 0.50f), new Color(0.00f, 0.00f, 0.33f))
            },
            {
                "Olive",
                new ColorData(new Color(0.50f, 0.50f, 0.00f), new Color(0.33f, 0.33f, 0.00f))
            },
        };

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔══════════════╗
        //* ║ Monobehaviour ║
        //* ╚══════════════╝

        static ColoredHierarchy()
        {
            EditorApplication.hierarchyWindowItemOnGUI += ColorHierarchyObject;
        }

        private static void ColorHierarchyObject(int instanceID, Rect selectionRect)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            Object hierarchyObject = EditorUtility.InstanceIDToObject(instanceID);
#pragma warning restore CS0618 // Type or member is obsolete
            if (hierarchyObject == null || !hierarchyObject.name.StartsWith(commandPrefix))
                return;

            var colorInfo = ParseColorFromName(hierarchyObject.name);
            if (colorInfo.HasValue)
            {
                DrawColoredHierarchyItem(selectionRect, hierarchyObject.name, colorInfo.Value);
            }
        }

        private static ColorData? ParseColorFromName(string objectName)
        {
            if (!objectName.StartsWith(commandPrefix))
                return null;

            // Extract the color part (everything between commandPrefix and first space)
            string colorPart = objectName.Substring(commandPrefix.Length);
            int spaceIndex = colorPart.IndexOf(' ');
            if (spaceIndex == -1)
                return null; // No space found, invalid format

            string colorIdentifier = colorPart.Substring(0, spaceIndex).Trim();

            // Check if it's a hex color
            if (hexColorPattern.IsMatch(colorIdentifier))
            {
                return ParseHexColor(colorIdentifier);
            }

            // Check if it's a predefined color
            if (predefinedColors.TryGetValue(colorIdentifier, out ColorData colorData))
            {
                return colorData;
            }

            return null; // Color not found
        }

        private static ColorData ParseHexColor(string hexColor)
        {
            Color backgroundColor = Color.white;

            if (ColorUtility.TryParseHtmlString(hexColor, out backgroundColor))
            {
                // Generate contrasting text color based on luminance
                Color textColor = GetContrastingTextColor(backgroundColor);
                return new ColorData(backgroundColor, textColor);
            }

            // Fallback to white background if parsing fails
            return new ColorData(Color.white, Color.black);
        }

        private static Color GetContrastingTextColor(Color backgroundColor)
        {
            // Calculate relative luminance using sRGB coefficients
            float luminance =
                0.299f * backgroundColor.r
                + 0.587f * backgroundColor.g
                + 0.114f * backgroundColor.b;

            // Return black text for light backgrounds, white text for dark backgrounds
            if (luminance > 0.5f)
            {
                // Light background - use darker text
                return new Color(
                    backgroundColor.r * 0.3f,
                    backgroundColor.g * 0.3f,
                    backgroundColor.b * 0.3f,
                    1.0f
                );
            }
            else
            {
                // Dark background - use lighter text
                return new Color(
                    Mathf.Min(1.0f, backgroundColor.r + 0.6f),
                    Mathf.Min(1.0f, backgroundColor.g + 0.6f),
                    Mathf.Min(1.0f, backgroundColor.b + 0.6f),
                    1.0f
                );
            }
        }

        private static void DrawColoredHierarchyItem(
            Rect selectionRect,
            string objectName,
            ColorData colorData
        )
        {
            // Extract display name (remove color prefix)
            string displayName = GetDisplayName(objectName);

            // Create rects for background and text
            Rect offsetRect = new Rect(selectionRect.position + offset, selectionRect.size);
            Rect bgRect = new Rect(
                selectionRect.x,
                selectionRect.y,
                selectionRect.width + 50,
                selectionRect.height
            );

            // Draw background
            EditorGUI.DrawRect(bgRect, colorData.backgroundColor);

            // Draw text with custom style
            GUIStyle labelStyle = new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = colorData.textColor },
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
            };

            EditorGUI.LabelField(offsetRect, displayName, labelStyle);
        }

        private static string GetDisplayName(string objectName)
        {
            if (!objectName.StartsWith(commandPrefix))
                return objectName;

            string afterPrefix = objectName.Substring(commandPrefix.Length);
            int spaceIndex = afterPrefix.IndexOf(' ');

            if (spaceIndex == -1)
                return objectName; // No space found, return original name

            return afterPrefix.Substring(spaceIndex + 1);
        }

        public static string[] GetAvailableColors()
        {
            var colors = new string[predefinedColors.Count];
            predefinedColors.Keys.CopyTo(colors, 0);
            return colors;
        }

        public static bool IsValidColorIdentifier(string colorIdentifier)
        {
            if (string.IsNullOrEmpty(colorIdentifier))
                return false;

            // Check predefined colors
            if (predefinedColors.ContainsKey(colorIdentifier))
                return true;

            // Check hex pattern
            if (hexColorPattern.IsMatch(colorIdentifier))
            {
                return ColorUtility.TryParseHtmlString(colorIdentifier, out _);
            }

            return false;
        }

        public static string FormatColoredName(string colorIdentifier, string objectName)
        {
            if (!IsValidColorIdentifier(colorIdentifier))
                return objectName;

            return $"{commandPrefix}{colorIdentifier} {objectName}";
        }
    }
}
#endif
