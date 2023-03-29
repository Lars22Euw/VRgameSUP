using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RowDetection : MonoBehaviour
{

    [SerializeField] GameObject Player;
    Rigidbody rigidbody;
    [SerializeField] ParkourCounter parkourCounter;
    public LocomotionTechnique locomotion;
    public bool isRowing = false;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = Player.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("coin"))
        {
            parkourCounter.coinCount += 1;
            Player.GetComponent<AudioSource>().Play();
            other.gameObject.SetActive(false);
        }

        // Trigger zones of board are tagged as paddle
        if (other.tag == "Paddle")
        {
            isRowing = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Paddle")
        {
            locomotion.lastPosValid = false;
            isRowing = false;

        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name == "left")
        {
            locomotion.leftForce += locomotion.Row(transform.position) * locomotion.dampen;
        }
        if (other.gameObject.name == "right")
        {
            locomotion.rightForce += locomotion.Row(transform.position) * locomotion.dampen;
        }
    }

}
