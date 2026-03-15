using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ReTryButtonManager : MonoBehaviour
{
    [SerializeField] private InputAction SAction;
    [SerializeField] private InputAction ShiftAction;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        SAction?.Enable();
        ShiftAction?.Enable();
        SAction.performed += CheckShift;
        ShiftAction.performed += CheckS;
    }

    public void CheckShift(InputAction.CallbackContext ctx)
    {
        if (ShiftAction.IsPressed())
        {
            SAction.performed -= CheckShift;
            ShiftAction.performed -= CheckS;
            SceneReload();
        }
    }

    public void CheckS(InputAction.CallbackContext ctx)
    {
        if (SAction.IsPressed())
        {
            SAction.performed -= CheckS;
            ShiftAction.performed -= CheckShift;
            SceneReload();
        }
    }

    private void OnDestroy()
    {
        SAction?.Disable();
        ShiftAction?.Disable();
    }

    public void SceneReload()
    {
        SceneManager.LoadScene("GameScene");
    }
}
