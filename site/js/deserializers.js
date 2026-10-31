var Deserializers = {}
Deserializers["UnityEngine.JointSpring"] = function (request, data, root) {
  var i358 = root || request.c( 'UnityEngine.JointSpring' )
  var i359 = data
  i358.spring = i359[0]
  i358.damper = i359[1]
  i358.targetPosition = i359[2]
  return i358
}

Deserializers["UnityEngine.JointMotor"] = function (request, data, root) {
  var i360 = root || request.c( 'UnityEngine.JointMotor' )
  var i361 = data
  i360.m_TargetVelocity = i361[0]
  i360.m_Force = i361[1]
  i360.m_FreeSpin = i361[2]
  return i360
}

Deserializers["UnityEngine.JointLimits"] = function (request, data, root) {
  var i362 = root || request.c( 'UnityEngine.JointLimits' )
  var i363 = data
  i362.m_Min = i363[0]
  i362.m_Max = i363[1]
  i362.m_Bounciness = i363[2]
  i362.m_BounceMinVelocity = i363[3]
  i362.m_ContactDistance = i363[4]
  i362.minBounce = i363[5]
  i362.maxBounce = i363[6]
  return i362
}

Deserializers["UnityEngine.JointDrive"] = function (request, data, root) {
  var i364 = root || request.c( 'UnityEngine.JointDrive' )
  var i365 = data
  i364.m_PositionSpring = i365[0]
  i364.m_PositionDamper = i365[1]
  i364.m_MaximumForce = i365[2]
  i364.m_UseAcceleration = i365[3]
  return i364
}

Deserializers["UnityEngine.SoftJointLimitSpring"] = function (request, data, root) {
  var i366 = root || request.c( 'UnityEngine.SoftJointLimitSpring' )
  var i367 = data
  i366.m_Spring = i367[0]
  i366.m_Damper = i367[1]
  return i366
}

Deserializers["UnityEngine.SoftJointLimit"] = function (request, data, root) {
  var i368 = root || request.c( 'UnityEngine.SoftJointLimit' )
  var i369 = data
  i368.m_Limit = i369[0]
  i368.m_Bounciness = i369[1]
  i368.m_ContactDistance = i369[2]
  return i368
}

Deserializers["UnityEngine.WheelFrictionCurve"] = function (request, data, root) {
  var i370 = root || request.c( 'UnityEngine.WheelFrictionCurve' )
  var i371 = data
  i370.m_ExtremumSlip = i371[0]
  i370.m_ExtremumValue = i371[1]
  i370.m_AsymptoteSlip = i371[2]
  i370.m_AsymptoteValue = i371[3]
  i370.m_Stiffness = i371[4]
  return i370
}

Deserializers["UnityEngine.JointAngleLimits2D"] = function (request, data, root) {
  var i372 = root || request.c( 'UnityEngine.JointAngleLimits2D' )
  var i373 = data
  i372.m_LowerAngle = i373[0]
  i372.m_UpperAngle = i373[1]
  return i372
}

Deserializers["UnityEngine.JointMotor2D"] = function (request, data, root) {
  var i374 = root || request.c( 'UnityEngine.JointMotor2D' )
  var i375 = data
  i374.m_MotorSpeed = i375[0]
  i374.m_MaximumMotorTorque = i375[1]
  return i374
}

Deserializers["UnityEngine.JointSuspension2D"] = function (request, data, root) {
  var i376 = root || request.c( 'UnityEngine.JointSuspension2D' )
  var i377 = data
  i376.m_DampingRatio = i377[0]
  i376.m_Frequency = i377[1]
  i376.m_Angle = i377[2]
  return i376
}

Deserializers["UnityEngine.JointTranslationLimits2D"] = function (request, data, root) {
  var i378 = root || request.c( 'UnityEngine.JointTranslationLimits2D' )
  var i379 = data
  i378.m_LowerTranslation = i379[0]
  i378.m_UpperTranslation = i379[1]
  return i378
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Material"] = function (request, data, root) {
  var i380 = root || new pc.UnityMaterial()
  var i381 = data
  i380.name = i381[0]
  request.r(i381[1], i381[2], 0, i380, 'shader')
  i380.renderQueue = i381[3]
  i380.enableInstancing = !!i381[4]
  var i383 = i381[5]
  var i382 = []
  for(var i = 0; i < i383.length; i += 1) {
    i382.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Material+FloatParameter', i383[i + 0]) );
  }
  i380.floatParameters = i382
  var i385 = i381[6]
  var i384 = []
  for(var i = 0; i < i385.length; i += 1) {
    i384.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Material+ColorParameter', i385[i + 0]) );
  }
  i380.colorParameters = i384
  var i387 = i381[7]
  var i386 = []
  for(var i = 0; i < i387.length; i += 1) {
    i386.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Material+VectorParameter', i387[i + 0]) );
  }
  i380.vectorParameters = i386
  var i389 = i381[8]
  var i388 = []
  for(var i = 0; i < i389.length; i += 1) {
    i388.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Material+TextureParameter', i389[i + 0]) );
  }
  i380.textureParameters = i388
  var i391 = i381[9]
  var i390 = []
  for(var i = 0; i < i391.length; i += 1) {
    i390.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Material+MaterialFlag', i391[i + 0]) );
  }
  i380.materialFlags = i390
  return i380
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Material+FloatParameter"] = function (request, data, root) {
  var i394 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Material+FloatParameter' )
  var i395 = data
  i394.name = i395[0]
  i394.value = i395[1]
  return i394
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Material+ColorParameter"] = function (request, data, root) {
  var i398 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Material+ColorParameter' )
  var i399 = data
  i398.name = i399[0]
  i398.value = new pc.Color(i399[1], i399[2], i399[3], i399[4])
  return i398
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Material+VectorParameter"] = function (request, data, root) {
  var i402 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Material+VectorParameter' )
  var i403 = data
  i402.name = i403[0]
  i402.value = new pc.Vec4( i403[1], i403[2], i403[3], i403[4] )
  return i402
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Material+TextureParameter"] = function (request, data, root) {
  var i406 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Material+TextureParameter' )
  var i407 = data
  i406.name = i407[0]
  request.r(i407[1], i407[2], 0, i406, 'value')
  return i406
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Material+MaterialFlag"] = function (request, data, root) {
  var i410 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Material+MaterialFlag' )
  var i411 = data
  i410.name = i411[0]
  i410.enabled = !!i411[1]
  return i410
}

Deserializers["Luna.Unity.DTO.UnityEngine.Textures.Texture2D"] = function (request, data, root) {
  var i412 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Textures.Texture2D' )
  var i413 = data
  i412.name = i413[0]
  i412.width = i413[1]
  i412.height = i413[2]
  i412.mipmapCount = i413[3]
  i412.anisoLevel = i413[4]
  i412.filterMode = i413[5]
  i412.hdr = !!i413[6]
  i412.format = i413[7]
  i412.wrapMode = i413[8]
  i412.alphaIsTransparency = !!i413[9]
  i412.alphaSource = i413[10]
  i412.graphicsFormat = i413[11]
  i412.sRGBTexture = !!i413[12]
  i412.desiredColorSpace = i413[13]
  i412.wrapU = i413[14]
  i412.wrapV = i413[15]
  return i412
}

Deserializers["Luna.Unity.DTO.UnityEngine.Scene.Scene"] = function (request, data, root) {
  var i414 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Scene.Scene' )
  var i415 = data
  i414.name = i415[0]
  i414.index = i415[1]
  i414.startup = !!i415[2]
  return i414
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.Camera"] = function (request, data, root) {
  var i416 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.Camera' )
  var i417 = data
  i416.aspect = i417[0]
  i416.orthographic = !!i417[1]
  i416.orthographicSize = i417[2]
  i416.backgroundColor = new pc.Color(i417[3], i417[4], i417[5], i417[6])
  i416.nearClipPlane = i417[7]
  i416.farClipPlane = i417[8]
  i416.fieldOfView = i417[9]
  i416.depth = i417[10]
  i416.clearFlags = i417[11]
  i416.cullingMask = i417[12]
  i416.rect = i417[13]
  request.r(i417[14], i417[15], 0, i416, 'targetTexture')
  i416.usePhysicalProperties = !!i417[16]
  i416.focalLength = i417[17]
  i416.sensorSize = new pc.Vec2( i417[18], i417[19] )
  i416.lensShift = new pc.Vec2( i417[20], i417[21] )
  i416.gateFit = i417[22]
  i416.commandBufferCount = i417[23]
  i416.cameraType = i417[24]
  i416.enabled = !!i417[25]
  return i416
}

Deserializers["UnityEngine.Rendering.Universal.UniversalAdditionalCameraData"] = function (request, data, root) {
  var i418 = root || request.c( 'UnityEngine.Rendering.Universal.UniversalAdditionalCameraData' )
  var i419 = data
  i418.m_RenderShadows = !!i419[0]
  i418.m_RequiresDepthTextureOption = i419[1]
  i418.m_RequiresOpaqueTextureOption = i419[2]
  i418.m_CameraType = i419[3]
  var i421 = i419[4]
  var i420 = new (System.Collections.Generic.List$1(Bridge.ns('UnityEngine.Camera')))
  for(var i = 0; i < i421.length; i += 2) {
  request.r(i421[i + 0], i421[i + 1], 1, i420, '')
  }
  i418.m_Cameras = i420
  i418.m_RendererIndex = i419[5]
  i418.m_VolumeLayerMask = UnityEngine.LayerMask.FromIntegerValue( i419[6] )
  request.r(i419[7], i419[8], 0, i418, 'm_VolumeTrigger')
  i418.m_VolumeFrameworkUpdateModeOption = i419[9]
  i418.m_RenderPostProcessing = !!i419[10]
  i418.m_Antialiasing = i419[11]
  i418.m_AntialiasingQuality = i419[12]
  i418.m_StopNaN = !!i419[13]
  i418.m_Dithering = !!i419[14]
  i418.m_ClearDepth = !!i419[15]
  i418.m_AllowXRRendering = !!i419[16]
  i418.m_AllowHDROutput = !!i419[17]
  i418.m_UseScreenCoordOverride = !!i419[18]
  i418.m_ScreenSizeOverride = new pc.Vec4( i419[19], i419[20], i419[21], i419[22] )
  i418.m_ScreenCoordScaleBias = new pc.Vec4( i419[23], i419[24], i419[25], i419[26] )
  i418.m_RequiresDepthTexture = !!i419[27]
  i418.m_RequiresColorTexture = !!i419[28]
  i418.m_Version = i419[29]
  i418.m_TaaSettings = request.d('UnityEngine.Rendering.Universal.TemporalAA+Settings', i419[30], i418.m_TaaSettings)
  return i418
}

Deserializers["UnityEngine.Rendering.Universal.TemporalAA+Settings"] = function (request, data, root) {
  var i424 = root || request.c( 'UnityEngine.Rendering.Universal.TemporalAA+Settings' )
  var i425 = data
  i424.m_Quality = i425[0]
  i424.m_FrameInfluence = i425[1]
  i424.m_JitterScale = i425[2]
  i424.m_MipBias = i425[3]
  i424.m_VarianceClampScale = i425[4]
  i424.m_ContrastAdaptiveSharpening = i425[5]
  return i424
}

Deserializers["Luna.Unity.DTO.UnityEngine.Scene.GameObject"] = function (request, data, root) {
  var i426 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Scene.GameObject' )
  var i427 = data
  i426.name = i427[0]
  i426.tagId = i427[1]
  i426.enabled = !!i427[2]
  i426.isStatic = !!i427[3]
  i426.layer = i427[4]
  return i426
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.MeshFilter"] = function (request, data, root) {
  var i428 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.MeshFilter' )
  var i429 = data
  request.r(i429[0], i429[1], 0, i428, 'sharedMesh')
  return i428
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.MeshRenderer"] = function (request, data, root) {
  var i430 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.MeshRenderer' )
  var i431 = data
  request.r(i431[0], i431[1], 0, i430, 'additionalVertexStreams')
  i430.enabled = !!i431[2]
  request.r(i431[3], i431[4], 0, i430, 'sharedMaterial')
  var i433 = i431[5]
  var i432 = []
  for(var i = 0; i < i433.length; i += 2) {
  request.r(i433[i + 0], i433[i + 1], 2, i432, '')
  }
  i430.sharedMaterials = i432
  i430.receiveShadows = !!i431[6]
  i430.shadowCastingMode = i431[7]
  i430.sortingLayerID = i431[8]
  i430.sortingOrder = i431[9]
  i430.lightmapIndex = i431[10]
  i430.lightmapSceneIndex = i431[11]
  i430.lightmapScaleOffset = new pc.Vec4( i431[12], i431[13], i431[14], i431[15] )
  i430.lightProbeUsage = i431[16]
  i430.reflectionProbeUsage = i431[17]
  return i430
}

