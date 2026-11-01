var Deserializers = {}
Deserializers["UnityEngine.JointSpring"] = function (request, data, root) {
  var i362 = root || request.c( 'UnityEngine.JointSpring' )
  var i363 = data
  i362.spring = i363[0]
  i362.damper = i363[1]
  i362.targetPosition = i363[2]
  return i362
}

Deserializers["UnityEngine.JointMotor"] = function (request, data, root) {
  var i364 = root || request.c( 'UnityEngine.JointMotor' )
  var i365 = data
  i364.m_TargetVelocity = i365[0]
  i364.m_Force = i365[1]
  i364.m_FreeSpin = i365[2]
  return i364
}

Deserializers["UnityEngine.JointLimits"] = function (request, data, root) {
  var i366 = root || request.c( 'UnityEngine.JointLimits' )
  var i367 = data
  i366.m_Min = i367[0]
  i366.m_Max = i367[1]
  i366.m_Bounciness = i367[2]
  i366.m_BounceMinVelocity = i367[3]
  i366.m_ContactDistance = i367[4]
  i366.minBounce = i367[5]
  i366.maxBounce = i367[6]
  return i366
}

Deserializers["UnityEngine.JointDrive"] = function (request, data, root) {
  var i368 = root || request.c( 'UnityEngine.JointDrive' )
  var i369 = data
  i368.m_PositionSpring = i369[0]
  i368.m_PositionDamper = i369[1]
  i368.m_MaximumForce = i369[2]
  i368.m_UseAcceleration = i369[3]
  return i368
}

Deserializers["UnityEngine.SoftJointLimitSpring"] = function (request, data, root) {
  var i370 = root || request.c( 'UnityEngine.SoftJointLimitSpring' )
  var i371 = data
  i370.m_Spring = i371[0]
  i370.m_Damper = i371[1]
  return i370
}

Deserializers["UnityEngine.SoftJointLimit"] = function (request, data, root) {
  var i372 = root || request.c( 'UnityEngine.SoftJointLimit' )
  var i373 = data
  i372.m_Limit = i373[0]
  i372.m_Bounciness = i373[1]
  i372.m_ContactDistance = i373[2]
  return i372
}

Deserializers["UnityEngine.WheelFrictionCurve"] = function (request, data, root) {
  var i374 = root || request.c( 'UnityEngine.WheelFrictionCurve' )
  var i375 = data
  i374.m_ExtremumSlip = i375[0]
  i374.m_ExtremumValue = i375[1]
  i374.m_AsymptoteSlip = i375[2]
  i374.m_AsymptoteValue = i375[3]
  i374.m_Stiffness = i375[4]
  return i374
}

Deserializers["UnityEngine.JointAngleLimits2D"] = function (request, data, root) {
  var i376 = root || request.c( 'UnityEngine.JointAngleLimits2D' )
  var i377 = data
  i376.m_LowerAngle = i377[0]
  i376.m_UpperAngle = i377[1]
  return i376
}

Deserializers["UnityEngine.JointMotor2D"] = function (request, data, root) {
  var i378 = root || request.c( 'UnityEngine.JointMotor2D' )
  var i379 = data
  i378.m_MotorSpeed = i379[0]
  i378.m_MaximumMotorTorque = i379[1]
  return i378
}

Deserializers["UnityEngine.JointSuspension2D"] = function (request, data, root) {
  var i380 = root || request.c( 'UnityEngine.JointSuspension2D' )
  var i381 = data
  i380.m_DampingRatio = i381[0]
  i380.m_Frequency = i381[1]
  i380.m_Angle = i381[2]
  return i380
}

Deserializers["UnityEngine.JointTranslationLimits2D"] = function (request, data, root) {
  var i382 = root || request.c( 'UnityEngine.JointTranslationLimits2D' )
  var i383 = data
  i382.m_LowerTranslation = i383[0]
  i382.m_UpperTranslation = i383[1]
  return i382
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Material"] = function (request, data, root) {
  var i384 = root || new pc.UnityMaterial()
  var i385 = data
  i384.name = i385[0]
  request.r(i385[1], i385[2], 0, i384, 'shader')
  i384.renderQueue = i385[3]
  i384.enableInstancing = !!i385[4]
  var i387 = i385[5]
  var i386 = []
  for(var i = 0; i < i387.length; i += 1) {
    i386.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Material+FloatParameter', i387[i + 0]) );
  }
  i384.floatParameters = i386
  var i389 = i385[6]
  var i388 = []
  for(var i = 0; i < i389.length; i += 1) {
    i388.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Material+ColorParameter', i389[i + 0]) );
  }
  i384.colorParameters = i388
  var i391 = i385[7]
  var i390 = []
  for(var i = 0; i < i391.length; i += 1) {
    i390.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Material+VectorParameter', i391[i + 0]) );
  }
  i384.vectorParameters = i390
  var i393 = i385[8]
  var i392 = []
  for(var i = 0; i < i393.length; i += 1) {
    i392.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Material+TextureParameter', i393[i + 0]) );
  }
  i384.textureParameters = i392
  var i395 = i385[9]
  var i394 = []
  for(var i = 0; i < i395.length; i += 1) {
    i394.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Material+MaterialFlag', i395[i + 0]) );
  }
  i384.materialFlags = i394
  return i384
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Material+FloatParameter"] = function (request, data, root) {
  var i398 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Material+FloatParameter' )
  var i399 = data
  i398.name = i399[0]
  i398.value = i399[1]
  return i398
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Material+ColorParameter"] = function (request, data, root) {
  var i402 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Material+ColorParameter' )
  var i403 = data
  i402.name = i403[0]
  i402.value = new pc.Color(i403[1], i403[2], i403[3], i403[4])
  return i402
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Material+VectorParameter"] = function (request, data, root) {
  var i406 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Material+VectorParameter' )
  var i407 = data
  i406.name = i407[0]
  i406.value = new pc.Vec4( i407[1], i407[2], i407[3], i407[4] )
  return i406
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Material+TextureParameter"] = function (request, data, root) {
  var i410 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Material+TextureParameter' )
  var i411 = data
  i410.name = i411[0]
  request.r(i411[1], i411[2], 0, i410, 'value')
  return i410
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Material+MaterialFlag"] = function (request, data, root) {
  var i414 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Material+MaterialFlag' )
  var i415 = data
  i414.name = i415[0]
  i414.enabled = !!i415[1]
  return i414
}

Deserializers["Luna.Unity.DTO.UnityEngine.Textures.Texture2D"] = function (request, data, root) {
  var i416 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Textures.Texture2D' )
  var i417 = data
  i416.name = i417[0]
  i416.width = i417[1]
  i416.height = i417[2]
  i416.mipmapCount = i417[3]
  i416.anisoLevel = i417[4]
  i416.filterMode = i417[5]
  i416.hdr = !!i417[6]
  i416.format = i417[7]
  i416.wrapMode = i417[8]
  i416.alphaIsTransparency = !!i417[9]
  i416.alphaSource = i417[10]
  i416.graphicsFormat = i417[11]
  i416.sRGBTexture = !!i417[12]
  i416.desiredColorSpace = i417[13]
  i416.wrapU = i417[14]
  i416.wrapV = i417[15]
  return i416
}

Deserializers["Luna.Unity.DTO.UnityEngine.Scene.Scene"] = function (request, data, root) {
  var i418 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Scene.Scene' )
  var i419 = data
  i418.name = i419[0]
  i418.index = i419[1]
  i418.startup = !!i419[2]
  return i418
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.Camera"] = function (request, data, root) {
  var i420 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.Camera' )
  var i421 = data
  i420.aspect = i421[0]
  i420.orthographic = !!i421[1]
  i420.orthographicSize = i421[2]
  i420.backgroundColor = new pc.Color(i421[3], i421[4], i421[5], i421[6])
  i420.nearClipPlane = i421[7]
  i420.farClipPlane = i421[8]
  i420.fieldOfView = i421[9]
  i420.depth = i421[10]
  i420.clearFlags = i421[11]
  i420.cullingMask = i421[12]
  i420.rect = i421[13]
  request.r(i421[14], i421[15], 0, i420, 'targetTexture')
  i420.usePhysicalProperties = !!i421[16]
  i420.focalLength = i421[17]
  i420.sensorSize = new pc.Vec2( i421[18], i421[19] )
  i420.lensShift = new pc.Vec2( i421[20], i421[21] )
  i420.gateFit = i421[22]
  i420.commandBufferCount = i421[23]
  i420.cameraType = i421[24]
  i420.enabled = !!i421[25]
  return i420
}

Deserializers["UnityEngine.Rendering.Universal.UniversalAdditionalCameraData"] = function (request, data, root) {
  var i422 = root || request.c( 'UnityEngine.Rendering.Universal.UniversalAdditionalCameraData' )
  var i423 = data
  i422.m_RenderShadows = !!i423[0]
  i422.m_RequiresDepthTextureOption = i423[1]
  i422.m_RequiresOpaqueTextureOption = i423[2]
  i422.m_CameraType = i423[3]
  var i425 = i423[4]
  var i424 = new (System.Collections.Generic.List$1(Bridge.ns('UnityEngine.Camera')))
  for(var i = 0; i < i425.length; i += 2) {
  request.r(i425[i + 0], i425[i + 1], 1, i424, '')
  }
  i422.m_Cameras = i424
  i422.m_RendererIndex = i423[5]
  i422.m_VolumeLayerMask = UnityEngine.LayerMask.FromIntegerValue( i423[6] )
  request.r(i423[7], i423[8], 0, i422, 'm_VolumeTrigger')
  i422.m_VolumeFrameworkUpdateModeOption = i423[9]
  i422.m_RenderPostProcessing = !!i423[10]
  i422.m_Antialiasing = i423[11]
  i422.m_AntialiasingQuality = i423[12]
  i422.m_StopNaN = !!i423[13]
  i422.m_Dithering = !!i423[14]
  i422.m_ClearDepth = !!i423[15]
  i422.m_AllowXRRendering = !!i423[16]
  i422.m_AllowHDROutput = !!i423[17]
  i422.m_UseScreenCoordOverride = !!i423[18]
  i422.m_ScreenSizeOverride = new pc.Vec4( i423[19], i423[20], i423[21], i423[22] )
  i422.m_ScreenCoordScaleBias = new pc.Vec4( i423[23], i423[24], i423[25], i423[26] )
  i422.m_RequiresDepthTexture = !!i423[27]
  i422.m_RequiresColorTexture = !!i423[28]
  i422.m_Version = i423[29]
  i422.m_TaaSettings = request.d('UnityEngine.Rendering.Universal.TemporalAA+Settings', i423[30], i422.m_TaaSettings)
  return i422
}

Deserializers["UnityEngine.Rendering.Universal.TemporalAA+Settings"] = function (request, data, root) {
  var i428 = root || request.c( 'UnityEngine.Rendering.Universal.TemporalAA+Settings' )
  var i429 = data
  i428.m_Quality = i429[0]
  i428.m_FrameInfluence = i429[1]
  i428.m_JitterScale = i429[2]
  i428.m_MipBias = i429[3]
  i428.m_VarianceClampScale = i429[4]
  i428.m_ContrastAdaptiveSharpening = i429[5]
  return i428
}

Deserializers["Luna.Unity.DTO.UnityEngine.Scene.GameObject"] = function (request, data, root) {
  var i430 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Scene.GameObject' )
  var i431 = data
  i430.name = i431[0]
  i430.tagId = i431[1]
  i430.enabled = !!i431[2]
  i430.isStatic = !!i431[3]
  i430.layer = i431[4]
  return i430
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.MeshFilter"] = function (request, data, root) {
  var i432 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.MeshFilter' )
  var i433 = data
  request.r(i433[0], i433[1], 0, i432, 'sharedMesh')
  return i432
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.MeshRenderer"] = function (request, data, root) {
  var i434 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.MeshRenderer' )
  var i435 = data
  request.r(i435[0], i435[1], 0, i434, 'additionalVertexStreams')
  i434.enabled = !!i435[2]
  request.r(i435[3], i435[4], 0, i434, 'sharedMaterial')
  var i437 = i435[5]
  var i436 = []
  for(var i = 0; i < i437.length; i += 2) {
  request.r(i437[i + 0], i437[i + 1], 2, i436, '')
  }
  i434.sharedMaterials = i436
  i434.receiveShadows = !!i435[6]
  i434.shadowCastingMode = i435[7]
  i434.sortingLayerID = i435[8]
  i434.sortingOrder = i435[9]
  i434.lightmapIndex = i435[10]
  i434.lightmapSceneIndex = i435[11]
  i434.lightmapScaleOffset = new pc.Vec4( i435[12], i435[13], i435[14], i435[15] )
  i434.lightProbeUsage = i435[16]
  i434.reflectionProbeUsage = i435[17]
  return i434
}

