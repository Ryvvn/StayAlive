using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using Unity.Netcode.Components;


public class ClientNetworkTransform : NetworkTransform
{
    //Disable authority 
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