Deserializers["CastleDestroyers.Core.GameBootstrap"] = function (request, data, root) {
  var i436 = root || request.c( 'CastleDestroyers.Core.GameBootstrap' )
  var i437 = data
  request.r(i437[0], i437[1], 0, i436, 'config')
  request.r(i437[2], i437[3], 0, i436, 'mainCamera')
  var i439 = i437[4]
  var i438 = []
  for(var i = 0; i < i439.length; i += 2) {
  request.r(i439[i + 0], i439[i + 1], 2, i438, '')
  }
  i436.backFilters = i438
  request.r(i437[5], i437[6], 0, i436, 'decorFilter')
  request.r(i437[7], i437[8], 0, i436, 'frontFilter')
  request.r(i437[9], i437[10], 0, i436, 'waveFilter')
  request.r(i437[11], i437[12], 0, i436, 'cannonsFilter')
  var i441 = i437[13]
  var i440 = []
  for(var i = 0; i < i441.length; i += 2) {
  request.r(i441[i + 0], i441[i + 1], 2, i440, '')
  }
  i436.castleFilters = i440
  request.r(i437[14], i437[15], 0, i436, 'undercarriageFilter')
  request.r(i437[16], i437[17], 0, i436, 'particlesFilter')
  request.r(i437[18], i437[19], 0, i436, 'smokeFilter')
  request.r(i437[20], i437[21], 0, i436, 'rocketsFilter')
  request.r(i437[22], i437[23], 0, i436, 'boostersFilter')
  request.r(i437[24], i437[25], 0, i436, 'explosionsFilter')
  request.r(i437[26], i437[27], 0, i436, 'aimDotsFilter')
  request.r(i437[28], i437[29], 0, i436, 'aimPanel')
  request.r(i437[30], i437[31], 0, i436, 'aimPanelBackground')
  request.r(i437[32], i437[33], 0, i436, 'aimPowerText')
  request.r(i437[34], i437[35], 0, i436, 'aimAngleText')
  var i443 = i437[36]
  var i442 = []
  for(var i = 0; i < i443.length; i += 2) {
  request.r(i443[i + 0], i443[i + 1], 2, i442, '')
  }
  i436.aimCaptions = i442
  request.r(i437[37], i437[38], 0, i436, 'aimDegree')
  request.r(i437[39], i437[40], 0, i436, 'hudFilter')
  request.r(i437[41], i437[42], 0, i436, 'gameCanvas')
  request.r(i437[43], i437[44], 0, i436, 'bannerRoot')
  request.r(i437[45], i437[46], 0, i436, 'bannerBackground')
  request.r(i437[47], i437[48], 0, i436, 'bannerText')
  request.r(i437[49], i437[50], 0, i436, 'handRoot')
  var i445 = i437[51]
  var i444 = []
  for(var i = 0; i < i445.length; i += 2) {
  request.r(i445[i + 0], i445[i + 1], 2, i444, '')
  }
  i436.handImages = i444
  request.r(i437[52], i437[53], 0, i436, 'endCardRoot')
  request.r(i437[54], i437[55], 0, i436, 'endCardDim')
  request.r(i437[56], i437[57], 0, i436, 'endCardContent')
  request.r(i437[58], i437[59], 0, i436, 'endCardButton')
  request.r(i437[60], i437[61], 0, i436, 'endCardIcon')
  request.r(i437[62], i437[63], 0, i436, 'endCardCaption')
  request.r(i437[64], i437[65], 0, i436, 'endCardButtonFace')
  request.r(i437[66], i437[67], 0, i436, 'endCardButtonEdge')
  request.r(i437[68], i437[69], 0, i436, 'endCardButtonText')
  request.r(i437[70], i437[71], 0, i436, 'endCardDisc')
  request.r(i437[72], i437[73], 0, i436, 'endCardRestart')
  request.r(i437[74], i437[75], 0, i436, 'endCardRestartDisc')
  request.r(i437[76], i437[77], 0, i436, 'endCardRestartIcon')
  request.r(i437[78], i437[79], 0, i436, 'endCardBlueCastle')
  request.r(i437[80], i437[81], 0, i436, 'endCardRedCastle')
  var i447 = i437[82]
  var i446 = []
  for(var i = 0; i < i447.length; i += 2) {
  request.r(i447[i + 0], i447[i + 1], 2, i446, '')
  }
  i436.endCardLayout = i446
  var i449 = i437[83]
  var i448 = []
  for(var i = 0; i < i449.length; i += 2) {
  request.r(i449[i + 0], i449[i + 1], 2, i448, '')
  }
  i436.soundClips = i448
  var i451 = i437[84]
  var i450 = []
  for(var i = 0; i < i451.length; i += 2) {
  request.r(i451[i + 0], i451[i + 1], 2, i450, '')
  }
  i436.soundSources = i450
  request.r(i437[85], i437[86], 0, i436, 'terrainRoot')
  request.r(i437[87], i437[88], 0, i436, 'terrainMaterial')
  request.r(i437[89], i437[90], 0, i436, 'flatMaterial')
  request.r(i437[91], i437[92], 0, i436, 'rocketMaterial')
  request.r(i437[93], i437[94], 0, i436, 'castleMaterial')
  i436.terrainSortingOrder = i437[95]
  i436.chunkSortingOrder = i437[96]
  i436.castleChunkSortingOrder = i437[97]
  request.r(i437[98], i437[99], 0, i436, 'debugRoot')
  request.r(i437[100], i437[101], 0, i436, 'debugOverlay')
  return i436
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.AudioSource"] = function (request, data, root) {
  var i464 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.AudioSource' )
  var i465 = data
  request.r(i465[0], i465[1], 0, i464, 'clip')
  request.r(i465[2], i465[3], 0, i464, 'outputAudioMixerGroup')
  i464.playOnAwake = !!i465[4]
  i464.loop = !!i465[5]
  i464.time = i465[6]
  i464.volume = i465[7]
  i464.pitch = i465[8]
  i464.enabled = !!i465[9]
  return i464
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.RectTransform"] = function (request, data, root) {
  var i466 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.RectTransform' )
  var i467 = data
  i466.pivot = new pc.Vec2( i467[0], i467[1] )
  i466.anchorMin = new pc.Vec2( i467[2], i467[3] )
  i466.anchorMax = new pc.Vec2( i467[4], i467[5] )
  i466.sizeDelta = new pc.Vec2( i467[6], i467[7] )
  i466.anchoredPosition3D = new pc.Vec3( i467[8], i467[9], i467[10] )
  i466.rotation = new pc.Quat(i467[11], i467[12], i467[13], i467[14])
  i466.scale = new pc.Vec3( i467[15], i467[16], i467[17] )
  return i466
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.Canvas"] = function (request, data, root) {
  var i468 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.Canvas' )
  var i469 = data
  i468.planeDistance = i469[0]
  i468.referencePixelsPerUnit = i469[1]
  i468.isFallbackOverlay = !!i469[2]
  i468.renderMode = i469[3]
  i468.renderOrder = i469[4]
  i468.sortingLayerName = i469[5]
  i468.sortingOrder = i469[6]
  i468.scaleFactor = i469[7]
  request.r(i469[8], i469[9], 0, i468, 'worldCamera')
  i468.overrideSorting = !!i469[10]
  i468.pixelPerfect = !!i469[11]
  i468.targetDisplay = i469[12]
  i468.overridePixelPerfect = !!i469[13]
  i468.enabled = !!i469[14]
  return i468
}

Deserializers["UnityEngine.UI.CanvasScaler"] = function (request, data, root) {
  var i470 = root || request.c( 'UnityEngine.UI.CanvasScaler' )
  var i471 = data
  i470.m_UiScaleMode = i471[0]
  i470.m_ReferencePixelsPerUnit = i471[1]
  i470.m_ScaleFactor = i471[2]
  i470.m_ReferenceResolution = new pc.Vec2( i471[3], i471[4] )
  i470.m_ScreenMatchMode = i471[5]
  i470.m_MatchWidthOrHeight = i471[6]
  i470.m_PhysicalUnit = i471[7]
  i470.m_FallbackScreenDPI = i471[8]
  i470.m_DefaultSpriteDPI = i471[9]
  i470.m_DynamicPixelsPerUnit = i471[10]
  i470.m_PresetInfoIsWorld = !!i471[11]
  return i470
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.CanvasRenderer"] = function (request, data, root) {
  var i472 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.CanvasRenderer' )
  var i473 = data
  i472.cullTransparentMesh = !!i473[0]
  return i472
}

Deserializers["UnityEngine.UI.Image"] = function (request, data, root) {
  var i474 = root || request.c( 'UnityEngine.UI.Image' )
  var i475 = data
  request.r(i475[0], i475[1], 0, i474, 'm_Sprite')
  i474.m_Type = i475[2]
  i474.m_PreserveAspect = !!i475[3]
  i474.m_FillCenter = !!i475[4]
  i474.m_FillMethod = i475[5]
  i474.m_FillAmount = i475[6]
  i474.m_FillClockwise = !!i475[7]
  i474.m_FillOrigin = i475[8]
  i474.m_UseSpriteMesh = !!i475[9]
  i474.m_PixelsPerUnitMultiplier = i475[10]
  request.r(i475[11], i475[12], 0, i474, 'm_Material')
  i474.m_Maskable = !!i475[13]
  i474.m_Color = new pc.Color(i475[14], i475[15], i475[16], i475[17])
  i474.m_RaycastTarget = !!i475[18]
  i474.m_RaycastPadding = new pc.Vec4( i475[19], i475[20], i475[21], i475[22] )
  return i474
}

Deserializers["UnityEngine.UI.Text"] = function (request, data, root) {
  var i476 = root || request.c( 'UnityEngine.UI.Text' )
  var i477 = data
  i476.m_FontData = request.d('UnityEngine.UI.FontData', i477[0], i476.m_FontData)
  i476.m_Text = i477[1]
  request.r(i477[2], i477[3], 0, i476, 'm_Material')
  i476.m_Maskable = !!i477[4]
  i476.m_Color = new pc.Color(i477[5], i477[6], i477[7], i477[8])
  i476.m_RaycastTarget = !!i477[9]
  i476.m_RaycastPadding = new pc.Vec4( i477[10], i477[11], i477[12], i477[13] )
  return i476
}

Deserializers["UnityEngine.UI.FontData"] = function (request, data, root) {
  var i478 = root || request.c( 'UnityEngine.UI.FontData' )
  var i479 = data
  request.r(i479[0], i479[1], 0, i478, 'm_Font')
  i478.m_FontSize = i479[2]
  i478.m_FontStyle = i479[3]
  i478.m_BestFit = !!i479[4]
  i478.m_MinSize = i479[5]
  i478.m_MaxSize = i479[6]
  i478.m_Alignment = i479[7]
  i478.m_AlignByGeometry = !!i479[8]
  i478.m_RichText = !!i479[9]
  i478.m_HorizontalOverflow = i479[10]
  i478.m_VerticalOverflow = i479[11]
  i478.m_LineSpacing = i479[12]
  return i478
}

Deserializers["UnityEngine.UI.GraphicRaycaster"] = function (request, data, root) {
  var i480 = root || request.c( 'UnityEngine.UI.GraphicRaycaster' )
  var i481 = data
  i480.m_IgnoreReversedGraphics = !!i481[0]
  i480.m_BlockingObjects = i481[1]
  i480.m_BlockingMask = UnityEngine.LayerMask.FromIntegerValue( i481[2] )
  return i480
}

Deserializers["CastleDestroyers.DebugTools.DebugOverlay"] = function (request, data, root) {
  var i482 = root || request.c( 'CastleDestroyers.DebugTools.DebugOverlay' )
  var i483 = data
  request.r(i483[0], i483[1], 0, i482, 'canvasScaler')
  request.r(i483[2], i483[3], 0, i482, 'panelRoot')
  request.r(i483[4], i483[5], 0, i482, 'toggleButton')
  request.r(i483[6], i483[7], 0, i482, 'toggleLabel')
  request.r(i483[8], i483[9], 0, i482, 'statsText')
  request.r(i483[10], i483[11], 0, i482, 'statsBackground')
  var i485 = i483[12]
  var i484 = []
  for(var i = 0; i < i485.length; i += 2) {
  request.r(i485[i + 0], i485[i + 1], 2, i484, '')
  }
  i482.buttonImages = i484
  var i487 = i483[13]
  var i486 = []
  for(var i = 0; i < i487.length; i += 2) {
  request.r(i487[i + 0], i487[i + 1], 2, i486, '')
  }
  i482.buttonLabels = i486
  request.r(i483[14], i483[15], 0, i482, 'nextStateButton')
  request.r(i483[16], i483[17], 0, i482, 'slowButton')
  request.r(i483[18], i483[19], 0, i482, 'rocketsButton')
  request.r(i483[20], i483[21], 0, i482, 'themeButton')
  request.r(i483[22], i483[23], 0, i482, 'restartButton')
  request.r(i483[24], i483[25], 0, i482, 'blastButton')
  request.r(i483[26], i483[27], 0, i482, 'cameraButton')
  request.r(i483[28], i483[29], 0, i482, 'shotX20Button')
  request.r(i483[30], i483[31], 0, i482, 'shotX10Button')
  request.r(i483[32], i483[33], 0, i482, 'shotX50Button')
  request.r(i483[34], i483[35], 0, i482, 'blastLabel')
  request.r(i483[36], i483[37], 0, i482, 'cameraLabel')
  request.r(i483[38], i483[39], 0, i482, 'rocketsLabel')
  request.r(i483[40], i483[41], 0, i482, 'themeLabel')
  return i482
}