Deserializers["CastleDestroyers.Core.GameBootstrap"] = function (request, data, root) {
  var i440 = root || request.c( 'CastleDestroyers.Core.GameBootstrap' )
  var i441 = data
  request.r(i441[0], i441[1], 0, i440, 'config')
  request.r(i441[2], i441[3], 0, i440, 'mainCamera')
  var i443 = i441[4]
  var i442 = []
  for(var i = 0; i < i443.length; i += 2) {
  request.r(i443[i + 0], i443[i + 1], 2, i442, '')
  }
  i440.backFilters = i442
  request.r(i441[5], i441[6], 0, i440, 'decorFilter')
  request.r(i441[7], i441[8], 0, i440, 'frontFilter')
  request.r(i441[9], i441[10], 0, i440, 'waveFilter')
  request.r(i441[11], i441[12], 0, i440, 'cannonsFilter')
  var i445 = i441[13]
  var i444 = []
  for(var i = 0; i < i445.length; i += 2) {
  request.r(i445[i + 0], i445[i + 1], 2, i444, '')
  }
  i440.castleFilters = i444
  request.r(i441[14], i441[15], 0, i440, 'undercarriageFilter')
  request.r(i441[16], i441[17], 0, i440, 'particlesFilter')
  request.r(i441[18], i441[19], 0, i440, 'smokeFilter')
  request.r(i441[20], i441[21], 0, i440, 'rocketsFilter')
  request.r(i441[22], i441[23], 0, i440, 'boostersFilter')
  request.r(i441[24], i441[25], 0, i440, 'explosionsFilter')
  request.r(i441[26], i441[27], 0, i440, 'aimDotsFilter')
  request.r(i441[28], i441[29], 0, i440, 'aimPanel')
  request.r(i441[30], i441[31], 0, i440, 'aimPanelBackground')
  request.r(i441[32], i441[33], 0, i440, 'aimPowerText')
  request.r(i441[34], i441[35], 0, i440, 'aimAngleText')
  var i447 = i441[36]
  var i446 = []
  for(var i = 0; i < i447.length; i += 2) {
  request.r(i447[i + 0], i447[i + 1], 2, i446, '')
  }
  i440.aimCaptions = i446
  request.r(i441[37], i441[38], 0, i440, 'aimDegree')
  request.r(i441[39], i441[40], 0, i440, 'hudFilter')
  request.r(i441[41], i441[42], 0, i440, 'gameCanvas')
  request.r(i441[43], i441[44], 0, i440, 'bannerRoot')
  request.r(i441[45], i441[46], 0, i440, 'bannerBackground')
  request.r(i441[47], i441[48], 0, i440, 'bannerText')
  request.r(i441[49], i441[50], 0, i440, 'handRoot')
  var i449 = i441[51]
  var i448 = []
  for(var i = 0; i < i449.length; i += 2) {
  request.r(i449[i + 0], i449[i + 1], 2, i448, '')
  }
  i440.handImages = i448
  request.r(i441[52], i441[53], 0, i440, 'endCardRoot')
  request.r(i441[54], i441[55], 0, i440, 'endCardContent')
  request.r(i441[56], i441[57], 0, i440, 'endCardButton')
  request.r(i441[58], i441[59], 0, i440, 'endCardIcon')
  request.r(i441[60], i441[61], 0, i440, 'endCardCaption')
  request.r(i441[62], i441[63], 0, i440, 'endCardButtonFace')
  request.r(i441[64], i441[65], 0, i440, 'endCardButtonEdge')
  request.r(i441[66], i441[67], 0, i440, 'endCardButtonText')
  request.r(i441[68], i441[69], 0, i440, 'endCardRestart')
  request.r(i441[70], i441[71], 0, i440, 'endCardRestartDisc')
  request.r(i441[72], i441[73], 0, i440, 'endCardRestartIcon')
  request.r(i441[74], i441[75], 0, i440, 'endCardBlueCastle')
  request.r(i441[76], i441[77], 0, i440, 'endCardRedCastle')
  var i451 = i441[78]
  var i450 = []
  for(var i = 0; i < i451.length; i += 2) {
  request.r(i451[i + 0], i451[i + 1], 2, i450, '')
  }
  i440.endCardLayout = i450
  var i453 = i441[79]
  var i452 = []
  for(var i = 0; i < i453.length; i += 2) {
  request.r(i453[i + 0], i453[i + 1], 2, i452, '')
  }
  i440.endCardStars = i452
  request.r(i441[80], i441[81], 0, i440, 'endCardHand')
  var i455 = i441[82]
  var i454 = []
  for(var i = 0; i < i455.length; i += 2) {
  request.r(i455[i + 0], i455[i + 1], 2, i454, '')
  }
  i440.endCardHandImages = i454
  request.r(i441[83], i441[84], 0, i440, 'finaleFilter')
  var i457 = i441[85]
  var i456 = []
  for(var i = 0; i < i457.length; i += 2) {
  request.r(i457[i + 0], i457[i + 1], 2, i456, '')
  }
  i440.soundClips = i456
  var i459 = i441[86]
  var i458 = []
  for(var i = 0; i < i459.length; i += 2) {
  request.r(i459[i + 0], i459[i + 1], 2, i458, '')
  }
  i440.soundSources = i458
  request.r(i441[87], i441[88], 0, i440, 'terrainRoot')
  request.r(i441[89], i441[90], 0, i440, 'terrainMaterial')
  request.r(i441[91], i441[92], 0, i440, 'flatMaterial')
  request.r(i441[93], i441[94], 0, i440, 'rocketMaterial')
  request.r(i441[95], i441[96], 0, i440, 'castleMaterial')
  i440.terrainSortingOrder = i441[97]
  i440.chunkSortingOrder = i441[98]
  i440.castleChunkSortingOrder = i441[99]
  request.r(i441[100], i441[101], 0, i440, 'debugRoot')
  request.r(i441[102], i441[103], 0, i440, 'debugOverlay')
  return i440
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.AudioSource"] = function (request, data, root) {
  var i472 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.AudioSource' )
  var i473 = data
  request.r(i473[0], i473[1], 0, i472, 'clip')
  request.r(i473[2], i473[3], 0, i472, 'outputAudioMixerGroup')
  i472.playOnAwake = !!i473[4]
  i472.loop = !!i473[5]
  i472.time = i473[6]
  i472.volume = i473[7]
  i472.pitch = i473[8]
  i472.enabled = !!i473[9]
  return i472
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.RectTransform"] = function (request, data, root) {
  var i474 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.RectTransform' )
  var i475 = data
  i474.pivot = new pc.Vec2( i475[0], i475[1] )
  i474.anchorMin = new pc.Vec2( i475[2], i475[3] )
  i474.anchorMax = new pc.Vec2( i475[4], i475[5] )
  i474.sizeDelta = new pc.Vec2( i475[6], i475[7] )
  i474.anchoredPosition3D = new pc.Vec3( i475[8], i475[9], i475[10] )
  i474.rotation = new pc.Quat(i475[11], i475[12], i475[13], i475[14])
  i474.scale = new pc.Vec3( i475[15], i475[16], i475[17] )
  return i474
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.Canvas"] = function (request, data, root) {
  var i476 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.Canvas' )
  var i477 = data
  i476.planeDistance = i477[0]
  i476.referencePixelsPerUnit = i477[1]
  i476.isFallbackOverlay = !!i477[2]
  i476.renderMode = i477[3]
  i476.renderOrder = i477[4]
  i476.sortingLayerName = i477[5]
  i476.sortingOrder = i477[6]
  i476.scaleFactor = i477[7]
  request.r(i477[8], i477[9], 0, i476, 'worldCamera')
  i476.overrideSorting = !!i477[10]
  i476.pixelPerfect = !!i477[11]
  i476.targetDisplay = i477[12]
  i476.overridePixelPerfect = !!i477[13]
  i476.enabled = !!i477[14]
  return i476
}

Deserializers["UnityEngine.UI.CanvasScaler"] = function (request, data, root) {
  var i478 = root || request.c( 'UnityEngine.UI.CanvasScaler' )
  var i479 = data
  i478.m_UiScaleMode = i479[0]
  i478.m_ReferencePixelsPerUnit = i479[1]
  i478.m_ScaleFactor = i479[2]
  i478.m_ReferenceResolution = new pc.Vec2( i479[3], i479[4] )
  i478.m_ScreenMatchMode = i479[5]
  i478.m_MatchWidthOrHeight = i479[6]
  i478.m_PhysicalUnit = i479[7]
  i478.m_FallbackScreenDPI = i479[8]
  i478.m_DefaultSpriteDPI = i479[9]
  i478.m_DynamicPixelsPerUnit = i479[10]
  i478.m_PresetInfoIsWorld = !!i479[11]
  return i478
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.CanvasRenderer"] = function (request, data, root) {
  var i480 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.CanvasRenderer' )
  var i481 = data
  i480.cullTransparentMesh = !!i481[0]
  return i480
}

Deserializers["UnityEngine.UI.Image"] = function (request, data, root) {
  var i482 = root || request.c( 'UnityEngine.UI.Image' )
  var i483 = data
  request.r(i483[0], i483[1], 0, i482, 'm_Sprite')
  i482.m_Type = i483[2]
  i482.m_PreserveAspect = !!i483[3]
  i482.m_FillCenter = !!i483[4]
  i482.m_FillMethod = i483[5]
  i482.m_FillAmount = i483[6]
  i482.m_FillClockwise = !!i483[7]
  i482.m_FillOrigin = i483[8]
  i482.m_UseSpriteMesh = !!i483[9]
  i482.m_PixelsPerUnitMultiplier = i483[10]
  request.r(i483[11], i483[12], 0, i482, 'm_Material')
  i482.m_Maskable = !!i483[13]
  i482.m_Color = new pc.Color(i483[14], i483[15], i483[16], i483[17])
  i482.m_RaycastTarget = !!i483[18]
  i482.m_RaycastPadding = new pc.Vec4( i483[19], i483[20], i483[21], i483[22] )
  return i482
}

Deserializers["UnityEngine.UI.Text"] = function (request, data, root) {
  var i484 = root || request.c( 'UnityEngine.UI.Text' )
  var i485 = data
  i484.m_FontData = request.d('UnityEngine.UI.FontData', i485[0], i484.m_FontData)
  i484.m_Text = i485[1]
  request.r(i485[2], i485[3], 0, i484, 'm_Material')
  i484.m_Maskable = !!i485[4]
  i484.m_Color = new pc.Color(i485[5], i485[6], i485[7], i485[8])
  i484.m_RaycastTarget = !!i485[9]
  i484.m_RaycastPadding = new pc.Vec4( i485[10], i485[11], i485[12], i485[13] )
  return i484
}

Deserializers["UnityEngine.UI.FontData"] = function (request, data, root) {
  var i486 = root || request.c( 'UnityEngine.UI.FontData' )
  var i487 = data
  request.r(i487[0], i487[1], 0, i486, 'm_Font')
  i486.m_FontSize = i487[2]
  i486.m_FontStyle = i487[3]
  i486.m_BestFit = !!i487[4]
  i486.m_MinSize = i487[5]
  i486.m_MaxSize = i487[6]
  i486.m_Alignment = i487[7]
  i486.m_AlignByGeometry = !!i487[8]
  i486.m_RichText = !!i487[9]
  i486.m_HorizontalOverflow = i487[10]
  i486.m_VerticalOverflow = i487[11]
  i486.m_LineSpacing = i487[12]
  return i486
}

Deserializers["UnityEngine.UI.GraphicRaycaster"] = function (request, data, root) {
  var i488 = root || request.c( 'UnityEngine.UI.GraphicRaycaster' )
  var i489 = data
  i488.m_IgnoreReversedGraphics = !!i489[0]
  i488.m_BlockingObjects = i489[1]
  i488.m_BlockingMask = UnityEngine.LayerMask.FromIntegerValue( i489[2] )
  return i488
}

Deserializers["CastleDestroyers.DebugTools.DebugOverlay"] = function (request, data, root) {
  var i490 = root || request.c( 'CastleDestroyers.DebugTools.DebugOverlay' )
  var i491 = data
  request.r(i491[0], i491[1], 0, i490, 'canvasScaler')
  request.r(i491[2], i491[3], 0, i490, 'panelRoot')
  request.r(i491[4], i491[5], 0, i490, 'toggleButton')
  request.r(i491[6], i491[7], 0, i490, 'toggleLabel')
  request.r(i491[8], i491[9], 0, i490, 'statsText')
  request.r(i491[10], i491[11], 0, i490, 'statsBackground')
  var i493 = i491[12]
  var i492 = []
  for(var i = 0; i < i493.length; i += 2) {
  request.r(i493[i + 0], i493[i + 1], 2, i492, '')
  }
  i490.buttonImages = i492
  var i495 = i491[13]
  var i494 = []
  for(var i = 0; i < i495.length; i += 2) {
  request.r(i495[i + 0], i495[i + 1], 2, i494, '')
  }
  i490.buttonLabels = i494
  request.r(i491[14], i491[15], 0, i490, 'nextStateButton')
  request.r(i491[16], i491[17], 0, i490, 'slowButton')
  request.r(i491[18], i491[19], 0, i490, 'rocketsButton')
  request.r(i491[20], i491[21], 0, i490, 'themeButton')
  request.r(i491[22], i491[23], 0, i490, 'restartButton')
  request.r(i491[24], i491[25], 0, i490, 'blastButton')
  request.r(i491[26], i491[27], 0, i490, 'cameraButton')
  request.r(i491[28], i491[29], 0, i490, 'shotX20Button')
  request.r(i491[30], i491[31], 0, i490, 'shotX10Button')
  request.r(i491[32], i491[33], 0, i490, 'shotX50Button')
  request.r(i491[34], i491[35], 0, i490, 'blastLabel')
  request.r(i491[36], i491[37], 0, i490, 'cameraLabel')
  request.r(i491[38], i491[39], 0, i490, 'rocketsLabel')
  request.r(i491[40], i491[41], 0, i490, 'themeLabel')
  return i490
}

