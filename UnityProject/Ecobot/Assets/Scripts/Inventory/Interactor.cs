using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour  // ! remake переделать в интерфейс? -> VISITOR PATTERN?
{
    public Transform InteractionPoint;
    public LayerMask InteractionLayer;
    public float InteractionPointRadius;
    public bool IsInteracting { get; private set; }

    private void Update()
    {
        // я так понимаю, что это один из способов как сделать взаимодействие.
        // remake ! думаю стоит потом переделать на raycast. и в целом наверное стоит сделать это на стороне игрока, а не предмета
        var colliders = Physics.OverlapSphere(InteractionPoint.position, InteractionPointRadius, InteractionLayer);

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                var interactable = colliders[i].GetComponent<IInteractable>();

                if (interactable != null)
                {
                    StartInteraction(interactable);
                }
            }
        }
    }

    private void StartInteraction(IInteractable interactable)
    {
        interactable.Interact(this, out bool success);
        IsInteracting = true;
    }

    void EndInteraction()
    {
        IsInteracting = false;
    }
}