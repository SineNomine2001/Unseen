using System.Collections.Generic;
namespace UnityEngine.Rendering.LWRP
{
    /// <summary>
    /// Copy the given color buffer to the given destination color buffer.
    ///
    /// You can use this pass to copy a color buffer to the destination,
    /// so you can use it later in rendering. For example, you can copy
    /// the opaque texture to use it for distortion effects.
    /// </summary>
    internal class BlitPassVolumeFogSRP : UnityEngine.Rendering.Universal.ScriptableRenderPass
    {
        //v0.4  - Unity 2020.1
#if UNITY_2020_2_OR_NEWER
        public BlitVolumeFogSRP.BlitSettings settings;
        UnityEngine.Rendering.Universal.RenderTargetHandle _handle;
        public override void OnCameraSetup(CommandBuffer cmd, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
        {
            _handle.Init(settings.textureId);
            destination = (settings.destination == BlitVolumeFogSRP.Target.Color)
                ? UnityEngine.Rendering.Universal.RenderTargetHandle.CameraTarget
                : _handle;

            var renderer = renderingData.cameraData.renderer;
            source = renderer.cameraColorTarget;
        }
#endif

        //v1.9.9.7 - Ethereal v1.1.8f
        public List<Camera> extraCameras = new List<Camera>();
        public int extraCameraID = 0; //assign 0 for reflection camera, 1 to N for choosing from the extra cameras list

        //v1.9.9.6 - Ethereal v1.1.8e
        public bool isForDualCameras = false;

        //v1.9.9.5 - Ethereal v1.1.8
        //Add visible lights count - renderingData.cullResults.visibleLights.Length

        ////v1.9.9.4 - Ethereal 1.1.7 - Control sampling for no noise option
        //[Tooltip("Volume sampling control, noise to no noise ratio, 0 is zero noise (x), no noise sampling step length (y) & noise sampling step lengths (z,w)")]
        public Vector4 volumeSamplingControl = new Vector4(1, 1, 1, 1);

        //v1.9.9.3
        //[Tooltip("Volume Shadow control (x-unity shadow distance, y,z-shadow atten power & offset, w-)")]
        public Vector4 shadowsControl = new Vector4(500, 1, 1, 0);

        //v1.9.9.2
        public bool enableSunShafts = false;//simple screen space sun shafts

        //v1.9.9.1
        public List<Light> lightsArray = new List<Light>();

        //v1.9.9
        public Vector4 lightControlA = new Vector4(1, 1, 1, 1);
        public Vector4 lightControlB = new Vector4(1, 1, 1, 1);
        public bool controlByColor = false;
        public Light lightA;
        public Light lightB;//grab colors of the two lights to apply volume to

        //v1.7
        public int lightCount = 3;

        //v1.6
        public bool isForReflections = false;
        public Camera reflectCamera;

        public float blendVolumeLighting = 0;
        public float LightRaySamples = 8;
        public Vector4 stepsControl = new Vector4(0, 0, 1, 1);
        public Vector4 lightNoiseControl = new Vector4(0.6f, 0.75f, 1, 1);  //v1.5

        //FOG URP /////////////
        //FOG URP /////////////
        //FOG URP /////////////
        //public float blend =  0.5f;
        public Color _FogColor = Color.white / 2;
        //fog params
        public Texture2D noiseTexture;
        public float _startDistance = 30f;
        public float _fogHeight = 0.75f;
        public float _fogDensity = 1f;
        public float _cameraRoll = 0.0f;
        public Vector4 _cameraDiff = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        public float _cameraTiltSign = 1;
        public float heightDensity = 1;
        public float noiseDensity = 1;
        public float noiseScale = 1;
        public float noiseThickness = 1;
        public Vector3 noiseSpeed = new Vector4(1f, 1f, 1f);
        public float occlusionDrop = 1f;
        public float occlusionExp = 1f;
        public int noise3D = 1;
        public float startDistance = 1;
        public float luminance = 1;
        public float lumFac = 1;
        public float ScatterFac = 1;
        public float TurbFac = 1;
        public float HorizFac = 1;
        public float turbidity = 1;
        public float reileigh = 1;
        public float mieCoefficient = 1;
        public float mieDirectionalG = 1;
        public float bias = 1;
        public float contrast = 1;
        public Color TintColor = new Color(1, 1, 1, 1);
        public Vector3 TintColorK = new Vector3(0, 0, 0);
        public Vector3 TintColorL = new Vector3(0, 0, 0);
        public Vector4 Sun = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        public bool FogSky = true;
        public float ClearSkyFac = 1f;
        public Vector4 PointL = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        public Vector4 PointLParams = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

        bool _useRadialDistance = false;
        bool _fadeToSkybox = true;

        bool allowHDR = false;
        //END FOG URP //////////////////
        //END FOG URP //////////////////
        //END FOG URP //////////////////


        //SUN SHAFTS         
        public BlitVolumeFogSRP.BlitSettings.SunShaftsResolution resolution = BlitVolumeFogSRP.BlitSettings.SunShaftsResolution.Normal;
        public BlitVolumeFogSRP.BlitSettings.ShaftsScreenBlendMode screenBlendMode = BlitVolumeFogSRP.BlitSettings.ShaftsScreenBlendMode.Screen;
        public Vector3 sunTransform = new Vector3(0f, 0f, 0f); // Transform sunTransform;
        public int radialBlurIterations = 2;
        public Color sunColor = Color.white;
        public Color sunThreshold = new Color(0.87f, 0.74f, 0.65f);
        public float sunShaftBlurRadius = 2.5f;
        public float sunShaftIntensity = 1.15f;
        public float maxRadius = 0.75f;
        public bool useDepthTexture = true;
        public float blend = 0.5f;
                
        public enum RenderTarget
        {
            Color,
            RenderTexture,
        }
        public bool inheritFromController = true;

        public bool enableFog = true;

        public Material blitMaterial = null;
        //public Material blitMaterialFOG = null;
        public int blitShaderPassIndex = 0;
        public FilterMode filterMode { get; set; }

        private RenderTargetIdentifier source { get; set; }
        private UnityEngine.Rendering.Universal.RenderTargetHandle destination { get; set; }

        UnityEngine.Rendering.Universal.RenderTargetHandle m_TemporaryColorTexture;
        string m_ProfilerTag;


        //SUN SHAFTS
        RenderTexture lrColorB;
        UnityEngine.Rendering.Universal.RenderTargetHandle lrDepthBuffer;

        /// <summary>
        /// Create the CopyColorPass
        /// </summary>
        public BlitPassVolumeFogSRP(UnityEngine.Rendering.Universal.RenderPassEvent renderPassEvent, Material blitMaterial, int blitShaderPassIndex, string tag, BlitVolumeFogSRP.BlitSettings settings)
        {
            this.enableFog = settings.enableFog;

            this.inheritFromController = settings.inheritFromController;
            this.renderPassEvent = renderPassEvent;
            this.blitMaterial = blitMaterial;
            this.blitShaderPassIndex = blitShaderPassIndex;
            m_ProfilerTag = tag;
            m_TemporaryColorTexture.Init("_TemporaryColorTexture");

            //SUN SHAFTS
            this.resolution = settings.resolution;
            this.screenBlendMode = settings.screenBlendMode;
            this.sunTransform = settings.sunTransform;
            this.radialBlurIterations = settings.radialBlurIterations;
            this.sunColor = settings.sunColor;
            this.sunThreshold = settings.sunThreshold;
            this.sunShaftBlurRadius = settings.sunShaftBlurRadius;
            this.sunShaftIntensity = settings.sunShaftIntensity;
            this.maxRadius = settings.maxRadius;
            this.useDepthTexture = settings.useDepthTexture;
            this.blend = settings.blend;

            ////// VOLUME FOG URP /////////////////
            //FOG URP /////////////
            //FOG URP /////////////
            //FOG URP /////////////
            //this.blend =  0.5f;
            this._FogColor = settings._FogColor;
            //fog params
            this.noiseTexture = settings.noiseTexture;
            this._startDistance = settings._startDistance;
            this._fogHeight = settings._fogHeight;
            this._fogDensity = settings._fogDensity;
            this._cameraRoll = settings._cameraRoll;
            this._cameraDiff = settings._cameraDiff;
            this._cameraTiltSign = settings._cameraTiltSign;
            this.heightDensity = settings.heightDensity;
            this.noiseDensity = settings.noiseDensity;
            this.noiseScale = settings.noiseScale;
            this.noiseThickness = settings.noiseThickness;
            this.noiseSpeed = settings.noiseSpeed;
            this.occlusionDrop = settings.occlusionDrop;
            this.occlusionExp = settings.occlusionExp;
            this.noise3D = settings.noise3D;
            this.startDistance = settings.startDistance;
            this.luminance = settings.luminance;
            this.lumFac = settings.lumFac;
            this.ScatterFac = settings.ScatterFac;
            this.TurbFac = settings.TurbFac;
            this.HorizFac = settings.HorizFac;
            this.turbidity = settings.turbidity;
            this.reileigh = settings.reileigh;
            this.mieCoefficient = settings.mieCoefficient;
            this.mieDirectionalG = settings.mieDirectionalG;
            this.bias = settings.bias;
            this.contrast = settings.contrast;
            this.TintColor = settings.TintColor;
            this.TintColorK = settings.TintColorK;
            this.TintColorL = settings.TintColorL;
            this.Sun = settings.Sun;
            this.FogSky = settings.FogSky;
            this.ClearSkyFac = settings.ClearSkyFac;
            this.PointL = settings.PointL;
            this.PointLParams = settings.PointLParams;
            this._useRadialDistance = settings._useRadialDistance;
            this._fadeToSkybox = settings._fadeToSkybox;
            //END FOG URP //////////////////
            //END FOG URP //////////////////
            //END FOG URP //////////////////
            ////// END VOLUME FOG URP /////////////////
            this.blendVolumeLighting = settings.blendVolumeLighting;
            //this.LightRaySamples = settings.LightRaySamples;
            this.isForReflections = settings.isForReflections;

            //v1.9.9.6 - Ethereal v1.1.8e
            this.isForDualCameras = settings.isForDualCameras;

            //v1.9.9.7 - Ethereal v1.1.8f
            this.extraCameraID = settings.extraCameraID;
        }

        /// <summary>
        /// Configure the pass with the source and destination to execute on.
        /// </summary>
        /// <param name="source">Source Render Target</param>
        /// <param name="destination">Destination Render Target</param>
        public void Setup(RenderTargetIdentifier source, UnityEngine.Rendering.Universal.RenderTargetHandle destination)
        {
            this.source = source;
            this.destination = destination;
        }
        
        connectSuntoVolumeFogURP connector;

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
        {
            //grab settings if script on scene camera
            if (connector == null)
            {
                connector = renderingData.cameraData.camera.GetComponent<connectSuntoVolumeFogURP>();
                if (connector == null && Camera.main != null)
                {
                    connector = Camera.main.GetComponent<connectSuntoVolumeFogURP>();
                }
            }
            //Debug.Log(Camera.main.GetComponent<connectSuntoVolumeFogURP>().sun.transform.position);
            if (inheritFromController && connector != null)
            {
                this.enableFog = connector.enableFog;

                this.sunTransform = new Vector3(connector.Sun.x, connector.Sun.y, connector.Sun.z);// connector.sun.transform.position;
                this.screenBlendMode = connector.screenBlendMode;
                //public Vector3 sunTransform = new Vector3(0f, 0f, 0f); 
                this.radialBlurIterations = connector.radialBlurIterations;
                this.sunColor = connector.sunColor;
                this.sunThreshold = connector.sunThreshold;
                this.sunShaftBlurRadius = connector.sunShaftBlurRadius;
                this.sunShaftIntensity = connector.sunShaftIntensity;
                this.maxRadius = connector.maxRadius;
                this.useDepthTexture = connector.useDepthTexture;

                ////// VOLUME FOG URP /////////////////
                //FOG URP /////////////
                //FOG URP /////////////
                //FOG URP /////////////
                //this.blend =  0.5f;
                this._FogColor = connector._FogColor;
                //fog params
                this.noiseTexture = connector.noiseTexture;
                this._startDistance = connector._startDistance;
                this._fogHeight = connector._fogHeight;
                this._fogDensity = connector._fogDensity;
                this._cameraRoll = connector._cameraRoll;
                this._cameraDiff = connector._cameraDiff;
                this._cameraTiltSign = connector._cameraTiltSign;
                this.heightDensity = connector.heightDensity;
                this.noiseDensity = connector.noiseDensity;
                this.noiseScale = connector.noiseScale;
                this.noiseThickness = connector.noiseThickness;
                this.noiseSpeed = connector.noiseSpeed;
                this.occlusionDrop = connector.occlusionDrop;
                this.occlusionExp = connector.occlusionExp;
                this.noise3D = connector.noise3D;
                this.startDistance = connector.startDistance;
                this.luminance = connector.luminance;
                this.lumFac = connector.lumFac;
                this.ScatterFac = connector.ScatterFac;
                this.TurbFac = connector.TurbFac;
                this.HorizFac = connector.HorizFac;
                this.turbidity = connector.turbidity;
                this.reileigh = connector.reileigh;
                this.mieCoefficient = connector.mieCoefficient;
                this.mieDirectionalG = connector.mieDirectionalG;
                this.bias = connector.bias;
                this.contrast = connector.contrast;
                this.TintColor = connector.TintColor;
                this.TintColorK = connector.TintColorK;
                this.TintColorL = connector.TintColorL;
                this.Sun = connector.Sun;
                this.FogSky = connector.FogSky;
                this.ClearSkyFac = connector.ClearSkyFac;
                this.PointL = connector.PointL;
                this.PointLParams = connector.PointLParams;
                this._useRadialDistance = connector._useRadialDistance;
                this._fadeToSkybox = connector._fadeToSkybox;

                this.allowHDR = connector.allowHDR;
                //END FOG URP //////////////////
                //END FOG URP //////////////////
                //END FOG URP //////////////////
                ////// END VOLUME FOG URP /////////////////

                this.blendVolumeLighting = connector.blendVolumeLighting;
                this.LightRaySamples = connector.LightRaySamples;
                this.stepsControl = connector.stepsControl;
                this.lightNoiseControl = connector.lightNoiseControl;

                //v1.6
                this.reflectCamera = connector.reflectCamera;

                //v1.7
                this.lightCount = connector.lightCount;

                //v1.9.9
                this.lightControlA = connector.lightControlA;
                this.lightControlB = connector.lightControlB;
                this.controlByColor = connector.controlByColor;
                this.lightA = connector.lightA;
                this.lightB = connector.lightB;

                //v1.9.9
                this.lightsArray = connector.lightsArray;

                //v1.9.9.2
                this.enableSunShafts = connector.enableSunShafts;

                //v1.9.9.3
                this.shadowsControl = connector.shadowsControl;

                //v1.9.9.4
                this.volumeSamplingControl = connector.volumeSamplingControl;

                //v1.9.9.7 - Ethereal v1.1.8f
                this.extraCameras = connector.extraCameras;
            }

            //if still null, disable effect
            bool connectorFound = true;
            if (connector == null)
            {
                connectorFound = false;
            }

            if (enableFog && connectorFound)
            {
                CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

                RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
                opaqueDesc.depthBufferBits = 0;

                // Can't read and write to same color target, create a temp render target to blit. 
                if (destination == UnityEngine.Rendering.Universal.RenderTargetHandle.CameraTarget)
                {
                    cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, filterMode);
                    //RenderShafts(context, renderingData, cmd, opaqueDesc);

                    RenderFog(context, renderingData, cmd, opaqueDesc);


                    //v1.9.9.2
                    if (enableSunShafts)
                    {
                        if (connector.sun != null)
                        {
                            this.sunTransform = new Vector3(connector.sun.position.x, connector.sun.position.y, connector.sun.position.z);
                            RenderShafts(context, renderingData, cmd, opaqueDesc);
                        }
                        else
                        {
                            CommandBufferPool.Release(cmd);//release fog here v1.9.9.2
                        }
                    }
                    else
                    {
                        CommandBufferPool.Release(cmd);//release fog here v1.9.9.2
                    }
                }
            }
            else
            {
                //v1.9.9.2 - if no fog
                if (enableSunShafts && connectorFound && connector.sun != null)
                {
                    CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

                    RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
                    opaqueDesc.depthBufferBits = 0;

                    // Can't read and write to same color target, create a temp render target to blit. 
                    if (destination == UnityEngine.Rendering.Universal.RenderTargetHandle.CameraTarget)
                    {
                        cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, filterMode);                            
                        this.sunTransform = new Vector3(connector.sun.position.x, connector.sun.position.y, connector.sun.position.z);
                        RenderShafts(context, renderingData, cmd, opaqueDesc);                         
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (destination == UnityEngine.Rendering.Universal.RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);            
                cmd.ReleaseTemporaryRT(lrDepthBuffer.id);               
            }
        }    

        //SUN SHAFTS
        public void RenderShafts(ScriptableRenderContext context, UnityEngine.Rendering.Universal.RenderingData renderingData, CommandBuffer cmd, RenderTextureDescriptor opaqueDesc)
        {           
            opaqueDesc.depthBufferBits = 0;          
            Material sheetSHAFTS = blitMaterial;
           
            sheetSHAFTS.SetFloat("_Blend", blend);
            
            Camera camera = Camera.main;

            //v1.7.1 - Solve editor flickering
            if (Camera.current != null)
            {
                camera = Camera.current;
            }

            // we actually need to check this every frame
            if (useDepthTexture)
            {               
                camera.depthTextureMode |= DepthTextureMode.Depth;
            }           

            Vector3 v = Vector3.one * 0.5f;
            if (sunTransform != Vector3.zero) {
                v = camera.WorldToViewportPoint(sunTransform);
            }
            else {
                v = new Vector3(0.5f, 0.5f, 0.0f);
            }
            
            //v0.1
            int rtW = opaqueDesc.width;
            int rtH = opaqueDesc.height;

            cmd.GetTemporaryRT(lrDepthBuffer.id, opaqueDesc, filterMode);

            sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(1.0f, 1.0f, 0.0f, 0.0f) * sunShaftBlurRadius);
            sheetSHAFTS.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, maxRadius));
            sheetSHAFTS.SetVector("_SunThreshold", sunThreshold);

