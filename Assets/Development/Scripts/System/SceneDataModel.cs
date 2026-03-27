using System;
using UnityEngine;

[Serializable]
public class InteractableData
{
    public int id;
    public bool wasInteractedWith;
}

[Serializable]
public class SceneData
{
    public InteractableData[] interactables;
}