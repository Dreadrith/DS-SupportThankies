using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using static DreadScripts.Common.SupportThankies.Helper;

namespace DreadScripts.Common.SupportThankies
{
	internal sealed class WebLoadedTexture
	{
		private Texture2D _texture;
		private bool requireReload = true;

		internal Texture2D texture
		{
			get
			{
				if (hasLoaded)
				{
					if (requireReload && !_texture) ReloadTexture();
					return _texture;
				}

				if (isLoading) return null;
				if (!loadOnGet || hasConnected) return null;

				hasConnected = true;
				isLoading = true;
				DownloadTexture();
				return null;
			}
		}

		private readonly string url;
		private readonly bool loadOnGet;
		private readonly string savePath;

		internal bool hasLoaded;
		internal bool isLoading;
		private bool hasConnected;
		private bool canDraw;

		internal WebLoadedTexture(string url, bool loadOnGet, string savePath)
		{
			this.url = url;
			this.loadOnGet = loadOnGet;
			this.savePath = savePath;
		}

		internal void DownloadTexture()
		{
			if (ReloadTexture()) return;

			UnityWebRequest client = new UnityWebRequest(url) {downloadHandler = new DownloadHandlerBuffer()};
			client.SendWebRequest().completed += op =>
			{
				if (client.isDone && !client.isHttpError && !client.isNetworkError)
				{
					try
					{
						byte[] textureData = client.downloadHandler.data;
						_texture = new Texture2D(0, 0);
						_texture.LoadImage(textureData);
						_texture.Apply();
						hasLoaded = true;
						if (string.IsNullOrWhiteSpace(savePath)) return;
						SaveTextureToSession(textureData, savePath);
						requireReload = true;
					}
					finally
					{
						client.Dispose();
					}
				}
				else client.Dispose();
			};
			isLoading = false;
		}

		internal void DrawPatternTexture(Rect textureRect, PatternLayoutData layoutData = default) => DrawTexture(textureRect, TextureLayoutMethod.Pattern, layoutData);
		internal void DrawTexture(Rect textureRect) => DrawTexture(textureRect, TextureLayoutMethod.StretchToFill);
		internal void DrawTexture(Rect textureRect, TextureLayoutMethod layoutMethod, PatternLayoutData layoutData = default)
		{
			bool b = GetCanDraw();
			if (b)
			{
				if (layoutMethod == TextureLayoutMethod.Pattern)
				{
					float xScale, yScale;
					Vector2 offset;
					if (!layoutData.initialized)
					{
						xScale = yScale = (texture.width / 256f + texture.height/256f) / 2f;
						offset = new Vector2(texture.width / 2f, texture.height / 2f);
					}
					else
					{
						xScale = layoutData.xTileScale;
						yScale = layoutData.yTileScale;
						offset = layoutData.offset;
					}
					
					float xTile = textureRect.width/texture.width * xScale;
					float yTile = textureRect.height/texture.height * yScale;
					Vector2 scale = new Vector2(xTile, yTile);
					
					GUI.DrawTextureWithTexCoords(textureRect, texture, new Rect(offset, scale));

				} else
				{
					ScaleMode sm = 
						layoutMethod == TextureLayoutMethod.ScaleToFill ? ScaleMode.ScaleAndCrop : 
						layoutMethod == TextureLayoutMethod.ScaleToFit ? ScaleMode.ScaleToFit : 
						                                                 ScaleMode.StretchToFill;
					GUI.DrawTexture(textureRect, texture, sm);
				}
			}
			else DrawPlaceholder(textureRect);
		}

		internal void Reset()
		{
			if (!string.IsNullOrEmpty(savePath))
				SessionState.EraseIntArray(savePath);

			_texture = null;
			canDraw = false;
			hasConnected = false;
			hasLoaded = false;
			isLoading = false;
			requireReload = true;
		}

		internal bool ReloadTexture()
		{
			if (requireReload && !string.IsNullOrWhiteSpace(savePath))
			{
				requireReload = false;
				var loadedTexture = LoadTextureFromSession(savePath);
				if (loadedTexture != null)
				{
					_texture = loadedTexture;
					hasLoaded = true;
					isLoading = false;
					requireReload = true;
				}
			}

			return _texture;
		}

		private void DrawPlaceholder(Rect r) => GUI.Box(r, GUIContent.none);
		
		internal bool GetCanDraw()
		{
			if (canDraw) return true;
			if (texture == null) return false;
			if (Event.current.type == EventType.Layout) canDraw = true;
			return true;
		}
		
		internal enum TextureLayoutMethod
		{
			ScaleToFill,
			StretchToFill,
			ScaleToFit,
			Pattern
		}
		
		internal struct PatternLayoutData
		{
			internal readonly bool initialized;
			internal readonly float xTileScale;
			internal readonly float yTileScale;
			internal readonly Vector2 offset;

			internal PatternLayoutData(float scale) : this(Vector2.zero, scale, scale) {}
			internal PatternLayoutData(float xTileScale, float yTileScale) : this(Vector2.zero, xTileScale, yTileScale) {}
			internal PatternLayoutData(Vector2 offset, float scale) : this(offset, scale, scale) {}
			internal PatternLayoutData(Vector2 offset, float xTileScale, float yTileScale)
			{
				initialized = true;
				this.xTileScale = xTileScale;
				this.yTileScale = yTileScale;
				this.offset = offset;
			}
			
			
		}
	}
}

