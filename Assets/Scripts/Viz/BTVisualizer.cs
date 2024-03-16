using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Distributions;
using System.Linq;

using UnityEngine.UI;
using TMPro;

public class BTVisualizer : MonoBehaviour
{
    public GameObject vizLocationPrefab;
    public GameObject deltaPointPrefab;
    public GameObject visualMarkerPrefab;
    public GameObject linePrefab;
    public GameObject lineHandlePrefab;
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

    public float xRange { get { return xlim[1] - xlim[0]; } }
    public float yRange { get { return ylim[1] - ylim[0]; } }

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
    [Header("Axes")]
    public ScaleBar scaleBar;
    public Axes axes;
    [Header("Cross-Section Plotter")]
    public LinePlotter linePlotter;

    private Marker prevHover;

    public bool normalizedScale = false;


    // Interactability fields
    private bool _activeUpdating = false;
    private Marker activeMarker;
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

    // ConstraintBox
    public ConstraintBox constraintBox;

    private bool _panning = false;
    private Vector3 clickPanPos;
    private float orig_camSize;
    private float orig_camZ;

    private double[,] tds;
    private Matrix<double> tempDeltas;

    [Header("Grid Snap")]
    public bool snapMarkersToGrid = false;
    public float gridSpacing = 0.05f;

    private Line workingLine;

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

    public void SetSnapGridSpacing(float newSpacing) {
        gridSpacing = newSpacing;
    }
    public void SetSnapToGrid(bool toThis) {
        snapMarkersToGrid = toThis;
    }

    public void ClearDistribution() {
        foreach (DeltaPoint p in deltaPoints) {
            Destroy(p.gameObject);
        }
        _dirtyDeltaPoints = true;
        deltaPoints.Clear();

        distribution.ClearDeltas();
        distribution.ClearTempDeltas();
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

        tds = new double[,]{
            {0.0, 0.0},
        };
        tempDeltas = Matrix<double>.Build.DenseOfArray(tds);
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
        if (activeMarker != null) { Destroy(activeMarker.gameObject); }
        activeMarker = null;
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
        UpdateVisualElements();
    }

    private void UpdateVisualElements() {
        UpdateAxes();
        UpdateConstraintBox();
    }

    private void UpdateAxes() {
        axes.SetToBounds(new Vector2(width, height));
        axes.UpdateXAxis(xlim);
        axes.UpdateYAxis(ylim);
    }
    public void UpdateConstraintBox() {
        Vector2 origin = DToGSpace(Vector2.zero);
        constraintBox.CreateBox(
            DToGSpace(new Vector2(-constraintBox.bounds.x, constraintBox.bounds.y)),
            DToGSpace(new Vector2(constraintBox.bounds.x, constraintBox.bounds.y)),
            DToGSpace(new Vector2(constraintBox.bounds.x, -constraintBox.bounds.y)),
            DToGSpace(new Vector2(-constraintBox.bounds.x, -constraintBox.bounds.y))
        );
    }

    public Matrix<double> ProbabilityAt(Matrix<double> points) {
        return distribution.Prob(points);
    }

    public void UpdateVizualization() {
        Matrix<double> probabilities = ProbabilityAt(locations);
        if (probabilities == null) { ClearVisualization(); return; }
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
        p.HideDetails();
    }
    public void AddVisualMarkerAt(VisualMarker v) {
        v.HideDetails();
    }
    public void AddLineHandleAt(LineHandle lh) {
        lh.HideDetails();
    }

