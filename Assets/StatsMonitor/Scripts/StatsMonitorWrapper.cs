// 
// Created 8/23/2015 21:10:13
// Copyright © Hexagon Star Softworks. All Rights Reserved.
// http://www.hexagonstar.com/
//  

using StatsMonitor.Core;
using StatsMonitor.Util;
using UnityEngine;


namespace StatsMonitor
{
	/// <summary>
	///		A wrapper for the actual stats monitor class (StatsMonitor)
	///		which does the main work for stats monitor. This class provides a wrapping
	///		UI canvas for the Stats Monitor.
	/// </summary>
	[DisallowMultipleComponent]
	public class StatsMonitorWrapper : MonoBehaviour
	{
		// ----------------------------------------------------------------------------
		// Constants
		// ----------------------------------------------------------------------------

		public const string WRAPPER_NAME = "Stats Monitor";


		// ----------------------------------------------------------------------------
		// Properties
		// ----------------------------------------------------------------------------

		internal static readonly Anchors anchors = new Anchors();

		public static StatsMonitorWrapper instance { get; private set; }
		private StatsMonitor _statsMonitor;
		private Canvas _canvas;


		// ----------------------------------------------------------------------------
		// Accessors
		// ----------------------------------------------------------------------------

		private static StatsMonitorWrapper InternalInstance
		{
			get
			{
				if (instance == null)
				{
					StatsMonitorWrapper statsMonitorWrapper = FindObjectOfType<StatsMonitorWrapper>();
					if (statsMonitorWrapper != null)
					{
						instance = statsMonitorWrapper;
					}
					else
					{
						GameObject container = new GameObject(WRAPPER_NAME);
						container.AddComponent<StatsMonitorWrapper>();
					}
				}
				return instance;
			}
		}


		// ----------------------------------------------------------------------------
		// Constructor
		// ----------------------------------------------------------------------------

		private StatsMonitorWrapper()
		{
			/* Prevent direct instantiation! */
		}


		// ----------------------------------------------------------------------------
		// Public Methods
		// ----------------------------------------------------------------------------

		public static StatsMonitorWrapper AddToScene()
		{
			return InternalInstance;
		}


		public void SetRenderMode(RenderMode renderMode)
		{
			if (renderMode == RenderMode.Overlay)
			{
				_canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceOverlay;
				_canvas.sortingOrder = short.MaxValue;
			}
			else if (renderMode == RenderMode.Camera)
			{
				_canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceCamera;
				_canvas.worldCamera = Camera.current ?? Camera.main;
				/* Set SM to be on the front-most layer since we can't create a sorting layer
				   via script! */
				_canvas.sortingLayerName = SortingLayer.layers[SortingLayer.layers.Length - 1].name;
				_canvas.sortingOrder = short.MaxValue;
			}
		}


		// ----------------------------------------------------------------------------
		// Protected & Private Methods
		// ----------------------------------------------------------------------------

		private void CreateUI()
		{
			/* Create UI canvas used for all StatsMonitorWrapper components. */
			_canvas = gameObject.AddComponent<Canvas>();
			_canvas.pixelPerfect = true;

			RectTransform tr = gameObject.GetComponent<RectTransform>();
			tr.pivot = Vector2.up;
			tr.anchorMin = Vector2.up;
			tr.anchorMax = Vector2.up;
			tr.anchoredPosition = new Vector2(0.0f, 0.0f);

			/* Find the widget game object to enable it on start. */
			GameObject widget = transform.Find(StatsMonitor.NAME).gameObject;
			widget.SetActive(true);

			/* Find StatsMonitor child object. */
			_statsMonitor = FindObjectOfType<StatsMonitor>();
			_statsMonitor.wrapper = this;
		}


		private void DisposeInternal()
		{
			if (_statsMonitor != null) _statsMonitor.Dispose();
			Destroy(this);
			if (instance == this) instance = null;
		}


		// ----------------------------------------------------------------------------
		// Unity Callbacks
		// ----------------------------------------------------------------------------

		private void Awake()
		{
			if (instance == null) instance = this;
			DontDestroyOnLoad(gameObject);
			CreateUI();
			SetRenderMode(_statsMonitor.RenderMode);
			Utils.AddToUILayer(gameObject);
		}


		// ----------------------------------------------------------------------------
		// Editor Integration
		// ----------------------------------------------------------------------------

#if UNITY_EDITOR
		private const string MENU_PATH = "GameObject/Create Other/" + StatsMonitor.NAME;

		[UnityEditor.MenuItem(MENU_PATH, false)]
		private static void AddToSceneInEditor()
		{
			StatsMonitorWrapper statsMonitorWrapper = FindObjectOfType<StatsMonitorWrapper>();
			if (statsMonitorWrapper == null)
			{
				GameObject wrapper = GameObject.Find(WRAPPER_NAME);
				if (wrapper == null)
				{
					wrapper = new GameObject(WRAPPER_NAME);
					UnityEditor.Undo.RegisterCreatedObjectUndo(wrapper, "Create " + WRAPPER_NAME);
					Utils.ResetTransform(wrapper);
					Utils.AddToUILayer(wrapper);
					wrapper.AddComponent<StatsMonitorWrapper>();
					
					GameObject widget = new GameObject(StatsMonitor.NAME);
					widget.transform.parent = wrapper.transform;
					widget.AddComponent<RectTransform>();
					widget.AddComponent<StatsMonitor>();
					Utils.AddToUILayer(widget);
					widget.SetActive(false);
				}
				else
				{
					Debug.LogWarning("Another object named " + WRAPPER_NAME
						+ " already exists in the scene! Rename or delete it"
						+ " before trying to add " + WRAPPER_NAME + ".");
					
				}
				UnityEditor.Selection.activeObject = wrapper;
			}
			else
			{
				Debug.LogWarning(WRAPPER_NAME + " already exists in the scene!");
			}
		}
	#endif
	}
}
