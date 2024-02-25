using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Distributions;

using UnityEngine.UI;
using TMPro;

public class BTVisualizer : MonoBehaviour
{
    public GameObject vizLocationPrefab;
    public GameObject deltaPointPrefab;
    public Vector2 xlim = new Vector2(-1f, 1f);
    public Vector2 ylim = new Vector2(-1f, 1f);
    public float step = 0.1f;
    private int numFeatures;
    public Vector2 xBounds = new Vector2(-4f, 4f);
    public Vector2 yBounds = new Vector2(-4f, 4f);
    public float width { get {return xBounds[1] - xBounds[0]; } }
    public float height { get {return yBounds[1] - yBounds[0]; } }

    public Vector2 dRange { get { return new Vector2(xRange, yRange); }}
    public Vector2 gRange { get { return new Vector2(width, height); }}
    // Values that scale world space to 
    // public float xScale { get {return (width/xRange); } }
    // public float yScale { get {return (height/yRange); } }
    // Ratios to get from 1 unit of delta-space to x units of global space
    public Vector2 dgRatio { get {return new Vector2(gRange.x/dRange.x, gRange.y/dRange.y); }}
    // Ratios to get from 1 unit of global-space to x units of delta space
    public Vector2 gdRatio { get {return new Vector2(dRange.x/gRange.x, dRange.y/gRange.y); }}
    // Values that represent 

    private BradleyTerryDistribution distribution;
    private Matrix<double> locations;    // [# locations x # features]
    private VizLocation[,] vizLocs;

    private float xRange { get { return xlim[1] - xlim[0]; } }
    private float yRange { get { return ylim[1] - ylim[0]; } }

    private int xSteps { get { return (int)Mathf.Round(xRange / step); } }
    private int ySteps { get { return (int)Mathf.Round(yRange / step); } }

    private Vector2 dTemp = Vector2.right;
    [Range(0.1f, 5f)]
    public float mag = 1f;
    public float speed = 5f;

    private Matrix<double> defDeltas;

    public float scrollSpeed = 1f;
    private Camera cam;

    // Visual Elements
    public ScaleBar scaleBar;
    public Axes axes;

    private DeltaPoint prevHover;

    public bool normalizedScale = false;


    // Interactability fields
    private bool _activeUpdating = false;
    private DeltaPoint activeDeltaPoint;
    public List<DeltaPoint> deltaPoints;
    private bool _dirtyDeltaPoints = true;
    private Matrix<double> _setDeltaPointLocations;
    private Matrix<double> setDeltaPointLocations {
        get {
            if (_dirtyDeltaPoints) {
                double[,] locs = new double[deltaPoints.Count, 2];
                for (int i = 0; i < deltaPoints.Count; i++) {
                    Vector2 l = deltaPoints[i].dSpaceLocation;
                    locs[i,0] = l.x;
                    locs[i,1] = l.y;
                }
                _setDeltaPointLocations = Matrix<double>.Build.DenseOfArray( locs );
                _dirtyDeltaPoints = false;
            }
            return _setDeltaPointLocations;
        }
    }

    public Slider xlimSlider;
    public Slider ylimSlider;
    public Slider stepSlider;
    private Vector2 orig_xlim = new Vector2(-1f, 1f);
    private Vector2 orig_ylim = new Vector2(-1f, 1f);
    private float orig_step = 0.1f;

    private bool _panning = false;
    private Vector3 clickPanPos;
    private float orig_camSize;
    private float orig_camZ;

    public void RecenterCamera() {
        cam.transform.position = new Vector3(0f, 0f, orig_camZ);
        if (cam.orthographic) {
            cam.orthographicSize = orig_camSize;
        }
    }
    public void ResetSettings() {
        xlim = orig_xlim;
        ylim = orig_ylim;
        step = orig_step;
        if (xlimSlider != null) { xlimSlider.value = xlim[1]; }
        if (ylimSlider != null) { ylimSlider.value = ylim[1]; }
        if (stepSlider != null) { stepSlider.value = step; }
    }
    public void SetXLim(float newVal) {
        xlim = new Vector2(-newVal, newVal);
    }
    public void SetYLim(float newVal) {
        ylim = new Vector2(-newVal, newVal);
    }
    public void SetStepSize(float newVal) {
        step = newVal;
    }

    public void SetNormalizedScale(bool normalized) {
        normalizedScale = normalized;
        scaleBar.normalized = normalizedScale;
        UpdateVizualization();
    }
    public void ToggleNormalizedScale() {
        normalizedScale = !normalizedScale;
        scaleBar.normalized = normalizedScale;
        UpdateVizualization();
    }

