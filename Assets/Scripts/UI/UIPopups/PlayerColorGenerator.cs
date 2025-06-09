using UnityEngine;

namespace UI.UIPopups
{
    public static class PlayerColorGenerator
    {
        private static readonly Color[] PlayerColors = 
        {
            new Color(1f, 0.3f, 0.3f),       // Red
            new Color(0.3f, 0.6f, 1f),       // Blue
            new Color(0.3f, 1f, 0.3f),       // Green
            new Color(1f, 0.8f, 0.2f),       // Yellow
            new Color(1f, 0.4f, 1f),         // Magenta
            new Color(0.2f, 1f, 1f),         // Cyan
            new Color(1f, 0.6f, 0.2f),       // Orange
            new Color(0.8f, 0.3f, 1f),       // Purple
            new Color(1f, 0.7f, 0.8f),       // Pink
            new Color(0.6f, 0.9f, 0.3f)      // Lime
        };

        public static Color GetRandomPlayerColor()
        {
            int randomIndex = Random.Range(0, PlayerColors.Length);
            return PlayerColors[randomIndex];
        }

        public static string ColorToHex(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }

        public static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString($"#{hex}", out Color color))
            {
                return color;
            }
            return Color.white;
        }
    }
}