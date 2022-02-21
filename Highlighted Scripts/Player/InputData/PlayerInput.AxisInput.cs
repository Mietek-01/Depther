using UnityEngine;

public partial class PlayerInput
{
    [System.Serializable]
    public class AxisInput : InputData<string, float>
    {
        public AxisInput(string key, string forAction) : base(key, forAction) { }

        protected override void Reset()
        {
            Result = 0f;
        }

        public override void Update()
        {
            Result = Input.GetAxis(key);
        }
    }
}
