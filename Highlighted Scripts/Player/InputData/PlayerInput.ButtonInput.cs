using UnityEngine;

public partial class PlayerInput
{
    [System.Serializable]
    public class ButtonInput : InputData<KeyCode, bool>
    {
        // Allows to handle two keys assigned to one action, for example jump
        [SerializeField] protected KeyCode additionalKey = KeyCode.None;

        public bool Released => Input.GetKeyUp(key);

        public ButtonInput(KeyCode key, string forAction) : base(key, forAction) { }

        public override void Reset()
        {
            Result = false;
        }

        public override void Update()
        {
            Result = Input.GetKeyDown(key);

            if (!Result)
                if (additionalKey != KeyCode.None)
                    Result = Input.GetKeyDown(additionalKey);
        }
    }
}
