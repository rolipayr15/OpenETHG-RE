using UnityEngine;

public class HerdSimCore : MonoBehaviour
{
	public HerdSimController _controller;

	public Transform _scanner;

	public Transform _collider;

	public Transform _model;

	public Renderer _renderer;

	public float _hitPoints;

	public int _type;

	public float _minSize;

	public float _avoidAngle;

	public float _avoidDistance;

	public float _avoidSpeed;

	public float _stopDistance;

	private float _rotateCounterR;

	private float _rotateCounterL;

	public bool _pushHalfTheTime;

	private bool _pushToggle;

	public float _pushDistance;

	public float _pushForce;

	private bool _scan;

	public Vector3 _roamingArea;

	public float _walkSpeed;

	public float _runSpeed;

	public float _damping;

	public int _idleProbablity;

	public float _runChance;

	public Vector3 _waypoint;

	public float _speed;

	public float _targetSpeed;

	public int _mode;

	public Vector3 _startPosition;

	private bool _reachedWaypoint;

	private int _lerpCounter;

	public bool _scared;

	public Transform _scaredOf;

	public bool _eating;

	public Transform _food;

	public float _groundCheckInterval;

	public string _groundTag;

	private Vector3 _ground;

	private Quaternion _groundRot;

	private bool _grounded;

	public float _maxGroundAngle;

	public float _maxFall;

	public float _fakeGravity;

	public LayerMask _herdLayerMask;

	public HerdSimCore _leader;

	public Vector3 _leaderArea;

	public int _leaderSize;

	public float _leaderAreaMultiplier;

	public int _maxHerdSize;

	public int _minHerdSize;

	public float _herdDistance;

	private int _herdSize;

	public bool _dead;

	public float _randomDeath;

	public Material _deadMaterial;

	public bool _scaryCorpse;

	public string _animIdle;

	public float _animIdleSpeed;

	public string _animSleep;

	public float _animSleepSpeed;

	public string _animWalk;

	public float _animWalkSpeed;

	public string _animRun;

	public float _animRunSpeed;

	public string _animDead;

	public float _animDeadSpeed;

	public float _idleToSleepSeconds;

	private float _sleepCounter;

	private bool _idle;

	private int _updateCounter;

	public int _updateDivisor;

	private static int _updateNextSeed;

	private int _updateSeed;

	private float _newDelta;

	public bool _enabled;

	public LayerMask _groundLayerMask;

	public LayerMask _pushyLayerMask;

	public string _herdSimLayerName;

	private int _groundIndex;

	private int _herdSimIndex;

	private Transform _thisTR;

	public void Disable(bool disableModel, bool disableCollider)
	{
	}

	public void Enable()
	{
	}

	public void Damage(float d)
	{
	}

	public void Effects()
	{
	}

	public void Death()
	{
	}

	public void Corpse()
	{
	}

	public void FindLeader()
	{
	}

	public void Wander()
	{
	}

	public void Init()
	{
	}

	public void Start()
	{
	}

	public void AnimationHandler()
	{
	}

	public void Scare(Transform t)
	{
	}

	public void EndScare()
	{
	}

	public void Food(Transform t)
	{
	}

	public void Pushy()
	{
	}

	public void UnFlock()
	{
	}

	public void WalkTimeOut()
	{
	}

	public void Update()
	{
	}

	public void GroundCheck()
	{
	}

	public bool Avoidance()
	{
		return false;
	}

	public void OnDrawGizmos()
	{
	}
}
