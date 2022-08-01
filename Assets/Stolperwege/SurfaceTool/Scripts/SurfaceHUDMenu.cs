using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SurfaceHUDMenu : MonoBehaviour {

    /************************************************************************************
    Selection BUTTONS
    ************************************************************************************/

    public InteractiveButton DrawerButton { get; private set; }
    public InteractiveButton RecognizerButton { get; private set; }
    public InteractiveButton ConnectorButton { get; private set; }

    public GameObject SelectionMenu { get; private set; }
    public GameObject DrawerMenu { get; private set; }
    public GameObject RecognizerMenu { get; private set; }

    /************************************************************************************
    DRAWER BUTTONS
    ************************************************************************************/

    private InteractiveButton FingerButton;
    private InteractiveButton PointerButton;
    //private InteractiveButton FreeButton;
    //private InteractiveButton AngularButton;
    private InteractiveButton RectangularButton;
    private InteractiveButton SquareButton;
    private InteractiveButton EllipseButton;
    private InteractiveButton CircleButton;

    /************************************************************************************
    RECOGNIZER VARIABLES
    ************************************************************************************/

    private InteractiveButton IgnoreNoRightTriangle;
    private InteractiveButton IgnoreSmallSurfaces;
    public InteractiveButton StartRecognition { get; private set; }
    public InteractiveButton StopRecognition { get; private set; }
    public InteractiveButton CleanObject { get; private set; }
    public HUDMenuStatusBox StatusBox { get; private set; }

    private SurfaceToolInterface Interface;

    public void Init()
    {
        Interface = SceneController.GetInterface<SurfaceToolInterface>();


        InitSelectionMenu();
        InitDrawerMenu();
        InitRecognizerMenu();
        
    }


    private void InitSelectionMenu()
    {
        SelectionMenu = transform.Find("SelectionSubMenu").gameObject;

        DrawerButton = SelectionMenu.transform.Find("HUD_Button_Drawer").GetComponent<InteractiveButton>();
        DrawerButton.OnClick = () => { ActivateDrawerSubmenu(!DrawerMenu.activeInHierarchy); };

        RecognizerButton = SelectionMenu.transform.Find("HUD_Button_Recognizer").GetComponent<InteractiveButton>();
        RecognizerButton.OnClick = () => { ActivateRecognizerSubmenu(!RecognizerMenu.activeInHierarchy); };

        ConnectorButton = SelectionMenu.transform.Find("HUD_Button_Connector").GetComponent<InteractiveButton>();
    }

    private void InitDrawerMenu()
    {
        DrawerMenu = transform.Find("DrawerSubMenu").gameObject;

        FingerButton = DrawerMenu.transform.Find("PointerSelection/HUD_Button_Finger").GetComponent<InteractiveButton>();
        FingerButton.OnClick = () => {
            Interface.DrawMode = FingerButton.ButtonOn ? SurfaceToolInterface._DrawMode.None : SurfaceToolInterface._DrawMode.Finger;
        };

        PointerButton = DrawerMenu.transform.Find("PointerSelection/HUD_Button_Pointer").GetComponent<InteractiveButton>();
        PointerButton.OnClick = () => {
            Interface.DrawMode = PointerButton.ButtonOn ? SurfaceToolInterface._DrawMode.None : SurfaceToolInterface._DrawMode.Pointer;
        };

        //FreeButton = DrawerMenu.transform.Find("ShapeSelection/HUD_Button_Free").GetComponent<InteractiveButton>();
        //AngularButton = DrawerMenu.transform.Find("ShapeSelection/HUD_Button_Angular").GetComponent<InteractiveButton>();
        RectangularButton = DrawerMenu.transform.Find("ShapeSelection/HUD_Button_Rectangular").GetComponent<InteractiveButton>();
        RectangularButton.OnClick = () => { Interface.DrawType = SurfaceToolInterface._DrawType.Rectangular; };
        SquareButton = DrawerMenu.transform.Find("ShapeSelection/HUD_Button_Square").GetComponent<InteractiveButton>();
        SquareButton.OnClick = () => { Interface.DrawType = SurfaceToolInterface._DrawType.Square; };
        EllipseButton = DrawerMenu.transform.Find("ShapeSelection/HUD_Button_Ellipse").GetComponent<InteractiveButton>();
        EllipseButton.OnClick = () => { Interface.DrawType = SurfaceToolInterface._DrawType.Ellipse; };
        CircleButton = DrawerMenu.transform.Find("ShapeSelection/HUD_Button_Circle").GetComponent<InteractiveButton>();
        CircleButton.OnClick = () => { Interface.DrawType = SurfaceToolInterface._DrawType.Circle; };
    }

    private void InitRecognizerMenu()
    {
        RecognizerMenu = transform.Find("RecognizerSubMenu").gameObject;
        IgnoreNoRightTriangle = RecognizerMenu.transform.Find("HUD_Button_IgnoreNoRightTriangle").GetComponent<InteractiveButton>();
        IgnoreSmallSurfaces = RecognizerMenu.transform.Find("HUD_Button_IgnoreSmallSurfaces").GetComponent<InteractiveButton>();

        StartRecognition = RecognizerMenu.transform.Find("HUD_Button_Start").GetComponent<InteractiveButton>();
        StopRecognition = RecognizerMenu.transform.Find("HUD_Button_Stop").GetComponent<InteractiveButton>();
        CleanObject = RecognizerMenu.transform.Find("HUD_Button_Clean").GetComponent<InteractiveButton>();
        StatusBox = RecognizerMenu.transform.Find("Information").GetComponent<HUDMenuStatusBox>();
        StatusBox.Init();
    }

    public void ActualizeDrawerButtons()
    {
        FingerButton.ButtonOn = Interface.DrawMode == SurfaceToolInterface._DrawMode.Finger;
        PointerButton.ButtonOn = Interface.DrawMode == SurfaceToolInterface._DrawMode.Pointer;
        //FreeButton.ButtonOn = Interface.DrawType == SurfaceToolInterface._DrawType.Free;
        //AngularButton.ButtonOn = Interface.DrawType == SurfaceToolInterface._DrawType.Angular;
        RectangularButton.ButtonOn = Interface.DrawType == SurfaceToolInterface._DrawType.Rectangular;
        SquareButton.ButtonOn = Interface.DrawType == SurfaceToolInterface._DrawType.Square;
        EllipseButton.ButtonOn = Interface.DrawType == SurfaceToolInterface._DrawType.Ellipse;
        CircleButton.ButtonOn = Interface.DrawType == SurfaceToolInterface._DrawType.Circle;
    }

    public void ActualizeRecognizerButtons()
    {
        IgnoreNoRightTriangle.ButtonOn = Interface.SurfaceRecognizer.AngleFilter == SurfaceRecognizer.SurfaceAngleFilter.With90Deg;
        IgnoreSmallSurfaces.ButtonOn = Interface.SurfaceRecognizer.SizeFilter == SurfaceRecognizer.SurfaceSizeFilter.Big;
        StatusBox.SetFocus("No object in focus.");
        StatusBox.TurnOffStatus();
    }

    public void ActivateDrawerSubmenu(bool status)
    {
        if (ConnectorButton.ButtonOn) ConnectorButton.ButtonOn = false;
        DrawerButton.ButtonOn = status;
        DrawerMenu.SetActive(status);
        if (status && RecognizerMenu.activeInHierarchy) ActivateRecognizerSubmenu(false);
        ActualizeDrawerButtons();
        Interface.SurfaceTool = (status) ? SurfaceToolInterface.SurfaceMode.Drawer : SurfaceToolInterface.SurfaceMode.None;
    }

    public void ActivateRecognizerSubmenu(bool status)
    {
        if (ConnectorButton.ButtonOn) ConnectorButton.ButtonOn = false;
        RecognizerButton.ButtonOn = status;
        RecognizerMenu.SetActive(status);
        if (status && DrawerMenu.activeInHierarchy) ActivateDrawerSubmenu(false);
        ActualizeRecognizerButtons();
        Interface.SurfaceTool = (RecognizerButton.ButtonOn) ? SurfaceToolInterface.SurfaceMode.Recognizer : SurfaceToolInterface.SurfaceMode.None;
        if (RecognizerButton.ButtonOn) Interface.SurfaceRecognizer.SelectedObject = null;
    }

//    public IEnumerator HandleConnectorMode()
//    {
//        ConnectorButton.ButtonOn = !ConnectorButton.ButtonOn;
//        if (ActiveSubmenu != null)
//        {
//            SMenuActive = false;
//            while (_sMAnimOn) yield return null;
//            ActiveSubmenu = null;
//        }
//        StolperwegeHelper.player.SurfaceTool = (ConnectorButton.ButtonOn) ? OVRPlayerController.SurfaceMode.Connector : OVRPlayerController.SurfaceMode.None;
//    }

}
