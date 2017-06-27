using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CityCamera : MonoBehaviour {

    public Transform mapPivotReference;
    public Transform manualCamViewSpot;
    public NavMeshAgent agent;

    public bool cancelDelayedUnfocus = true;
    public bool randomFreeCamPosition = true;
    public float baseHeight = 2;
    public float objectOfInteresetCheckDistance = 20;
    public float rotationSpeed = 1;
    public float camInputTime;
    public Vector2 rotationOffsetConstraints = new Vector2(30, 30);

    [HideInInspector, System.NonSerialized]
    public Vector3 targetRotationOffset = Vector3.zero;

    public GameObject FocusedGameObject {
        get {
            return focusedGameObject;
        }
    }

    protected GameObject focusedGameObject;
    protected Transform focusTargetTransform;
    protected Renderer focusTarget;
    protected Vector3 rotationOffset = Vector3.zero;
    protected Collider[] paintableObjectsColliders;
    protected Collider objectOfInterest;
    protected float manualY;
    protected float focusDistance = 5;
    protected bool isManualMode = false;
    protected bool isometricFocus = true;

    protected const float manualYMaxValue = 10f;
    protected const float manualYMinValue = 0f;
    protected const float agentAutoSpeed = 3f;
    protected const float agentManualSpeed = 6f;
    protected const float agentFocusedRoamSpeed = 10f;

    protected float ManualY {
        get {
            return manualY;
        }
        set {
            manualY = Mathf.Clamp(value, manualYMinValue, manualYMaxValue);
        }
    }

    protected Transform pivot {
        get { return transform.parent; }
    }

    [ContextMenu("Warp to random place")]
    public void WarpCameraToRandomPlace() {
        agent.Warp(RandomPointOnMap());
        UpdateNavMeshAgentAfterWarp();
    }

    public void WarpCameraToPosition(Vector3 position) {
        agent.Warp(GetClosestPointOnMap(position, 10000));
        UpdateNavMeshAgentAfterWarp();
    }

    public void SetObjectOfInterest(Collider objectOfInterest) {
        if (objectOfInterest != null) {
            randomFreeCamPosition = false;
            this.objectOfInterest = objectOfInterest;
            agent.speed = agentFocusedRoamSpeed;
            agent.stoppingDistance = 20;
        }
        else {
            agent.speed = agentAutoSpeed;
            randomFreeCamPosition = true;
            agent.stoppingDistance = 0;
        }
    }

    public void Pan(Vector3 delta) {
        TargetRotationOffset += delta;
    }

    public void Zoom(float delta) {
        ManualY += Mathf.Lerp(0, delta, Time.deltaTime);
    }

    public void MoveCameraToClosestPlace(Vector3 position) {
        try {
            SetObjectOfInterest(null);
            agent.SetDestination(GetClosestPointOnMap(position, mapPivotReference.localScale.magnitude * 2));
            agent.Resume();
        }
        catch { }
    }

    public void DelayedUnfocus(float delay) {
        if (IsInvoking("DelayedUnfocus")) CancelInvoke("DelayedUnfocus");
        Invoke("DelayedUnfocus", delay);
    }

    protected void DelayedUnfocus() {
        if (cancelDelayedUnfocus) return;
        SetFocusTarget(null);
        Debug.Log("delayed unfocus");
    }

    /// <summary>
    /// To unset target: SetFocusTarget(null)
    /// </summary>
    /// <param name="t">target</param>
    public void SetFocusTarget(GameObject t, bool isometricFocus = true) {
        if (t == null) {
            focusTarget = null;
            focusTargetTransform = null;
            focusedGameObject = null;
        }
        else {
            focusedGameObject = t;
            MeshRenderer[] renderes = t.GetComponentsInChildren<MeshRenderer>();
            if (renderes != null && renderes.Length > 0) {
                this.isometricFocus = isometricFocus;
                focusTargetTransform = t.transform;
                focusTarget = renderes.OrderByDescending(_ => _.bounds.size.sqrMagnitude).First();
                focusDistance = Mathf.Clamp(focusTarget.bounds.size.magnitude * 1.2f, 3, 999);
                //objectOfInterest = null;
                pivot.transform.position = pos = Quaternion.AngleAxis((isometricFocus ? -35 : 0), focusTargetTransform.up) * Quaternion.AngleAxis((isometricFocus ? -45 : 0), focusTargetTransform.right) * focusTargetTransform.forward * focusDistance + focusTarget.bounds.center;
                pivot.LookAt(focusTarget.bounds.center);
            }
            else {
                Debug.LogError("Target doesn't have any mesh renderer.", t);
            }
        }
        TargetRotationOffset = new Vector3(0, 0, 0);
    }

    public Vector3 TargetRotationOffset {
        get {
            return targetRotationOffset;
        }
        set {
            targetRotationOffset.x = Mathf.Clamp(value.x, -rotationOffsetConstraints.x, rotationOffsetConstraints.x);
            if (IsManualMode) {
                targetRotationOffset.y = value.y;
            }
            else {
                targetRotationOffset.y = Mathf.Clamp(value.y, -rotationOffsetConstraints.y, rotationOffsetConstraints.y);
            }
            time = 0;
        }
    }

    public bool IsManualMode {
        get {
            return isManualMode;
        }
        set {
            if (value == isManualMode) return;
            if (value == true && isManualMode == false) {
                agent.speed = agentManualSpeed;
                agent.Stop();
                camInputTime = Time.time;
                if (FocusedGameObject != null) SetFocusTarget(null);
            }
            else if (isManualMode == true) {
                agent.speed = agentAutoSpeed;
                agent.Resume();
            }
            isManualMode = value;
        }
    }

    protected void LateUpdate() {
        pos = pivot.position;

        if (focusTarget) {
            FocusedCamera();
        }
        else {
            FreeRoamCamera(randomFreeCamPosition);
        }

        pivot.position = pos;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(rotationOffset), Time.deltaTime * 8);
        if (focusTarget == null) {
            rotationOffset.x = Mathf.LerpAngle(rotationOffset.x, TargetRotationOffset.x, Time.deltaTime * 4);
        }
        else {
            rotationOffset.x = Mathf.LerpAngle(rotationOffset.x, 0, Time.deltaTime * 4);
        }
        time += Time.deltaTime;
        TargetRotationOffset = Vector3.Lerp(TargetRotationOffset, Vector3.zero, time / 10);
    }

    [ContextMenu("Toggle manual mode")]
    protected void ToggleManualMode() {
        IsManualMode = !IsManualMode;
    }

    protected void UpdateNavMeshAgentAfterWarp() {
        targetRot = pivot.rotation = Quaternion.Slerp(pivot.rotation, agent.transform.rotation, Time.deltaTime);
        destinationPos = RandomPointOnMap();
        agent.SetDestination(destinationPos);
    }

    [ContextMenu("Focus test")]
    protected void FocusTest() {
        GameObject[] go = FindObjectsOfType<GameObject>().Where(_ => _.layer == LayerMask.NameToLayer("PaintableObjects") && (_.transform.parent ? _.transform.parent.gameObject.layer != _.layer : true)).ToArray();
        if (go.Length > 0) SetFocusTarget(go[Random.Range(0, go.Length - 1)]);
    }

    [ContextMenu("Unser focus")]
    protected void UnsetFocus() {
        SetFocusTarget(null);
    }

    protected Vector3 GetClosestPointOnMap(Vector3 position, float maxDistance) {
        NavMeshHit navMeshHit = new NavMeshHit();
        NavMesh.SamplePosition(position, out navMeshHit, maxDistance, NavMesh.AllAreas);
        return navMeshHit.position;
    }

    protected Vector3 RandomPointOnMap() {
        bool foundPos = false;
        NavMeshHit navMeshHit = new NavMeshHit();
        int reTryCount = 0;
        while (!foundPos && reTryCount < 10) {
            Vector2 randomCirclePos = Random.insideUnitCircle;
            Vector3 targetPos = new Vector3(randomCirclePos.x * mapPivotReference.localScale.x * 0.5f, 0, randomCirclePos.y * mapPivotReference.localScale.z * 0.5f) + mapPivotReference.position;
            foundPos = NavMesh.SamplePosition(targetPos, out navMeshHit, mapPivotReference.localScale.magnitude * 2, 2 << 2);
            reTryCount++;
        }
        return foundPos ? navMeshHit.position : Vector3.zero;
    }

    protected Vector3 destinationPos;
    protected Vector3 pos;
    protected float targetY = 2;
    protected Quaternion targetRot;
    protected float verticalCollisionOffset;
    protected float heightVel;
    protected float time;

    protected void SetAgentPosition(Vector3 pos) {
        destinationPos = pos;
        agent.SetDestination(destinationPos);
    }

    protected void RandomTargetPos() {
        destinationPos = RandomPointOnMap();
        agent.SetDestination(destinationPos);
    }

    protected float GetAverageFloat(float oldValue, float newValue, int wage) {
        oldValue = (oldValue * wage + newValue) / (wage + 1);
        return oldValue;
    }

    protected void FocusedCamera() {
        CalculateCollisionOffset();
        pos = Vector3.Slerp(pos, Quaternion.AngleAxis((isometricFocus ? -35 - TargetRotationOffset.y * 2 : 0), focusTargetTransform.up) * Quaternion.AngleAxis((isometricFocus ? -45 + TargetRotationOffset.x / 2 : 0), focusTargetTransform.right) * focusTargetTransform.forward * focusDistance + focusTarget.bounds.center, Time.deltaTime * 3);
        pivot.rotation = Quaternion.Slerp(pivot.rotation, Quaternion.LookRotation(focusTarget.bounds.center - pos), Time.deltaTime * 2);
    }

    protected void FreeRoamCamera(bool random = true) {
        if (IsManualMode == false) {
            ManualY = Mathf.Lerp(ManualY, 0, Time.deltaTime);

            if (random) {
                if (Vector3.Distance(agent.transform.position, destinationPos) < 5)
                    RandomTargetPos();
            }
            else {
                if (objectOfInterest != null) SetAgentPosition(objectOfInterest.transform.position);
            }

            //find new object of interest
            if (objectOfInterest == null) {
                paintableObjectsColliders = Physics.OverlapSphere(pivot.position, objectOfInteresetCheckDistance, 1 << 10); //layer 10 - paintableObjects
                if (paintableObjectsColliders.Length > 0) {
                    objectOfInterest = paintableObjectsColliders.OrderBy(_ => (pivot.position - _.transform.position).sqrMagnitude).First();
                }
            }
            //object of interest is no longer near camera
            if (random && objectOfInterest && Vector3.Distance(pivot.position, objectOfInterest.transform.position) > objectOfInteresetCheckDistance)
                objectOfInterest = null;

            CalculateCollisionOffset();
            targetY = Mathf.SmoothDamp(targetY, objectOfInterest ? objectOfInterest.bounds.center.y : baseHeight, ref heightVel, 3);
            pos = Vector3.Slerp(pos, agent.transform.position, Time.deltaTime);
            pos.y = Mathf.Clamp(targetY + verticalCollisionOffset, baseHeight, 9999);
            pos.y += ManualY;
            targetRot = Quaternion.Slerp(targetRot, (objectOfInterest ? Quaternion.LookRotation(objectOfInterest.bounds.center - pivot.position, Vector3.up) :
                Quaternion.AngleAxis((ManualY / manualYMaxValue) * 90, agent.transform.right) * agent.transform.rotation), objectOfInterest ? Time.deltaTime * 50 : Time.deltaTime * 0.5f);
            pivot.rotation = Quaternion.Slerp(pivot.rotation, targetRot, Time.deltaTime / rotationSpeed);
        }
        else {
            if (manualCamViewSpot == null) return;
            pos = Vector3.Slerp(pos, manualCamViewSpot.position, Time.deltaTime / 5);
            pivot.rotation = Quaternion.Slerp(pivot.rotation, Quaternion.AngleAxis(TargetRotationOffset.y, Vector3.up) * Quaternion.LookRotation(Vector3.down), Time.deltaTime / rotationSpeed);
        }
    }

    protected float collisionDecay = 0;
    protected void CalculateCollisionOffset() {
        bool isFocusingCar = (focusTarget != null && focusTarget.tag == "TrafficCar");
        if (!isFocusingCar && Physics.OverlapSphere(pivot.position, focusTarget ? 3 : 10, 1 << 10).Where(_ => _.tag == "TrafficCar").Any()) {
            collisionDecay = 1;
        }

        if (collisionDecay > 0.1f) {
            verticalCollisionOffset = Mathf.Lerp(verticalCollisionOffset, Mathf.Clamp(10 - (objectOfInterest ? objectOfInterest.bounds.center.y : baseHeight), 0, 9999), Time.deltaTime * 2);
        }
        else {
            verticalCollisionOffset = Mathf.Lerp(verticalCollisionOffset, 0, Time.deltaTime * 0.2f);
        }
        collisionDecay = Mathf.Lerp(collisionDecay, 0, Time.deltaTime * 0.5f);
    }
}