Deserializers["UnityEngine.EventSystems.EventSystem"] = function (request, data, root) {
  var i496 = root || request.c( 'UnityEngine.EventSystems.EventSystem' )
  var i497 = data
  request.r(i497[0], i497[1], 0, i496, 'm_FirstSelected')
  i496.m_sendNavigationEvents = !!i497[2]
  i496.m_DragThreshold = i497[3]
  return i496
}

Deserializers["UnityEngine.EventSystems.StandaloneInputModule"] = function (request, data, root) {
  var i498 = root || request.c( 'UnityEngine.EventSystems.StandaloneInputModule' )
  var i499 = data
  i498.m_HorizontalAxis = i499[0]
  i498.m_VerticalAxis = i499[1]
  i498.m_SubmitButton = i499[2]
  i498.m_CancelButton = i499[3]
  i498.m_InputActionsPerSecond = i499[4]
  i498.m_RepeatDelay = i499[5]
  i498.m_ForceModuleActive = !!i499[6]
  i498.m_SendPointerHoverToParent = !!i499[7]
  return i498
}

Deserializers["UnityEngine.UI.GridLayoutGroup"] = function (request, data, root) {
  var i500 = root || request.c( 'UnityEngine.UI.GridLayoutGroup' )
  var i501 = data
  i500.m_StartCorner = i501[0]
  i500.m_StartAxis = i501[1]
  i500.m_CellSize = new pc.Vec2( i501[2], i501[3] )
  i500.m_Spacing = new pc.Vec2( i501[4], i501[5] )
  i500.m_Constraint = i501[6]
  i500.m_ConstraintCount = i501[7]
  i500.m_Padding = UnityEngine.RectOffset.FromPaddings(i501[8], i501[9], i501[10], i501[11])
  i500.m_ChildAlignment = i501[12]
  return i500
}

Deserializers["UnityEngine.UI.Button"] = function (request, data, root) {
  var i502 = root || request.c( 'UnityEngine.UI.Button' )
  var i503 = data
  i502.m_OnClick = request.d('UnityEngine.UI.Button+ButtonClickedEvent', i503[0], i502.m_OnClick)
  i502.m_Navigation = request.d('UnityEngine.UI.Navigation', i503[1], i502.m_Navigation)
  i502.m_Transition = i503[2]
  i502.m_Colors = request.d('UnityEngine.UI.ColorBlock', i503[3], i502.m_Colors)
  i502.m_SpriteState = request.d('UnityEngine.UI.SpriteState', i503[4], i502.m_SpriteState)
  i502.m_AnimationTriggers = request.d('UnityEngine.UI.AnimationTriggers', i503[5], i502.m_AnimationTriggers)
  i502.m_Interactable = !!i503[6]
  request.r(i503[7], i503[8], 0, i502, 'm_TargetGraphic')
  return i502
}

Deserializers["UnityEngine.UI.Button+ButtonClickedEvent"] = function (request, data, root) {
  var i504 = root || request.c( 'UnityEngine.UI.Button+ButtonClickedEvent' )
  var i505 = data
  i504.m_PersistentCalls = request.d('UnityEngine.Events.PersistentCallGroup', i505[0], i504.m_PersistentCalls)
  return i504
}

Deserializers["UnityEngine.Events.PersistentCallGroup"] = function (request, data, root) {
  var i506 = root || request.c( 'UnityEngine.Events.PersistentCallGroup' )
  var i507 = data
  var i509 = i507[0]
  var i508 = new (System.Collections.Generic.List$1(Bridge.ns('UnityEngine.Events.PersistentCall')))
  for(var i = 0; i < i509.length; i += 1) {
    i508.add(request.d('UnityEngine.Events.PersistentCall', i509[i + 0]));
  }
  i506.m_Calls = i508
  return i506
}

Deserializers["UnityEngine.Events.PersistentCall"] = function (request, data, root) {
  var i512 = root || request.c( 'UnityEngine.Events.PersistentCall' )
  var i513 = data
  request.r(i513[0], i513[1], 0, i512, 'm_Target')
  i512.m_TargetAssemblyTypeName = i513[2]
  i512.m_MethodName = i513[3]
  i512.m_Mode = i513[4]
  i512.m_Arguments = request.d('UnityEngine.Events.ArgumentCache', i513[5], i512.m_Arguments)
  i512.m_CallState = i513[6]
  return i512
}

Deserializers["UnityEngine.UI.Navigation"] = function (request, data, root) {
  var i514 = root || request.c( 'UnityEngine.UI.Navigation' )
  var i515 = data
  i514.m_Mode = i515[0]
  i514.m_WrapAround = !!i515[1]
  request.r(i515[2], i515[3], 0, i514, 'm_SelectOnUp')
  request.r(i515[4], i515[5], 0, i514, 'm_SelectOnDown')
  request.r(i515[6], i515[7], 0, i514, 'm_SelectOnLeft')
  request.r(i515[8], i515[9], 0, i514, 'm_SelectOnRight')
  return i514
}

Deserializers["UnityEngine.UI.ColorBlock"] = function (request, data, root) {
  var i516 = root || request.c( 'UnityEngine.UI.ColorBlock' )
  var i517 = data
  i516.m_NormalColor = new pc.Color(i517[0], i517[1], i517[2], i517[3])
  i516.m_HighlightedColor = new pc.Color(i517[4], i517[5], i517[6], i517[7])
  i516.m_PressedColor = new pc.Color(i517[8], i517[9], i517[10], i517[11])
  i516.m_SelectedColor = new pc.Color(i517[12], i517[13], i517[14], i517[15])
  i516.m_DisabledColor = new pc.Color(i517[16], i517[17], i517[18], i517[19])
  i516.m_ColorMultiplier = i517[20]
  i516.m_FadeDuration = i517[21]
  return i516
}

Deserializers["UnityEngine.UI.SpriteState"] = function (request, data, root) {
  var i518 = root || request.c( 'UnityEngine.UI.SpriteState' )
  var i519 = data
  request.r(i519[0], i519[1], 0, i518, 'm_HighlightedSprite')
  request.r(i519[2], i519[3], 0, i518, 'm_PressedSprite')
  request.r(i519[4], i519[5], 0, i518, 'm_SelectedSprite')
  request.r(i519[6], i519[7], 0, i518, 'm_DisabledSprite')
  return i518
}

Deserializers["UnityEngine.UI.AnimationTriggers"] = function (request, data, root) {
  var i520 = root || request.c( 'UnityEngine.UI.AnimationTriggers' )
  var i521 = data
  i520.m_NormalTrigger = i521[0]
  i520.m_HighlightedTrigger = i521[1]
  i520.m_PressedTrigger = i521[2]
  i520.m_SelectedTrigger = i521[3]
  i520.m_DisabledTrigger = i521[4]
  return i520
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderSettings"] = function (request, data, root) {
  var i522 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.RenderSettings' )
  var i523 = data
  i522.ambientIntensity = i523[0]
  i522.reflectionIntensity = i523[1]
  i522.ambientMode = i523[2]
  i522.ambientLight = new pc.Color(i523[3], i523[4], i523[5], i523[6])
  i522.ambientSkyColor = new pc.Color(i523[7], i523[8], i523[9], i523[10])
  i522.ambientGroundColor = new pc.Color(i523[11], i523[12], i523[13], i523[14])
  i522.ambientEquatorColor = new pc.Color(i523[15], i523[16], i523[17], i523[18])
  i522.fogColor = new pc.Color(i523[19], i523[20], i523[21], i523[22])
  i522.fogEndDistance = i523[23]
  i522.fogStartDistance = i523[24]
  i522.fogDensity = i523[25]
  i522.fog = !!i523[26]
  request.r(i523[27], i523[28], 0, i522, 'skybox')
  i522.fogMode = i523[29]
  var i525 = i523[30]
  var i524 = []
  for(var i = 0; i < i525.length; i += 1) {
    i524.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+Lightmap', i525[i + 0]) );
  }
  i522.lightmaps = i524
  i522.lightProbes = request.d('Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+LightProbes', i523[31], i522.lightProbes)
  i522.lightmapsMode = i523[32]
  i522.mixedBakeMode = i523[33]
  i522.environmentLightingMode = i523[34]
  i522.ambientProbe = new pc.SphericalHarmonicsL2(i523[35])
  request.r(i523[36], i523[37], 0, i522, 'customReflection')
  request.r(i523[38], i523[39], 0, i522, 'defaultReflection')
  i522.defaultReflectionMode = i523[40]
  i522.defaultReflectionResolution = i523[41]
  i522.sunLightObjectId = i523[42]
  i522.pixelLightCount = i523[43]
  i522.defaultReflectionHDR = !!i523[44]
  i522.hasLightDataAsset = !!i523[45]
  i522.hasManualGenerate = !!i523[46]
  return i522
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+Lightmap"] = function (request, data, root) {
  var i528 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+Lightmap' )
  var i529 = data
  request.r(i529[0], i529[1], 0, i528, 'lightmapColor')
  request.r(i529[2], i529[3], 0, i528, 'lightmapDirection')
  request.r(i529[4], i529[5], 0, i528, 'shadowMask')
  return i528
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+LightProbes"] = function (request, data, root) {
  var i530 = root || new UnityEngine.LightProbes()
  var i531 = data
  return i530
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.UniversalRenderPipelineAsset"] = function (request, data, root) {
  var i538 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.UniversalRenderPipelineAsset' )
  var i539 = data
  i538.AdditionalLightsRenderingMode = i539[0]
  i538.LightRenderingMode = request.d('Luna.Unity.DTO.UnityEngine.Assets.LightRenderingMode', i539[1], i538.LightRenderingMode)
  i538.MainLightRenderingModeValue = i539[2]
  i538.SupportsMainLightShadows = !!i539[3]
  i538.MixedLightingSupported = !!i539[4]
  i538.MainLightShadowmapResolutionValue = i539[5]
  i538.SupportsSoftShadows = !!i539[6]
  i538.SoftShadowQualityValue = i539[7]
  i538.ShadowDistance = i539[8]
  i538.ShadowCascadeCount = i539[9]
  i538.Cascade2Split = i539[10]
  i538.Cascade3Split = new pc.Vec2( i539[11], i539[12] )
  i538.Cascade4Split = new pc.Vec3( i539[13], i539[14], i539[15] )
  i538.CascadeBorder = i539[16]
  i538.ShadowDepthBias = i539[17]
  i538.ShadowNormalBias = i539[18]
  i538.RequireDepthTexture = !!i539[19]
  i538.RequireOpaqueTexture = !!i539[20]
  i538.scriptableRendererData = request.d('Luna.Unity.DTO.UnityEngine.Assets.ScriptableRendererData', i539[21], i538.scriptableRendererData)
  return i538
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.LightRenderingMode"] = function (request, data, root) {
  var i540 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.LightRenderingMode' )
  var i541 = data
  i540.Disabled = i541[0]
  i540.PerVertex = i541[1]
  i540.PerPixel = i541[2]
  return i540
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ScriptableRendererData"] = function (request, data, root) {
  var i542 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ScriptableRendererData' )
  var i543 = data
  i542.opaqueLayerMask = i543[0]
  i542.transparentLayerMask = i543[1]
  var i545 = i543[2]
  var i544 = []
  for(var i = 0; i < i545.length; i += 1) {
    i544.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.RenderObjects', i545[i + 0]) );
  }
  i542.RenderObjectsFeatures = i544
  i542.name = i543[3]
  return i542
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderObjects"] = function (request, data, root) {
  var i548 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.RenderObjects' )
  var i549 = data
  i548.settings = request.d('Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+RenderObjectsSettings', i549[0], i548.settings)
  i548.name = i549[1]
  i548.typeName = i549[2]
  return i548
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader"] = function (request, data, root) {
  var i550 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader' )
  var i551 = data
  var i553 = i551[0]
  var i552 = new (System.Collections.Generic.List$1(Bridge.ns('Luna.Unity.DTO.UnityEngine.Assets.Shader+ShaderCompilationError')))
  for(var i = 0; i < i553.length; i += 1) {
    i552.add(request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+ShaderCompilationError', i553[i + 0]));
  }
  i550.ShaderCompilationErrors = i552
  i550.name = i551[1]
  i550.guid = i551[2]
  var i555 = i551[3]
  var i554 = []
  for(var i = 0; i < i555.length; i += 1) {
    i554.push( i555[i + 0] );
  }
  i550.shaderDefinedKeywords = i554
  var i557 = i551[4]
  var i556 = []
  for(var i = 0; i < i557.length; i += 1) {
    i556.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass', i557[i + 0]) );
  }
  i550.passes = i556
  var i559 = i551[5]
  var i558 = []
  for(var i = 0; i < i559.length; i += 1) {
    i558.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+UsePass', i559[i + 0]) );
  }
  i550.usePasses = i558
  var i561 = i551[6]
  var i560 = []
  for(var i = 0; i < i561.length; i += 1) {
    i560.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+DefaultParameterValue', i561[i + 0]) );
  }
  i550.defaultParameterValues = i560
  request.r(i551[7], i551[8], 0, i550, 'unityFallbackShader')
  i550.readDepth = !!i551[9]
  i550.hasDepthOnlyPass = !!i551[10]
  i550.isCreatedByShaderGraph = !!i551[11]
  i550.disableBatching = !!i551[12]
  i550.compiled = !!i551[13]
  return i550
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+ShaderCompilationError"] = function (request, data, root) {
  var i564 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+ShaderCompilationError' )
  var i565 = data
  i564.shaderName = i565[0]
  i564.errorMessage = i565[1]
  return i564
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass"] = function (request, data, root) {
  var i570 = root || new pc.UnityShaderPass()
  var i571 = data
  i570.id = i571[0]
  i570.subShaderIndex = i571[1]
  i570.name = i571[2]
  i570.passType = i571[3]
  i570.grabPassTextureName = i571[4]
  i570.usePass = !!i571[5]
  i570.zTest = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i571[6], i570.zTest)
  i570.zWrite = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i571[7], i570.zWrite)
  i570.culling = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i571[8], i570.culling)
  i570.blending = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Blending', i571[9], i570.blending)
  i570.alphaBlending = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Blending', i571[10], i570.alphaBlending)
  i570.colorWriteMask = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i571[11], i570.colorWriteMask)
  i570.offsetUnits = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i571[12], i570.offsetUnits)
  i570.offsetFactor = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i571[13], i570.offsetFactor)
  i570.stencilRef = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i571[14], i570.stencilRef)
  i570.stencilReadMask = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i571[15], i570.stencilReadMask)
  i570.stencilWriteMask = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i571[16], i570.stencilWriteMask)
  i570.stencilOp = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+StencilOp', i571[17], i570.stencilOp)
  i570.stencilOpFront = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+StencilOp', i571[18], i570.stencilOpFront)
  i570.stencilOpBack = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+StencilOp', i571[19], i570.stencilOpBack)
  var i573 = i571[20]
  var i572 = []
  for(var i = 0; i < i573.length; i += 1) {
    i572.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Tag', i573[i + 0]) );
  }
  i570.tags = i572
  var i575 = i571[21]
  var i574 = []
  for(var i = 0; i < i575.length; i += 1) {
    i574.push( i575[i + 0] );
  }
  i570.passDefinedKeywords = i574
  var i577 = i571[22]
  var i576 = []
  for(var i = 0; i < i577.length; i += 1) {
    i576.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+KeywordGroup', i577[i + 0]) );
  }
  i570.passDefinedKeywordGroups = i576
  var i579 = i571[23]
  var i578 = []
  for(var i = 0; i < i579.length; i += 1) {
    i578.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Variant', i579[i + 0]) );
  }
  i570.variants = i578
  var i581 = i571[24]
  var i580 = []
  for(var i = 0; i < i581.length; i += 1) {
    i580.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Variant', i581[i + 0]) );
  }
  i570.excludedVariants = i580
  i570.hasDepthReader = !!i571[25]
  return i570
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value"] = function (request, data, root) {
  var i582 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value' )
  var i583 = data
  i582.val = i583[0]
  i582.name = i583[1]
  return i582
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Blending"] = function (request, data, root) {
  var i584 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Blending' )
  var i585 = data
  i584.src = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i585[0], i584.src)
  i584.dst = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i585[1], i584.dst)
  i584.op = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i585[2], i584.op)
  return i584
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+StencilOp"] = function (request, data, root) {
  var i586 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+StencilOp' )
  var i587 = data
  i586.pass = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i587[0], i586.pass)
  i586.fail = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i587[1], i586.fail)
  i586.zFail = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i587[2], i586.zFail)
  i586.comp = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i587[3], i586.comp)
  return i586
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Tag"] = function (request, data, root) {
  var i590 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Tag' )
  var i591 = data
  i590.name = i591[0]
  i590.value = i591[1]
  return i590
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+KeywordGroup"] = function (request, data, root) {
  var i594 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+KeywordGroup' )
  var i595 = data
  var i597 = i595[0]
  var i596 = []
  for(var i = 0; i < i597.length; i += 1) {
    i596.push( i597[i + 0] );
  }
  i594.keywords = i596
  i594.hasDiscard = !!i595[1]
  return i594
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Variant"] = function (request, data, root) {
  var i600 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Variant' )
  var i601 = data
  i600.passId = i601[0]
  i600.subShaderIndex = i601[1]
  var i603 = i601[2]
  var i602 = []
  for(var i = 0; i < i603.length; i += 1) {
    i602.push( i603[i + 0] );
  }
  i600.keywords = i602
  i600.vertexProgram = i601[3]
  i600.fragmentProgram = i601[4]
  i600.exportedForWebGl2 = !!i601[5]
  i600.readDepth = !!i601[6]
  return i600
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+UsePass"] = function (request, data, root) {
  var i606 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+UsePass' )
  var i607 = data
  request.r(i607[0], i607[1], 0, i606, 'shader')
  i606.pass = i607[2]
  return i606
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+DefaultParameterValue"] = function (request, data, root) {
  var i610 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+DefaultParameterValue' )
  var i611 = data
  i610.name = i611[0]
  i610.type = i611[1]
  i610.value = new pc.Vec4( i611[2], i611[3], i611[4], i611[5] )
  i610.textureValue = i611[6]
  i610.shaderPropertyFlag = i611[7]
  return i610
}