Deserializers["UnityEngine.EventSystems.EventSystem"] = function (request, data, root) {
  var i488 = root || request.c( 'UnityEngine.EventSystems.EventSystem' )
  var i489 = data
  request.r(i489[0], i489[1], 0, i488, 'm_FirstSelected')
  i488.m_sendNavigationEvents = !!i489[2]
  i488.m_DragThreshold = i489[3]
  return i488
}

Deserializers["UnityEngine.EventSystems.StandaloneInputModule"] = function (request, data, root) {
  var i490 = root || request.c( 'UnityEngine.EventSystems.StandaloneInputModule' )
  var i491 = data
  i490.m_HorizontalAxis = i491[0]
  i490.m_VerticalAxis = i491[1]
  i490.m_SubmitButton = i491[2]
  i490.m_CancelButton = i491[3]
  i490.m_InputActionsPerSecond = i491[4]
  i490.m_RepeatDelay = i491[5]
  i490.m_ForceModuleActive = !!i491[6]
  i490.m_SendPointerHoverToParent = !!i491[7]
  return i490
}

Deserializers["UnityEngine.UI.GridLayoutGroup"] = function (request, data, root) {
  var i492 = root || request.c( 'UnityEngine.UI.GridLayoutGroup' )
  var i493 = data
  i492.m_StartCorner = i493[0]
  i492.m_StartAxis = i493[1]
  i492.m_CellSize = new pc.Vec2( i493[2], i493[3] )
  i492.m_Spacing = new pc.Vec2( i493[4], i493[5] )
  i492.m_Constraint = i493[6]
  i492.m_ConstraintCount = i493[7]
  i492.m_Padding = UnityEngine.RectOffset.FromPaddings(i493[8], i493[9], i493[10], i493[11])
  i492.m_ChildAlignment = i493[12]
  return i492
}

Deserializers["UnityEngine.UI.Button"] = function (request, data, root) {
  var i494 = root || request.c( 'UnityEngine.UI.Button' )
  var i495 = data
  i494.m_OnClick = request.d('UnityEngine.UI.Button+ButtonClickedEvent', i495[0], i494.m_OnClick)
  i494.m_Navigation = request.d('UnityEngine.UI.Navigation', i495[1], i494.m_Navigation)
  i494.m_Transition = i495[2]
  i494.m_Colors = request.d('UnityEngine.UI.ColorBlock', i495[3], i494.m_Colors)
  i494.m_SpriteState = request.d('UnityEngine.UI.SpriteState', i495[4], i494.m_SpriteState)
  i494.m_AnimationTriggers = request.d('UnityEngine.UI.AnimationTriggers', i495[5], i494.m_AnimationTriggers)
  i494.m_Interactable = !!i495[6]
  request.r(i495[7], i495[8], 0, i494, 'm_TargetGraphic')
  return i494
}

Deserializers["UnityEngine.UI.Button+ButtonClickedEvent"] = function (request, data, root) {
  var i496 = root || request.c( 'UnityEngine.UI.Button+ButtonClickedEvent' )
  var i497 = data
  i496.m_PersistentCalls = request.d('UnityEngine.Events.PersistentCallGroup', i497[0], i496.m_PersistentCalls)
  return i496
}

Deserializers["UnityEngine.Events.PersistentCallGroup"] = function (request, data, root) {
  var i498 = root || request.c( 'UnityEngine.Events.PersistentCallGroup' )
  var i499 = data
  var i501 = i499[0]
  var i500 = new (System.Collections.Generic.List$1(Bridge.ns('UnityEngine.Events.PersistentCall')))
  for(var i = 0; i < i501.length; i += 1) {
    i500.add(request.d('UnityEngine.Events.PersistentCall', i501[i + 0]));
  }
  i498.m_Calls = i500
  return i498
}

Deserializers["UnityEngine.Events.PersistentCall"] = function (request, data, root) {
  var i504 = root || request.c( 'UnityEngine.Events.PersistentCall' )
  var i505 = data
  request.r(i505[0], i505[1], 0, i504, 'm_Target')
  i504.m_TargetAssemblyTypeName = i505[2]
  i504.m_MethodName = i505[3]
  i504.m_Mode = i505[4]
  i504.m_Arguments = request.d('UnityEngine.Events.ArgumentCache', i505[5], i504.m_Arguments)
  i504.m_CallState = i505[6]
  return i504
}

Deserializers["UnityEngine.UI.Navigation"] = function (request, data, root) {
  var i506 = root || request.c( 'UnityEngine.UI.Navigation' )
  var i507 = data
  i506.m_Mode = i507[0]
  i506.m_WrapAround = !!i507[1]
  request.r(i507[2], i507[3], 0, i506, 'm_SelectOnUp')
  request.r(i507[4], i507[5], 0, i506, 'm_SelectOnDown')
  request.r(i507[6], i507[7], 0, i506, 'm_SelectOnLeft')
  request.r(i507[8], i507[9], 0, i506, 'm_SelectOnRight')
  return i506
}

Deserializers["UnityEngine.UI.ColorBlock"] = function (request, data, root) {
  var i508 = root || request.c( 'UnityEngine.UI.ColorBlock' )
  var i509 = data
  i508.m_NormalColor = new pc.Color(i509[0], i509[1], i509[2], i509[3])
  i508.m_HighlightedColor = new pc.Color(i509[4], i509[5], i509[6], i509[7])
  i508.m_PressedColor = new pc.Color(i509[8], i509[9], i509[10], i509[11])
  i508.m_SelectedColor = new pc.Color(i509[12], i509[13], i509[14], i509[15])
  i508.m_DisabledColor = new pc.Color(i509[16], i509[17], i509[18], i509[19])
  i508.m_ColorMultiplier = i509[20]
  i508.m_FadeDuration = i509[21]
  return i508
}

Deserializers["UnityEngine.UI.SpriteState"] = function (request, data, root) {
  var i510 = root || request.c( 'UnityEngine.UI.SpriteState' )
  var i511 = data
  request.r(i511[0], i511[1], 0, i510, 'm_HighlightedSprite')
  request.r(i511[2], i511[3], 0, i510, 'm_PressedSprite')
  request.r(i511[4], i511[5], 0, i510, 'm_SelectedSprite')
  request.r(i511[6], i511[7], 0, i510, 'm_DisabledSprite')
  return i510
}

Deserializers["UnityEngine.UI.AnimationTriggers"] = function (request, data, root) {
  var i512 = root || request.c( 'UnityEngine.UI.AnimationTriggers' )
  var i513 = data
  i512.m_NormalTrigger = i513[0]
  i512.m_HighlightedTrigger = i513[1]
  i512.m_PressedTrigger = i513[2]
  i512.m_SelectedTrigger = i513[3]
  i512.m_DisabledTrigger = i513[4]
  return i512
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderSettings"] = function (request, data, root) {
  var i514 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.RenderSettings' )
  var i515 = data
  i514.ambientIntensity = i515[0]
  i514.reflectionIntensity = i515[1]
  i514.ambientMode = i515[2]
  i514.ambientLight = new pc.Color(i515[3], i515[4], i515[5], i515[6])
  i514.ambientSkyColor = new pc.Color(i515[7], i515[8], i515[9], i515[10])
  i514.ambientGroundColor = new pc.Color(i515[11], i515[12], i515[13], i515[14])
  i514.ambientEquatorColor = new pc.Color(i515[15], i515[16], i515[17], i515[18])
  i514.fogColor = new pc.Color(i515[19], i515[20], i515[21], i515[22])
  i514.fogEndDistance = i515[23]
  i514.fogStartDistance = i515[24]
  i514.fogDensity = i515[25]
  i514.fog = !!i515[26]
  request.r(i515[27], i515[28], 0, i514, 'skybox')
  i514.fogMode = i515[29]
  var i517 = i515[30]
  var i516 = []
  for(var i = 0; i < i517.length; i += 1) {
    i516.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+Lightmap', i517[i + 0]) );
  }
  i514.lightmaps = i516
  i514.lightProbes = request.d('Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+LightProbes', i515[31], i514.lightProbes)
  i514.lightmapsMode = i515[32]
  i514.mixedBakeMode = i515[33]
  i514.environmentLightingMode = i515[34]
  i514.ambientProbe = new pc.SphericalHarmonicsL2(i515[35])
  request.r(i515[36], i515[37], 0, i514, 'customReflection')
  request.r(i515[38], i515[39], 0, i514, 'defaultReflection')
  i514.defaultReflectionMode = i515[40]
  i514.defaultReflectionResolution = i515[41]
  i514.sunLightObjectId = i515[42]
  i514.pixelLightCount = i515[43]
  i514.defaultReflectionHDR = !!i515[44]
  i514.hasLightDataAsset = !!i515[45]
  i514.hasManualGenerate = !!i515[46]
  return i514
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+Lightmap"] = function (request, data, root) {
  var i520 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+Lightmap' )
  var i521 = data
  request.r(i521[0], i521[1], 0, i520, 'lightmapColor')
  request.r(i521[2], i521[3], 0, i520, 'lightmapDirection')
  request.r(i521[4], i521[5], 0, i520, 'shadowMask')
  return i520
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+LightProbes"] = function (request, data, root) {
  var i522 = root || new UnityEngine.LightProbes()
  var i523 = data
  return i522
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.UniversalRenderPipelineAsset"] = function (request, data, root) {
  var i530 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.UniversalRenderPipelineAsset' )
  var i531 = data
  i530.AdditionalLightsRenderingMode = i531[0]
  i530.LightRenderingMode = request.d('Luna.Unity.DTO.UnityEngine.Assets.LightRenderingMode', i531[1], i530.LightRenderingMode)
  i530.MainLightRenderingModeValue = i531[2]
  i530.SupportsMainLightShadows = !!i531[3]
  i530.MixedLightingSupported = !!i531[4]
  i530.MainLightShadowmapResolutionValue = i531[5]
  i530.SupportsSoftShadows = !!i531[6]
  i530.SoftShadowQualityValue = i531[7]
  i530.ShadowDistance = i531[8]
  i530.ShadowCascadeCount = i531[9]
  i530.Cascade2Split = i531[10]
  i530.Cascade3Split = new pc.Vec2( i531[11], i531[12] )
  i530.Cascade4Split = new pc.Vec3( i531[13], i531[14], i531[15] )
  i530.CascadeBorder = i531[16]
  i530.ShadowDepthBias = i531[17]
  i530.ShadowNormalBias = i531[18]
  i530.RequireDepthTexture = !!i531[19]
  i530.RequireOpaqueTexture = !!i531[20]
  i530.scriptableRendererData = request.d('Luna.Unity.DTO.UnityEngine.Assets.ScriptableRendererData', i531[21], i530.scriptableRendererData)
  return i530
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.LightRenderingMode"] = function (request, data, root) {
  var i532 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.LightRenderingMode' )
  var i533 = data
  i532.Disabled = i533[0]
  i532.PerVertex = i533[1]
  i532.PerPixel = i533[2]
  return i532
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ScriptableRendererData"] = function (request, data, root) {
  var i534 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ScriptableRendererData' )
  var i535 = data
  i534.opaqueLayerMask = i535[0]
  i534.transparentLayerMask = i535[1]
  var i537 = i535[2]
  var i536 = []
  for(var i = 0; i < i537.length; i += 1) {
    i536.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.RenderObjects', i537[i + 0]) );
  }
  i534.RenderObjectsFeatures = i536
  i534.name = i535[3]
  return i534
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderObjects"] = function (request, data, root) {
  var i540 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.RenderObjects' )
  var i541 = data
  i540.settings = request.d('Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+RenderObjectsSettings', i541[0], i540.settings)
  i540.name = i541[1]
  i540.typeName = i541[2]
  return i540
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader"] = function (request, data, root) {
  var i542 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader' )
  var i543 = data
  var i545 = i543[0]
  var i544 = new (System.Collections.Generic.List$1(Bridge.ns('Luna.Unity.DTO.UnityEngine.Assets.Shader+ShaderCompilationError')))
  for(var i = 0; i < i545.length; i += 1) {
    i544.add(request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+ShaderCompilationError', i545[i + 0]));
  }
  i542.ShaderCompilationErrors = i544
  i542.name = i543[1]
  i542.guid = i543[2]
  var i547 = i543[3]
  var i546 = []
  for(var i = 0; i < i547.length; i += 1) {
    i546.push( i547[i + 0] );
  }
  i542.shaderDefinedKeywords = i546
  var i549 = i543[4]
  var i548 = []
  for(var i = 0; i < i549.length; i += 1) {
    i548.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass', i549[i + 0]) );
  }
  i542.passes = i548
  var i551 = i543[5]
  var i550 = []
  for(var i = 0; i < i551.length; i += 1) {
    i550.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+UsePass', i551[i + 0]) );
  }
  i542.usePasses = i550
  var i553 = i543[6]
  var i552 = []
  for(var i = 0; i < i553.length; i += 1) {
    i552.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+DefaultParameterValue', i553[i + 0]) );
  }
  i542.defaultParameterValues = i552
  request.r(i543[7], i543[8], 0, i542, 'unityFallbackShader')
  i542.readDepth = !!i543[9]
  i542.hasDepthOnlyPass = !!i543[10]
  i542.isCreatedByShaderGraph = !!i543[11]
  i542.disableBatching = !!i543[12]
  i542.compiled = !!i543[13]
  return i542
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+ShaderCompilationError"] = function (request, data, root) {
  var i556 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+ShaderCompilationError' )
  var i557 = data
  i556.shaderName = i557[0]
  i556.errorMessage = i557[1]
  return i556
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass"] = function (request, data, root) {
  var i562 = root || new pc.UnityShaderPass()
  var i563 = data
  i562.id = i563[0]
  i562.subShaderIndex = i563[1]
  i562.name = i563[2]
  i562.passType = i563[3]
  i562.grabPassTextureName = i563[4]
  i562.usePass = !!i563[5]
  i562.zTest = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i563[6], i562.zTest)
  i562.zWrite = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i563[7], i562.zWrite)
  i562.culling = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i563[8], i562.culling)
  i562.blending = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Blending', i563[9], i562.blending)
  i562.alphaBlending = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Blending', i563[10], i562.alphaBlending)
  i562.colorWriteMask = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i563[11], i562.colorWriteMask)
  i562.offsetUnits = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i563[12], i562.offsetUnits)
  i562.offsetFactor = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i563[13], i562.offsetFactor)
  i562.stencilRef = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i563[14], i562.stencilRef)
  i562.stencilReadMask = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i563[15], i562.stencilReadMask)
  i562.stencilWriteMask = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i563[16], i562.stencilWriteMask)
  i562.stencilOp = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+StencilOp', i563[17], i562.stencilOp)
  i562.stencilOpFront = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+StencilOp', i563[18], i562.stencilOpFront)
  i562.stencilOpBack = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+StencilOp', i563[19], i562.stencilOpBack)
  var i565 = i563[20]
  var i564 = []
  for(var i = 0; i < i565.length; i += 1) {
    i564.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Tag', i565[i + 0]) );
  }
  i562.tags = i564
  var i567 = i563[21]
  var i566 = []
  for(var i = 0; i < i567.length; i += 1) {
    i566.push( i567[i + 0] );
  }
  i562.passDefinedKeywords = i566
  var i569 = i563[22]
  var i568 = []
  for(var i = 0; i < i569.length; i += 1) {
    i568.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+KeywordGroup', i569[i + 0]) );
  }
  i562.passDefinedKeywordGroups = i568
  var i571 = i563[23]
  var i570 = []
  for(var i = 0; i < i571.length; i += 1) {
    i570.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Variant', i571[i + 0]) );
  }
  i562.variants = i570
  var i573 = i563[24]
  var i572 = []
  for(var i = 0; i < i573.length; i += 1) {
    i572.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Variant', i573[i + 0]) );
  }
  i562.excludedVariants = i572
  i562.hasDepthReader = !!i563[25]
  return i562
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value"] = function (request, data, root) {
  var i574 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value' )
  var i575 = data
  i574.val = i575[0]
  i574.name = i575[1]
  return i574
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Blending"] = function (request, data, root) {
  var i576 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Blending' )
  var i577 = data
  i576.src = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i577[0], i576.src)
  i576.dst = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i577[1], i576.dst)
  i576.op = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i577[2], i576.op)
  return i576
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+StencilOp"] = function (request, data, root) {
  var i578 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+StencilOp' )
  var i579 = data
  i578.pass = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i579[0], i578.pass)
  i578.fail = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i579[1], i578.fail)
  i578.zFail = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i579[2], i578.zFail)
  i578.comp = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i579[3], i578.comp)
  return i578
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Tag"] = function (request, data, root) {
  var i582 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Tag' )
  var i583 = data
  i582.name = i583[0]
  i582.value = i583[1]
  return i582
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+KeywordGroup"] = function (request, data, root) {
  var i586 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+KeywordGroup' )
  var i587 = data
  var i589 = i587[0]
  var i588 = []
  for(var i = 0; i < i589.length; i += 1) {
    i588.push( i589[i + 0] );
  }
  i586.keywords = i588
  i586.hasDiscard = !!i587[1]
  return i586
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Variant"] = function (request, data, root) {
  var i592 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Variant' )
  var i593 = data
  i592.passId = i593[0]
  i592.subShaderIndex = i593[1]
  var i595 = i593[2]
  var i594 = []
  for(var i = 0; i < i595.length; i += 1) {
    i594.push( i595[i + 0] );
  }
  i592.keywords = i594
  i592.vertexProgram = i593[3]
  i592.fragmentProgram = i593[4]
  i592.exportedForWebGl2 = !!i593[5]
  i592.readDepth = !!i593[6]
  return i592
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+UsePass"] = function (request, data, root) {
  var i598 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+UsePass' )
  var i599 = data
  request.r(i599[0], i599[1], 0, i598, 'shader')
  i598.pass = i599[2]
  return i598
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+DefaultParameterValue"] = function (request, data, root) {
  var i602 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+DefaultParameterValue' )
  var i603 = data
  i602.name = i603[0]
  i602.type = i603[1]
  i602.value = new pc.Vec4( i603[2], i603[3], i603[4], i603[5] )
  i602.textureValue = i603[6]
  i602.shaderPropertyFlag = i603[7]
  return i602
}

