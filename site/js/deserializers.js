var Deserializers = {}
Deserializers["UnityEngine.JointSpring"] = function (request, data, root) {
  var i322 = root || request.c( 'UnityEngine.JointSpring' )
  var i323 = data
  i322.spring = i323[0]
  i322.damper = i323[1]
  i322.targetPosition = i323[2]
  return i322
}

Deserializers["UnityEngine.JointMotor"] = function (request, data, root) {
  var i324 = root || request.c( 'UnityEngine.JointMotor' )
  var i325 = data
  i324.m_TargetVelocity = i325[0]
  i324.m_Force = i325[1]
  i324.m_FreeSpin = i325[2]
  return i324
}

Deserializers["UnityEngine.JointLimits"] = function (request, data, root) {
  var i326 = root || request.c( 'UnityEngine.JointLimits' )
  var i327 = data
  i326.m_Min = i327[0]
  i326.m_Max = i327[1]
  i326.m_Bounciness = i327[2]
  i326.m_BounceMinVelocity = i327[3]
  i326.m_ContactDistance = i327[4]
  i326.minBounce = i327[5]
  i326.maxBounce = i327[6]
  return i326
}

Deserializers["UnityEngine.JointDrive"] = function (request, data, root) {
  var i328 = root || request.c( 'UnityEngine.JointDrive' )
  var i329 = data
  i328.m_PositionSpring = i329[0]
  i328.m_PositionDamper = i329[1]
  i328.m_MaximumForce = i329[2]
  i328.m_UseAcceleration = i329[3]
  return i328
}

Deserializers["UnityEngine.SoftJointLimitSpring"] = function (request, data, root) {
  var i330 = root || request.c( 'UnityEngine.SoftJointLimitSpring' )
  var i331 = data
  i330.m_Spring = i331[0]
  i330.m_Damper = i331[1]
  return i330
}

Deserializers["UnityEngine.SoftJointLimit"] = function (request, data, root) {
  var i332 = root || request.c( 'UnityEngine.SoftJointLimit' )
  var i333 = data
  i332.m_Limit = i333[0]
  i332.m_Bounciness = i333[1]
  i332.m_ContactDistance = i333[2]
  return i332
}

Deserializers["UnityEngine.WheelFrictionCurve"] = function (request, data, root) {
  var i334 = root || request.c( 'UnityEngine.WheelFrictionCurve' )
  var i335 = data
  i334.m_ExtremumSlip = i335[0]
  i334.m_ExtremumValue = i335[1]
  i334.m_AsymptoteSlip = i335[2]
  i334.m_AsymptoteValue = i335[3]
  i334.m_Stiffness = i335[4]
  return i334
}

Deserializers["UnityEngine.JointAngleLimits2D"] = function (request, data, root) {
  var i336 = root || request.c( 'UnityEngine.JointAngleLimits2D' )
  var i337 = data
  i336.m_LowerAngle = i337[0]
  i336.m_UpperAngle = i337[1]
  return i336
}

Deserializers["UnityEngine.JointMotor2D"] = function (request, data, root) {
  var i338 = root || request.c( 'UnityEngine.JointMotor2D' )
  var i339 = data
  i338.m_MotorSpeed = i339[0]
  i338.m_MaximumMotorTorque = i339[1]
  return i338
}

Deserializers["UnityEngine.JointSuspension2D"] = function (request, data, root) {
  var i340 = root || request.c( 'UnityEngine.JointSuspension2D' )
  var i341 = data
  i340.m_DampingRatio = i341[0]
  i340.m_Frequency = i341[1]
  i340.m_Angle = i341[2]
  return i340
}

Deserializers["UnityEngine.JointTranslationLimits2D"] = function (request, data, root) {
  var i342 = root || request.c( 'UnityEngine.JointTranslationLimits2D' )
  var i343 = data
  i342.m_LowerTranslation = i343[0]
  i342.m_UpperTranslation = i343[1]
  return i342
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Material"] = function (request, data, root) {
  var i344 = root || new pc.UnityMaterial()
  var i345 = data
  i344.name = i345[0]
  request.r(i345[1], i345[2], 0, i344, 'shader')
  i344.renderQueue = i345[3]
  i344.enableInstancing = !!i345[4]
  var i347 = i345[5]
  var i346 = []
  for(var i = 0; i < i347.length; i += 1) {
    i346.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Material+FloatParameter', i347[i + 0]) );
  }
  i344.floatParameters = i346
  var i349 = i345[6]
  var i348 = []
  for(var i = 0; i < i349.length; i += 1) {
    i348.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Material+ColorParameter', i349[i + 0]) );
  }
  i344.colorParameters = i348
  var i351 = i345[7]
  var i350 = []
  for(var i = 0; i < i351.length; i += 1) {
    i350.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Material+VectorParameter', i351[i + 0]) );
  }
  i344.vectorParameters = i350
  var i353 = i345[8]
  var i352 = []
  for(var i = 0; i < i353.length; i += 1) {
    i352.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Material+TextureParameter', i353[i + 0]) );
  }
  i344.textureParameters = i352
  var i355 = i345[9]
  var i354 = []
  for(var i = 0; i < i355.length; i += 1) {
    i354.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Material+MaterialFlag', i355[i + 0]) );
  }
  i344.materialFlags = i354
  return i344
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Material+FloatParameter"] = function (request, data, root) {
  var i358 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Material+FloatParameter' )
  var i359 = data
  i358.name = i359[0]
  i358.value = i359[1]
  return i358
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Material+ColorParameter"] = function (request, data, root) {
  var i362 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Material+ColorParameter' )
  var i363 = data
  i362.name = i363[0]
  i362.value = new pc.Color(i363[1], i363[2], i363[3], i363[4])
  return i362
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Material+VectorParameter"] = function (request, data, root) {
  var i366 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Material+VectorParameter' )
  var i367 = data
  i366.name = i367[0]
  i366.value = new pc.Vec4( i367[1], i367[2], i367[3], i367[4] )
  return i366
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Material+TextureParameter"] = function (request, data, root) {
  var i370 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Material+TextureParameter' )
  var i371 = data
  i370.name = i371[0]
  request.r(i371[1], i371[2], 0, i370, 'value')
  return i370
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Material+MaterialFlag"] = function (request, data, root) {
  var i374 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Material+MaterialFlag' )
  var i375 = data
  i374.name = i375[0]
  i374.enabled = !!i375[1]
  return i374
}

Deserializers["Luna.Unity.DTO.UnityEngine.Textures.Texture2D"] = function (request, data, root) {
  var i376 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Textures.Texture2D' )
  var i377 = data
  i376.name = i377[0]
  i376.width = i377[1]
  i376.height = i377[2]
  i376.mipmapCount = i377[3]
  i376.anisoLevel = i377[4]
  i376.filterMode = i377[5]
  i376.hdr = !!i377[6]
  i376.format = i377[7]
  i376.wrapMode = i377[8]
  i376.alphaIsTransparency = !!i377[9]
  i376.alphaSource = i377[10]
  i376.graphicsFormat = i377[11]
  i376.sRGBTexture = !!i377[12]
  i376.desiredColorSpace = i377[13]
  i376.wrapU = i377[14]
  i376.wrapV = i377[15]
  return i376
}

Deserializers["Luna.Unity.DTO.UnityEngine.Scene.Scene"] = function (request, data, root) {
  var i378 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Scene.Scene' )
  var i379 = data
  i378.name = i379[0]
  i378.index = i379[1]
  i378.startup = !!i379[2]
  return i378
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.Camera"] = function (request, data, root) {
  var i380 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.Camera' )
  var i381 = data
  i380.aspect = i381[0]
  i380.orthographic = !!i381[1]
  i380.orthographicSize = i381[2]
  i380.backgroundColor = new pc.Color(i381[3], i381[4], i381[5], i381[6])
  i380.nearClipPlane = i381[7]
  i380.farClipPlane = i381[8]
  i380.fieldOfView = i381[9]
  i380.depth = i381[10]
  i380.clearFlags = i381[11]
  i380.cullingMask = i381[12]
  i380.rect = i381[13]
  request.r(i381[14], i381[15], 0, i380, 'targetTexture')
  i380.usePhysicalProperties = !!i381[16]
  i380.focalLength = i381[17]
  i380.sensorSize = new pc.Vec2( i381[18], i381[19] )
  i380.lensShift = new pc.Vec2( i381[20], i381[21] )
  i380.gateFit = i381[22]
  i380.commandBufferCount = i381[23]
  i380.cameraType = i381[24]
  i380.enabled = !!i381[25]
  return i380
}

Deserializers["UnityEngine.Rendering.Universal.UniversalAdditionalCameraData"] = function (request, data, root) {
  var i382 = root || request.c( 'UnityEngine.Rendering.Universal.UniversalAdditionalCameraData' )
  var i383 = data
  i382.m_RenderShadows = !!i383[0]
  i382.m_RequiresDepthTextureOption = i383[1]
  i382.m_RequiresOpaqueTextureOption = i383[2]
  i382.m_CameraType = i383[3]
  var i385 = i383[4]
  var i384 = new (System.Collections.Generic.List$1(Bridge.ns('UnityEngine.Camera')))
  for(var i = 0; i < i385.length; i += 2) {
  request.r(i385[i + 0], i385[i + 1], 1, i384, '')
  }
  i382.m_Cameras = i384
  i382.m_RendererIndex = i383[5]
  i382.m_VolumeLayerMask = UnityEngine.LayerMask.FromIntegerValue( i383[6] )
  request.r(i383[7], i383[8], 0, i382, 'm_VolumeTrigger')
  i382.m_VolumeFrameworkUpdateModeOption = i383[9]
  i382.m_RenderPostProcessing = !!i383[10]
  i382.m_Antialiasing = i383[11]
  i382.m_AntialiasingQuality = i383[12]
  i382.m_StopNaN = !!i383[13]
  i382.m_Dithering = !!i383[14]
  i382.m_ClearDepth = !!i383[15]
  i382.m_AllowXRRendering = !!i383[16]
  i382.m_AllowHDROutput = !!i383[17]
  i382.m_UseScreenCoordOverride = !!i383[18]
  i382.m_ScreenSizeOverride = new pc.Vec4( i383[19], i383[20], i383[21], i383[22] )
  i382.m_ScreenCoordScaleBias = new pc.Vec4( i383[23], i383[24], i383[25], i383[26] )
  i382.m_RequiresDepthTexture = !!i383[27]
  i382.m_RequiresColorTexture = !!i383[28]
  i382.m_Version = i383[29]
  i382.m_TaaSettings = request.d('UnityEngine.Rendering.Universal.TemporalAA+Settings', i383[30], i382.m_TaaSettings)
  return i382
}

Deserializers["UnityEngine.Rendering.Universal.TemporalAA+Settings"] = function (request, data, root) {
  var i388 = root || request.c( 'UnityEngine.Rendering.Universal.TemporalAA+Settings' )
  var i389 = data
  i388.m_Quality = i389[0]
  i388.m_FrameInfluence = i389[1]
  i388.m_JitterScale = i389[2]
  i388.m_MipBias = i389[3]
  i388.m_VarianceClampScale = i389[4]
  i388.m_ContrastAdaptiveSharpening = i389[5]
  return i388
}

