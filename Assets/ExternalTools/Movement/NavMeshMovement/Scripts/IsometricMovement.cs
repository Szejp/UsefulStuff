using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IsometricMovement : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent _agent;
    [SerializeField]
    private float _speed;


    private Vector3 _agentDestination;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _agentDestination = _agent.transform.position;

        if (Input.GetKey(KeyCode.W))
            _agentDestination += Vector3.forward * _speed;
        if (Input.GetKey(KeyCode.S))
            _agentDestination -= Vector3.forward * _speed;
        if (Input.GetKey(KeyCode.D))
            _agentDestination += Vector3.right * _speed;
        if (Input.GetKey(KeyCode.A))
            _agentDestination += Vector3.left * _speed;

        _agent.SetDestination(_agentDestination);
        transform.position = _agent.transform.position;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(_agent.destination, 0.2f);
    }
}