    private void MakeActiveDeltaPoint() {
        Vector3 point = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(cam.transform.position.z)));
        GameObject newDeltaPoint = Instantiate(deltaPointPrefab, point, Quaternion.identity, transform);
        DeltaPoint dp = newDeltaPoint.GetComponent<DeltaPoint>();
        dp.visualizer = this;
        SetActiveMarker(dp);
    }
    private void MakeActiveVisualMarker() {
        Vector3 point = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(cam.transform.position.z)));
        GameObject newMarker = Instantiate(visualMarkerPrefab, point, Quaternion.identity, transform);
        VisualMarker vm = newMarker.GetComponent<VisualMarker>();
        vm.visualizer = this;
        SetActiveMarker(vm);
    }
    private void MakeActiveLineHandle() {
        if (workingLine == null) { StartNewLine(); }
        // Make new handle
        Vector3 point = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(cam.transform.position.z)));
        GameObject newMarker = Instantiate(lineHandlePrefab, point, Quaternion.identity, workingLine.transform);
        LineHandle lh = newMarker.GetComponent<LineHandle>();
        lh.visualizer = this;
        SetActiveMarker(lh);
        // Apply handle to working line, make a new working line if needed.
        if (workingLine != null) {
            if (workingLine.h1 == null) {
                workingLine.h1 = lh;
            }
            else if (workingLine.h2 == null) {
                workingLine.h2 = lh;
                StartNewLine();
            }
        }
        linePlotter.ShowDetails();
    }

    public void SetActiveMarker(Marker m) {
        activeMarker = m;
    }

    private void ZoomCam(float amount) {
        if (cam.orthographic) {
            cam.orthographicSize = Mathf.Max(1f, cam.orthographicSize + (amount));
        } else {
            cam.transform.Translate(new Vector3(0f, 0f, amount));
        }
    }

    private void StartNewLine() {
        GameObject newLine = Instantiate(linePrefab, transform);
        workingLine = newLine.GetComponent<Line>();
    }

    private void UpdateCrossSection(LineHandle lh) {
        if (lh.line.h1 == null || lh.line.h2 == null) { return; }
        if (lh.line.h1.gSpaceLocation == lh.line.h2.gSpaceLocation) { return; }
        Matrix<double> samplePoints = lh.line.SampleDSpaceMatrix(linePlotter.samples);
        Matrix<double> probabilities = ProbabilityAt(samplePoints);
        if (probabilities == null) { linePlotter.PlotZeroes(); return; }
        double max = probabilities.Row(0).AbsoluteMaximum();
        double min = probabilities.Row(0).AbsoluteMinimum();
        if (!linePlotter.normalizedScale) { 
            if (max == min) { linePlotter.PlotZeroes(); return; }
            probabilities = (probabilities - min) / (max - min);
        }
        float[] values = new float[probabilities.RowCount*probabilities.ColumnCount];
        for (int i = 0; i < probabilities.ColumnCount; i++) {
            values[i] = (float)probabilities.At(0,i);
        }
        // float[] values = probabilities.Storage.ToRowMajorArray().Cast<float>().ToArray();
        float plotterSizeX = Mathf.Max(
            linePlotter.rescale ? linePlotter.sizeRescaleRatio * lh.line.segmentLengthG : linePlotter.minSize.x,
            linePlotter.minSize.x
        );
        linePlotter.rt.sizeDelta = new Vector2(
            plotterSizeX,
            linePlotter.minSize.y
        );
        linePlotter.Plot( values );


        // Plot at offset from handle location
        // linePlotter.transform.position = lh.gSpaceLocation + linePlotter.offset;
        // Plot at an orthogonal offset from midpoint
        // linePlotter.transform.position = lh.line.midpointG + (1* (new Vector2(1f, lh.line.orthoSlopeG)).normalized);
        // Plot in opposite direction from current handle
        Vector2 d = lh.line.DiffToG(lh).normalized;
        linePlotter.transform.position = lh.gSpaceLocation + (1* d);

        if (linePlotter.rescale) {
            // Scale plotter size with length of line
            linePlotter.axes.SetToBounds(new Vector2(plotterSizeX, linePlotter.minSize.y));
        } else {
            // Have set plotter size
            linePlotter.axes.SetToBounds(new Vector2(linePlotter.minSize.x, linePlotter.minSize.y));
        }
        linePlotter.axes.UpdateXAxis(0f, lh.line.segmentLengthD);

        if (!linePlotter.normalizedScale) { 
            linePlotter.axes.UpdateYAxis((float)min, (float)max);
        } else { 
            linePlotter.axes.UpdateYAxis(0f, 1f);
        }
    }

    void Update() {
        // Camera Zooming
        ZoomCam(Input.mouseScrollDelta.y * scrollSpeed);

        // Hover commands
        if (!_activeUpdating) {
        RaycastHit hoverhitInfo;
        Ray hoverray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(hoverray, out hoverhitInfo)) {
            Marker p = hoverhitInfo.transform.GetComponent<Marker>();
            if (p != null) {
                if (prevHover == p) {
                    p.UpdateDetails();
                    LineHandle lh = p as LineHandle;
                    if (lh != null) {
                        UpdateCrossSection(lh);
                    }
                } else {
                    if (prevHover != null) { prevHover.HideDetails(); }
                    prevHover = p;
                    p.ShowDetails();
                    p.UpdateDetails();
                    LineHandle lh = p as LineHandle;
                    if (lh != null) {
                        linePlotter.ShowDetails();
                        UpdateCrossSection(lh);
                    }
                }
            } else {
                if (prevHover != null) {
                    prevHover.HideDetails();
                    LineHandle lh = prevHover as LineHandle;
                    if (lh != null) {
                        linePlotter.HideDetails();
                    }
                    prevHover = null;
                }
            }
        } else {
            if (prevHover != null) {
                prevHover.HideDetails();
                LineHandle lh = prevHover as LineHandle;
                if (lh != null) {
                    linePlotter.HideDetails();
                }
                prevHover = null;
            }
        }
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            _activeUpdating = !_activeUpdating;
            if (!_activeUpdating) { DeselectActiveMarker(); }
            else { MakeActiveDeltaPoint(); }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1)) {
            _activeUpdating = !_activeUpdating;
            if (!_activeUpdating) { DeselectActiveMarker(); }
            else { MakeActiveVisualMarker(); }
        }
        else if (Input.GetKeyDown(KeyCode.Q)) {
            _activeUpdating = false;
            if (!_activeUpdating) { DeselectActiveMarker(); }
            ClearDistribution();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            _activeUpdating = !_activeUpdating;
            if (!_activeUpdating) { DeselectActiveMarker(); }
            else { MakeActiveLineHandle(); }
        }

        if (_activeUpdating) {
            ActiveUpdateMarker(activeMarker);
            if (Input.GetMouseButtonDown(1)) {
                _activeUpdating = false;
                DeselectActiveMarker();
            }
        }
        // Be on the lookout for other iteractions if not updating a point
        else {
            if (Input.GetMouseButtonDown(0)) {
                RaycastHit hitInfo;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hitInfo)) {
                    Marker p = hitInfo.transform.GetComponent<Marker>();
                    if (p != null) {
                        SelectMarker(p);
                    }
                }
            }
        }
    }

    private void DeselectActiveMarker() {
        DeltaPoint p = activeMarker as DeltaPoint;
        if (p != null) {
            distribution.ClearTempDeltas();
            UpdateVizualization();
        }

        LineHandle lh = activeMarker as LineHandle;
        if (lh != null) {
            Line l = lh.line;
            if (l != null) {
                if (l.h1 != null) { Destroy(l.h1.gameObject); }
                if (l.h2 != null) { Destroy(l.h2.gameObject); }
                Destroy(l.gameObject);
            }
            linePlotter.HideDetails();
        }

        if (activeMarker != null) { Destroy(activeMarker.gameObject); }
        activeMarker = null;
    }

    private void SelectMarker(Marker m) {
        if (m == null) { return; }
        SetActiveMarker(m);

        DeltaPoint p = m as DeltaPoint;
        if (p != null) {
            deltaPoints.Remove(p);
            _activeUpdating = true;
            _dirtyDeltaPoints = true;
            distribution.SetDeltas(setDeltaPointLocations);
            p.ShowDetails();
        }

        VisualMarker v = m as VisualMarker;
        if (v != null) {
            _activeUpdating = true;
            v.ShowDetails();
        }

        LineHandle lh = m as LineHandle;
        if (lh != null) {
            _activeUpdating = true;
            lh.ShowDetails();
            linePlotter.ShowDetails();
        }
    }

    private void ActiveUpdateMarker(Marker m) {
        if (m == null) { return; }

        DeltaPoint p = m as DeltaPoint;
        if (p != null) {
            // Update active delta point position
            UpdateMarkerPosition(p);
            // Update the delta
            Vector2 activeLoc = p.dSpaceLocation;
            tempDeltas.At(0,0, activeLoc.x);
            tempDeltas.At(0,1, activeLoc.y);
            distribution.SetTempDeltas(tempDeltas);

            // tds[0,0] = activeLoc.x;
            // tds[0,1] = activeLoc.y;
            // distribution.OverwriteTempDeltas(tds);
            
            UpdateVizualization();
            p.UpdateDetails();

            if (Input.GetMouseButtonDown(0)) {
                AddDeltaPointAt(p);
                MakeActiveDeltaPoint();
            }
        }
        
        VisualMarker v = m as VisualMarker;
        if (v != null) {
            // Update active delta point position
            UpdateMarkerPosition(v);
            v.UpdateDetails();

            if (Input.GetMouseButtonDown(0)) {
                AddVisualMarkerAt(v);
                MakeActiveVisualMarker();
            }
        }

        LineHandle lh = m as LineHandle;
        if (lh != null) {
            // Update active delta point position
            UpdateMarkerPosition(lh);
            lh.UpdateDetails();
            UpdateCrossSection(lh);

            if (Input.GetMouseButtonDown(0)) {
                AddLineHandleAt(lh);
                MakeActiveLineHandle();
            }
        }
    }

    private void UpdateMarkerPosition(Marker m) {
        if (snapMarkersToGrid) {
            Vector3 gSpaceMousePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(cam.transform.position.z)));
            Vector2 dSpaceMousePos = GToDSpace(gSpaceMousePos);
            // Round in d-space
            Vector2 unitSpacingVector = dSpaceMousePos / gridSpacing;
            Vector2 snappedGPos = DToGSpace(new Vector2(
                gridSpacing*Mathf.Round(unitSpacingVector.x),
                gridSpacing*Mathf.Round(unitSpacingVector.y)
            ));
            m.transform.position = new Vector3(snappedGPos.x, snappedGPos.y, 0f);
        } else {
            m.transform.position = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(cam.transform.position.z)));
        }
    }
}
