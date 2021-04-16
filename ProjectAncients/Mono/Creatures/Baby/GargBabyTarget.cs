﻿using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace ProjectAncients.Mono
{
	public class GargBabyTarget : HandTarget, IHandTarget
	{
		public bool cinematicPlaying = false;
		Animator animator;
		LiveMixin lm;
		SwimBehaviour swimBehaviour;

		void Start()
		{
			animator = transform.parent.GetComponentInChildren<Animator>();
			swimBehaviour = GetComponentInParent<SwimBehaviour>();
			lm = GetComponentInParent<LiveMixin>();
			gameObject.layer = 13;
		}
		public void OnHandHover(GUIHand hand)
		{
			if (CanInteract())
			{
				HandReticle.main.SetInteractText("PlayWithFish", true, HandReticle.Hand.Right);
				HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
			}
		}

		public void OnHandClick(GUIHand hand)
		{
			if (CanInteract())
			{
				PlayCinematic();
			}
		}

		public void PlayCinematic()
		{
			StartCoroutine(Cinematic());
		}
		private IEnumerator Cinematic()
		{
			cinematicPlaying = true;
			swimBehaviour.Idle();
			swimBehaviour.LookAt(Player.main.transform);
			animator.SetFloat("random", Random.value);
			animator.SetTrigger("cin_play");
			yield return new WaitForSeconds(6f);
			swimBehaviour.LookAt(null);
			cinematicPlaying = false;
		}

		bool CanInteract()
		{
			if (cinematicPlaying)
			{
				return false;
			}
			if (!lm.IsAlive())
			{
				return false;
			}
			return true;
		}
	}
}
