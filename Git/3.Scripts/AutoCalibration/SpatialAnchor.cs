using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialAnchor : MonoBehaviour
{
    private string NumUuidsPlayerPref;
    private OVRSpatialAnchor _spatialAnchor;
    [SerializeField]
    private CalibrationPoint calibrationPoint;

    private MeshRenderer sphereRenderer;

    public static SpatialAnchor FirstAnchorInstance { get; private set; }
    public static SpatialAnchor SecondAnchorInstance { get; private set; }

    private void Awake()
    {
        sphereRenderer = GetComponentInChildren<MeshRenderer>();
        _spatialAnchor = GetComponent<OVRSpatialAnchor>();
        ChangeMeshVisibility(false);

        if (calibrationPoint == CalibrationPoint.FirstPoint)
            NumUuidsPlayerPref = AutoCalibrationAnchors.FirstCalibrrationPointUuid;
        else
            NumUuidsPlayerPref = AutoCalibrationAnchors.SecondCalibrrationPointUuid;

        if (calibrationPoint == CalibrationPoint.FirstPoint)
        {
            if (FirstAnchorInstance != null)
                FirstAnchorInstance.ChangeMeshVisibility(false);

            FirstAnchorInstance = this;
        }

        else if (calibrationPoint == CalibrationPoint.SecondPoint)
        {
            if (SecondAnchorInstance != null)
                SecondAnchorInstance.ChangeMeshVisibility(false);

            SecondAnchorInstance = this;
        }
    }

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        while (_spatialAnchor && !_spatialAnchor.Created)
        {
            yield return null;
        }

        OVRSpatialAnchor.SaveOptions saveOptions = new OVRSpatialAnchor.SaveOptions()
        {
            Storage = OVRSpace.StorageLocation.Local
        };

        GetComponent<OVRSpatialAnchor>().Save(saveOptions, (anchor, success) =>
        {
            if (!success) return;

            // Write uuid of saved anchor to file
            if (!PlayerPrefs.HasKey(NumUuidsPlayerPref))
            {
                PlayerPrefs.SetInt(NumUuidsPlayerPref, 0);
            }

            PlayerPrefs.SetString(NumUuidsPlayerPref, anchor.Uuid.ToString());
        });
    }
    public void ChangeMeshVisibility(bool state)
    {
        sphereRenderer.forceRenderingOff = !state;
    }

    private enum CalibrationPoint
    {
        FirstPoint = 0,
        SecondPoint = 1
    }
}
