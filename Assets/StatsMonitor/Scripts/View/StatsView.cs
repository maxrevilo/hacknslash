// 
// Created 9/1/2015 16:01:46
// Copyright © Hexagon Star Softworks. All Rights Reserved.
// http://www.hexagonstar.com/
//  

using StatsMonitor.Core;
using StatsMonitor.Util;
using UnityEngine;
using UnityEngine.UI;


namespace StatsMonitor.View
{
	/// <summary>
	///		View class that displays the textual stats information.
	/// </summary>
	internal class StatsView : View2D
	{
		// ----------------------------------------------------------------------------
		// Properties
		// ----------------------------------------------------------------------------

		private readonly StatsMonitor _statsMonitor;

		private Text _text1;
		private Text _text2;
		private Text _text3;
		private Text _text4;

		private string[] _fpsTemplates;
		private string _fpsMinTemplate;
		private string _fpsMaxTemplate;
		private string _fpsAvgTemplate;
		private string _fxuTemplate;
		private string _msTemplate;
		private string _objTemplate;
		private string _memTotalTemplate;
		private string _memAllocTemplate;
		private string _memMonoTemplate;


		// ----------------------------------------------------------------------------
		// Constructor
		// ----------------------------------------------------------------------------

		internal StatsView(StatsMonitor statsMonitor)
		{
			_statsMonitor = statsMonitor;
			Invalidate();
		}


		// ----------------------------------------------------------------------------
		// Public Methods
		// ----------------------------------------------------------------------------

		public override void Reset()
		{
			/* Clear all text fields. */
			_text1.text = _text2.text = _text3.text = _text4.text = "";
		}


		public override void Update()
		{
			_text1.text = _fpsTemplates[_statsMonitor.fpsLevel] + _statsMonitor.fps + "</color>";

			_text2.text =
				_fpsMinTemplate + (_statsMonitor.fpsMin > -1 ? _statsMonitor.fpsMin : 0) + "</color>\n"
				+ _fpsMaxTemplate + (_statsMonitor.fpsMax > -1 ? _statsMonitor.fpsMax : 0) + "</color>";

			_text3.text =
				_fpsAvgTemplate + _statsMonitor.fpsAvg + "</color> " + _msTemplate + "" + _statsMonitor.ms.ToString("F1") + "MS</color> "
				+ _fxuTemplate + _statsMonitor.fixedUpdateRate + " </color>\n"
				+ _objTemplate + "OBJ:" + _statsMonitor.renderedObjectCount + "/" + _statsMonitor.renderObjectCount
				+ "/" + _statsMonitor.objectCount + "</color>";

			_text4.text =
				_memTotalTemplate + _statsMonitor.memTotal.ToString("F1") + "MB</color> "
				+ _memAllocTemplate + _statsMonitor.memAlloc.ToString("F1") + "MB</color> "
				+ _memMonoTemplate + _statsMonitor.memMono.ToString("F1") + "MB</color>";
		}


		public override void Dispose()
		{
			Destroy(_text1);
			Destroy(_text2);
			Destroy(_text3);
			Destroy(_text4);
			_text1 = _text2 = _text3 = _text4 = null;
			base.Dispose();
		}


		// ----------------------------------------------------------------------------
		// Protected & Private Methods
		// ----------------------------------------------------------------------------

		protected override GameObject CreateChildren()
		{
			_fpsTemplates = new string[3];

			GameObject container = new GameObject();
			container.name = "StatsView";
			container.transform.parent = _statsMonitor.transform;

			var g = new GraphicsFactory(container, _statsMonitor.colorFPS, _statsMonitor.fontFace, _statsMonitor.fontSizeSmall);
			_text1 = g.Text("Text1", "FPS:000");
			_text2 = g.Text("Text2", "MIN:000\nMAX:000");
			_text3 = g.Text("Text3", "AVG:000\n[000.0 MS]");
			_text4 = g.Text("Text4", "TOTAL:000.0MB ALLOC:000.0MB MONO:00.0MB");

			return container;
		}