Deserializers["Luna.Unity.DTO.UnityEngine.Scene.GameObject"] = function (request, data, root) {
  var i390 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Scene.GameObject' )
  var i391 = data
  i390.name = i391[0]
  i390.tagId = i391[1]
  i390.enabled = !!i391[2]
  i390.isStatic = !!i391[3]
  i390.layer = i391[4]
  return i390
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.MeshFilter"] = function (request, data, root) {
  var i392 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.MeshFilter' )
  var i393 = data
  request.r(i393[0], i393[1], 0, i392, 'sharedMesh')
  return i392
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.MeshRenderer"] = function (request, data, root) {
  var i394 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.MeshRenderer' )
  var i395 = data
  request.r(i395[0], i395[1], 0, i394, 'additionalVertexStreams')
  i394.enabled = !!i395[2]
  request.r(i395[3], i395[4], 0, i394, 'sharedMaterial')
  var i397 = i395[5]
  var i396 = []
  for(var i = 0; i < i397.length; i += 2) {
  request.r(i397[i + 0], i397[i + 1], 2, i396, '')
  }
  i394.sharedMaterials = i396
  i394.receiveShadows = !!i395[6]
  i394.shadowCastingMode = i395[7]
  i394.sortingLayerID = i395[8]
  i394.sortingOrder = i395[9]
  i394.lightmapIndex = i395[10]
  i394.lightmapSceneIndex = i395[11]
  i394.lightmapScaleOffset = new pc.Vec4( i395[12], i395[13], i395[14], i395[15] )
  i394.lightProbeUsage = i395[16]
  i394.reflectionProbeUsage = i395[17]
  return i394
}

Deserializers["CastleDestroyers.Core.GameBootstrap"] = function (request, data, root) {
  var i400 = root || request.c( 'CastleDestroyers.Core.GameBootstrap' )
  var i401 = data
  request.r(i401[0], i401[1], 0, i400, 'config')
  request.r(i401[2], i401[3], 0, i400, 'mainCamera')
  var i403 = i401[4]
  var i402 = []
  for(var i = 0; i < i403.length; i += 2) {
  request.r(i403[i + 0], i403[i + 1], 2, i402, '')
  }
  i400.backFilters = i402
  request.r(i401[5], i401[6], 0, i400, 'decorFilter')
  request.r(i401[7], i401[8], 0, i400, 'frontFilter')
  request.r(i401[9], i401[10], 0, i400, 'waveFilter')
  request.r(i401[11], i401[12], 0, i400, 'cannonsFilter')
  var i405 = i401[13]
  var i404 = []
  for(var i = 0; i < i405.length; i += 2) {
  request.r(i405[i + 0], i405[i + 1], 2, i404, '')
  }
  i400.castleFilters = i404
  request.r(i401[14], i401[15], 0, i400, 'undercarriageFilter')
  request.r(i401[16], i401[17], 0, i400, 'particlesFilter')
  request.r(i401[18], i401[19], 0, i400, 'smokeFilter')
  request.r(i401[20], i401[21], 0, i400, 'rocketsFilter')
  request.r(i401[22], i401[23], 0, i400, 'boostersFilter')
  request.r(i401[24], i401[25], 0, i400, 'explosionsFilter')
  request.r(i401[26], i401[27], 0, i400, 'aimDotsFilter')
  request.r(i401[28], i401[29], 0, i400, 'aimPanel')
  request.r(i401[30], i401[31], 0, i400, 'aimPanelBackground')
  request.r(i401[32], i401[33], 0, i400, 'aimPowerText')
  request.r(i401[34], i401[35], 0, i400, 'aimAngleText')
  var i407 = i401[36]
  var i406 = []
  for(var i = 0; i < i407.length; i += 2) {
  request.r(i407[i + 0], i407[i + 1], 2, i406, '')
  }
  i400.aimCaptions = i406
  request.r(i401[37], i401[38], 0, i400, 'aimDegree')
  request.r(i401[39], i401[40], 0, i400, 'hudFilter')
  request.r(i401[41], i401[42], 0, i400, 'gameCanvas')
  request.r(i401[43], i401[44], 0, i400, 'bannerRoot')
  request.r(i401[45], i401[46], 0, i400, 'bannerBackground')
  request.r(i401[47], i401[48], 0, i400, 'bannerText')
  request.r(i401[49], i401[50], 0, i400, 'handRoot')
  var i409 = i401[51]
  var i408 = []
  for(var i = 0; i < i409.length; i += 2) {
  request.r(i409[i + 0], i409[i + 1], 2, i408, '')
  }
  i400.handImages = i408
  request.r(i401[52], i401[53], 0, i400, 'endCardRoot')
  request.r(i401[54], i401[55], 0, i400, 'endCardDim')
  request.r(i401[56], i401[57], 0, i400, 'endCardContent')
  request.r(i401[58], i401[59], 0, i400, 'endCardButton')
  request.r(i401[60], i401[61], 0, i400, 'endCardIcon')
  request.r(i401[62], i401[63], 0, i400, 'endCardCaption')
  request.r(i401[64], i401[65], 0, i400, 'endCardButtonFace')
  request.r(i401[66], i401[67], 0, i400, 'endCardButtonEdge')
  request.r(i401[68], i401[69], 0, i400, 'endCardButtonText')
  request.r(i401[70], i401[71], 0, i400, 'endCardDisc')
  request.r(i401[72], i401[73], 0, i400, 'endCardBlueCastle')
  request.r(i401[74], i401[75], 0, i400, 'endCardRedCastle')
  var i411 = i401[76]
  var i410 = []
  for(var i = 0; i < i411.length; i += 2) {
  request.r(i411[i + 0], i411[i + 1], 2, i410, '')
  }
  i400.endCardLayout = i410
  var i413 = i401[77]
  var i412 = []
  for(var i = 0; i < i413.length; i += 2) {
  request.r(i413[i + 0], i413[i + 1], 2, i412, '')
  }
  i400.soundClips = i412
  var i415 = i401[78]
  var i414 = []
  for(var i = 0; i < i415.length; i += 2) {
  request.r(i415[i + 0], i415[i + 1], 2, i414, '')
  }
  i400.soundSources = i414
  request.r(i401[79], i401[80], 0, i400, 'terrainRoot')
  request.r(i401[81], i401[82], 0, i400, 'terrainMaterial')
  request.r(i401[83], i401[84], 0, i400, 'flatMaterial')
  request.r(i401[85], i401[86], 0, i400, 'rocketMaterial')
  request.r(i401[87], i401[88], 0, i400, 'castleMaterial')
  i400.terrainSortingOrder = i401[89]
  i400.chunkSortingOrder = i401[90]
  i400.castleChunkSortingOrder = i401[91]
  request.r(i401[92], i401[93], 0, i400, 'debugRoot')
  request.r(i401[94], i401[95], 0, i400, 'debugOverlay')
  return i400
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.AudioSource"] = function (request, data, root) {
  var i428 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.AudioSource' )
  var i429 = data
  request.r(i429[0], i429[1], 0, i428, 'clip')
  request.r(i429[2], i429[3], 0, i428, 'outputAudioMixerGroup')
  i428.playOnAwake = !!i429[4]
  i428.loop = !!i429[5]
  i428.time = i429[6]
  i428.volume = i429[7]
  i428.pitch = i429[8]
  i428.enabled = !!i429[9]
  return i428
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.RectTransform"] = function (request, data, root) {
  var i430 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.RectTransform' )
  var i431 = data
  i430.pivot = new pc.Vec2( i431[0], i431[1] )
  i430.anchorMin = new pc.Vec2( i431[2], i431[3] )
  i430.anchorMax = new pc.Vec2( i431[4], i431[5] )
  i430.sizeDelta = new pc.Vec2( i431[6], i431[7] )
  i430.anchoredPosition3D = new pc.Vec3( i431[8], i431[9], i431[10] )
  i430.rotation = new pc.Quat(i431[11], i431[12], i431[13], i431[14])
  i430.scale = new pc.Vec3( i431[15], i431[16], i431[17] )
  return i430
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.Canvas"] = function (request, data, root) {
  var i432 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.Canvas' )
  var i433 = data
  i432.planeDistance = i433[0]
  i432.referencePixelsPerUnit = i433[1]
  i432.isFallbackOverlay = !!i433[2]
  i432.renderMode = i433[3]
  i432.renderOrder = i433[4]
  i432.sortingLayerName = i433[5]
  i432.sortingOrder = i433[6]
  i432.scaleFactor = i433[7]
  request.r(i433[8], i433[9], 0, i432, 'worldCamera')
  i432.overrideSorting = !!i433[10]
  i432.pixelPerfect = !!i433[11]
  i432.targetDisplay = i433[12]
  i432.overridePixelPerfect = !!i433[13]
  i432.enabled = !!i433[14]
  return i432
}

Deserializers["UnityEngine.UI.CanvasScaler"] = function (request, data, root) {
  var i434 = root || request.c( 'UnityEngine.UI.CanvasScaler' )
  var i435 = data
  i434.m_UiScaleMode = i435[0]
  i434.m_ReferencePixelsPerUnit = i435[1]
  i434.m_ScaleFactor = i435[2]
  i434.m_ReferenceResolution = new pc.Vec2( i435[3], i435[4] )
  i434.m_ScreenMatchMode = i435[5]
  i434.m_MatchWidthOrHeight = i435[6]
  i434.m_PhysicalUnit = i435[7]
  i434.m_FallbackScreenDPI = i435[8]
  i434.m_DefaultSpriteDPI = i435[9]
  i434.m_DynamicPixelsPerUnit = i435[10]
  i434.m_PresetInfoIsWorld = !!i435[11]
  return i434
}

Deserializers["Luna.Unity.DTO.UnityEngine.Components.CanvasRenderer"] = function (request, data, root) {
  var i436 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Components.CanvasRenderer' )
  var i437 = data
  i436.cullTransparentMesh = !!i437[0]
  return i436
}

Deserializers["UnityEngine.UI.Image"] = function (request, data, root) {
  var i438 = root || request.c( 'UnityEngine.UI.Image' )
  var i439 = data
  request.r(i439[0], i439[1], 0, i438, 'm_Sprite')
  i438.m_Type = i439[2]
  i438.m_PreserveAspect = !!i439[3]
  i438.m_FillCenter = !!i439[4]
  i438.m_FillMethod = i439[5]
  i438.m_FillAmount = i439[6]
  i438.m_FillClockwise = !!i439[7]
  i438.m_FillOrigin = i439[8]
  i438.m_UseSpriteMesh = !!i439[9]
  i438.m_PixelsPerUnitMultiplier = i439[10]
  request.r(i439[11], i439[12], 0, i438, 'm_Material')
  i438.m_Maskable = !!i439[13]
  i438.m_Color = new pc.Color(i439[14], i439[15], i439[16], i439[17])
  i438.m_RaycastTarget = !!i439[18]
  i438.m_RaycastPadding = new pc.Vec4( i439[19], i439[20], i439[21], i439[22] )
  return i438
}

Deserializers["UnityEngine.UI.Text"] = function (request, data, root) {
  var i440 = root || request.c( 'UnityEngine.UI.Text' )
  var i441 = data
  i440.m_FontData = request.d('UnityEngine.UI.FontData', i441[0], i440.m_FontData)
  i440.m_Text = i441[1]
  request.r(i441[2], i441[3], 0, i440, 'm_Material')
  i440.m_Maskable = !!i441[4]
  i440.m_Color = new pc.Color(i441[5], i441[6], i441[7], i441[8])
  i440.m_RaycastTarget = !!i441[9]
  i440.m_RaycastPadding = new pc.Vec4( i441[10], i441[11], i441[12], i441[13] )
  return i440
}

