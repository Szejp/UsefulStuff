using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class NavMeshMovement : Movement {

    [ContextMenu("TeleportTest")]
    public void TeleporTest() {
        Teleport(Vector3.zero);
    }

    [SerializeField]
    private NavMeshAgent _agent;
    [SerializeField]
    private float _speed;
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

    // Update is called once per frame
    private void Update() {
        if (Input.GetKey(KeyCode.W))
            MoveForwards();
        if (Input.GetKey(KeyCode.S))
            MoveBackwards();
        if (Input.GetKey(KeyCode.D))
            MoveRight();
        if (Input.GetKey(KeyCode.A))
            MoveLeft();

        _agent.SetDestination(_agentDestination);
        _agentDestination = _agent.transform.position;
        transform.position = _agent.transform.position;
    }

    private void OnDrawGizmos() {
        Gizmos.DrawSphere(_agent.destination, 0.2f);
    }
}
