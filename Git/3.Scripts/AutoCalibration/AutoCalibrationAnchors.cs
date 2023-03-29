using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoCalibrationAnchors : MonoBehaviour
{
    [SerializeField] private OVRSpatialAnchor _firstAnchorPrefab;
    [SerializeField] private OVRSpatialAnchor _secondAnchorPrefab;
    private CalibrationPassthroughState _calibrationPassthroughState;
    private Action<OVRSpatialAnchor.UnboundAnchor, bool> _onLoadAnchor;
    public const string NumUuidsPlayerPref = "numUuids";
    public const string FirstCalibrrationPointUuid = "fCalibrationPoint";
    public const string SecondCalibrrationPointUuid = "sCalibrationPoint";
    private bool firstPoint;
    private bool firstAnchor = true;

    private void Awake()
    {
        _onLoadAnchor = OnLocalized;
        firstPoint = true;
    }

    private void Start()
    {
        _calibrationPassthroughState = GetComponent<CalibrationPassthroughState>();
        LoadTwoAnchors();
        _calibrationPassthroughState.SetPassthroughHiddenState(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameSettings.LocalInstance != null)
        {
            if (GameSettings.LocalInstance.UseControllerInstead)
            {
                if (OVRInput.GetUp(OVRInput.RawButton.Y))
                    _calibrationPassthroughState.SetPassthroughHiddenState(!_calibrationPassthroughState
                        .GetPassthroughHiddenState());

                else if (OVRInput.GetUp(OVRInput.RawButton.X) && GameSettings.LocalInstance.UseExperimenatalCalibration && !_calibrationPassthroughState.GetPassthroughHiddenState())
                    GenerateObject(false);
            }

            else
            {
                if (OVRInput.GetUp(OVRInput.RawButton.B))
                    _calibrationPassthroughState.SetPassthroughHiddenState(!_calibrationPassthroughState
                        .GetPassthroughHiddenState());

                else if (OVRInput.GetUp(OVRInput.RawButton.A) && GameSettings.LocalInstance.UseExperimenatalCalibration && !_calibrationPassthroughState.GetPassthroughHiddenState())
                    GenerateObject(true);
            }
        }
    }

    private void GenerateObject(bool isRightController)
    {
        OVRPose controllerPose = new OVRPose()
        {
            position = OVRInput.GetLocalControllerPosition(isRightController
                ? OVRInput.Controller.RTouch
                : OVRInput.Controller.LTouch),
            orientation =
                OVRInput.GetLocalControllerRotation(isRightController ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch)
        };

        OVRPose wolrOvrPose = OVRExtensions.ToWorldSpacePose(controllerPose);
        Instantiate(firstPoint ? _firstAnchorPrefab : _secondAnchorPrefab, wolrOvrPose.position, wolrOvrPose.orientation);
        firstPoint = !firstPoint;
    }

    private void LoadTwoAnchors()
    {
        LoadAnchorByKey(FirstCalibrrationPointUuid);
        LoadAnchorByKey(SecondCalibrrationPointUuid);
    }

    private void LoadAnchorByKey(string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            Guid[] anchorUuid = new Guid[1];
            anchorUuid[0] = new Guid(PlayerPrefs.GetString(key));
            Load(new OVRSpatialAnchor.LoadOptions
            {
                Timeout = 0,
                StorageLocation = OVRSpace.StorageLocation.Local,
                Uuids = anchorUuid
            });
        }
    }

    private void Load(OVRSpatialAnchor.LoadOptions options) => OVRSpatialAnchor.LoadUnboundAnchors(options, anchors =>
    {
        if (anchors == null)
        {
            Log("Query failed.");
            return;
        }

        foreach (var anchor in anchors)
        {
            if (anchor.Localized)
            {
                _onLoadAnchor(anchor, true);
            }
            else if (!anchor.Localizing)
            {
                anchor.Localize(_onLoadAnchor);
            }
        }
    });

    private void OnLocalized(OVRSpatialAnchor.UnboundAnchor unboundAnchor, bool success)
    {
        if (!success)
        {
            Log($"{unboundAnchor} Localization failed!");
            return;
        }

        var pose = unboundAnchor.Pose;
        var spatialAnchor = Instantiate(firstAnchor ? _firstAnchorPrefab : _secondAnchorPrefab, pose.position, pose.rotation);
        firstAnchor = !firstAnchor;
        unboundAnchor.BindTo(spatialAnchor);
        spatialAnchor.GetComponent<SpatialAnchor>().ChangeMeshVisibility(!_calibrationPassthroughState.GetPassthroughHiddenState());
    }

    private static void Log(string message) => Debug.LogError($"[SpatialAnchorsUnity]: {message}");
}