Deserializers["Luna.Unity.DTO.UnityEngine.Textures.Sprite"] = function (request, data, root) {
  var i604 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Textures.Sprite' )
  var i605 = data
  i604.name = i605[0]
  request.r(i605[1], i605[2], 0, i604, 'texture')
  i604.aabb = i605[3]
  i604.vertices = i605[4]
  i604.triangles = i605[5]
  i604.textureRect = UnityEngine.Rect.MinMaxRect(i605[6], i605[7], i605[8], i605[9])
  i604.packedRect = UnityEngine.Rect.MinMaxRect(i605[10], i605[11], i605[12], i605[13])
  i604.border = new pc.Vec4( i605[14], i605[15], i605[16], i605[17] )
  i604.transparency = i605[18]
  i604.bounds = i605[19]
  i604.pixelsPerUnit = i605[20]
  i604.textureWidth = i605[21]
  i604.textureHeight = i605[22]
  i604.nativeSize = new pc.Vec2( i605[23], i605[24] )
  i604.pivot = new pc.Vec2( i605[25], i605[26] )
  i604.textureRectOffset = new pc.Vec2( i605[27], i605[28] )
  return i604
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.AudioClip"] = function (request, data, root) {
  var i606 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.AudioClip' )
  var i607 = data
  i606.name = i607[0]
  return i606
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Font"] = function (request, data, root) {
  var i608 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Font' )
  var i609 = data
  i608.name = i609[0]
  i608.ascent = i609[1]
  i608.originalLineHeight = i609[2]
  i608.fontSize = i609[3]
  var i611 = i609[4]
  var i610 = []
  for(var i = 0; i < i611.length; i += 1) {
    i610.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Font+CharacterInfo', i611[i + 0]) );
  }
  i608.characterInfo = i610
  request.r(i609[5], i609[6], 0, i608, 'texture')
  i608.originalFontSize = i609[7]
  return i608
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Font+CharacterInfo"] = function (request, data, root) {
  var i614 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Font+CharacterInfo' )
  var i615 = data
  i614.index = i615[0]
  i614.advance = i615[1]
  i614.bearing = i615[2]
  i614.glyphWidth = i615[3]
  i614.glyphHeight = i615[4]
  i614.minX = i615[5]
  i614.maxX = i615[6]
  i614.minY = i615[7]
  i614.maxY = i615[8]
  i614.uvBottomLeftX = i615[9]
  i614.uvBottomLeftY = i615[10]
  i614.uvBottomRightX = i615[11]
  i614.uvBottomRightY = i615[12]
  i614.uvTopLeftX = i615[13]
  i614.uvTopLeftY = i615[14]
  i614.uvTopRightX = i615[15]
  i614.uvTopRightY = i615[16]
  return i614
}

