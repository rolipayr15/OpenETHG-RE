using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class KrasueDarkFog : MonoBehaviour
{
	public sealed class LayerIndexAttribute : PropertyAttribute
	{
	}

	private class FogLayer
	{
		public ParticleSystem particles;

		public float radiusScale;

		public float heightScale;

		public float alpha;
	}

	[SerializeField]
	private Transform redAirRoot;

	[SerializeField]
	private Material fogMaterial;

	[SerializeField]
	[LayerIndex]
	private int fogLayer;

	[SerializeField]
	private float heightOffset = 1.05f;

	[SerializeField]
	private float radius = 1f;

	[SerializeField]
	[Range(0.05f, 1f)]
	private float thickness = 0.65f;

	[SerializeField]
	[Range(0.05f, 0.5f)]
	private float followLag = 0.14f;

	[SerializeField]
	[Range(0.4f, 2f)]
	private float tailLength = 1.5f;

	[SerializeField]
	private Color fogColor = new Color(0.22f, 0.28f, 0.24f, 0.9f);

	private readonly FogLayer[] fogLayers = new FogLayer[4];

	private Transform fogRoot;

	private ParticleSystem tailParticles;

	private Material runtimeMaterial;

	private Vector3 followVelocity;

	private bool snapped;

	private float lastRadius;

	private float lastThickness;

	private float lastTailLength;

	private Color lastFogColor;

	private static Texture2D sharedFogTexture;

	private bool EnsureFogCreated()
	{
		if (fogRoot != null)
		{
			return true;
		}
		runtimeMaterial = CreateRuntimeMaterial();
		if (runtimeMaterial == null)
		{
			return false;
		}
		GameObject root = new GameObject("KrasueSwampFogRoot");
		root.layer = Mathf.Clamp(fogLayer, 0, 31);
		fogRoot = root.transform;
		fogRoot.SetParent(null, true);
		fogLayers[0] = CreateFogSystem("SwampFogSheets", 44, new Vector2(3f, 4.2f), new Vector2(0.95f, 1.5f), new Vector2(0.4f, 0.68f), 11f, 1f, 0.75f, 0.32f, 0.45f, 0.055f, new Vector2(0.02f, 0.07f), -0.1f, 0f, 0.38f, 0.1f, ParticleSystemSimulationSpace.Local, 0f);
		fogLayers[1] = CreateFogSystem("SwampFogFill", 30, new Vector2(2.3f, 3.3f), new Vector2(0.8f, 1.35f), new Vector2(0.45f, 0.78f), 9f, 0.75f, 1f, 0.28f, 0.35f, 0.04f, new Vector2(0.04f, 0.11f), 0.08f, 0.12f, 0.55f, 0.14f, ParticleSystemSimulationSpace.Local, 0f);
		fogLayers[2] = CreateFogSystem("SwampFogTendrils", 18, new Vector2(2f, 3f), new Vector2(0.7f, 1.1f), new Vector2(0.35f, 0.58f), 6f, 0.62f, 0.95f, 0.26f, 0.4f, 0.03f, new Vector2(0.1f, 0.2f), 0.22f, 0.35f, 0.5f, 0.22f, ParticleSystemSimulationSpace.Local, 0f);
		// The tail lives in world space so it trails behind the flying head.
		fogLayers[3] = CreateFogSystem("SwampFogTail", 48, TailLifetime(), new Vector2(0.62f, 1f), new Vector2(0.24f, 0.4f), 0f, 0.42f, 0.42f, 0.24f, 0.24f, 0.028f, new Vector2(0.015f, 0.055f), -0.08f, 0f, 0.35f, 0.22f, ParticleSystemSimulationSpace.World, 12f);
		tailParticles = fogLayers[3].particles;
		ApplyLiveSettings(true);
		SnapToTarget();
		return true;
	}

	private void OnQualityChanged(GraphicsQuality quality)
	{
		ApplyQuality(quality, true);
	}

	// Low quality drops the fog and the red air entirely.
	private void ApplyQuality(GraphicsQuality quality, bool restartFog)
	{
		bool enabledForQuality = quality != GraphicsQuality.Low;
		if (redAirRoot != null && redAirRoot.gameObject.activeSelf != enabledForQuality)
		{
			redAirRoot.gameObject.SetActive(enabledForQuality);
		}
		if (!enabledForQuality || EnsureFogCreated())
		{
			SetFogVisible(enabledForQuality, restartFog);
		}
	}

	// The soft-particle shader on Best quality needs the camera depth texture.
	private void OnCameraPreCull(Camera camera)
	{
		if (GraphicsQualityManager.CurrentQuality != GraphicsQuality.Best || camera == null || camera.cameraType != CameraType.Game || fogRoot == null || !fogRoot.gameObject.activeInHierarchy)
		{
			return;
		}
		if ((camera.cullingMask & (1 << fogRoot.gameObject.layer)) != 0 && (camera.depthTextureMode & DepthTextureMode.Depth) == 0)
		{
			camera.depthTextureMode |= DepthTextureMode.Depth;
		}
	}

	private void LateUpdate()
	{
		if (fogRoot == null || !fogRoot.gameObject.activeSelf)
		{
			return;
		}
		Vector3 target = transform.position + Vector3.up * heightOffset;
		if (snapped && (fogRoot.position - target).sqrMagnitude <= 25f)
		{
			fogRoot.position = Vector3.SmoothDamp(fogRoot.position, target, ref followVelocity, followLag, float.PositiveInfinity, Time.deltaTime);
			Vector3 offset = fogRoot.position - target;
			float maxDistance = Mathf.Max(0.12f, radius * 0.3f);
			float sqrDistance = offset.sqrMagnitude;
			if (sqrDistance > maxDistance * maxDistance)
			{
				float distance = Mathf.Sqrt(sqrDistance);
				Vector3 direction = distance > 1E-05f ? offset / distance : Vector3.zero;
				fogRoot.position = target + direction * maxDistance;
			}
		}
		else
		{
			// Teleports (and the first frame) jump straight to the target and drop the stale tail.
			SnapToTarget(true);
		}
		ApplyLiveSettings(false);
	}

	private void OnEnable()
	{
		GraphicsQualityManager.QualityChanged -= OnQualityChanged;
		GraphicsQualityManager.QualityChanged += OnQualityChanged;
		Camera.onPreCull -= OnCameraPreCull;
		Camera.onPreCull += OnCameraPreCull;
		ApplyQuality(GraphicsQualityManager.CurrentQuality, true);
	}

	private void OnDisable()
	{
		GraphicsQualityManager.QualityChanged -= OnQualityChanged;
		Camera.onPreCull -= OnCameraPreCull;
		if (fogRoot != null)
		{
			fogRoot.gameObject.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		GraphicsQualityManager.QualityChanged -= OnQualityChanged;
		Camera.onPreCull -= OnCameraPreCull;
		if (fogRoot != null)
		{
			Destroy(fogRoot.gameObject);
		}
		if (runtimeMaterial != null)
		{
			Destroy(runtimeMaterial);
		}
	}

	private void SnapToTarget(bool clearTail = false)
	{
		bool tailWasPlaying = false;
		if (clearTail)
		{
			tailWasPlaying = tailParticles != null && tailParticles.isPlaying;
			if (tailParticles != null)
			{
				tailParticles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
		}
		followVelocity = Vector3.zero;
		if (fogRoot != null)
		{
			fogRoot.position = transform.position + Vector3.up * heightOffset;
		}
		if (tailWasPlaying && fogRoot != null && fogRoot.gameObject.activeInHierarchy)
		{
			tailParticles.Play(false);
		}
		snapped = true;
	}

	private void SetFogVisible(bool visible, bool restartParticles)
	{
		if (fogRoot == null)
		{
			return;
		}
		fogRoot.gameObject.SetActive(visible);
		if (!visible)
		{
			return;
		}
		SnapToTarget();
		if (!restartParticles)
		{
			return;
		}
		for (int i = 0; i < fogLayers.Length; i++)
		{
			FogLayer layer = fogLayers[i];
			if (layer == null || layer.particles == null)
			{
				continue;
			}
			if (layer.particles == tailParticles)
			{
				layer.particles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
			layer.particles.Play(true);
		}
	}

	// Pushes inspector tweaks (radius, thickness, tail, colour) into the running systems.
	private void ApplyLiveSettings(bool force)
	{
		bool radiusChanged;
		bool thicknessChanged;
		bool tailChanged;
		bool colorChanged;
		if (force)
		{
			radiusChanged = true;
			thicknessChanged = true;
			tailChanged = true;
			colorChanged = true;
		}
		else
		{
			radiusChanged = radius != lastRadius;
			thicknessChanged = thickness != lastThickness;
			tailChanged = tailLength != lastTailLength;
			colorChanged = fogColor != lastFogColor;
			if (!radiusChanged && !thicknessChanged && !tailChanged && !colorChanged)
			{
				return;
			}
		}
		lastRadius = radius;
		lastThickness = thickness;
		lastTailLength = tailLength;
		lastFogColor = fogColor;
		if (tailChanged && tailParticles != null)
		{
			ParticleSystem.MainModule tailMain = tailParticles.main;
			Vector2 lifetime = TailLifetime();
			tailMain.startLifetime = new ParticleSystem.MinMaxCurve(lifetime.x, lifetime.y);
		}
		for (int i = 0; i < fogLayers.Length; i++)
		{
			FogLayer layer = fogLayers[i];
			if (layer == null || layer.particles == null)
			{
				continue;
			}
			if (radiusChanged)
			{
				ParticleSystem.ShapeModule shape = layer.particles.shape;
				shape.radius = radius * layer.radiusScale;
				shape.scale = new Vector3(1f, layer.heightScale, 1f);
			}
			if (thicknessChanged || colorChanged)
			{
				ParticleSystem.MainModule main = layer.particles.main;
				main.startColor = CreateFogColor(layer.alpha);
			}
		}
	}

	private Vector2 TailLifetime()
	{
		return new Vector2(tailLength * 0.75f, tailLength * 1.15f);
	}

	private ParticleSystem.MinMaxGradient CreateFogColor(float alpha)
	{
		float a = fogColor.a * alpha * thickness;
		return new ParticleSystem.MinMaxGradient(new Color(fogColor.r * 0.72f, fogColor.g * 0.78f, fogColor.b * 0.7f, a * 0.78f), new Color(fogColor.r * 1.15f, fogColor.g * 1.25f, fogColor.b * 1.05f, a));
	}

	private Material CreateRuntimeMaterial()
	{
		Material material;
		if (fogMaterial != null)
		{
			material = new Material(fogMaterial);
		}
		else
		{
			Shader shader = Shader.Find("Eyes/KrasueDarkFog");
			if (shader == null)
			{
				return null;
			}
			material = new Material(shader);
		}
		material.name = "KrasueSwampFog_Runtime";
		material.hideFlags = HideFlags.HideAndDontSave;
		material.mainTexture = GetFogTexture();
		material.SetColor("_TintColor", Color.white);
		return material;
	}

	// Procedural wispy sheet: a wavy horizontal band, faded at the sides, broken up by value noise.
	private static Texture2D GetFogTexture()
	{
		if (sharedFogTexture != null)
		{
			return sharedFogTexture;
		}
		Texture2D texture = new Texture2D(128, 64, TextureFormat.RGBA32, false, true);
		texture.name = "KrasueSwampFogWisp";
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.filterMode = FilterMode.Bilinear;
		texture.hideFlags = HideFlags.HideAndDontSave;
		Color32[] pixels = new Color32[8192];
		for (int y = 0; y < 64; y++)
		{
			float v = (y + 0.5f) / 64f;
			float cy = v * 2f - 1f;
			for (int x = 0; x < 128; x++)
			{
				float u = (x + 0.5f) / 128f;
				float coarse = ValueNoise(u * 7f, v * 4f);
				float fine = ValueNoise(u * 17f + 5.3f, v * 9f + 2.1f);
				float cx = u * 2f - 1f;
				float edge = Mathf.Clamp01((1f - Mathf.Abs(cx)) * 3.2f);
				float wave = Mathf.Sin(cx * 5.5f) * 0.1f + Mathf.Sin(cx * 12f + 1.7f) * 0.035f;
				float band = Mathf.Clamp01(1f - Mathf.Abs(cy - wave) / 0.88f);
				float density = Mathf.Clamp01(coarse * 0.45f + 0.35f + fine * 0.2f);
				float shape = SmoothStep01(edge) * SmoothStep01(band);
				float breakup = Mathf.Lerp(0.18f, 0.82f, SmoothStep01(density)) * 0.65f + 0.35f;
				float alpha = Mathf.Pow(Mathf.Clamp01(shape * breakup), 1.35f);
				pixels[y * 128 + x] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)(alpha * 255f));
			}
		}
		texture.SetPixels32(pixels);
		texture.Apply(false, true);
		sharedFogTexture = texture;
		return sharedFogTexture;
	}

	private static float SmoothStep01(float t)
	{
		return t * t * (3f - 2f * t);
	}

	private static float ValueNoise(float x, float y)
	{
		int ix = Mathf.FloorToInt(x);
		int iy = Mathf.FloorToInt(y);
		float fx = x - ix;
		float fy = y - iy;
		float sx = Mathf.Clamp01(fx * fx * (3f - 2f * fx));
		float sy = Mathf.Clamp01(fy * fy * (3f - 2f * fy));
		float bottom = Mathf.Lerp(Hash(ix, iy), Hash(ix + 1, iy), sx);
		float top = Mathf.Lerp(Hash(ix, iy + 1), Hash(ix + 1, iy + 1), sx);
		return Mathf.Lerp(bottom, top, sy);
	}

	private static float Hash(int x, int y)
	{
		unchecked
		{
			uint h = (uint)(x * 0x165667b1 + y * 0x27d4eb2f);
			h ^= h >> 13;
			return ((h * 0xf19f61u) & 0xffffffu) / 16777215f;
		}
	}

	private FogLayer CreateFogSystem(string objectName, int maxParticles, Vector2 lifetime, Vector2 startWidth, Vector2 startHeight, float emissionRate, float radiusScale, float heightScale, float maxScreenSize, float alpha, float drift, Vector2 verticalDrift, float verticalOffset, float rotationCenter, float rotationSpread, float lengthGrowth, ParticleSystemSimulationSpace simulationSpace, float emissionRateOverDistance)
	{
		GameObject go = new GameObject(objectName);
		go.layer = Mathf.Clamp(fogLayer, 0, 31);
		go.transform.SetParent(fogRoot, false);
		go.transform.localPosition = Vector3.up * verticalOffset;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;
		ParticleSystem particleSystem = go.AddComponent<ParticleSystem>();
		particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		bool world = simulationSpace == ParticleSystemSimulationSpace.World;

		ParticleSystem.MainModule main = particleSystem.main;
		main.loop = true;
		main.prewarm = emissionRate > 0f;
		main.playOnAwake = true;
		main.duration = 4f;
		main.maxParticles = maxParticles;
		main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime.x, lifetime.y);
		main.startSpeed = new ParticleSystem.MinMaxCurve(0f, 0.025f);
		main.startSize3D = true;
		main.startSizeX = new ParticleSystem.MinMaxCurve(startWidth.x, startWidth.y);
		main.startSizeY = new ParticleSystem.MinMaxCurve(startHeight.x, startHeight.y);
		main.startSizeZ = new ParticleSystem.MinMaxCurve(1f, 1f);
		main.startRotation = new ParticleSystem.MinMaxCurve(rotationCenter - rotationSpread, rotationCenter + rotationSpread);
		main.startColor = CreateFogColor(alpha);
		main.simulationSpace = simulationSpace;
		main.scalingMode = world ? ParticleSystemScalingMode.Hierarchy : ParticleSystemScalingMode.Local;
		main.gravityModifier = 0f;
		main.cullingMode = world ? ParticleSystemCullingMode.AlwaysSimulate : ParticleSystemCullingMode.PauseAndCatchup;

		ParticleSystem.EmissionModule emission = particleSystem.emission;
		emission.enabled = true;
		emission.rateOverTime = emissionRate;
		emission.rateOverDistance = emissionRateOverDistance;

		ParticleSystem.ShapeModule shape = particleSystem.shape;
		shape.enabled = true;
		shape.shapeType = ParticleSystemShapeType.Sphere;
		shape.radius = radius * radiusScale;
		shape.radiusThickness = 1f;
		shape.scale = new Vector3(1f, heightScale, 1f);

		ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
		colorOverLifetime.enabled = true;
		Gradient gradient = new Gradient();
		gradient.SetKeys(new GradientColorKey[2]
		{
			new GradientColorKey(Color.white, 0f),
			new GradientColorKey(Color.white, 1f)
		}, new GradientAlphaKey[4]
		{
			new GradientAlphaKey(0f, 0f),
			new GradientAlphaKey(0.82f, 0.12f),
			new GradientAlphaKey(0.72f, 0.82f),
			new GradientAlphaKey(0f, 1f)
		});
		colorOverLifetime.color = gradient;

		ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
		sizeOverLifetime.enabled = true;
		sizeOverLifetime.separateAxes = true;
		sizeOverLifetime.x = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.9f, 1f, 1f + lengthGrowth));
		sizeOverLifetime.y = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.82f, 1f, 1.02f));
		sizeOverLifetime.z = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 1f));

		ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particleSystem.velocityOverLifetime;
		velocityOverLifetime.enabled = true;
		velocityOverLifetime.space = simulationSpace;
		velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-drift, drift);
		velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(verticalDrift.x, verticalDrift.y);
		velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-drift, drift);

		ParticleSystem.RotationOverLifetimeModule rotationOverLifetime = particleSystem.rotationOverLifetime;
		rotationOverLifetime.enabled = true;
		rotationOverLifetime.separateAxes = true;
		rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.055f, 0.055f);

		ParticleSystem.NoiseModule noise = particleSystem.noise;
		noise.enabled = true;
		noise.separateAxes = true;
		noise.strengthX = 0.07f;
		noise.strengthY = 0.018f;
		noise.strengthZ = 0.07f;
		noise.frequency = 0.16f;
		noise.scrollSpeed = 0.07f;
		noise.octaveCount = 1;
		noise.quality = ParticleSystemNoiseQuality.Low;
		noise.damping = true;

		DisableExpensiveModules(particleSystem);

		if (go.TryGetComponent(out ParticleSystemRenderer particleRenderer))
		{
			particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
			particleRenderer.alignment = ParticleSystemRenderSpace.View;
			particleRenderer.sharedMaterial = runtimeMaterial;
			particleRenderer.shadowCastingMode = ShadowCastingMode.Off;
			particleRenderer.receiveShadows = false;
			particleRenderer.lightProbeUsage = LightProbeUsage.Off;
			particleRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
			particleRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
			particleRenderer.minParticleSize = 0f;
			particleRenderer.maxParticleSize = maxScreenSize;
			particleRenderer.allowRoll = true;
			particleRenderer.pivot = Vector3.zero;
			particleRenderer.sortMode = ParticleSystemSortMode.Distance;
			particleRenderer.sortingOrder = 2;
		}
		particleSystem.Play(true);
		return new FogLayer
		{
			particles = particleSystem,
			radiusScale = radiusScale,
			heightScale = heightScale,
			alpha = alpha
		};
	}

	private static void DisableExpensiveModules(ParticleSystem particleSystem)
	{
		ParticleSystem.CollisionModule collision = particleSystem.collision;
		collision.enabled = false;
		ParticleSystem.TriggerModule trigger = particleSystem.trigger;
		trigger.enabled = false;
		ParticleSystem.SubEmittersModule subEmitters = particleSystem.subEmitters;
		subEmitters.enabled = false;
		ParticleSystem.LightsModule lights = particleSystem.lights;
		lights.enabled = false;
		ParticleSystem.TrailModule trails = particleSystem.trails;
		trails.enabled = false;
		ParticleSystem.InheritVelocityModule inheritVelocity = particleSystem.inheritVelocity;
		inheritVelocity.enabled = false;
		ParticleSystem.ForceOverLifetimeModule forceOverLifetime = particleSystem.forceOverLifetime;
		forceOverLifetime.enabled = false;
		ParticleSystem.ExternalForcesModule externalForces = particleSystem.externalForces;
		externalForces.enabled = false;
		ParticleSystem.TextureSheetAnimationModule textureSheetAnimation = particleSystem.textureSheetAnimation;
		textureSheetAnimation.enabled = false;
		ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetime = particleSystem.limitVelocityOverLifetime;
		limitVelocityOverLifetime.enabled = false;
	}
}