Deserializers["Luna.Unity.DTO.UnityEngine.Textures.Sprite"] = function (request, data, root) {
  var i612 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Textures.Sprite' )
  var i613 = data
  i612.name = i613[0]
  request.r(i613[1], i613[2], 0, i612, 'texture')
  i612.aabb = i613[3]
  i612.vertices = i613[4]
  i612.triangles = i613[5]
  i612.textureRect = UnityEngine.Rect.MinMaxRect(i613[6], i613[7], i613[8], i613[9])
  i612.packedRect = UnityEngine.Rect.MinMaxRect(i613[10], i613[11], i613[12], i613[13])
  i612.border = new pc.Vec4( i613[14], i613[15], i613[16], i613[17] )
  i612.transparency = i613[18]
  i612.bounds = i613[19]
  i612.pixelsPerUnit = i613[20]
  i612.textureWidth = i613[21]
  i612.textureHeight = i613[22]
  i612.nativeSize = new pc.Vec2( i613[23], i613[24] )
  i612.pivot = new pc.Vec2( i613[25], i613[26] )
  i612.textureRectOffset = new pc.Vec2( i613[27], i613[28] )
  return i612
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.AudioClip"] = function (request, data, root) {
  var i614 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.AudioClip' )
  var i615 = data
  i614.name = i615[0]
  return i614
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Font"] = function (request, data, root) {
  var i616 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Font' )
  var i617 = data
  i616.name = i617[0]
  i616.ascent = i617[1]
  i616.originalLineHeight = i617[2]
  i616.fontSize = i617[3]
  var i619 = i617[4]
  var i618 = []
  for(var i = 0; i < i619.length; i += 1) {
    i618.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Font+CharacterInfo', i619[i + 0]) );
  }
  i616.characterInfo = i618
  request.r(i617[5], i617[6], 0, i616, 'texture')
  i616.originalFontSize = i617[7]
  return i616
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Font+CharacterInfo"] = function (request, data, root) {
  var i622 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Font+CharacterInfo' )
  var i623 = data
  i622.index = i623[0]
  i622.advance = i623[1]
  i622.bearing = i623[2]
  i622.glyphWidth = i623[3]
  i622.glyphHeight = i623[4]
  i622.minX = i623[5]
  i622.maxX = i623[6]
  i622.minY = i623[7]
  i622.maxY = i623[8]
  i622.uvBottomLeftX = i623[9]
  i622.uvBottomLeftY = i623[10]
  i622.uvBottomRightX = i623[11]
  i622.uvBottomRightY = i623[12]
  i622.uvTopLeftX = i623[13]
  i622.uvTopLeftY = i623[14]
  i622.uvTopRightX = i623[15]
  i622.uvTopRightY = i623[16]
  return i622
}