Deserializers["CastleDestroyers.Core.GameConfig"] = function (request, data, root) {
  var i616 = root || request.c( 'CastleDestroyers.Core.GameConfig' )
  var i617 = data
  request.r(i617[0], i617[1], 0, i616, 'theme')
  i616.rngSeed = i617[2]
  i616.maxPlayerTurns = i617[3]
  i616.maxSessionSeconds = i617[4]
  i616.maxDeltaTime = i617[5]
  i616.directorEnabled = !!i617[6]
  i616.directorMaxLossPerAiTurn = i617[7]
  i616.directorEnemyKeepCannons = i617[8]
  i616.directorFinisherWeight = i617[9]
  i616.bonusPickEnabled = !!i617[10]
  var i619 = i617[11]
  var i618 = []
  for(var i = 0; i < i619.length; i += 1) {
    i618.push( request.d('CastleDestroyers.Core.TurnHint', i619[i + 0]) );
  }
  i616.turnHints = i618
  i616.bootDuration = i617[12]
  i616.introDuration = i617[13]
  i616.playerAimDuration = i617[14]
  i616.aiAimDuration = i617[15]
  i616.fireDuration = i617[16]
  i616.flightDuration = i617[17]
  i616.flightMaxDuration = i617[18]
  i616.resolveDuration = i617[19]
  i616.victoryDuration = i617[20]
  i616.victoryLoserHold = i617[21]
  i616.defeatDuration = i617[22]
  i616.victoryFlightCap = i617[23]
  var i621 = i617[24]
  var i620 = []
  for(var i = 0; i < i621.length; i += 1) {
    i620.push( request.d('CastleDestroyers.Core.AiShotPreset', i621[i + 0]) );
  }
  i616.aiPresets = i620
  i616.aiPresetOrder = i617[25]
  i616.aiAngleNoiseDeg = i617[26]
  i616.aiPowerNoise = i617[27]
  i616.aiDecoyDeg = i617[28]
  i616.aiDecoyShare = i617[29]
  i616.aiTurnSpeed = i617[30]
  i616.handStartDelay = i617[31]
  i616.handDemoDuration = i617[32]
  i616.handDemoRepeats = i617[33]
  i616.handRepeatDelay = i617[34]
  i616.hintPanDuration = i617[35]
  i616.handStartViewport = new pc.Vec2( i617[36], i617[37] )
  i616.boosterHighlightPulse = i617[38]
  i616.boosterHighlightSpeed = i617[39]
  i616.hudFlashTime = i617[40]
  i616.hudShakeAmplitude = i617[41]
  i616.hudShakeTime = i617[42]
  i616.hudShakeFrequency = i617[43]
  i616.bannerDuration = i617[44]
  i616.uiPopTime = i617[45]
  i616.endCardFadeTime = i617[46]
  i616.endCardDimAlpha = i617[47]
  i616.hudLandscapeScale = i617[48]
  i616.endCardTapDelay = i617[49]
  i616.restartHitRadius = i617[50]
  i616.ctaPulse = i617[51]
  i616.ctaPulseSpeed = i617[52]
  i616.victoryJumpHeight = i617[53]
  i616.victoryJumpSpeed = i617[54]
  i616.victoryJumpPhase = i617[55]
  i616.fireworkBursts = i617[56]
  i616.fireworkInterval = i617[57]
  i616.fireworkSparks = i617[58]
  i616.fireworkSpeed = i617[59]
  i616.fireworkSparkSize = i617[60]
  i616.fireworkLife = i617[61]
  i616.fireworkGravity = i617[62]
  i616.fireworkLift = i617[63]
  i616.fireworkSpread = i617[64]
  i616.aimMinDeg = i617[65]
  i616.aimMaxDeg = i617[66]
  i616.aimMaxDragScreenFraction = i617[67]
  i616.aimDeadZone = i617[68]
  i616.aimTurnSpeed = i617[69]
  i616.aimPreviewFraction = i617[70]
  i616.aimDotCount = i617[71]
  i616.aimDotSizeStart = i617[72]
  i616.aimDotSizeEnd = i617[73]
  i616.aimDotAlphaEnd = i617[74]
  i616.aimPanelOffset = new pc.Vec2( i617[75], i617[76] )
  i616.aimPanelHudGap = i617[77]
  i616.aimPointAtFinger = !!i617[78]
  i616.aimPointPower = i617[79]
  i616.launchSpeedMin = i617[80]
  i616.launchSpeedMax = i617[81]
  i616.gravity = i617[82]
  i616.playerCannons = i617[83]
  i616.enemyCannons = i617[84]
  i616.rocketsPerCannon = i617[85]
  i616.rocketInterval = i617[86]
  i616.cannonHitRadius = i617[87]
  i616.cannonSupportRadius = i617[88]
  i616.cannonSupportShare = i617[89]
  i616.cannonDropSpeed = i617[90]
  i616.maxActiveRockets = i617[91]
  i616.stressRocketCount = i617[92]
  i616.rocketLength = i617[93]
  i616.rocketWidth = i617[94]
  i616.rocketCollisionStep = i617[95]
  i616.rocketSplashWidth = i617[96]
  i616.recoilDistance = i617[97]
  i616.cannonFireSquash = i617[98]
  i616.cannonDetachSquash = i617[99]
  i616.cannonDetachSquashTime = i617[100]
  i616.cannonJumpSquash = i617[101]
  i616.cannonSquashLengthShare = i617[102]
  i616.recoilReturnSpeed = i617[103]
  i616.muzzleFlashSize = i617[104]
  i616.muzzleFlashLife = i617[105]
  i616.stressRocketAngleDeg = i617[106]
  i616.stressRocketSpeed = i617[107]
  i616.stressSpawnPerFrame = i617[108]
  var i623 = i617[109]
  var i622 = []
  for(var i = 0; i < i623.length; i += 1) {
    i622.push( request.d('CastleDestroyers.Core.BoosterSpec', i623[i + 0]) );
  }
  i616.boosters = i622
  i616.boosterWidth = i617[110]
  i616.boosterHeight = i617[111]
  i616.boosterNudgeReach = i617[112]
  i616.boosterNudgeInset = i617[113]
  i616.boosterNudgeScreenMargin = i617[114]
  i616.boosterSpreadDeg = i617[115]
  i616.boosterSpawnStagger = i617[116]
  i616.boosterStreams = i617[117]
  i616.boosterExitFocus = i617[118]
  i616.boosterMaxCraterScale = i617[119]
  i616.boosterPulse = i617[120]
  i616.boosterPulseSpeed = i617[121]
  i616.boosterSquash = i617[122]
  i616.boosterSquashDecay = i617[123]
  i616.boosterPuffCount = i617[124]
  i616.boosterPuffInterval = i617[125]
  i616.boosterPuffSize = i617[126]
  i616.boosterPuffSpeed = i617[127]
  i616.boosterPuffLife = i617[128]
  i616.boosterPopupHeight = i617[129]
  i616.boosterPopupOffsetY = i617[130]
  i616.boosterPopupHold = i617[131]
  i616.boosterPopupFade = i617[132]
  i616.boosterPopupRise = i617[133]
  request.r(i617[134], i617[135], 0, i616, 'terrainMask')
  i616.terrainMaskOrigin = new pc.Vec2( i617[136], i617[137] )
  i616.maskTexelsPerMeter = i617[138]
  i616.maskTileSize = i617[139]
  i616.maxTileUploadsPerFrame = i617[140]
  i616.craterRadiusGround = i617[141]
  i616.rimWidth = i617[142]
  i616.rimScallop = !!i617[143]
  i616.rimScallopTeeth = i617[144]
  i616.rimScallopDepth = i617[145]
  i616.maskEdgeSoftness = i617[146]
  i616.connectivityInterval = i617[147]
  i616.connectivityCellTexels = i617[148]
  i616.minChunkCells = i617[149]
  i616.maxIslandsPerSolve = i617[150]
  i616.chunkSlots = i617[151]
  i616.chunkTextureSize = i617[152]
  i616.chunkGravity = i617[153]
  i616.chunkMaxDrift = i617[154]
  i616.chunkMaxSpinDeg = i617[155]
  i616.decorMaxSpinDeg = i617[156]
  i616.decorAnchorDepth = i617[157]
  i616.waterWaveAmplitude = i617[158]
  i616.waterWaveFrequency = i617[159]
  i616.waterWaveSpeed = i617[160]
  i616.waterWaveStep = i617[161]
  i616.waterRimThickness = i617[162]
  i616.fxParticleCapacity = i617[163]
  i616.craterDustPuffs = i617[164]
  i616.dustMaxPuffs = i617[165]
  i616.dustSize = i617[166]
  i616.dustSpeed = i617[167]
  i616.dustLife = i617[168]
  i616.dustGravity = i617[169]
  i616.splashDrops = i617[170]
  i616.splashDropsPerMeter = i617[171]
  i616.splashDropSize = i617[172]
  i616.splashSpeed = i617[173]
  i616.splashSpread = i617[174]
  i616.splashLife = i617[175]
  i616.splashGravity = i617[176]
  i616.explosionPoolSize = i617[177]
  i616.explosionMergeRadius = i617[178]
  i616.explosionMergeGrowth = i617[179]
  i616.explosionMaxStrength = i617[180]
  i616.explosionRadius = i617[181]
  i616.explosionLife = i617[182]
  i616.explosionShards = i617[183]
  i616.explosionShardSpeed = i617[184]
  i616.explosionShardSize = i617[185]
  i616.explosionShardLife = i617[186]
  i616.shakeMax = i617[187]
  i616.shakeDecay = i617[188]
  i616.shakePerExplosion = i617[189]
  i616.shakeFrequency = i617[190]
  i616.smokeCapacity = i617[191]
  i616.smokeLodRocketThreshold = i617[192]
  i616.smokeLodGrowth = i617[193]
  i616.smokeInterval = i617[194]
  i616.smokeLife = i617[195]
  i616.smokeSizeStart = i617[196]
  i616.smokeSizeEnd = i617[197]
  i616.smokeFlameFraction = i617[198]
  i616.smokeAlpha = i617[199]
  i616.castleConnectivityCellTexels = i617[200]
  i616.castleLoadTexels = i617[201]
  i616.castleMinChunkCells = i617[202]
  i616.craterRadiusCastle = i617[203]
  i616.castleRimWidth = i617[204]
  i616.castleRimDarken = i617[205]
  i616.castleCraterTeethMin = i617[206]
  i616.castleCraterTeethMax = i617[207]
  i616.castleCraterNoise = i617[208]
  i616.castleCrackChance = i617[209]
  i616.castleCrackCount = i617[210]
  i616.castleCrackLength = i617[211]
  i616.castleCrackWidth = i617[212]
  i616.castleCrackSegments = i617[213]
  i616.castleCrackBendDeg = i617[214]
  i616.castleDustPuffs = i617[215]
  i616.castleChunkSlots = i617[216]
  i616.castleChunkTextureSize = i617[217]
  i616.castleUploadInterval = i617[218]
  i616.platformHitsPerCrack = i617[219]
  i616.castleMaxTiltDeg = i617[220]
  i616.trackMaxTiltDeg = i617[221]
  i616.trackTiltSpeed = i617[222]
  i616.castleSettleSpeed = i617[223]
  i616.swayMaxDeg = i617[224]
  i616.swayFrequency = i617[225]
  i616.swayDamping = i617[226]
  i616.swayImpulseDeg = i617[227]
  i616.swayMaxSpeedDeg = i617[228]
  i616.swayBlastReach = i617[229]
  i616.cannonDetachSpeed = i617[230]
  i616.cannonDetachLift = i617[231]
  i616.cannonDetachSpinDeg = i617[232]
  i616.cannonSplashWidth = i617[233]
  i616.levelWidth = i617[234]
  i616.waterLevel = i617[235]
  i616.levelBottom = i617[236]
  i616.playerCastleX = i617[237]
  i616.enemyCastleX = i617[238]
  i616.trackHeight = i617[239]
  i616.platformHeight = i617[240]
  i616.parallaxFactors = i617[241]
  i616.backBandBase = i617[242]
  i616.moonPosition = new pc.Vec2( i617[243], i617[244] )
  i616.cloudStep = i617[245]
  i616.soundVolume = i617[246]
  i616.soundCooldown = i617[247]
  i616.cameraSmoothTime = i617[248]
  i616.castleFrameWidth = i617[249]
  i616.centerFrameWidth = i617[250]
  i616.centerFrameMinHeight = i617[251]
  i616.centerFrameMaxHeight = i617[252]
  i616.cameraFlightTopMargin = i617[253]
  i616.cameraCenterHold = i617[254]
  i616.targetFrameWidth = i617[255]
  i616.minFrameHeight = i617[256]
  i616.castleLeadX = i617[257]
  i616.castleFocusLiftY = i617[258]
  i616.centerFocus = new pc.Vec2( i617[259], i617[260] )
  i616.cameraHeadFollow = i617[261]
  i616.rainEnabled = !!i617[262]
  i616.rainPerSecond = i617[263]
  i616.rainSpeed = i617[264]
  i616.rainDropSize = i617[265]
  i616.lightBeamEnabled = !!i617[266]
  i616.showPersistentCta = !!i617[267]
  i616.debugSlowScale = i617[268]
  i616.debugHudRefresh = i617[269]
  i616.debugBlastRadii = i617[270]
  i616.debugBlastHoldInterval = i617[271]
  i616.debugBlastUiBand = i617[272]
  i616.debugToggleBand = i617[273]
  i616.debugToggleAlpha = i617[274]
  i616.debugPanelAlpha = i617[275]
  i616.debugOverviewWidth = i617[276]
  var i625 = i617[277]
  var i624 = []
  for(var i = 0; i < i625.length; i += 1) {
    i624.push( request.d('CastleDestroyers.Core.TurnHint', i625[i + 0]) );
  }
  i616.debugBoosterShots = i624
  i616.debugBoosterShotSalvos = i617[278]
  return i616
}

Deserializers["CastleDestroyers.Core.TurnHint"] = function (request, data, root) {
  var i628 = root || request.c( 'CastleDestroyers.Core.TurnHint' )
  var i629 = data
  i628.label = i629[0]
  i628.highlightBoosterMultiplier = i629[1]
  i628.showHand = !!i629[2]
  i628.handAngleDeg = i629[3]
  i628.handPower01 = i629[4]
  i628.panToBooster = !!i629[5]
  return i628
}

Deserializers["CastleDestroyers.Core.AiShotPreset"] = function (request, data, root) {
  var i632 = root || request.c( 'CastleDestroyers.Core.AiShotPreset' )
  var i633 = data
  i632.label = i633[0]
  i632.angleDeg = i633[1]
  i632.power01 = i633[2]
  return i632
}

Deserializers["CastleDestroyers.Core.BoosterSpec"] = function (request, data, root) {
  var i636 = root || request.c( 'CastleDestroyers.Core.BoosterSpec' )
  var i637 = data
  i636.multiplier = i637[0]
  i636.position = new pc.Vec2( i637[1], i637[2] )
  return i636
}

Deserializers["CastleDestroyers.Core.ThemePalette"] = function (request, data, root) {
  var i638 = root || request.c( 'CastleDestroyers.Core.ThemePalette' )
  var i639 = data
  i638.preset = i639[0]
  i638.night = request.d('CastleDestroyers.Core.PaletteSet', i639[1], i638.night)
  i638.sunset = request.d('CastleDestroyers.Core.PaletteSet', i639[2], i638.sunset)
  i638.nightArt = request.d('CastleDestroyers.Core.ThemeArt', i639[3], i638.nightArt)
  i638.sunsetArt = request.d('CastleDestroyers.Core.ThemeArt', i639[4], i638.sunsetArt)
  return i638
}

