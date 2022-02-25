using UnityEngine;

public partial class PlayerInput
{
    [System.Serializable]
    public class DoubleTapInput : InputData<KeyCode, bool>
    {
        // Allows to handle two keys assigned to one action
        [SerializeField] protected KeyCode additionalKey = KeyCode.None;
        [SerializeField] float activationDelta = .25f;

        float whenLastTap = 0f;

        public DoubleTapInput(KeyCode key, string forAction) : base(key, forAction) { }

        public override void Reset()
        {
            whenLastTap = 0f;
            Result = false;
        }

        public override void Update()
        {
            Result = false;

            if (Input.GetKeyDown(key) || (additionalKey != KeyCode.None && Input.GetKeyDown(additionalKey)))
            {
                if (Time.unscaledTime - whenLastTap < activationDelta)
                    Result = true;

                whenLastTap = Time.unscaledTime;
            }
        }
    }
}