Deserializers["CastleDestroyers.Core.GameConfig"] = function (request, data, root) {
  var i624 = root || request.c( 'CastleDestroyers.Core.GameConfig' )
  var i625 = data
  request.r(i625[0], i625[1], 0, i624, 'theme')
  i624.rngSeed = i625[2]
  i624.maxPlayerTurns = i625[3]
  i624.maxSessionSeconds = i625[4]
  i624.maxDeltaTime = i625[5]
  i624.directorEnabled = !!i625[6]
  i624.directorMaxLossPerAiTurn = i625[7]
  i624.directorEnemyKeepCannons = i625[8]
  i624.directorEnemyKeepTurns = i625[9]
  i624.directorFinisherWeight = i625[10]
  i624.bonusPickEnabled = !!i625[11]
  var i627 = i625[12]
  var i626 = []
  for(var i = 0; i < i627.length; i += 1) {
    i626.push( request.d('CastleDestroyers.Core.TurnHint', i627[i + 0]) );
  }
  i624.turnHints = i626
  i624.bootDuration = i625[13]
  i624.introDuration = i625[14]
  i624.playerAimDuration = i625[15]
  i624.aiAimDuration = i625[16]
  i624.fireDuration = i625[17]
  i624.flightDuration = i625[18]
  i624.flightMaxDuration = i625[19]
  i624.resolveDuration = i625[20]
  i624.victoryDuration = i625[21]
  i624.victoryLoserHold = i625[22]
  i624.defeatDuration = i625[23]
  i624.victoryFlightCap = i625[24]
  var i629 = i625[25]
  var i628 = []
  for(var i = 0; i < i629.length; i += 1) {
    i628.push( request.d('CastleDestroyers.Core.AiShotPreset', i629[i + 0]) );
  }
  i624.aiPresets = i628
  i624.aiPresetOrder = i625[26]
  i624.aiAngleNoiseDeg = i625[27]
  i624.aiPowerNoise = i625[28]
  i624.aiDecoyDeg = i625[29]
  i624.aiDecoyShare = i625[30]
  i624.aiTurnSpeed = i625[31]
  i624.handStartDelay = i625[32]
  i624.handDemoDuration = i625[33]
  i624.handDemoRepeats = i625[34]
  i624.handRepeatDelay = i625[35]
  i624.hintPanDuration = i625[36]
  i624.handStartViewport = new pc.Vec2( i625[37], i625[38] )
  i624.boosterHighlightPulse = i625[39]
  i624.boosterHighlightSpeed = i625[40]
  i624.hudFlashTime = i625[41]
  i624.hudShakeAmplitude = i625[42]
  i624.hudShakeTime = i625[43]
  i624.hudShakeFrequency = i625[44]
  i624.bannerDuration = i625[45]
  i624.uiPopTime = i625[46]
  i624.endCardFadeTime = i625[47]
  i624.endCardDimAlpha = i625[48]
  i624.hudLandscapeScale = i625[49]
  i624.endCardTapDelay = i625[50]
  i624.restartHitRadius = i625[51]
  i624.ctaPulse = i625[52]
  i624.ctaPulseSpeed = i625[53]
  i624.storeUrl = i625[54]
  i624.finaleBlastDelay = i625[55]
  i624.finaleBlastPower = i625[56]
  i624.finaleBlastLife = i625[57]
  i624.finaleBlastLift = i625[58]
  i624.finaleCraterRadius = i625[59]
  i624.finaleCraterLift = i625[60]
  i624.finaleDustPuffs = i625[61]
  i624.finaleShake = i625[62]
  i624.finaleSlowScale = i625[63]
  i624.finaleSlowTime = i625[64]
  i624.finaleDebrisPieces = i625[65]
  i624.finaleDebrisSpeed = i625[66]
  i624.finaleDebrisSize = i625[67]
  i624.finaleDebrisLife = i625[68]
  i624.finaleDebrisGravity = i625[69]
  i624.finaleFlashTime = i625[70]
  i624.finaleRingRadius = i625[71]
  i624.finaleRingWidth = i625[72]
  i624.finaleRingLife = i625[73]
  i624.finaleRays = i625[74]
  i624.finaleRaySpeed = i625[75]
  i624.finaleRayLength = i625[76]
  i624.finaleRayGrowTime = i625[77]
  i624.finaleHaloRadius = i625[78]
  i624.finaleHaloRim = i625[79]
  i624.finaleHaloPulse = i625[80]
  i624.finaleHaloPulseSpeed = i625[81]
  i624.finaleConfettiCapacity = i625[82]
  i624.finaleCannonPieces = i625[83]
  i624.finaleStarPieces = i625[84]
  i624.finaleRainInterval = i625[85]
  i624.finaleConfettiGravity = i625[86]
  i624.finaleConfettiDrag = i625[87]
  i624.finaleConfettiFall = i625[88]
  i624.finaleConfettiSize = i625[89]
  i624.endSlamTime = i625[90]
  i624.endSlamFrom = i625[91]
  i624.endLogoWobbleDeg = i625[92]
  i624.endLogoWobbleSpeed = i625[93]
  i624.endCastleDelay = i625[94]
  i624.endCastleDrop = i625[95]
  i624.endCastleDropTime = i625[96]
  i624.endHopHeight = i625[97]
  i624.endHopSpeed = i625[98]
  i624.endHopSquash = i625[99]
  i624.endStarDelay = i625[100]
  i624.endStarStep = i625[101]
  i624.endStarPopTime = i625[102]
  i624.endStarSpinDeg = i625[103]
  i624.endStarPulse = i625[104]
  i624.endStarPulseSpeed = i625[105]
  i624.endStarPhase = i625[106]
  i624.endCountDelay = i625[107]
  i624.endCountTime = i625[108]
  i624.endCountPunch = i625[109]
  i624.endButtonDelay = i625[110]
  i624.endRestartDelay = i625[111]
  i624.endHandDelay = i625[112]
  i624.endHandBob = i625[113]
  i624.endHandSpeed = i625[114]
  i624.endImpactShake = i625[115]
  i624.endImpactDecay = i625[116]
  i624.endShakeFrequency = i625[117]
  i624.ctaWigglePeriod = i625[118]
  i624.ctaWiggleTime = i625[119]
  i624.ctaWiggleDeg = i625[120]
  i624.victoryJumpHeight = i625[121]
  i624.victoryJumpSpeed = i625[122]
  i624.victoryJumpPhase = i625[123]
  i624.fireworkBursts = i625[124]
  i624.fireworkInterval = i625[125]
  i624.fireworkSparks = i625[126]
  i624.fireworkSpeed = i625[127]
  i624.fireworkSparkSize = i625[128]
  i624.fireworkLife = i625[129]
  i624.fireworkGravity = i625[130]
  i624.fireworkLift = i625[131]
  i624.fireworkSpread = i625[132]
  i624.aimMinDeg = i625[133]
  i624.aimMaxDeg = i625[134]
  i624.aimMaxDragScreenFraction = i625[135]
  i624.aimDeadZone = i625[136]
  i624.aimTurnSpeed = i625[137]
  i624.aimPreviewFraction = i625[138]
  i624.aimDotCount = i625[139]
  i624.aimDotSizeStart = i625[140]
  i624.aimDotSizeEnd = i625[141]
  i624.aimDotAlphaEnd = i625[142]
  i624.aimPanelOffset = new pc.Vec2( i625[143], i625[144] )
  i624.aimPanelHudGap = i625[145]
  i624.aimPointAtFinger = !!i625[146]
  i624.aimPointPower = i625[147]
  i624.launchSpeedMin = i625[148]
  i624.launchSpeedMax = i625[149]
  i624.gravity = i625[150]
  i624.playerCannons = i625[151]
  i624.enemyCannons = i625[152]
  i624.rocketsPerCannon = i625[153]
  i624.rocketInterval = i625[154]
  i624.cannonHitRadius = i625[155]
  i624.cannonSupportRadius = i625[156]
  i624.cannonSupportShare = i625[157]
  i624.cannonDropSpeed = i625[158]
  i624.maxActiveRockets = i625[159]
  i624.stressRocketCount = i625[160]
  i624.rocketLength = i625[161]
  i624.rocketWidth = i625[162]
  i624.rocketCollisionStep = i625[163]
  i624.rocketSplashWidth = i625[164]
  i624.recoilDistance = i625[165]
  i624.cannonFireSquash = i625[166]
  i624.cannonDetachSquash = i625[167]
  i624.cannonDetachSquashTime = i625[168]
  i624.cannonJumpSquash = i625[169]
  i624.cannonSquashLengthShare = i625[170]
  i624.recoilReturnSpeed = i625[171]
  i624.muzzleFlashSize = i625[172]
  i624.muzzleFlashLife = i625[173]
  i624.stressRocketAngleDeg = i625[174]
  i624.stressRocketSpeed = i625[175]
  i624.stressSpawnPerFrame = i625[176]
  var i631 = i625[177]
  var i630 = []
  for(var i = 0; i < i631.length; i += 1) {
    i630.push( request.d('CastleDestroyers.Core.BoosterSpec', i631[i + 0]) );
  }
  i624.boosters = i630
  i624.boosterWidth = i625[178]
  i624.boosterHeight = i625[179]
  i624.boosterNudgeReach = i625[180]
  i624.boosterNudgeInset = i625[181]
  i624.boosterNudgeScreenMargin = i625[182]
  i624.boosterSpreadDeg = i625[183]
  i624.boosterSpawnStagger = i625[184]
  i624.boosterStreams = i625[185]
  i624.boosterExitFocus = i625[186]
  i624.boosterMaxCraterScale = i625[187]
  i624.boosterPulse = i625[188]
  i624.boosterPulseSpeed = i625[189]
  i624.boosterSquash = i625[190]
  i624.boosterSquashDecay = i625[191]
  i624.boosterPuffCount = i625[192]
  i624.boosterPuffInterval = i625[193]
  i624.boosterPuffSize = i625[194]
  i624.boosterPuffSpeed = i625[195]
  i624.boosterPuffLife = i625[196]
  i624.boosterPopupHeight = i625[197]
  i624.boosterPopupOffsetY = i625[198]
  i624.boosterPopupHold = i625[199]
  i624.boosterPopupFade = i625[200]
  i624.boosterPopupRise = i625[201]
  request.r(i625[202], i625[203], 0, i624, 'terrainMask')
  i624.terrainMaskOrigin = new pc.Vec2( i625[204], i625[205] )
  i624.maskTexelsPerMeter = i625[206]
  i624.maskTileSize = i625[207]
  i624.maxTileUploadsPerFrame = i625[208]
  i624.craterRadiusGround = i625[209]
  i624.rimWidth = i625[210]
  i624.rimScallop = !!i625[211]
  i624.rimScallopTeeth = i625[212]
  i624.rimScallopDepth = i625[213]
  i624.maskEdgeSoftness = i625[214]
  i624.connectivityInterval = i625[215]
  i624.connectivityCellTexels = i625[216]
  i624.minChunkCells = i625[217]
  i624.maxIslandsPerSolve = i625[218]
  i624.chunkSlots = i625[219]
  i624.chunkTextureSize = i625[220]
  i624.chunkGravity = i625[221]
  i624.chunkMaxDrift = i625[222]
  i624.chunkMaxSpinDeg = i625[223]
  i624.decorMaxSpinDeg = i625[224]
  i624.decorAnchorDepth = i625[225]
  i624.waterWaveAmplitude = i625[226]
  i624.waterWaveFrequency = i625[227]
  i624.waterWaveSpeed = i625[228]
  i624.waterWaveStep = i625[229]
  i624.waterRimThickness = i625[230]
  i624.fxParticleCapacity = i625[231]
  i624.craterDustPuffs = i625[232]
  i624.dustMaxPuffs = i625[233]
  i624.dustSize = i625[234]
  i624.dustSpeed = i625[235]
  i624.dustLife = i625[236]
  i624.dustGravity = i625[237]
  i624.splashDrops = i625[238]
  i624.splashDropsPerMeter = i625[239]
  i624.splashDropSize = i625[240]
  i624.splashSpeed = i625[241]
  i624.splashSpread = i625[242]
  i624.splashLife = i625[243]
  i624.splashGravity = i625[244]
  i624.explosionPoolSize = i625[245]
  i624.explosionMergeRadius = i625[246]
  i624.explosionMergeGrowth = i625[247]
  i624.explosionMaxStrength = i625[248]
  i624.explosionRadius = i625[249]
  i624.explosionLife = i625[250]
  i624.explosionShards = i625[251]
  i624.explosionShardSpeed = i625[252]
  i624.explosionShardSize = i625[253]
  i624.explosionShardLife = i625[254]
  i624.shakeMax = i625[255]
  i624.shakeDecay = i625[256]
  i624.shakePerExplosion = i625[257]
  i624.shakeFrequency = i625[258]
  i624.smokeCapacity = i625[259]
  i624.smokeLodRocketThreshold = i625[260]
  i624.smokeLodGrowth = i625[261]
  i624.smokeInterval = i625[262]
  i624.smokeLife = i625[263]
  i624.smokeSizeStart = i625[264]
  i624.smokeSizeEnd = i625[265]
  i624.smokeFlameFraction = i625[266]
  i624.smokeAlpha = i625[267]
  i624.castleConnectivityCellTexels = i625[268]
  i624.castleLoadTexels = i625[269]
  i624.castleMinChunkCells = i625[270]
  i624.craterRadiusCastle = i625[271]
  i624.castleRimWidth = i625[272]
  i624.castleRimDarken = i625[273]
  i624.castleCraterTeethMin = i625[274]
  i624.castleCraterTeethMax = i625[275]
  i624.castleCraterNoise = i625[276]
  i624.castleCrackChance = i625[277]
  i624.castleCrackCount = i625[278]
  i624.castleCrackLength = i625[279]
  i624.castleCrackWidth = i625[280]
  i624.castleCrackSegments = i625[281]
  i624.castleCrackBendDeg = i625[282]
  i624.castleDustPuffs = i625[283]
  i624.castleChunkSlots = i625[284]
  i624.castleChunkTextureSize = i625[285]
  i624.castleUploadInterval = i625[286]
  i624.platformHitsPerCrack = i625[287]
  i624.castleMaxTiltDeg = i625[288]
  i624.trackMaxTiltDeg = i625[289]
  i624.trackTiltSpeed = i625[290]
  i624.castleSettleSpeed = i625[291]
  i624.swayMaxDeg = i625[292]
  i624.swayFrequency = i625[293]
  i624.swayDamping = i625[294]
  i624.swayImpulseDeg = i625[295]
  i624.swayMaxSpeedDeg = i625[296]
  i624.swayBlastReach = i625[297]
  i624.cannonDetachSpeed = i625[298]
  i624.cannonDetachLift = i625[299]
  i624.cannonDetachSpinDeg = i625[300]
  i624.cannonSplashWidth = i625[301]
  i624.levelWidth = i625[302]
  i624.waterLevel = i625[303]
  i624.levelBottom = i625[304]
  i624.playerCastleX = i625[305]
  i624.enemyCastleX = i625[306]
  i624.trackHeight = i625[307]
  i624.platformHeight = i625[308]
  i624.parallaxFactors = i625[309]
  i624.backBandBase = i625[310]
  i624.moonPosition = new pc.Vec2( i625[311], i625[312] )
  i624.cloudStep = i625[313]
  i624.soundVolume = i625[314]
  i624.soundCooldown = i625[315]
  i624.cameraSmoothTime = i625[316]
  i624.castleFrameWidth = i625[317]
  i624.centerFrameWidth = i625[318]
  i624.centerFrameMinHeight = i625[319]
  i624.centerFrameMaxHeight = i625[320]
  i624.cameraFlightTopMargin = i625[321]
  i624.cameraCenterHold = i625[322]
  i624.targetFrameWidth = i625[323]
  i624.minFrameHeight = i625[324]
  i624.castleLeadX = i625[325]
  i624.castleFocusLiftY = i625[326]
  i624.centerFocus = new pc.Vec2( i625[327], i625[328] )
  i624.cameraHeadFollow = i625[329]
  i624.rainEnabled = !!i625[330]
  i624.rainPerSecond = i625[331]
  i624.rainSpeed = i625[332]
  i624.rainDropSize = i625[333]
  i624.lightBeamEnabled = !!i625[334]
  i624.showPersistentCta = !!i625[335]
  i624.debugSlowScale = i625[336]
  i624.debugHudRefresh = i625[337]
  i624.debugBlastRadii = i625[338]
  i624.debugBlastHoldInterval = i625[339]
  i624.debugBlastUiBand = i625[340]
  i624.debugToggleBand = i625[341]
  i624.debugToggleAlpha = i625[342]
  i624.debugPanelAlpha = i625[343]
  i624.debugOverviewWidth = i625[344]
  var i633 = i625[345]
  var i632 = []
  for(var i = 0; i < i633.length; i += 1) {
    i632.push( request.d('CastleDestroyers.Core.TurnHint', i633[i + 0]) );
  }
  i624.debugBoosterShots = i632
  i624.debugBoosterShotSalvos = i625[346]
  return i624
}

Deserializers["CastleDestroyers.Core.TurnHint"] = function (request, data, root) {
  var i636 = root || request.c( 'CastleDestroyers.Core.TurnHint' )
  var i637 = data
  i636.label = i637[0]
  i636.highlightBoosterMultiplier = i637[1]
  i636.showHand = !!i637[2]
  i636.handAngleDeg = i637[3]
  i636.handPower01 = i637[4]
  i636.panToBooster = !!i637[5]
  return i636
}

Deserializers["CastleDestroyers.Core.AiShotPreset"] = function (request, data, root) {
  var i640 = root || request.c( 'CastleDestroyers.Core.AiShotPreset' )
  var i641 = data
  i640.label = i641[0]
  i640.angleDeg = i641[1]
  i640.power01 = i641[2]
  return i640
}

Deserializers["CastleDestroyers.Core.BoosterSpec"] = function (request, data, root) {
  var i644 = root || request.c( 'CastleDestroyers.Core.BoosterSpec' )
  var i645 = data
  i644.multiplier = i645[0]
  i644.position = new pc.Vec2( i645[1], i645[2] )
  return i644
}

Deserializers["CastleDestroyers.Core.ThemePalette"] = function (request, data, root) {
  var i646 = root || request.c( 'CastleDestroyers.Core.ThemePalette' )
  var i647 = data
  i646.preset = i647[0]
  i646.night = request.d('CastleDestroyers.Core.PaletteSet', i647[1], i646.night)
  i646.sunset = request.d('CastleDestroyers.Core.PaletteSet', i647[2], i646.sunset)
  i646.nightArt = request.d('CastleDestroyers.Core.ThemeArt', i647[3], i646.nightArt)
  i646.sunsetArt = request.d('CastleDestroyers.Core.ThemeArt', i647[4], i646.sunsetArt)
  return i646
}

