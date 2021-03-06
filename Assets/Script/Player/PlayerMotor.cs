using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    CharacterController controller;
    float baseSpeed = 10.0f;
    float rotSpeedX = 6.0f;
    float rotSpeedY = 3f;
    Vector3 moveVector;
    [SerializeField] GameObject brokenPlane;
    GameScene gameScene;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        // set trail belong to plane
        GameObject trail =
            Instantiate(GameManager.Instance.playerTrails[SaveManager.Instance.state.activeTrail]) as GameObject;
        trail.transform.SetParent(transform);
        trail.transform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0));
        brokenPlane.SetActive(false);
    }

    void Start()
    {
        gameScene = GameObject.FindObjectOfType<GameScene>();
    }

    void Update()
    {
        if (GameManager.Instance.finishedLevel)
        {
            return;
        }
        moveVector = transform.forward * baseSpeed;
        Vector3 inputs = GameManager.Instance.GetPlayerInput();

        // Get the delta direction
        Vector3 yaw = inputs.x * transform.right * rotSpeedX * Time.deltaTime;
        Vector3 pitch = inputs.y * transform.up * rotSpeedY * Time.deltaTime;
        Vector3 dir = yaw + pitch;

        // Make sure we limit the player from doing a loop
        float maxX = Quaternion.LookRotation(moveVector + dir).eulerAngles.x;
        maxX -= 10f;
//        Debug.Log("Max X: " + maxX);
        // If hes not going too far up/down, add the direction to the moveVector
        if (maxX < 90 && maxX > 60 || maxX > 270 && maxX < 290) // if maxX = 60 -> loop
        {
            // Too far!, don't do anything
        }
        else
        {
            moveVector += dir;
            transform.rotation = Quaternion.LookRotation(moveVector);
        }
        controller.Move(moveVector * Time.deltaTime);
    }

    // call when character controller hit a collider while performing a Move
    void OnControllerColliderHit(ControllerColliderHit target)
    {
        if (target.gameObject.CompareTag("Ring"))
        {
            GameManager.Instance.finishedLevel = true;
            BreakThePlane();
            target.gameObject.GetComponent<MeshCollider>().enabled = false; // disable collider of ring

            // wait 1 second, show Game Over Panel
            gameScene.GameOver();
        }
        if (target.gameObject.CompareTag("Token"))
        {
            Destroy(target.gameObject);
            SaveManager.Instance.state.gold++;
            // update gold value in GameScene
            gameScene.UpdateGold();
        }
        if (target.gameObject.CompareTag("Obstacle"))
        {
            GameManager.Instance.finishedLevel = true;
            BreakThePlane();
            target.gameObject.GetComponent<MeshCollider>().enabled = false;
            gameScene.GameOver();
        }
        if (target.gameObject.CompareTag("DeadLine"))
        {
            GameManager.Instance.finishedLevel = true;
            gameObject.SetActive(false);
            gameScene.GameOver();
        }
    }

    void BreakThePlane()
    {
        // active broken model, disactive plane model
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        brokenPlane.SetActive(true);

        // unparent the plane's parts
        brokenPlane.transform.DetachChildren();
    }


}
