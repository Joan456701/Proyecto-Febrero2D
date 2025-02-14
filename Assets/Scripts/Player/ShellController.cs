using UnityEngine;

public class ShellController : MonoBehaviour
{
    #region Variables
    public BoxCollider2D cCollider;
    public SpriteRenderer cRender;
    public Transform playerTransform;
    public Vector3 offset;

    private bool isShellRemoved = false;
    #endregion

    #region Metodos Unity
    private void Start()
    {
        cCollider.enabled = false;
        cRender.enabled = false;

    }

    //Caparazon quitado
    public void ShellTaked()
    {
        isShellRemoved = true;
        cCollider.transform.SetParent(null);
        cCollider.enabled = true;
        cRender.enabled = true;
    }

    //Caparazon puesto
    public void ShellPut(Transform parentTransform)
    {
        isShellRemoved = false;
        cCollider.transform.SetParent(parentTransform);
        cCollider.enabled = false;
        cRender.enabled = false;
    }

    //Posicion de caparazon al quitartelo
    private void ShellPosition()
    {
        if (isShellRemoved)
        {
            transform.position = playerTransform.position + offset;
        }
    }

    //Variable para limitar acciones
    public bool ShellRemoved()
    { return isShellRemoved; }

    public bool ShellOn()
    { return !isShellRemoved; }
    #endregion
}
