using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using static DreadScripts.Common.SupportThankies.Helper;
using static DreadScripts.Common.SupportThankies.StylesAndContent;

namespace DreadScripts.Common.SupportThankies
{
	internal class Supporter
	{
		internal readonly string rawData;
		internal readonly List<SupporterContent> nameContents;
		internal readonly List<SupporterContent> prefixContents;
		internal readonly List<SupporterContent> suffixContents;
		internal readonly WebLoadedTexture backgroundImage;
		internal readonly WebLoadedTexture.TextureLayoutMethod backgroundLayoutMethod;
		internal readonly Color? backgroundColor;
		internal readonly Color? borderColor;
		internal readonly Color? nameColor;
		internal readonly string tooltip;
		internal readonly string onClickUrl;
		internal readonly object splitState = ReflectionSplitterGUILayout.CreateSplitterState(1,1,1);
		internal Rect lastRect;

		internal Supporter(string rawData)
		{
			this.rawData = rawData;
			TryExtractValue("onclick", out onClickUrl);
			tooltip = TryExtractValue("tooltip", out string tt) ? tt : Strings.supporterTooltipOptions.GetRandom();
			if (!(TryExtractValue("bgtype", out string typeString) && Enum.TryParse(typeString, true, out backgroundLayoutMethod)))
				backgroundLayoutMethod = WebLoadedTexture.TextureLayoutMethod.Pattern;

			if (TryExtractValue("name", out string name)) nameContents = SupporterContent.Parse(name);
			if (TryExtractValue("prefix", out string prefix)) prefixContents = SupporterContent.Parse(prefix);
			if (TryExtractValue("suffix", out string suffix)) suffixContents = SupporterContent.Parse(suffix);
			if (TryExtractValue("namecolor", out string nameHex)) nameColor = ColorUtility.TryParseHtmlString(nameHex, out Color c) ? c : (Color?) null;
			if (TryExtractValue("bgcolor", out string bgHex)) backgroundColor = ColorUtility.TryParseHtmlString(bgHex, out Color c) ? c : (Color?) null;
			if (TryExtractValue("bordercolor", out string borderHex)) borderColor = ColorUtility.TryParseHtmlString(borderHex, out Color c) ? c : (Color?) null;
			if (TryExtractValue("bgimage", out string bgUrl)) backgroundImage = new WebLoadedTexture(bgUrl, true, bgUrl);
		}

		internal void Draw(float lineHeight = 20)
		{
			//if (hasColor) DrawBorderedBox(lastRect.Shrink(3), color.Faded(0.7f), 3, color.Overlayed(new Color(0,0,0,0.5f)).Faded(0.4f));
			var r = lastRect.Shrink(2);
			using (new ColoredScope(ColoredScope.ColoringType.General, backgroundColor != null ? GUI.color.Overlayed(backgroundColor.Value) : GUI.color))
				backgroundImage?.DrawTexture(r, backgroundLayoutMethod);
			DrawBorderedBox(r, backgroundImage != null ? Color.clear : backgroundColor ?? new Color(0,0,0,0.4f), borderColor ?? default, 1);

			using (new GUILayout.VerticalScope())
			{
				using (new GUILayout.VerticalScope())
				{
					GUILayout.FlexibleSpace();
					//EditorGUILayout.GetControlRect(GUILayout.Height(boxHeight), GUILayout.Width(10));
					ReflectionSplitterGUILayout.BeginSplit(splitState, null, false);
					using (new GUILayout.HorizontalScope())
					{
						GUILayout.Space(8);
						if (prefixContents != null)
							foreach (var c in prefixContents)
								c.Draw(Styles.prefixLabelStyle, lineHeight);
						else GUILayout.Label(GUIContent.none);
					}

					using (new GUILayout.HorizontalScope())
					{
						GUILayout.FlexibleSpace();
						if (nameContents != null)
						{
							using (new ColoredScope(ColoredScope.ColoringType.General, nameColor ?? GUI.color))
								foreach (var c in nameContents)
									c.Draw(Styles.supporterLabelStyle, lineHeight);
						}

						GUILayout.FlexibleSpace();
					}

					using (new GUILayout.HorizontalScope())
					{
						GUILayout.FlexibleSpace();
						if (suffixContents != null)
							foreach (var c in suffixContents)
								c.Draw(Styles.suffixLabelStyle, lineHeight);
						else GUILayout.Label(GUIContent.none);
						GUILayout.Space(8);
					}

					ReflectionSplitterGUILayout.EndSplit();
					GUILayout.FlexibleSpace();
				}
				if (Event.current.type == EventType.Repaint)
					lastRect = GUILayoutUtility.GetLastRect();

				GUILayout.Space(4);
			}

			GUI.Label(lastRect, new GUIContent(string.Empty, tooltip));
			if (!string.IsNullOrWhiteSpace(onClickUrl))
				if (IfRectClicked(lastRect))
					Application.OpenURL(onClickUrl);
		}
		
		internal bool TryExtractValue(string tagName, out string value)
		{
			string pattern = "<"+tagName+"=(.*?)>(?:<|$)";
			Match match = Regex.Match(rawData, pattern);
			bool success = match.Success;
			value = success ? match.Groups[1].Value : null;
			return success;
		}
	}

	internal struct SupporterContent
	{
		internal GUIContent label;
		internal WebLoadedTexture icon;
		internal bool isIcon;

		internal SupporterContent(GUIContent label)
		{
			this.label = label;
			icon = null;
			isIcon = false;
		}

		internal SupporterContent(WebLoadedTexture icon)
		{
			label = GUIContent.none;
			this.icon = icon;
			isIcon = true;
		}

		internal void Draw(GUIStyle style, float lineHeight = 20)
		{
			if (isIcon) GUILayout.Label(icon.texture, style, GUILayout.Width(lineHeight), GUILayout.Height(lineHeight));
			else GUILayout.Label(label, style, GUILayout.ExpandWidth(false), GUILayout.Height(lineHeight));
		}

		internal void Draw(Rect r)
		{
			if (isIcon) icon.DrawTexture(r);
			else GUI.Label(r, label, Styles.supporterLabelStyle);
		}

		internal static List<SupporterContent> Parse(string data)
		{
			var list = new List<SupporterContent>();
			var m = Regex.Match(data, @"<image=(.+?)>");
			while (m.Success)
			{
				string url = m.Groups[1].Value;
				if (m.Index > 0) list.Add(new SupporterContent(new GUIContent(data.Substring(0, m.Index))));
				list.Add(new SupporterContent(new WebLoadedTexture(url, true, url)));
				data = data.Substring(m.Index + m.Length);
				m = Regex.Match(data, @"<image=(.+?)>");
			}

			if (!string.IsNullOrEmpty(data)) list.Add(new SupporterContent(new GUIContent(data)));
			return list;
		}
	}
}