Deserializers["CastleDestroyers.Core.PaletteSet"] = function (request, data, root) {
  var i648 = root || request.c( 'CastleDestroyers.Core.PaletteSet' )
  var i649 = data
  i648.sky = UnityEngine.Color32.ConstructColor(i649[0], i649[1], i649[2], i649[3])
  i648.skyHorizon = UnityEngine.Color32.ConstructColor(i649[4], i649[5], i649[6], i649[7])
  i648.backgroundHaze = UnityEngine.Color32.ConstructColor(i649[8], i649[9], i649[10], i649[11])
  i648.backgroundFar = UnityEngine.Color32.ConstructColor(i649[12], i649[13], i649[14], i649[15])
  i648.backgroundFarShade = UnityEngine.Color32.ConstructColor(i649[16], i649[17], i649[18], i649[19])
  i648.backgroundMid = UnityEngine.Color32.ConstructColor(i649[20], i649[21], i649[22], i649[23])
  i648.backgroundNear = UnityEngine.Color32.ConstructColor(i649[24], i649[25], i649[26], i649[27])
  i648.backgroundNearLight = UnityEngine.Color32.ConstructColor(i649[28], i649[29], i649[30], i649[31])
  i648.skyStar = UnityEngine.Color32.ConstructColor(i649[32], i649[33], i649[34], i649[35])
  i648.moon = UnityEngine.Color32.ConstructColor(i649[36], i649[37], i649[38], i649[39])
  i648.moonShade = UnityEngine.Color32.ConstructColor(i649[40], i649[41], i649[42], i649[43])
  i648.ground = UnityEngine.Color32.ConstructColor(i649[44], i649[45], i649[46], i649[47])
  i648.groundShade = UnityEngine.Color32.ConstructColor(i649[48], i649[49], i649[50], i649[51])
  i648.groundBuried = UnityEngine.Color32.ConstructColor(i649[52], i649[53], i649[54], i649[55])
  i648.groundBand = UnityEngine.Color32.ConstructColor(i649[56], i649[57], i649[58], i649[59])
  i648.groundRim = UnityEngine.Color32.ConstructColor(i649[60], i649[61], i649[62], i649[63])
  i648.groundRimLight = UnityEngine.Color32.ConstructColor(i649[64], i649[65], i649[66], i649[67])
  i648.groundCrater = UnityEngine.Color32.ConstructColor(i649[68], i649[69], i649[70], i649[71])
  i648.bush = UnityEngine.Color32.ConstructColor(i649[72], i649[73], i649[74], i649[75])
  i648.bushLight = UnityEngine.Color32.ConstructColor(i649[76], i649[77], i649[78], i649[79])
  i648.treeLight = UnityEngine.Color32.ConstructColor(i649[80], i649[81], i649[82], i649[83])
  i648.treeShade = UnityEngine.Color32.ConstructColor(i649[84], i649[85], i649[86], i649[87])
  i648.water = UnityEngine.Color32.ConstructColor(i649[88], i649[89], i649[90], i649[91])
  i648.waterHighlight = UnityEngine.Color32.ConstructColor(i649[92], i649[93], i649[94], i649[95])
  i648.waterRim = UnityEngine.Color32.ConstructColor(i649[96], i649[97], i649[98], i649[99])
  i648.castleStone = UnityEngine.Color32.ConstructColor(i649[100], i649[101], i649[102], i649[103])
  i648.castleStoneShade = UnityEngine.Color32.ConstructColor(i649[104], i649[105], i649[106], i649[107])
  i648.castleBrick = UnityEngine.Color32.ConstructColor(i649[108], i649[109], i649[110], i649[111])
  i648.castleCrack = UnityEngine.Color32.ConstructColor(i649[112], i649[113], i649[114], i649[115])
  i648.castleWindow = UnityEngine.Color32.ConstructColor(i649[116], i649[117], i649[118], i649[119])
  i648.castleWindowLit = UnityEngine.Color32.ConstructColor(i649[120], i649[121], i649[122], i649[123])
  i648.roofPlayer = UnityEngine.Color32.ConstructColor(i649[124], i649[125], i649[126], i649[127])
  i648.roofPlayerShade = UnityEngine.Color32.ConstructColor(i649[128], i649[129], i649[130], i649[131])
  i648.roofEnemy = UnityEngine.Color32.ConstructColor(i649[132], i649[133], i649[134], i649[135])
  i648.roofEnemyShade = UnityEngine.Color32.ConstructColor(i649[136], i649[137], i649[138], i649[139])
  i648.cannonPlayer = UnityEngine.Color32.ConstructColor(i649[140], i649[141], i649[142], i649[143])
  i648.cannonPlayerShade = UnityEngine.Color32.ConstructColor(i649[144], i649[145], i649[146], i649[147])
  i648.cannonEnemy = UnityEngine.Color32.ConstructColor(i649[148], i649[149], i649[150], i649[151])
  i648.cannonEnemyShade = UnityEngine.Color32.ConstructColor(i649[152], i649[153], i649[154], i649[155])
  i648.platform = UnityEngine.Color32.ConstructColor(i649[156], i649[157], i649[158], i649[159])
  i648.platformShade = UnityEngine.Color32.ConstructColor(i649[160], i649[161], i649[162], i649[163])
  i648.track = UnityEngine.Color32.ConstructColor(i649[164], i649[165], i649[166], i649[167])
  i648.trackWheel = UnityEngine.Color32.ConstructColor(i649[168], i649[169], i649[170], i649[171])
  i648.booster10 = UnityEngine.Color32.ConstructColor(i649[172], i649[173], i649[174], i649[175])
  i648.booster10Cap = UnityEngine.Color32.ConstructColor(i649[176], i649[177], i649[178], i649[179])
  i648.booster20 = UnityEngine.Color32.ConstructColor(i649[180], i649[181], i649[182], i649[183])
  i648.booster20Cap = UnityEngine.Color32.ConstructColor(i649[184], i649[185], i649[186], i649[187])
  i648.booster50 = UnityEngine.Color32.ConstructColor(i649[188], i649[189], i649[190], i649[191])
  i648.booster50Cap = UnityEngine.Color32.ConstructColor(i649[192], i649[193], i649[194], i649[195])
  i648.rocketBody = UnityEngine.Color32.ConstructColor(i649[196], i649[197], i649[198], i649[199])
  i648.rocketShade = UnityEngine.Color32.ConstructColor(i649[200], i649[201], i649[202], i649[203])
  i648.rocketPlayer = UnityEngine.Color32.ConstructColor(i649[204], i649[205], i649[206], i649[207])
  i648.rocketEnemy = UnityEngine.Color32.ConstructColor(i649[208], i649[209], i649[210], i649[211])
  i648.rocketTail = UnityEngine.Color32.ConstructColor(i649[212], i649[213], i649[214], i649[215])
  i648.smoke = UnityEngine.Color32.ConstructColor(i649[216], i649[217], i649[218], i649[219])
  i648.blastCore = UnityEngine.Color32.ConstructColor(i649[220], i649[221], i649[222], i649[223])
  i648.blastMid = UnityEngine.Color32.ConstructColor(i649[224], i649[225], i649[226], i649[227])
  i648.blastEdge = UnityEngine.Color32.ConstructColor(i649[228], i649[229], i649[230], i649[231])
  i648.hudPanel = UnityEngine.Color32.ConstructColor(i649[232], i649[233], i649[234], i649[235])
  i648.hudTrack = UnityEngine.Color32.ConstructColor(i649[236], i649[237], i649[238], i649[239])
  i648.hudHealth = UnityEngine.Color32.ConstructColor(i649[240], i649[241], i649[242], i649[243])
  i648.hudHealthShade = UnityEngine.Color32.ConstructColor(i649[244], i649[245], i649[246], i649[247])
  i648.hudGold = UnityEngine.Color32.ConstructColor(i649[248], i649[249], i649[250], i649[251])
  i648.hudGoldShade = UnityEngine.Color32.ConstructColor(i649[252], i649[253], i649[254], i649[255])
  i648.hudText = UnityEngine.Color32.ConstructColor(i649[256], i649[257], i649[258], i649[259])
  return i648
}

Deserializers["CastleDestroyers.Core.ThemeArt"] = function (request, data, root) {
  var i650 = root || request.c( 'CastleDestroyers.Core.ThemeArt' )
  var i651 = data
  request.r(i651[0], i651[1], 0, i650, 'atlas')
  request.r(i651[2], i651[3], 0, i650, 'castleBlue')
  request.r(i651[4], i651[5], 0, i650, 'castleRed')
  request.r(i651[6], i651[7], 0, i650, 'backFar')
  request.r(i651[8], i651[9], 0, i650, 'backMid')
  request.r(i651[10], i651[11], 0, i650, 'backNear')
  request.r(i651[12], i651[13], 0, i650, 'terrain')
  return i650
}

Deserializers["CastleDestroyers.Destruction.TerrainMaskData"] = function (request, data, root) {
  var i652 = root || request.c( 'CastleDestroyers.Destruction.TerrainMaskData' )
  var i653 = data
  i652.width = i653[0]
  i652.height = i653[1]
  i652.solidRuns = i653[2]
  i652.grassRuns = i653[3]
  i652.buriedRuns = i653[4]
  return i652
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Resources"] = function (request, data, root) {
  var i654 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Resources' )
  var i655 = data
  var i657 = i655[0]
  var i656 = []
  for(var i = 0; i < i657.length; i += 1) {
    i656.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Resources+File', i657[i + 0]) );
  }
  i654.files = i656
  i654.componentToPrefabIds = i655[1]
  return i654
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Resources+File"] = function (request, data, root) {
  var i660 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Resources+File' )
  var i661 = data
  i660.path = i661[0]
  request.r(i661[1], i661[2], 0, i660, 'unityObject')
  return i660
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings"] = function (request, data, root) {
  var i662 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings' )
  var i663 = data
  var i665 = i663[0]
  var i664 = []
  for(var i = 0; i < i665.length; i += 1) {
    i664.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+ScriptsExecutionOrder', i665[i + 0]) );
  }
  i662.scriptsExecutionOrder = i664
  var i667 = i663[1]
  var i666 = []
  for(var i = 0; i < i667.length; i += 1) {
    i666.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+SortingLayer', i667[i + 0]) );
  }
  i662.sortingLayers = i666
  var i669 = i663[2]
  var i668 = []
  for(var i = 0; i < i669.length; i += 1) {
    i668.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+CullingLayer', i669[i + 0]) );
  }
  i662.cullingLayers = i668
  i662.timeSettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+TimeSettings', i663[3], i662.timeSettings)
  i662.physicsSettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings', i663[4], i662.physicsSettings)
  i662.physics2DSettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings', i663[5], i662.physics2DSettings)
  i662.qualitySettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.QualitySettings', i663[6], i662.qualitySettings)
  i662.enableRealtimeShadows = !!i663[7]
  i662.enableAutoInstancing = !!i663[8]
  i662.enableStaticBatching = !!i663[9]
  i662.enableDynamicBatching = !!i663[10]
  i662.usePreservativeDynamicBatching = !!i663[11]
  i662.lightmapEncodingQuality = i663[12]
  i662.desiredColorSpace = i663[13]
  var i671 = i663[14]
  var i670 = []
  for(var i = 0; i < i671.length; i += 1) {
    i670.push( i671[i + 0] );
  }
  i662.allTags = i670
  return i662
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+ScriptsExecutionOrder"] = function (request, data, root) {
  var i674 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+ScriptsExecutionOrder' )
  var i675 = data
  i674.name = i675[0]
  i674.value = i675[1]
  return i674
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+SortingLayer"] = function (request, data, root) {
  var i678 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+SortingLayer' )
  var i679 = data
  i678.id = i679[0]
  i678.name = i679[1]
  i678.value = i679[2]
  return i678
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+CullingLayer"] = function (request, data, root) {
  var i682 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+CullingLayer' )
  var i683 = data
  i682.id = i683[0]
  i682.name = i683[1]
  return i682
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+TimeSettings"] = function (request, data, root) {
  var i684 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+TimeSettings' )
  var i685 = data
  i684.fixedDeltaTime = i685[0]
  i684.maximumDeltaTime = i685[1]
  i684.timeScale = i685[2]
  i684.maximumParticleTimestep = i685[3]
  return i684
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings"] = function (request, data, root) {
  var i686 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings' )
  var i687 = data
  i686.gravity = new pc.Vec3( i687[0], i687[1], i687[2] )
  i686.defaultSolverIterations = i687[3]
  i686.bounceThreshold = i687[4]
  i686.autoSyncTransforms = !!i687[5]
  i686.autoSimulation = !!i687[6]
  var i689 = i687[7]
  var i688 = []
  for(var i = 0; i < i689.length; i += 1) {
    i688.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings+CollisionMask', i689[i + 0]) );
  }
  i686.collisionMatrix = i688
  return i686
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings+CollisionMask"] = function (request, data, root) {
  var i692 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings+CollisionMask' )
  var i693 = data
  i692.enabled = !!i693[0]
  i692.layerId = i693[1]
  i692.otherLayerId = i693[2]
  return i692
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings"] = function (request, data, root) {
  var i694 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings' )
  var i695 = data
  request.r(i695[0], i695[1], 0, i694, 'material')
  i694.gravity = new pc.Vec2( i695[2], i695[3] )
  i694.positionIterations = i695[4]
  i694.velocityIterations = i695[5]
  i694.velocityThreshold = i695[6]
  i694.maxLinearCorrection = i695[7]
  i694.maxAngularCorrection = i695[8]
  i694.maxTranslationSpeed = i695[9]
  i694.maxRotationSpeed = i695[10]
  i694.baumgarteScale = i695[11]
  i694.baumgarteTOIScale = i695[12]
  i694.timeToSleep = i695[13]
  i694.linearSleepTolerance = i695[14]
  i694.angularSleepTolerance = i695[15]
  i694.defaultContactOffset = i695[16]
  i694.autoSimulation = !!i695[17]
  i694.queriesHitTriggers = !!i695[18]
  i694.queriesStartInColliders = !!i695[19]
  i694.callbacksOnDisable = !!i695[20]
  i694.reuseCollisionCallbacks = !!i695[21]
  i694.autoSyncTransforms = !!i695[22]
  var i697 = i695[23]
  var i696 = []
  for(var i = 0; i < i697.length; i += 1) {
    i696.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings+CollisionMask', i697[i + 0]) );
  }
  i694.collisionMatrix = i696
  return i694
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings+CollisionMask"] = function (request, data, root) {
  var i700 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings+CollisionMask' )
  var i701 = data
  i700.enabled = !!i701[0]
  i700.layerId = i701[1]
  i700.otherLayerId = i701[2]
  return i700
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.QualitySettings"] = function (request, data, root) {
  var i702 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.QualitySettings' )
  var i703 = data
  var i705 = i703[0]
  var i704 = []
  for(var i = 0; i < i705.length; i += 1) {
    i704.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.QualitySettings', i705[i + 0]) );
  }
  i702.qualityLevels = i704
  var i707 = i703[1]
  var i706 = []
  for(var i = 0; i < i707.length; i += 1) {
    i706.push( i707[i + 0] );
  }
  i702.names = i706
  i702.shadows = i703[2]
  i702.anisotropicFiltering = i703[3]
  i702.antiAliasing = i703[4]
  i702.lodBias = i703[5]
  i702.shadowCascades = i703[6]
  i702.shadowDistance = i703[7]
  i702.shadowmaskMode = i703[8]
  i702.shadowProjection = i703[9]
  i702.shadowResolution = i703[10]
  i702.softParticles = !!i703[11]
  i702.softVegetation = !!i703[12]
  i702.activeColorSpace = i703[13]
  i702.desiredColorSpace = i703[14]
  i702.masterTextureLimit = i703[15]
  i702.maxQueuedFrames = i703[16]
  i702.particleRaycastBudget = i703[17]
  i702.pixelLightCount = i703[18]
  i702.realtimeReflectionProbes = !!i703[19]
  i702.shadowCascade2Split = i703[20]
  i702.shadowCascade4Split = new pc.Vec3( i703[21], i703[22], i703[23] )
  i702.streamingMipmapsActive = !!i703[24]
  i702.vSyncCount = i703[25]
  i702.asyncUploadBufferSize = i703[26]
  i702.asyncUploadTimeSlice = i703[27]
  i702.billboardsFaceCameraPosition = !!i703[28]
  i702.shadowNearPlaneOffset = i703[29]
  i702.streamingMipmapsMemoryBudget = i703[30]
  i702.maximumLODLevel = i703[31]
  i702.streamingMipmapsAddAllCameras = !!i703[32]
  i702.streamingMipmapsMaxLevelReduction = i703[33]
  i702.streamingMipmapsRenderersPerFrame = i703[34]
  i702.resolutionScalingFixedDPIFactor = i703[35]
  i702.streamingMipmapsMaxFileIORequests = i703[36]
  i702.currentQualityLevel = i703[37]
  return i702
}

