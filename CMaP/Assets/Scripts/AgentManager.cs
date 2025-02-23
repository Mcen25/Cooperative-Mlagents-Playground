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
        targetTransform.transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0, Random.Range(-4f, 4f));
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

        float moveSpeed = 1f;
        transform.localPosition += new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime;

        float rotationSpeed = 100f;
        transform.Rotate(0f, bodyRotation * rotationSpeed * Time.deltaTime, 0f);
        
        if (raycastVisionObject != null)
        {
            float tiltSpeed = 50f;
            raycastVisionObject.transform.Rotate(sensorTilt * tiltSpeed * Time.deltaTime, 0f, 0f);
        }
        base.OnActionReceived(actions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Goal>(out Goal goal))
        {
            AddReward(+1f);
            EndEpisode();
        }

        if (other.TryGetComponent<Wall>(out Wall wall))
        {
            AddReward(-1f);
            EndEpisode();
        }
    }
}
