using UnityEngine;

namespace DreadScripts.Common.SupportThankies
{
	internal sealed class ColoredScope : System.IDisposable
    {
        internal enum ColoringType
        {
            BG = 1 << 0,
            FG = 1 << 1,
            General = 1 << 2,
            All = BG | FG | General
        }

        private readonly Color[] ogColors = new Color[3];
        private readonly ColoringType coloringType;
        private bool changedAnyColor;

        private void MemorizeColor()
        {
            changedAnyColor = true;
            ogColors[0] = GUI.backgroundColor;
            ogColors[1] = GUI.contentColor;
            ogColors[2] = GUI.color;
        }

        private void SetColors(Color color)
        {
            MemorizeColor();

            if (coloringType.HasFlag(ColoringType.BG))
                GUI.backgroundColor = color;

            if (coloringType.HasFlag(ColoringType.FG))
                GUI.contentColor = color;

            if (coloringType.HasFlag(ColoringType.General))
                GUI.color = color;
        }

        internal ColoredScope(ColoringType type, Color color)
        {
            coloringType = type;
            SetColors(color);
        }

        public void Dispose()
        {
            if (!changedAnyColor) return;

            if (coloringType.HasFlag(ColoringType.BG))
                GUI.backgroundColor = ogColors[0];
            if (coloringType.HasFlag(ColoringType.FG))
                GUI.contentColor = ogColors[1];
            if (coloringType.HasFlag(ColoringType.General))
                GUI.color = ogColors[2];
        }
    }
}
