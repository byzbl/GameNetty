using System.Collections;
using System.Collections.Generic;
using ET;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour
{
    public InputField account;
    public InputField password;
    public Button loginBtn;

    public Button testBtn;

    // Start is called before the first frame update
    void Start()
    {
        loginBtn.onClick.AddListener(OnClickLogin);
        testBtn.onClick.AddListener(OnClickTest);
    }

    // Update is called once per frame
    void OnClickLogin()
    {
        LoginHelper.Login(Init.Root, account.text, password.text).Coroutine();
    }

    async void OnClickTest()
    {
        M2C_TestResponse M2C_TestResponse = await Init.Root.GetComponent<ClientSenderComponent>().Call(
            new C2M_TestRequest
            {
                request = "123"
            }) as M2C_TestResponse;
       
    }
}