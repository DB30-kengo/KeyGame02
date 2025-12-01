#if UNITY_EDITOR
using System;
using UnityEngine;

namespace UnityEngine.Rendering.Sampling
{
    internal class SamplingResources
    {
    }
}

namespace UnityEngine.Rendering.UnifiedRayTracing
{
    internal enum RayTracingBackend
    {
        None,
    }

    internal interface IRayTracingShader
    {
    }

    internal class RayTracingResources : IDisposable
    {
        public void Dispose()
        {
        }
    }

    internal class RayTracingContext : IDisposable
    {
        public RayTracingBackend backend = RayTracingBackend.None;

        public void Dispose()
        {
        }
    }

    internal class AccelStructAdapter : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
#endif