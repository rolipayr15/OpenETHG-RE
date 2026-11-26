using System.Collections.Generic;
using UnityEngine;

namespace Characters.Mover
{
	// Menu diorama goose: keeps picking idle clips from GooseDioramaChain, avoiding clips its neighbours are playing.
	public class GooseDioramaIdle : MonoBehaviour
	{
		[SerializeField]
		private Animator anim;

		[SerializeField]
		private GooseDioramaIdle[] others;

		[SerializeField]
		private float animatorSpeed = 1f;

		[Tooltip("Loops of the current clip before the next pick (random in this range).")]
		[SerializeField]
		private Vector2 loopsPerClip = new Vector2(2f, 4f);

		[SerializeField]
		private float crossFadeSeconds = 0.3f;

		private float switchAtNormalizedTime;

		public string Current { get; private set; }

		private void OnEnable()
		{
			if (anim == null)
			{
				return;
			}
			anim.speed = animatorSpeed;
			string first = GooseDioramaChain.Next(null, Taken(), Random.value, Random.value);
			// Random phase so several geese never move in sync.
			Play(first, Random.Range(0f, 0.9f));
		}

		private void Update()
		{
			if (anim == null || Current == null)
			{
				return;
			}
			AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
			if (anim.IsInTransition(0) || state.normalizedTime < switchAtNormalizedTime)
			{
				return;
			}
			Play(GooseDioramaChain.Next(Current, Taken(), Random.value, Random.value), 0f);
		}

		private List<string> Taken()
		{
			List<string> taken = new List<string>();
			if (others != null)
			{
				foreach (GooseDioramaIdle other in others)
				{
					if (other != null && other != this && other.Current != null)
					{
						taken.Add(other.Current);
					}
				}
			}
			return taken;
		}

		private void Play(string state, float startOffset)
		{
			Current = state;
			switchAtNormalizedTime = Random.Range(loopsPerClip.x, loopsPerClip.y) + startOffset;
			anim.CrossFade(state, crossFadeSeconds, 0, startOffset);
		}
	}
}