Deserializers["UnityEngine.UI.FontData"] = function (request, data, root) {
  var i442 = root || request.c( 'UnityEngine.UI.FontData' )
  var i443 = data
  request.r(i443[0], i443[1], 0, i442, 'm_Font')
  i442.m_FontSize = i443[2]
  i442.m_FontStyle = i443[3]
  i442.m_BestFit = !!i443[4]
  i442.m_MinSize = i443[5]
  i442.m_MaxSize = i443[6]
  i442.m_Alignment = i443[7]
  i442.m_AlignByGeometry = !!i443[8]
  i442.m_RichText = !!i443[9]
  i442.m_HorizontalOverflow = i443[10]
  i442.m_VerticalOverflow = i443[11]
  i442.m_LineSpacing = i443[12]
  return i442
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderSettings"] = function (request, data, root) {
  var i444 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.RenderSettings' )
  var i445 = data
  i444.ambientIntensity = i445[0]
  i444.reflectionIntensity = i445[1]
  i444.ambientMode = i445[2]
  i444.ambientLight = new pc.Color(i445[3], i445[4], i445[5], i445[6])
  i444.ambientSkyColor = new pc.Color(i445[7], i445[8], i445[9], i445[10])
  i444.ambientGroundColor = new pc.Color(i445[11], i445[12], i445[13], i445[14])
  i444.ambientEquatorColor = new pc.Color(i445[15], i445[16], i445[17], i445[18])
  i444.fogColor = new pc.Color(i445[19], i445[20], i445[21], i445[22])
  i444.fogEndDistance = i445[23]
  i444.fogStartDistance = i445[24]
  i444.fogDensity = i445[25]
  i444.fog = !!i445[26]
  request.r(i445[27], i445[28], 0, i444, 'skybox')
  i444.fogMode = i445[29]
  var i447 = i445[30]
  var i446 = []
  for(var i = 0; i < i447.length; i += 1) {
    i446.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+Lightmap', i447[i + 0]) );
  }
  i444.lightmaps = i446
  i444.lightProbes = request.d('Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+LightProbes', i445[31], i444.lightProbes)
  i444.lightmapsMode = i445[32]
  i444.mixedBakeMode = i445[33]
  i444.environmentLightingMode = i445[34]
  i444.ambientProbe = new pc.SphericalHarmonicsL2(i445[35])
  request.r(i445[36], i445[37], 0, i444, 'customReflection')
  request.r(i445[38], i445[39], 0, i444, 'defaultReflection')
  i444.defaultReflectionMode = i445[40]
  i444.defaultReflectionResolution = i445[41]
  i444.sunLightObjectId = i445[42]
  i444.pixelLightCount = i445[43]
  i444.defaultReflectionHDR = !!i445[44]
  i444.hasLightDataAsset = !!i445[45]
  i444.hasManualGenerate = !!i445[46]
  return i444
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+Lightmap"] = function (request, data, root) {
  var i450 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+Lightmap' )
  var i451 = data
  request.r(i451[0], i451[1], 0, i450, 'lightmapColor')
  request.r(i451[2], i451[3], 0, i450, 'lightmapDirection')
  request.r(i451[4], i451[5], 0, i450, 'shadowMask')
  return i450
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+LightProbes"] = function (request, data, root) {
  var i452 = root || new UnityEngine.LightProbes()
  var i453 = data
  return i452
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.UniversalRenderPipelineAsset"] = function (request, data, root) {
  var i460 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.UniversalRenderPipelineAsset' )
  var i461 = data
  i460.AdditionalLightsRenderingMode = i461[0]
  i460.LightRenderingMode = request.d('Luna.Unity.DTO.UnityEngine.Assets.LightRenderingMode', i461[1], i460.LightRenderingMode)
  i460.MainLightRenderingModeValue = i461[2]
  i460.SupportsMainLightShadows = !!i461[3]
  i460.MixedLightingSupported = !!i461[4]
  i460.MainLightShadowmapResolutionValue = i461[5]
  i460.SupportsSoftShadows = !!i461[6]
  i460.SoftShadowQualityValue = i461[7]
  i460.ShadowDistance = i461[8]
  i460.ShadowCascadeCount = i461[9]
  i460.Cascade2Split = i461[10]
  i460.Cascade3Split = new pc.Vec2( i461[11], i461[12] )
  i460.Cascade4Split = new pc.Vec3( i461[13], i461[14], i461[15] )
  i460.CascadeBorder = i461[16]
  i460.ShadowDepthBias = i461[17]
  i460.ShadowNormalBias = i461[18]
  i460.RequireDepthTexture = !!i461[19]
  i460.RequireOpaqueTexture = !!i461[20]
  i460.scriptableRendererData = request.d('Luna.Unity.DTO.UnityEngine.Assets.ScriptableRendererData', i461[21], i460.scriptableRendererData)
  return i460
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.LightRenderingMode"] = function (request, data, root) {
  var i462 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.LightRenderingMode' )
  var i463 = data
  i462.Disabled = i463[0]
  i462.PerVertex = i463[1]
  i462.PerPixel = i463[2]
  return i462
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ScriptableRendererData"] = function (request, data, root) {
  var i464 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ScriptableRendererData' )
  var i465 = data
  i464.opaqueLayerMask = i465[0]
  i464.transparentLayerMask = i465[1]
  var i467 = i465[2]
  var i466 = []
  for(var i = 0; i < i467.length; i += 1) {
    i466.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.RenderObjects', i467[i + 0]) );
  }
  i464.RenderObjectsFeatures = i466
  i464.name = i465[3]
  return i464
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderObjects"] = function (request, data, root) {
  var i470 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.RenderObjects' )
  var i471 = data
  i470.settings = request.d('Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+RenderObjectsSettings', i471[0], i470.settings)
  i470.name = i471[1]
  i470.typeName = i471[2]
  return i470
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader"] = function (request, data, root) {
  var i472 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader' )
  var i473 = data
  var i475 = i473[0]
  var i474 = new (System.Collections.Generic.List$1(Bridge.ns('Luna.Unity.DTO.UnityEngine.Assets.Shader+ShaderCompilationError')))
  for(var i = 0; i < i475.length; i += 1) {
    i474.add(request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+ShaderCompilationError', i475[i + 0]));
  }
  i472.ShaderCompilationErrors = i474
  i472.name = i473[1]
  i472.guid = i473[2]
  var i477 = i473[3]
  var i476 = []
  for(var i = 0; i < i477.length; i += 1) {
    i476.push( i477[i + 0] );
  }
  i472.shaderDefinedKeywords = i476
  var i479 = i473[4]
  var i478 = []
  for(var i = 0; i < i479.length; i += 1) {
    i478.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass', i479[i + 0]) );
  }
  i472.passes = i478
  var i481 = i473[5]
  var i480 = []
  for(var i = 0; i < i481.length; i += 1) {
    i480.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+UsePass', i481[i + 0]) );
  }
  i472.usePasses = i480
  var i483 = i473[6]
  var i482 = []
  for(var i = 0; i < i483.length; i += 1) {
    i482.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+DefaultParameterValue', i483[i + 0]) );
  }
  i472.defaultParameterValues = i482
  request.r(i473[7], i473[8], 0, i472, 'unityFallbackShader')
  i472.readDepth = !!i473[9]
  i472.hasDepthOnlyPass = !!i473[10]
  i472.isCreatedByShaderGraph = !!i473[11]
  i472.disableBatching = !!i473[12]
  i472.compiled = !!i473[13]
  return i472
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+ShaderCompilationError"] = function (request, data, root) {
  var i486 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+ShaderCompilationError' )
  var i487 = data
  i486.shaderName = i487[0]
  i486.errorMessage = i487[1]
  return i486
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass"] = function (request, data, root) {
  var i492 = root || new pc.UnityShaderPass()
  var i493 = data
  i492.id = i493[0]
  i492.subShaderIndex = i493[1]
  i492.name = i493[2]
  i492.passType = i493[3]
  i492.grabPassTextureName = i493[4]
  i492.usePass = !!i493[5]
  i492.zTest = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i493[6], i492.zTest)
  i492.zWrite = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i493[7], i492.zWrite)
  i492.culling = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i493[8], i492.culling)
  i492.blending = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Blending', i493[9], i492.blending)
  i492.alphaBlending = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Blending', i493[10], i492.alphaBlending)
  i492.colorWriteMask = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i493[11], i492.colorWriteMask)
  i492.offsetUnits = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i493[12], i492.offsetUnits)
  i492.offsetFactor = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i493[13], i492.offsetFactor)
  i492.stencilRef = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i493[14], i492.stencilRef)
  i492.stencilReadMask = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i493[15], i492.stencilReadMask)
  i492.stencilWriteMask = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i493[16], i492.stencilWriteMask)
  i492.stencilOp = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+StencilOp', i493[17], i492.stencilOp)
  i492.stencilOpFront = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+StencilOp', i493[18], i492.stencilOpFront)
  i492.stencilOpBack = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+StencilOp', i493[19], i492.stencilOpBack)
  var i495 = i493[20]
  var i494 = []
  for(var i = 0; i < i495.length; i += 1) {
    i494.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Tag', i495[i + 0]) );
  }
  i492.tags = i494
  var i497 = i493[21]
  var i496 = []
  for(var i = 0; i < i497.length; i += 1) {
    i496.push( i497[i + 0] );
  }
  i492.passDefinedKeywords = i496
  var i499 = i493[22]
  var i498 = []
  for(var i = 0; i < i499.length; i += 1) {
    i498.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+KeywordGroup', i499[i + 0]) );
  }
  i492.passDefinedKeywordGroups = i498
  var i501 = i493[23]
  var i500 = []
  for(var i = 0; i < i501.length; i += 1) {
    i500.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Variant', i501[i + 0]) );
  }
  i492.variants = i500
  var i503 = i493[24]
  var i502 = []
  for(var i = 0; i < i503.length; i += 1) {
    i502.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Variant', i503[i + 0]) );
  }
  i492.excludedVariants = i502
  i492.hasDepthReader = !!i493[25]
  return i492
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value"] = function (request, data, root) {
  var i504 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value' )
  var i505 = data
  i504.val = i505[0]
  i504.name = i505[1]
  return i504
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Blending"] = function (request, data, root) {
  var i506 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Blending' )
  var i507 = data
  i506.src = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i507[0], i506.src)
  i506.dst = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i507[1], i506.dst)
  i506.op = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i507[2], i506.op)
  return i506
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+StencilOp"] = function (request, data, root) {
  var i508 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+StencilOp' )
  var i509 = data
  i508.pass = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i509[0], i508.pass)
  i508.fail = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i509[1], i508.fail)
  i508.zFail = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i509[2], i508.zFail)
  i508.comp = request.d('Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value', i509[3], i508.comp)
  return i508
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Tag"] = function (request, data, root) {
  var i512 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Tag' )
  var i513 = data
  i512.name = i513[0]
  i512.value = i513[1]
  return i512
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+KeywordGroup"] = function (request, data, root) {
  var i516 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+KeywordGroup' )
  var i517 = data
  var i519 = i517[0]
  var i518 = []
  for(var i = 0; i < i519.length; i += 1) {
    i518.push( i519[i + 0] );
  }
  i516.keywords = i518
  i516.hasDiscard = !!i517[1]
  return i516
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Variant"] = function (request, data, root) {
  var i522 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Variant' )
  var i523 = data
  i522.passId = i523[0]
  i522.subShaderIndex = i523[1]
  var i525 = i523[2]
  var i524 = []
  for(var i = 0; i < i525.length; i += 1) {
    i524.push( i525[i + 0] );
  }
  i522.keywords = i524
  i522.vertexProgram = i523[3]
  i522.fragmentProgram = i523[4]
  i522.exportedForWebGl2 = !!i523[5]
  i522.readDepth = !!i523[6]
  return i522
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+UsePass"] = function (request, data, root) {
  var i528 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+UsePass' )
  var i529 = data
  request.r(i529[0], i529[1], 0, i528, 'shader')
  i528.pass = i529[2]
  return i528
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Shader+DefaultParameterValue"] = function (request, data, root) {
  var i532 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Shader+DefaultParameterValue' )
  var i533 = data
  i532.name = i533[0]
  i532.type = i533[1]
  i532.value = new pc.Vec4( i533[2], i533[3], i533[4], i533[5] )
  i532.textureValue = i533[6]
  i532.shaderPropertyFlag = i533[7]
  return i532
}

