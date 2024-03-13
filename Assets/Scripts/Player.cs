using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour {

    public static Player Instance { get; private set; }

    public EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs {
        public ClearCounter selectedCounter;
    }

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMask;

    private float rotateSpeed = 14f;
    private float playerHeight = 2f;
    private float playerRadius = .7f;
    private float interactDistance = 2f;
    private bool isWalking;
    private bool canMove;
    private Vector3 lastInteractDir;
    private ClearCounter selectedCounter;

    public bool CanMove => canMove;
    public bool IsWalking => isWalking;

    private void Awake() {
        if(Instance != null) {
            Debug.LogError("There's more than one player");
        }
        Instance = this;
    }

    private void Start() {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
    }

    private void GameInput_OnInteractAction(object sender,System.EventArgs e) {
        Debug.Log(selectedCounter);
        if (selectedCounter != null) {
            selectedCounter.Interact();
        }
    }

    private void Update() {
        HandleMovement();
        HandleInteractions();
    }

    private void HandleInteractions() {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x,0f,inputVector.y);
        if (moveDir != Vector3.zero) {
            lastInteractDir = moveDir;
        }
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycasthit, interactDistance, countersLayerMask)) {
            if(raycasthit.transform.TryGetComponent(out ClearCounter clearCounter)) {
                if (clearCounter != selectedCounter) {
                    SetSelectedCounter(clearCounter);
                } 
            } else {
                SetSelectedCounter(null);
            }
        } else {
            SetSelectedCounter(null);
        }
    }

    private void HandleMovement() {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x,0f,inputVector.y);
        float moveDistance = moveSpeed * Time.deltaTime;

        canMove = CanMoveToDirection(moveDir,moveDistance);
        if (!canMove) {
            // Cannot move towards moveDir
            // Attempt only x movement
            Vector3 moveDirX = new Vector3(moveDir.x,0,0).normalized;
            canMove = CanMoveToDirection(moveDirX,moveDistance);

            if (canMove) {
                // Can only move on the x
                moveDir = moveDirX;
            } else {
                // Cannot move on the x
                // Attempt only z movement
                Vector3 moveDirZ = new Vector3(0,0,moveDir.z).normalized;
                canMove = CanMoveToDirection(moveDirZ,moveDistance);

                if (canMove) {
                    // Can only move on the z
                    moveDir = moveDirZ;
                }
            }
        }

        if (canMove) {
            transform.position += moveDir * moveDistance;
        }

        isWalking = (moveDir != Vector3.zero);
        transform.forward = Vector3.Slerp(transform.forward,moveDir,Time.deltaTime * rotateSpeed);
    }

    private bool CanMoveToDirection(Vector3 moveDirection, float moveDistance) {
        return !Physics.CapsuleCast(transform.position,transform.position + Vector3.up * playerHeight,playerRadius,moveDirection,moveDistance);
    }

    private void SetSelectedCounter(ClearCounter selectedCounter) {
        this.selectedCounter = selectedCounter;
        OnSelectedCounterChanged?.Invoke(this,new OnSelectedCounterChangedEventArgs {
            selectedCounter = selectedCounter
        });
    }
}