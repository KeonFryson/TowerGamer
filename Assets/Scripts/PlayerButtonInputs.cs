using UnityEngine;

public class PlayerButtonInput : MonoBehaviour
{


    private InputSystem_Actions inputActions;
    private TowerShopUI towerShopUI;


    void Awake()
    {

        inputActions = new InputSystem_Actions();
        towerShopUI = FindFirstObjectByType<TowerShopUI>();
        inputActions.UI.TowerShop.performed += ctx => towerShopUI.ToggleShop();

    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {




    }



    void Update()
    {



    }



}
