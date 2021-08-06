using System.Collections;
using System.Collections.Generic;
using Invector.vCharacterController; 
using UnityEngine;

public class GrabMechanic : MonoBehaviour
{
    #region variables
    private bool isGrabbing;
    private bool EnableGrab;

    private GameObject player;

    [SerializeField] private float m_MaxDistance = 10;
    private bool HitDetect;
    private bool m_HitDetect;

    public float BoxColliderSize = 1.5f;
    public float HeightOffset = 0;
    private Collider m_Collider;
    private RaycastHit m_Hit;

    [HideInInspector]
    public GameObject targetCube;

    private RigidbodyConstraints rbConstraints;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        m_Collider = GetComponent<Collider>();
        rbConstraints = GetComponent<Rigidbody>().constraints;
    }

    // Update is called once per frame
    void Update()
    {
        CubeDetection();
        InputHandler();
    }

    private void InputHandler()
    {
        if (Input.GetAxis("RT") == 1 && EnableGrab)
        {
            if (!isGrabbing)
            {
                Grab();
            }
        }
        if (Input.GetAxis("RT") == 0 || !EnableGrab)
        {
            if (isGrabbing)
            {
                Release();
            }
        }
    }

    private void Grab()
    {
        if (targetCube != null && 
            !targetCube.transform.parent.parent.GetComponent<BlockBehavior>().isDestroying)
        {
            GetComponent<MoveBoxController>().EnableBoxMovement();
            //get forward axis and set player position and rotation to directly face cube at set distance
            SetPlayerPositionAndRotation();
           

            
            //disable player rotation and camera rotation doesnt affect the player's rotation
            GetComponent<vThirdPersonInput>().cc.GetComponent<vThirdPersonMotor>().lockRotation = true;
            GetComponent<vThirdPersonInput>().cc.GetComponent<vThirdPersonMotor>().lockMovement = true;

            GetComponent<vThirdPersonInput>().SetDragMovement();
            //if camera isnt facing directly begind the player, set it 
            GetComponent<vThirdPersonInput>().SetCameraRotation();

            //print(targetCube.name);
            //print(targetCube.transform.parent.parent.name);
            targetCube.transform.parent.GetComponentInParent<BlockBehavior>().SetDragging();

            GetComponent<MoveBoxController>().SetPushPointPosition();
            GetComponent<MoveBoxController>().ParentToPushPoint();


            isGrabbing = true;
            GetComponent<vThirdPersonInput>().cc.Strafe();


            /*
            GetComponent<CapsuleCollider>().enabled = false;
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;

            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Rigidbody>().useGravity = false;
            */

        }
    }

    private void Release()
    {
        //player can ONLY release cube if it's reach 
        //a whole number (-1, 0, 1) on the  X/Z axis 
        if (targetCube != null && 
            !targetCube.transform.parent.parent.GetComponent<BlockBehavior>().isDestroying)
        {
            
            GetComponent<MoveBoxController>().DeParentToPushPoint();

            targetCube.transform.parent.GetComponentInParent<BlockBehavior>().RoundCubeLocation();
            targetCube.transform.parent.parent.eulerAngles = Vector3.zero;
            targetCube.transform.parent.GetComponentInParent<BlockBehavior>().SetGround();
            GetComponent<vThirdPersonInput>().SetDragMovement();

            GetComponent<vThirdPersonInput>().StopDragMovement(true);

            GetComponent<vThirdPersonInput>().cc.GetComponent<vThirdPersonMotor>().lockRotation = false;
            GetComponent<vThirdPersonInput>().cc.GetComponent<vThirdPersonMotor>().lockMovement = false;
            isGrabbing = false;

            /*
            GetComponent<CapsuleCollider>().enabled = true;
            GetComponent<Rigidbody>().constraints = rbConstraints;
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().useGravity = true;
            */


            GetComponent<vThirdPersonInput>().cc.Strafe();
            GetComponent<MoveBoxController>().EnableBoxMovement();
        }
    }

    private void CubeDetection()
    {
        m_HitDetect = Physics.BoxCast(m_Collider.bounds.center,
            transform.localScale * (BoxColliderSize * .15f),
            transform.forward, out m_Hit,
            transform.rotation, m_MaxDistance);

        if (m_HitDetect)
        {
            //Output the name of the Collider your Box hit
            //Debug.Log(gameObject.name+ " Hit : " + m_Hit.collider.name);
            if (m_Hit.collider.tag == "CubeHandle")
            {
                targetCube = m_Hit.collider.gameObject;
                //print("hittting block");
                //print("m_Hit.distance: " + m_Hit.distance);
                if (m_Hit.distance <= 0)
                {
                    print("making contact with cube");
                }



                //if (m_Hit.distance < .45f ||
                    //m_Hit.distance > .35f)
                if (m_Hit.distance < 1f ||
                    m_Hit.distance > .75f)
                {
                    if (!EnableGrab)
                    {
                        EnableGrab = true;
                    }
                }
                else
                {
                    if (isGrabbing)
                    {
                        Release();
                    }
                    else
                    {
                        EnableGrab = false;
                    }
                }
            }
        }
        else
        {
            if (isGrabbing)
            {
                Release();
            }
            targetCube = null;
        }
    }

    private void SetCameraBehindPlayer()
    {

    }

    private void SetPlayerPositionAndRotation()
    {
        Vector3 directionToTarget = transform.position - targetCube.transform.position;
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        float distance = directionToTarget.magnitude;
        //set new rotation to face cube
        //transform.LookAt(targetCube.transform.position, transform.up);
        
        
        //print("distance: "+distance);
        //set player position further from cube if too close.
        if (/*Mathf.Abs(angle) < 90*/  distance < 1.5f) {
            transform.position = transform.position + (transform.forward * -.4f);
            //Debug.Log("target is in front of me");
        }
    }

    private Vector3 GetPosition()
    {
        Vector3 pos = new Vector3(transform.position.x, 
            transform.position.y + HeightOffset, 
            transform.position.z);
        return pos; 
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 tmpPos = GetPosition();
        //Check if there has been a hit yet
        if (m_HitDetect)
        {
            //Draw a Ray forward from GameObject toward the hit
            Gizmos.DrawRay(tmpPos, (transform.forward) * m_Hit.distance);
            //Draw a cube that extends to where the hit exists
            Gizmos.DrawWireCube(tmpPos + (transform.forward) * m_Hit.distance, transform.localScale * BoxColliderSize);
        }
        //If there hasn't been a hit yet, draw the ray at the maximum distance
        else
        {
            //Draw a Ray forward from GameObject toward the maximum distance
            Gizmos.DrawRay(tmpPos, (transform.forward) * m_MaxDistance);
            //Draw a cube at the maximum distance
            Gizmos.DrawWireCube(tmpPos + (transform.forward) * m_MaxDistance, transform.localScale * BoxColliderSize);
        }
    }
}