Deserializers["CastleDestroyers.Core.PaletteSet"] = function (request, data, root) {
  var i640 = root || request.c( 'CastleDestroyers.Core.PaletteSet' )
  var i641 = data
  i640.sky = UnityEngine.Color32.ConstructColor(i641[0], i641[1], i641[2], i641[3])
  i640.skyHorizon = UnityEngine.Color32.ConstructColor(i641[4], i641[5], i641[6], i641[7])
  i640.backgroundHaze = UnityEngine.Color32.ConstructColor(i641[8], i641[9], i641[10], i641[11])
  i640.backgroundFar = UnityEngine.Color32.ConstructColor(i641[12], i641[13], i641[14], i641[15])
  i640.backgroundFarShade = UnityEngine.Color32.ConstructColor(i641[16], i641[17], i641[18], i641[19])
  i640.backgroundMid = UnityEngine.Color32.ConstructColor(i641[20], i641[21], i641[22], i641[23])
  i640.backgroundNear = UnityEngine.Color32.ConstructColor(i641[24], i641[25], i641[26], i641[27])
  i640.backgroundNearLight = UnityEngine.Color32.ConstructColor(i641[28], i641[29], i641[30], i641[31])
  i640.skyStar = UnityEngine.Color32.ConstructColor(i641[32], i641[33], i641[34], i641[35])
  i640.moon = UnityEngine.Color32.ConstructColor(i641[36], i641[37], i641[38], i641[39])
  i640.moonShade = UnityEngine.Color32.ConstructColor(i641[40], i641[41], i641[42], i641[43])
  i640.ground = UnityEngine.Color32.ConstructColor(i641[44], i641[45], i641[46], i641[47])
  i640.groundShade = UnityEngine.Color32.ConstructColor(i641[48], i641[49], i641[50], i641[51])
  i640.groundBuried = UnityEngine.Color32.ConstructColor(i641[52], i641[53], i641[54], i641[55])
  i640.groundBand = UnityEngine.Color32.ConstructColor(i641[56], i641[57], i641[58], i641[59])
  i640.groundRim = UnityEngine.Color32.ConstructColor(i641[60], i641[61], i641[62], i641[63])
  i640.groundRimLight = UnityEngine.Color32.ConstructColor(i641[64], i641[65], i641[66], i641[67])
  i640.groundCrater = UnityEngine.Color32.ConstructColor(i641[68], i641[69], i641[70], i641[71])
  i640.bush = UnityEngine.Color32.ConstructColor(i641[72], i641[73], i641[74], i641[75])
  i640.bushLight = UnityEngine.Color32.ConstructColor(i641[76], i641[77], i641[78], i641[79])
  i640.treeLight = UnityEngine.Color32.ConstructColor(i641[80], i641[81], i641[82], i641[83])
  i640.treeShade = UnityEngine.Color32.ConstructColor(i641[84], i641[85], i641[86], i641[87])
  i640.water = UnityEngine.Color32.ConstructColor(i641[88], i641[89], i641[90], i641[91])
  i640.waterHighlight = UnityEngine.Color32.ConstructColor(i641[92], i641[93], i641[94], i641[95])
  i640.waterRim = UnityEngine.Color32.ConstructColor(i641[96], i641[97], i641[98], i641[99])
  i640.castleStone = UnityEngine.Color32.ConstructColor(i641[100], i641[101], i641[102], i641[103])
  i640.castleStoneShade = UnityEngine.Color32.ConstructColor(i641[104], i641[105], i641[106], i641[107])
  i640.castleBrick = UnityEngine.Color32.ConstructColor(i641[108], i641[109], i641[110], i641[111])
  i640.castleCrack = UnityEngine.Color32.ConstructColor(i641[112], i641[113], i641[114], i641[115])
  i640.castleWindow = UnityEngine.Color32.ConstructColor(i641[116], i641[117], i641[118], i641[119])
  i640.castleWindowLit = UnityEngine.Color32.ConstructColor(i641[120], i641[121], i641[122], i641[123])
  i640.roofPlayer = UnityEngine.Color32.ConstructColor(i641[124], i641[125], i641[126], i641[127])
  i640.roofPlayerShade = UnityEngine.Color32.ConstructColor(i641[128], i641[129], i641[130], i641[131])
  i640.roofEnemy = UnityEngine.Color32.ConstructColor(i641[132], i641[133], i641[134], i641[135])
  i640.roofEnemyShade = UnityEngine.Color32.ConstructColor(i641[136], i641[137], i641[138], i641[139])
  i640.cannonPlayer = UnityEngine.Color32.ConstructColor(i641[140], i641[141], i641[142], i641[143])
  i640.cannonPlayerShade = UnityEngine.Color32.ConstructColor(i641[144], i641[145], i641[146], i641[147])
  i640.cannonEnemy = UnityEngine.Color32.ConstructColor(i641[148], i641[149], i641[150], i641[151])
  i640.cannonEnemyShade = UnityEngine.Color32.ConstructColor(i641[152], i641[153], i641[154], i641[155])
  i640.platform = UnityEngine.Color32.ConstructColor(i641[156], i641[157], i641[158], i641[159])
  i640.platformShade = UnityEngine.Color32.ConstructColor(i641[160], i641[161], i641[162], i641[163])
  i640.track = UnityEngine.Color32.ConstructColor(i641[164], i641[165], i641[166], i641[167])
  i640.trackWheel = UnityEngine.Color32.ConstructColor(i641[168], i641[169], i641[170], i641[171])
  i640.booster10 = UnityEngine.Color32.ConstructColor(i641[172], i641[173], i641[174], i641[175])
  i640.booster10Cap = UnityEngine.Color32.ConstructColor(i641[176], i641[177], i641[178], i641[179])
  i640.booster20 = UnityEngine.Color32.ConstructColor(i641[180], i641[181], i641[182], i641[183])
  i640.booster20Cap = UnityEngine.Color32.ConstructColor(i641[184], i641[185], i641[186], i641[187])
  i640.booster50 = UnityEngine.Color32.ConstructColor(i641[188], i641[189], i641[190], i641[191])
  i640.booster50Cap = UnityEngine.Color32.ConstructColor(i641[192], i641[193], i641[194], i641[195])
  i640.rocketBody = UnityEngine.Color32.ConstructColor(i641[196], i641[197], i641[198], i641[199])
  i640.rocketShade = UnityEngine.Color32.ConstructColor(i641[200], i641[201], i641[202], i641[203])
  i640.rocketPlayer = UnityEngine.Color32.ConstructColor(i641[204], i641[205], i641[206], i641[207])
  i640.rocketEnemy = UnityEngine.Color32.ConstructColor(i641[208], i641[209], i641[210], i641[211])
  i640.rocketTail = UnityEngine.Color32.ConstructColor(i641[212], i641[213], i641[214], i641[215])
  i640.smoke = UnityEngine.Color32.ConstructColor(i641[216], i641[217], i641[218], i641[219])
  i640.blastCore = UnityEngine.Color32.ConstructColor(i641[220], i641[221], i641[222], i641[223])
  i640.blastMid = UnityEngine.Color32.ConstructColor(i641[224], i641[225], i641[226], i641[227])
  i640.blastEdge = UnityEngine.Color32.ConstructColor(i641[228], i641[229], i641[230], i641[231])
  i640.hudPanel = UnityEngine.Color32.ConstructColor(i641[232], i641[233], i641[234], i641[235])
  i640.hudTrack = UnityEngine.Color32.ConstructColor(i641[236], i641[237], i641[238], i641[239])
  i640.hudHealth = UnityEngine.Color32.ConstructColor(i641[240], i641[241], i641[242], i641[243])
  i640.hudHealthShade = UnityEngine.Color32.ConstructColor(i641[244], i641[245], i641[246], i641[247])
  i640.hudGold = UnityEngine.Color32.ConstructColor(i641[248], i641[249], i641[250], i641[251])
  i640.hudGoldShade = UnityEngine.Color32.ConstructColor(i641[252], i641[253], i641[254], i641[255])
  i640.hudText = UnityEngine.Color32.ConstructColor(i641[256], i641[257], i641[258], i641[259])
  return i640
}

Deserializers["CastleDestroyers.Core.ThemeArt"] = function (request, data, root) {
  var i642 = root || request.c( 'CastleDestroyers.Core.ThemeArt' )
  var i643 = data
  request.r(i643[0], i643[1], 0, i642, 'atlas')
  request.r(i643[2], i643[3], 0, i642, 'castleBlue')
  request.r(i643[4], i643[5], 0, i642, 'castleRed')
  request.r(i643[6], i643[7], 0, i642, 'backFar')
  request.r(i643[8], i643[9], 0, i642, 'backMid')
  request.r(i643[10], i643[11], 0, i642, 'backNear')
  request.r(i643[12], i643[13], 0, i642, 'terrain')
  return i642
}

Deserializers["CastleDestroyers.Destruction.TerrainMaskData"] = function (request, data, root) {
  var i644 = root || request.c( 'CastleDestroyers.Destruction.TerrainMaskData' )
  var i645 = data
  i644.width = i645[0]
  i644.height = i645[1]
  i644.solidRuns = i645[2]
  i644.grassRuns = i645[3]
  i644.buriedRuns = i645[4]
  return i644
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Resources"] = function (request, data, root) {
  var i646 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Resources' )
  var i647 = data
  var i649 = i647[0]
  var i648 = []
  for(var i = 0; i < i649.length; i += 1) {
    i648.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Resources+File', i649[i + 0]) );
  }
  i646.files = i648
  i646.componentToPrefabIds = i647[1]
  return i646
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Resources+File"] = function (request, data, root) {
  var i652 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Resources+File' )
  var i653 = data
  i652.path = i653[0]
  request.r(i653[1], i653[2], 0, i652, 'unityObject')
  return i652
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings"] = function (request, data, root) {
  var i654 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings' )
  var i655 = data
  var i657 = i655[0]
  var i656 = []
  for(var i = 0; i < i657.length; i += 1) {
    i656.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+ScriptsExecutionOrder', i657[i + 0]) );
  }
  i654.scriptsExecutionOrder = i656
  var i659 = i655[1]
  var i658 = []
  for(var i = 0; i < i659.length; i += 1) {
    i658.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+SortingLayer', i659[i + 0]) );
  }
  i654.sortingLayers = i658
  var i661 = i655[2]
  var i660 = []
  for(var i = 0; i < i661.length; i += 1) {
    i660.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+CullingLayer', i661[i + 0]) );
  }
  i654.cullingLayers = i660
  i654.timeSettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+TimeSettings', i655[3], i654.timeSettings)
  i654.physicsSettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings', i655[4], i654.physicsSettings)
  i654.physics2DSettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings', i655[5], i654.physics2DSettings)
  i654.qualitySettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.QualitySettings', i655[6], i654.qualitySettings)
  i654.enableRealtimeShadows = !!i655[7]
  i654.enableAutoInstancing = !!i655[8]
  i654.enableStaticBatching = !!i655[9]
  i654.enableDynamicBatching = !!i655[10]
  i654.usePreservativeDynamicBatching = !!i655[11]
  i654.lightmapEncodingQuality = i655[12]
  i654.desiredColorSpace = i655[13]
  var i663 = i655[14]
  var i662 = []
  for(var i = 0; i < i663.length; i += 1) {
    i662.push( i663[i + 0] );
  }
  i654.allTags = i662
  return i654
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+ScriptsExecutionOrder"] = function (request, data, root) {
  var i666 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+ScriptsExecutionOrder' )
  var i667 = data
  i666.name = i667[0]
  i666.value = i667[1]
  return i666
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+SortingLayer"] = function (request, data, root) {
  var i670 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+SortingLayer' )
  var i671 = data
  i670.id = i671[0]
  i670.name = i671[1]
  i670.value = i671[2]
  return i670
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+CullingLayer"] = function (request, data, root) {
  var i674 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+CullingLayer' )
  var i675 = data
  i674.id = i675[0]
  i674.name = i675[1]
  return i674
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+TimeSettings"] = function (request, data, root) {
  var i676 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+TimeSettings' )
  var i677 = data
  i676.fixedDeltaTime = i677[0]
  i676.maximumDeltaTime = i677[1]
  i676.timeScale = i677[2]
  i676.maximumParticleTimestep = i677[3]
  return i676
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings"] = function (request, data, root) {
  var i678 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings' )
  var i679 = data
  i678.gravity = new pc.Vec3( i679[0], i679[1], i679[2] )
  i678.defaultSolverIterations = i679[3]
  i678.bounceThreshold = i679[4]
  i678.autoSyncTransforms = !!i679[5]
  i678.autoSimulation = !!i679[6]
  var i681 = i679[7]
  var i680 = []
  for(var i = 0; i < i681.length; i += 1) {
    i680.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings+CollisionMask', i681[i + 0]) );
  }
  i678.collisionMatrix = i680
  return i678
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings+CollisionMask"] = function (request, data, root) {
  var i684 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings+CollisionMask' )
  var i685 = data
  i684.enabled = !!i685[0]
  i684.layerId = i685[1]
  i684.otherLayerId = i685[2]
  return i684
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings"] = function (request, data, root) {
  var i686 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings' )
  var i687 = data
  request.r(i687[0], i687[1], 0, i686, 'material')
  i686.gravity = new pc.Vec2( i687[2], i687[3] )
  i686.positionIterations = i687[4]
  i686.velocityIterations = i687[5]
  i686.velocityThreshold = i687[6]
  i686.maxLinearCorrection = i687[7]
  i686.maxAngularCorrection = i687[8]
  i686.maxTranslationSpeed = i687[9]
  i686.maxRotationSpeed = i687[10]
  i686.baumgarteScale = i687[11]
  i686.baumgarteTOIScale = i687[12]
  i686.timeToSleep = i687[13]
  i686.linearSleepTolerance = i687[14]
  i686.angularSleepTolerance = i687[15]
  i686.defaultContactOffset = i687[16]
  i686.autoSimulation = !!i687[17]
  i686.queriesHitTriggers = !!i687[18]
  i686.queriesStartInColliders = !!i687[19]
  i686.callbacksOnDisable = !!i687[20]
  i686.reuseCollisionCallbacks = !!i687[21]
  i686.autoSyncTransforms = !!i687[22]
  var i689 = i687[23]
  var i688 = []
  for(var i = 0; i < i689.length; i += 1) {
    i688.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings+CollisionMask', i689[i + 0]) );
  }
  i686.collisionMatrix = i688
  return i686
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings+CollisionMask"] = function (request, data, root) {
  var i692 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings+CollisionMask' )
  var i693 = data
  i692.enabled = !!i693[0]
  i692.layerId = i693[1]
  i692.otherLayerId = i693[2]
  return i692
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.QualitySettings"] = function (request, data, root) {
  var i694 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.QualitySettings' )
  var i695 = data
  var i697 = i695[0]
  var i696 = []
  for(var i = 0; i < i697.length; i += 1) {
    i696.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.QualitySettings', i697[i + 0]) );
  }
  i694.qualityLevels = i696
  var i699 = i695[1]
  var i698 = []
  for(var i = 0; i < i699.length; i += 1) {
    i698.push( i699[i + 0] );
  }
  i694.names = i698
  i694.shadows = i695[2]
  i694.anisotropicFiltering = i695[3]
  i694.antiAliasing = i695[4]
  i694.lodBias = i695[5]
  i694.shadowCascades = i695[6]
  i694.shadowDistance = i695[7]
  i694.shadowmaskMode = i695[8]
  i694.shadowProjection = i695[9]
  i694.shadowResolution = i695[10]
  i694.softParticles = !!i695[11]
  i694.softVegetation = !!i695[12]
  i694.activeColorSpace = i695[13]
  i694.desiredColorSpace = i695[14]
  i694.masterTextureLimit = i695[15]
  i694.maxQueuedFrames = i695[16]
  i694.particleRaycastBudget = i695[17]
  i694.pixelLightCount = i695[18]
  i694.realtimeReflectionProbes = !!i695[19]
  i694.shadowCascade2Split = i695[20]
  i694.shadowCascade4Split = new pc.Vec3( i695[21], i695[22], i695[23] )
  i694.streamingMipmapsActive = !!i695[24]
  i694.vSyncCount = i695[25]
  i694.asyncUploadBufferSize = i695[26]
  i694.asyncUploadTimeSlice = i695[27]
  i694.billboardsFaceCameraPosition = !!i695[28]
  i694.shadowNearPlaneOffset = i695[29]
  i694.streamingMipmapsMemoryBudget = i695[30]
  i694.maximumLODLevel = i695[31]
  i694.streamingMipmapsAddAllCameras = !!i695[32]
  i694.streamingMipmapsMaxLevelReduction = i695[33]
  i694.streamingMipmapsRenderersPerFrame = i695[34]
  i694.resolutionScalingFixedDPIFactor = i695[35]
  i694.streamingMipmapsMaxFileIORequests = i695[36]
  i694.currentQualityLevel = i695[37]
  return i694
}

