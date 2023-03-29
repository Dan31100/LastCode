using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrationPassthroughState : MonoBehaviour
{
    [SerializeField] private OVRPassthroughLayer _ovrPassthroughLayer;

    public void SetPassthroughHiddenState(bool state)
    {
        _ovrPassthroughLayer.hidden = state;
        SetAllAnchorsVisibility(!state);
    }

    public bool GetPassthroughHiddenState()
    {
        return _ovrPassthroughLayer.hidden;
    }

    private void SetAllAnchorsVisibility(bool state)
    {
        if (SpatialAnchor.FirstAnchorInstance)
            SpatialAnchor.FirstAnchorInstance.ChangeMeshVisibility(state);

        if (SpatialAnchor.SecondAnchorInstance)
            SpatialAnchor.SecondAnchorInstance.ChangeMeshVisibility(state);
    }
}
