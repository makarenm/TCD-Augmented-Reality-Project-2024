﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// NOTE: This script is a modified version of the Turret script from the Smart Turret Template. See Source.txt for original link to asset.

[System.Serializable]
public class TurretParameters
{
	[Header("Status")]
	[Tooltip("Activate or deactivate the Turret")]
	public bool active;

	public bool canFire;

	[Header("Shooting")]
	[Tooltip("Burst the force when hit")]
	public float power;

	[Tooltip("Pause between shooting")]
	[Range(0.5f, 2)]
	public float ShootingDelay;

	[Tooltip("Radius of the turret view")]
	public float radius;
}

[System.Serializable]
public class TurretFX
{
	[Tooltip("Muzzle transform position")]
	public Transform muzzle;

	[Tooltip("Spawn this GameObject when shooting")]
	public GameObject shotFX;
}

[System.Serializable]
public class TurretAudio
{
	public AudioClip shotClip;
}

[System.Serializable]
public class TurretTargeting
{
	[Tooltip("Speed of aiming at the target")]
	public float aimingSpeed;

	[Tooltip("Pause before the aiming")]
	public float aimingDelay;

	[Tooltip("GameObject with folowing tags will be identify as enemy")]
	public string[] tagsToFire;

	public List<Collider> targets = new();
	public Collider target;
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(STT_Actor))]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Animator))]
public class STT_Turret : MonoBehaviour
{
	public TurretParameters parameters;
	public TurretTargeting targeting;
	public TurretFX VFX;
	public TurretAudio SFX;

	void Awake()
	{
		GetComponent<SphereCollider>().isTrigger = true;
		GetComponent<SphereCollider>().radius = parameters.radius;
		GetComponent<BoxCollider>().size = new Vector3(2, 2, 2);
		GetComponent<BoxCollider>().center = new Vector3(0, 1, 0);
	}

	void FixedUpdate()
	{
		if (!parameters.active)
		{
			return;
		}

		if (targeting.target == null)
		{
			ClearTargets();
		}
		else
		{
			Aiming();
			Invoke(nameof(Shooting), parameters.ShootingDelay);
		}
	}

	public void UpgradeTo(int lvl)
	{
		switch (lvl)
		{
			case 2:
				parameters.ShootingDelay /= 2;
				break;
			case 3:
				parameters.radius *= 2;
				break;
			case 4:
				parameters.power *= 2;
				break;
			default:
				break;
		}
	}

	#region Aiming and Shooting
	private void Shot()
	{
		GetComponent<AudioSource>().PlayOneShot(SFX.shotClip, Random.Range(0.75f, 1));
		GetComponent<Animator>().SetTrigger("Shot");
		GameObject newShotFX = Instantiate(VFX.shotFX, VFX.muzzle);
		Destroy(newShotFX, 2);
	}

	private void Shooting()
	{
		if (targeting.target == null || !parameters.canFire)
		{
			return;
		}
		if (Physics.Raycast(VFX.muzzle.position, VFX.muzzle.transform.forward, out RaycastHit hit, parameters.radius))
		{
			if (CheckTags(hit.collider))
			{
				Shot();
				hit.collider.GetComponent<STT_Actor>().ReceiveDamage(parameters.power, hit.point);
			}
			ClearTargets();
			CancelInvoke();
		}
	}

	public void Aiming()
	{
		if (targeting.target == null)
		{
			return;
		}
		Vector3 delta = targeting.target.transform.position - transform.position;
		float angle = Vector3.Angle(transform.forward, delta);
		Vector3 cross = Vector3.Cross(transform.forward, delta);
		GetComponent<Rigidbody>().AddTorque(angle * targeting.aimingSpeed * cross);
	}
	#endregion

	#region Targeting
	private void OnTriggerEnter(Collider other)
	{
		if (!parameters.active)
		{
			return;
		}

		ClearTargets();

		if (CheckTags(other))
		{
			if (targeting.targets.Count == 0)
			{
				targeting.target = other.GetComponent<Collider>();
			}
			targeting.targets.Add(other.GetComponent<Collider>());
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!parameters.active)
		{
			return;
		}

		ClearTargets();

		if (CheckTags(other))
		{
			targeting.targets.Remove(other.GetComponent<Collider>());
			if (targeting.targets.Count != 0)
			{
				targeting.target = targeting.targets.First();
			}
			else
			{
				targeting.target = null;
			}
		}
	}

	private bool CheckTags(Collider toMatch)
	{
		bool Match = false;
		foreach (string tag in targeting.tagsToFire)
		{
			if (toMatch.CompareTag(tag))
			{
				Match = true;
			}
		}
		return Match;
	}

	private void ClearTargets()
	{
		if (targeting.target != null)
		{
			if (targeting.target.GetComponent<Collider>().enabled == false)
			{
				targeting.targets.Remove(targeting.target);
			}
		}
		foreach (Collider target in targeting.targets.ToList())
		{
			if (target == null)
			{
				targeting.targets.Remove(target);
			}

			if (targeting.targets.Count != 0)
			{
				targeting.target = targeting.targets.First();
			}
			else
			{
				targeting.target = null;
			}
		}
	}
	#endregion
}