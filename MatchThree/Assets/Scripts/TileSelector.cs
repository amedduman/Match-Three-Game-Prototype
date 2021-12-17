using System;
using UnityEngine;

public class TileSelector : MonoBehaviour
{
    [SerializeField] Camera gameCam;
    [SerializeField] LayerMask tileLayerMask;

    public Action<Tile> OnTileSelected;
    bool canRaycast;
    
    #region RegisterEvents

    void OnEnable()
    {
        InputManager.Instance.OnMouseButtonDown += HandlePlayerInput;
    }

    void OnDisable()
    {
        InputManager.Instance.OnMouseButtonDown -= HandlePlayerInput;
    }

    #endregion

    void HandlePlayerInput()
    {
        canRaycast = true;
    }

    void FixedUpdate()
    {
        if (!canRaycast) return;
        
        // calculate necessary info for ray
        Vector3 rayOrigin = gameCam.transform.position;
        Vector3 rayDestination = new Vector3(gameCam.ScreenToWorldPoint(Input.mousePosition).x,
            gameCam.ScreenToWorldPoint(Input.mousePosition).y
            ,0);
        Vector3 rayDirection = rayDestination - rayOrigin;
        
        Ray ray = new Ray(rayOrigin, rayDirection);
        
        // cast a ray from camera to mouse pos
        RaycastHit2D hitInfo = Physics2D.GetRayIntersection(ray, Mathf.Infinity, tileLayerMask);
        
        if (hitInfo.transform != null) // hit something?
        {
            if (hitInfo.transform.CompareTag("Tile")) // hit tile?
            {
                Tile selectedTile = hitInfo.transform.GetComponent<Tile>();
                OnTileSelected?.Invoke(selectedTile);
            }
        }
        
        canRaycast = false;
    }

}