Deserializers["UnityEngine.Events.ArgumentCache"] = function (request, data, root) {
  var i702 = root || request.c( 'UnityEngine.Events.ArgumentCache' )
  var i703 = data
  request.r(i703[0], i703[1], 0, i702, 'm_ObjectArgument')
  i702.m_ObjectArgumentAssemblyTypeName = i703[2]
  i702.m_IntArgument = i703[3]
  i702.m_FloatArgument = i703[4]
  i702.m_StringArgument = i703[5]
  i702.m_BoolArgument = !!i703[6]
  return i702
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+RenderObjectsSettings"] = function (request, data, root) {
  var i704 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+RenderObjectsSettings' )
  var i705 = data
  i704.Event = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i705[0], i704.Event)
  i704.filterSettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+FilterSettings', i705[1], i704.filterSettings)
  i704.overrideMaterialId = i705[2]
  i704.overrideMaterialPassIndex = i705[3]
  i704.overrideShaderId = i705[4]
  i704.overrideShaderPassIndex = i705[5]
  i704.overrideMode = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i705[6], i704.overrideMode)
  i704.overrideDepthState = !!i705[7]
  i704.depthCompareFunction = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i705[8], i704.depthCompareFunction)
  i704.enableWrite = !!i705[9]
  i704.stencilSettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.StencilStateData', i705[10], i704.stencilSettings)
  i704.cameraSettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+CustomCameraSettings', i705[11], i704.cameraSettings)
  return i704
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.EnumDescription"] = function (request, data, root) {
  var i706 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.EnumDescription' )
  var i707 = data
  i706.Value = i707[0]
  return i706
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+FilterSettings"] = function (request, data, root) {
  var i708 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+FilterSettings' )
  var i709 = data
  i708.RenderQueueType = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i709[0], i708.RenderQueueType)
  i708.LayerMask = i709[1]
  var i711 = i709[2]
  var i710 = []
  for(var i = 0; i < i711.length; i += 1) {
    i710.push( i711[i + 0] );
  }
  i708.PassNames = i710
  return i708
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.StencilStateData"] = function (request, data, root) {
  var i712 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.StencilStateData' )
  var i713 = data
  i712.overrideStencilState = !!i713[0]
  i712.stencilReference = i713[1]
  i712.stencilCompareFunctionValue = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i713[2], i712.stencilCompareFunctionValue)
  i712.passOperationValue = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i713[3], i712.passOperationValue)
  i712.failOperationValue = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i713[4], i712.failOperationValue)
  i712.zFailOperationValue = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i713[5], i712.zFailOperationValue)
  return i712
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+CustomCameraSettings"] = function (request, data, root) {
  var i714 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+CustomCameraSettings' )
  var i715 = data
  i714.overrideCamera = !!i715[0]
  i714.restoreCamera = !!i715[1]
  i714.offset = new pc.Vec4( i715[2], i715[3], i715[4], i715[5] )
  i714.cameraFieldOfView = i715[6]
  return i714
}

Deserializers.fields = {"Luna.Unity.DTO.UnityEngine.Assets.Material":{"name":0,"shader":1,"renderQueue":3,"enableInstancing":4,"floatParameters":5,"colorParameters":6,"vectorParameters":7,"textureParameters":8,"materialFlags":9},"Luna.Unity.DTO.UnityEngine.Assets.Material+FloatParameter":{"name":0,"value":1},"Luna.Unity.DTO.UnityEngine.Assets.Material+ColorParameter":{"name":0,"value":1},"Luna.Unity.DTO.UnityEngine.Assets.Material+VectorParameter":{"name":0,"value":1},"Luna.Unity.DTO.UnityEngine.Assets.Material+TextureParameter":{"name":0,"value":1},"Luna.Unity.DTO.UnityEngine.Assets.Material+MaterialFlag":{"name":0,"enabled":1},"Luna.Unity.DTO.UnityEngine.Textures.Texture2D":{"name":0,"width":1,"height":2,"mipmapCount":3,"anisoLevel":4,"filterMode":5,"hdr":6,"format":7,"wrapMode":8,"alphaIsTransparency":9,"alphaSource":10,"graphicsFormat":11,"sRGBTexture":12,"desiredColorSpace":13,"wrapU":14,"wrapV":15},"Luna.Unity.DTO.UnityEngine.Scene.Scene":{"name":0,"index":1,"startup":2},"Luna.Unity.DTO.UnityEngine.Components.Camera":{"aspect":0,"orthographic":1,"orthographicSize":2,"backgroundColor":3,"nearClipPlane":7,"farClipPlane":8,"fieldOfView":9,"depth":10,"clearFlags":11,"cullingMask":12,"rect":13,"targetTexture":14,"usePhysicalProperties":16,"focalLength":17,"sensorSize":18,"lensShift":20,"gateFit":22,"commandBufferCount":23,"cameraType":24,"enabled":25},"Luna.Unity.DTO.UnityEngine.Scene.GameObject":{"name":0,"tagId":1,"enabled":2,"isStatic":3,"layer":4},"Luna.Unity.DTO.UnityEngine.Components.MeshFilter":{"sharedMesh":0},"Luna.Unity.DTO.UnityEngine.Components.MeshRenderer":{"additionalVertexStreams":0,"enabled":2,"sharedMaterial":3,"sharedMaterials":5,"receiveShadows":6,"shadowCastingMode":7,"sortingLayerID":8,"sortingOrder":9,"lightmapIndex":10,"lightmapSceneIndex":11,"lightmapScaleOffset":12,"lightProbeUsage":16,"reflectionProbeUsage":17},"Luna.Unity.DTO.UnityEngine.Components.AudioSource":{"clip":0,"outputAudioMixerGroup":2,"playOnAwake":4,"loop":5,"time":6,"volume":7,"pitch":8,"enabled":9},"Luna.Unity.DTO.UnityEngine.Components.RectTransform":{"pivot":0,"anchorMin":2,"anchorMax":4,"sizeDelta":6,"anchoredPosition3D":8,"rotation":11,"scale":15},"Luna.Unity.DTO.UnityEngine.Components.Canvas":{"planeDistance":0,"referencePixelsPerUnit":1,"isFallbackOverlay":2,"renderMode":3,"renderOrder":4,"sortingLayerName":5,"sortingOrder":6,"scaleFactor":7,"worldCamera":8,"overrideSorting":10,"pixelPerfect":11,"targetDisplay":12,"overridePixelPerfect":13,"enabled":14},"Luna.Unity.DTO.UnityEngine.Components.CanvasRenderer":{"cullTransparentMesh":0},"Luna.Unity.DTO.UnityEngine.Assets.RenderSettings":{"ambientIntensity":0,"reflectionIntensity":1,"ambientMode":2,"ambientLight":3,"ambientSkyColor":7,"ambientGroundColor":11,"ambientEquatorColor":15,"fogColor":19,"fogEndDistance":23,"fogStartDistance":24,"fogDensity":25,"fog":26,"skybox":27,"fogMode":29,"lightmaps":30,"lightProbes":31,"lightmapsMode":32,"mixedBakeMode":33,"environmentLightingMode":34,"ambientProbe":35,"customReflection":36,"defaultReflection":38,"defaultReflectionMode":40,"defaultReflectionResolution":41,"sunLightObjectId":42,"pixelLightCount":43,"defaultReflectionHDR":44,"hasLightDataAsset":45,"hasManualGenerate":46},"Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+Lightmap":{"lightmapColor":0,"lightmapDirection":2,"shadowMask":4},"Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+LightProbes":{"bakedProbes":0,"positions":1,"hullRays":2,"tetrahedra":3,"neighbours":4,"matrices":5},"Luna.Unity.DTO.UnityEngine.Assets.UniversalRenderPipelineAsset":{"AdditionalLightsRenderingMode":0,"LightRenderingMode":1,"MainLightRenderingModeValue":2,"SupportsMainLightShadows":3,"MixedLightingSupported":4,"MainLightShadowmapResolutionValue":5,"SupportsSoftShadows":6,"SoftShadowQualityValue":7,"ShadowDistance":8,"ShadowCascadeCount":9,"Cascade2Split":10,"Cascade3Split":11,"Cascade4Split":13,"CascadeBorder":16,"ShadowDepthBias":17,"ShadowNormalBias":18,"RequireDepthTexture":19,"RequireOpaqueTexture":20,"scriptableRendererData":21},"Luna.Unity.DTO.UnityEngine.Assets.LightRenderingMode":{"Disabled":0,"PerVertex":1,"PerPixel":2},"Luna.Unity.DTO.UnityEngine.Assets.ScriptableRendererData":{"opaqueLayerMask":0,"transparentLayerMask":1,"RenderObjectsFeatures":2,"name":3},"Luna.Unity.DTO.UnityEngine.Assets.RenderObjects":{"settings":0,"name":1,"typeName":2},"Luna.Unity.DTO.UnityEngine.Assets.Shader":{"ShaderCompilationErrors":0,"name":1,"guid":2,"shaderDefinedKeywords":3,"passes":4,"usePasses":5,"defaultParameterValues":6,"unityFallbackShader":7,"readDepth":9,"hasDepthOnlyPass":10,"isCreatedByShaderGraph":11,"disableBatching":12,"compiled":13},"Luna.Unity.DTO.UnityEngine.Assets.Shader+ShaderCompilationError":{"shaderName":0,"errorMessage":1},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass":{"id":0,"subShaderIndex":1,"name":2,"passType":3,"grabPassTextureName":4,"usePass":5,"zTest":6,"zWrite":7,"culling":8,"blending":9,"alphaBlending":10,"colorWriteMask":11,"offsetUnits":12,"offsetFactor":13,"stencilRef":14,"stencilReadMask":15,"stencilWriteMask":16,"stencilOp":17,"stencilOpFront":18,"stencilOpBack":19,"tags":20,"passDefinedKeywords":21,"passDefinedKeywordGroups":22,"variants":23,"excludedVariants":24,"hasDepthReader":25},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value":{"val":0,"name":1},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Blending":{"src":0,"dst":1,"op":2},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+StencilOp":{"pass":0,"fail":1,"zFail":2,"comp":3},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Tag":{"name":0,"value":1},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+KeywordGroup":{"keywords":0,"hasDiscard":1},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Variant":{"passId":0,"subShaderIndex":1,"keywords":2,"vertexProgram":3,"fragmentProgram":4,"exportedForWebGl2":5,"readDepth":6},"Luna.Unity.DTO.UnityEngine.Assets.Shader+UsePass":{"shader":0,"pass":2},"Luna.Unity.DTO.UnityEngine.Assets.Shader+DefaultParameterValue":{"name":0,"type":1,"value":2,"textureValue":6,"shaderPropertyFlag":7},"Luna.Unity.DTO.UnityEngine.Textures.Sprite":{"name":0,"texture":1,"aabb":3,"vertices":4,"triangles":5,"textureRect":6,"packedRect":10,"border":14,"transparency":18,"bounds":19,"pixelsPerUnit":20,"textureWidth":21,"textureHeight":22,"nativeSize":23,"pivot":25,"textureRectOffset":27},"Luna.Unity.DTO.UnityEngine.Assets.AudioClip":{"name":0},"Luna.Unity.DTO.UnityEngine.Assets.Font":{"name":0,"ascent":1,"originalLineHeight":2,"fontSize":3,"characterInfo":4,"texture":5,"originalFontSize":7},"Luna.Unity.DTO.UnityEngine.Assets.Font+CharacterInfo":{"index":0,"advance":1,"bearing":2,"glyphWidth":3,"glyphHeight":4,"minX":5,"maxX":6,"minY":7,"maxY":8,"uvBottomLeftX":9,"uvBottomLeftY":10,"uvBottomRightX":11,"uvBottomRightY":12,"uvTopLeftX":13,"uvTopLeftY":14,"uvTopRightX":15,"uvTopRightY":16},"Luna.Unity.DTO.UnityEngine.Assets.Resources":{"files":0,"componentToPrefabIds":1},"Luna.Unity.DTO.UnityEngine.Assets.Resources+File":{"path":0,"unityObject":1},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings":{"scriptsExecutionOrder":0,"sortingLayers":1,"cullingLayers":2,"timeSettings":3,"physicsSettings":4,"physics2DSettings":5,"qualitySettings":6,"enableRealtimeShadows":7,"enableAutoInstancing":8,"enableStaticBatching":9,"enableDynamicBatching":10,"usePreservativeDynamicBatching":11,"lightmapEncodingQuality":12,"desiredColorSpace":13,"allTags":14},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+ScriptsExecutionOrder":{"name":0,"value":1},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+SortingLayer":{"id":0,"name":1,"value":2},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+CullingLayer":{"id":0,"name":1},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+TimeSettings":{"fixedDeltaTime":0,"maximumDeltaTime":1,"timeScale":2,"maximumParticleTimestep":3},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings":{"gravity":0,"defaultSolverIterations":3,"bounceThreshold":4,"autoSyncTransforms":5,"autoSimulation":6,"collisionMatrix":7},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings+CollisionMask":{"enabled":0,"layerId":1,"otherLayerId":2},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings":{"material":0,"gravity":2,"positionIterations":4,"velocityIterations":5,"velocityThreshold":6,"maxLinearCorrection":7,"maxAngularCorrection":8,"maxTranslationSpeed":9,"maxRotationSpeed":10,"baumgarteScale":11,"baumgarteTOIScale":12,"timeToSleep":13,"linearSleepTolerance":14,"angularSleepTolerance":15,"defaultContactOffset":16,"autoSimulation":17,"queriesHitTriggers":18,"queriesStartInColliders":19,"callbacksOnDisable":20,"reuseCollisionCallbacks":21,"autoSyncTransforms":22,"collisionMatrix":23},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings+CollisionMask":{"enabled":0,"layerId":1,"otherLayerId":2},"Luna.Unity.DTO.UnityEngine.Assets.QualitySettings":{"qualityLevels":0,"names":1,"shadows":2,"anisotropicFiltering":3,"antiAliasing":4,"lodBias":5,"shadowCascades":6,"shadowDistance":7,"shadowmaskMode":8,"shadowProjection":9,"shadowResolution":10,"softParticles":11,"softVegetation":12,"activeColorSpace":13,"desiredColorSpace":14,"masterTextureLimit":15,"maxQueuedFrames":16,"particleRaycastBudget":17,"pixelLightCount":18,"realtimeReflectionProbes":19,"shadowCascade2Split":20,"shadowCascade4Split":21,"streamingMipmapsActive":24,"vSyncCount":25,"asyncUploadBufferSize":26,"asyncUploadTimeSlice":27,"billboardsFaceCameraPosition":28,"shadowNearPlaneOffset":29,"streamingMipmapsMemoryBudget":30,"maximumLODLevel":31,"streamingMipmapsAddAllCameras":32,"streamingMipmapsMaxLevelReduction":33,"streamingMipmapsRenderersPerFrame":34,"resolutionScalingFixedDPIFactor":35,"streamingMipmapsMaxFileIORequests":36,"currentQualityLevel":37},"Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+RenderObjectsSettings":{"Event":0,"filterSettings":1,"overrideMaterialId":2,"overrideMaterialPassIndex":3,"overrideShaderId":4,"overrideShaderPassIndex":5,"overrideMode":6,"overrideDepthState":7,"depthCompareFunction":8,"enableWrite":9,"stencilSettings":10,"cameraSettings":11},"Luna.Unity.DTO.UnityEngine.Assets.EnumDescription":{"Value":0},"Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+FilterSettings":{"RenderQueueType":0,"LayerMask":1,"PassNames":2},"Luna.Unity.DTO.UnityEngine.Assets.StencilStateData":{"overrideStencilState":0,"stencilReference":1,"stencilCompareFunctionValue":2,"passOperationValue":3,"failOperationValue":4,"zFailOperationValue":5},"Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+CustomCameraSettings":{"overrideCamera":0,"restoreCamera":1,"offset":2,"cameraFieldOfView":6}}