Deserializers["Luna.Unity.DTO.UnityEngine.Textures.Sprite"] = function (request, data, root) {
  var i534 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Textures.Sprite' )
  var i535 = data
  i534.name = i535[0]
  request.r(i535[1], i535[2], 0, i534, 'texture')
  i534.aabb = i535[3]
  i534.vertices = i535[4]
  i534.triangles = i535[5]
  i534.textureRect = UnityEngine.Rect.MinMaxRect(i535[6], i535[7], i535[8], i535[9])
  i534.packedRect = UnityEngine.Rect.MinMaxRect(i535[10], i535[11], i535[12], i535[13])
  i534.border = new pc.Vec4( i535[14], i535[15], i535[16], i535[17] )
  i534.transparency = i535[18]
  i534.bounds = i535[19]
  i534.pixelsPerUnit = i535[20]
  i534.textureWidth = i535[21]
  i534.textureHeight = i535[22]
  i534.nativeSize = new pc.Vec2( i535[23], i535[24] )
  i534.pivot = new pc.Vec2( i535[25], i535[26] )
  i534.textureRectOffset = new pc.Vec2( i535[27], i535[28] )
  return i534
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.AudioClip"] = function (request, data, root) {
  var i536 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.AudioClip' )
  var i537 = data
  i536.name = i537[0]
  return i536
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Font"] = function (request, data, root) {
  var i538 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Font' )
  var i539 = data
  i538.name = i539[0]
  i538.ascent = i539[1]
  i538.originalLineHeight = i539[2]
  i538.fontSize = i539[3]
  var i541 = i539[4]
  var i540 = []
  for(var i = 0; i < i541.length; i += 1) {
    i540.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Font+CharacterInfo', i541[i + 0]) );
  }
  i538.characterInfo = i540
  request.r(i539[5], i539[6], 0, i538, 'texture')
  i538.originalFontSize = i539[7]
  return i538
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Font+CharacterInfo"] = function (request, data, root) {
  var i544 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Font+CharacterInfo' )
  var i545 = data
  i544.index = i545[0]
  i544.advance = i545[1]
  i544.bearing = i545[2]
  i544.glyphWidth = i545[3]
  i544.glyphHeight = i545[4]
  i544.minX = i545[5]
  i544.maxX = i545[6]
  i544.minY = i545[7]
  i544.maxY = i545[8]
  i544.uvBottomLeftX = i545[9]
  i544.uvBottomLeftY = i545[10]
  i544.uvBottomRightX = i545[11]
  i544.uvBottomRightY = i545[12]
  i544.uvTopLeftX = i545[13]
  i544.uvTopLeftY = i545[14]
  i544.uvTopRightX = i545[15]
  i544.uvTopRightY = i545[16]
  return i544
}

Deserializers["CastleDestroyers.Core.GameConfig"] = function (request, data, root) {
  var i546 = root || request.c( 'CastleDestroyers.Core.GameConfig' )
  var i547 = data
  request.r(i547[0], i547[1], 0, i546, 'theme')
  i546.rngSeed = i547[2]
  i546.maxPlayerTurns = i547[3]
  i546.maxSessionSeconds = i547[4]
  i546.maxDeltaTime = i547[5]
  i546.directorEnabled = !!i547[6]
  i546.directorMaxLossPerAiTurn = i547[7]
  i546.directorEnemyKeepCannons = i547[8]
  i546.directorFinisherWeight = i547[9]
  i546.bonusPickEnabled = !!i547[10]
  var i549 = i547[11]
  var i548 = []
  for(var i = 0; i < i549.length; i += 1) {
    i548.push( request.d('CastleDestroyers.Core.TurnHint', i549[i + 0]) );
  }
  i546.turnHints = i548
  i546.bootDuration = i547[12]
  i546.introDuration = i547[13]
  i546.playerAimDuration = i547[14]
  i546.aiAimDuration = i547[15]
  i546.fireDuration = i547[16]
  i546.flightDuration = i547[17]
  i546.flightMaxDuration = i547[18]
  i546.resolveDuration = i547[19]
  i546.victoryDuration = i547[20]
  i546.victoryLoserHold = i547[21]
  i546.defeatDuration = i547[22]
  i546.victoryFlightCap = i547[23]
  var i551 = i547[24]
  var i550 = []
  for(var i = 0; i < i551.length; i += 1) {
    i550.push( request.d('CastleDestroyers.Core.AiShotPreset', i551[i + 0]) );
  }
  i546.aiPresets = i550
  i546.aiPresetOrder = i547[25]
  i546.aiAngleNoiseDeg = i547[26]
  i546.aiPowerNoise = i547[27]
  i546.aiDecoyDeg = i547[28]
  i546.aiDecoyShare = i547[29]
  i546.aiTurnSpeed = i547[30]
  i546.handStartDelay = i547[31]
  i546.handDemoDuration = i547[32]
  i546.handDemoRepeats = i547[33]
  i546.handRepeatDelay = i547[34]
  i546.hintPanDuration = i547[35]
  i546.handStartViewport = new pc.Vec2( i547[36], i547[37] )
  i546.boosterHighlightPulse = i547[38]
  i546.boosterHighlightSpeed = i547[39]
  i546.hudFlashTime = i547[40]
  i546.hudShakeAmplitude = i547[41]
  i546.hudShakeTime = i547[42]
  i546.hudShakeFrequency = i547[43]
  i546.bannerDuration = i547[44]
  i546.uiPopTime = i547[45]
  i546.endCardFadeTime = i547[46]
  i546.endCardDimAlpha = i547[47]
  i546.hudLandscapeScale = i547[48]
  i546.endCardTapDelay = i547[49]
  i546.ctaPulse = i547[50]
  i546.ctaPulseSpeed = i547[51]
  i546.victoryJumpHeight = i547[52]
  i546.victoryJumpSpeed = i547[53]
  i546.victoryJumpPhase = i547[54]
  i546.fireworkBursts = i547[55]
  i546.fireworkInterval = i547[56]
  i546.fireworkSparks = i547[57]
  i546.fireworkSpeed = i547[58]
  i546.fireworkSparkSize = i547[59]
  i546.fireworkLife = i547[60]
  i546.fireworkGravity = i547[61]
  i546.fireworkLift = i547[62]
  i546.fireworkSpread = i547[63]
  i546.aimMinDeg = i547[64]
  i546.aimMaxDeg = i547[65]
  i546.aimMaxDragScreenFraction = i547[66]
  i546.aimDeadZone = i547[67]
  i546.aimTurnSpeed = i547[68]
  i546.aimPreviewFraction = i547[69]
  i546.aimDotCount = i547[70]
  i546.aimDotSizeStart = i547[71]
  i546.aimDotSizeEnd = i547[72]
  i546.aimDotAlphaEnd = i547[73]
  i546.aimPanelOffset = new pc.Vec2( i547[74], i547[75] )
  i546.aimPanelHudGap = i547[76]
  i546.aimPointAtFinger = !!i547[77]
  i546.aimPointPower = i547[78]
  i546.launchSpeedMin = i547[79]
  i546.launchSpeedMax = i547[80]
  i546.gravity = i547[81]
  i546.playerCannons = i547[82]
  i546.enemyCannons = i547[83]
  i546.rocketsPerCannon = i547[84]
  i546.rocketInterval = i547[85]
  i546.cannonHitRadius = i547[86]
  i546.cannonSupportRadius = i547[87]
  i546.cannonSupportShare = i547[88]
  i546.cannonDropSpeed = i547[89]
  i546.maxActiveRockets = i547[90]
  i546.stressRocketCount = i547[91]
  i546.rocketLength = i547[92]
  i546.rocketWidth = i547[93]
  i546.rocketCollisionStep = i547[94]
  i546.rocketSplashWidth = i547[95]
  i546.recoilDistance = i547[96]
  i546.cannonFireSquash = i547[97]
  i546.cannonDetachSquash = i547[98]
  i546.cannonDetachSquashTime = i547[99]
  i546.cannonJumpSquash = i547[100]
  i546.cannonSquashLengthShare = i547[101]
  i546.recoilReturnSpeed = i547[102]
  i546.muzzleFlashSize = i547[103]
  i546.muzzleFlashLife = i547[104]
  i546.stressRocketAngleDeg = i547[105]
  i546.stressRocketSpeed = i547[106]
  i546.stressSpawnPerFrame = i547[107]
  var i553 = i547[108]
  var i552 = []
  for(var i = 0; i < i553.length; i += 1) {
    i552.push( request.d('CastleDestroyers.Core.BoosterSpec', i553[i + 0]) );
  }
  i546.boosters = i552
  i546.boosterWidth = i547[109]
  i546.boosterHeight = i547[110]
  i546.boosterNudgeReach = i547[111]
  i546.boosterNudgeInset = i547[112]
  i546.boosterNudgeScreenMargin = i547[113]
  i546.boosterSpreadDeg = i547[114]
  i546.boosterSpawnStagger = i547[115]
  i546.boosterStreams = i547[116]
  i546.boosterExitFocus = i547[117]
  i546.boosterMaxCraterScale = i547[118]
  i546.boosterPulse = i547[119]
  i546.boosterPulseSpeed = i547[120]
  i546.boosterSquash = i547[121]
  i546.boosterSquashDecay = i547[122]
  i546.boosterPuffCount = i547[123]
  i546.boosterPuffInterval = i547[124]
  i546.boosterPuffSize = i547[125]
  i546.boosterPuffSpeed = i547[126]
  i546.boosterPuffLife = i547[127]
  i546.boosterPopupHeight = i547[128]
  i546.boosterPopupOffsetY = i547[129]
  i546.boosterPopupHold = i547[130]
  i546.boosterPopupFade = i547[131]
  i546.boosterPopupRise = i547[132]
  request.r(i547[133], i547[134], 0, i546, 'terrainMask')
  i546.terrainMaskOrigin = new pc.Vec2( i547[135], i547[136] )
  i546.maskTexelsPerMeter = i547[137]
  i546.maskTileSize = i547[138]
  i546.maxTileUploadsPerFrame = i547[139]
  i546.craterRadiusGround = i547[140]
  i546.rimWidth = i547[141]
  i546.rimScallop = !!i547[142]
  i546.rimScallopTeeth = i547[143]
  i546.rimScallopDepth = i547[144]
  i546.maskEdgeSoftness = i547[145]
  i546.connectivityInterval = i547[146]
  i546.connectivityCellTexels = i547[147]
  i546.minChunkCells = i547[148]
  i546.maxIslandsPerSolve = i547[149]
  i546.chunkSlots = i547[150]
  i546.chunkTextureSize = i547[151]
  i546.chunkGravity = i547[152]
  i546.chunkMaxDrift = i547[153]
  i546.chunkMaxSpinDeg = i547[154]
  i546.decorMaxSpinDeg = i547[155]
  i546.decorAnchorDepth = i547[156]
  i546.waterWaveAmplitude = i547[157]
  i546.waterWaveFrequency = i547[158]
  i546.waterWaveSpeed = i547[159]
  i546.waterWaveStep = i547[160]
  i546.waterRimThickness = i547[161]
  i546.fxParticleCapacity = i547[162]
  i546.craterDustPuffs = i547[163]
  i546.dustMaxPuffs = i547[164]
  i546.dustSize = i547[165]
  i546.dustSpeed = i547[166]
  i546.dustLife = i547[167]
  i546.dustGravity = i547[168]
  i546.splashDrops = i547[169]
  i546.splashDropsPerMeter = i547[170]
  i546.splashDropSize = i547[171]
  i546.splashSpeed = i547[172]
  i546.splashSpread = i547[173]
  i546.splashLife = i547[174]
  i546.splashGravity = i547[175]
  i546.explosionPoolSize = i547[176]
  i546.explosionMergeRadius = i547[177]
  i546.explosionMergeGrowth = i547[178]
  i546.explosionMaxStrength = i547[179]
  i546.explosionRadius = i547[180]
  i546.explosionLife = i547[181]
  i546.explosionShards = i547[182]
  i546.explosionShardSpeed = i547[183]
  i546.explosionShardSize = i547[184]
  i546.explosionShardLife = i547[185]
  i546.shakeMax = i547[186]
  i546.shakeDecay = i547[187]
  i546.shakePerExplosion = i547[188]
  i546.shakeFrequency = i547[189]
  i546.smokeCapacity = i547[190]
  i546.smokeLodRocketThreshold = i547[191]
  i546.smokeLodGrowth = i547[192]
  i546.smokeInterval = i547[193]
  i546.smokeLife = i547[194]
  i546.smokeSizeStart = i547[195]
  i546.smokeSizeEnd = i547[196]
  i546.smokeFlameFraction = i547[197]
  i546.smokeAlpha = i547[198]
  i546.castleConnectivityCellTexels = i547[199]
  i546.castleLoadTexels = i547[200]
  i546.castleMinChunkCells = i547[201]
  i546.craterRadiusCastle = i547[202]
  i546.castleRimWidth = i547[203]
  i546.castleRimDarken = i547[204]
  i546.castleCraterTeethMin = i547[205]
  i546.castleCraterTeethMax = i547[206]
  i546.castleCraterNoise = i547[207]
  i546.castleCrackChance = i547[208]
  i546.castleCrackCount = i547[209]
  i546.castleCrackLength = i547[210]
  i546.castleCrackWidth = i547[211]
  i546.castleCrackSegments = i547[212]
  i546.castleCrackBendDeg = i547[213]
  i546.castleDustPuffs = i547[214]
  i546.castleChunkSlots = i547[215]
  i546.castleChunkTextureSize = i547[216]
  i546.castleUploadInterval = i547[217]
  i546.platformHitsPerCrack = i547[218]
  i546.castleMaxTiltDeg = i547[219]
  i546.trackMaxTiltDeg = i547[220]
  i546.trackTiltSpeed = i547[221]
  i546.castleSettleSpeed = i547[222]
  i546.swayMaxDeg = i547[223]
  i546.swayFrequency = i547[224]
  i546.swayDamping = i547[225]
  i546.swayImpulseDeg = i547[226]
  i546.swayMaxSpeedDeg = i547[227]
  i546.swayBlastReach = i547[228]
  i546.cannonDetachSpeed = i547[229]
  i546.cannonDetachLift = i547[230]
  i546.cannonDetachSpinDeg = i547[231]
  i546.cannonSplashWidth = i547[232]
  i546.levelWidth = i547[233]
  i546.waterLevel = i547[234]
  i546.levelBottom = i547[235]
  i546.playerCastleX = i547[236]
  i546.enemyCastleX = i547[237]
  i546.trackHeight = i547[238]
  i546.platformHeight = i547[239]
  i546.parallaxFactors = i547[240]
  i546.backBandBase = i547[241]
  i546.moonPosition = new pc.Vec2( i547[242], i547[243] )
  i546.cloudStep = i547[244]
  i546.soundVolume = i547[245]
  i546.soundCooldown = i547[246]
  i546.cameraSmoothTime = i547[247]
  i546.castleFrameWidth = i547[248]
  i546.centerFrameWidth = i547[249]
  i546.centerFrameMinHeight = i547[250]
  i546.centerFrameMaxHeight = i547[251]
  i546.cameraFlightTopMargin = i547[252]
  i546.cameraCenterHold = i547[253]
  i546.targetFrameWidth = i547[254]
  i546.minFrameHeight = i547[255]
  i546.castleLeadX = i547[256]
  i546.castleFocusLiftY = i547[257]
  i546.centerFocus = new pc.Vec2( i547[258], i547[259] )
  i546.cameraHeadFollow = i547[260]
  i546.rainEnabled = !!i547[261]
  i546.rainPerSecond = i547[262]
  i546.rainSpeed = i547[263]
  i546.rainDropSize = i547[264]
  i546.lightBeamEnabled = !!i547[265]
  i546.showPersistentCta = !!i547[266]
  i546.debugSlowScale = i547[267]
  i546.debugHudRefresh = i547[268]
  i546.debugBlastRadii = i547[269]
  i546.debugBlastHoldInterval = i547[270]
  i546.debugBlastUiBand = i547[271]
  i546.debugToggleBand = i547[272]
  i546.debugOverviewWidth = i547[273]
  var i555 = i547[274]
  var i554 = []
  for(var i = 0; i < i555.length; i += 1) {
    i554.push( request.d('CastleDestroyers.Core.TurnHint', i555[i + 0]) );
  }
  i546.debugBoosterShots = i554
  i546.debugBoosterShotSalvos = i547[275]
  return i546
}

