
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class test : UdonSharpBehaviour
{
    private Camera _camera;
    public GameObject cube;
    private Quaternion originrot;
    private Vector3 originpos;
    void Start()
    {
        _camera = gameObject.GetComponent<Camera>();
        _camera.enabled = true;
        originrot = cube.transform.rotation;
        originpos = cube.transform.position;
    }

    void Update()
    {
        cube.transform.rotation = originrot * Quaternion.Inverse(gameObject.transform.localRotation);
        cube.transform.position = originpos - gameObject.transform.localPosition;
    }
}
