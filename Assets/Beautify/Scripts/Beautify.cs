/// <summary>
/// Copyright 2016 Kronnect - All rights reserved
/// </summary>
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//#define DEBUG_BEAUTIFY

namespace BeautifyEffect
{
	public enum BEAUTIFY_QUALITY
	{
		BestQuality,
		BestPerformance
	}

	public enum BEAUTIFY_PRESET
	{
		Soft = 10,
		Medium = 20,
		Strong = 30,
		Exaggerated = 40,
		Custom = 999
	}

	public enum BEAUTIFY_TMO {
		Linear = 0,
		ACES   = 10
	}

	[ExecuteInEditMode, RequireComponent (typeof(Camera)), DisallowMultipleComponent]
	[AddComponentMenu ("Image Effects/Rendering/Beautify")]
	[HelpURL("http://kronnect.com/taptapgo")]
	#if UNITY_5_4_OR_NEWER
	[ImageEffectAllowedInSceneView]
	#endif
	public class Beautify : MonoBehaviour
	{
	

								#region General settings

		[SerializeField]
		BEAUTIFY_PRESET
			_preset = BEAUTIFY_PRESET.Medium;

		public BEAUTIFY_PRESET preset {
			get { return _preset; }
			set {
				if (_preset != value) {
					_preset = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		BEAUTIFY_QUALITY
			_quality = BEAUTIFY_QUALITY.BestQuality;

		public BEAUTIFY_QUALITY quality {
			get { return _quality; }
			set {
				if (_quality != value) {
					_quality = value;
					UpdateQualitySettings ();
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		bool
			_compareMode = false;

		public bool compareMode {
			get { return _compareMode; }
			set {
				if (_compareMode != value) {
					_compareMode = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_compareLineAngle = 1.4f;

		public float compareLineAngle {
			get { return _compareLineAngle; }
			set {
				if (_compareLineAngle != value) {
					_compareLineAngle = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_compareLineWidth = 0.002f;

		public float compareLineWidth {
			get { return _compareLineWidth; }
			set {
				if (_compareLineWidth != value) {
					_compareLineWidth = value;
					UpdateMaterialProperties ();
				}
			}
		}

		#endregion

		#region RGB Dither
		
		[SerializeField]
		float
			_dither = 0.02f;
		
		public float dither {
			get { return _dither; }
			set {
				if (_dither != value) {
					_preset = BEAUTIFY_PRESET.Custom;
					_dither = value;
					UpdateMaterialProperties ();
				}
			}
		}
		
		[SerializeField]
		float
			_ditherDepth = 0f;
		
		public float ditherDepth {
			get { return _ditherDepth; }
			set {
				if (_ditherDepth != value) {
					_preset = BEAUTIFY_PRESET.Custom;
					_ditherDepth = value;
					UpdateMaterialProperties ();
				}
			}
		}
		
		#endregion

		#region Sharpen Settings


		[SerializeField]
		float
			_sharpenMinDepth = 0f;

		public float sharpenMinDepth {
			get { return _sharpenMinDepth; }
			set {
				if (_sharpenMinDepth != value) {
					_sharpenMinDepth = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_sharpenMaxDepth = 0.999f;

		public float sharpenMaxDepth {
			get { return _sharpenMaxDepth; }
			set {
				if (_sharpenMaxDepth != value) {
					_sharpenMaxDepth = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_sharpen = 2f;

		public float sharpen {
			get { return _sharpen; }
			set {
				if (_sharpen != value) {
					_preset = BEAUTIFY_PRESET.Custom;
					_sharpen = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_sharpenDepthThreshold = 0.035f;

		public float sharpenDepthThreshold {
			get { return _sharpenDepthThreshold; }
			set {
				if (_sharpenDepthThreshold != value) {
					_preset = BEAUTIFY_PRESET.Custom;
					_sharpenDepthThreshold = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		Color
			_tintColor = new Color (1, 1, 1, 0);

		public Color tintColor {
			get { return _tintColor; }
			set {
				if (_tintColor != value) {
					_tintColor = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_sharpenRelaxation = 0.08f;

		public float sharpenRelaxation {
			get { return _sharpenRelaxation; }
			set {
				if (_sharpenRelaxation != value) {
					_preset = BEAUTIFY_PRESET.Custom;
					_sharpenRelaxation = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_sharpenClamp = 0.45f;

		public float sharpenClamp {
			get { return _sharpenClamp; }
			set {
				if (_sharpenClamp != value) {
					_preset = BEAUTIFY_PRESET.Custom;
					_sharpenClamp = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_sharpenMotionSensibility = 0.5f;

		public float sharpenMotionSensibility {
			get { return _sharpenMotionSensibility; }
			set {
				if (_sharpenMotionSensibility != value) {
					_sharpenMotionSensibility = value;
					UpdateMaterialProperties ();
				}
			}
		}

		#endregion

		#region Color grading

		[SerializeField]
		float
			_saturate = 1f;

		public float saturate {
			get { return _saturate; }
			set {
				if (_saturate != value) {
					_preset = BEAUTIFY_PRESET.Custom;
					_saturate = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_contrast = 1.02f;

		public float contrast {
			get { return _contrast; }
			set {
				if (_contrast != value) {
					_preset = BEAUTIFY_PRESET.Custom;
					_contrast = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_brightness = 1.05f;

		public float brightness {
			get { return _brightness; }
			set {
				if (_brightness != value) {
					_preset = BEAUTIFY_PRESET.Custom;
					_brightness = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_daltonize = 0f;
		
		public float daltonize {
			get { return _daltonize; }
			set {
				if (_daltonize != value) {
					_preset = BEAUTIFY_PRESET.Custom;
					_daltonize = value;
					UpdateMaterialProperties ();
				}
			}
		}

		#endregion

		#region Vignetting

		[SerializeField]
		bool
			_vignetting = false;

		public bool vignetting {
			get { return _vignetting; }
			set {
				if (_vignetting != value) {
					_vignetting = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		Color
			_vignettingColor = new Color (0.3f, 0.3f, 0.3f, 0.05f);

		public Color vignettingColor {
			get { return _vignettingColor; }
			set {
				if (_vignettingColor != value) {
					_vignettingColor = value;
					UpdateMaterialProperties ();
				}
			}
		}
		
		[SerializeField]
		bool
			_vignettingCircularShape = false;

		public bool vignettingCircularShape {
			get { return _vignettingCircularShape; }
			set {
				if (_vignettingCircularShape != value) {
					_vignettingCircularShape = value;
					UpdateMaterialProperties ();
				}
			}
		}

		#endregion

		#region Frame

		[SerializeField]
		bool
			_frame = false;

		public bool frame {
			get { return _frame; }
			set {
				if (_frame != value) {
					_frame = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		Color
			_frameColor = new Color (1, 1, 1, 0.047f);

		public Color frameColor {
			get { return _frameColor; }
			set {
				if (_frameColor != value) {
					_frameColor = value;
					UpdateMaterialProperties ();
				}
			}
		}

		#endregion

		#region Sepia

		[SerializeField]
		bool
			_sepia = false;

		public bool sepia {
			get { return _sepia; }
			set {
				if (_sepia != value) { 
					_sepia = value; 
					if (_sepia) {
						_nightVision = false;
						_thermalVision = false;
					}
					UpdateMaterialProperties (); 
				} 
			}
		}

		[SerializeField]
		float
			_sepiaIntensity = 1f;

		public float sepiaIntensity {
			get { return _sepiaIntensity; }
			set {
				if (_sepiaIntensity != value) {
					_sepiaIntensity = value;
					UpdateMaterialProperties ();
				}
			}
		}

		#endregion

		#region Night Vision

		[SerializeField]
		bool
			_nightVision = false;

		public bool nightVision {
			get { return _nightVision; }
			set {
				if (_nightVision != value) {
					_nightVision = value; 
					if (_nightVision) {
						_thermalVision = false;
						_sepia = false;
						_vignetting = true;
						_vignettingColor = new Color (0, 0, 0, 32f / 255f);
						_vignettingCircularShape = true;
					} else {
						_vignetting = false;
					}
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		Color
			_nightVisionColor = new Color (0.5f, 1f, 0.5f, 0.5f);

		public Color nightVisionColor {
			get { return _nightVisionColor; }
			set {
				if (_nightVisionColor != value) { 
					_nightVisionColor = value;
					UpdateMaterialProperties ();
				}
			}
		}

		#endregion

		#region Outline

		[SerializeField]
		bool
			_outline = false;

		public bool outline {
			get { return _outline; }
			set {
				if (_outline != value) {
					_outline = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		Color
			_outlineColor = new Color (0, 0, 0, 0.8f);

		public Color outlineColor {
			get { return _outlineColor; }
			set {
				if (_outlineColor != value) {
					_outlineColor = value;
					UpdateMaterialProperties ();
				}
			}
		}

		#endregion

		#region Thermal Vision

		[SerializeField]
		bool
			_thermalVision = false;

		public bool thermalVision {
			get { return _thermalVision; }
			set {
				if (_thermalVision != value) {
					_thermalVision = value;
					if (_thermalVision) {
						_nightVision = false;
						_sepia = false;
						_vignetting = true;
						_vignettingColor = new Color (1f, 16f / 255f, 16f / 255f, 18f / 255f);
						_vignettingCircularShape = true;
					} else {
						_vignetting = false;
					}
					UpdateMaterialProperties ();
				} 
			}
		}

		#endregion

		#region Lens Dirt

		[SerializeField]
		bool
			_lensDirt = false;

		public bool lensDirt {
			get { return _lensDirt; }
			set {
				if (_lensDirt != value) {
					_lensDirt = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_lensDirtThreshold = 0.5f;

		public float lensDirtThreshold {
			get { return _lensDirtThreshold; }
			set {
				if (_lensDirtThreshold != value) {
					_lensDirtThreshold = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_lensDirtIntensity = 0.9f;

		public float lensDirtIntensity {
			get { return _lensDirtIntensity; }
			set {
				if (_lensDirtIntensity != value) {
					_lensDirtIntensity = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		Texture2D
			_lensDirtTexture;

		public Texture2D lensDirtTexture {
			get { return _lensDirtTexture; }
			set {
				if (_lensDirtTexture != value) {
					_lensDirtTexture = value;
					UpdateMaterialProperties ();
				}
			}
		}

		#endregion

		#region Bloom

		[SerializeField]
		bool
			_bloom = false;

		public bool bloom {
			get { return _bloom; }
			set {
				if (_bloom != value) {
					_bloom = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_bloomIntensity = 1f;

		public float bloomIntensity {
			get { return _bloomIntensity; }
			set {
				if (_bloomIntensity != value) {
					_bloomIntensity = Mathf.Abs (value);
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		bool
			_bloomAntiflicker = false;

		public bool bloomAntiflicker {
			get { return _bloomAntiflicker; }
			set {
				if (_bloomAntiflicker != value) {
					_bloomAntiflicker = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_bloomThreshold = 0.75f;

		public float bloomThreshold {
			get { return _bloomThreshold; }
			set {
				if (_bloomThreshold != value) {
					_bloomThreshold = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		bool
			_bloomCustomize = false;

		public bool bloomCustomize {
			get { return _bloomCustomize; }
			set {
				if (_bloomCustomize != value) {
					_bloomCustomize = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		bool
			_bloomDebug = false;

		public bool bloomDebug {
			get { return _bloomDebug; }
			set {
				if (_bloomDebug != value) {
					_bloomDebug = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_bloomWeight0 = 0.5f;

		public float bloomWeight0 {
			get { return _bloomWeight0; }
			set {
				if (_bloomWeight0 != value) {
					_bloomWeight0 = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_bloomWeight1 = 0.5f;

		public float bloomWeight1 {
			get { return _bloomWeight1; }
			set {
				if (_bloomWeight1 != value) {
					_bloomWeight1 = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_bloomWeight2 = 0.5f;

		public float bloomWeight2 {
			get { return _bloomWeight2; }
			set {
				if (_bloomWeight2 != value) {
					_bloomWeight2 = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_bloomWeight3 = 0.5f;

		public float bloomWeight3 {
			get { return _bloomWeight3; }
			set {
				if (_bloomWeight3 != value) {
					_bloomWeight3 = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_bloomWeight4 = 0.5f;

		public float bloomWeight4 {
			get { return _bloomWeight4; }
			set {
				if (_bloomWeight4 != value) {
					_bloomWeight4 = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_bloomWeight5 = 0.5f;

		public float bloomWeight5 {
			get { return _bloomWeight5; }
			set {
				if (_bloomWeight5 != value) {
					_bloomWeight5 = value;
					UpdateMaterialProperties ();
				}
			}
		}

		#endregion

		#region Anamorphic Flares


		[SerializeField]
		bool
			_anamorphicFlares = false;

		public bool anamorphicFlares {
			get { return _anamorphicFlares; }
			set {
				if (_anamorphicFlares != value) {
					_anamorphicFlares = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_anamorphicFlaresIntensity = 1f;

		public float anamorphicFlaresIntensity {
			get { return _anamorphicFlaresIntensity; }
			set {
				if (_anamorphicFlaresIntensity != value) {
					_anamorphicFlaresIntensity = Mathf.Abs (value);
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		bool
			_anamorphicFlaresAntiflicker = false;

		public bool anamorphicFlaresAntiflicker {
			get { return _anamorphicFlaresAntiflicker; }
			set {
				if (_anamorphicFlaresAntiflicker != value) {
					_anamorphicFlaresAntiflicker = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_anamorphicFlaresThreshold = 0.75f;

		public float anamorphicFlaresThreshold {
			get { return _anamorphicFlaresThreshold; }
			set {
				if (_anamorphicFlaresThreshold != value) {
					_anamorphicFlaresThreshold = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_anamorphicFlaresSpread = 1f;

		public float anamorphicFlaresSpread {
			get { return _anamorphicFlaresSpread; }
			set {
				if (_anamorphicFlaresSpread != value) {
					_anamorphicFlaresSpread = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		bool
			_anamorphicFlaresVertical = false;

		public bool anamorphicFlaresVertical {
			get { return _anamorphicFlaresVertical; }
			set {
				if (_anamorphicFlaresVertical != value) {
					_anamorphicFlaresVertical = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		Color
			_anamorphicFlaresTint = new Color (0.5f, 0.5f, 1f, 0f);

		public Color anamorphicFlaresTint {
			get { return _anamorphicFlaresTint; }
			set {
				if (_anamorphicFlaresTint != value) {
					_anamorphicFlaresTint = value;
					UpdateMaterialProperties ();
				}
			}
		}

		#endregion

		#region Depth of Field


		[SerializeField]
		bool
			_depthOfField = false;
		
		public bool depthOfField {
			get { return _depthOfField; }
			set {
				if (_depthOfField != value) {
					_depthOfField = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		bool
			_depthOfFieldTransparencySupport = false;
		
		public bool depthOfFieldTransparencySupport {
			get { return _depthOfFieldTransparencySupport; }
			set {
				if (_depthOfFieldTransparencySupport != value) {
					_depthOfFieldTransparencySupport = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		Transform 
			_depthOfFieldTargetFocus;
		
		public Transform depthOfFieldTargetFocus {
			get { return _depthOfFieldTargetFocus; }
			set {
				if (_depthOfFieldTargetFocus != value) {
					_depthOfFieldTargetFocus = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		bool
			_depthOfFieldDebug = false;
		
		public bool depthOfFieldDebug {
			get { return _depthOfFieldDebug; }
			set {
				if (_depthOfFieldDebug != value) {
					_depthOfFieldDebug = value;
					UpdateMaterialProperties ();
				}
			}
		}
		
		[SerializeField]
		bool
			_depthOfFieldAutofocus = false;

		public bool depthOfFieldAutofocus {
			get { return _depthOfFieldAutofocus; }
			set {
				if (_depthOfFieldAutofocus != value) {
					_depthOfFieldAutofocus = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		LayerMask
			_depthOfFieldAutofocusLayerMask = -1;
		
		public LayerMask depthOfFieldAutofocusLayerMask {
			get { return _depthOfFieldAutofocusLayerMask; }
			set {
				if (_depthOfFieldAutofocusLayerMask != value) {
					_depthOfFieldAutofocusLayerMask = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_depthOfFieldDistance = 1f;

		public float depthOfFieldDistance {
			get { return _depthOfFieldDistance; }
			set {
				if (_depthOfFieldDistance != value) {
					_depthOfFieldDistance = Mathf.Max (value, 1f);
					UpdateMaterialProperties ();
				}
			}
		}
		
		[SerializeField]
		float
			_depthOfFieldFocusSpeed = 1f;
		
		public float depthOfFieldFocusSpeed {
			get { return _depthOfFieldFocusSpeed; }
			set {
				if (_depthOfFieldFocusSpeed != value) {
					_depthOfFieldFocusSpeed = Mathf.Clamp (value, 0.001f, 1f);
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		int
			_depthOfFieldDownsampling = 2;
		
		public int depthOfFieldDownsampling {
			get { return _depthOfFieldDownsampling; }
			set {
				if (_depthOfFieldDownsampling != value) {
					_depthOfFieldDownsampling = Mathf.Max (value, 1);
					UpdateMaterialProperties ();
				}
			}
		}
		
		[SerializeField]
		int
			_depthOfFieldMaxSamples = 4;
		
		public int depthOfFieldMaxSamples {
			get { return _depthOfFieldMaxSamples; }
			set {
				if (_depthOfFieldMaxSamples != value) {
					_depthOfFieldMaxSamples = Mathf.Max (value, 2);
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_depthOfFieldFocalLength = 0.050f;
		
		public float depthOfFieldFocalLength {
			get { return _depthOfFieldFocalLength; }
			set {
				if (_depthOfFieldFocalLength != value) {
					_depthOfFieldFocalLength = Mathf.Abs (value);
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_depthOfFieldAperture = 2.8f;
		
		public float depthOfFieldAperture {
			get { return _depthOfFieldAperture; }
			set {
				if (_depthOfFieldAperture != value) {
					_depthOfFieldAperture = Mathf.Abs (value);
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		bool
			_depthOfFieldForegroundBlur = true;
		
		public bool depthOfFieldForegroundBlur {
			get { return _depthOfFieldForegroundBlur; }
			set {
				if (_depthOfFieldForegroundBlur != value) {
					_depthOfFieldForegroundBlur = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_depthOfFieldForegroundDistance = 0.25f;
		
		public float depthOfFieldForegroundDistance {
			get { return _depthOfFieldForegroundDistance; }
			set {
				if (_depthOfFieldForegroundDistance != value) {
					_depthOfFieldForegroundDistance = Mathf.Abs (value);
					UpdateMaterialProperties ();
				}
			}
		}

		
		[SerializeField]
		bool
			_depthOfFieldBokeh = true;
		
		public bool depthOfFieldBokeh {
			get { return _depthOfFieldBokeh; }
			set {
				if (_depthOfFieldBokeh != value) {
					_depthOfFieldBokeh = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_depthOfFieldBokehThreshold = 1f;
		
		public float depthOfFieldBokehThreshold {
			get { return _depthOfFieldBokehThreshold; }
			set {
				if (_depthOfFieldBokehThreshold != value) {
					_depthOfFieldBokehThreshold = Mathf.Max (value, 0f);
					UpdateMaterialProperties ();
				}
			}
		}
		
		[SerializeField]
		float
			_depthOfFieldBokehIntensity = 2f;
		
		public float depthOfFieldBokehIntensity {
			get { return _depthOfFieldBokehIntensity; }
			set {
				if (_depthOfFieldBokehIntensity != value) {
					_depthOfFieldBokehIntensity = Mathf.Max (value, 0);
					UpdateMaterialProperties ();
				}
			}
		}

		#endregion

		#region Eye Adaptation

		
		[SerializeField]
		bool
			_eyeAdaptation = false;
		
		public bool eyeAdaptation {
			get { return _eyeAdaptation; }
			set {
				if (_eyeAdaptation != value) {
					_eyeAdaptation = value;
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_eyeAdaptationMinExposure = 0.2f;
		
		public float eyeAdaptationMinExposure {
			get { return _eyeAdaptationMinExposure; }
			set {
				if (_eyeAdaptationMinExposure != value) {
					_eyeAdaptationMinExposure = Mathf.Clamp01 (value);
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_eyeAdaptationMaxExposure = 2f;
		
		public float eyeAdaptationMaxExposure {
			get { return _eyeAdaptationMaxExposure; }
			set {
				if (_eyeAdaptationMaxExposure != value) {
					_eyeAdaptationMaxExposure = Mathf.Clamp (value, 1f, 5f);
					UpdateMaterialProperties ();
				}
			}
		}

		[SerializeField]
		float
			_eyeAdaptationSpeedToLight = 1f;
		
		public float eyeAdaptationSpeedToLight {
			get { return _eyeAdaptationSpeedToLight; }
			set {
				if (_eyeAdaptationSpeedToLight != value) {
					_eyeAdaptationSpeedToLight = Mathf.Clamp01(value);
					UpdateMaterialProperties ();
				}
			}
		}
		
		[SerializeField]
		float
			_eyeAdaptationSpeedToDark = 0.7f;
		
		public float eyeAdaptationSpeedToDark {
			get { return _eyeAdaptationSpeedToDark; }
			set {
				if (_eyeAdaptationSpeedToDark != value) {
					_eyeAdaptationSpeedToDark = Mathf.Clamp01(value);
					UpdateMaterialProperties ();
				}
			}
		}

		#endregion

		#region Purkinje effect

		[SerializeField]
		bool
			_purkinje = false;
		
		public bool purkinje {
			get { return _purkinje; }
			set {
				if (_purkinje != value) {
					_purkinje = value;
					UpdateMaterialProperties ();
				}
			}
		}


		[SerializeField]
		float
			_purkinjeAmount = 1f;
		
		public float purkinjeAmount {
			get { return _purkinjeAmount; }
			set {
				if (_purkinjeAmount != value) {
					_purkinjeAmount = Mathf.Clamp(value, 0f, 5f);
					UpdateMaterialProperties ();
				}
			}
		}
		
		[SerializeField]
		float
			_purkinjeLuminanceThreshold = 0.15f;
		
		public float purkinjeLuminanceThreshold {
			get { return _purkinjeLuminanceThreshold; }
			set {
				if (purkinjeLuminanceThreshold != value) {
					_purkinjeLuminanceThreshold = Mathf.Clamp(value, 0f, 1f);
					UpdateMaterialProperties ();
				}
			}
		}

		#endregion

		#region Tonemapping

		
		[SerializeField]
		BEAUTIFY_TMO
			_tonemap = BEAUTIFY_TMO.Linear;
		
		public BEAUTIFY_TMO tonemap {
			get { return _tonemap; }
			set {
				if (_tonemap != value) {
					_tonemap = value;
					if (_tonemap == BEAUTIFY_TMO.ACES) {
						_saturate = 0;
						_contrast = 1f;
					}
					UpdateMaterialProperties ();
				}
			}
		}

		#endregion


		public static Beautify instance { 
			get { 
				if (_beautify == null) {
					foreach (Camera camera in Camera.allCameras) {
						_beautify = camera.GetComponent<Beautify> ();
						if (_beautify != null)
							break;
					}
				}
				return _beautify;
			} 
		}

		public Camera cameraEffect { get { return currentCamera; } }

		// Internal stuff **************************************************************************************************************
		
		public bool isDirty;
		static Beautify _beautify;
		
		// Shader keywords
		public static string SKW_BLOOM = "BEAUTIFY_BLOOM";
		public static string SKW_SEPIA = "BEAUTIFY_SEPIA";
		public static string SKW_NIGHT_VISION = "BEAUTIFY_NIGHT_VISION";
		public static string SKW_THERMAL_VISION = "BEAUTIFY_THERMAL_VISION";
		public static string SKW_OUTLINE = "BEAUTIFY_OUTLINE";
		public static string SKW_FRAME = "BEAUTIFY_FRAME";
		public static string SKW_TINT = "BEAUTIFY_TINT";
		public static string SKW_DALTONIZE = "BEAUTIFY_DALTONIZE";
		public static string SKW_DIRT = "BEAUTIFY_DIRT";
		public static string SKW_VIGNETTING = "BEAUTIFY_VIGNETTING";
		public static string SKW_DEPTH_OF_FIELD = "BEAUTIFY_DEPTH_OF_FIELD";
		public static string SKW_DEPTH_OF_FIELD_TRANSPARENT = "BEAUTIFY_DEPTH_OF_FIELD_TRANSPARENT";
		public static string SKW_EYE_ADAPTATION = "BEAUTIFY_EYE_ADAPTATION";
		public static string SKW_TONEMAP_ACES = "BEAUTIFY_TONEMAP_ACES";
		public static string SKW_PURKINJE = "BEAUTIFY_PURKINJE";

		Material bMatDesktop, bMatMobile;

#if UNITY_5_4_OR_NEWER // to support ImageEffectAllowedInScene attribute
		[SerializeField]
#endif
		Material
			bMat;
		Camera currentCamera;
		Vector3 camPrevForward, camPrevPos;
		float currSens;
		int renderPass;
		RenderTextureFormat rtFormat;
		RenderTexture[] rt, rtAF, rtEA;
		RenderTexture rtEAacum, rtEAHist;
		float dofPrevDistance, dofLastAutofocusDistance;
//		float dofLastFocusCheck;
		Vector4 dofLastBokehData;
		Camera sceneCamera, depthCam;
		GameObject depthCamObj;
		List<string> shaderKeywords;

		#region Game loop events

		// Creates a private material used to the effect
		void OnEnable ()
		{
			currentCamera = GetComponent<Camera> ();
			if (currentCamera.depthTextureMode == DepthTextureMode.None) {
				currentCamera.depthTextureMode = DepthTextureMode.Depth;
			}
//			dofLastFocusCheck = -100;
			UpdateMaterialProperties ();
		}

		void OnDestroy ()
		{
			if (depthCamObj != null) {
				DestroyImmediate (depthCamObj);
				depthCamObj = null;
			}
			if (rtEAacum!=null) rtEAacum.Release();
			if (rtEAHist!=null) rtEAHist.Release();
			if (bMatDesktop != null) {
				DestroyImmediate (bMatDesktop);
				bMatDesktop = null;
			}
			if (bMatMobile != null) {
				DestroyImmediate (bMatMobile);
				bMatMobile = null;
			}
			bMat = null;
		}

		void Reset ()
		{
			UpdateMaterialProperties ();
		}

		void LateUpdate ()
		{
			if (bMat == null || !Application.isPlaying || _sharpenMotionSensibility <= 0)
				return;
			
			float angleDiff = Vector3.Angle (camPrevForward, currentCamera.transform.forward) * _sharpenMotionSensibility;
			float posDiff = (currentCamera.transform.position - camPrevPos).sqrMagnitude * 10f * _sharpenMotionSensibility;
			
			float diff = angleDiff + posDiff;
			if (diff > 0.1f) {
				camPrevForward = currentCamera.transform.forward;
				camPrevPos = currentCamera.transform.position;
				if (diff > _sharpenMotionSensibility)
					diff = _sharpenMotionSensibility;
				currSens += diff;
				float min = _sharpen * _sharpenMotionSensibility * 0.75f;
				float max = _sharpen * (1f + _sharpenMotionSensibility) * 0.5f;
				currSens = Mathf.Clamp (currSens, min, max);
			} else {
				if (currSens <= 0.001f)
					return;
				currSens *= 0.75f;
			}
			float tempSharpen = Mathf.Clamp (_sharpen - currSens, 0, _sharpen);
			UpdateSharpenParams (tempSharpen);
		}

		void OnPreRender ()
		{
			
			if (!enabled || !gameObject.activeSelf || !depthOfField || !_depthOfFieldTransparencySupport || cameraEffect==null || bMat==null)
				return;
			
			if (depthCam==null) {
				if (depthCamObj == null) {
					depthCamObj = new GameObject ("DepthCamera");
					depthCamObj.hideFlags = HideFlags.HideAndDontSave;
					depthCam = depthCamObj.AddComponent<Camera> ();
					depthCam.enabled = false;
				} else {
					depthCam = depthCamObj.GetComponent<Camera> ();
					if (depthCam==null) {
						DestroyImmediate(depthCamObj);
						depthCamObj=null;
						return;
					}
				}
			}
			depthCam.CopyFrom (cameraEffect);
			depthCam.depthTextureMode = DepthTextureMode.None;
			RenderTexture depthTexture = RenderTexture.GetTemporary (cameraEffect.pixelWidth / _depthOfFieldDownsampling, cameraEffect.pixelHeight / _depthOfFieldDownsampling, 16, RenderTextureFormat.ARGB32);
			depthCam.backgroundColor = new Color (1, 1, 1, 1);
			depthCam.clearFlags = CameraClearFlags.SolidColor;
			depthCam.targetTexture = depthTexture;
			depthCam.RenderWithShader (Shader.Find ("Beautify/CopyDepth"), "RenderType");
			bMat.SetTexture ("_DepthTexture", depthTexture);
			RenderTexture.ReleaseTemporary(depthTexture);
		}


		[ImageEffectTransformsToLDR]
		void OnRenderImage (RenderTexture source, RenderTexture destination)
		{
			if (bMat == null || !enabled) {
				Graphics.Blit (source, destination);
				return;
			}
				                                         
			if (_depthOfField) {
				if (sceneCamera == null && Camera.current != null && Camera.current.name.Equals ("SceneCamera")) {
					sceneCamera = Camera.current;
				}

				if (Camera.current != sceneCamera) {
					UpdateDepthOfFieldData ();

					int pass = _quality == BEAUTIFY_QUALITY.BestQuality ? 12: 6;
					RenderTexture rtDB = RenderTexture.GetTemporary (source.width / _depthOfFieldDownsampling, source.height / _depthOfFieldDownsampling, 0, rtFormat);
					Graphics.Blit (source, rtDB, bMat, pass);

					if (_quality == BEAUTIFY_QUALITY.BestQuality) {
						pass = _depthOfFieldBokeh ? 14: 19;
					} else {
						pass = _depthOfFieldBokeh ? 8: 15;
					}
					BlurThisDoF (rtDB, pass);

					if (_depthOfFieldDebug) {
						source.MarkRestoreExpected ();
						pass = _quality == BEAUTIFY_QUALITY.BestQuality ? 13: 7;
						Graphics.Blit (rtDB, destination, bMat, pass);
						return;
					}

					bMat.SetTexture ("_DoFTex", rtDB);
					RenderTexture.ReleaseTemporary (rtDB);
				} else {
					// Cancels DoF
					bMat.SetVector ("_BokehData", new Vector4 (10000, 0, 0, 0));
				}
			}

			if (_lensDirt || _bloom || _anamorphicFlares) {

				RenderTexture rtBloom = null, rtCustomBloom = null;

				int PYRAMID_COUNT, size;
				if (_quality == BEAUTIFY_QUALITY.BestPerformance) {
					PYRAMID_COUNT = 4;
					size = 256;
				} else {
					PYRAMID_COUNT = 5;
					size = 512;
				}
				int sizeAF = size;
				float aspectRatio = (float)source.height / source.width;

				// Bloom buffers
				if (rt == null || rt.Length != PYRAMID_COUNT + 1) {
					rt = new RenderTexture[PYRAMID_COUNT + 1]; 
				}
				// Anamorphic flare buffers
				if (rtAF == null || rtAF.Length != PYRAMID_COUNT + 1) {
					rtAF = new RenderTexture[PYRAMID_COUNT + 1]; 
				}

				if (_bloom || (_lensDirt && !_anamorphicFlares)) {
					UpdateMaterialBloomIntensityAndThreshold ();

					for (int k = 0; k <= PYRAMID_COUNT; k++) {
						rt [k] = RenderTexture.GetTemporary (size, Mathf.Max (1, (int)(size * aspectRatio)), 0, rtFormat);
						size /= 2;
					}
					rtBloom = rt [0];

					if (_quality == BEAUTIFY_QUALITY.BestQuality && _bloomAntiflicker) {
						Graphics.Blit (source, rt [0], bMat, 9);
					} else {
						Graphics.Blit (source, rt [0], bMat, 2);
					}

					BlurThis (rt [0]);

					for (int k = 0; k < PYRAMID_COUNT; k++) {
						if (_quality == BEAUTIFY_QUALITY.BestPerformance) {
							Graphics.Blit (rt [k], rt [k + 1]);
						} else {
							Graphics.Blit (rt [k], rt [k + 1], bMat, 7);
							if (k < PYRAMID_COUNT) {
								BlurThis (rt [k + 1]);
							}
						}
					}

					if (_bloom) {
						for (int k = PYRAMID_COUNT; k > 0; k--) {
							rt [k - 1].MarkRestoreExpected ();
							Graphics.Blit (rt [k], rt [k - 1], bMat, _quality == BEAUTIFY_QUALITY.BestQuality ? 8: 13 );
						}
						if (quality == BEAUTIFY_QUALITY.BestQuality && _bloomCustomize) {
							bMat.SetTexture ("_BloomTex4", rt [4]);
							bMat.SetTexture ("_BloomTex3", rt [3]);
							bMat.SetTexture ("_BloomTex2", rt [2]);
							bMat.SetTexture ("_BloomTex1", rt [1]);
							bMat.SetTexture ("_BloomTex", rt [0]);
							rtCustomBloom = RenderTexture.GetTemporary (rt [0].width, rt [0].height, 0, rtFormat);
							rtBloom = rtCustomBloom;
							Graphics.Blit (rt [PYRAMID_COUNT], rtBloom, bMat, 6);
						}
					}
											
				}
												
				// anamorphic flares
				if (_anamorphicFlares) {
					UpdateMaterialAnamorphicIntensityAndThreshold ();

					for (int origSize = sizeAF, k = 0; k <= PYRAMID_COUNT; k++) {
						if (_anamorphicFlaresVertical) {
							rtAF [k] = RenderTexture.GetTemporary (origSize, Mathf.Max (1, (int)(sizeAF * aspectRatio / _anamorphicFlaresSpread)), 0, rtFormat);
						} else {
							rtAF [k] = RenderTexture.GetTemporary (Mathf.Max (1, (int)(sizeAF * aspectRatio / _anamorphicFlaresSpread)), origSize, 0, rtFormat);
						}
						sizeAF /= 2;
					}

					if (_anamorphicFlaresAntiflicker && _quality == BEAUTIFY_QUALITY.BestQuality) {
						Graphics.Blit (source, rtAF [0], bMat, 9);
					} else {
						Graphics.Blit (source, rtAF [0], bMat, 2);
					}

					rtAF [0] = BlurThisOneDirection (rtAF [0], _anamorphicFlaresVertical);

					for (int k = 0; k < PYRAMID_COUNT; k++) {
						if (_quality == BEAUTIFY_QUALITY.BestPerformance) {
							Graphics.Blit (rtAF [k], rtAF [k + 1]);
						} else {
							Graphics.Blit (rtAF [k], rtAF [k + 1], bMat, 7);
						}
						if (k < PYRAMID_COUNT) {
							rtAF [k + 1] = BlurThisOneDirection (rtAF [k + 1], _anamorphicFlaresVertical);
						}
					}

					for (int k = PYRAMID_COUNT; k > 0; k--) {
						rtAF [k - 1].MarkRestoreExpected ();
						if (k == 1) {
							Graphics.Blit (rtAF [k], rtAF [k - 1], bMat, _quality == BEAUTIFY_QUALITY.BestQuality ? 10: 14); // applies intensity in last stage
						} else {
							Graphics.Blit (rtAF [k], rtAF [k - 1], bMat, _quality == BEAUTIFY_QUALITY.BestQuality ? 8: 13);
						}
					}
					if (_bloom) {
						if (_lensDirt) {
							rtBloom.MarkRestoreExpected ();
							Graphics.Blit (rtAF [3], rt[3], bMat, _quality == BEAUTIFY_QUALITY.BestQuality ? 11: 13);
						}
						rtBloom.MarkRestoreExpected ();
						Graphics.Blit (rtAF [0], rtBloom, bMat, _quality == BEAUTIFY_QUALITY.BestQuality ? 11: 13);
					} else {
						rtBloom = rtAF [0];
					}

					UpdateMaterialBloomIntensityAndThreshold ();
				}
				bMat.SetTexture ("_BloomTex", rtBloom);


				if (_lensDirt) {
					bMat.SetTexture ("_ScreenLum", (_anamorphicFlares && !_bloom) ? rtAF[3] : rt [3]);
				}

				for (int k = 0; k <= PYRAMID_COUNT; k++) {
					if (rt [k] != null) {
						RenderTexture.ReleaseTemporary (rt [k]);
						rt [k] = null;
					}
					if (rtAF [k] != null) {
						RenderTexture.ReleaseTemporary (rtAF [k]);
						rtAF [k] = null;
					}
				}

				if (rtCustomBloom != null) {
					RenderTexture.ReleaseTemporary (rtCustomBloom);
				}
												
			}

			if (_lensDirt) {
				Vector4 dirtData = new Vector4 (1.0f, 1.0f / (1.01f - _lensDirtIntensity), _lensDirtThreshold, 0);
				bMat.SetVector ("_Dirt", dirtData);
			}

			// tonemap + eye adaptation + purkinje
			bool requiresLuminanceComputation = Application.isPlaying && (_eyeAdaptation || _purkinje);
			if (requiresLuminanceComputation) {
				int rtEACount = _quality == BEAUTIFY_QUALITY.BestQuality ? 9: 8;
				int sizeEA = (int)Mathf.Pow(2, rtEACount);
				if (rtEA==null || rtEA.Length < rtEACount) rtEA = new RenderTexture[rtEACount];
				for (int k = 0; k < rtEACount; k++) {
					rtEA [k] = RenderTexture.GetTemporary (sizeEA, sizeEA, 0, rtFormat);
					sizeEA /= 2;
				}
				Graphics.Blit (source, rtEA[0]);
				int lumRT = rtEACount-1;
				int basePass = _quality == BEAUTIFY_QUALITY.BestQuality ? 15: 9;
				for (int k = 0; k < lumRT; k++) {
					Graphics.Blit(rtEA[k], rtEA[k+1], bMat, k == 0 ? basePass: basePass+1);
				}	
				bMat.SetTexture("_EALumSrc", rtEA[lumRT]);
				if (rtEAacum==null) {
					rtEAacum = new RenderTexture(2,2,0,rtFormat);
					Graphics.Blit(rtEA[lumRT], rtEAacum);
					rtEAHist = new RenderTexture(2,2,0,rtFormat);
					Graphics.Blit(rtEAacum, rtEAHist);
				} else {
					rtEAacum.MarkRestoreExpected();
					Graphics.Blit(rtEA[lumRT], rtEAacum, bMat, basePass+2);
					Graphics.Blit(rtEAacum, rtEAHist, bMat, basePass+3);
				}
				bMat.SetTexture("_EAHist", rtEAHist);
				bMat.SetTexture("_EALum", rtEAacum);
				for (int k = 0; k < rtEACount;k++) {
					RenderTexture.ReleaseTemporary(rtEA[k]);
				}
			}

			Graphics.Blit (source, destination, bMat, renderPass);


		}

		void BlurThis (RenderTexture rt)
		{
			RenderTexture rt2 = RenderTexture.GetTemporary (rt.width, rt.height, 0, rtFormat);
			rt2.filterMode = FilterMode.Bilinear;
			Graphics.Blit (rt, rt2, bMat, 4);
			rt.DiscardContents ();
			Graphics.Blit (rt2, rt, bMat, 5);
			RenderTexture.ReleaseTemporary (rt2);
		}

		RenderTexture BlurThisOneDirection (RenderTexture rt, bool vertical)
		{
			RenderTexture rt2 = RenderTexture.GetTemporary (rt.width, rt.height, 0, rtFormat);
			rt2.filterMode = FilterMode.Bilinear;
			Graphics.Blit (rt, rt2, bMat, vertical ? 5 : 4);
			RenderTexture.ReleaseTemporary (rt);
			return rt2;
		}

		void BlurThisDoF (RenderTexture rt, int renderPass)
		{
			RenderTexture rt2 = RenderTexture.GetTemporary (rt.width, rt.height, 0, rtFormat);
			RenderTexture rt3 = RenderTexture.GetTemporary (rt.width, rt.height, 0, rtFormat);
			rt2.filterMode = FilterMode.Bilinear;
			rt3.filterMode = FilterMode.Bilinear;
			UpdateDepthOfFieldBlurData (new Vector2 (0.44721f, -0.89443f));
			Graphics.Blit (rt, rt2, bMat, renderPass);
			UpdateDepthOfFieldBlurData (new Vector2 (-1f, 0f));
			Graphics.Blit (rt2, rt3, bMat, renderPass);
			UpdateDepthOfFieldBlurData (new Vector2 (0.44721f, 0.89443f));
			rt.DiscardContents ();
			Graphics.Blit (rt3, rt, bMat, renderPass);
			RenderTexture.ReleaseTemporary (rt3);
			RenderTexture.ReleaseTemporary (rt2);
		}


	#endregion

	#region Settings stuff

		void UpdateQualitySettings ()
		{
			if (_quality == BEAUTIFY_QUALITY.BestPerformance) {
				_depthOfFieldDownsampling = 2;
				_depthOfFieldMaxSamples = 4;
			} else {
				_depthOfFieldDownsampling = 1;
				_depthOfFieldMaxSamples = 8;
			}
		}

		void UpdateMaterialProperties ()
		{

			rtFormat = SystemInfo.SupportsRenderTextureFormat (RenderTextureFormat.ARGBHalf) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
			if (_quality == BEAUTIFY_QUALITY.BestQuality) {
				if (bMatDesktop == null) {
					bMatDesktop = Instantiate (Resources.Load<Material> ("Materials/Beautify"));
					bMatDesktop.hideFlags = HideFlags.DontSave;
				}
				bMat = bMatDesktop;
			} else {
				if (bMatMobile == null) {
					bMatMobile = Instantiate (Resources.Load<Material> ("Materials/BeautifyMobile"));
					bMatMobile.hideFlags = HideFlags.DontSave;
				}
				bMat = bMatMobile;
			}

			bool linearColorSpace = (QualitySettings.activeColorSpace == ColorSpace.Linear);

			switch (_preset) {
			case BEAUTIFY_PRESET.Soft:
				_sharpen = 2.0f;
				if (linearColorSpace)
					_sharpen *= 2f;
				_sharpenDepthThreshold = 0.035f;
				_sharpenRelaxation = 0.065f;
				_sharpenClamp = 0.4f;
				_saturate = 0.5f;
				_contrast = 1.005f;
				_brightness = 1.05f;
				_dither = 0.02f;
				_ditherDepth = 0;
				_daltonize = 0;
				break;
			case BEAUTIFY_PRESET.Medium:
				_sharpen = 3f;
				if (linearColorSpace)
					_sharpen *= 2f;
				_sharpenDepthThreshold = 0.035f;
				_sharpenRelaxation = 0.07f;
				_sharpenClamp = 0.45f;
				_saturate = 1.0f;
				_contrast = 1.02f;
				_brightness = 1.05f;
				_dither = 0.02f;
				_ditherDepth = 0;
				_daltonize = 0;
				break;
			case BEAUTIFY_PRESET.Strong:
				_sharpen = 4.75f;
				if (linearColorSpace)
					_sharpen *= 2f;
				_sharpenDepthThreshold = 0.035f;
				_sharpenRelaxation = 0.075f;
				_sharpenClamp = 0.5f;
				_saturate = 1.5f;
				_contrast = 1.03f;
				_brightness = 1.05f;
				_dither = 0.022f;
				_ditherDepth = 0;
				_daltonize = 0;
				break;
			case BEAUTIFY_PRESET.Exaggerated:
				_sharpen = 6f;
				if (linearColorSpace)
					_sharpen *= 2f;
				_sharpenDepthThreshold = 0.035f;
				_sharpenRelaxation = 0.08f;
				_sharpenClamp = 0.55f;
				_saturate = 2.25f;
				_contrast = 1.035f;
				_brightness = 1.05f;
				_dither = 0.025f;
				_ditherDepth = 0;
				_daltonize = 0;
				break;
			}
			isDirty = true;

			if (bMat == null)
				return;
			renderPass = 1;

			// sharpen settings
			UpdateSharpenParams (_sharpen);

			// dither settings
			bool isOrtho = (currentCamera != null && currentCamera.orthographic);
			bMat.SetVector ("_Dither", new Vector4 (_dither, isOrtho ? 0 : _ditherDepth, (_sharpenMaxDepth + _sharpenMinDepth) * 0.5f, Mathf.Abs (_sharpenMaxDepth - _sharpenMinDepth) * 0.5f + (isOrtho ? 1000.0f : 0f)));
			float cont = linearColorSpace ? 1.0f + (_contrast - 1.0f) / 2.2f : _contrast;

			// color grading settings
			bMat.SetVector ("_ColorBoost", new Vector4 (_brightness, cont, _saturate, _daltonize * 10f));

			// vignetting FX
			Color vignettingColorAdjusted = _vignettingColor;
			vignettingColorAdjusted.a *= _vignetting ? 32f : 0f;
			bMat.SetColor ("_Vignetting", vignettingColorAdjusted);
			if (currentCamera != null) {
				bMat.SetFloat ("_VignettingAspectRatio", _vignettingCircularShape ? 1.0f / currentCamera.aspect : 1.0f);
			}

			// frame FX
			Vector4 frameColorAdjusted = new Vector4 (_frameColor.r, _frameColor.g, _frameColor.b, (1.00001f - _frameColor.a) * 0.5f);
			if (!_frame)
				frameColorAdjusted.w = 0;
			bMat.SetVector ("_Frame", frameColorAdjusted);

			// outline FX
			bMat.SetColor ("_Outline", _outlineColor);

			// bloom
			UpdateMaterialBloomIntensityAndThreshold ();
			float bloomWeightsSum = 0.000001f + _bloomWeight0 + _bloomWeight1 + _bloomWeight2 + _bloomWeight3 + _bloomWeight4 + _bloomWeight5;
			bMat.SetVector ("_BloomWeights", new Vector4 (_bloomWeight0, _bloomWeight1, _bloomWeight2, _bloomWeight3));
			bMat.SetVector ("_BloomWeights2", new Vector4 (_bloomWeight4, _bloomWeight5, 0, bloomWeightsSum));
			if (_bloomDebug && (_bloom || _anamorphicFlares))
				renderPass = 3;

			// lens dirt
			if (_lensDirtTexture == null) {
				_lensDirtTexture = Resources.Load<Texture2D> ("Textures/dirt2");
			}
			bMat.SetTexture ("_OverlayTex", _lensDirtTexture);

			// anamorphic flares
			bMat.SetColor ("_AFTint", _anamorphicFlaresTint);

			// final config
			if (_compareMode) {
				renderPass = 0;
				bMat.SetVector ("_CompareParams", new Vector4 (Mathf.Cos (_compareLineAngle), Mathf.Sin (_compareLineAngle), -Mathf.Cos (_compareLineAngle), _compareLineWidth));
			}
			if (shaderKeywords == null)
				shaderKeywords = new List<string> ();
			else
				shaderKeywords.Clear ();
			if (_sepia) {
				shaderKeywords.Add (SKW_SEPIA);
				bMat.SetColor ("_FXColor", new Color (0, 0, 0, _sepiaIntensity));
			} else if (_nightVision) {
				shaderKeywords.Add (SKW_NIGHT_VISION);
				Color nightVisionAdjusted = _nightVisionColor;
				if (linearColorSpace) {
					nightVisionAdjusted.a *= 5.0f * nightVisionAdjusted.a;
				} else {
					nightVisionAdjusted.a *= 3.0f * nightVisionAdjusted.a;
				}
				nightVisionAdjusted.r = nightVisionAdjusted.r * nightVisionAdjusted.a;
				nightVisionAdjusted.g = nightVisionAdjusted.g * nightVisionAdjusted.a;
				nightVisionAdjusted.b = nightVisionAdjusted.b * nightVisionAdjusted.a;
				bMat.SetColor ("_FXColor", nightVisionAdjusted);
			} else if (_thermalVision) {
				shaderKeywords.Add (SKW_THERMAL_VISION);
			} else if (_tintColor.a > 0) {
				shaderKeywords.Add (SKW_TINT);
				bMat.SetColor ("_FXColor", _tintColor);
			} else if (_daltonize > 0) {
				shaderKeywords.Add (SKW_DALTONIZE);
			}
			if (_vignetting)
				shaderKeywords.Add (SKW_VIGNETTING);
			if (_outline)
				shaderKeywords.Add (SKW_OUTLINE);
			if (_lensDirt)
				shaderKeywords.Add (SKW_DIRT);
			if (_bloom || _anamorphicFlares)
				shaderKeywords.Add (SKW_BLOOM);
			if (_depthOfField) {
				if (_depthOfFieldTransparencySupport) {
					shaderKeywords.Add (SKW_DEPTH_OF_FIELD_TRANSPARENT);
				} else {
					shaderKeywords.Add (SKW_DEPTH_OF_FIELD);
				}
			}
			if (_eyeAdaptation) {
				Vector4 eaData = new Vector4(_eyeAdaptationMinExposure, _eyeAdaptationMaxExposure, _eyeAdaptationSpeedToDark, _eyeAdaptationSpeedToLight);
				bMat.SetVector("_EyeAdaptation", eaData);
				shaderKeywords.Add (SKW_EYE_ADAPTATION);
			}
			if (_quality == BEAUTIFY_QUALITY.BestQuality) {
				if (_tonemap == BEAUTIFY_TMO.ACES) 
					shaderKeywords.Add (SKW_TONEMAP_ACES);
			}
			if (_purkinje) {
				Vector3 purkinjeData = new Vector3(_purkinjeAmount, _purkinjeLuminanceThreshold, 0);
				bMat.SetVector("_Purkinje", purkinjeData);
				shaderKeywords.Add (SKW_PURKINJE);
			}

			bMat.shaderKeywords = shaderKeywords.ToArray ();

			#if DEBUG_BEAUTIFY
												Debug.Log("*** DEBUG: Updating material properties...");
												Debug.Log("Linear color space: " + linearColorSpace.ToString());
												Debug.Log("Preset: " + _preset.ToString());
												Debug.Log("Sharpen: " + _sharpen.ToString());
												Debug.Log("Dither: " + _dither.ToString());
												Debug.Log("Contrast: " + cont.ToString());
												Debug.Log("Bloom: " + _bloom.ToString());
												Debug.Log("Bloom Intensity: " + _bloomIntensity.ToString());
												Debug.Log("Bloom Threshold: " + bloomThreshold.ToString());
												Debug.Log("Bloom Weight: " + bloomWeightsSum.ToString());
			#endif
		}

		void UpdateMaterialBloomIntensityAndThreshold ()
		{
			float threshold = _bloomThreshold;
			if (QualitySettings.activeColorSpace == ColorSpace.Linear) {
				threshold *= threshold;
			}
			bMat.SetVector ("_Bloom", new Vector4 (_bloomIntensity + (_anamorphicFlares ? 0.0001f : 0f), 0, 0, threshold));
		}

		void UpdateMaterialAnamorphicIntensityAndThreshold ()
		{
			float threshold = _anamorphicFlaresThreshold;
			if (QualitySettings.activeColorSpace == ColorSpace.Linear) {
				threshold *= threshold;
			}
			float intensity = _anamorphicFlaresIntensity / (_bloomIntensity + 0.0001f);
			bMat.SetVector ("_Bloom", new Vector4 (intensity, 0, 0, threshold));
		}

		void UpdateSharpenParams (float sharpen)
		{
			bMat.SetVector ("_Sharpen", new Vector4 (sharpen, _sharpenDepthThreshold, _sharpenClamp, _sharpenRelaxation));
		}

		public void SetBloomWeights (float w0, float w1, float w2, float w3, float w4, float w5)
		{
			_bloomWeight0 = w0;
			_bloomWeight1 = w1;
			_bloomWeight2 = w2;
			_bloomWeight3 = w3;
			_bloomWeight4 = w4;
			_bloomWeight5 = w5;
			UpdateMaterialProperties ();
		}

		void UpdateDepthOfFieldData () {
			// TODO: get focal length from camera FOV: FOV = 2 arctan (x/2f) x = diagonal of film (0.024mm)
			float d;
			if (_depthOfFieldAutofocus) {
//				if (!Application.isPlaying || Time.time - dofLastFocusCheck > 0.5f) {
//					dofLastFocusCheck = Time.time;
					UpdateDoFAutofocusDistance ();
//				}
				d = dofLastAutofocusDistance > 0 ? dofLastAutofocusDistance : currentCamera.farClipPlane;
			} else if (_depthOfFieldTargetFocus != null) {
//				if (!Application.isPlaying || Time.time - dofLastFocusCheck > 0.01f) {
//					dofLastFocusCheck = Time.time;
					Vector3 spos = currentCamera.WorldToScreenPoint (_depthOfFieldTargetFocus.position);
					if (spos.z < 0) {
						d = currentCamera.farClipPlane;
					} else {
						d = Vector3.Distance (currentCamera.transform.position, _depthOfFieldTargetFocus.position);
					}
//				} else {
//					d = dofPrevDistance > 0 ? dofPrevDistance : currentCamera.farClipPlane;
//				}
			} else {
				d = _depthOfFieldDistance;
			}
			dofPrevDistance = Mathf.Lerp (dofPrevDistance, d, _depthOfFieldFocusSpeed);
			float dofCoc = _depthOfFieldAperture * (_depthOfFieldFocalLength / Mathf.Max (dofPrevDistance - _depthOfFieldFocalLength, 0.001f)) * (1f / 0.024f);
			dofLastBokehData = new Vector4 (dofPrevDistance, dofCoc, 0, 0);
			bMat.SetVector ("_BokehData", dofLastBokehData);
			bMat.SetVector ("_BokehData2", new Vector4 (_depthOfFieldForegroundBlur ? _depthOfFieldForegroundDistance : currentCamera.farClipPlane, _depthOfFieldMaxSamples, _depthOfFieldBokehThreshold, _depthOfFieldBokehIntensity*_depthOfFieldBokehIntensity));
		}

		void UpdateDepthOfFieldBlurData (Vector2 blurDir)
		{
			float downsamplingRatio = 1f / (float)_depthOfFieldDownsampling;
			blurDir *= downsamplingRatio;
			dofLastBokehData.z = blurDir.x;
			dofLastBokehData.w = blurDir.y;
			bMat.SetVector ("_BokehData", dofLastBokehData);
		}

		void UpdateDoFAutofocusDistance ()
		{
			Ray r = new Ray (currentCamera.transform.position, currentCamera.transform.forward);
			RaycastHit hit;
			if (Physics.Raycast (r, out hit, currentCamera.farClipPlane, _depthOfFieldAutofocusLayerMask)) {
				dofLastAutofocusDistance = hit.distance;
			} else {
				dofLastAutofocusDistance = currentCamera.farClipPlane;
			}
		}

								#endregion
	
	}

}