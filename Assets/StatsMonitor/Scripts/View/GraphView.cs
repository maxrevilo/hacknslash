// 
// Created 8/25/2015 17:24:34
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
	///		View class that displays the stats graph.
	/// </summary>
	internal class GraphView : View2D
	{
		// ----------------------------------------------------------------------------
		// Properties
		// ----------------------------------------------------------------------------


		private readonly StatsMonitor _statsMonitor;
		private RawImage _image;
		private Bitmap2D _graph;
		private int _oldWidth;
		private int _width;
		private int _height;
		private int _graphStartX;
		private int _graphMaxY;
		private int _memCeiling;
		private int _lastGCCollectionCount = -1;
		private Color?[] _fpsColors;


		// ----------------------------------------------------------------------------
		// Constructor
		// ----------------------------------------------------------------------------

		public GraphView(StatsMonitor statsMonitor)
		{
			_statsMonitor = statsMonitor;
			Invalidate();
		}


		// ----------------------------------------------------------------------------
		// Public Methods
		// ----------------------------------------------------------------------------

		public override void Reset()
		{
			if (_graph != null) _graph.Clear();
		}


		public override void Update()
		{
			if (_graph == null) return;

			/* Total Mem */
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, (int)Mathf.Ceil((_statsMonitor.memTotal / _memCeiling) * _height)), _statsMonitor.colorMemTotal);

			/* Alloc Mem */
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, (int)Mathf.Ceil((_statsMonitor.memAlloc / _memCeiling) * _height)), _statsMonitor.colorMemAlloc);

			/* Mono Mem */
			int monoMem = (int)Mathf.Ceil((_statsMonitor.memMono / _memCeiling) * _height);
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, monoMem), _statsMonitor.colorMemMono);

			/* MS */
			int ms = (int)_statsMonitor.ms >> 1;
			if (ms == monoMem) ms += 1; // Don't overlay mono mem as they are often in the same range.
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, ms), _statsMonitor.colorMS);

			/* FPS */
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, ((_statsMonitor.fps / (_statsMonitor.fpsMax > 60 ? _statsMonitor.fpsMax : 60)) * _graphMaxY) - 1), _statsMonitor.colorFPS);

			/* GC. */
			if (_lastGCCollectionCount != System.GC.CollectionCount(0))
			{
				_lastGCCollectionCount = System.GC.CollectionCount(0);
				_graph.FillColumn(_graphStartX, 0, 5, _statsMonitor.colorGCBlip);
			}

			_graph.Scroll(-1, 0, _fpsColors[_statsMonitor.fpsLevel]);
			_graph.Apply();
		}


		public override void Dispose()
		{
			if (_graph != null) _graph.Dispose();
			_graph = null;
			Destroy(_image);
			_image = null;
			base.Dispose();
		}


		// ----------------------------------------------------------------------------
		// Internal Methods
		// ----------------------------------------------------------------------------

		internal void SetWidth(float width)
		{
			_width = (int)width;
		}


		// ----------------------------------------------------------------------------
		// Protected & Private Methods
		// ----------------------------------------------------------------------------

		protected override GameObject CreateChildren()
		{
			_fpsColors = new Color?[3];

			GameObject container = new GameObject();
			container.name = "GraphView";
			container.transform.parent = _statsMonitor.transform;

			_graph = new Bitmap2D(10, 10, _statsMonitor.colorGraphBG);

			_image = container.AddComponent<RawImage>();
			_image.rectTransform.sizeDelta = new Vector2(10, 10);
			_image.color = Color.white;
			_image.texture = _graph.texture;

			/* Calculate estimated memory ceiling for application. */
			int sysMem = SystemInfo.systemMemorySize;
			if (sysMem <= 1024) _memCeiling = 512;
			else if (sysMem > 1024 && sysMem <= 2048) _memCeiling = 1024;
			else _memCeiling = 2048;

			return container;
		}


		protected override void UpdateStyle()
		{
			if (_graph != null) _graph.color = _statsMonitor.colorGraphBG;
			if (_statsMonitor.colorOutline.a > 0.0f)
				GraphicsFactory.AddOutlineAndShadow(_image.gameObject, _statsMonitor.colorOutline);
			else
				GraphicsFactory.RemoveEffects(_image.gameObject);
			_fpsColors[0] = null;
			_fpsColors[1] = new Color(_statsMonitor.colorFPSWarning.r, _statsMonitor.colorFPSWarning.g, _statsMonitor.colorFPSWarning.b, _statsMonitor.colorFPSWarning.a / 4);
			_fpsColors[2] = new Color(_statsMonitor.ColorFPSCritical.r, _statsMonitor.ColorFPSCritical.g, _statsMonitor.ColorFPSCritical.b, _statsMonitor.ColorFPSCritical.a / 4);
		}


		protected override void UpdateLayout()
		{
			/* Make sure that dimensions for text size are valid! */
			if ((_width > 0 && _statsMonitor.graphHeight > 0) && (_statsMonitor.graphHeight != _height || _oldWidth != _width))
			{
				_oldWidth = _width;

				_height = _statsMonitor.graphHeight;
				_height = _height % 2 == 0 ? _height : _height + 1;

				/* The X position in the graph for pixels to be drawn. */
				_graphStartX = _width - 1;
				_graphMaxY = _height - 1;

				_image.rectTransform.sizeDelta = new Vector2(_width, _height);
				_graph.Resize(_width, _height);
				_graph.Clear();

				SetRTransformValues(0, 0, _width, _height, Vector2.one);
			}
		}
	}
}
