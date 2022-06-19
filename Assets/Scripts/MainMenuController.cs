using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private GameObject setupMenuContainer;
    private GameObject beginMenuContainer;
    private GameObject mainMenu;
    private Button beginButton;
    private Button confirmButton;
    private InputField widthInput;
    private InputField lengthInput;
    private InputField heightInput;
    private Text errorMessage;
    void Start()
    {

        //Load the main menu object
        mainMenu = GameObject.Find("MainMenu");
        //Load the containers for each submenu
        beginMenuContainer = mainMenu.transform.Find("BeginMenuContainer").gameObject;
        setupMenuContainer = mainMenu.transform.Find("SetupMenuContainer").gameObject;

        //add a listener for the first button
        beginButton = beginMenuContainer.transform.Find("BeginButton").GetComponent<Button>();
        beginButton.onClick.AddListener(delegate { beginButtonClick(); });

        //load the components of the second submenu
        widthInput = setupMenuContainer.transform.Find("InputWidth").GetComponent<InputField>();
        lengthInput = setupMenuContainer.transform.Find("InputLength").GetComponent<InputField>();
        heightInput = setupMenuContainer.transform.Find("InputHeight").GetComponent<InputField>();
        confirmButton = setupMenuContainer.transform.Find("ConfirmButton").GetComponent<Button>();
        errorMessage = setupMenuContainer.transform.Find("ErrorMessageText").GetComponent<Text>();

        //add a listener for the second button
        confirmButton.onClick.AddListener(delegate { confirmButtonClick(); });

       // Debug.Log(widthInput.gameObject.name);


    }

    void beginButtonClick()
    {
        beginMenuContainer.SetActive(false);
        setupMenuContainer.SetActive(true);
    }

    void confirmButtonClick()
    {
        //Debug.Log(widthInput.text + " " + lengthInput.text);
        if (widthInput.text.Length > 0 && lengthInput.text.Length > 0)
        {
            Room.Width = float.Parse(widthInput.text);
            Room.Length = float.Parse(lengthInput.text);
            if (heightInput.text.Length > 0)
            {
                Room.Height = float.Parse(heightInput.text);
            }
            SceneManager.LoadScene("VirtualRoomScene");
        }
        else
        {
            errorMessage.gameObject.SetActive(true);
        }
    }

}
