///////////////////////////////////////////////////////////////////////////////
//                                                                           //
// Planar Reflections Probe for Unity                                        //
//                                                                           //
// Universal RP Port made by Marcell Hermanowski                             //
// Author: Rafael Bordoni                                                    //
// Date: January 25, 2022                                                    //
// Last Update: April 14, 2023                                               //
// Email: rafaelbordoni00@gmail.com                                          //
// Repository: https://github.com/eldskald/planar-reflections-unity          //
//                                                                           //
///////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways, AddComponentMenu("Rendering/Planar Reflections Probe")]
public class PlanarReflectionsProbe : MonoBehaviour
{
    [Space(10)]
    public bool useCustomNormal = false;
    public Vector3 customNormal;
    [Space(10)]
    [Range(0.01f, 1.0f)] public float reflectionsQuality = 1f;
    public float farClipPlane = 1000;
    public bool renderBackground = true;
    [Space(10)]
    public bool renderInEditor = false;
    
    [Header("Manual Assignments")]
    public Camera reflectionCamera; // Assigned manually
    private RenderTexture reflectionTexture; // Now dynamically created

    private Skybox _probeSkybox;

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += PreRender;
        CreateRenderTexture();
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= PreRender;
        CleanupRenderTexture();
    }

    private bool CheckCamera(Camera cam)
    {
        if (cam.cameraType == CameraType.Reflection) return true;
        if (!renderInEditor && cam.cameraType == CameraType.SceneView) return true;
        return false;
    }

    private void PreRender(ScriptableRenderContext context, Camera cam)
    {
        if (CheckCamera(cam) || reflectionCamera == null) return;
        
        CreateRenderTexture(); // Ensure RenderTexture is valid
        
        Vector3 normal = GetNormal();
        UpdateProbeSettings(cam);
        UpdateProbeTransform(cam, normal);
        CalculateObliqueProjection(normal);
        
        UniversalRenderPipeline.RenderSingleCamera(context, reflectionCamera);
        reflectionTexture.SetGlobalShaderProperty("_PlanarReflectionsTex");
    }

    private void UpdateProbeSettings(Camera cam)
    {
        reflectionCamera.CopyFrom(cam);
        reflectionCamera.enabled = false;
        reflectionCamera.cameraType = CameraType.Reflection;
        reflectionCamera.farClipPlane = farClipPlane;
        _probeSkybox = reflectionCamera.GetComponent<Skybox>();
        if (!_probeSkybox) _probeSkybox = reflectionCamera.gameObject.AddComponent<Skybox>();

        Skybox mainCameraSkybox = cam.GetComponent<Skybox>();
        if (_probeSkybox == null) {
            _probeSkybox = reflectionCamera.gameObject.AddComponent<Skybox>();
        }

        // If the main camera has a Skybox, copy it. Otherwise, use RenderSettings.skybox.
        if (mainCameraSkybox != null && mainCameraSkybox.material != null) {
            _probeSkybox.material = renderBackground ? mainCameraSkybox.material : null;
        } else {
            _probeSkybox.material = renderBackground ? RenderSettings.skybox : null;
        }

        _probeSkybox.enabled = (_probeSkybox.material != null);

        reflectionCamera.targetTexture = reflectionTexture;
    }

    private Vector3 GetNormal()
    {
        if (!useCustomNormal) return transform.forward;
        return customNormal == Vector3.zero ? Vector3.up : customNormal.normalized;
    }

    private void UpdateProbeTransform(Camera cam, Vector3 normal)
    {
        Vector3 proj = normal * Vector3.Dot(normal, cam.transform.position - transform.position);
        reflectionCamera.transform.position = cam.transform.position - 2 * proj;
        
        Vector3 probeForward = Vector3.Reflect(cam.transform.forward, normal);
        Vector3 probeUp = Vector3.Reflect(cam.transform.up, normal);
        reflectionCamera.transform.LookAt(reflectionCamera.transform.position + probeForward, probeUp);
    }

    private void CalculateObliqueProjection(Vector3 normal)
    {
        Matrix4x4 viewMatrix = reflectionCamera.worldToCameraMatrix;
        Vector3 viewPosition = viewMatrix.MultiplyPoint(transform.position);
        Vector3 viewNormal = viewMatrix.MultiplyVector(normal);
        Vector4 plane = new Vector4(viewNormal.x, viewNormal.y, viewNormal.z, -Vector3.Dot(viewPosition, viewNormal));
        reflectionCamera.projectionMatrix = reflectionCamera.CalculateObliqueMatrix(plane);
    }

    private void CreateRenderTexture() 
    {
        if (reflectionTexture != null && 
            reflectionTexture.width == (int)(Screen.width * reflectionsQuality) &&
            reflectionTexture.height == (int)(Screen.height * reflectionsQuality)) return;
        
        CleanupRenderTexture();
        
        reflectionTexture = new RenderTexture(
            (int)(Screen.width * reflectionsQuality), 
            (int)(Screen.height * reflectionsQuality), 
            16, 
            RenderTextureFormat.ARGB32
        ) {
            name = "Planar Reflection Texture",
            useMipMap = false,
            autoGenerateMips = false
        };
    }

    private void CleanupRenderTexture()
    {
        if (reflectionTexture) {
            reflectionTexture.Release();
            DestroyImmediate(reflectionTexture);
            reflectionTexture = null;
        }
    }    

}
