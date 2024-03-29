﻿using System.Collections.Generic;
using Engine.Animation;
using Engine.Core;
using Engine.Core._2D;
using Engine.Core._3D;
using Engine.Interfaces._3D;
using Engine.Lighting;
using Engine.Props;
using Engine.Shaders;
using GlmSharp;

namespace Engine.Graphics._3D.Rendering
{
	public class MasterRenderer3D : IRenderTargetUser3D
	{
		private Shader shadowMapShader;
		private RenderTarget shadowMapTarget;
		private ModelRenderer modelRenderer;
		private SpriteBatch3D spriteBatch3D;
		private SkeletonRenderer skeletonRenderer;

		public MasterRenderer3D()
		{
			// Note that shader attributes aren't set here since custom VAOs are created per renderer.
			shadowMapShader = new Shader();
			shadowMapShader.Attach(ShaderTypes.Vertex, "ShadowMap.vert");
			shadowMapShader.Attach(ShaderTypes.Fragment, "ShadowMap.frag");
			shadowMapShader.Initialize();
			shadowMapShader.Use();
			shadowMapShader.SetUniform("image", 0);

			// These default values are arbitrary, just to make sure something shows up.
			Light = new GlobalLight();
			Light.Direction = vec3.UnitX;
			Light.Color = Color.White;
			Light.AmbientIntensity = 0.1f;

			// TODO: Consider making shadow map size reloadable.
			var accessor = Properties.Access();
			var size = accessor.GetInt("shadow.map.size");

			shadowMapTarget = new RenderTarget(size, size, RenderTargetFlags.Depth);
			modelRenderer = new ModelRenderer(Light);
			spriteBatch3D = new SpriteBatch3D(Light);
			skeletonRenderer = new SkeletonRenderer(Light);

			IsEnabled = true;
		}
		
		public GlobalLight Light { get; }

		// Exposing this publicly is useful for shadow map visualization.
		public RenderTarget ShadowTarget => shadowMapTarget;

		// These are used for debug purposes.
		public ModelRenderer Models => modelRenderer;
		public SpriteBatch3D Sprites => spriteBatch3D;
		public SkeletonRenderer Skeletons => skeletonRenderer;

		// Setting the VP matrix externally allows the same batch of models to be rendered from different perspectives
		// (e.g. reflections in puddles).
		public mat4 VpMatrix { get; set; }

		public float ShadowNearPlane { get; set; }
		public float ShadowFarPlane { get; set; }

		public bool IsEnabled { get; set; }

		public void Dispose()
		{
			shadowMapShader.Dispose();
			shadowMapTarget.Dispose();
			spriteBatch3D.Dispose();
			modelRenderer.Dispose();
			skeletonRenderer.Dispose();
		}

		public void Add(Model model)
		{
			modelRenderer.Add(model);
		}

		public void Add(Sprite3D sprite)
		{
			spriteBatch3D.Add(sprite);
		}

		public void Add(Skeleton skeleton)
		{
			skeletonRenderer.Add(skeleton);
		}

		public void Remove(Model model)
		{
			modelRenderer.Remove(model);
		}

		public void Remove(Sprite3D sprite)
		{
			spriteBatch3D.Remove(sprite);
		}

		public void Remove(Skeleton skeleton)
		{
			skeletonRenderer.Remove(skeleton);
		}

		public void DrawTargets()
		{
			if (!IsEnabled)
			{
				return;
			}

			Light.RecomputeMatrices(VpMatrix);

			shadowMapTarget.Apply();

			DrawShadow(modelRenderer);
			DrawShadow(spriteBatch3D);
			DrawShadow(skeletonRenderer);
		}

		private void DrawShadow<K, V>(AbstractRenderer3D<K, V> renderer) where V : IRenderable3D
		{
			// Skeletons use a custom shadow shader (since skeletal vertices are transformed differently).
			var shadowShader = renderer.ShadowShader ?? shadowMapShader;

			shadowShader.Use();
			renderer.PrepareShadow();

			List<V> items;

			while ((items = renderer.RetrieveNext()) != null)
			{
				foreach (V item in items)
				{
					// Even if the object doesn't cast a shadow, its world matrix is recomputed here for use during
					// normal rendering.
					item.RecomputeWorldMatrix();

					if (!item.IsShadowCaster)
					{
						continue;
					}

					shadowShader.SetUniform("lightMatrix", Light.Matrix * item.WorldMatrix);
					renderer.Draw(item, null);
				}
			}
		}

		public void Draw()
		{
			if (!IsEnabled)
			{
				return;
			}

			shadowMapTarget.Bind(1);

			Draw(modelRenderer);
			Draw(spriteBatch3D);
			Draw(skeletonRenderer);
		}

		private void Draw<K, V>(AbstractRenderer3D<K, V> renderer) where V : IRenderable3D
		{
			renderer.Prepare();

			List<V> items;

			while ((items = renderer.RetrieveNext()) != null)
			{
				foreach (V item in items)
				{
					renderer.Draw(item, VpMatrix);
				}
			}
		}
	}
}