Deserializers["UnityEngine.Events.ArgumentCache"] = function (request, data, root) {
  var i710 = root || request.c( 'UnityEngine.Events.ArgumentCache' )
  var i711 = data
  request.r(i711[0], i711[1], 0, i710, 'm_ObjectArgument')
  i710.m_ObjectArgumentAssemblyTypeName = i711[2]
  i710.m_IntArgument = i711[3]
  i710.m_FloatArgument = i711[4]
  i710.m_StringArgument = i711[5]
  i710.m_BoolArgument = !!i711[6]
  return i710
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+RenderObjectsSettings"] = function (request, data, root) {
  var i712 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+RenderObjectsSettings' )
  var i713 = data
  i712.Event = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i713[0], i712.Event)
  i712.filterSettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+FilterSettings', i713[1], i712.filterSettings)
  i712.overrideMaterialId = i713[2]
  i712.overrideMaterialPassIndex = i713[3]
  i712.overrideShaderId = i713[4]
  i712.overrideShaderPassIndex = i713[5]
  i712.overrideMode = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i713[6], i712.overrideMode)
  i712.overrideDepthState = !!i713[7]
  i712.depthCompareFunction = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i713[8], i712.depthCompareFunction)
  i712.enableWrite = !!i713[9]
  i712.stencilSettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.StencilStateData', i713[10], i712.stencilSettings)
  i712.cameraSettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+CustomCameraSettings', i713[11], i712.cameraSettings)
  return i712
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.EnumDescription"] = function (request, data, root) {
  var i714 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.EnumDescription' )
  var i715 = data
  i714.Value = i715[0]
  return i714
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+FilterSettings"] = function (request, data, root) {
  var i716 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+FilterSettings' )
  var i717 = data
  i716.RenderQueueType = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i717[0], i716.RenderQueueType)
  i716.LayerMask = i717[1]
  var i719 = i717[2]
  var i718 = []
  for(var i = 0; i < i719.length; i += 1) {
    i718.push( i719[i + 0] );
  }
  i716.PassNames = i718
  return i716
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.StencilStateData"] = function (request, data, root) {
  var i720 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.StencilStateData' )
  var i721 = data
  i720.overrideStencilState = !!i721[0]
  i720.stencilReference = i721[1]
  i720.stencilCompareFunctionValue = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i721[2], i720.stencilCompareFunctionValue)
  i720.passOperationValue = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i721[3], i720.passOperationValue)
  i720.failOperationValue = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i721[4], i720.failOperationValue)
  i720.zFailOperationValue = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i721[5], i720.zFailOperationValue)
  return i720
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+CustomCameraSettings"] = function (request, data, root) {
  var i722 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+CustomCameraSettings' )
  var i723 = data
  i722.overrideCamera = !!i723[0]
  i722.restoreCamera = !!i723[1]
  i722.offset = new pc.Vec4( i723[2], i723[3], i723[4], i723[5] )
  i722.cameraFieldOfView = i723[6]
  return i722
}

Deserializers.fields = {"Luna.Unity.DTO.UnityEngine.Assets.Material":{"name":0,"shader":1,"renderQueue":3,"enableInstancing":4,"floatParameters":5,"colorParameters":6,"vectorParameters":7,"textureParameters":8,"materialFlags":9},"Luna.Unity.DTO.UnityEngine.Assets.Material+FloatParameter":{"name":0,"value":1},"Luna.Unity.DTO.UnityEngine.Assets.Material+ColorParameter":{"name":0,"value":1},"Luna.Unity.DTO.UnityEngine.Assets.Material+VectorParameter":{"name":0,"value":1},"Luna.Unity.DTO.UnityEngine.Assets.Material+TextureParameter":{"name":0,"value":1},"Luna.Unity.DTO.UnityEngine.Assets.Material+MaterialFlag":{"name":0,"enabled":1},"Luna.Unity.DTO.UnityEngine.Textures.Texture2D":{"name":0,"width":1,"height":2,"mipmapCount":3,"anisoLevel":4,"filterMode":5,"hdr":6,"format":7,"wrapMode":8,"alphaIsTransparency":9,"alphaSource":10,"graphicsFormat":11,"sRGBTexture":12,"desiredColorSpace":13,"wrapU":14,"wrapV":15},"Luna.Unity.DTO.UnityEngine.Scene.Scene":{"name":0,"index":1,"startup":2},"Luna.Unity.DTO.UnityEngine.Components.Camera":{"aspect":0,"orthographic":1,"orthographicSize":2,"backgroundColor":3,"nearClipPlane":7,"farClipPlane":8,"fieldOfView":9,"depth":10,"clearFlags":11,"cullingMask":12,"rect":13,"targetTexture":14,"usePhysicalProperties":16,"focalLength":17,"sensorSize":18,"lensShift":20,"gateFit":22,"commandBufferCount":23,"cameraType":24,"enabled":25},"Luna.Unity.DTO.UnityEngine.Scene.GameObject":{"name":0,"tagId":1,"enabled":2,"isStatic":3,"layer":4},"Luna.Unity.DTO.UnityEngine.Components.MeshFilter":{"sharedMesh":0},"Luna.Unity.DTO.UnityEngine.Components.MeshRenderer":{"additionalVertexStreams":0,"enabled":2,"sharedMaterial":3,"sharedMaterials":5,"receiveShadows":6,"shadowCastingMode":7,"sortingLayerID":8,"sortingOrder":9,"lightmapIndex":10,"lightmapSceneIndex":11,"lightmapScaleOffset":12,"lightProbeUsage":16,"reflectionProbeUsage":17},"Luna.Unity.DTO.UnityEngine.Components.AudioSource":{"clip":0,"outputAudioMixerGroup":2,"playOnAwake":4,"loop":5,"time":6,"volume":7,"pitch":8,"enabled":9},"Luna.Unity.DTO.UnityEngine.Components.RectTransform":{"pivot":0,"anchorMin":2,"anchorMax":4,"sizeDelta":6,"anchoredPosition3D":8,"rotation":11,"scale":15},"Luna.Unity.DTO.UnityEngine.Components.Canvas":{"planeDistance":0,"referencePixelsPerUnit":1,"isFallbackOverlay":2,"renderMode":3,"renderOrder":4,"sortingLayerName":5,"sortingOrder":6,"scaleFactor":7,"worldCamera":8,"overrideSorting":10,"pixelPerfect":11,"targetDisplay":12,"overridePixelPerfect":13,"enabled":14},"Luna.Unity.DTO.UnityEngine.Components.CanvasRenderer":{"cullTransparentMesh":0},"Luna.Unity.DTO.UnityEngine.Assets.RenderSettings":{"ambientIntensity":0,"reflectionIntensity":1,"ambientMode":2,"ambientLight":3,"ambientSkyColor":7,"ambientGroundColor":11,"ambientEquatorColor":15,"fogColor":19,"fogEndDistance":23,"fogStartDistance":24,"fogDensity":25,"fog":26,"skybox":27,"fogMode":29,"lightmaps":30,"lightProbes":31,"lightmapsMode":32,"mixedBakeMode":33,"environmentLightingMode":34,"ambientProbe":35,"customReflection":36,"defaultReflection":38,"defaultReflectionMode":40,"defaultReflectionResolution":41,"sunLightObjectId":42,"pixelLightCount":43,"defaultReflectionHDR":44,"hasLightDataAsset":45,"hasManualGenerate":46},"Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+Lightmap":{"lightmapColor":0,"lightmapDirection":2,"shadowMask":4},"Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+LightProbes":{"bakedProbes":0,"positions":1,"hullRays":2,"tetrahedra":3,"neighbours":4,"matrices":5},"Luna.Unity.DTO.UnityEngine.Assets.UniversalRenderPipelineAsset":{"AdditionalLightsRenderingMode":0,"LightRenderingMode":1,"MainLightRenderingModeValue":2,"SupportsMainLightShadows":3,"MixedLightingSupported":4,"MainLightShadowmapResolutionValue":5,"SupportsSoftShadows":6,"SoftShadowQualityValue":7,"ShadowDistance":8,"ShadowCascadeCount":9,"Cascade2Split":10,"Cascade3Split":11,"Cascade4Split":13,"CascadeBorder":16,"ShadowDepthBias":17,"ShadowNormalBias":18,"RequireDepthTexture":19,"RequireOpaqueTexture":20,"scriptableRendererData":21},"Luna.Unity.DTO.UnityEngine.Assets.LightRenderingMode":{"Disabled":0,"PerVertex":1,"PerPixel":2},"Luna.Unity.DTO.UnityEngine.Assets.ScriptableRendererData":{"opaqueLayerMask":0,"transparentLayerMask":1,"RenderObjectsFeatures":2,"name":3},"Luna.Unity.DTO.UnityEngine.Assets.RenderObjects":{"settings":0,"name":1,"typeName":2},"Luna.Unity.DTO.UnityEngine.Assets.Shader":{"ShaderCompilationErrors":0,"name":1,"guid":2,"shaderDefinedKeywords":3,"passes":4,"usePasses":5,"defaultParameterValues":6,"unityFallbackShader":7,"readDepth":9,"hasDepthOnlyPass":10,"isCreatedByShaderGraph":11,"disableBatching":12,"compiled":13},"Luna.Unity.DTO.UnityEngine.Assets.Shader+ShaderCompilationError":{"shaderName":0,"errorMessage":1},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass":{"id":0,"subShaderIndex":1,"name":2,"passType":3,"grabPassTextureName":4,"usePass":5,"zTest":6,"zWrite":7,"culling":8,"blending":9,"alphaBlending":10,"colorWriteMask":11,"offsetUnits":12,"offsetFactor":13,"stencilRef":14,"stencilReadMask":15,"stencilWriteMask":16,"stencilOp":17,"stencilOpFront":18,"stencilOpBack":19,"tags":20,"passDefinedKeywords":21,"passDefinedKeywordGroups":22,"variants":23,"excludedVariants":24,"hasDepthReader":25},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value":{"val":0,"name":1},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Blending":{"src":0,"dst":1,"op":2},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+StencilOp":{"pass":0,"fail":1,"zFail":2,"comp":3},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Tag":{"name":0,"value":1},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+KeywordGroup":{"keywords":0,"hasDiscard":1},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Variant":{"passId":0,"subShaderIndex":1,"keywords":2,"vertexProgram":3,"fragmentProgram":4,"exportedForWebGl2":5,"readDepth":6},"Luna.Unity.DTO.UnityEngine.Assets.Shader+UsePass":{"shader":0,"pass":2},"Luna.Unity.DTO.UnityEngine.Assets.Shader+DefaultParameterValue":{"name":0,"type":1,"value":2,"textureValue":6,"shaderPropertyFlag":7},"Luna.Unity.DTO.UnityEngine.Textures.Sprite":{"name":0,"texture":1,"aabb":3,"vertices":4,"triangles":5,"textureRect":6,"packedRect":10,"border":14,"transparency":18,"bounds":19,"pixelsPerUnit":20,"textureWidth":21,"textureHeight":22,"nativeSize":23,"pivot":25,"textureRectOffset":27},"Luna.Unity.DTO.UnityEngine.Assets.AudioClip":{"name":0},"Luna.Unity.DTO.UnityEngine.Assets.Font":{"name":0,"ascent":1,"originalLineHeight":2,"fontSize":3,"characterInfo":4,"texture":5,"originalFontSize":7},"Luna.Unity.DTO.UnityEngine.Assets.Font+CharacterInfo":{"index":0,"advance":1,"bearing":2,"glyphWidth":3,"glyphHeight":4,"minX":5,"maxX":6,"minY":7,"maxY":8,"uvBottomLeftX":9,"uvBottomLeftY":10,"uvBottomRightX":11,"uvBottomRightY":12,"uvTopLeftX":13,"uvTopLeftY":14,"uvTopRightX":15,"uvTopRightY":16},"Luna.Unity.DTO.UnityEngine.Assets.Resources":{"files":0,"componentToPrefabIds":1},"Luna.Unity.DTO.UnityEngine.Assets.Resources+File":{"path":0,"unityObject":1},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings":{"scriptsExecutionOrder":0,"sortingLayers":1,"cullingLayers":2,"timeSettings":3,"physicsSettings":4,"physics2DSettings":5,"qualitySettings":6,"enableRealtimeShadows":7,"enableAutoInstancing":8,"enableStaticBatching":9,"enableDynamicBatching":10,"usePreservativeDynamicBatching":11,"lightmapEncodingQuality":12,"desiredColorSpace":13,"allTags":14},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+ScriptsExecutionOrder":{"name":0,"value":1},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+SortingLayer":{"id":0,"name":1,"value":2},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+CullingLayer":{"id":0,"name":1},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+TimeSettings":{"fixedDeltaTime":0,"maximumDeltaTime":1,"timeScale":2,"maximumParticleTimestep":3},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings":{"gravity":0,"defaultSolverIterations":3,"bounceThreshold":4,"autoSyncTransforms":5,"autoSimulation":6,"collisionMatrix":7},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings+CollisionMask":{"enabled":0,"layerId":1,"otherLayerId":2},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings":{"material":0,"gravity":2,"positionIterations":4,"velocityIterations":5,"velocityThreshold":6,"maxLinearCorrection":7,"maxAngularCorrection":8,"maxTranslationSpeed":9,"maxRotationSpeed":10,"baumgarteScale":11,"baumgarteTOIScale":12,"timeToSleep":13,"linearSleepTolerance":14,"angularSleepTolerance":15,"defaultContactOffset":16,"autoSimulation":17,"queriesHitTriggers":18,"queriesStartInColliders":19,"callbacksOnDisable":20,"reuseCollisionCallbacks":21,"autoSyncTransforms":22,"collisionMatrix":23},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings+CollisionMask":{"enabled":0,"layerId":1,"otherLayerId":2},"Luna.Unity.DTO.UnityEngine.Assets.QualitySettings":{"qualityLevels":0,"names":1,"shadows":2,"anisotropicFiltering":3,"antiAliasing":4,"lodBias":5,"shadowCascades":6,"shadowDistance":7,"shadowmaskMode":8,"shadowProjection":9,"shadowResolution":10,"softParticles":11,"softVegetation":12,"activeColorSpace":13,"desiredColorSpace":14,"masterTextureLimit":15,"maxQueuedFrames":16,"particleRaycastBudget":17,"pixelLightCount":18,"realtimeReflectionProbes":19,"shadowCascade2Split":20,"shadowCascade4Split":21,"streamingMipmapsActive":24,"vSyncCount":25,"asyncUploadBufferSize":26,"asyncUploadTimeSlice":27,"billboardsFaceCameraPosition":28,"shadowNearPlaneOffset":29,"streamingMipmapsMemoryBudget":30,"maximumLODLevel":31,"streamingMipmapsAddAllCameras":32,"streamingMipmapsMaxLevelReduction":33,"streamingMipmapsRenderersPerFrame":34,"resolutionScalingFixedDPIFactor":35,"streamingMipmapsMaxFileIORequests":36,"currentQualityLevel":37},"Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+RenderObjectsSettings":{"Event":0,"filterSettings":1,"overrideMaterialId":2,"overrideMaterialPassIndex":3,"overrideShaderId":4,"overrideShaderPassIndex":5,"overrideMode":6,"overrideDepthState":7,"depthCompareFunction":8,"enableWrite":9,"stencilSettings":10,"cameraSettings":11},"Luna.Unity.DTO.UnityEngine.Assets.EnumDescription":{"Value":0},"Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+FilterSettings":{"RenderQueueType":0,"LayerMask":1,"PassNames":2},"Luna.Unity.DTO.UnityEngine.Assets.StencilStateData":{"overrideStencilState":0,"stencilReference":1,"stencilCompareFunctionValue":2,"passOperationValue":3,"failOperationValue":4,"zFailOperationValue":5},"Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+CustomCameraSettings":{"overrideCamera":0,"restoreCamera":1,"offset":2,"cameraFieldOfView":6}}

