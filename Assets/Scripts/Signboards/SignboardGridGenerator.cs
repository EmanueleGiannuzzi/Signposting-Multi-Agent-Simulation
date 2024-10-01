using UnityEngine;

public class SignboardGridGenerator : MonoBehaviour {
    public IFCSignBoard SignboardTemplate;
    [Tooltip("point/meter")]
    public float resolution = 0.5f;

    public float signboardHeight = 2f;
    public float signboardOrientation = -90f;

    private GameObject signboardGridGroup;// The children of this object are the signboards generated

    public GameObject GetSignboardGridGroup() {
        return signboardGridGroup;
    }

    private static VisibilityPlaneData[] getVisibilityPlanes() {
        return VisibilityPlaneHelper.GetVisibilityPlanes();
    }

    public void DeleteObjects() {
        Utility.DeleteGroup(Constants.SIGNBOARD_GRID_GROUP_TAG);
    }

    public void GenerateGrid() {
        DeleteObjects();
        signboardGridGroup = Utility.CreateNewGroup(Constants.SIGNBOARD_GRID_GROUP_TAG);
        GenerateGrid(signboardGridGroup);
    }

    private static void generateSignboardBack(GameObject signboardObj, Material signboardBackMaterial) {
        Transform parentTransform = signboardObj.transform;
        
        GameObject signboardBackObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        signboardBackObj.name = signboardObj.name + "(Mirror)";
        signboardBackObj.transform.rotation = Quaternion.AngleAxis(180f, Vector3.up) * parentTransform.rotation;
        signboardBackObj.transform.position = parentTransform.position;
        signboardBackObj.transform.localScale = parentTransform.localScale;
        signboardBackObj.transform.parent = parentTransform;

        Utility.DestroyObject(signboardBackObj.GetComponent<Collider>());
        signboardBackObj.GetComponent<MeshRenderer>().sharedMaterial = signboardBackMaterial;
    }

    private void generateMainSignboard(int gridX, int gridZ, Color signboardColor, Vector3 position, float sideWidth, float sideHeight, GameObject visPlaneParent, Material signboardBackMaterial) {
        GameObject signboardObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        signboardObj.tag = Constants.SIGNBOARDS_TAG;
        Utility.DestroyObject(signboardObj.GetComponent<Collider>());
        signboardObj.name = "Signboard [" + gridX + ", " + gridZ + "]";
        position.x -= sideWidth / 2;
        position.z -= sideHeight / 2;
        signboardObj.transform.position = position;
        signboardObj.transform.rotation = Quaternion.Euler(-90f, 0f, signboardOrientation);
        signboardObj.transform.localScale = new Vector3(-0.08f, 1f, 0.08f);
        signboardObj.transform.parent = visPlaneParent.transform;

        IFCSignBoard signboard = signboardObj.AddComponent<IFCSignBoard>();
        signboardObj.AddComponent<SignboardVisibility>();
        signboard.CopyDataFrom(SignboardTemplate);
        signboard.Color = signboardColor;

        MeshRenderer signboardRenderer = signboardObj.GetComponent<MeshRenderer>();
        Material tempMaterial = new Material(signboardRenderer.sharedMaterial) {
            color = signboardColor
        };
        signboardRenderer.sharedMaterial = tempMaterial;

        generateSignboardBack(signboardObj, signboardBackMaterial);

        signboardObj.AddComponent<SignboardGrid>();
        SignboardGrid signboardGrid = signboardObj.GetComponent<SignboardGrid>();
        signboardGrid.planeLocalIndex = new Vector2Int(gridX, gridZ);
    }

    public void GenerateGrid(GameObject parent) {
        VisibilityPlaneData[] visibilityPlanes = getVisibilityPlanes();
        int visPlaneSize = visibilityPlanes.Length;
        
        for(int visPlaneId = 0; visPlaneId < visPlaneSize; visPlaneId++) {
            VisibilityPlaneData visibilityPlane = visibilityPlanes[visPlaneId];

            GameObject visPlaneParent = new GameObject(visibilityPlane.name) {
                transform = {
                    parent = parent.transform
                }
            };

            float visibilityPlaneHeight = visibilityPlane.OriginalFloorHeight;

            Bounds meshRendererBounds = visibilityPlane.GetComponent<MeshRenderer>().bounds;
            float planeWidth = meshRendererBounds.extents.x * 2;
            float planeHeight = meshRendererBounds.extents.z * 2;
            int widthResolution = (int)Mathf.Floor(planeWidth * this.resolution);
            int heightResolution = (int)Mathf.Floor(planeHeight * this.resolution);

            Vector3 cornerMax = meshRendererBounds.max;

            Material signboardMaterial = new Material(Shader.Find("Standard")) {
                color = Color.gray
            };

            for(int z = 0; z < heightResolution; z++) {
                for(int x = 0; x < widthResolution; x++) {
                    float sideWidth = planeWidth / widthResolution;
                    float sideHeight = planeHeight / heightResolution;
                    Vector3 position = new Vector3(cornerMax.x - (sideWidth * x), visibilityPlaneHeight + signboardHeight, cornerMax.z - (sideHeight * z));
                    if(Utility.HorizontalPlaneContainsPoint(visibilityPlane.GetComponent<MeshFilter>().sharedMesh, visibilityPlane.transform.InverseTransformPoint(position), (planeWidth / widthResolution), (planeHeight / heightResolution))) {
                        Color signboardColor = new Color(
                            Random.Range(0f, 1f),
                            Random.Range(0f, 1f),
                            Random.Range(0f, 1f)
                        );
                        generateMainSignboard(x, z, signboardColor, position, sideWidth, sideHeight, visPlaneParent, signboardMaterial);
                    }
                }
            }
        }
    }
}
