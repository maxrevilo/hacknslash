************************************
*             BEAUTIFY             *
*    (C) Copyright 2016 Kronnect   * 
*            README FILE           *
************************************


How to use this asset
---------------------
We recommend importing the asset in an empty project and run the Demo Scenes provided to get an idea of the overall functionality.
Read the documentation and experiment with the tool.

Later, you can import the asset into your project excluding the demo folder.

Important: in Unity 5.4, the Game View has a scale slider. The demo scenes print some info texts on top of screen. If you don't see the texts, choose "Free Aspect" and/or reduce scale to show entire screen.

Hint: to use the asset, select your camera and add "Beautify" script to it.



Documentation/API reference
---------------------------
The PDF is located in the Documentation folder. It contains additional instructions and description about the asset, as well as some recommendations.



Support
-------
Please read the documentation PDF and browse/play with the demo scene and sample source code included before contacting us for support :-)

* Support: contact@kronnect.me
* Website-Forum: http://kronnect.me
* Twitter: @KronnectGames



Future updates
--------------

All our assets follow an incremental development process by which a few beta releases are published on our support forum (kronnect.com).
We encourage you to signup and engage our forum. The forum is the primary support and feature discussions medium.

Of course, all updates of Beautify will be eventually available on the Asset Store.



Version history
---------------

Version 4.1.2
- Fixed Single Pass Stereo Rendering on some configurations

Version 4.1.1
- Added support for Unity 5.5
- Fixed lens dirt effect not working correctly when bloom and anamorphic flares is enabled

Version 4.1
- Added transparency support to Depth of Field effect
- Improved bokeh effect with option to enable/disable it

Version 4.0
- Added ACES tonemap operator
- Added eye adaptation effect
- Added purkinje effect (achromatic vision in the dark + spectrum shift)
- New build options to optimize compilation time and build size
- Better bloom & anamorphic flares when using best performance setting
- Added layer mask field to depth of field autofocus option
- Fixed anamorphic flares vertical spread using incorrect aspect ratio

Version 3.2.1
- Fixed depth of field goint to full blur strength when looking aside an assigned target focus
- Fixed depth of field shader unroll issue

Version 3.2 Current version
- Added autofocus option to depth of field
- Fixed depth of field affecting scene camera

Version 3.1 2016-OCT-14
- Added depth of field
- Fixed daltonize filter issue with pure black pixels

Version 3.0.0 2016-OCT-5
- Added anamorphic flares!
- Added sepia intensity slider
- Improved bloom performance

Version 2.4.1 2016-SEP-6
- Demo folder resources renamed to DemoSources to prevent accidental inclusion in builds
- Fixed issue when changing Beautify properties using scripting from Awake event

Version 2.4 2016-SEP-1
- Improved support for 2D / orthographic camera

Version 2.3 2016-AUG-28
- Added vignetting circular shape option
- Improved bloom effect in gamma color space and mobile
- Added 3 new lens dirt textures

Version 2.2.2 2016-AUG-24
- Fixed compare mode with DX/Antialias enabled
- Throttled sharpen presets in linear color space

Version 2.2.1 2016-AUG-17
- Effect in Scene View now updates correctly in Unity 5.4

Version 2.2 Current version
- VR: Experimental Single Pass Stereo Rendering support for VR (Unity 5.4)
- Effect now shows in scene view in Unity 5.4
- New Compare Mode options

Version 2.1 2016-AGO-01
- Bloom antiflicker filter

Version 2.0 2016-JUL-02
- Redesigned inspector
- New extra effects

Version 1.0 2016-MAY-31
- Initial Release






