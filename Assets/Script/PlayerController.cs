using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{   
    // 스피드 조정 변수    
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    private float applySpeed;
    [SerializeField]
    private float crouchSpeed;    

    [SerializeField]
    private float jumpForce;

    // 해당 캐릭터에 대한 상태 변수
    private bool isRun = false;
    private bool isGround = true;
    private bool isCrouch = false;

    // 앉았을 때 얼마나 앉을지 결정하는 변수
    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;

    private CapsuleCollider capsuleCollider;

    // 필요한 컴포넌트
    [SerializeField]
    private Camera theCamera;
     private Rigidbody myRigid;    

    [SerializeField]
    private float loockSensitivity;

    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotationX = 0;

    // Start is called before the first frame update
    void Start() {
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        applySpeed = walkSpeed;
        originPosY = theCamera.transform.localPosition.y;
        applyCrouchPosY = originPosY;
    }

    // Update is called once per frame
    void Update() {
        IsGround();
        TryJump();
        TryRun();
        TryCrouch();
        Move();
        CameraRotation();
        CharacterRotation();
    }

    // [ 함수 ] 캐릭터 앉기 함수 
    private void TryCrouch() {
        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            Crouch();
        } 
    }

    private void Crouch() {
        isCrouch = !isCrouch;

        if (isCrouch) {
            applySpeed = crouchSpeed;   
            applyCrouchPosY = crouchPosY;
        } else {
            applySpeed = walkSpeed; 
            applyCrouchPosY = originPosY;
        }
        
        StartCoroutine(CrouchCoroutine());
    }

    IEnumerator CrouchCoroutine() {
        float posY = theCamera.transform.localPosition.y;
        int count = 0;

        while (posY != applyCrouchPosY) {
            count++;
            posY = Mathf.Lerp(posY, applyCrouchPosY, 0.3f);    
            theCamera.transform.localPosition = new Vector3(0, posY, 0);
            
            if (count > 15) {
                break;                
            }

            yield return null;
        }

        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0f);
     }

    private void IsGround() {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
    }

    // [ 함수 ] 해당 캐릭터의 점프에 대한 함수 
    private void TryJump() {
        if((Input.GetKeyDown(KeyCode.Space) && isGround)) {
            Jump();
        }
    }

    private void Jump() {
        // 앉은 상태에서 점프 시, 앉은 상태 해제 
        if (isCrouch) {
            Crouch();
        } else {
            myRigid.velocity = transform.up * jumpForce;
        }
    }

    // [ 함수 ] 해당 캐릭터가 달리고 있는지 확인하는 함수
    private void TryRun() {
        if(Input.GetKey(KeyCode.LeftShift)) {
            Running();
        } else if(Input.GetKeyUp(KeyCode.LeftShift)) {
            RunningCancel();
        }
    }

    private void Running() {
        if (isCrouch) {
            Crouch();
        }

        isRun = true;
        applySpeed = runSpeed;
    }

    private void RunningCancel() {
        isRun = false;
        applySpeed = walkSpeed;
    }

    // [ 힘스 ] 캐릭터 이동 함수 
    private void Move() {
        float moveDirX = Input.GetAxisRaw("Horizontal");
        float moveDirZ = Input.GetAxisRaw("Vertical");
        
        // ( -1, 0, 0) * -1씩에 대한 계산법
        // 삼각 함수를 생각하면 됨
        Vector3 moveHorizontal = (transform.right * moveDirX);
        Vector3 moveVertical = (transform.forward * moveDirZ);

        Vector3 velocity = ((moveHorizontal + moveVertical).normalized * applySpeed);

        myRigid.MovePosition(transform.position + velocity * Time.deltaTime);
    } 

    // [ 함수 ] 캐릭터 좌우 회전 관련 함수
    private void CharacterRotation() {
        float yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 characterRotationY = new Vector3(0f, yRotation, 0f) * loockSensitivity;

        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(characterRotationY));
    }

    // [ 함수 ] 캐릭터 카메라 관련 함수
    private void CameraRotation() {
        float xRotation = Input.GetAxisRaw("Mouse Y");
        float cameraRotationX = (xRotation * loockSensitivity);

        currentCameraRotationX -= cameraRotationX;
        // 현재 설정한 cameraRotationLimit값에 따른 -값과 +값의 그 중간 값에 대해 값을 지정해준다.
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }
}