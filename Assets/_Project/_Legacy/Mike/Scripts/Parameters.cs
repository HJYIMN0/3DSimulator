using UnityEngine;

public static class Parameters
{
    [Header("Setting Movement")]
    public static string ParameterFloatSpeed = "Speed";
    public static string ParameterFloatDirection = "Direction";
    public static string ParameterFloatTurning = "Turning";

    public static string ParameterClipName1 = "HumanF@Idle01";
    public static string ParameterClipName2 = "HumanF@Idle02";


    [Header("Parameter Generic")]
    public static string ParameterTriggerOnGenericAction = "GenericAction";

    [Header("Parameter Movement")]
    public static string ParameterTriggerOnIdle = "Idle";
    public static string ParameterTriggerOnMovingGround = "MovingGround";
    public static string ParameterTriggerOnMovingAir = "MovingAir";
    public static string ParameterTriggerStandUpFront = "Stand Up Front";
    public static string ParameterTriggerStandUpBack = "Stand Up Back";
    public static string ParameterTriggerDeath = "Death";

}