Deserializers.requiredComponents = {"32":[33],"34":[33],"35":[33],"36":[33],"37":[33],"38":[33],"39":[40],"41":[2],"42":[43],"44":[43],"45":[43],"46":[43],"47":[43],"48":[43],"49":[50],"51":[50],"52":[50],"53":[50],"54":[50],"55":[50],"56":[50],"57":[50],"58":[50],"59":[50],"60":[50],"61":[50],"62":[50],"63":[2],"64":[7],"65":[66],"67":[66],"20":[11],"68":[27],"69":[20],"70":[11],"71":[2],"4":[2],"72":[73],"74":[11],"75":[23,11],"76":[7],"77":[23,11],"78":[11],"79":[11],"80":[7,11],"81":[11,23],"82":[11],"83":[11],"25":[20],"12":[23,11],"84":[11],"22":[20],"85":[11],"29":[11],"86":[11],"87":[11],"88":[11],"89":[11],"90":[11],"91":[11],"92":[11],"93":[23,11],"94":[11],"95":[11],"96":[11],"97":[11],"13":[23,11],"98":[11],"99":[27],"100":[27],"28":[27],"101":[27],"102":[2],"103":[2]}

Deserializers.types = ["UnityEngine.Shader","UnityEngine.Texture2D","UnityEngine.Camera","UnityEngine.MonoBehaviour","UnityEngine.Rendering.Universal.UniversalAdditionalCameraData","UnityEngine.AudioListener","UnityEngine.MeshFilter","UnityEngine.MeshRenderer","UnityEngine.Material","CastleDestroyers.Core.GameBootstrap","CastleDestroyers.Core.GameConfig","UnityEngine.RectTransform","UnityEngine.UI.Image","UnityEngine.UI.Text","UnityEngine.GameObject","UnityEngine.Sprite","UnityEngine.AudioClip","UnityEngine.AudioSource","UnityEngine.Transform","CastleDestroyers.DebugTools.DebugOverlay","UnityEngine.Canvas","UnityEngine.EventSystems.UIBehaviour","UnityEngine.UI.CanvasScaler","UnityEngine.CanvasRenderer","UnityEngine.Font","UnityEngine.UI.GraphicRaycaster","UnityEngine.UI.Button","UnityEngine.EventSystems.EventSystem","UnityEngine.EventSystems.StandaloneInputModule","UnityEngine.UI.GridLayoutGroup","CastleDestroyers.Core.ThemePalette","CastleDestroyers.Destruction.TerrainMaskData","UnityEngine.AudioLowPassFilter","UnityEngine.AudioBehaviour","UnityEngine.AudioHighPassFilter","UnityEngine.AudioReverbFilter","UnityEngine.AudioDistortionFilter","UnityEngine.AudioEchoFilter","UnityEngine.AudioChorusFilter","UnityEngine.Cloth","UnityEngine.SkinnedMeshRenderer","UnityEngine.FlareLayer","UnityEngine.CharacterJoint","UnityEngine.Rigidbody","UnityEngine.ConfigurableJoint","UnityEngine.ConstantForce","UnityEngine.FixedJoint","UnityEngine.HingeJoint","UnityEngine.SpringJoint","UnityEngine.CompositeCollider2D","UnityEngine.Rigidbody2D","UnityEngine.Joint2D","UnityEngine.AnchoredJoint2D","UnityEngine.SpringJoint2D","UnityEngine.DistanceJoint2D","UnityEngine.FrictionJoint2D","UnityEngine.HingeJoint2D","UnityEngine.RelativeJoint2D","UnityEngine.SliderJoint2D","UnityEngine.TargetJoint2D","UnityEngine.FixedJoint2D","UnityEngine.WheelJoint2D","UnityEngine.ConstantForce2D","UnityEngine.StreamingController","UnityEngine.TextMesh","UnityEngine.Tilemaps.TilemapRenderer","UnityEngine.Tilemaps.Tilemap","UnityEngine.Tilemaps.TilemapCollider2D","UnityEngine.InputSystem.UI.InputSystemUIInputModule","UnityEngine.InputSystem.UI.TrackedDeviceRaycaster","UnityEngine.Rendering.UI.UIFoldout","UnityEngine.Rendering.Universal.PixelPerfectCamera","UnityEngine.Rendering.Universal.UniversalAdditionalLightData","UnityEngine.Light","TMPro.TMP_Dropdown","TMPro.TMP_SelectionCaret","TMPro.TMP_SubMesh","TMPro.TMP_SubMeshUI","TMPro.TMP_Text","TMPro.TextContainer","TMPro.TextMeshPro","TMPro.TextMeshProUGUI","UnityEngine.UI.Dropdown","UnityEngine.UI.Graphic","UnityEngine.UI.AspectRatioFitter","UnityEngine.UI.ContentSizeFitter","UnityEngine.UI.HorizontalLayoutGroup","UnityEngine.UI.HorizontalOrVerticalLayoutGroup","UnityEngine.UI.LayoutElement","UnityEngine.UI.LayoutGroup","UnityEngine.UI.VerticalLayoutGroup","UnityEngine.UI.Mask","UnityEngine.UI.MaskableGraphic","UnityEngine.UI.RawImage","UnityEngine.UI.RectMask2D","UnityEngine.UI.ScrollRect","UnityEngine.UI.Scrollbar","UnityEngine.UI.Slider","UnityEngine.UI.Toggle","UnityEngine.EventSystems.BaseInputModule","UnityEngine.EventSystems.PointerInputModule","UnityEngine.EventSystems.TouchInputModule","UnityEngine.EventSystems.Physics2DRaycaster","UnityEngine.EventSystems.PhysicsRaycaster"]

Deserializers.unityVersion = "6000.0.85f1";

Deserializers.productName = "castle-destroyers-playable";

Deserializers.lunaInitializationTime = "";

Deserializers.lunaDaysRunning = "1.4";

Deserializers.lunaVersion = "7.2.0";

Deserializers.lunaSHA = "ea08d29afe2968efcb8d91d5624f033c6485cc68";

Deserializers.creativeName = "";

Deserializers.lunaAppID = "0";

Deserializers.projectId = "be284490fffa27a4285aa984f25b4867";

Deserializers.packagesInfo = "com.unity.inputsystem: 1.20.0\ncom.unity.ugui: 2.0.0\ncom.unity.render-pipelines.universal: 17.0.4";

Deserializers.externalJsLibraries = "";

Deserializers.androidLink = ( typeof window !== "undefined")&&window.$environment.packageConfig.androidLink?window.$environment.packageConfig.androidLink:'Empty';

Deserializers.iosLink = ( typeof window !== "undefined")&&window.$environment.packageConfig.iosLink?window.$environment.packageConfig.iosLink:'Empty';

Deserializers.base64Enabled = "False";

Deserializers.minifyEnabled = "True";

Deserializers.isForceUncompressed = "False";

Deserializers.isAntiAliasingEnabled = "False";

Deserializers.isRuntimeAnalysisEnabledForCode = "False";

Deserializers.runtimeAnalysisExcludedClassesCount = "0";

Deserializers.runtimeAnalysisExcludedMethodsCount = "0";

Deserializers.runtimeAnalysisExcludedModules = "";

Deserializers.isRuntimeAnalysisEnabledForShaders = "True";

Deserializers.isRealtimeShadowsEnabled = "False";

Deserializers.isLunaCompilerV2Used = "False";

Deserializers.companyName = "DefaultCompany";

Deserializers.buildPlatform = "StandaloneWindows64";

Deserializers.applicationIdentifier = "com.DefaultCompany.2D-Project";

Deserializers.disableAntiAliasing = true;

Deserializers.graphicsConstraint = 24;

Deserializers.linearColorSpace = false;

Deserializers.buildID = "b20a833e-f1af-4ca6-9a0b-656a33495709";

Deserializers.runtimeInitializeOnLoadInfos = [[["UnityEngine","Rendering","DebugUpdater","RuntimeInit"],["Unity","PerformanceTesting","PerformanceTest","ResetStaticsOnLoad"],["Unity","Burst","BurstCompiler","ResetStaticsOnLoad"],["UnityEngine","Experimental","Rendering","ScriptableRuntimeReflectionSystemSettings","ScriptingDirtyReflectionSystemInstance"]],[["Unity","Pipeline","Console","ConsoleLogCapture","RuntimeBootstrap"],["Unity","Pipeline","RuntimePipelineBootstrap","RuntimeBootstrap"],["UnityEngine","InputSystem","PlayerInput","InitializeGlobalPlayerState"],["UnityEngine","InputSystem","InputSystem","RuntimeInitialize"],["UnityEngine","InputSystem","InputActionState","InitializeGlobalActionState"],["UnityEngine","AI","NavMesh","ClearPreUpdateListeners"]],[["$BurstDirectCallInitializer","Initialize"],["$BurstDirectCallInitializer","Initialize"],["$BurstDirectCallInitializer","Initialize"],["$BurstDirectCallInitializer","Initialize"]],[["UnityEngine","Experimental","Rendering","XRSystem","XRSystemInit"]],[["Unity","PerformanceTesting","Data","RunSettings","ResetStaticsOnLoad"],["Unity","PerformanceTesting","PlayerCallbacks","ResetStaticsOnLoad"],["UnityEngine","InputSystem","Plugins","InputForUI","InputSystemProvider","Bootstrap"],["UnityEngine","InputSystem","EnhancedTouch","Touch","InitializeGlobalTouchState"],["UnityEngine","InputSystem","Users","InputUser","InitializeGlobalUserState"],["UnityEngine","InputSystem","UI","InputSystemUIInputModule","ResetDefaultActions"]]];

Deserializers.typeNameToIdMap = function(){ var i = 0; return Deserializers.types.reduce( function( res, item ) { res[ item ] = i++; return res; }, {} ) }()

