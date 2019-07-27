using System;

namespace RS9000
{
    public enum MessageType
    {
        Initialize,
        Heartbeat,
        SwitchMode,
        DisplayRadar,
        DisplayControl,
        RadarPower,
        AntennaPower,
        RadarBeep,
        TargetLock,
    }
}
