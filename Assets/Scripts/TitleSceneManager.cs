using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] private InputAction AnyKeyAction;
    [SerializeField] private InputAction SAction;
    [SerializeField] private InputAction ShiftAction;

    [SerializeField] private GameObject ExplainCanvas;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AnyKeyAction?.Enable();
        SAction?.Enable();
        ShiftAction?.Enable();
        ExplainCanvas.SetActive(false);
        AnyKeyAction.performed += ActivateExplainCanvas;
    }

    public void ActivateExplainCanvas(InputAction.CallbackContext ctx)
    {
        ExplainCanvas.SetActive(true);
        AnyKeyAction.performed -= ActivateExplainCanvas;
        SAction.performed += CheckShift;
        ShiftAction.performed += CheckS;
    }

    public void CheckShift(InputAction.CallbackContext ctx)
    {
        if (ShiftAction.IsPressed())
        {
            SAction.performed -= CheckShift;
            ShiftAction.performed -= CheckS;
            GotoPlayScene();
        }
    }

    public void CheckS(InputAction.CallbackContext ctx)
    {
        if (SAction.IsPressed())
        {
            SAction.performed -= CheckS;
            ShiftAction.performed -= CheckShift;
            GotoPlayScene();
        }
    }

    private void GotoPlayScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void OnDestroy()
    {
        AnyKeyAction?.Disable();
        SAction?.Disable();
    }
}
