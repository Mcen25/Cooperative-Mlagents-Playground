using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AgentManager : Agent
{

    [Header("TEAM")]
    public int teamID;

    [SerializeField] private GameObject targetTransform;

    [Header("Child Objects")]
    [SerializeField] private GameObject raycastVisionObject;

    [Header("HEALTH")] public int AgentHealth;

    private Vector3 initialPosition;

    public override void Initialize()
    {
        initialPosition = transform.localPosition;
    }
    public override void OnEpisodeBegin()
    {
        transform.localPosition = initialPosition;
        targetTransform.transform.localPosition = new Vector3(Random.Range(-4f, 4f), 1.4f, Random.Range(-4f, 4f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float sensorTilt = actions.ContinuousActions[2];
        float bodyRotation = actions.ContinuousActions[3];

        float moveSpeed = 5f;
        transform.localPosition += new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime;

        float rotationSpeed = 100f;
        transform.Rotate(0f, bodyRotation * rotationSpeed * Time.deltaTime, 0f);
        
        if (raycastVisionObject != null)
        {
            float tiltSpeed = 50f;
            // Get the current tilt angle and convert to -180..180 range
            float currentTilt = raycastVisionObject.transform.localEulerAngles.x;
            if (currentTilt > 180f) currentTilt -= 360f;
            // Update tilt and clamp to -20f and 20f
            float newTilt = Mathf.Clamp(currentTilt + actions.ContinuousActions[2] * tiltSpeed * Time.deltaTime, -20f, 20f);
            var currentEuler = raycastVisionObject.transform.localEulerAngles;
            currentEuler.x = newTilt;
            raycastVisionObject.transform.localEulerAngles = currentEuler;
        }
        base.OnActionReceived(actions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        // Initialize all actions to zero
        for (int i = 0; i < continuousActionsOut.Length; i++)
            continuousActionsOut[i] = 0f;
        
        if (continuousActionsOut.Length > 0)
            continuousActionsOut[0] = Input.GetAxis("Horizontal");
        if (continuousActionsOut.Length > 1)
            continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Goal>(out Goal goal))
        {
            SetReward(+1f);
            EndEpisode();
        }

        if (other.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(-1f);
            EndEpisode();
        }
    }
}
