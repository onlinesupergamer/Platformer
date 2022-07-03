using UnityEngine;
using System.Collections;
public class Movement : MonoBehaviour
{
    Player_Input m_playerInput;

    private void Awake()
    {
        m_playerInput = new Player_Input();

        m_playerInput.CharacterControls.Move.started += context => 
        {
            Debug.Log(context.ReadValue<Vector2>());
        
        };
    }

    void OnEnable()
    {
        m_playerInput.CharacterControls.Enable();
    }

    void OnDisable()
    {
        m_playerInput.CharacterControls.Disable();
    }


    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else 
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
    
}
