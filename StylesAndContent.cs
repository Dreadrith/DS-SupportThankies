using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static DreadScripts.Common.SupportThankies.Strings;

namespace DreadScripts.Common.SupportThankies
{
	internal static class StylesAndContent
	{
		internal static ContentContainer _Content;
		internal static ContentContainer Content => _Content ?? (_Content = new ContentContainer());
		internal class ContentContainer
		{
			internal readonly WebLoadedTexture showWindowIcon = new WebLoadedTexture(SUPPORTERS_ICON, true, SUPPORTERS_ICON_SAVE_PATH);
			internal readonly WebLoadedTexture kofiButton = new WebLoadedTexture(KOFI_BUTTON, true, KOFI_BUTTON_SAVE_PATH);
		}
		
		internal static StylesContainer _Styles;
		internal static StylesContainer Styles => _Styles ?? (_Styles = new StylesContainer());
		internal class StylesContainer
		{
			internal readonly GUIStyle titleStyle = new GUIStyle(EditorStyles.whiteLabel)
			{
				alignment = TextAnchor.MiddleCenter,
				fontStyle = FontStyle.Bold,
				fontSize = 18
			};
			
			internal readonly GUIStyle supporterLabelStyle = new GUIStyle(EditorStyles.whiteLabel)
			{
				alignment = TextAnchor.MiddleCenter,
				fontStyle = FontStyle.Bold,
				fontSize = 16,
				richText = true
			};
            
			internal readonly GUIStyle prefixLabelStyle = new GUIStyle(EditorStyles.whiteLabel)
			{
				alignment = TextAnchor.MiddleLeft,
				fontStyle = FontStyle.Bold,
				fontSize = 16,
				richText = true
			};
			
			internal readonly GUIStyle suffixLabelStyle = new GUIStyle(EditorStyles.whiteLabel)
			{
				alignment = TextAnchor.MiddleRight,
				fontStyle = FontStyle.Bold,
				fontSize = 16,
				richText = true
			};

		}
	}
}
