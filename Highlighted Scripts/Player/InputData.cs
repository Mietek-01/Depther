using UnityEngine;

public partial class PlayerInput
{
    public abstract class InputData
    {
        [SerializeField] protected bool enabled = true;

        // Allows to assign Input to an action e.g. a jump 
        public string ForAction { get; private set; }

        public InputData(string forAction)
        {
            ForAction = forAction;
        }

        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;

                if (enabled == false)
                    Reset();
            }
        }

        protected abstract void Reset();

        public abstract void Update();
    }

    // I must inherit to use polymorphism
    public abstract class InputData<T, T1> : InputData
    {
        // Determines key for selected type of input
        // e.g. KeyCode for ButtonInput,string for AxisInput, int for MouseInput
        [SerializeField] protected T key;

        // The result of input e.g. for ButtonInput or MouseInput it is a bool 
        // but for AxisInput it is a float
        public T1 Result { get; protected set; }

        public InputData(T key, string forAction) : base(forAction)
        {
            this.key = key;
        }
    }

    [System.Serializable]
    public class ButtonInput : InputData<KeyCode, bool>
    {
        // Allows to handle two keys assigned to one action, for example jump
        [SerializeField] protected KeyCode additionalKey = KeyCode.None;

        public bool Released => Input.GetKeyUp(key);

        public ButtonInput(KeyCode key, string forAction) : base(key, forAction)
        {
        }

        protected override void Reset()
        {
            Result = false;
        }

        public override void Update()
        {
            if (enabled)
            {
                Result = Input.GetKeyDown(key);

                if (!Result)
                    if (additionalKey != KeyCode.None)
                        Result = Input.GetKeyDown(additionalKey);
            }
        }
    }

    [System.Serializable]
    public class AxisInput : InputData<string, float>
    {
        public AxisInput(string key, string forAction) : base(key, forAction)
        {
        }

        protected override void Reset()
        {
            Result = 0f;
        }

        public override void Update()
        {
            if (enabled)
                Result = Input.GetAxis(key);
        }
    }

    [System.Serializable]
    public class MouseInput : InputData<int, bool>
    {
        public bool Released => Input.GetMouseButtonUp(key);

        public MouseInput(int key, string forAction) : base(key, forAction)
        {
        }

        protected override void Reset()
        {
            Result = false;
        }

        public override void Update()
        {
            if (enabled)
                Result = Input.GetMouseButtonDown(key);
        }
    }

    [System.Serializable]
    public class DoubleTapInput : InputData<KeyCode, bool>
    {
        // It allows to handle two keys assigned to one action
        [SerializeField] protected KeyCode additionalKey = KeyCode.None;
        [SerializeField] float activationDelta = .25f;

        float whenLastTap = 0f;

        public DoubleTapInput(KeyCode key, string forAction) : base(key, forAction)
        {
        }

        protected override void Reset()
        {
            whenLastTap = 0f;
            Result = false;
        }

        public override void Update()
        {
            if (enabled)
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
}
