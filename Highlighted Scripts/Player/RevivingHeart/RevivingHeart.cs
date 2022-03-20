using Cinemachine;
using UnityEngine;

public class RevivingHeart : MonoBehaviour
{
    [SerializeField] float speed = 2f;
    [SerializeField] float whenStart = 2f;

    [SerializeField] GameObject reviveFX;
    [SerializeField] GameObject playerPrefab;

    public Vector3 TargetPosition { get; set; }

    void Awake()
    {
        Invoke("SetEnableToTrue", whenStart);
        enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position == TargetPosition)
            CreatePlayer();
        else
        {
            transform.position = Vector3.MoveTowards(transform.position
                , TargetPosition, Time.deltaTime * speed);

            speed += 1f;
        }
    }

    private void CreatePlayer()
    {
        GetComponent<CinemachineImpulseSource>().GenerateImpulse();

        Destroy(Instantiate(reviveFX, transform.position
            , Quaternion.identity, GameplayManager.DynamicContainerOfCurrentZone), 5f);

        Instantiate(playerPrefab, transform.position + Vector3.up * 2, Quaternion.identity
            , GameplayManager.CurrentZone.transform);

        Destroy(gameObject);
    }

    void SetEnableToTrue()
    {
        enabled = true;
    }
}
