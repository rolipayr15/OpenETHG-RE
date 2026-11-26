using Bibloteka;
using UnityEngine;

public class BreakLessonClock : MonoBehaviour
{
	[SerializeField]
	private MeshFilter meshFilter;

	[SerializeField]
	private Transform clockHand;

	[SerializeField]
	private Transform clockHand2;

	private Mesh mesh;

	private Color breakColor = new Color(0.4f, 0.4f, 0.4f, 0.3f);

	private Color lessonColor = new Color(0f, 0f, 0f, 0.3f);

	private LessonBreakManager lessonManager;

	private bool startWithLesson;

	private float currentPhaseToFullCycleRatio;

	private Quaternion startRotation;

	private Vector3 rotateAround = new Vector3(1f, 0f, 0f);

	private void Start()
	{
		lessonManager = CustomSingleton<LessonBreakManager>.Instance;
		if (meshFilter == null)
		{
			return;
		}
		mesh = meshFilter.mesh;
		UpdateMeshColors();
		if (clockHand != null)
		{
			startRotation = clockHand.localRotation;
		}
	}

	private void Update()
	{
		if (lessonManager == null)
		{
			return;
		}
		Quaternion rotation = startRotation * Quaternion.AngleAxis(lessonManager.CycleProgress * 360f, rotateAround);
		if (clockHand != null)
		{
			clockHand.localRotation = rotation;
			if (clockHand2 != null)
			{
				clockHand2.localRotation = Quaternion.Inverse(rotation);
			}
		}
	}

	private void UpdateMeshColors()
	{
		if (lessonManager == null)
		{
			return;
		}
		startWithLesson = lessonManager.startWithLesson;
		currentPhaseToFullCycleRatio = lessonManager.CurrentPhaseToFullCycleRatio;
		if (mesh == null)
		{
			return;
		}
		Color[] colors = mesh.colors;
		for (int i = 0; i < colors.Length; i++)
		{
			bool afterFirstPhase = currentPhaseToFullCycleRatio <= Utils.RoundToNearest(colors[i].r, 0.05f);
			colors[i] = afterFirstPhase == startWithLesson ? breakColor : lessonColor;
		}
		mesh.colors = colors;
	}
}
