using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshMovement : Movement {

    public NavMeshAgent agent {
        get {
            return _agent;
        }
    }

    private NavMeshAgent _agent;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private Transform _mapPivotReference;
    private Vector3 _agentDestination;

    public override void MoveLeft() {
        _agentDestination += Vector3.left * _speed;
    }

    public override void MoveRight() {
        _agentDestination += Vector3.right * _speed;
    }

    public override void MoveForwards() {
        _agentDestination += Vector3.forward * _speed;
    }

    public override void MoveBackwards() {
        _agentDestination -= Vector3.forward * _speed;
    }

    public override void Teleport(Vector3 position) {
        _agent.transform.position = position;
    }

    public override void MoveUp() { }
    public override void MoveDown() { }

    [ContextMenu("TeleportTest")]
    public void TeleporTest() {
        Teleport(Vector3.zero);
    }

    public void MoveToDestination(Vector3 destination) {
        _agentDestination = destination;
    }

    public void RandomTargetPos() {
        Vector3 targetPos = RandomPointOnMap(3 << 4);
        _agent.SetDestination(targetPos);
    }

    private Vector3 GetClosestPointOnMap(Vector3 position, float maxDistance) {
        NavMeshHit navMeshHit = new NavMeshHit();
        NavMesh.SamplePosition(position, out navMeshHit, maxDistance, NavMesh.AllAreas);
        return navMeshHit.position;
    }

    private Vector3 RandomPointOnMap(int areaMask) {
        bool foundPos = false;
        NavMeshHit navMeshHit = new NavMeshHit();
        int reTryCount = 0;
        while (!foundPos && reTryCount < 10) {
            Vector2 randomCirclePos = Random.insideUnitCircle;
            Vector3 targetPos = new Vector3(randomCirclePos.x * _mapPivotReference.localScale.x * 0.5f, 0, randomCirclePos.y * _mapPivotReference.localScale.z * 0.5f) + _mapPivotReference.position;
            foundPos = NavMesh.SamplePosition(targetPos, out navMeshHit, _mapPivotReference.localScale.magnitude * 2, areaMask);
            reTryCount++;
        }

        if (foundPos) {
            return navMeshHit.position;
        }
        else {
            Debug.Log("[NavMeshMovement] Could not find random point on map");
            return Vector3.zero;
        }
    }

    private void Awake() {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update() {
        _agent?.SetDestination(_agentDestination);
        transform.position = _agent.transform.position;
        transform.rotation = _agent.transform.rotation;
    }

    private void OnDrawGizmos() {
        Gizmos.DrawSphere(transform.position + transform.forward, 0.2f);
    }
}
