using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DreadScripts.Common.SupportThankies
{
	internal static class Helper
	{
		internal static T GetRandom<T>(this T[] arr) => arr[Random.Range(0, arr.Length)];
		
		#region Rect Stuff
		public static Rect Shrink(this Rect r, float amount)
		{
			r.x += amount;
			r.y += amount;
			r.width -= amount * 2;
			r.height -= amount * 2;
			return r;
		}

		public static Rect AssertAspect(Rect r, float aspectRatio)
		{
			Rect newRect = r;
			if (r.width / r.height > aspectRatio)
			{
				newRect.width = r.height * aspectRatio;
				newRect.x += (r.width - newRect.width) / 2;
			}
			else
			{
				newRect.height = r.width / aspectRatio;
				newRect.y += (r.height - newRect.height) / 2;
			}

			return newRect;
		}
		#endregion

		#region Texture & Color
		internal static Color Overlayed(this Color target, Color overlay)
		{
			float a = overlay.a + target.a * (1 - overlay.a);
			float r = (overlay.r * overlay.a + target.r * target.a * (1 - overlay.a)) / a;
			float g = (overlay.g * overlay.a + target.g * target.a * (1 - overlay.a)) / a;
			float b = (overlay.b * overlay.a + target.b * target.a * (1 - overlay.a)) / a;
			return new Color(r, g, b, a);
		}
		
		internal static Rect DrawBorderedBox(Rect rect, Color boxColor = default, Color borderColor = default, float thickness = 3)
		{
			bool boxHasColor = boxColor != Color.clear;
			bool borderHasColor = borderColor != Color.clear;
			if (boxHasColor || borderHasColor)
			{
				float altThickness = thickness + 2;
				Rect altRect = rect;
				altRect.x -= altThickness / 2;
				altRect.width += altThickness;
				altRect.y -= altThickness / 2;
				altRect.height += altThickness;

				if (boxHasColor) GUI.DrawTexture(rect, TextureFromColor(boxColor), ScaleMode.StretchToFill, false, 0, boxColor, 0, 8);
				if (borderHasColor) GUI.DrawTexture(altRect, TextureFromColor(borderColor), ScaleMode.StretchToFill, false, 0, borderColor, thickness, 8);
			}			

			Rect layoutAreaRect = rect;
			layoutAreaRect.x += 4;
			layoutAreaRect.width -= 8;
			layoutAreaRect.y += 4;
			layoutAreaRect.height -= 8;
			return layoutAreaRect;
		}
		
		private static Texture2D tempTexture;
		internal static Texture2D TextureFromColor(Color color)
		{
			//Object.DestroyImmediate(tempTexture);
			if (tempTexture == null)
			{
				tempTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false)
				{
					filterMode = FilterMode.Point,
					anisoLevel = 0,
				};
			}
			tempTexture.SetPixel(0, 0, color);
			tempTexture.Apply();
			return tempTexture;
		}
		
		internal static void SaveTextureToSession(byte[] data, string sessionPath)
		{
			var ints = ByteToIntArray(data);
			SessionState.SetIntArray(sessionPath, ints);
		}
		internal static Texture2D LoadTextureFromSession(string sessionPath)
		{
			int[] textureData = SessionState.GetIntArray(sessionPath, null);
			if (textureData != null)
			{
				try
				{
					var bytes = IntToByteArray(textureData);
					var subTexture = new Texture2D(0, 0);
					subTexture.LoadImage(bytes);
					subTexture.Apply();

					return subTexture;
				}
				catch (Exception e)
				{
					Debug.LogException(e);
					SessionState.EraseIntArray(sessionPath);
				}
			}

			return null;
		}
		#endregion
		
		#region Clickable
		internal static bool ClickableButton(string label, params GUILayoutOption[] options) => ClickableButton(new GUIContent(label), null, options);
		internal static bool ClickableButton(string label, GUIStyle style, params GUILayoutOption[] options) => ClickableButton(new GUIContent(label), style, options);
		internal static bool ClickableButton(GUIContent label, GUIStyle style, params GUILayoutOption[] options)
		{
			if (style == null) style = GUI.skin.button;
			bool clicked = GUILayout.Button(label, style, options);
			Rect r = GUILayoutUtility.GetLastRect();
			EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
			return clicked;
		}

		internal static bool IfRectClicked(Rect r)
		{
			EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
			var e = Event.current;
			return e.button == 0 && e.type == EventType.MouseDown && r.Contains(e.mousePosition);
		}
		#endregion
		
		#region Misc
		private static int[] ByteToIntArray(byte[] bytes)
		{
			var size = bytes.Length;
			var ints = new int[size];
			for (var index = 0; index < size; index++)
				ints[index] = bytes[index];

			return ints;
		}
		
		private static byte[] IntToByteArray(int[] ints)
		{
			byte[] bytes = new byte[ints.Length];
			for (int i = 0; i < ints.Length; i++)
				bytes[i] = (byte) ints[i];

			return bytes;
		}
		#endregion
	}
}
