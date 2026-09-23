using BS.Physics;
using BS.Player;
using Unity.Collections;
using Unity.U2D.Physics;
using UnityEngine;

// All Codes worked well on 2020-07-06

public class ctw_Platform_behavior : MonoBehaviour
{
	public bool Trigger = false;

	[SerializeField] float _solidAbove = 0.2f;
	[SerializeField] float _passBelow = 1f;
	[SerializeField] float _dropRange = 3f;
	[SerializeField] float _dropCooldown = 0.5f;

	PhysicsBodyComponent _bodyComponent;
	PlayerController _player;
	float _dropRemaining;
	PhysicsMask _solidContacts;
	bool _appliedTrigger;
	bool _hasApplied;

	void Awake()
	{
		_bodyComponent = GetComponent<PhysicsBodyComponent>();

		foreach (Collider2D legacy in GetComponents<Collider2D>())
			legacy.enabled = false;
	}

	void Start()
	{
		_player = FindAnyObjectByType<PlayerController>();
	}

	void Control()
	{
		if (_player == null)
			return;

		float playerY = _player.Position.y;
		float platformY = transform.position.y;

		if (_dropRemaining > 0f)
		{
			_dropRemaining -= Time.deltaTime;
			return;
		}

		if (playerY >= platformY + _solidAbove)
		{
			Trigger = false;

			if (_player._down && playerY - platformY <= _dropRange)
			{
				Trigger = true;
				_dropRemaining = _dropCooldown;
			}
		}

		if (playerY < platformY - _passBelow)
			Trigger = true;
	}

	void ApplyTrigger()
	{
		if (_hasApplied && _appliedTrigger == Trigger)
			return;

		if (_bodyComponent == null)
			return;

		PhysicsBody body = _bodyComponent.Body;
		if (!body.isValid)
			return;

		NativeArray<PhysicsShape> shapes = body.GetShapes(Allocator.Temp);
		try
		{
			if (!_hasApplied && shapes.Length > 0)
				_solidContacts = shapes[0].contactFilter.contacts;

			for (int i = 0; i < shapes.Length; i++)
			{
				PhysicsShape shape = shapes[i];
				PhysicsShape.ContactFilter filter = shape.contactFilter;
				filter.contacts = Trigger ? PhysicsMask.None : _solidContacts;
				shape.contactFilter = filter;
			}
		}
		finally
		{
			shapes.Dispose();
		}

		_appliedTrigger = Trigger;
		_hasApplied = true;
	}

	void Update()
	{
		Control();
		ApplyTrigger();
	}
}
