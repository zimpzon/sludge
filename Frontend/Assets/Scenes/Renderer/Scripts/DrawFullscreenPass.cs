//namespace UnityEngine.Rendering.Universal
//{
//    /// <summary>
//    /// Draws full screen mesh using given material and pass and reading from source target.
//    /// </summary>
//    internal class DrawFullscreenPass : ScriptableRenderPass
//    {
//        public FilterMode filterMode { get; set; }
//        public DrawFullscreenFeature.Settings settings;

//        RTHandle source;
//        RTHandle destination;

//        int sourceId;
//        int destinationId;
//        bool isSourceAndDestinationSameTarget;

//        string m_ProfilerTag;

//        public DrawFullscreenPass(string tag)
//        {
//            m_ProfilerTag = tag;
//        }

//        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
//        {
//            RenderTextureDescriptor blitTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
//            blitTargetDescriptor.depthBufferBits = 0;

//            var renderer = renderingData.cameraData.renderer;

//            // src = camera color
//            sourceId = -1;
//            source = renderer.cameraColorTargetHandle;

//            // dst = camera color
//            destinationId = -1;
//            destination = renderer.cameraColorTargetHandle;
//        }

//        /// <inheritdoc/>
//        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//        {
//            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

//            // Can't read and write to same color target, create a temp render target to blit. 
//            if (isSourceAndDestinationSameTarget)
//            {
//                Blit(cmd, source, destination, settings.blitMaterial, settings.blitMaterialPassIndex);
//                Blit(cmd, destination, source);
//            }
//            else
//            {
//                Blit(cmd, source, destination, settings.blitMaterial, settings.blitMaterialPassIndex);
//            }

//            context.ExecuteCommandBuffer(cmd);
//            CommandBufferPool.Release(cmd);
//        }

//        /// <inheritdoc/>
//        public override void FrameCleanup(CommandBuffer cmd)
//        {
//            if (destinationId != -1)
//                cmd.ReleaseTemporaryRT(destinationId);

//            if (source == destination && sourceId != -1)
//                cmd.ReleaseTemporaryRT(sourceId);
//        }
//    }
//}
