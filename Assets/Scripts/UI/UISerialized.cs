using UnityEngine;
using UnityEngine.UI;

public class UISerialized : WindowComponent
{
//BINDING_DEFINITION_BEGIN
//BINDING_DEFINITION_END
   
    private void Awake()
    {
        //BINDING_CODE_BEGIN
        //BINDING_CODE_END

        SerializedView component = GetComponent<SerializedView>();
        component.AddClick("ButtonMain", () => WindowManager.Instance.Open("UIMain"));
        component.AddClick("ButtonPop", () => WindowManager.Instance.Open("UIPop"));
        component.AddClick("ButtonWidget", () => WindowManager.Instance.Open("UIWidget"));
    } 
}