using UnityEngine;
using UnityEditor;
using Unity.AI.Navigation;

using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

public class IfcOpenShellParser : MonoBehaviour {
    [Header("Special Areas")]
    [SerializeField] private string[] walkableAreas = { "IfcSlab", "IfcStair", "IfcStairFlight", "IfcWallStandardCase", "IfcSite" };
    [SerializeField] private string[] navMeshIgnoredAreas = { "IfcDoor" };

    [Header("IFC Signboard Definition")]
    [SerializeField] private string ifcSignTag = "IfcSign";
    [SerializeField] private string ifcSignboardPropertiesName = "Pset_SignboardCommon";
    [SerializeField] private string ifcSignViewingDistanceProperty = "ViewingDistance";
    [SerializeField] private string ifcSignViewingAngleProperty = "ViewingAngle";
    [SerializeField] private string ifcSignMinimumReadingTimeProperty = "MinimumReadingTime";


    [Header("Loader")]
    public bool deleteTemporaryFiles = true;

    private GameObject loadedOBJ;
    private XmlDocument loadedXML;

    private const int NAVMESH_NOT_WALKABLE_AREA_TYPE = 1;
    private const int WALKABLE_LAYER = 9;

    private readonly Dummiesman.OBJLoader objLoader = new() {
        SplitMode = Dummiesman.SplitMode.Object
    };

    public void LoadIFC() {
        string ifcPath = EditorUtility.OpenFilePanel("Import IFC", "", "ifc");
        loadIFC(ifcPath);
    }

    private void loadIFC(string ifcPath) {
        string objPath = Path.ChangeExtension(ifcPath, "obj");
        string mtlPath = Path.ChangeExtension(ifcPath, "mtl");
        string xmlPath = Path.ChangeExtension(ifcPath, "xml");

        Dependencies.IfcConverter.StartConvertAndWait(ifcPath, objPath, xmlPath);

        if(File.Exists(objPath) && File.Exists(mtlPath) && File.Exists(xmlPath)) {
            loadOBJ(objPath, mtlPath);
            LoadXML(xmlPath);
        }
        else {
            Debug.LogError("Error during parse");
        }

        if(this.deleteTemporaryFiles) {
            Utility.DeleteFile(objPath);
            Utility.DeleteFile(mtlPath);
            Utility.DeleteFile(xmlPath);
        }
    }

    public void LoadOBJ_MTL_XMLFile() {
        string objPath = EditorUtility.OpenFilePanel("Import OBJ", "", "obj");
        if(!string.IsNullOrEmpty(objPath)) {
            string mtlPath = EditorUtility.OpenFilePanel("Import MTL", "", "mtl");
            if(!string.IsNullOrEmpty(mtlPath)) {
                string xmlPath = EditorUtility.OpenFilePanel("Import XML", "", "xml");

                if(!string.IsNullOrEmpty(xmlPath)) {
                    loadOBJ(objPath, mtlPath);
                    LoadXML(xmlPath);
                }
            }
        }
    }

    private void loadOBJ(string objPath, string mtlPath) {
        loadedOBJ = objLoader.Load(objPath, mtlPath);

        if(loadedOBJ != null) {
            // turn -90 on the X-Axis (CAD/BIM uses Z up)
            loadedOBJ.transform.Rotate(-90, 0, 0);
        }
    }

    public void LoadXML(string path) {

        loadedXML = new XmlDocument();
        loadedXML.Load(path);

        const string basePath = @"//ifc/decomposition";

        GameObject root = new GameObject {
            name = Path.GetFileNameWithoutExtension(path)
        };

        foreach(XmlNode node in loadedXML.SelectNodes(basePath + "/IfcProject")) {
            addElements(node, root);
        }

        DestroyImmediate(loadedOBJ);

        Debug.Log("Loaded XML");
    }

    private void addElements(XmlNode node, GameObject parent) {
        if(node.Attributes.GetNamedItem("id") != null) {
            string id = node.Attributes.GetNamedItem("id").Value;
            string name = "";
            if(node.Attributes.GetNamedItem("Name") != null) {
                name = node.Attributes.GetNamedItem("Name").Value;
            }
            name += "[" + node.Name + "]";

            string searchPath = /*fileName + "/" +*/ id;
            GameObject goElement = GameObject.Find(searchPath);

            if(goElement == null) {
                goElement = new GameObject();
            }

            if(goElement != null) {

                goElement.name = name;
                if(parent != null) {
                    goElement.transform.SetParent(parent.transform);
                }

                addProperties(node, goElement);
                handleSpecialObjects(ref goElement, node);

                foreach(XmlNode child in node.ChildNodes) {
                    addElements(child, goElement);
                }
            }
        }
    }

    private bool isIfcWalkableArea(string ifcClass) {
        return this.walkableAreas.Contains(ifcClass);
    }

    private bool isIfcIgnoreFromBuildArea(string ifcClass) {
        return this.navMeshIgnoredAreas.Contains(ifcClass);
    }

    private bool isIfcSign(string ifcClass) {
        return ifcClass.Equals(ifcSignTag);
    }

