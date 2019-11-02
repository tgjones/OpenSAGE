﻿using OpenSage.Graphics;
using OpenSage.Graphics.Shaders;
using Veldrid;

namespace OpenSage.Content.Loaders
{
    internal sealed class AssetLoadContext
    {
        public string Language { get; }
        public GraphicsDevice GraphicsDevice { get; }
        public StandardGraphicsResources StandardGraphicsResources { get; }
        public ShaderResourceManager ShaderResources { get; }
        public AssetStore AssetStore { get; }

        public AssetLoadContext(
            string language,
            GraphicsDevice graphicsDevice,
            StandardGraphicsResources standardGraphicsResources,
            ShaderResourceManager shaderResources,
            AssetStore assetStore)
        {
            Language = language;
            GraphicsDevice = graphicsDevice;
            StandardGraphicsResources = standardGraphicsResources;
            ShaderResources = shaderResources;
            AssetStore = assetStore;
        }
    }
}