		protected override void UpdateStyle()
		{
			_text1.font = _statsMonitor.fontFace;
			_text1.fontSize = _statsMonitor.FontSizeLarge;
			_text2.font = _statsMonitor.fontFace;
			_text2.fontSize = _statsMonitor.FontSizeSmall;
			_text3.font = _statsMonitor.fontFace;
			_text3.fontSize = _statsMonitor.FontSizeSmall;
			_text4.font = _statsMonitor.fontFace;
			_text4.fontSize = _statsMonitor.FontSizeSmall;

			if (_statsMonitor.colorOutline.a > 0.0f)
			{
				GraphicsFactory.AddOutlineAndShadow(_text1.gameObject, _statsMonitor.colorOutline);
				GraphicsFactory.AddOutlineAndShadow(_text2.gameObject, _statsMonitor.colorOutline);
				GraphicsFactory.AddOutlineAndShadow(_text3.gameObject, _statsMonitor.colorOutline);
				GraphicsFactory.AddOutlineAndShadow(_text4.gameObject, _statsMonitor.colorOutline);
			}
			else
			{
				GraphicsFactory.RemoveEffects(_text1.gameObject);
				GraphicsFactory.RemoveEffects(_text2.gameObject);
				GraphicsFactory.RemoveEffects(_text3.gameObject);
				GraphicsFactory.RemoveEffects(_text4.gameObject);
			}

			_fpsTemplates[0] = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorFPS) + ">FPS:";
			_fpsTemplates[1] = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorFPSWarning) + ">FPS:";
			_fpsTemplates[2] = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorFPSCritical) + ">FPS:";
			_fpsMinTemplate = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorFPSMin) + ">MIN:";
			_fpsMaxTemplate = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorFPSMax) + ">MAX:";
			_fpsAvgTemplate = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorFPSAvg) + ">AVG:";
			_fxuTemplate = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorFXD) + ">FXD:";
			_msTemplate = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorMS) + ">";
			_objTemplate = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorObjCount) + ">";
			_memTotalTemplate = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorMemTotal) + ">TOTAL:";
			_memAllocTemplate = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorMemAlloc) + ">ALLOC:";
			_memMonoTemplate = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorMemMono) + ">MONO:";
		}


		protected override void UpdateLayout()
		{
			int padding = _statsMonitor.padding;
			int hSpacing = _statsMonitor.spacing;
			int vSpacing = (_statsMonitor.spacing / 4);

			/* Make sure that string lengths keep initial set length before resizing text. */
			_text1.text = PadString(_text1.text, 7, 1);
			_text2.text = PadString(_text2.text.Split('\n')[0], 7, 2);
			_text3.text = PadString(_text3.text.Split('\n')[0], 20, 2);
			_text4.text = PadString(_text4.text, 39, 1);

			_text1.rectTransform.anchoredPosition = new Vector2(padding, -padding);
			int x = padding + (int)_text1.preferredWidth + hSpacing;
			_text2.rectTransform.anchoredPosition = new Vector2(x, -padding);
			x += (int)_text2.preferredWidth + hSpacing;
			_text3.rectTransform.anchoredPosition = new Vector2(x, -padding);
			x = padding;

			/* Workaround for correct preferredHeight which we'd have to wait for the next frame. */
			int text2DoubleHeight = (int)_text2.preferredHeight * 2;
			int y = padding + ((int)_text1.preferredHeight >= text2DoubleHeight ? (int)_text1.preferredHeight : text2DoubleHeight) + vSpacing;
			_text4.rectTransform.anchoredPosition = new Vector2(x, -y);
			y += (int)_text4.preferredHeight + padding;

			/* Update container size. */
			float row1Width = padding + _text1.preferredWidth + hSpacing + _text2.preferredWidth + hSpacing + _text3.preferredWidth + padding;
			float row2Width = padding + _text4.preferredWidth + padding;

			/* Pick larger width & normalize to even number to prevent texture glitches. */
			int w = row1Width > row2Width ? (int)row1Width : (int)row2Width;
			w = w % 2 == 0 ? w : w + 1;

			SetRTransformValues(0, 0, w, y, Vector2.one);
		}


		private static string PadString(string s, int minChars, int numRows)
		{
			s = Utils.StripHTMLTags(s);
			if (s.Length >= minChars) return s;
			int len = minChars - s.Length;
			for (int i = 0; i < len; i++)
			{
				s += "_";
			}
			return s;
		}
	}
}
