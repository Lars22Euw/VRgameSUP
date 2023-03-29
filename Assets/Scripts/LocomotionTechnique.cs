using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.XR;
using PDollarGestureRecognizer;
using System.IO;
using System;
using TMPro;

public class LocomotionTechnique : MonoBehaviour
{
    // Please implement your locomotion technique in this script. 
    public GameObject hmd;
    //[SerializeField] private Vector3 startPos;

    /////////////////////////////////////////////////////////
    // These are for the game mechanism.
    public ParkourCounter parkourCounter;
    public string stage;
    public SelectionTaskMeasure selectionTaskMeasure;


    // my vars:
    Rigidbody rigidbody;
    [SerializeField] float rotationalDrag = 0.8f;
    [SerializeField] float forwardDrag = 0.99f;
    [SerializeField] float sidewaysDrag = 0.7f;
    [SerializeField] float rotationClamp = 1f;
    [SerializeField] float forwardClamp = 10f;
    public float leftForce = 0f;
    public float rightForce = 0f;
    public float dampen = 5000f;
    float rotationDampening = 0.02f;
    Vector3 lastPosLocal;
    public bool lastPosValid = false;
    public bool inTask = false;

    [Space]
    public GameObject leftPaddle;
    public GameObject rightPaddle;

    void Start()
    {
        rigidbody = this.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // revised movement concept based on kayaking implementation


        // update board velocity to simulate drag of water - ie slow down
        var localVelocity = transform.InverseTransformDirection(rigidbody.velocity);
        var newLocalVelocity = new Vector3(localVelocity.x * sidewaysDrag, 0, localVelocity.z * forwardDrag);
        rigidbody.velocity = transform.TransformDirection(newLocalVelocity);
        rigidbody.AddTorque(-rigidbody.angularVelocity * rotationalDrag);   // rotates board

        // new movement vector based on combined left and right momentum
        float forwardForceToApply = Mathf.Min((leftForce + rightForce) / 60, forwardClamp);
        float rotationForceToApply = ((leftForce - rightForce) / 60 * rotationDampening);

        // accelaration check - gaining speed
        if (forwardForceToApply > 0)
        {
            rigidbody.AddRelativeForce(new Vector3(0, 0, forwardForceToApply));
        }
        // break check - slowing down?
        else if (forwardForceToApply < 0)
        {
            rigidbody.AddRelativeForce(new Vector3(0, 0, forwardForceToApply/10));
        }
        rigidbody.AddRelativeTorque(new Vector3(0, rotationForceToApply, 0));

        leftForce = updateForce(leftForce);
        rightForce = updateForce(rightForce);
    }

    // reduces force with each update to approach 0 asymptotically
    private float updateForce(float force)
    {
        float updatedForce = force - force / 60;
        if (Mathf.Abs(updatedForce) < 0.5f)
        {
            updatedForce = 0f;
        }
        return updatedForce;
    }

    public float Row(Vector3 pos)
    {
        if (!lastPosValid)
        {
            lastPosLocal = gameObject.transform.InverseTransformPoint(pos);
            lastPosValid = true;
            return 0;
        }

        // calc change in forward position
        var posLocal = gameObject.transform.InverseTransformPoint(pos);
        var forward = -(posLocal.z - lastPosLocal.z);
        lastPosLocal = posLocal;
        return forward;
    }

    void resetMovement()
    {
        rightForce = 0f;
        leftForce = 0f;
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }

    void Update()
    {
        // switch paddle between rowing and interaction position
        if (OVRInput.GetUp(OVRInput.Button.One) || OVRInput.GetUp(OVRInput.Button.Three)) 
        {
            leftPaddle.SetActive(!leftPaddle.activeSelf);
            rightPaddle.SetActive(!rightPaddle.activeSelf);
            lastPosValid = false;
        }

        // reset momentum
        if (OVRInput.Get(OVRInput.Button.Two) || OVRInput.Get(OVRInput.Button.Four))
        {
            resetMovement();
        }
    }

    void OnTriggerEnter(Collider other)
    {

        // These are for the game mechanism.
        if (other.CompareTag("banner"))
        {
            stage = other.gameObject.name;
            parkourCounter.isStageChange = true;
        }
        else if (other.CompareTag("objectInteractionTask"))
        {
            if (inTask) return;

            inTask = true;
            resetMovement();
            // rotation: facing the user's entering direction
            float tempValueY = other.transform.position.y > 0 ? 12 : 0;
            Vector3 tmpTarget = new Vector3(hmd.transform.position.x, tempValueY, hmd.transform.position.z);
            selectionTaskMeasure.taskUI.transform.LookAt(tmpTarget);
            selectionTaskMeasure.taskUI.transform.Rotate(new Vector3(0, 180f, 0));
            selectionTaskMeasure.StartOneTask();
        }
        else if (other.CompareTag("coin"))
        {
            parkourCounter.coinCount += 1;
            this.GetComponent<AudioSource>().Play();
            other.gameObject.SetActive(false);
        }
        // These are for the game mechanism.
    }


    public void StartInteraction(GameObject target)
    {

    }

}