Deserializers["CastleDestroyers.Core.TurnHint"] = function (request, data, root) {
  var i558 = root || request.c( 'CastleDestroyers.Core.TurnHint' )
  var i559 = data
  i558.label = i559[0]
  i558.highlightBoosterMultiplier = i559[1]
  i558.showHand = !!i559[2]
  i558.handAngleDeg = i559[3]
  i558.handPower01 = i559[4]
  i558.panToBooster = !!i559[5]
  return i558
}

Deserializers["CastleDestroyers.Core.AiShotPreset"] = function (request, data, root) {
  var i562 = root || request.c( 'CastleDestroyers.Core.AiShotPreset' )
  var i563 = data
  i562.label = i563[0]
  i562.angleDeg = i563[1]
  i562.power01 = i563[2]
  return i562
}

Deserializers["CastleDestroyers.Core.BoosterSpec"] = function (request, data, root) {
  var i566 = root || request.c( 'CastleDestroyers.Core.BoosterSpec' )
  var i567 = data
  i566.multiplier = i567[0]
  i566.position = new pc.Vec2( i567[1], i567[2] )
  return i566
}

Deserializers["CastleDestroyers.Core.ThemePalette"] = function (request, data, root) {
  var i568 = root || request.c( 'CastleDestroyers.Core.ThemePalette' )
  var i569 = data
  i568.preset = i569[0]
  i568.night = request.d('CastleDestroyers.Core.PaletteSet', i569[1], i568.night)
  i568.sunset = request.d('CastleDestroyers.Core.PaletteSet', i569[2], i568.sunset)
  i568.nightArt = request.d('CastleDestroyers.Core.ThemeArt', i569[3], i568.nightArt)
  i568.sunsetArt = request.d('CastleDestroyers.Core.ThemeArt', i569[4], i568.sunsetArt)
  return i568
}

Deserializers["CastleDestroyers.Core.PaletteSet"] = function (request, data, root) {
  var i570 = root || request.c( 'CastleDestroyers.Core.PaletteSet' )
  var i571 = data
  i570.sky = UnityEngine.Color32.ConstructColor(i571[0], i571[1], i571[2], i571[3])
  i570.skyHorizon = UnityEngine.Color32.ConstructColor(i571[4], i571[5], i571[6], i571[7])
  i570.backgroundHaze = UnityEngine.Color32.ConstructColor(i571[8], i571[9], i571[10], i571[11])
  i570.backgroundFar = UnityEngine.Color32.ConstructColor(i571[12], i571[13], i571[14], i571[15])
  i570.backgroundFarShade = UnityEngine.Color32.ConstructColor(i571[16], i571[17], i571[18], i571[19])
  i570.backgroundMid = UnityEngine.Color32.ConstructColor(i571[20], i571[21], i571[22], i571[23])
  i570.backgroundNear = UnityEngine.Color32.ConstructColor(i571[24], i571[25], i571[26], i571[27])
  i570.backgroundNearLight = UnityEngine.Color32.ConstructColor(i571[28], i571[29], i571[30], i571[31])
  i570.skyStar = UnityEngine.Color32.ConstructColor(i571[32], i571[33], i571[34], i571[35])
  i570.moon = UnityEngine.Color32.ConstructColor(i571[36], i571[37], i571[38], i571[39])
  i570.moonShade = UnityEngine.Color32.ConstructColor(i571[40], i571[41], i571[42], i571[43])
  i570.ground = UnityEngine.Color32.ConstructColor(i571[44], i571[45], i571[46], i571[47])
  i570.groundShade = UnityEngine.Color32.ConstructColor(i571[48], i571[49], i571[50], i571[51])
  i570.groundBuried = UnityEngine.Color32.ConstructColor(i571[52], i571[53], i571[54], i571[55])
  i570.groundBand = UnityEngine.Color32.ConstructColor(i571[56], i571[57], i571[58], i571[59])
  i570.groundRim = UnityEngine.Color32.ConstructColor(i571[60], i571[61], i571[62], i571[63])
  i570.groundRimLight = UnityEngine.Color32.ConstructColor(i571[64], i571[65], i571[66], i571[67])
  i570.groundCrater = UnityEngine.Color32.ConstructColor(i571[68], i571[69], i571[70], i571[71])
  i570.bush = UnityEngine.Color32.ConstructColor(i571[72], i571[73], i571[74], i571[75])
  i570.bushLight = UnityEngine.Color32.ConstructColor(i571[76], i571[77], i571[78], i571[79])
  i570.treeLight = UnityEngine.Color32.ConstructColor(i571[80], i571[81], i571[82], i571[83])
  i570.treeShade = UnityEngine.Color32.ConstructColor(i571[84], i571[85], i571[86], i571[87])
  i570.water = UnityEngine.Color32.ConstructColor(i571[88], i571[89], i571[90], i571[91])
  i570.waterHighlight = UnityEngine.Color32.ConstructColor(i571[92], i571[93], i571[94], i571[95])
  i570.waterRim = UnityEngine.Color32.ConstructColor(i571[96], i571[97], i571[98], i571[99])
  i570.castleStone = UnityEngine.Color32.ConstructColor(i571[100], i571[101], i571[102], i571[103])
  i570.castleStoneShade = UnityEngine.Color32.ConstructColor(i571[104], i571[105], i571[106], i571[107])
  i570.castleBrick = UnityEngine.Color32.ConstructColor(i571[108], i571[109], i571[110], i571[111])
  i570.castleCrack = UnityEngine.Color32.ConstructColor(i571[112], i571[113], i571[114], i571[115])
  i570.castleWindow = UnityEngine.Color32.ConstructColor(i571[116], i571[117], i571[118], i571[119])
  i570.castleWindowLit = UnityEngine.Color32.ConstructColor(i571[120], i571[121], i571[122], i571[123])
  i570.roofPlayer = UnityEngine.Color32.ConstructColor(i571[124], i571[125], i571[126], i571[127])
  i570.roofPlayerShade = UnityEngine.Color32.ConstructColor(i571[128], i571[129], i571[130], i571[131])
  i570.roofEnemy = UnityEngine.Color32.ConstructColor(i571[132], i571[133], i571[134], i571[135])
  i570.roofEnemyShade = UnityEngine.Color32.ConstructColor(i571[136], i571[137], i571[138], i571[139])
  i570.cannonPlayer = UnityEngine.Color32.ConstructColor(i571[140], i571[141], i571[142], i571[143])
  i570.cannonPlayerShade = UnityEngine.Color32.ConstructColor(i571[144], i571[145], i571[146], i571[147])
  i570.cannonEnemy = UnityEngine.Color32.ConstructColor(i571[148], i571[149], i571[150], i571[151])
  i570.cannonEnemyShade = UnityEngine.Color32.ConstructColor(i571[152], i571[153], i571[154], i571[155])
  i570.platform = UnityEngine.Color32.ConstructColor(i571[156], i571[157], i571[158], i571[159])
  i570.platformShade = UnityEngine.Color32.ConstructColor(i571[160], i571[161], i571[162], i571[163])
  i570.track = UnityEngine.Color32.ConstructColor(i571[164], i571[165], i571[166], i571[167])
  i570.trackWheel = UnityEngine.Color32.ConstructColor(i571[168], i571[169], i571[170], i571[171])
  i570.booster10 = UnityEngine.Color32.ConstructColor(i571[172], i571[173], i571[174], i571[175])
  i570.booster10Cap = UnityEngine.Color32.ConstructColor(i571[176], i571[177], i571[178], i571[179])
  i570.booster20 = UnityEngine.Color32.ConstructColor(i571[180], i571[181], i571[182], i571[183])
  i570.booster20Cap = UnityEngine.Color32.ConstructColor(i571[184], i571[185], i571[186], i571[187])
  i570.booster50 = UnityEngine.Color32.ConstructColor(i571[188], i571[189], i571[190], i571[191])
  i570.booster50Cap = UnityEngine.Color32.ConstructColor(i571[192], i571[193], i571[194], i571[195])
  i570.rocketBody = UnityEngine.Color32.ConstructColor(i571[196], i571[197], i571[198], i571[199])
  i570.rocketShade = UnityEngine.Color32.ConstructColor(i571[200], i571[201], i571[202], i571[203])
  i570.rocketPlayer = UnityEngine.Color32.ConstructColor(i571[204], i571[205], i571[206], i571[207])
  i570.rocketEnemy = UnityEngine.Color32.ConstructColor(i571[208], i571[209], i571[210], i571[211])
  i570.rocketTail = UnityEngine.Color32.ConstructColor(i571[212], i571[213], i571[214], i571[215])
  i570.smoke = UnityEngine.Color32.ConstructColor(i571[216], i571[217], i571[218], i571[219])
  i570.blastCore = UnityEngine.Color32.ConstructColor(i571[220], i571[221], i571[222], i571[223])
  i570.blastMid = UnityEngine.Color32.ConstructColor(i571[224], i571[225], i571[226], i571[227])
  i570.blastEdge = UnityEngine.Color32.ConstructColor(i571[228], i571[229], i571[230], i571[231])
  i570.hudPanel = UnityEngine.Color32.ConstructColor(i571[232], i571[233], i571[234], i571[235])
  i570.hudTrack = UnityEngine.Color32.ConstructColor(i571[236], i571[237], i571[238], i571[239])
  i570.hudHealth = UnityEngine.Color32.ConstructColor(i571[240], i571[241], i571[242], i571[243])
  i570.hudHealthShade = UnityEngine.Color32.ConstructColor(i571[244], i571[245], i571[246], i571[247])
  i570.hudGold = UnityEngine.Color32.ConstructColor(i571[248], i571[249], i571[250], i571[251])
  i570.hudGoldShade = UnityEngine.Color32.ConstructColor(i571[252], i571[253], i571[254], i571[255])
  i570.hudText = UnityEngine.Color32.ConstructColor(i571[256], i571[257], i571[258], i571[259])
  return i570
}

