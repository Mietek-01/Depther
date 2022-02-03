using Cinemachine;
using UnityEngine;

public class RevivingHeart : MonoBehaviour
{
    [SerializeField] float speed = 2f;
    [SerializeField] float whenStart = 2f;

    [SerializeField] GameObject reviveFX;
    [SerializeField] GameObject playerPrefab;

    public Vector3 TargetPosition { get; set; }

    float startTime;

    // Start is called before the first frame update
    void Awake()
    {
        startTime = Time.time + whenStart;
    }

    // Update is called once per frame
    void Update()
    {
        if (startTime < Time.time)
        {
            // I must save the bloom intensity to set it after a reset as it reverts to the default value. I dont know why
            UnityEngine.Rendering.Universal.Bloom bloom;
            float savedBloomIntensity = 0f;

            if (GameplayManager.GlobalVolume.profile.TryGet<UnityEngine.Rendering.Universal.Bloom>(out bloom))
                savedBloomIntensity = (float)bloom.intensity;
            else
                Debug.LogError("I didnt find the bloom");

            transform.position = Vector3.MoveTowards(transform.position
                , TargetPosition, Time.deltaTime * speed);

            if (transform.position == TargetPosition)
            {
                GetComponent<CinemachineImpulseSource>().GenerateImpulse();

                Destroy(Instantiate(reviveFX, transform.position
                    , Quaternion.identity, GameplayManager._DynamicOfCurrentZone), 5f);

                Instantiate(playerPrefab, transform.position + Vector3.up * 2, Quaternion.identity
                    , GameplayManager.CurrentZone.transform);

                Destroy(gameObject);
            }
            else
                speed += 1f;
        }
    }

}