Deserializers.requiredComponents = {"32":[33],"34":[33],"35":[33],"36":[33],"37":[33],"38":[33],"39":[40],"41":[2],"42":[43],"44":[43],"45":[43],"46":[43],"47":[43],"48":[43],"49":[50],"51":[50],"52":[50],"53":[50],"54":[50],"55":[50],"56":[50],"57":[50],"58":[50],"59":[50],"60":[50],"61":[50],"62":[50],"63":[2],"64":[7],"65":[66],"67":[66],"20":[11],"68":[27],"69":[20],"70":[11],"71":[2],"4":[2],"72":[73],"74":[11],"75":[23,11],"76":[7],"77":[23,11],"78":[11],"79":[11],"80":[7,11],"81":[11,23],"82":[11],"83":[11],"25":[20],"12":[23,11],"84":[11],"22":[20],"85":[11],"29":[11],"86":[11],"87":[11],"88":[11],"89":[11],"90":[11],"91":[11],"92":[11],"93":[23,11],"94":[11],"95":[11],"96":[11],"97":[11],"13":[23,11],"98":[11],"99":[27],"100":[27],"28":[27],"101":[27],"102":[2],"103":[2]}

Deserializers.types = ["UnityEngine.Shader","UnityEngine.Texture2D","UnityEngine.Camera","UnityEngine.MonoBehaviour","UnityEngine.Rendering.Universal.UniversalAdditionalCameraData","UnityEngine.AudioListener","UnityEngine.MeshFilter","UnityEngine.MeshRenderer","UnityEngine.Material","CastleDestroyers.Core.GameBootstrap","CastleDestroyers.Core.GameConfig","UnityEngine.RectTransform","UnityEngine.UI.Image","UnityEngine.UI.Text","UnityEngine.GameObject","UnityEngine.Sprite","UnityEngine.AudioClip","UnityEngine.AudioSource","UnityEngine.Transform","CastleDestroyers.DebugTools.DebugOverlay","UnityEngine.Canvas","UnityEngine.EventSystems.UIBehaviour","UnityEngine.UI.CanvasScaler","UnityEngine.CanvasRenderer","UnityEngine.Font","UnityEngine.UI.GraphicRaycaster","UnityEngine.UI.Button","UnityEngine.EventSystems.EventSystem","UnityEngine.EventSystems.StandaloneInputModule","UnityEngine.UI.GridLayoutGroup","CastleDestroyers.Core.ThemePalette","CastleDestroyers.Destruction.TerrainMaskData","UnityEngine.AudioLowPassFilter","UnityEngine.AudioBehaviour","UnityEngine.AudioHighPassFilter","UnityEngine.AudioReverbFilter","UnityEngine.AudioDistortionFilter","UnityEngine.AudioEchoFilter","UnityEngine.AudioChorusFilter","UnityEngine.Cloth","UnityEngine.SkinnedMeshRenderer","UnityEngine.FlareLayer","UnityEngine.CharacterJoint","UnityEngine.Rigidbody","UnityEngine.ConfigurableJoint","UnityEngine.ConstantForce","UnityEngine.FixedJoint","UnityEngine.HingeJoint","UnityEngine.SpringJoint","UnityEngine.CompositeCollider2D","UnityEngine.Rigidbody2D","UnityEngine.Joint2D","UnityEngine.AnchoredJoint2D","UnityEngine.SpringJoint2D","UnityEngine.DistanceJoint2D","UnityEngine.FrictionJoint2D","UnityEngine.HingeJoint2D","UnityEngine.RelativeJoint2D","UnityEngine.SliderJoint2D","UnityEngine.TargetJoint2D","UnityEngine.FixedJoint2D","UnityEngine.WheelJoint2D","UnityEngine.ConstantForce2D","UnityEngine.StreamingController","UnityEngine.TextMesh","UnityEngine.Tilemaps.TilemapRenderer","UnityEngine.Tilemaps.Tilemap","UnityEngine.Tilemaps.TilemapCollider2D","UnityEngine.InputSystem.UI.InputSystemUIInputModule","UnityEngine.InputSystem.UI.TrackedDeviceRaycaster","UnityEngine.Rendering.UI.UIFoldout","UnityEngine.Rendering.Universal.PixelPerfectCamera","UnityEngine.Rendering.Universal.UniversalAdditionalLightData","UnityEngine.Light","TMPro.TMP_Dropdown","TMPro.TMP_SelectionCaret","TMPro.TMP_SubMesh","TMPro.TMP_SubMeshUI","TMPro.TMP_Text","TMPro.TextContainer","TMPro.TextMeshPro","TMPro.TextMeshProUGUI","UnityEngine.UI.Dropdown","UnityEngine.UI.Graphic","UnityEngine.UI.AspectRatioFitter","UnityEngine.UI.ContentSizeFitter","UnityEngine.UI.HorizontalLayoutGroup","UnityEngine.UI.HorizontalOrVerticalLayoutGroup","UnityEngine.UI.LayoutElement","UnityEngine.UI.LayoutGroup","UnityEngine.UI.VerticalLayoutGroup","UnityEngine.UI.Mask","UnityEngine.UI.MaskableGraphic","UnityEngine.UI.RawImage","UnityEngine.UI.RectMask2D","UnityEngine.UI.ScrollRect","UnityEngine.UI.Scrollbar","UnityEngine.UI.Slider","UnityEngine.UI.Toggle","UnityEngine.EventSystems.BaseInputModule","UnityEngine.EventSystems.PointerInputModule","UnityEngine.EventSystems.TouchInputModule","UnityEngine.EventSystems.Physics2DRaycaster","UnityEngine.EventSystems.PhysicsRaycaster"]

Deserializers.unityVersion = "6000.0.85f1";

Deserializers.productName = "castle-destroyers-playable";

Deserializers.lunaInitializationTime = "";

Deserializers.lunaDaysRunning = "1.9";

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

Deserializers.buildID = "fda35df5-f583-418f-bfe3-69f074baf9ed";

Deserializers.runtimeInitializeOnLoadInfos = [[["UnityEngine","Rendering","DebugUpdater","RuntimeInit"],["Unity","PerformanceTesting","PerformanceTest","ResetStaticsOnLoad"],["Unity","Burst","BurstCompiler","ResetStaticsOnLoad"],["UnityEngine","Experimental","Rendering","ScriptableRuntimeReflectionSystemSettings","ScriptingDirtyReflectionSystemInstance"]],[["Unity","Pipeline","Console","ConsoleLogCapture","RuntimeBootstrap"],["Unity","Pipeline","RuntimePipelineBootstrap","RuntimeBootstrap"],["UnityEngine","InputSystem","PlayerInput","InitializeGlobalPlayerState"],["UnityEngine","InputSystem","InputSystem","RuntimeInitialize"],["UnityEngine","InputSystem","InputActionState","InitializeGlobalActionState"],["UnityEngine","AI","NavMesh","ClearPreUpdateListeners"]],[["$BurstDirectCallInitializer","Initialize"],["$BurstDirectCallInitializer","Initialize"],["$BurstDirectCallInitializer","Initialize"],["$BurstDirectCallInitializer","Initialize"]],[["UnityEngine","Experimental","Rendering","XRSystem","XRSystemInit"]],[["Unity","PerformanceTesting","Data","RunSettings","ResetStaticsOnLoad"],["Unity","PerformanceTesting","PlayerCallbacks","ResetStaticsOnLoad"],["UnityEngine","InputSystem","Plugins","InputForUI","InputSystemProvider","Bootstrap"],["UnityEngine","InputSystem","EnhancedTouch","Touch","InitializeGlobalTouchState"],["UnityEngine","InputSystem","Users","InputUser","InitializeGlobalUserState"],["UnityEngine","InputSystem","UI","InputSystemUIInputModule","ResetDefaultActions"]]];

Deserializers.typeNameToIdMap = function(){ var i = 0; return Deserializers.types.reduce( function( res, item ) { res[ item ] = i++; return res; }, {} ) }()