Deserializers["CastleDestroyers.Core.ThemeArt"] = function (request, data, root) {
  var i572 = root || request.c( 'CastleDestroyers.Core.ThemeArt' )
  var i573 = data
  request.r(i573[0], i573[1], 0, i572, 'atlas')
  request.r(i573[2], i573[3], 0, i572, 'castleBlue')
  request.r(i573[4], i573[5], 0, i572, 'castleRed')
  request.r(i573[6], i573[7], 0, i572, 'backFar')
  request.r(i573[8], i573[9], 0, i572, 'backMid')
  request.r(i573[10], i573[11], 0, i572, 'backNear')
  request.r(i573[12], i573[13], 0, i572, 'terrain')
  return i572
}

Deserializers["CastleDestroyers.Destruction.TerrainMaskData"] = function (request, data, root) {
  var i574 = root || request.c( 'CastleDestroyers.Destruction.TerrainMaskData' )
  var i575 = data
  i574.width = i575[0]
  i574.height = i575[1]
  i574.solidRuns = i575[2]
  i574.grassRuns = i575[3]
  i574.buriedRuns = i575[4]
  return i574
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Resources"] = function (request, data, root) {
  var i576 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Resources' )
  var i577 = data
  var i579 = i577[0]
  var i578 = []
  for(var i = 0; i < i579.length; i += 1) {
    i578.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.Resources+File', i579[i + 0]) );
  }
  i576.files = i578
  i576.componentToPrefabIds = i577[1]
  return i576
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.Resources+File"] = function (request, data, root) {
  var i582 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.Resources+File' )
  var i583 = data
  i582.path = i583[0]
  request.r(i583[1], i583[2], 0, i582, 'unityObject')
  return i582
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings"] = function (request, data, root) {
  var i584 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings' )
  var i585 = data
  var i587 = i585[0]
  var i586 = []
  for(var i = 0; i < i587.length; i += 1) {
    i586.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+ScriptsExecutionOrder', i587[i + 0]) );
  }
  i584.scriptsExecutionOrder = i586
  var i589 = i585[1]
  var i588 = []
  for(var i = 0; i < i589.length; i += 1) {
    i588.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+SortingLayer', i589[i + 0]) );
  }
  i584.sortingLayers = i588
  var i591 = i585[2]
  var i590 = []
  for(var i = 0; i < i591.length; i += 1) {
    i590.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+CullingLayer', i591[i + 0]) );
  }
  i584.cullingLayers = i590
  i584.timeSettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+TimeSettings', i585[3], i584.timeSettings)
  i584.physicsSettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings', i585[4], i584.physicsSettings)
  i584.physics2DSettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings', i585[5], i584.physics2DSettings)
  i584.qualitySettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.QualitySettings', i585[6], i584.qualitySettings)
  i584.enableRealtimeShadows = !!i585[7]
  i584.enableAutoInstancing = !!i585[8]
  i584.enableStaticBatching = !!i585[9]
  i584.enableDynamicBatching = !!i585[10]
  i584.usePreservativeDynamicBatching = !!i585[11]
  i584.lightmapEncodingQuality = i585[12]
  i584.desiredColorSpace = i585[13]
  var i593 = i585[14]
  var i592 = []
  for(var i = 0; i < i593.length; i += 1) {
    i592.push( i593[i + 0] );
  }
  i584.allTags = i592
  return i584
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+ScriptsExecutionOrder"] = function (request, data, root) {
  var i596 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+ScriptsExecutionOrder' )
  var i597 = data
  i596.name = i597[0]
  i596.value = i597[1]
  return i596
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+SortingLayer"] = function (request, data, root) {
  var i600 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+SortingLayer' )
  var i601 = data
  i600.id = i601[0]
  i600.name = i601[1]
  i600.value = i601[2]
  return i600
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+CullingLayer"] = function (request, data, root) {
  var i604 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+CullingLayer' )
  var i605 = data
  i604.id = i605[0]
  i604.name = i605[1]
  return i604
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+TimeSettings"] = function (request, data, root) {
  var i606 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+TimeSettings' )
  var i607 = data
  i606.fixedDeltaTime = i607[0]
  i606.maximumDeltaTime = i607[1]
  i606.timeScale = i607[2]
  i606.maximumParticleTimestep = i607[3]
  return i606
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings"] = function (request, data, root) {
  var i608 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings' )
  var i609 = data
  i608.gravity = new pc.Vec3( i609[0], i609[1], i609[2] )
  i608.defaultSolverIterations = i609[3]
  i608.bounceThreshold = i609[4]
  i608.autoSyncTransforms = !!i609[5]
  i608.autoSimulation = !!i609[6]
  var i611 = i609[7]
  var i610 = []
  for(var i = 0; i < i611.length; i += 1) {
    i610.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings+CollisionMask', i611[i + 0]) );
  }
  i608.collisionMatrix = i610
  return i608
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings+CollisionMask"] = function (request, data, root) {
  var i614 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings+CollisionMask' )
  var i615 = data
  i614.enabled = !!i615[0]
  i614.layerId = i615[1]
  i614.otherLayerId = i615[2]
  return i614
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings"] = function (request, data, root) {
  var i616 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings' )
  var i617 = data
  request.r(i617[0], i617[1], 0, i616, 'material')
  i616.gravity = new pc.Vec2( i617[2], i617[3] )
  i616.positionIterations = i617[4]
  i616.velocityIterations = i617[5]
  i616.velocityThreshold = i617[6]
  i616.maxLinearCorrection = i617[7]
  i616.maxAngularCorrection = i617[8]
  i616.maxTranslationSpeed = i617[9]
  i616.maxRotationSpeed = i617[10]
  i616.baumgarteScale = i617[11]
  i616.baumgarteTOIScale = i617[12]
  i616.timeToSleep = i617[13]
  i616.linearSleepTolerance = i617[14]
  i616.angularSleepTolerance = i617[15]
  i616.defaultContactOffset = i617[16]
  i616.autoSimulation = !!i617[17]
  i616.queriesHitTriggers = !!i617[18]
  i616.queriesStartInColliders = !!i617[19]
  i616.callbacksOnDisable = !!i617[20]
  i616.reuseCollisionCallbacks = !!i617[21]
  i616.autoSyncTransforms = !!i617[22]
  var i619 = i617[23]
  var i618 = []
  for(var i = 0; i < i619.length; i += 1) {
    i618.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings+CollisionMask', i619[i + 0]) );
  }
  i616.collisionMatrix = i618
  return i616
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings+CollisionMask"] = function (request, data, root) {
  var i622 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings+CollisionMask' )
  var i623 = data
  i622.enabled = !!i623[0]
  i622.layerId = i623[1]
  i622.otherLayerId = i623[2]
  return i622
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.QualitySettings"] = function (request, data, root) {
  var i624 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.QualitySettings' )
  var i625 = data
  var i627 = i625[0]
  var i626 = []
  for(var i = 0; i < i627.length; i += 1) {
    i626.push( request.d('Luna.Unity.DTO.UnityEngine.Assets.QualitySettings', i627[i + 0]) );
  }
  i624.qualityLevels = i626
  var i629 = i625[1]
  var i628 = []
  for(var i = 0; i < i629.length; i += 1) {
    i628.push( i629[i + 0] );
  }
  i624.names = i628
  i624.shadows = i625[2]
  i624.anisotropicFiltering = i625[3]
  i624.antiAliasing = i625[4]
  i624.lodBias = i625[5]
  i624.shadowCascades = i625[6]
  i624.shadowDistance = i625[7]
  i624.shadowmaskMode = i625[8]
  i624.shadowProjection = i625[9]
  i624.shadowResolution = i625[10]
  i624.softParticles = !!i625[11]
  i624.softVegetation = !!i625[12]
  i624.activeColorSpace = i625[13]
  i624.desiredColorSpace = i625[14]
  i624.masterTextureLimit = i625[15]
  i624.maxQueuedFrames = i625[16]
  i624.particleRaycastBudget = i625[17]
  i624.pixelLightCount = i625[18]
  i624.realtimeReflectionProbes = !!i625[19]
  i624.shadowCascade2Split = i625[20]
  i624.shadowCascade4Split = new pc.Vec3( i625[21], i625[22], i625[23] )
  i624.streamingMipmapsActive = !!i625[24]
  i624.vSyncCount = i625[25]
  i624.asyncUploadBufferSize = i625[26]
  i624.asyncUploadTimeSlice = i625[27]
  i624.billboardsFaceCameraPosition = !!i625[28]
  i624.shadowNearPlaneOffset = i625[29]
  i624.streamingMipmapsMemoryBudget = i625[30]
  i624.maximumLODLevel = i625[31]
  i624.streamingMipmapsAddAllCameras = !!i625[32]
  i624.streamingMipmapsMaxLevelReduction = i625[33]
  i624.streamingMipmapsRenderersPerFrame = i625[34]
  i624.resolutionScalingFixedDPIFactor = i625[35]
  i624.streamingMipmapsMaxFileIORequests = i625[36]
  i624.currentQualityLevel = i625[37]
  return i624
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+RenderObjectsSettings"] = function (request, data, root) {
  var i632 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+RenderObjectsSettings' )
  var i633 = data
  i632.Event = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i633[0], i632.Event)
  i632.filterSettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+FilterSettings', i633[1], i632.filterSettings)
  i632.overrideMaterialId = i633[2]
  i632.overrideMaterialPassIndex = i633[3]
  i632.overrideShaderId = i633[4]
  i632.overrideShaderPassIndex = i633[5]
  i632.overrideMode = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i633[6], i632.overrideMode)
  i632.overrideDepthState = !!i633[7]
  i632.depthCompareFunction = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i633[8], i632.depthCompareFunction)
  i632.enableWrite = !!i633[9]
  i632.stencilSettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.StencilStateData', i633[10], i632.stencilSettings)
  i632.cameraSettings = request.d('Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+CustomCameraSettings', i633[11], i632.cameraSettings)
  return i632
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.EnumDescription"] = function (request, data, root) {
  var i634 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.EnumDescription' )
  var i635 = data
  i634.Value = i635[0]
  return i634
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+FilterSettings"] = function (request, data, root) {
  var i636 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+FilterSettings' )
  var i637 = data
  i636.RenderQueueType = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i637[0], i636.RenderQueueType)
  i636.LayerMask = i637[1]
  var i639 = i637[2]
  var i638 = []
  for(var i = 0; i < i639.length; i += 1) {
    i638.push( i639[i + 0] );
  }
  i636.PassNames = i638
  return i636
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.StencilStateData"] = function (request, data, root) {
  var i640 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.StencilStateData' )
  var i641 = data
  i640.overrideStencilState = !!i641[0]
  i640.stencilReference = i641[1]
  i640.stencilCompareFunctionValue = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i641[2], i640.stencilCompareFunctionValue)
  i640.passOperationValue = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i641[3], i640.passOperationValue)
  i640.failOperationValue = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i641[4], i640.failOperationValue)
  i640.zFailOperationValue = request.d('Luna.Unity.DTO.UnityEngine.Assets.EnumDescription', i641[5], i640.zFailOperationValue)
  return i640
}

