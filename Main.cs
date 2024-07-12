using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using static DreadScripts.Common.SupportThankies.Helper;
using static DreadScripts.Common.SupportThankies.Strings;
using static DreadScripts.Common.SupportThankies.StylesAndContent;

namespace DreadScripts.Common.SupportThankies
{
	public class SupportThankies : EditorWindow
	{
		private static bool isLoadingSupporters;
		private static bool hasLoadedSupporters;
		private static bool failedLoadingSupporters;
		private static bool triedLoadingSupporters => hasLoadedSupporters || failedLoadingSupporters;
		private static string supporterLoadError;

		private static GUIContent supporterThanksLabel;
		private static Supporter[] supporters;
		private static string supportersRawText;

		private static Rect currentControlRect = Rect.zero;
		private static Rect nextControlRect = Rect.zero;
		private static Vector2 scroll;
		
		private static object horSplitterState = ReflectionSplitterGUILayout.CreateSplitterState(1);
		private static object verSplitterState = ReflectionSplitterGUILayout.CreateSplitterState(1);
		private static int lastCountPerLine = 1;
		private static int lastLineCount = 1;
		

		private static void RandomizeThanksLabel() => supporterThanksLabel = new GUIContent(thanksTextOptions.GetRandom(), thanksTooltipOptions.GetRandom());

		public static void DrawThanksButton()
		{
			var r = EditorGUILayout.GetControlRect(false, 16, GUIStyle.none, GUILayout.Width(16));
			r.x -= 2;
			Content.showWindowIcon.DrawTexture(r);
			if (IfRectClicked(r)) ShowWindow();
		}

		public static void ShowWindow()
		{
			var w = GetWindow<SupportThankies>(windowTitleOptions.GetRandom());
			w.titleContent.image = Content.showWindowIcon.texture;
		}

		public void OnGUI()
		{
			if (!triedLoadingSupporters && !isLoadingSupporters)
				_ = LoadSupporters();

			if (isLoadingSupporters)
			{
				GUILayout.Label("Loading supporters...", Styles.titleStyle);
			}

			if (failedLoadingSupporters)
			{
				GUILayout.Label("Failed to load supporters.", Styles.titleStyle);
				if (!string.IsNullOrWhiteSpace(supporterLoadError)) EditorGUILayout.HelpBox(supporterLoadError, MessageType.Error);
				if (ClickableButton("Retry", EditorStyles.toolbarButton)) ResetLoad();
			}

			if (hasLoadedSupporters)
			{

				using (new GUILayout.HorizontalScope("in bigtitle"))
				{
					GUILayout.Label(supporterThanksLabel, Styles.titleStyle);
				}
				DrawSupportersText();
			}

			DrawSupportersFooter();


			// if (ClickableButton("Refresh"))
			// {
			// 	ResetLoad();
			// 	Close();
			// 	Content.showWindowIcon.Reset();
			// 	Content.kofiButton.Reset();
			// 	ShowWindow();
			// }
		}

		public void DrawSupportersText()
		{
			var e = Event.current;
			var tempRect = EditorGUILayout.GetControlRect(GUILayout.Height(60), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

			// This is to avoid the stupid ass Control Repaint error and other jank shit from Unity
			switch (e.type)
			{
				case EventType.Layout:
					currentControlRect = nextControlRect;
					break;
				case EventType.Repaint:
					nextControlRect = tempRect;
					break;
			}

			int count = supporters.Length;
			float YtoXRatio = currentControlRect.width / currentControlRect.height;
			float lineHeight = 25;
			int countPerLine = Mathf.Clamp(Mathf.Min(Mathf.RoundToInt(YtoXRatio), Mathf.CeilToInt((lineHeight + 4) * count / currentControlRect.height)), 1, count);
			int lineCount = Mathf.CeilToInt((float) count / countPerLine);
			
			if (lastCountPerLine != countPerLine)
			{
				lastCountPerLine = countPerLine;
				horSplitterState = ReflectionSplitterGUILayout.CreateSplitterState(Enumerable.Repeat(1f, countPerLine).ToArray());
			}

			if (lastLineCount != lineCount)
			{
				lastLineCount = lineCount;
				verSplitterState = ReflectionSplitterGUILayout.CreateSplitterState(Enumerable.Repeat(1f, lineCount).ToArray());
			}
			
			GUILayout.BeginArea(currentControlRect);
			scroll = EditorGUILayout.BeginScrollView(scroll);
			var ind = 0;

			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Space(4);
				ReflectionSplitterGUILayout.BeginSplit(horSplitterState, null, false);
				for (int i = 0; i < countPerLine; i++)
				{
					using (new GUILayout.HorizontalScope())
					{
						using (new GUILayout.VerticalScope())
						{
							ReflectionSplitterGUILayout.BeginSplit(verSplitterState);
							for (int j = 0; j < lineCount; j++)
							{
								if (ind >= supporters.Length) GUILayout.Label(GUIContent.none);
								else supporters[ind++].Draw(lineHeight);
							}
							ReflectionSplitterGUILayout.EndSplit();
						}
						if (i < countPerLine - 1)
							GUILayout.Space(4);
					}
				}
				ReflectionSplitterGUILayout.EndSplit();
			}

			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}

		public static void DrawSupportersFooter()
		{
			Rect footerRect = GUILayoutUtility.GetRect(100, 200, 16, 32);
			Rect kofiRect = AssertAspect(footerRect, 6.25f);
			GUI.DrawTexture(footerRect, TextureFromColor(Color.white), ScaleMode.StretchToFill, false, 0, new Color(0.075f, 0.765f, 1f), 0, 8);
			Content.kofiButton.DrawTexture(kofiRect);
			if (IfRectClicked(footerRect))
				OpenKofiWebsite();
		}
		
		public static void OpenKofiWebsite() => Application.OpenURL(KOFI_URL);

		public async Task LoadSupporters()
		{
			if (triedLoadingSupporters || isLoadingSupporters) return;
			isLoadingSupporters = true;
			using (var awb = new AsyncUnityWebRequest(SUPPORTERS_RAW_URL, UnityWebRequest.kHttpVerbGET))
			{
				var request = awb.request;
				request.useHttpContinue = false;
				request.downloadHandler = new DownloadHandlerBuffer();
				request.timeout = 10;
				await awb.Process();
				isLoadingSupporters = false;

				if (awb.failed)
				{
					failedLoadingSupporters = true;
					supporterLoadError = request.error;
					return;
				}

				try
				{
					supportersRawText = request.downloadHandler.text;
					ParseSupporters();
					hasLoadedSupporters = true;
				}
				catch (Exception e)
				{
					failedLoadingSupporters = true;
					supporterLoadError = e.ToString();
					throw;
				}
			}
		}

		public void ParseSupporters()
		{
			string[] lines = supportersRawText.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
			supporters = new Supporter[lines.Length];
			for (var i = 0; i < lines.Length; i++)
				supporters[i] = new Supporter(lines[i]);
			
			Repaint();
		}

		public void OnEnable() => RandomizeThanksLabel();

		public void OnLostFocus() { Close(); }

		public static void ResetLoad()
		{
			isLoadingSupporters = hasLoadedSupporters = failedLoadingSupporters = false;
			supporterLoadError = null;
		}

		
	}
}
