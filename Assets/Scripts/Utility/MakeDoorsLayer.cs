using UnityEngine;

    public class MakeDoorsLayer : MonoBehaviour {
        private void OnValidate() {
            checkDoor(gameObject);
            Utility.DestroyObject(gameObject.GetComponent<MakeDoorsLayer>());
        }

        private void checkDoor(GameObject go) {
            IFCData ifcData = go.GetComponent<IFCData>();
            if (ifcData && ifcData.IFCClass == "IfcDoor") {
                go.layer = Constants.TRAVERSABLE_LAYER;
            }
            foreach (Transform child in go.transform) {
                checkDoor(child.gameObject);
            }
        }
    }