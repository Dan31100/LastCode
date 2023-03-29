using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoCalibration : MonoBehaviour
{
    private GameObject _VRWorldPointA;
    private GameObject _VRWorldPointB;
    private GameObject _realWorldPointA;
    private GameObject _realWorldPointB;
    private AudioSource _checkSound;
    private CalibrationLostDetector _lostDetector;
    private Calibration _calibration;
    private CalibrationPassthroughState _calibrationPassthrough;


    private IEnumerator Start()
    {
        _calibration = GetComponent<Calibration>();
        _checkSound = GetComponent<AudioSource>();
        _calibrationPassthrough = GetComponent<CalibrationPassthroughState>();
        _lostDetector = GetComponent<CalibrationLostDetector>();
        UpdateVRPoints();

        while (!_realWorldPointA && !_realWorldPointB)
        {
            UpdateAnchors();
            yield return new WaitForSeconds(1);
        }

        while (GameSettings.LocalInstance == null)
        {
            yield return new WaitForSeconds(1);
        }

        if (GameSettings.LocalInstance.UseExperimenatalCalibration)
        {
            Calibrate();
            yield return new WaitForSeconds(1);
            Calibrate();
        }

        yield return null;
        StartCoroutine(AutoCalibrationCoroutine());
        yield break;
    }

    private IEnumerator AutoCalibrationCoroutine()
    {
        while (true)
        {
            if ((GameSettings.LocalInstance && GameSettings.LocalInstance.UseExperimenatalCalibration) 
                && (!_calibrationPassthrough.GetPassthroughHiddenState() || _lostDetector.Uncalibrated))
            {
                yield return new WaitForSeconds(1);
                UpdateVRPoints();
                UpdateAnchors();
                Calibrate();
                yield return new WaitForSeconds(1);
                Calibrate();

                yield return new WaitForSeconds(8);
            }

            yield return null;
        }
    }

    private void UpdateAnchors()
    {
        if (SpatialAnchor.FirstAnchorInstance != null)
            _realWorldPointA = SpatialAnchor.FirstAnchorInstance.gameObject;
        if (SpatialAnchor.SecondAnchorInstance != null)
            _realWorldPointB = SpatialAnchor.SecondAnchorInstance.gameObject;
    }

    private void UpdateVRPoints()
    {
        _VRWorldPointA = _calibration.VRCalibrationPointA;
        _VRWorldPointB = _calibration.VRCalibrationPointB;
    }

    void Calibrate()
    {
        Vector3 translation = ((_VRWorldPointA.transform.position + _VRWorldPointB.transform.position) / 2) - ((_realWorldPointA.transform.position + _realWorldPointB.transform.position) / 2);
        this.transform.position += translation;

        Quaternion rotOfset = Quaternion.Euler(new Vector3(0, Quaternion.FromToRotation(
            (_VRWorldPointB.transform.position - _VRWorldPointA.transform.position), (_realWorldPointB.transform.position - _realWorldPointA.transform.position)
            ).eulerAngles.y, 0));

        transform.RotateAround(_realWorldPointA.transform.position, Vector3.up, -rotOfset.eulerAngles.y);
        _checkSound.Play();

        if (_lostDetector != null)
        {
            _lostDetector.RestoreCalibrationSign();
        }
    }
}