    private void handleSpecialObjects(ref GameObject goElement, XmlNode node) {
        MeshFilter goMeshFilter = goElement.GetComponent<MeshFilter>();
        if(goMeshFilter != null && goMeshFilter.sharedMesh != null) {
            goElement.AddComponent<MeshCollider>();
            if(isIfcIgnoreFromBuildArea(node.Name)) {
                NavMeshModifier navmeshModifier = goElement.AddComponent<NavMeshModifier>();
                navmeshModifier.ignoreFromBuild = true;
            }
            else if(isIfcSign(node.Name)) {
                loadSignboardData(ref goElement);
            }
            else {
                if (isIfcWalkableArea(node.Name)) {
                    goElement.layer = WALKABLE_LAYER;
                }
                else{
                    NavMeshModifier navmeshModifier = goElement.AddComponent<NavMeshModifier>();
                    navmeshModifier.overrideArea = true;
                    navmeshModifier.area = NAVMESH_NOT_WALKABLE_AREA_TYPE;
                }
            }
        }
    }

    private void loadSignboardData(ref GameObject goElement) {
        if(goElement.TryGetComponent<IFCData>(out IFCData signboardIFCData)) {
            List<IFCProperty> signboardProperties = signboardIFCData.propertySets.Find(property => property.propSetName == ifcSignboardPropertiesName).properties;
            if(signboardProperties != null) {
                IFCSignBoard signBoard = goElement.AddComponent<IFCSignBoard>();
                try {
                    signBoard.ViewingDistance = stringToFloat(signboardProperties.Find(property => property.propName == ifcSignViewingDistanceProperty).propValue);
                    signBoard.ViewingAngle = stringToFloat(signboardProperties.Find(property => property.propName == ifcSignViewingAngleProperty).propValue);
                    signBoard.MinimumReadingTime = stringToFloat(signboardProperties.Find(property => property.propName == ifcSignMinimumReadingTimeProperty).propValue);
                }
                catch(Exception e) {
                    Debug.LogError($"Error parsing properties from Signboard {goElement.name} ({e.Message}).");
                }
            }
            else {
                Debug.LogError($"No IFCPropertySet named {ifcSignboardPropertiesName} in Signboard {goElement.name}.");
            }
        }
        else {
            Debug.LogError($"No IFCData in Signboard {goElement.name}.");
        }
    }

    private void addProperties(XmlNode node, GameObject go) {
        IFCData ifcData = go.AddComponent(typeof(IFCData)) as IFCData;

        ifcData.IFCClass = node.Name;
        ifcData.STEPId = node.Attributes.GetNamedItem("id").Value;

        string nameProperty = "";
        if(node.Attributes.GetNamedItem("Name") != null) {
            nameProperty = node.Attributes.GetNamedItem("Name").Value;
        }
        ifcData.STEPName = nameProperty;
        // Initialize PropertySets and QuantitySets
        if(ifcData.propertySets == null)
            ifcData.propertySets = new List<IFCPropertySet>();
        if(ifcData.quantitySets == null)
            ifcData.quantitySets = new List<IFCPropertySet>();


        // Go through Properties (and Quantities and ...)
        foreach(XmlNode child in node.ChildNodes) {
            switch(child.Name) {
                case "IfcPropertySet":
                case "IfcElementQuantity":
                    // we only receive a link beware of # character
                    string link = child.Attributes.GetNamedItem("xlink:href").Value.TrimStart('#');
                    string path = String.Format("//ifc/properties/IfcPropertySet[@id='{0}']", link);
                    if(child.Name == "IfcElementQuantity")
                        path = String.Format("//ifc/quantities/IfcElementQuantity[@id='{0}']", link);
                    XmlNode propertySet = loadedXML.SelectSingleNode(path);
                    if(propertySet != null) {

                        // initialize this propertyset (Name, Id)
                        IFCPropertySet myPropertySet = new IFCPropertySet();
                        myPropertySet.propSetName = propertySet.Attributes.GetNamedItem("Name").Value;
                        myPropertySet.propSetId = propertySet.Attributes.GetNamedItem("id").Value;
                        if(myPropertySet.properties == null)
                            myPropertySet.properties = new List<IFCProperty>();

                        // run through property values
                        foreach(XmlNode property in propertySet.ChildNodes) {
                            string propName, propValue = "";
                            IFCProperty myProp = new IFCProperty();
                            propName = property.Attributes.GetNamedItem("Name").Value;

                            if(property.Name == "IfcPropertySingleValue")
                                propValue = property.Attributes.GetNamedItem("NominalValue").Value;
                            if(property.Name == "IfcQuantityLength")
                                propValue = property.Attributes.GetNamedItem("LengthValue").Value;
                            if(property.Name == "IfcQuantityArea")
                                propValue = property.Attributes.GetNamedItem("AreaValue").Value;
                            if(property.Name == "IfcQuantityVolume")
                                propValue = property.Attributes.GetNamedItem("VolumeValue").Value;
                            // Write property (name & value)
                            myProp.propName = propName;
                            myProp.propValue = propValue;
                            myPropertySet.properties.Add(myProp);
                        }

                        // add propertyset to IFCData
                        if(child.Name == "IfcPropertySet")
                            ifcData.propertySets.Add(myPropertySet);
                        if(child.Name == "IfcElementQuantity")
                            ifcData.quantitySets.Add(myPropertySet);

                    }
                    break;
                default:
                    break;
            }
        }
    }

    private static float stringToFloat(string str) {
        return float.Parse(str, CultureInfo.InvariantCulture.NumberFormat);
    }
}
