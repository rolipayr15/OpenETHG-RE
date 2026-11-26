using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Graphic))]
public class SprintIconPulse : BaseMeshEffect
{
	[SerializeField]
	[Min(0f)]
	private float pulseStrength = 0.04f;

	[SerializeField]
	[Min(0.1f)]
	private float pulseDuration = 1.8f;

	private float pulseTime;

	private float currentScale = 1f;

	private bool isPulsing;

	private void Update()
	{
		if (!isPulsing)
		{
			return;
		}
		pulseTime += Time.unscaledDeltaTime;
		currentScale = pulseStrength * Mathf.Sin(pulseTime / pulseDuration * Mathf.PI * 2f) + 1f;
		graphic.SetVerticesDirty();
	}

	public override void ModifyMesh(VertexHelper vertexHelper)
	{
		if (!IsActive() || Mathf.Approximately(currentScale, 1f))
		{
			return;
		}
		Rect rect = graphic.rectTransform.rect;
		Vector2 center = rect.center;
		UIVertex vertex = default;
		for (int i = 0; i < vertexHelper.currentVertCount; i++)
		{
			vertexHelper.PopulateUIVertex(ref vertex, i);
			vertex.position.x = center.x + (vertex.position.x - center.x) * currentScale;
			vertex.position.y = center.y + (vertex.position.y - center.y) * currentScale;
			vertexHelper.SetUIVertex(vertex, i);
		}
	}

	public void StartPulse()
	{
		if (!isPulsing)
		{
			isPulsing = true;
			pulseTime = 0f;
			currentScale = 1f;
		}
	}

	public void StopPulse()
	{
		isPulsing = false;
		pulseTime = 0f;
		currentScale = 1f;
		graphic.SetVerticesDirty();
	}

	public void SetPulsing(bool shouldPulse)
	{
		if (shouldPulse)
		{
			StartPulse();
		}
		else
		{
			StopPulse();
		}
	}

	protected override void OnDisable()
	{
		isPulsing = false;
		pulseTime = 0f;
		currentScale = 1f;
		base.OnDisable();
	}
}
