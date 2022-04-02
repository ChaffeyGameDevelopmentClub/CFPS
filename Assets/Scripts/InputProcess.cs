using System;
using UnityEngine;
using UnityEngine.UIElements;

/*
 * ---Old code I took from an old project of mine. Could probably be simplify, but should work as is---
 * 
 * See enum InputType at bottom of class for details on supported input types.
 * 
 * InputProcess is to be constructed/instantiated for desired input types.
 * The given input can be instantiated as a toggle or standard input.
 * Any instance of InputProcess must be called in the Update method of a script.
 * 
 * Declaration goes as follows:
 * InputProcess myInput = new InputProcess(name, bool, KeyCode);
 */
public class InputProcess
{
    public bool inputUp = false; //True for given input type on up press
    public bool inputDown = false; //True for given input type on down press

    private bool inputUpConst = false;
    private bool inputDownConst = false;
    private bool inputToggle = false;

    private bool instantInputUp = false;
    private bool instantInputDown = false;

    public string name;


    private InputType inputType;
    public KeyCode keyCode;
    private bool isMouseButton;
    public int mouseButton;

    //class constructor
    public InputProcess(string name, InputType inputType, KeyCode keyCode) //toggleMode true for toggle control
    {
        this.inputType = inputType;
        this.keyCode = keyCode;
        this.isMouseButton = false;
        this.name = name;
    }

    public InputProcess(string inputName, InputType inputType, int mouseButton)
    {
        this.inputType = inputType;
        this.mouseButton = mouseButton;
        this.isMouseButton = true;
        this.name = inputName;
    }

    //must be called in Update method 
    public void Process()
    {

        if (inputType == InputType.TOGGLE)
        {
            TogglePress();
        }
        else
        {
            Press();
        }

        switch (inputType)
        {
            case InputType.INSTANT:
                inputUp = instantInputUp;
                inputDown = instantInputDown;
                break;
            case InputType.CONSTANT:
                inputUp = inputUpConst;
                inputDown = inputDownConst;
                break;
            case InputType.TOGGLE:
                inputUp = !inputToggle;
                inputDown = inputToggle;
                break;
        }
    }

    // Method for toggled control
    private void TogglePress()
    {
        switch (isMouseButton)
        {
            case false when Input.GetKeyDown(keyCode) && !inputToggle:
                inputToggle = true;
                inputUpConst = false;
                inputUpConst = (Input.GetKeyUp(keyCode) && !inputUpConst && inputToggle) ? true : false;
                break;
            case false when Input.GetKeyDown(keyCode) && inputToggle && !inputUpConst:
                inputToggle = false;
                inputUpConst = (Input.GetKeyUp(keyCode) && !inputUpConst && inputToggle) ? true : false;
                break;
            case true when Input.GetMouseButtonDown(mouseButton) && !inputToggle:
                inputToggle = true;
                inputUpConst = false;
                inputUpConst = (Input.GetMouseButtonUp(mouseButton) && !inputUpConst && inputToggle) ? true : false;
                break;
            case true when Input.GetMouseButtonDown(mouseButton) && inputToggle && !inputUpConst:
                inputToggle = false;
                inputUpConst = (Input.GetMouseButtonUp(mouseButton) && !inputUpConst && inputToggle) ? true : false;
                break;
            default:
                break;
        }
    }

    // Method for on press control
    private void Press()
    {
        switch (isMouseButton)
        {
            case false when Input.GetKeyDown(keyCode) && !Input.GetKeyUp(keyCode):
                this.inputDownConst = true;
                this.inputUpConst = false;
                this.instantInputDown = true;
                break;
            case false when !Input.GetKeyDown(keyCode) && Input.GetKeyUp(keyCode):
                this.inputDownConst = false;
                this.inputUpConst = true;
                this.instantInputUp = true;
                break;
            case true when Input.GetMouseButtonDown(mouseButton) && !Input.GetMouseButtonUp(mouseButton):
                this.inputDownConst = true;
                this.inputUpConst = false;
                this.instantInputDown = true;
                break;
            case true when !Input.GetMouseButtonDown(mouseButton) && Input.GetMouseButtonUp(mouseButton):
                this.inputDownConst = false;
                this.inputUpConst = true;
                this.instantInputUp = true;
                break;
            default:
                this.instantInputUp = false;
                this.instantInputDown = false;
                break;
        }
    }

    /*
     * TOGGLE: True on input toggle
     * INSTANT: True on instant frame of input
     * CONSTANT: True on all frames of input
     **/
    public enum InputType
    {
        TOGGLE,
        INSTANT,
        CONSTANT,
    }
}