Deserializers["Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+CustomCameraSettings"] = function (request, data, root) {
  var i642 = root || request.c( 'Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+CustomCameraSettings' )
  var i643 = data
  i642.overrideCamera = !!i643[0]
  i642.restoreCamera = !!i643[1]
  i642.offset = new pc.Vec4( i643[2], i643[3], i643[4], i643[5] )
  i642.cameraFieldOfView = i643[6]
  return i642
}

Deserializers.fields = {"Luna.Unity.DTO.UnityEngine.Assets.Material":{"name":0,"shader":1,"renderQueue":3,"enableInstancing":4,"floatParameters":5,"colorParameters":6,"vectorParameters":7,"textureParameters":8,"materialFlags":9},"Luna.Unity.DTO.UnityEngine.Assets.Material+FloatParameter":{"name":0,"value":1},"Luna.Unity.DTO.UnityEngine.Assets.Material+ColorParameter":{"name":0,"value":1},"Luna.Unity.DTO.UnityEngine.Assets.Material+VectorParameter":{"name":0,"value":1},"Luna.Unity.DTO.UnityEngine.Assets.Material+TextureParameter":{"name":0,"value":1},"Luna.Unity.DTO.UnityEngine.Assets.Material+MaterialFlag":{"name":0,"enabled":1},"Luna.Unity.DTO.UnityEngine.Textures.Texture2D":{"name":0,"width":1,"height":2,"mipmapCount":3,"anisoLevel":4,"filterMode":5,"hdr":6,"format":7,"wrapMode":8,"alphaIsTransparency":9,"alphaSource":10,"graphicsFormat":11,"sRGBTexture":12,"desiredColorSpace":13,"wrapU":14,"wrapV":15},"Luna.Unity.DTO.UnityEngine.Scene.Scene":{"name":0,"index":1,"startup":2},"Luna.Unity.DTO.UnityEngine.Components.Camera":{"aspect":0,"orthographic":1,"orthographicSize":2,"backgroundColor":3,"nearClipPlane":7,"farClipPlane":8,"fieldOfView":9,"depth":10,"clearFlags":11,"cullingMask":12,"rect":13,"targetTexture":14,"usePhysicalProperties":16,"focalLength":17,"sensorSize":18,"lensShift":20,"gateFit":22,"commandBufferCount":23,"cameraType":24,"enabled":25},"Luna.Unity.DTO.UnityEngine.Scene.GameObject":{"name":0,"tagId":1,"enabled":2,"isStatic":3,"layer":4},"Luna.Unity.DTO.UnityEngine.Components.MeshFilter":{"sharedMesh":0},"Luna.Unity.DTO.UnityEngine.Components.MeshRenderer":{"additionalVertexStreams":0,"enabled":2,"sharedMaterial":3,"sharedMaterials":5,"receiveShadows":6,"shadowCastingMode":7,"sortingLayerID":8,"sortingOrder":9,"lightmapIndex":10,"lightmapSceneIndex":11,"lightmapScaleOffset":12,"lightProbeUsage":16,"reflectionProbeUsage":17},"Luna.Unity.DTO.UnityEngine.Components.AudioSource":{"clip":0,"outputAudioMixerGroup":2,"playOnAwake":4,"loop":5,"time":6,"volume":7,"pitch":8,"enabled":9},"Luna.Unity.DTO.UnityEngine.Components.RectTransform":{"pivot":0,"anchorMin":2,"anchorMax":4,"sizeDelta":6,"anchoredPosition3D":8,"rotation":11,"scale":15},"Luna.Unity.DTO.UnityEngine.Components.Canvas":{"planeDistance":0,"referencePixelsPerUnit":1,"isFallbackOverlay":2,"renderMode":3,"renderOrder":4,"sortingLayerName":5,"sortingOrder":6,"scaleFactor":7,"worldCamera":8,"overrideSorting":10,"pixelPerfect":11,"targetDisplay":12,"overridePixelPerfect":13,"enabled":14},"Luna.Unity.DTO.UnityEngine.Components.CanvasRenderer":{"cullTransparentMesh":0},"Luna.Unity.DTO.UnityEngine.Assets.RenderSettings":{"ambientIntensity":0,"reflectionIntensity":1,"ambientMode":2,"ambientLight":3,"ambientSkyColor":7,"ambientGroundColor":11,"ambientEquatorColor":15,"fogColor":19,"fogEndDistance":23,"fogStartDistance":24,"fogDensity":25,"fog":26,"skybox":27,"fogMode":29,"lightmaps":30,"lightProbes":31,"lightmapsMode":32,"mixedBakeMode":33,"environmentLightingMode":34,"ambientProbe":35,"customReflection":36,"defaultReflection":38,"defaultReflectionMode":40,"defaultReflectionResolution":41,"sunLightObjectId":42,"pixelLightCount":43,"defaultReflectionHDR":44,"hasLightDataAsset":45,"hasManualGenerate":46},"Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+Lightmap":{"lightmapColor":0,"lightmapDirection":2,"shadowMask":4},"Luna.Unity.DTO.UnityEngine.Assets.RenderSettings+LightProbes":{"bakedProbes":0,"positions":1,"hullRays":2,"tetrahedra":3,"neighbours":4,"matrices":5},"Luna.Unity.DTO.UnityEngine.Assets.UniversalRenderPipelineAsset":{"AdditionalLightsRenderingMode":0,"LightRenderingMode":1,"MainLightRenderingModeValue":2,"SupportsMainLightShadows":3,"MixedLightingSupported":4,"MainLightShadowmapResolutionValue":5,"SupportsSoftShadows":6,"SoftShadowQualityValue":7,"ShadowDistance":8,"ShadowCascadeCount":9,"Cascade2Split":10,"Cascade3Split":11,"Cascade4Split":13,"CascadeBorder":16,"ShadowDepthBias":17,"ShadowNormalBias":18,"RequireDepthTexture":19,"RequireOpaqueTexture":20,"scriptableRendererData":21},"Luna.Unity.DTO.UnityEngine.Assets.LightRenderingMode":{"Disabled":0,"PerVertex":1,"PerPixel":2},"Luna.Unity.DTO.UnityEngine.Assets.ScriptableRendererData":{"opaqueLayerMask":0,"transparentLayerMask":1,"RenderObjectsFeatures":2,"name":3},"Luna.Unity.DTO.UnityEngine.Assets.RenderObjects":{"settings":0,"name":1,"typeName":2},"Luna.Unity.DTO.UnityEngine.Assets.Shader":{"ShaderCompilationErrors":0,"name":1,"guid":2,"shaderDefinedKeywords":3,"passes":4,"usePasses":5,"defaultParameterValues":6,"unityFallbackShader":7,"readDepth":9,"hasDepthOnlyPass":10,"isCreatedByShaderGraph":11,"disableBatching":12,"compiled":13},"Luna.Unity.DTO.UnityEngine.Assets.Shader+ShaderCompilationError":{"shaderName":0,"errorMessage":1},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass":{"id":0,"subShaderIndex":1,"name":2,"passType":3,"grabPassTextureName":4,"usePass":5,"zTest":6,"zWrite":7,"culling":8,"blending":9,"alphaBlending":10,"colorWriteMask":11,"offsetUnits":12,"offsetFactor":13,"stencilRef":14,"stencilReadMask":15,"stencilWriteMask":16,"stencilOp":17,"stencilOpFront":18,"stencilOpBack":19,"tags":20,"passDefinedKeywords":21,"passDefinedKeywordGroups":22,"variants":23,"excludedVariants":24,"hasDepthReader":25},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Value":{"val":0,"name":1},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Blending":{"src":0,"dst":1,"op":2},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+StencilOp":{"pass":0,"fail":1,"zFail":2,"comp":3},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Tag":{"name":0,"value":1},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+KeywordGroup":{"keywords":0,"hasDiscard":1},"Luna.Unity.DTO.UnityEngine.Assets.Shader+Pass+Variant":{"passId":0,"subShaderIndex":1,"keywords":2,"vertexProgram":3,"fragmentProgram":4,"exportedForWebGl2":5,"readDepth":6},"Luna.Unity.DTO.UnityEngine.Assets.Shader+UsePass":{"shader":0,"pass":2},"Luna.Unity.DTO.UnityEngine.Assets.Shader+DefaultParameterValue":{"name":0,"type":1,"value":2,"textureValue":6,"shaderPropertyFlag":7},"Luna.Unity.DTO.UnityEngine.Textures.Sprite":{"name":0,"texture":1,"aabb":3,"vertices":4,"triangles":5,"textureRect":6,"packedRect":10,"border":14,"transparency":18,"bounds":19,"pixelsPerUnit":20,"textureWidth":21,"textureHeight":22,"nativeSize":23,"pivot":25,"textureRectOffset":27},"Luna.Unity.DTO.UnityEngine.Assets.AudioClip":{"name":0},"Luna.Unity.DTO.UnityEngine.Assets.Font":{"name":0,"ascent":1,"originalLineHeight":2,"fontSize":3,"characterInfo":4,"texture":5,"originalFontSize":7},"Luna.Unity.DTO.UnityEngine.Assets.Font+CharacterInfo":{"index":0,"advance":1,"bearing":2,"glyphWidth":3,"glyphHeight":4,"minX":5,"maxX":6,"minY":7,"maxY":8,"uvBottomLeftX":9,"uvBottomLeftY":10,"uvBottomRightX":11,"uvBottomRightY":12,"uvTopLeftX":13,"uvTopLeftY":14,"uvTopRightX":15,"uvTopRightY":16},"Luna.Unity.DTO.UnityEngine.Assets.Resources":{"files":0,"componentToPrefabIds":1},"Luna.Unity.DTO.UnityEngine.Assets.Resources+File":{"path":0,"unityObject":1},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings":{"scriptsExecutionOrder":0,"sortingLayers":1,"cullingLayers":2,"timeSettings":3,"physicsSettings":4,"physics2DSettings":5,"qualitySettings":6,"enableRealtimeShadows":7,"enableAutoInstancing":8,"enableStaticBatching":9,"enableDynamicBatching":10,"usePreservativeDynamicBatching":11,"lightmapEncodingQuality":12,"desiredColorSpace":13,"allTags":14},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+ScriptsExecutionOrder":{"name":0,"value":1},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+SortingLayer":{"id":0,"name":1,"value":2},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+CullingLayer":{"id":0,"name":1},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+TimeSettings":{"fixedDeltaTime":0,"maximumDeltaTime":1,"timeScale":2,"maximumParticleTimestep":3},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings":{"gravity":0,"defaultSolverIterations":3,"bounceThreshold":4,"autoSyncTransforms":5,"autoSimulation":6,"collisionMatrix":7},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+PhysicsSettings+CollisionMask":{"enabled":0,"layerId":1,"otherLayerId":2},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings":{"material":0,"gravity":2,"positionIterations":4,"velocityIterations":5,"velocityThreshold":6,"maxLinearCorrection":7,"maxAngularCorrection":8,"maxTranslationSpeed":9,"maxRotationSpeed":10,"baumgarteScale":11,"baumgarteTOIScale":12,"timeToSleep":13,"linearSleepTolerance":14,"angularSleepTolerance":15,"defaultContactOffset":16,"autoSimulation":17,"queriesHitTriggers":18,"queriesStartInColliders":19,"callbacksOnDisable":20,"reuseCollisionCallbacks":21,"autoSyncTransforms":22,"collisionMatrix":23},"Luna.Unity.DTO.UnityEngine.Assets.ProjectSettings+Physics2DSettings+CollisionMask":{"enabled":0,"layerId":1,"otherLayerId":2},"Luna.Unity.DTO.UnityEngine.Assets.QualitySettings":{"qualityLevels":0,"names":1,"shadows":2,"anisotropicFiltering":3,"antiAliasing":4,"lodBias":5,"shadowCascades":6,"shadowDistance":7,"shadowmaskMode":8,"shadowProjection":9,"shadowResolution":10,"softParticles":11,"softVegetation":12,"activeColorSpace":13,"desiredColorSpace":14,"masterTextureLimit":15,"maxQueuedFrames":16,"particleRaycastBudget":17,"pixelLightCount":18,"realtimeReflectionProbes":19,"shadowCascade2Split":20,"shadowCascade4Split":21,"streamingMipmapsActive":24,"vSyncCount":25,"asyncUploadBufferSize":26,"asyncUploadTimeSlice":27,"billboardsFaceCameraPosition":28,"shadowNearPlaneOffset":29,"streamingMipmapsMemoryBudget":30,"maximumLODLevel":31,"streamingMipmapsAddAllCameras":32,"streamingMipmapsMaxLevelReduction":33,"streamingMipmapsRenderersPerFrame":34,"resolutionScalingFixedDPIFactor":35,"streamingMipmapsMaxFileIORequests":36,"currentQualityLevel":37},"Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+RenderObjectsSettings":{"Event":0,"filterSettings":1,"overrideMaterialId":2,"overrideMaterialPassIndex":3,"overrideShaderId":4,"overrideShaderPassIndex":5,"overrideMode":6,"overrideDepthState":7,"depthCompareFunction":8,"enableWrite":9,"stencilSettings":10,"cameraSettings":11},"Luna.Unity.DTO.UnityEngine.Assets.EnumDescription":{"Value":0},"Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+FilterSettings":{"RenderQueueType":0,"LayerMask":1,"PassNames":2},"Luna.Unity.DTO.UnityEngine.Assets.StencilStateData":{"overrideStencilState":0,"stencilReference":1,"stencilCompareFunctionValue":2,"passOperationValue":3,"failOperationValue":4,"zFailOperationValue":5},"Luna.Unity.DTO.UnityEngine.Assets.RenderObjects+CustomCameraSettings":{"overrideCamera":0,"restoreCamera":1,"offset":2,"cameraFieldOfView":6}}

