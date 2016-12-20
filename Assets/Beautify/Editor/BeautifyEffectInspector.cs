/// <summary>
/// Beautify effect inspector. Copyright 2016 Kronnect
/// </summary>
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace BeautifyEffect
{
	[CustomEditor (typeof(Beautify))]
	public class BeautifyEffectInspector : Editor
	{
		const string PRAGMA_COMMENT_MARK = "// Edited by Shader Control: ";
		const string PRAGMA_DISABLED_MARK = "// Disabled by Shader Control: ";
		const string PRAGMA_MULTICOMPILE = "#pragma multi_compile ";
		const string PRAGMA_UNDERSCORE = "__ ";
		Beautify _effect;
		Texture2D _headerTexture;
		static GUIStyle titleLabelStyle, labelBoldStyle, labelNormalStyle, sectionHeaderBoldStyle, sectionHeaderNormalStyle;
		static GUIStyle buttonNormalStyle, buttonPressedStyle, blackBack;
		static Color titleColor;
		bool expandSharpenSection, expandBloomSection, expandAFSection, expandDirtSection, expandDoFSection;
		bool expandEASection, expandPurkinjeSection, expandVignettingSection, expandDitherSection;
		bool toggleOptimizeBuild;
		GUIStyle blackStyle, commentStyle, disabledStyle, foldoutBold, foldoutNormal;
		List<BeautifySInfo> shaders;
		Vector2 scrollViewPos;

		void OnEnable ()
		{
			titleColor = EditorGUIUtility.isProSkin ? new Color (0.52f, 0.66f, 0.9f) : new Color (0.12f, 0.16f, 0.4f);
			_headerTexture = Resources.Load<Texture2D> ("beautifyHeader");
			_effect = (Beautify)target;
			blackBack = new GUIStyle ();
			blackBack.normal.background = MakeTex (4, 4, Color.black);
			expandSharpenSection = EditorPrefs.GetBool ("BeautifySharpenSection", false);
			expandBloomSection = EditorPrefs.GetBool ("BeautifyBloomSection", false);
			expandAFSection = EditorPrefs.GetBool ("BeautifyAFSection", false);
			expandDirtSection = EditorPrefs.GetBool ("BeautifyDirtSection", false);
			expandDoFSection = EditorPrefs.GetBool ("BeautifyDoFSection", false);
			expandEASection = EditorPrefs.GetBool ("BeautifyEASection", false);
			expandPurkinjeSection = EditorPrefs.GetBool ("BeautifyPurkinjeSection", false);
			expandVignettingSection = EditorPrefs.GetBool ("BeautifyVignettingSection", false);
			expandDitherSection = EditorPrefs.GetBool ("BeautifyDitherSection", false);
			ScanKeywords ();
		}

		void OnDestroy ()
		{
			// Restore folding sections state
			EditorPrefs.SetBool ("BeautifySharpenSection", expandSharpenSection);
			EditorPrefs.SetBool ("BeautifyBloomSection", expandBloomSection);
			EditorPrefs.SetBool ("BeautifyAFSection", expandAFSection);
			EditorPrefs.SetBool ("BeautifyDirtSection", expandDirtSection);
			EditorPrefs.SetBool ("BeautifyDoFSection", expandDoFSection);
			EditorPrefs.SetBool ("BeautifyEASection", expandEASection);
			EditorPrefs.SetBool ("BeautifyPurkinjeSection", expandPurkinjeSection);
			EditorPrefs.SetBool ("BeautifyVignettingSection", expandVignettingSection);
			EditorPrefs.SetBool ("BeautifyDitherSection", expandDitherSection);
		}

		public override void OnInspectorGUI ()
		{
			if (_effect == null)
				return;
			_effect.isDirty = false;

			// setup styles
			if (labelBoldStyle == null) {
				labelBoldStyle = new GUIStyle (EditorStyles.label); // GUI.skin.label);
				labelBoldStyle.fontStyle = FontStyle.Bold;
			}
			if (labelNormalStyle == null) {
				labelNormalStyle = new GUIStyle (EditorStyles.label); // GUI.skin.label);
			}
			if (sectionHeaderNormalStyle == null) {
				sectionHeaderNormalStyle = new GUIStyle (EditorStyles.foldout);
			}
			sectionHeaderNormalStyle.margin = new RectOffset (12, 0, 0, 0);
			if (sectionHeaderBoldStyle == null) {
				sectionHeaderBoldStyle = new GUIStyle (sectionHeaderNormalStyle);
			}
			sectionHeaderBoldStyle.fontStyle = FontStyle.Bold;
			if (buttonNormalStyle == null) {
				buttonNormalStyle = new GUIStyle (GUI.skin.button); // EditorStyles.miniButtonMid);
			}
			if (buttonPressedStyle == null) {
				buttonPressedStyle = new GUIStyle (buttonNormalStyle);
				buttonPressedStyle.fontStyle = FontStyle.Bold;
			}
			if (disabledStyle == null) {
				disabledStyle = new GUIStyle (EditorStyles.label);
			}
			disabledStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color (0.52f, 0.66f, 0.8f) : new Color (0.32f, 0.32f, 0.32f);
			if (foldoutBold == null) {
				foldoutBold = new GUIStyle (EditorStyles.foldout);
				foldoutBold.fontStyle = FontStyle.Bold;
			}
			if (foldoutNormal == null) {
				foldoutNormal = new GUIStyle (EditorStyles.foldout);
			}


			// draw interface
			EditorGUILayout.Separator ();
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;  
			GUILayout.BeginHorizontal (blackBack);
			GUILayout.Label (_headerTexture, GUILayout.ExpandWidth (true));
			GUI.skin.label.alignment = TextAnchor.MiddleLeft;  
			GUILayout.EndHorizontal ();
			if (!_effect.enabled) {
				EditorGUILayout.HelpBox ("Beautify disabled.", MessageType.Info);
			}
			EditorGUILayout.Separator ();

			EditorGUILayout.BeginHorizontal ();
			DrawLabel ("General Settings");
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label (new GUIContent ("Quality", "The mobile variant is simply less accurate but faster."), GUILayout.Width (90));
			_effect.quality = (BEAUTIFY_QUALITY)EditorGUILayout.EnumPopup (_effect.quality);
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label (new GUIContent ("Preset", "Quick configurations."), GUILayout.Width (90));
			_effect.preset = (BEAUTIFY_PRESET)EditorGUILayout.EnumPopup (_effect.preset);
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label (new GUIContent ("Compare Mode", "Shows a side by side comparison."), GUILayout.Width (90));
			_effect.compareMode = EditorGUILayout.Toggle (_effect.compareMode);
			if (GUILayout.Button (toggleOptimizeBuild ? "Hide Build Options" : "Build Options", toggleOptimizeBuild ? buttonPressedStyle : buttonNormalStyle, GUILayout.Width (toggleOptimizeBuild ? 130 : 100))) {
				toggleOptimizeBuild = !toggleOptimizeBuild;
			}
			if (GUILayout.Button ("Help", GUILayout.Width (50))) {
				EditorUtility.DisplayDialog ("Help", "Beautify is a full-screen image processing effect that makes your scenes crisp, vivid and intense.\n\nMove the mouse over a setting for a short description or read the provided documentation (PDF) for details and tips.\n\nVisit kronnect.com's forum for support and questions.\n\nPlease rate Beautify on the Asset Store! Thanks.", "Ok");
			}
			EditorGUILayout.EndHorizontal ();

			if (toggleOptimizeBuild) {
				EditorGUILayout.Separator ();
				DrawLabel ("Build Options");
				EditorGUILayout.HelpBox ("Select the features you want to use.\nUNSELECTED features will NOT be included in the build, reducing compilation time and build size.", MessageType.Info);
				EditorGUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Refresh", GUILayout.Width (60))) {
					ScanKeywords ();
					GUIUtility.ExitGUI ();
					return;
				}
				bool shaderChanged = false;
				for (int k=0; k<shaders.Count; k++) {
					if (shaders [k].pendingChanges)
						shaderChanged = true;
				}
				if (!shaderChanged)
					GUI.enabled = false;
				if (GUILayout.Button ("Save Changes", GUILayout.Width (110))) {
					UpdateShaders ();
				}
				GUI.enabled = true;
				EditorGUILayout.EndHorizontal ();
				if (shaders.Count > 0) {
					bool firstColumn = true;
					EditorGUILayout.BeginHorizontal ();
					BeautifySInfo shader = shaders [0];
					for (int k = 0; k < shader.keywords.Count; k++) {
						SCKeyword keyword = shader.keywords [k];
						if (keyword.isUnderscoreKeyword)
							continue;
						if (firstColumn) {
							EditorGUILayout.LabelField ("", GUILayout.Width (10));
						}
						bool prevState = keyword.enabled;
						keyword.enabled = EditorGUILayout.Toggle (prevState, GUILayout.Width (18));
						if (prevState != keyword.enabled) {
							shader.pendingChanges = true;
							GUIUtility.ExitGUI ();
							return;
						}
						string keywordName = SCKeywordChecker.Translate (keyword.name);
						if (!keyword.enabled) {
							EditorGUILayout.LabelField (keywordName, disabledStyle, GUILayout.Width (120));
						} else {
							EditorGUILayout.LabelField (keywordName, GUILayout.Width (120));
						}
						firstColumn = !firstColumn;
						if (firstColumn) {
							EditorGUILayout.EndHorizontal ();
							EditorGUILayout.BeginHorizontal ();
						}
					}
					EditorGUILayout.EndHorizontal ();
				}
				EditorGUILayout.Separator ();
				return;
			}

			if (_effect.compareMode) {
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent ("   Angle", "Angle of the separator line."), GUILayout.Width (90));
				_effect.compareLineAngle = EditorGUILayout.Slider (_effect.compareLineAngle, -Mathf.PI, Mathf.PI);
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent ("   Width", "Width of the separator line."), GUILayout.Width (90));
				_effect.compareLineWidth = EditorGUILayout.Slider (_effect.compareLineWidth, 0.0001f, 0.05f);
				EditorGUILayout.EndHorizontal ();
			}

			EditorGUILayout.Separator ();
			DrawLabel ("Image Enhancement");

			if (_effect.cameraEffect != null && !_effect.cameraEffect.hdr) {
				EditorGUILayout.HelpBox ("Some effects, like dither and bloom, works better with HDR enabled. Check your camera setting.", MessageType.Warning);
			}

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			expandSharpenSection = EditorGUILayout.Foldout (expandSharpenSection, new GUIContent ("Sharpen", "Sharpen intensity."), sectionHeaderNormalStyle); 
			EditorGUILayout.EndHorizontal ();
			_effect.sharpen = EditorGUILayout.Slider (_effect.sharpen, 0f, 12f);
			EditorGUILayout.EndHorizontal ();

			if (expandSharpenSection) {
				if (_effect.cameraEffect != null && !_effect.cameraEffect.orthographic) {
																
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Min/Max Depth", "Any pixel outside this depth range won't be affected by sharpen. Reduce range to create a depth-of-field-like effect."), GUILayout.Width (120));
					float minDepth = _effect.sharpenMinDepth;
					float maxDepth = _effect.sharpenMaxDepth;
					EditorGUILayout.MinMaxSlider (ref minDepth, ref maxDepth, 0f, 1.1f);
					_effect.sharpenMinDepth = minDepth;
					_effect.sharpenMaxDepth = maxDepth;
					EditorGUILayout.EndHorizontal ();

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Depth Threshold", "Reduces sharpen if depth difference around a pixel exceeds this value. Useful to prevent artifacts around wires or thin objects."), GUILayout.Width (120));
					_effect.sharpenDepthThreshold = EditorGUILayout.Slider (_effect.sharpenDepthThreshold, 0f, 0.05f);
					EditorGUILayout.EndHorizontal ();
				}

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent ("   Luminance Relax.", "Soften sharpen around a pixel with high contrast. Reduce this value to remove ghosting and protect fine drawings or wires over a flat surface."), GUILayout.Width (120));
				_effect.sharpenRelaxation = EditorGUILayout.Slider (_effect.sharpenRelaxation, 0f, 0.2f);
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent ("   Clamp", "Maximum pixel adjustment."), GUILayout.Width (120));
				_effect.sharpenClamp = EditorGUILayout.Slider (_effect.sharpenClamp, 0f, 1f);
				EditorGUILayout.EndHorizontal ();
			
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent ("   Motion Sensibility", "Increase to reduce sharpen to simulate a cheap motion blur and to reduce flickering when camera rotates or moves. This slider controls the amount of camera movement/rotation that contributes to sharpen reduction. Set this to 0 to disable this feature."), GUILayout.Width (120));
				_effect.sharpenMotionSensibility = EditorGUILayout.Slider (_effect.sharpenMotionSensibility, 0f, 1f);
				EditorGUILayout.EndHorizontal ();
			}

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			expandDitherSection = EditorGUILayout.Foldout (expandDitherSection, new GUIContent ("Dither", "Simulates more colors than RGB quantization can produce. Removes banding artifacts in gradients, like skybox. This setting controls the dithering strength."), sectionHeaderNormalStyle); 
			EditorGUILayout.EndHorizontal ();
			_effect.dither = EditorGUILayout.Slider (_effect.dither, 0f, 0.2f);
			EditorGUILayout.EndHorizontal ();
			if (expandDitherSection) {
				if (_effect.cameraEffect != null && !_effect.cameraEffect.orthographic) {
																
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Min Depth", "Will only remove bands on pixels beyond this depth. Useful if you only want to remove sky banding (set this to 0.99)"), GUILayout.Width (120));
					_effect.ditherDepth = EditorGUILayout.Slider (_effect.ditherDepth, 0f, 1f);
					EditorGUILayout.EndHorizontal ();
				}
			}

			EditorGUILayout.Separator ();
			DrawLabel ("Tonemapping & Color Grading");

			if (_effect.quality == BEAUTIFY_QUALITY.BestPerformance) {
				GUI.enabled = false;
			}
			EditorGUILayout.BeginHorizontal ();
			GUIStyle labelStyle = _effect.tonemap != BEAUTIFY_TMO.Linear ? labelBoldStyle : labelNormalStyle;
			GUILayout.Label (new GUIContent ("Tonemapping", "Converts high dynamic range colors into low dynamic range space according to a chosen tone mapping operator."), labelStyle, GUILayout.Width (90));
			if (isFeatureEnabled (Beautify.SKW_TONEMAP_ACES)) {
				_effect.tonemap = (BEAUTIFY_TMO)EditorGUILayout.EnumPopup (_effect.tonemap);
			}
			EditorGUILayout.EndHorizontal ();
			GUI.enabled = true;

			if (_effect.tonemap != BEAUTIFY_TMO.Linear) {
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent ("Exposure", "Exposure applied before tonemapping. Increase to make the image brighter."), GUILayout.Width (90));
				_effect.brightness = EditorGUILayout.FloatField(_effect.brightness, GUILayout.Width(60));
				EditorGUILayout.EndHorizontal ();
			}

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label (new GUIContent ("Vibrance", "Improves pixels color depending on their saturation."), GUILayout.Width (90));
			_effect.saturate = EditorGUILayout.Slider (_effect.saturate, -2f, 3f);
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			labelStyle = _effect.daltonize > 0 ? labelBoldStyle : labelNormalStyle;
			GUILayout.Label (new GUIContent ("Daltonize", "Similar to vibrance but mostly accentuate primary red, green and blue colors to compensate protanomaly (red deficiency), deuteranomaly (green deficiency) and tritanomaly (blue deficiency). This effect does not shift color hue hence it won't help completely red, green or blue color blindness. The effect will vary depending on each subject so this effect should be enabled on user demand."), labelStyle, GUILayout.Width (90));
			if (isFeatureEnabled (Beautify.SKW_DALTONIZE)) {
				_effect.daltonize = EditorGUILayout.Slider (_effect.daltonize, 0f, 2f);
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			labelStyle = _effect.tintColor.a > 0 ? labelBoldStyle : labelNormalStyle;
			GUILayout.Label (new GUIContent ("Tint", "Blends image with an optional color. Alpha specifies intensity."), labelStyle, GUILayout.Width (90));
			if (isFeatureEnabled (Beautify.SKW_TINT)) {
				_effect.tintColor = EditorGUILayout.ColorField (_effect.tintColor);
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label (new GUIContent ("Contrast", "Final image contrast adjustment. Allows you to create more vivid images."), GUILayout.Width (90));
			_effect.contrast = EditorGUILayout.Slider (_effect.contrast, 0.5f, 1.5f);
			EditorGUILayout.EndHorizontal ();

			if (_effect.tonemap == BEAUTIFY_TMO.Linear) {
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent ("Brightness", "Final image brightness adjustment."), GUILayout.Width (90));
				_effect.brightness = EditorGUILayout.Slider (_effect.brightness, 0f, 2f);
				EditorGUILayout.EndHorizontal ();
			}

			EditorGUILayout.Separator ();
			DrawLabel ("Lens & Lighting Effects");

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			labelStyle = _effect.bloom ? sectionHeaderBoldStyle : sectionHeaderNormalStyle;
			expandBloomSection = EditorGUILayout.Foldout (expandBloomSection, new GUIContent ("Bloom", "Produces fringes of light extending from the borders of bright areas, contributing to the illusion of an extremely bright light overwhelming the camera or eye capturing the scene."), labelStyle); 
			EditorGUILayout.EndHorizontal ();
			if (isFeatureEnabled (Beautify.SKW_BLOOM)) {
				_effect.bloom = EditorGUILayout.Toggle (_effect.bloom);
				if (expandBloomSection) {
					if (_effect.bloom) { 
						GUILayout.Label (new GUIContent ("Debug", "Enable to see bloom/anamorphic channel."));
						_effect.bloomDebug = EditorGUILayout.Toggle (_effect.bloomDebug, GUILayout.Width (40));
					}
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Intensity", "Bloom multiplier."), GUILayout.Width (90));
					_effect.bloomIntensity = EditorGUILayout.Slider (_effect.bloomIntensity, 0f, 10f);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Threshold", "Brightness sensibility."), GUILayout.Width (90));
					_effect.bloomThreshold = EditorGUILayout.Slider (_effect.bloomThreshold, 0f, 5f);
					if (_effect.quality == BEAUTIFY_QUALITY.BestQuality) {
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent ("   Reduce Flicker", "Enables an additional filter to reduce excess of flicker."), GUILayout.Width (90));
						_effect.bloomAntiflicker = EditorGUILayout.Toggle (_effect.bloomAntiflicker);
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent ("   Customize", "Edit bloom style parameters."), GUILayout.Width (90));
						_effect.bloomCustomize = EditorGUILayout.Toggle (_effect.bloomCustomize);
						if (_effect.bloomCustomize) {
							EditorGUILayout.EndHorizontal ();
							EditorGUILayout.BeginHorizontal ();
							GUILayout.Label ("   Presets", GUILayout.Width (90));
							if (GUILayout.Button ("Focused")) {
								_effect.SetBloomWeights (1f, 0.9f, 0.75f, 0.6f, 0.35f, 0.1f);
							}
							if (GUILayout.Button ("Regular")) {
								_effect.SetBloomWeights (0.85f, 0.95f, 1f, 0.9f, 0.77f, 0.6f);
							}
							if (GUILayout.Button ("Blurred")) {
								_effect.SetBloomWeights (0.2f, 0.4f, 0.6f, 0.75f, 0.9f, 1.0f);
							}
							EditorGUILayout.EndHorizontal ();
						
							EditorGUILayout.BeginHorizontal ();
							GUILayout.Label (new GUIContent ("   Weight 1", "First layer bloom weight."), GUILayout.Width (90));
							_effect.bloomWeight0 = EditorGUILayout.Slider (_effect.bloomWeight0, 0f, 1f);
							EditorGUILayout.EndHorizontal ();
							EditorGUILayout.BeginHorizontal ();
							GUILayout.Label (new GUIContent ("   Weight 2", "Second layer bloom weight."), GUILayout.Width (90));
							_effect.bloomWeight1 = EditorGUILayout.Slider (_effect.bloomWeight1, 0f, 1f);
							EditorGUILayout.EndHorizontal ();
							EditorGUILayout.BeginHorizontal ();
							GUILayout.Label (new GUIContent ("   Weight 3", "Third layer bloom weight."), GUILayout.Width (90));
							_effect.bloomWeight2 = EditorGUILayout.Slider (_effect.bloomWeight2, 0f, 1f);
							EditorGUILayout.EndHorizontal ();
							EditorGUILayout.BeginHorizontal ();
							GUILayout.Label (new GUIContent ("   Weight 4", "Fourth layer bloom weight."), GUILayout.Width (90));
							_effect.bloomWeight3 = EditorGUILayout.Slider (_effect.bloomWeight3, 0f, 1f);
							EditorGUILayout.EndHorizontal ();
							EditorGUILayout.BeginHorizontal ();
							GUILayout.Label (new GUIContent ("   Weight 5", "Fifth layer bloom weight."), GUILayout.Width (90));
							_effect.bloomWeight4 = EditorGUILayout.Slider (_effect.bloomWeight4, 0f, 1f);
							EditorGUILayout.EndHorizontal ();
							EditorGUILayout.BeginHorizontal ();
							GUILayout.Label (new GUIContent ("   Weight 6", "Sixth layer bloom weight."), GUILayout.Width (90));
							_effect.bloomWeight5 = EditorGUILayout.Slider (_effect.bloomWeight5, 0f, 1f);
						}
					}
				}
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			labelStyle = _effect.anamorphicFlares ? sectionHeaderBoldStyle : sectionHeaderNormalStyle;
			expandAFSection = EditorGUILayout.Foldout (expandAFSection, new GUIContent ("Anamorphic F.", "Also known as JJ Abrams flares, adds spectacular light streaks to your scene."), labelStyle); 
			EditorGUILayout.EndHorizontal ();
			if (isFeatureEnabled (Beautify.SKW_BLOOM)) {
				_effect.anamorphicFlares = EditorGUILayout.Toggle (_effect.anamorphicFlares);
				if (expandAFSection) {
					if (!_effect.bloom) {
						GUILayout.Label (new GUIContent ("Debug", "Enable to see bloom/anamorphic flares channel."));
						_effect.bloomDebug = EditorGUILayout.Toggle (_effect.bloomDebug, GUILayout.Width (40));
					}
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Intensity", "Flares light multiplier."), GUILayout.Width (90));
					_effect.anamorphicFlaresIntensity = EditorGUILayout.Slider (_effect.anamorphicFlaresIntensity, 0f, 10f);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Threshold", "Brightness sensibility."), GUILayout.Width (90));
					_effect.anamorphicFlaresThreshold = EditorGUILayout.Slider (_effect.anamorphicFlaresThreshold, 0f, 5f);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Spread", "Amplitude of the flares."), GUILayout.Width (90));
					_effect.anamorphicFlaresSpread = EditorGUILayout.Slider (_effect.anamorphicFlaresSpread, 0.1f, 2f);
					GUILayout.Label ("Vertical");
					_effect.anamorphicFlaresVertical = EditorGUILayout.Toggle (_effect.anamorphicFlaresVertical, GUILayout.Width (20));
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Tint", "Optional tint color for the anamorphic flares. Use color alpha component to blend between original color and the tint."), GUILayout.Width (90));
					_effect.anamorphicFlaresTint = EditorGUILayout.ColorField (_effect.anamorphicFlaresTint);
					if (_effect.quality == BEAUTIFY_QUALITY.BestQuality) {
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent ("   Reduce Flicker", "Enables an additional filter to reduce excess of flicker."), GUILayout.Width (90));
						_effect.anamorphicFlaresAntiflicker = EditorGUILayout.Toggle (_effect.anamorphicFlaresAntiflicker);
					}
				}
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			labelStyle = _effect.lensDirt ? sectionHeaderBoldStyle : sectionHeaderNormalStyle;
			expandDirtSection = EditorGUILayout.Foldout (expandDirtSection, new GUIContent ("Lens Dirt", "Enables lens dirt effect which intensifies when looking to a light (uses the nearest light to camera). You can assign other dirt textures directly to the shader material with name 'Beautify'."), labelStyle); 
			EditorGUILayout.EndHorizontal ();
			if (isFeatureEnabled (Beautify.SKW_DIRT)) {
				_effect.lensDirt = EditorGUILayout.Toggle (_effect.lensDirt);
				if (expandDirtSection) {
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Dirt Texture", "Texture used for the lens dirt effect."), GUILayout.Width (90));
					_effect.lensDirtTexture = (Texture2D)EditorGUILayout.ObjectField (_effect.lensDirtTexture, typeof(Texture2D), false);
					if (GUILayout.Button ("?", GUILayout.Width (20))) {
						EditorUtility.DisplayDialog ("Lens Dirt Texture", "You can find additional lens dirt textures inside \nBeautify/Resources/Textures folder.", "Ok");
					}
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Threshold", "This slider controls the visibility of lens dirt. A high value will make lens dirt only visible when looking directly towards a light source. A lower value will make lens dirt visible all time."), GUILayout.Width (90));
					_effect.lensDirtThreshold = EditorGUILayout.Slider (_effect.lensDirtThreshold, 0f, 1f);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Intensity", "This slider controls the maximum brightness of lens dirt effect."), GUILayout.Width (90));
					_effect.lensDirtIntensity = EditorGUILayout.Slider (_effect.lensDirtIntensity, 0f, 1f);
				}
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			labelStyle = _effect.depthOfField ? sectionHeaderBoldStyle : sectionHeaderNormalStyle;
			expandDoFSection = EditorGUILayout.Foldout (expandDoFSection, new GUIContent ("Depth of Field", "Blurs the image based on distance to focus point."), labelStyle); 
			EditorGUILayout.EndHorizontal ();
			if (isFeatureEnabled (Beautify.SKW_DEPTH_OF_FIELD)) {
				_effect.depthOfField = EditorGUILayout.Toggle (_effect.depthOfField);
				if (expandDoFSection) {
					if (_effect.depthOfField) {
						GUILayout.Label (new GUIContent ("Debug", "Enable to see depth of field focus area."));
						_effect.depthOfFieldDebug = EditorGUILayout.Toggle (_effect.depthOfFieldDebug, GUILayout.Width (40));
					}
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Autofocus", "Automatically focus the object in front of camera."), GUILayout.Width (90));
					_effect.depthOfFieldAutofocus = EditorGUILayout.Toggle (_effect.depthOfFieldAutofocus);
					EditorGUILayout.EndHorizontal ();
					if (_effect.depthOfFieldAutofocus) {
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent ("   Layer Mask", "Select which layers can be used for autofocus option."), GUILayout.Width (90));
						_effect.depthOfFieldAutofocusLayerMask = LayerMaskField(_effect.depthOfFieldAutofocusLayerMask);
						EditorGUILayout.EndHorizontal ();
					}
					if (!_effect.depthOfFieldAutofocus) {
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent ("   Focus Target", "Dynamically focus target."), GUILayout.Width (90));
						_effect.depthOfFieldTargetFocus = (Transform)EditorGUILayout.ObjectField (_effect.depthOfFieldTargetFocus, typeof(Transform), true);
						EditorGUILayout.EndHorizontal ();
						if (_effect.depthOfFieldTargetFocus == null) {
							EditorGUILayout.BeginHorizontal ();
							GUILayout.Label (new GUIContent ("   Focus Distance", "Distance to focus point."), GUILayout.Width (120));
							_effect.depthOfFieldDistance = EditorGUILayout.Slider (_effect.depthOfFieldDistance, 1f, 100f);
							EditorGUILayout.EndHorizontal ();
						}
					}
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Focus Speed", "1=immediate focus on distance or target."), GUILayout.Width (_effect.depthOfFieldTargetFocus == null ? 120 : 90));
					_effect.depthOfFieldFocusSpeed = EditorGUILayout.Slider (_effect.depthOfFieldFocusSpeed, 0.001f, 1f);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Focal Length", "Focal length of the virtual lens."), GUILayout.Width (_effect.depthOfFieldTargetFocus == null ? 120 : 90));
					_effect.depthOfFieldFocalLength = EditorGUILayout.Slider (_effect.depthOfFieldFocalLength, 0.005f, 0.5f);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Aperture", "Diameter of the aperture (mm)."), GUILayout.Width (90));
					GUILayout.Label ("f/", GUILayout.Width (15));
					_effect.depthOfFieldAperture = EditorGUILayout.FloatField (_effect.depthOfFieldAperture, GUILayout.Width (40));
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Foreground Blur", "Enables blur in front of focus object."), GUILayout.Width (120));
					_effect.depthOfFieldForegroundBlur = EditorGUILayout.Toggle (_effect.depthOfFieldForegroundBlur, GUILayout.Width (40));
					if (_effect.depthOfFieldForegroundBlur) {
						GUILayout.Label (new GUIContent ("Offset", "Distance from focus plane for foreground blur."), GUILayout.Width (50));
						_effect.depthOfFieldForegroundDistance = EditorGUILayout.FloatField (_effect.depthOfFieldForegroundDistance, GUILayout.Width (50));
					}
					EditorGUILayout.EndHorizontal ();

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Bokeh", "Bright spots will be augmented in defocused areas."), GUILayout.Width (120));
					_effect.depthOfFieldBokeh = EditorGUILayout.Toggle (_effect.depthOfFieldBokeh, GUILayout.Width (40));
					if (_effect.depthOfFieldBokeh) {
						if (_effect.depthOfFieldBokeh) {
							GUILayout.Label (new GUIContent ("Threshold", "Determines which bright spots will be augmented in defocused areas."));
							_effect.depthOfFieldBokehThreshold = EditorGUILayout.Slider (_effect.depthOfFieldBokehThreshold, 0.5f, 3f);
						}
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent ("   Bokeh Intensity", "Intensity multiplier for bright spots in defocused areas."), GUILayout.Width (120));
						_effect.depthOfFieldBokehIntensity = EditorGUILayout.Slider (_effect.depthOfFieldBokehIntensity, 0f, 8f);
					}
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Downsampling", "Reduces screen buffer size to improve performance."), GUILayout.Width (120));
					_effect.depthOfFieldDownsampling = EditorGUILayout.IntSlider (_effect.depthOfFieldDownsampling, 1, 5);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Sample Count", "Determines the maximum number of samples to be gathered in the effect."), GUILayout.Width (120));
					_effect.depthOfFieldMaxSamples = EditorGUILayout.IntSlider (_effect.depthOfFieldMaxSamples, 2, 16);
					GUILayout.Label ("(" + ((_effect.depthOfFieldMaxSamples - 1) * 2 + 1) + " samples)");
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Transparency", "Enable transparency support."), GUILayout.Width (90));
					_effect.depthOfFieldTransparencySupport = EditorGUILayout.Toggle (_effect.depthOfFieldTransparencySupport);
				}
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			labelStyle = _effect.eyeAdaptation ? sectionHeaderBoldStyle : sectionHeaderNormalStyle;
			expandEASection = EditorGUILayout.Foldout (expandEASection, new GUIContent ("Eye Adaptation", "Enables eye adaptation effect. Simulates retina response to quick luminance changes in the scene."), labelStyle); 
			EditorGUILayout.EndHorizontal ();
			if (isFeatureEnabled (Beautify.SKW_EYE_ADAPTATION)) {
				_effect.eyeAdaptation = EditorGUILayout.Toggle (_effect.eyeAdaptation);
				if (expandEASection) {
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Min Exposure", GUILayout.Width (90));
					_effect.eyeAdaptationMinExposure = EditorGUILayout.Slider (_effect.eyeAdaptationMinExposure, 0f, 1f);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Max Exposure", GUILayout.Width (90));
					_effect.eyeAdaptationMaxExposure = EditorGUILayout.Slider (_effect.eyeAdaptationMaxExposure, 1f, 5f);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Dark Adapt Speed", GUILayout.Width (120));
					_effect.eyeAdaptationSpeedToDark = EditorGUILayout.Slider (_effect.eyeAdaptationSpeedToDark, 0f, 1f);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Light Adapt Speed", GUILayout.Width (120));
					_effect.eyeAdaptationSpeedToLight = EditorGUILayout.Slider (_effect.eyeAdaptationSpeedToLight, 0f, 1f);
				}
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			labelStyle = _effect.purkinje ? sectionHeaderBoldStyle : sectionHeaderNormalStyle;
			expandPurkinjeSection = EditorGUILayout.Foldout (expandPurkinjeSection, new GUIContent ("Purkinje", "Simulates achromatic vision plus spectrum shift to blue in the dark."), labelStyle); 
			EditorGUILayout.EndHorizontal ();
			if (isFeatureEnabled (Beautify.SKW_PURKINJE)) {
				_effect.purkinje = EditorGUILayout.Toggle (_effect.purkinje);
				if (expandPurkinjeSection) {
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Shift Amount", "Spectrum shift to blue. A value of zero will not shift colors and stay in grayscale."), GUILayout.Width (90));
					_effect.purkinjeAmount = EditorGUILayout.Slider (_effect.purkinjeAmount, 0f, 5f);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Threshold", "Increase this value to augment the purkinje effect (applies to higher luminance levels)."), GUILayout.Width (90));
					_effect.purkinjeLuminanceThreshold = EditorGUILayout.Slider (_effect.purkinjeLuminanceThreshold, 0f, 1f);
				}
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.Separator ();
			DrawLabel ("Artistic Choices");

			if (_effect.vignetting || _effect.frame || _effect.outline || _effect.nightVision || _effect.thermalVision) {
				EditorGUILayout.HelpBox ("Customize the effects below using color picker. Alpha has special meaning depending on effect. Read the tooltip moving the mouse over the effect name.", MessageType.Info);
			}

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			labelStyle = _effect.vignetting ? sectionHeaderBoldStyle : sectionHeaderNormalStyle;
			expandVignettingSection = EditorGUILayout.Foldout (expandVignettingSection, new GUIContent ("Vignetting", "Enables colored vignetting effect. Color alpha specifies intensity of effect."), labelStyle); 
			EditorGUILayout.EndHorizontal ();
			_effect.vignetting = EditorGUILayout.Toggle (_effect.vignetting);
			if (expandVignettingSection) {
				GUILayout.Label (new GUIContent ("Color", "The color for the vignetting effect. Alpha specifies intensity of effect."), GUILayout.Width (40));
				_effect.vignettingColor = EditorGUILayout.ColorField (_effect.vignettingColor);
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent ("   Circular Shape", "Ignores screen aspect ratio showing a circular shape."), GUILayout.Width (90));
				_effect.vignettingCircularShape = EditorGUILayout.Toggle (_effect.vignettingCircularShape);
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			labelStyle = _effect.frame ? labelBoldStyle : labelNormalStyle;
			GUILayout.Label (new GUIContent ("Frame", "Enables colored frame effect. Color alpha specifies intensity of effect."), labelStyle, GUILayout.Width (90));
			_effect.frame = EditorGUILayout.Toggle (_effect.frame);
			if (_effect.frame) {
				GUILayout.Label (new GUIContent ("Color", "The color for the frame effect. Alpha specifies intensity of effect."), GUILayout.Width (40));
				_effect.frameColor = EditorGUILayout.ColorField (_effect.frameColor);
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			labelStyle = _effect.outline ? labelBoldStyle : labelNormalStyle;
			GUILayout.Label (new GUIContent ("Outline", "Enables outline (edge detection) effect. Color alpha specifies edge detection threshold."), labelStyle, GUILayout.Width (90));
			if (isFeatureEnabled (Beautify.SKW_OUTLINE)) {
				_effect.outline = EditorGUILayout.Toggle (_effect.outline);
				if (_effect.outline) {
					GUILayout.Label (new GUIContent ("Color", "The color for the outline. Alpha specifies edge detection threshold."), GUILayout.Width (40));
					_effect.outlineColor = EditorGUILayout.ColorField (_effect.outlineColor);
				}
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			labelStyle = _effect.sepia ? labelBoldStyle : labelNormalStyle;
			GUILayout.Label (new GUIContent ("Sepia", "Enables sepia color effect."), labelStyle, GUILayout.Width (90));
			if (isFeatureEnabled (Beautify.SKW_SEPIA)) {
				_effect.sepia = EditorGUILayout.Toggle (_effect.sepia, GUILayout.Width (40));
				if (_effect.sepia) {
					GUILayout.Label ("Intensity");
					_effect.sepiaIntensity = EditorGUILayout.Slider (_effect.sepiaIntensity, 0f, 1f);
				}
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			labelStyle = _effect.nightVision ? labelBoldStyle : labelNormalStyle;
			GUILayout.Label (new GUIContent ("Night Vision", "Enables night vision effect. Color alpha controls intensity. For a better result, enable Vignetting and set its color to (0,0,0,32)."), labelStyle, GUILayout.Width (90));
			if (isFeatureEnabled (Beautify.SKW_NIGHT_VISION)) {
				_effect.nightVision = EditorGUILayout.Toggle (_effect.nightVision);
				if (_effect.nightVision) {
					GUILayout.Label (new GUIContent ("Color", "The color for the night vision effect. Alpha controls intensity."), GUILayout.Width (40));
					_effect.nightVisionColor = EditorGUILayout.ColorField (_effect.nightVisionColor);
				}
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			labelStyle = _effect.thermalVision ? labelBoldStyle : labelNormalStyle;
			GUILayout.Label (new GUIContent ("Thermal Vision", "Enables thermal vision effect."), labelStyle, GUILayout.Width (90));
			if (isFeatureEnabled (Beautify.SKW_THERMAL_VISION)) {
				_effect.thermalVision = EditorGUILayout.Toggle (_effect.thermalVision);
			}
			EditorGUILayout.EndHorizontal ();

			if (_effect.isDirty) {
				EditorUtility.SetDirty (target);
			}


		}

		void DrawLabel (string s)
		{
			if (titleLabelStyle == null) {
				GUIStyle skurikenModuleTitleStyle = "ShurikenModuleTitle";
				titleLabelStyle = new GUIStyle (skurikenModuleTitleStyle);
				titleLabelStyle.contentOffset = new Vector2 (5f, -2f);
				titleLabelStyle.normal.textColor = titleColor;
				titleLabelStyle.fixedHeight = 22;
				titleLabelStyle.fontStyle = FontStyle.Bold;
			}

			GUILayout.Label (s, titleLabelStyle);
		}

		Texture2D MakeTex (int width, int height, Color col)
		{
			Color[] pix = new Color[width * height];
			
			for (int i = 0; i < pix.Length; i++)
				pix [i] = col;
			
			Texture2D result = new Texture2D (width, height);
			result.hideFlags = HideFlags.DontSave;
			result.SetPixels (pix);
			result.Apply ();
			
			return result;
		}

		LayerMask LayerMaskField(LayerMask layerMask) {
			List<string> layers = new List<string>();
			List<int> layerNumbers = new List<int>();
			
			for (int i = 0; i < 32; i++) {
				string layerName = LayerMask.LayerToName(i);
				if (layerName != "") {
					layers.Add(layerName);
					layerNumbers.Add(i);
				}
			}
			int maskWithoutEmpty = 0;
			for (int i = 0; i < layerNumbers.Count; i++) {
				if (((1 << layerNumbers[i]) & layerMask.value) > 0)
					maskWithoutEmpty |= (1 << i);
			}
			maskWithoutEmpty = EditorGUILayout.MaskField( "", maskWithoutEmpty, layers.ToArray());
			int mask = 0;
			for (int i = 0; i < layerNumbers.Count; i++) {
				if ((maskWithoutEmpty & (1 << i)) > 0)
					mask |= (1 << layerNumbers[i]);
			}
			layerMask.value = mask;
			return layerMask;
		}


			#region Shader handling
			
		void ScanKeywords ()
		{
			if (shaders == null)
				shaders = new List<BeautifySInfo> ();
			else
				shaders.Clear ();
			string[] guids = AssetDatabase.FindAssets ("Beautify t:Shader");
			for (int k = 0; k < guids.Length; k++) {
				string guid = guids [k];
				string path = AssetDatabase.GUIDToAssetPath (guid);
				BeautifySInfo shader = new BeautifySInfo ();
				shader.path = path;
				shader.name = Path.GetFileNameWithoutExtension (path);
				ScanShader (shader);
				if (shader.keywords.Count > 0) {
					shaders.Add (shader);
				}
			}
		}
			
		void ScanShader (BeautifySInfo shader)
		{
			// Inits shader
			shader.passes.Clear ();
			shader.keywords.Clear ();
			shader.pendingChanges = false;
			shader.editedByShaderControl = false;
				
			// Reads shader
			string[] shaderLines = File.ReadAllLines (shader.path);
			string[] separator = new string[] { " " };
			SCShaderPass currentPass = new SCShaderPass ();
			SCShaderPass basePass = null;
			int pragmaControl = 0;
			int pass = -1;
			SCKeywordLine keywordLine = new SCKeywordLine ();
			for (int k = 0; k < shaderLines.Length; k++) {
				string line = shaderLines [k].Trim ();
				if (line.Length == 0)
					continue;
				string lineUPPER = line.ToUpper ();
				if (lineUPPER.Equals ("PASS") || lineUPPER.StartsWith ("PASS ")) {
					if (pass >= 0) {
						currentPass.pass = pass;
						if (basePass != null)
							currentPass.Add (basePass.keywordLines);
						shader.Add (currentPass);
					} else if (currentPass.keywordCount > 0) {
						basePass = currentPass;
					}
					currentPass = new SCShaderPass ();
					pass++;
					continue;
				}
				int j = line.IndexOf (PRAGMA_COMMENT_MARK);
				if (j >= 0) {
					pragmaControl = 1;
				} else {
					j = line.IndexOf (PRAGMA_DISABLED_MARK);
					if (j >= 0)
						pragmaControl = 3;
				}
				j = line.IndexOf (PRAGMA_MULTICOMPILE);
				if (j >= 0) {
					if (pragmaControl != 2)
						keywordLine = new SCKeywordLine ();
					string[] kk = line.Substring (j + 22).Split (separator, StringSplitOptions.RemoveEmptyEntries);
					// Sanitize keywords
					for (int i = 0; i < kk.Length; i++)
						kk [i] = kk [i].Trim ();
					// Act on keywords
					switch (pragmaControl) {
					case 1: // Edited by Shader Control line
						shader.editedByShaderControl = true;
							// Add original keywords to current line
						for (int s = 0; s < kk.Length; s++) {
							keywordLine.Add (shader.GetKeyword (kk [s]));
						}
						pragmaControl = 2;
						break;
					case 2:
							// check enabled keywords
						keywordLine.DisableKeywords ();
						for (int s = 0; s < kk.Length; s++) {
							SCKeyword keyword = keywordLine.GetKeyword (kk [s]);
							if (keyword != null)
								keyword.enabled = true;
						}
						currentPass.Add (keywordLine);
						pragmaControl = 0;
						break;
					case 3: // disabled by Shader Control line
						shader.editedByShaderControl = true;
							// Add original keywords to current line
						for (int s = 0; s < kk.Length; s++) {
							SCKeyword keyword = shader.GetKeyword (kk [s]);
							keyword.enabled = false;
							keywordLine.Add (keyword);
						}
						currentPass.Add (keywordLine);
						pragmaControl = 0;
						break;
					case 0:
							// Add keywords to current line
						for (int s = 0; s < kk.Length; s++) {
							keywordLine.Add (shader.GetKeyword (kk [s]));
						}
						currentPass.Add (keywordLine);
						break;
					}
				}
			}
			currentPass.pass = Mathf.Max (pass, 0);
			if (basePass != null)
				currentPass.Add (basePass.keywordLines);
			shader.Add (currentPass);
		}
			
		void UpdateShaders ()
		{
			// normalize keywords
			if (shaders.Count > 0) {
				BeautifySInfo mainShader = shaders [0];
				BeautifySInfo shader = shaders [1];
				for (int k=0; k<mainShader.keywords.Count; k++) {
					SCKeyword mainKeyword = mainShader.keywords [k];
					SCKeyword keyword = shader.GetKeyword (mainKeyword.name);
					keyword.enabled = mainKeyword.enabled;
				}
			}

			// Update shader files
			for (int k=0; k<shaders.Count; k++) {
				BeautifySInfo shader = shaders [k];
				UpdateShader (shader);
			}
		}

		void UpdateShader (BeautifySInfo shader)
		{

			// Reads and updates shader from disk
			string[] shaderLines = File.ReadAllLines (shader.path);
			string[] separator = new string[] { " " };
			StringBuilder sb = new StringBuilder ();
			int pragmaControl = 0;
			shader.editedByShaderControl = false;
			SCKeywordLine keywordLine = new SCKeywordLine ();
			for (int k = 0; k < shaderLines.Length; k++) {
				int j = shaderLines [k].IndexOf (PRAGMA_COMMENT_MARK);
				if (j >= 0)
					pragmaControl = 1;
				j = shaderLines [k].IndexOf (PRAGMA_MULTICOMPILE);
				if (j >= 0) {
					if (pragmaControl != 2)
						keywordLine.Clear ();
					string[] kk = shaderLines [k].Substring (j + 22).Split (separator, StringSplitOptions.RemoveEmptyEntries);
					// Sanitize keywords
					for (int i = 0; i < kk.Length; i++)
						kk [i] = kk [i].Trim ();
					// Act on keywords
					switch (pragmaControl) {
					case 1:
							// Read original keywords
						for (int s = 0; s < kk.Length; s++) {
							SCKeyword keyword = shader.GetKeyword (kk [s]);
							keywordLine.Add (keyword);
						}
						pragmaControl = 2;
						break;
					case 0:
					case 2:
						if (pragmaControl == 0) {
							for (int s = 0; s < kk.Length; s++) {
								SCKeyword keyword = shader.GetKeyword (kk [s]);
								keywordLine.Add (keyword);
							}
						}
						int kCount = keywordLine.keywordCount;
						int kEnabledCount = keywordLine.keywordsEnabledCount;
						if (kEnabledCount < kCount) {
							// write original keywords
							if (kEnabledCount == 0) {
								sb.Append (PRAGMA_DISABLED_MARK);
							} else {
								sb.Append (PRAGMA_COMMENT_MARK);
							}
							shader.editedByShaderControl = true;
							sb.Append (PRAGMA_MULTICOMPILE);
							if (keywordLine.hasUnderscoreVariant)
								sb.Append (PRAGMA_UNDERSCORE);
							for (int s = 0; s < kCount; s++) {
								SCKeyword keyword = keywordLine.keywords [s];
								sb.Append (keyword.name);
								if (s < kCount - 1)
									sb.Append (" ");
							}
							sb.AppendLine ();
						}
							
						if (kEnabledCount > 0) {
							// Write actual keywords
							sb.Append (PRAGMA_MULTICOMPILE);
							if (keywordLine.hasUnderscoreVariant)
								sb.Append (PRAGMA_UNDERSCORE);
							for (int s = 0; s < kCount; s++) {
								SCKeyword keyword = keywordLine.keywords [s];
								if (keyword.enabled) {
									sb.Append (keyword.name);
									if (s < kCount - 1)
										sb.Append (" ");
								}
							}
							sb.AppendLine ();
						}
						pragmaControl = 0;
						break;
					}
				} else {
					sb.AppendLine (shaderLines [k]);
				}
			}
				
			// Writes modified shader
			File.WriteAllText (shader.path, sb.ToString ());
				
			AssetDatabase.Refresh ();
				
			ScanShader (shader); // Rescan shader
		}

		bool isFeatureEnabled (string name)
		{
			if (shaders.Count == 0)
				return false;
			SCKeyword keyword = shaders [0].GetKeyword (name);
			if (!keyword.enabled) {
				GUILayout.Label ("(feature disabled in build options)");
				return false;
			}
			return true;
		}
			
			
			#endregion

	}

}
