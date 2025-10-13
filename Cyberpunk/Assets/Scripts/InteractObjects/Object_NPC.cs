using UnityEngine;

public class Object_NPC : MonoBehaviour
{
    protected UI ui;

    protected virtual void Awake()
    {
        ui = FindFirstObjectByType<UI>();
    }

    protected virtual void Start()
    {

    }
    
    protected virtual void Update()
    {

    }
    
    public virtual void Interact()
    {

    }
}