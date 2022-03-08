using UnityEngine;

public partial class PlayerInput
{
    public abstract class InputData
    {
        [SerializeField] protected bool enabled = true;

        // Allows to assign Input to an action e.g. a jump 
        public string ForAction { get; private set; }

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

        public InputData(string forAction)
        {
            ForAction = forAction;
        }

        public abstract void Reset();

        public abstract void Update();
    }

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
}
