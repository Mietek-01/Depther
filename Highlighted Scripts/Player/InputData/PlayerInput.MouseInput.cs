using UnityEngine;

public partial class PlayerInput
{
    [System.Serializable]
    public class MouseInput : InputData<int, bool>
    {
        public bool Released => Input.GetMouseButtonUp(key);

        public MouseInput(int key, string forAction) : base(key, forAction) { }

        protected override void Reset()
        {
            Result = false;
        }

        public override void Update()
        {
            Result = Input.GetMouseButtonDown(key);
        }
    }
}
