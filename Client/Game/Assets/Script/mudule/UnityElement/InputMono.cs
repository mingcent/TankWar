using UnityEngine;
using FixPoint;


public class InputMono : MonoBehaviour
{

    public VInt2 inputUV;
    public bool isInputFire;

    void Start()
    {
    }

    public void Update()
    {
        if (World.isRuning)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            inputUV = new VInt2(new VInt(h).i, new VInt(v).i);

            isInputFire = Input.GetKey(KeyCode.Space);

            InputService.currentInput = new PlayerInput()
            {
                inputUV = inputUV,
                isInputFire = isInputFire,
            };
        }
    }
}