    // Convert world point to point in delta space
    public Vector2 GToDSpace(Vector2 worldPoint) {
        return new Vector2(
            (worldPoint.x*(gdRatio.x)) + (xlim[0] + (xRange/2f)),
            (worldPoint.y*(gdRatio.y)) + (ylim[0] + (yRange/2f))
        );
    }
    public Vector2 DToGSpace(Vector2 deltaPoint) {
        return new Vector2(
            (deltaPoint.x*(dgRatio.x)) + (xBounds[0] + (width/2f)),
            (deltaPoint.y*(dgRatio.y)) + (yBounds[0] + (height/2f))
        );
    }

    void Awake() {
        if (deltaPoints == null) { deltaPoints = new List<DeltaPoint>(); }
        orig_xlim = xlim;
        orig_ylim = ylim;
        orig_step = step;

        cam = Camera.main;
        orig_camZ = cam.transform.position.z;
        if (cam.orthographic) {
            orig_camSize = cam.orthographicSize;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        numFeatures = 2;
        distribution = new BradleyTerryDistribution(numFeatures);

        BuildField();
        UpdateVizualization();
    }

    public void RebuildField() {
        DestroyField();
        BuildField();
        _dirtyDeltaPoints = true;
        distribution.ClearDeltas();
        distribution.SetDeltas(setDeltaPointLocations);

        _activeUpdating = false;
        if (activeDeltaPoint != null) { Destroy(activeDeltaPoint.gameObject); }
        activeDeltaPoint = null;
        distribution.ClearTempDeltas();

        UpdateVizualization();
    }

    private void DestroyField() {
        for (int x = 0; x < vizLocs.GetLength(0); x++) {
            for (int y = 0; y < vizLocs.GetLength(1); y++) {
                Destroy(vizLocs[x,y].gameObject);
            }
        }
    }

    public void BuildField() {
        // Set up locational sampling points:
        double[,] data = new double[xSteps*ySteps, numFeatures];
        int location = 0;
        // Start at xlim[0],ylim[0] and going across columns (build up from bottom-left)
        for (int x = 0; x < xSteps; x++) {
            for (int y = 0; y < ySteps; y++) {
                // (x value, y value)
                data[location,0] = xlim[0] + ((x*step) + (step/2));
                data[location,1] = ylim[0] + ((y*step) + (step/2));
                location += 1;
            }
        }
        locations = Matrix<double>.Build.DenseOfArray(data);

        vizLocs = new VizLocation[xSteps, ySteps];
        location = 0;
        float xRatio = dgRatio.x;
        float yRatio = dgRatio.y;
        for (int x = 0; x < xSteps; x++) {
            for (int y = 0; y < ySteps; y++) {
                Vector3 newLoc = new Vector3(xRatio*step*(0.5f + x - (xSteps/2f)), yRatio*step*(0.5f + y - (ySteps/2f)), 1f);
                GameObject newTile = Instantiate(vizLocationPrefab, newLoc, Quaternion.identity, transform);
                // (x value, y value)
                VizLocation vl = newTile.GetComponent<VizLocation>();
                vl.SetSize(new Vector3(xRatio*step, yRatio*step, 1f));
                vizLocs[x,y] = vl;
                location += 1;
            }
        }
        UpdateAxes();
    }
    private void UpdateAxes() {
        axes.SetToBounds(new Vector2(width, height));
        axes.UpdateXAxis(xlim);
        axes.UpdateYAxis(ylim);
    }

    public void UpdateVizualization() {
        Matrix<double> probabilities = distribution.Prob(locations);
        double max = probabilities.Row(0).AbsoluteMaximum();
        double min = probabilities.Row(0).AbsoluteMinimum();
        // Debug.Log($"Minprob: {min}\nMaxprob: {max}");
        if (max == min) { ClearVisualization(); return; }
        if (!normalizedScale) { probabilities = (probabilities - min) / (max - min); }
        int location = 0;
        for (int x = 0; x < xSteps; x++) {
            for (int y = 0; y < ySteps; y++) {
                vizLocs[x,y].SetIntensity(probabilities.At(location, 0));
                location += 1;
            }
        }
        scaleBar.UpdateScale(vizLocs[0,0].gradient, min, max);
    }
    private void ClearVisualization() {
        int location = 0;
        for (int x = 0; x < xSteps; x++) {
            for (int y = 0; y < ySteps; y++) {
                vizLocs[x,y].SetIntensity(0);
                location += 1;
            }
        }
        scaleBar.UpdateScale(vizLocs[0,0].gradient, 0, 0);
    }

    public void AddDeltaPointAt(DeltaPoint p) {
        _dirtyDeltaPoints = true;
        deltaPoints.Add(p);
        distribution.SetDeltas(setDeltaPointLocations);
    }

    private void MakeActiveDeltaPoint() {
        Vector3 point = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(cam.transform.position.z)));
        GameObject newDeltaPoint = Instantiate(deltaPointPrefab, point, Quaternion.identity, transform);
        DeltaPoint dp = newDeltaPoint.GetComponent<DeltaPoint>();
        dp.visualizer = this;
        SetActiveDeltaPoint(dp);
    }
    public void SetActiveDeltaPoint(DeltaPoint p) {
        activeDeltaPoint = p;
    }

    private void ZoomCam(float amount) {
        if (cam.orthographic) {
            cam.orthographicSize = Mathf.Max(1f, cam.orthographicSize + (amount));
        } else {
            cam.transform.Translate(new Vector3(0f, 0f, amount));
        }
    }

    void Update() {
        // dTemp = mag * dTemp.normalized;
        // dTemp = Quaternion.AngleAxis(Time.deltaTime*speed, Vector3.forward) * dTemp;
        // distribution.ClearDeltas();
        // Matrix<double> testDeltas = Matrix<double>.Build.DenseOfArray(new double[,]{
        //     {dTemp.x, dTemp.y},
        // });
        // distribution.AddDeltas(defDeltas);
        // distribution.AddDeltas(testDeltas);
        // UpdateVizualization();

        // Camera cam = Camera.main;

        // Vector3 point = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(cam.transform.position.z)));
        // dTemp = GToDSpace(new Vector2(point.x, point.y));
        // Debug.Log($"MousePosition: {Input.mousePosition}\nWorld Position: {point}\nDelta Position: {dTemp}");
        // distribution.ClearDeltas();
        // Matrix<double> testDeltas = Matrix<double>.Build.DenseOfArray(new double[,]{
        //     {dTemp.x, dTemp.y},
        // });
        // distribution.AddDeltas(defDeltas);
        // distribution.AddDeltas(testDeltas);
        // UpdateVizualization();

        ZoomCam(Input.mouseScrollDelta.y * scrollSpeed);

        // Right click
        // if (Input.GetMouseButtonDown(1)) {
        //     _panning = false;
        //     clickPanPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(cam.transform.position.z)));
        // } else if (Input.GetMouseButtonUp(1)) {
        //     _panning = false;
        // }
        // if (_panning) {
        //     clickPanPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(cam.transform.position.z)));
        //     cam.transform.position = 
        // }

        // Hover commands
        RaycastHit hoverhitInfo;
        Ray hoverray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(hoverray, out hoverhitInfo)) {
            DeltaPoint p = hoverhitInfo.transform.GetComponent<DeltaPoint>();
            if (p != null) {
                if (prevHover == p) {
                    p.UpdateDetails();
                }
                else {
                    if (prevHover != null) { prevHover.HideDetails(); }
                    prevHover = p;
                    p.ShowDetails();
                    p.UpdateDetails();
                }
            } else {
                if (prevHover != null) {
                    prevHover.HideDetails();
                    prevHover = null;
                }
            }
        } else {
            if (prevHover != null) {
                prevHover.HideDetails();
                prevHover = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            _activeUpdating = !_activeUpdating;
            if (!_activeUpdating) {
                if (activeDeltaPoint != null) { Destroy(activeDeltaPoint.gameObject); }
                activeDeltaPoint = null;
                distribution.ClearTempDeltas();
                UpdateVizualization();
            }
            else {
                MakeActiveDeltaPoint();
            }
        }
        if (_activeUpdating) {
            if (activeDeltaPoint == null) {return;}
            // Update active delta point position
            activeDeltaPoint.transform.position = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(cam.transform.position.z)));
            // Update the delta
            Vector2 activeLoc = activeDeltaPoint.dSpaceLocation;
            Matrix<double> tempDeltas = Matrix<double>.Build.DenseOfArray(new double[,]{
                {activeLoc.x, activeLoc.y},
            });
            distribution.SetTempDeltas(tempDeltas);
            UpdateVizualization();

            if (Input.GetMouseButtonDown(0)) {
                AddDeltaPointAt(activeDeltaPoint);
                MakeActiveDeltaPoint();
            }
        }
        // Be on the lookout for other iteractions if not updating a point
        else {
            if (Input.GetMouseButtonDown(0)) {
                RaycastHit hitInfo;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hitInfo)) {
                    DeltaPoint p = hitInfo.transform.GetComponent<DeltaPoint>();
                    if (p != null) {
                        deltaPoints.Remove(p);
                        SetActiveDeltaPoint(p);
                        _activeUpdating = true;
                        _dirtyDeltaPoints = true;
                        distribution.SetDeltas(setDeltaPointLocations);
                    }
                }
            }
        }
    }
}