Deserializers.requiredComponents = {"26":[27],"28":[27],"29":[27],"30":[27],"31":[27],"32":[27],"33":[34],"35":[2],"36":[37],"38":[37],"39":[37],"40":[37],"41":[37],"42":[37],"43":[44],"45":[44],"46":[44],"47":[44],"48":[44],"49":[44],"50":[44],"51":[44],"52":[44],"53":[44],"54":[44],"55":[44],"56":[44],"57":[2],"58":[7],"59":[60],"61":[60],"19":[11],"62":[63],"64":[19],"65":[11],"66":[2],"4":[2],"67":[68],"69":[11],"70":[22,11],"71":[7],"72":[22,11],"73":[11],"74":[11],"75":[7,11],"76":[11,22],"77":[11],"78":[11],"79":[19],"12":[22,11],"80":[11],"21":[19],"81":[11],"82":[11],"83":[11],"84":[11],"85":[11],"86":[11],"87":[11],"88":[11],"89":[11],"90":[22,11],"91":[11],"92":[11],"93":[11],"94":[11],"13":[22,11],"95":[11],"96":[63],"97":[63],"98":[63],"99":[63],"100":[2],"101":[2]}

Deserializers.types = ["UnityEngine.Shader","UnityEngine.Texture2D","UnityEngine.Camera","UnityEngine.MonoBehaviour","UnityEngine.Rendering.Universal.UniversalAdditionalCameraData","UnityEngine.AudioListener","UnityEngine.MeshFilter","UnityEngine.MeshRenderer","UnityEngine.Material","CastleDestroyers.Core.GameBootstrap","CastleDestroyers.Core.GameConfig","UnityEngine.RectTransform","UnityEngine.UI.Image","UnityEngine.UI.Text","UnityEngine.GameObject","UnityEngine.Sprite","UnityEngine.AudioClip","UnityEngine.AudioSource","UnityEngine.Transform","UnityEngine.Canvas","UnityEngine.EventSystems.UIBehaviour","UnityEngine.UI.CanvasScaler","UnityEngine.CanvasRenderer","UnityEngine.Font","CastleDestroyers.Core.ThemePalette","CastleDestroyers.Destruction.TerrainMaskData","UnityEngine.AudioLowPassFilter","UnityEngine.AudioBehaviour","UnityEngine.AudioHighPassFilter","UnityEngine.AudioReverbFilter","UnityEngine.AudioDistortionFilter","UnityEngine.AudioEchoFilter","UnityEngine.AudioChorusFilter","UnityEngine.Cloth","UnityEngine.SkinnedMeshRenderer","UnityEngine.FlareLayer","UnityEngine.CharacterJoint","UnityEngine.Rigidbody","UnityEngine.ConfigurableJoint","UnityEngine.ConstantForce","UnityEngine.FixedJoint","UnityEngine.HingeJoint","UnityEngine.SpringJoint","UnityEngine.CompositeCollider2D","UnityEngine.Rigidbody2D","UnityEngine.Joint2D","UnityEngine.AnchoredJoint2D","UnityEngine.SpringJoint2D","UnityEngine.DistanceJoint2D","UnityEngine.FrictionJoint2D","UnityEngine.HingeJoint2D","UnityEngine.RelativeJoint2D","UnityEngine.SliderJoint2D","UnityEngine.TargetJoint2D","UnityEngine.FixedJoint2D","UnityEngine.WheelJoint2D","UnityEngine.ConstantForce2D","UnityEngine.StreamingController","UnityEngine.TextMesh","UnityEngine.Tilemaps.TilemapRenderer","UnityEngine.Tilemaps.Tilemap","UnityEngine.Tilemaps.TilemapCollider2D","UnityEngine.InputSystem.UI.InputSystemUIInputModule","UnityEngine.EventSystems.EventSystem","UnityEngine.InputSystem.UI.TrackedDeviceRaycaster","UnityEngine.Rendering.UI.UIFoldout","UnityEngine.Rendering.Universal.PixelPerfectCamera","UnityEngine.Rendering.Universal.UniversalAdditionalLightData","UnityEngine.Light","TMPro.TMP_Dropdown","TMPro.TMP_SelectionCaret","TMPro.TMP_SubMesh","TMPro.TMP_SubMeshUI","TMPro.TMP_Text","TMPro.TextContainer","TMPro.TextMeshPro","TMPro.TextMeshProUGUI","UnityEngine.UI.Dropdown","UnityEngine.UI.Graphic","UnityEngine.UI.GraphicRaycaster","UnityEngine.UI.AspectRatioFitter","UnityEngine.UI.ContentSizeFitter","UnityEngine.UI.GridLayoutGroup","UnityEngine.UI.HorizontalLayoutGroup","UnityEngine.UI.HorizontalOrVerticalLayoutGroup","UnityEngine.UI.LayoutElement","UnityEngine.UI.LayoutGroup","UnityEngine.UI.VerticalLayoutGroup","UnityEngine.UI.Mask","UnityEngine.UI.MaskableGraphic","UnityEngine.UI.RawImage","UnityEngine.UI.RectMask2D","UnityEngine.UI.ScrollRect","UnityEngine.UI.Scrollbar","UnityEngine.UI.Slider","UnityEngine.UI.Toggle","UnityEngine.EventSystems.BaseInputModule","UnityEngine.EventSystems.PointerInputModule","UnityEngine.EventSystems.StandaloneInputModule","UnityEngine.EventSystems.TouchInputModule","UnityEngine.EventSystems.Physics2DRaycaster","UnityEngine.EventSystems.PhysicsRaycaster"]

Deserializers.unityVersion = "6000.0.85f1";

Deserializers.productName = "castle-destroyers-playable";

Deserializers.lunaInitializationTime = "";

Deserializers.lunaDaysRunning = "1.3";

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

Deserializers.buildID = "90c3d3f7-0a30-4ccd-99f2-6aa49a456765";

Deserializers.runtimeInitializeOnLoadInfos = [[["UnityEngine","Rendering","DebugUpdater","RuntimeInit"],["Unity","PerformanceTesting","PerformanceTest","ResetStaticsOnLoad"],["Unity","Burst","BurstCompiler","ResetStaticsOnLoad"],["UnityEngine","Experimental","Rendering","ScriptableRuntimeReflectionSystemSettings","ScriptingDirtyReflectionSystemInstance"]],[["Unity","Pipeline","Console","ConsoleLogCapture","RuntimeBootstrap"],["Unity","Pipeline","RuntimePipelineBootstrap","RuntimeBootstrap"],["UnityEngine","InputSystem","PlayerInput","InitializeGlobalPlayerState"],["UnityEngine","InputSystem","InputSystem","RuntimeInitialize"],["UnityEngine","InputSystem","InputActionState","InitializeGlobalActionState"],["UnityEngine","AI","NavMesh","ClearPreUpdateListeners"]],[["$BurstDirectCallInitializer","Initialize"],["$BurstDirectCallInitializer","Initialize"],["$BurstDirectCallInitializer","Initialize"],["$BurstDirectCallInitializer","Initialize"]],[["UnityEngine","Experimental","Rendering","XRSystem","XRSystemInit"]],[["Unity","PerformanceTesting","Data","RunSettings","ResetStaticsOnLoad"],["Unity","PerformanceTesting","PlayerCallbacks","ResetStaticsOnLoad"],["UnityEngine","InputSystem","Plugins","InputForUI","InputSystemProvider","Bootstrap"],["UnityEngine","InputSystem","EnhancedTouch","Touch","InitializeGlobalTouchState"],["UnityEngine","InputSystem","Users","InputUser","InitializeGlobalUserState"],["UnityEngine","InputSystem","UI","InputSystemUIInputModule","ResetDefaultActions"]]];

Deserializers.typeNameToIdMap = function(){ var i = 0; return Deserializers.types.reduce( function( res, item ) { res[ item ] = i++; return res; }, {} ) }()

