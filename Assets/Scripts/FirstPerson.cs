using UnityEngine;
public class FirstPerson : MonoBehaviour {
    public Camera view;
    [Range(1,1.5f)] public float zoomMagnification=1.2f;
    [Range(0,10)] public float sprintFovIncrease=5;
    [Range(0,1)] public float sprintRoll=.25f;
    [Range(4,30)] public float sprintRecoverySpeed=18;
    float runningBlend; bool running;
    StationLookControls lookControls;
    public bool InputCaptured => captured;
    CharacterController motor; float pitch,vertical,phase; bool captured=true,showHelp; Vector3 origin;
    void Start(){ motor=GetComponent<CharacterController>(); origin=transform.position; Lock(true); Debug.Log("STATION_FPS_RUNNING"); }
    void Lock(bool value){captured=value; Cursor.lockState=value?CursorLockMode.Locked:CursorLockMode.None; Cursor.visible=!value;}
    public void Teleport(Vector3 position,float yaw,bool resetLook){if(!motor)motor=GetComponent<CharacterController>();motor.enabled=false;transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));vertical=0;motor.enabled=true;if(resetLook){pitch=0;view.transform.localRotation=Quaternion.identity;}var post=view.GetComponent<UnityEngine.Rendering.PostProcessing.PostProcessLayer>();if(post)post.ResetHistory();}
    void LateUpdate(){if(!lookControls)lookControls=FindFirstObjectByType<StationLookControls>();float normal=lookControls?lookControls.fieldOfView:66;bool zoom=captured&&Input.GetMouseButton(1);var d=GetComponent<StationDisplay>();bool menu=d&&d.MenuOpen;if(menu)zoom=false;bool sprintActive=running&&captured&&!menu&&!zoom;float recovery=sprintActive?4:sprintRecoverySpeed;runningBlend=Mathf.Lerp(runningBlend,sprintActive?1:0,1-Mathf.Exp(-recovery*Time.deltaTime));float target=zoom?2*Mathf.Atan(Mathf.Tan(normal*Mathf.Deg2Rad*.5f)/zoomMagnification)*Mathf.Rad2Deg:normal+runningBlend*sprintFovIncrease;view.fieldOfView=Mathf.Lerp(view.fieldOfView,target,1-Mathf.Exp(-(zoom?12:sprintActive?12:sprintRecoverySpeed)*Time.deltaTime));view.transform.localRotation=Quaternion.Euler(pitch,0,Mathf.Sin(phase*.5f)*sprintRoll*runningBlend);}
    void Update(){
        var display=GetComponent<StationDisplay>();if(display && display.MenuOpen)return;
        if(Input.GetKeyDown(KeyCode.F1))showHelp=!showHelp;
        if(Input.GetKeyDown(KeyCode.Escape)) Lock(!captured);
        if(!captured){if(Input.GetMouseButtonDown(0))Lock(true);return;}
        transform.Rotate(0,Input.GetAxisRaw("Mouse X")*1.65f,0);
        pitch=Mathf.Clamp(pitch-Input.GetAxisRaw("Mouse Y")*1.65f,-85,85); view.transform.localRotation=Quaternion.Euler(pitch,0,0);
        float x=Input.GetAxisRaw("Horizontal"),z=Input.GetAxisRaw("Vertical");
        Vector3 direction=Vector3.ClampMagnitude(transform.right*x+transform.forward*z,1);
        bool sprint=Input.GetKey(KeyCode.LeftShift), crouch=Input.GetKey(KeyCode.LeftControl);
        motor.height=crouch?1.25f:1.8f; motor.center=Vector3.up*motor.height*.5f;
        if(motor.isGrounded && vertical<0)vertical=-2;
        if(motor.isGrounded && Input.GetKeyDown(KeyCode.Space) && !crouch)vertical=4;
        vertical-=18*Time.deltaTime; motor.Move((direction*(crouch?1.5f:sprint?4.6f:2.65f)+Vector3.up*vertical)*Time.deltaTime);
        running=sprint&&!crouch&&motor.isGrounded&&new Vector2(motor.velocity.x,motor.velocity.z).magnitude>3;
        phase+=direction.magnitude*Time.deltaTime*(sprint?11:7);
        float eye=(crouch?1.12f:1.67f)+ (motor.isGrounded?Mathf.Sin(phase)*direction.magnitude*.014f:0);
        view.transform.localPosition=Vector3.Lerp(view.transform.localPosition,new Vector3(0,eye,0),Time.deltaTime*12);
        if(transform.position.y < -8 || Input.GetKeyDown(KeyCode.R)){var loop=FindFirstObjectByType<CorridorLoop>();if(loop)loop.ResetRun();else Teleport(origin,0,true);}
    }
    void OnGUI(){
        GUI.color=new Color(1,1,1,.65f); if(showHelp || Time.timeSinceLevelLoad<7)GUI.Label(new Rect(24,Screen.height-36,1400,25),"WASD  Yürü     SHIFT  Hızlan     CTRL  Eğil     SPACE  Zıpla     SAĞ TIK  Yakınlaş     ESC  Fareyi bırak     R  Yeni oyun     F1  Yardım     F2  Görüntü     F11  Tam ekran");
        if(!captured){GUI.Box(new Rect(Screen.width/2-145,Screen.height/2-30,290,60),"DEVAM ETMEK İÇİN TIKLA");}
    }
}