            if (!useDepthTexture)
            {               
                var format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9
                RenderTexture tmpBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, format);
                RenderTexture.active = tmpBuffer;
                GL.ClearWithSkybox(false, camera);
             
                sheetSHAFTS.SetTexture("_Skybox", tmpBuffer);               
                Blit(cmd, source, lrDepthBuffer.Identifier(), sheetSHAFTS, 3);

                RenderTexture.ReleaseTemporary(tmpBuffer);
            }
            else
            {               
                Blit(cmd, source, lrDepthBuffer.Identifier(), sheetSHAFTS, 2);
            }            

            Blit(cmd, source, m_TemporaryColorTexture.Identifier()); //KEEP BACKGROUND
           
            radialBlurIterations = Mathf.Clamp(radialBlurIterations, 1, 4);
            float ofs = sunShaftBlurRadius * (1.0f / 768.0f);

            sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
            sheetSHAFTS.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, maxRadius));

            for (int it2 = 0; it2 < radialBlurIterations; it2++)
            {
                // each iteration takes 2 * 6 samples
                // we update _BlurRadius each time to cheaply get a very smooth look
                lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0); 
                Blit(cmd, lrDepthBuffer.Identifier(), lrColorB, sheetSHAFTS, 1);
                cmd.ReleaseTemporaryRT(lrDepthBuffer.id);
                ofs = sunShaftBlurRadius * (((it2 * 2.0f + 1.0f) * 6.0f)) / 768.0f;             
                sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f)); 
                cmd.GetTemporaryRT(lrDepthBuffer.id, opaqueDesc, filterMode);                
                Blit(cmd, lrColorB, lrDepthBuffer.Identifier(), sheetSHAFTS, 1);
                RenderTexture.ReleaseTemporary(lrColorB);  
                ofs = sunShaftBlurRadius * (((it2 * 2.0f + 2.0f) * 6.0f)) / 768.0f;              
                sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
            }
            
            // put together:
            if (v.z >= 0.0f)
            {              
                sheetSHAFTS.SetVector("_SunColor", new Vector4(sunColor.r, sunColor.g, sunColor.b, sunColor.a) * sunShaftIntensity);
            }
            else
            {
                sheetSHAFTS.SetVector("_SunColor", Vector4.zero); // no backprojection !
            }
          
            cmd.SetGlobalTexture("_ColorBuffer", lrDepthBuffer.Identifier());          
            Blit(cmd, m_TemporaryColorTexture.Identifier(), source, sheetSHAFTS, (screenBlendMode == BlitVolumeFogSRP.BlitSettings.ShaftsScreenBlendMode.Screen) ? 0 : 4);
           
            cmd.ReleaseTemporaryRT(lrDepthBuffer.id);
            cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);           

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
           
            RenderTexture.ReleaseTemporary(lrColorB);
        }


        /////////////////////// VOLUME FOG SRP /////////////////////////////////////
        public void RenderFog(ScriptableRenderContext context, UnityEngine.Rendering.Universal.RenderingData renderingData, CommandBuffer cmd, RenderTextureDescriptor opaqueDesc)
        //public override void Render(PostProcessRenderContext context)
        {
            //var _material = context.propertySheets.Get(Shader.Find("Hidden/InverseProjectVFogLWRP"));
            Material _material = blitMaterial;

            //v1.9.9.5 - Ethereal v1.1.8
            //Add visible lights count - renderingData.cullResults.visibleLights.Length
            _material.SetInt("_visibleLightsCount",renderingData.cullResults.visibleLights.Length);

            _material.SetFloat("_DistanceOffset", _startDistance);
            _material.SetFloat("_Height", _fogHeight); //v0.1                                                                      
            _material.SetFloat("_cameraRoll", _cameraRoll);
            _material.SetVector("_cameraDiff", _cameraDiff);
            _material.SetFloat("_cameraTiltSign", _cameraTiltSign);

            var mode = RenderSettings.fogMode;
            if (mode == FogMode.Linear)
            {
                var start = RenderSettings.fogStartDistance;//RenderSettings.RenderfogStartDistance;
                var end = RenderSettings.fogEndDistance;
                var invDiff = 1.0f / Mathf.Max(end - start, 1.0e-6f);
                _material.SetFloat("_LinearGrad", -invDiff);
                _material.SetFloat("_LinearOffs", end * invDiff);
                _material.DisableKeyword("FOG_EXP");
                _material.DisableKeyword("FOG_EXP2");
            }
            else if (mode == FogMode.Exponential)
            {
                const float coeff = 1.4426950408f; // 1/ln(2)
                var density = RenderSettings.fogDensity;// RenderfogDensity;
                _material.SetFloat("_Density", coeff * density * _fogDensity);
                _material.EnableKeyword("FOG_EXP");
                _material.DisableKeyword("FOG_EXP2");
            }
            else // FogMode.ExponentialSquared
            {
                const float coeff = 1.2011224087f; // 1/sqrt(ln(2))
                var density = RenderSettings.fogDensity;//RenderfogDensity;
                _material.SetFloat("_Density", coeff * density * _fogDensity);
                _material.DisableKeyword("FOG_EXP");
                _material.EnableKeyword("FOG_EXP2");
            }
            if (_useRadialDistance)
                _material.EnableKeyword("RADIAL_DIST");
            else
                _material.DisableKeyword("RADIAL_DIST");

            if (_fadeToSkybox)
            {
                _material.DisableKeyword("USE_SKYBOX");
                _material.SetColor("_FogColor", _FogColor);// RenderfogColor);//v0.1            
            }
            else
            {
                _material.DisableKeyword("USE_SKYBOX");
                _material.SetColor("_FogColor", _FogColor);// RenderfogColor);
            }

            //v0.1 - v1.9.9.2
            //if (noiseTexture == null)
            //{
            //    noiseTexture = new Texture2D(1280, 720);
            //}
            if (_material != null && noiseTexture != null)
            {
                //if (noiseTexture == null)
                //{
                //    noiseTexture = new Texture2D(1280, 720);
                //}
                _material.SetTexture("_NoiseTex", noiseTexture);
            }

            // Calculate vectors towards frustum corners.
            Camera camera = Camera.main;

            if (isForReflections && reflectCamera != null)
            {
                // camera = reflectionc UnityEngine.Rendering.Universal.RenderingData.ca
                // ScriptableRenderContext context, UnityEngine.Rendering.Universal.RenderingData renderingData
                camera = reflectCamera;
            }

            if (isForReflections && isForDualCameras) //v1.9.9.7 - Ethereal v1.1.8f
            {
                //if list has members, choose 0 for 1st etc
                if (extraCameras.Count > 0 && extraCameraID >= 0 && extraCameraID < extraCameras.Count)
                {
                    camera = extraCameras[extraCameraID];
                }
            }

            //v1.7.1 - Solve editor flickering
            if (Camera.current != null)
            {
                camera = Camera.current;
            }

            var cam = camera;// GetComponent<Camera>();
            var camtr = cam.transform;


            ////////// SCATTER
            var camPos = camtr.position;
            float FdotC = camPos.y - _fogHeight;
            float paramK = (FdotC <= 0.0f ? 1.0f : 0.0f);


            ///// ffrustrum
            //float camNear = cam.nearClipPlane;
            //float camFar = cam.farClipPlane;
            //float camFov = cam.fieldOfView;
            //float camAspect = cam.aspect;

            //Matrix4x4 frustumCorners = Matrix4x4.identity;

            //float fovWHalf = camFov * 0.5f;

            //Vector3 toRight = camtr.right * camNear * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * camAspect;
            //Vector3 toTop = camtr.up * camNear * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

            //Vector3 topLeft = (camtr.forward * camNear - toRight + toTop);
            //float camScale = topLeft.magnitude * camFar / camNear;

            //topLeft.Normalize();
            //topLeft *= camScale;

            //Vector3 topRight = (camtr.forward * camNear + toRight + toTop);
            //topRight.Normalize();
            //topRight *= camScale;

            //Vector3 bottomRight = (camtr.forward * camNear + toRight - toTop);
            //bottomRight.Normalize();
            //bottomRight *= camScale;

            //Vector3 bottomLeft = (camtr.forward * camNear - toRight - toTop);
            //bottomLeft.Normalize();
            //bottomLeft *= camScale;

            //frustumCorners.SetRow(0, topLeft);
            //frustumCorners.SetRow(1, topRight);
            //frustumCorners.SetRow(2, bottomRight);
            //frustumCorners.SetRow(3, bottomLeft);

            //_material.SetMatrix("_FrustumCornersWS", frustumCorners);

            _material.SetVector("_CameraWS", camPos);
            _material.SetFloat("blendVolumeLighting", blendVolumeLighting);//v0.2 - SHADOWS
            _material.SetFloat("_RaySamples", LightRaySamples);
            _material.SetVector("_stepsControl", stepsControl);
            _material.SetVector("lightNoiseControl", lightNoiseControl);

            //Debug.Log("_HeightParams="+ new Vector4(_fogHeight, FdotC, paramK, heightDensity * 0.5f));

            _material.SetVector("_HeightParams", new Vector4(_fogHeight, FdotC, paramK, heightDensity * 0.5f));
            _material.SetVector("_DistanceParams", new Vector4(-Mathf.Max(startDistance, 0.0f), 0, 0, 0));
            _material.SetFloat("_NoiseDensity", noiseDensity);
            _material.SetFloat("_NoiseScale", noiseScale);
            _material.SetFloat("_NoiseThickness", noiseThickness);
            _material.SetVector("_NoiseSpeed", noiseSpeed);
            _material.SetFloat("_OcclusionDrop", occlusionDrop);
            _material.SetFloat("_OcclusionExp", occlusionExp);
            _material.SetInt("noise3D", noise3D);
            //SM v1.7
            _material.SetFloat("luminance", luminance);
            _material.SetFloat("lumFac", lumFac);
            _material.SetFloat("Multiplier1", ScatterFac);
            _material.SetFloat("Multiplier2", TurbFac);
            _material.SetFloat("Multiplier3", HorizFac);
            _material.SetFloat("turbidity", turbidity);
            _material.SetFloat("reileigh", reileigh);
            _material.SetFloat("mieCoefficient", mieCoefficient);
            _material.SetFloat("mieDirectionalG", mieDirectionalG);
            _material.SetFloat("bias", bias);
            _material.SetFloat("contrast", contrast);

            //v1.7.1 - Solve editor flickering
            Vector3 sunDir = Sun;// connector.sun.transform.forward;
            if ((Camera.current != null || isForDualCameras) && connector.sun != null) //v1.9.9.2  //v1.9.9.6 - Ethereal v1.1.8e
            {
                sunDir = connector.sun.transform.forward;
                sunDir = Quaternion.AngleAxis(-cam.transform.eulerAngles.y, Vector3.up) * -sunDir;
                sunDir = Quaternion.AngleAxis(cam.transform.eulerAngles.x, Vector3.left) * sunDir;
                sunDir = Quaternion.AngleAxis(-cam.transform.eulerAngles.z, Vector3.forward) * sunDir;
            }

            _material.SetVector("v3LightDir", sunDir);// Sun);//.forward); //v1.7.1
            _material.SetVector("_TintColor", new Vector4(TintColor.r, TintColor.g, TintColor.b, 1));//68, 155, 345
            _material.SetVector("_TintColorK", new Vector4(TintColorK.x, TintColorK.y, TintColorK.z, 1));
            _material.SetVector("_TintColorL", new Vector4(TintColorL.x, TintColorL.y, TintColorL.z, 1));

            //v1.6 - reflections
            if (isForReflections && !isForDualCameras) //v1.9.9.6 - Ethereal v1.1.8e
            {
                _material.SetFloat("_invertX", 1);
            }
            else
            {
                _material.SetFloat("_invertX", 0);
            }

            //v1.7
            _material.SetInt("lightCount", lightCount);

            //v1.9.9
            _material.SetVector("lightControlA", lightControlA);
            _material.SetVector("lightControlB", lightControlB);
            if (lightA)
            {
                _material.SetVector("lightAcolor", new Vector3(lightA.color.r, lightA.color.g, lightA.color.b));
                _material.SetFloat("lightAIntensity", lightA.intensity); 
            }
            if (lightB)
            {
                _material.SetVector("lightBcolor", new Vector3(lightB.color.r, lightB.color.g, lightB.color.b));
                _material.SetFloat("lightBIntensity", lightA.intensity);
            }
            if (controlByColor)
            {
                _material.SetInt("controlByColor", 1);
            }
            else
            {
                _material.SetInt("controlByColor", 0);
            }

            //v1.9.9.3
            _material.SetVector("shadowsControl", shadowsControl);

            //v1.9.9.4
            _material.SetVector("volumeSamplingControl", volumeSamplingControl);

            //v1.9.9.1
            // Debug.Log(_material.HasProperty("lightsArrayLength"));
            //Debug.Log(_material.HasProperty("controlByColor"));
            if (_material.HasProperty("lightsArrayLength") && lightsArray.Count > 0) //check for other shader versions
            {
                //pass array
                _material.SetVectorArray("_LightsArrayPos", new Vector4[32]);
                _material.SetVectorArray("_LightsArrayDir", new Vector4[32]);
                int countLights = lightsArray.Count;
                if(countLights > 32)
                {
                    countLights = 32;
                }
                _material.SetInt("lightsArrayLength", countLights);
                //Debug.Log(countLights);
                // material.SetFloatArray("_Points", new float[10]);
                //float[] array = new float[] { 1, 2, 3, 4 };
                Vector4[] posArray = new Vector4[countLights];
                Vector4[] dirArray = new Vector4[countLights];
                Vector4[] colArray = new Vector4[countLights];
                for (int i=0;i< countLights; i++)
                {
                    //posArray[i].x = lightsArray(0).
                    posArray[i].x = lightsArray[i].transform.position.x;
                    posArray[i].y = lightsArray[i].transform.position.y;
                    posArray[i].z = lightsArray[i].transform.position.z;
                    posArray[i].w = lightsArray[i].intensity;
                    //Debug.Log(posArray[i].w);
                    colArray[i].x = lightsArray[i].color.r;
                    colArray[i].y = lightsArray[i].color.g;
                    colArray[i].z = lightsArray[i].color.b;

                    //check if point light
                    if (lightsArray[i].type == LightType.Point)
                    {
                        dirArray[i].x = 0;
                        dirArray[i].y = 0;
                        dirArray[i].z = 0;
                    }
                    else
                    {
                        dirArray[i].x = lightsArray[i].transform.forward.x;
                        dirArray[i].y = lightsArray[i].transform.forward.y;
                        dirArray[i].z = lightsArray[i].transform.forward.z;
                    }
                    dirArray[i].w = lightsArray[i].range;
                }
                _material.SetVectorArray("_LightsArrayPos", posArray);
                _material.SetVectorArray("_LightsArrayDir", dirArray);
                _material.SetVectorArray("_LightsArrayColor", colArray);
                //material.SetFloatArray(array);
            }
            else
            {
                _material.SetInt("lightsArrayLength", 0);
            }


            float Foggy = 0;
            if (FogSky) //ClearSkyFac
            {
                Foggy = 1;
            }
            _material.SetFloat("FogSky", Foggy);
            _material.SetFloat("ClearSkyFac", ClearSkyFac);
            //////// END SCATTER

            //LOCAL LIGHT
            _material.SetVector("localLightPos", new Vector4(PointL.x, PointL.y, PointL.z, PointL.w));//68, 155, 345
            _material.SetVector("localLightColor", new Vector4(PointLParams.x, PointLParams.y, PointLParams.z, PointLParams.w));//68, 155, 345
                                                                                                                                //END LOCAL LIGHT

            //RENDER FINAL EFFECT
            int rtW = opaqueDesc.width;
            int rtH = opaqueDesc.height;
            //var format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9
            var format = allowHDR ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR; //v3.4.9 //v LWRP

           // Debug.Log(renderingData.cameraData.camera.allowHDR);

            //RenderTexture tmpBuffer1 = RenderTexture.GetTemporary(context.width, context.height, 0, format);
            RenderTexture tmpBuffer1 = RenderTexture.GetTemporary(rtW, rtH, 0, format);
            RenderTexture.active = tmpBuffer1;

            GL.ClearWithSkybox(false, camera);
            ////context.command.BlitFullscreenTriangle(context.source, tmpBuffer1);



            //Blit(cmd, source, m_TemporaryColorTexture.Identifier()); //KEEP BACKGROUND
            Blit(cmd, source, tmpBuffer1); //KEEP BACKGROUND
            // cmd.SetGlobalTexture("_ColorBuffer", lrDepthBuffer.Identifier());
            // Blit(cmd, m_TemporaryColorTexture.Identifier(), source, _material, (screenBlendMode == BlitVolumeFogSRP.BlitSettings.ShaftsScreenBlendMode.Screen) ? 0 : 4);





            _material.SetTexture("_MainTex", tmpBuffer1);

            //WORLD RECONSTRUCT        
            Matrix4x4 camToWorld = camera.cameraToWorldMatrix;// context.camera.cameraToWorldMatrix;
            //Debug.Log(camToWorld);
            _material.SetMatrix("_InverseView", camToWorld);

            /////context.command.BlitFullscreenTriangle(context.source, context.destination, _material, 0);
            //Blit(cmd, m_TemporaryColorTexture.Identifier(), source, _material, (screenBlendMode == BlitVolumeFogSRP.BlitSettings.ShaftsScreenBlendMode.Screen) ? 0 : 4);
            Blit(cmd, tmpBuffer1, source, _material, 6);

            RenderTexture.ReleaseTemporary(tmpBuffer1);
            //END RENDER FINAL EFFECT


            ////RELEASE TEMPORARY TEXTURES AND COMMAND BUFFER
            //cmd.ReleaseTemporaryRT(lrDepthBuffer.id);
            //cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
            context.ExecuteCommandBuffer(cmd);
            //CommandBufferPool.Release(cmd); //DO NOT release fog here because sun shafts may be active v1.9.9.2

        }
        /////////////////////// END VOLUME FOG SRP ///////////////////////////////// 

    }
}
