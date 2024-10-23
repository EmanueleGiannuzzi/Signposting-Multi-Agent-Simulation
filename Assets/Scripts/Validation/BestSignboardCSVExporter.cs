using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEngine;

public class BestSignboardCSVExporter : MonoBehaviour {
    private VisibilityHandler visibilityHandler;
    private int agentTypeSize;
    
    [Header("Output")]
    [SerializeField] private string testName = "";
    [SerializeField] private string outputFilePath = "";
    
    private void Awake() {
        visibilityHandler = FindObjectOfType<VisibilityHandler>();
        if(!visibilityHandler) {
            throw new MissingReferenceException($"No {nameof(VisibilityHandler)} found!");
        }
    }

    private void Start() {
        agentTypeSize = visibilityHandler.agentTypes.Length;
    }

    private List<SignboardData> createSignboardsData() {
        List<SignboardData> signboardData = new List<SignboardData>();
        IFCSignBoard[] signboards = SignboardHelper.GetIFCSignboards();
        
        foreach(IFCSignBoard signboard in signboards) {
            SignboardVisibility signboardVisibility = signboard.GetComponent<SignboardVisibility>();
            string name = signboard.name + "(" + signboard.transform.parent.name + ")";
            float[] coverage = new float[agentTypeSize];
            float[] visibility = new float[agentTypeSize];

            for(int i = 0; i < agentTypeSize; i++) {
                if (i < signboardVisibility.CoveragePerAgentType.Length) {
                    coverage[i] = signboardVisibility.CoveragePerAgentType[i];
                }

                if (i < signboardVisibility.VisibilityPerAgentType.Length) {
                    visibility[i] = signboardVisibility.VisibilityPerAgentType[i];
                }
            }
            signboardData.Add(new SignboardData(name, coverage, visibility));
        }
        return signboardData;
    }

    private float normalize(float val, float valMin, float valMax, float min, float max) {
        return (val - valMin) / (valMax - valMin) * (max - min) + min;
    }
    
    private List<SignboardData> normalizeSignboardData(List<SignboardData> signboardData) {
        if (signboardData.Count <= 0) {
            return signboardData;
        }

        const float MIN_NORMALIZED = 0f;
        const float MAX_NORMALIZED = 1f;

        int agentTypeSize = signboardData[0].AgentTypeSize;
            
        for (int i = 0; i < agentTypeSize; i++) {
            float minCoverage = float.MaxValue;
            float maxCoverage = -float.MaxValue;
            float minVisibility = float.MaxValue;
            float maxVisibility = -float.MaxValue;
            foreach (SignboardData signboard in signboardData) {
                minCoverage = Mathf.Min(signboard.Coverage[i], minCoverage);
                maxCoverage = Mathf.Max(signboard.Coverage[i], maxCoverage);
                minVisibility = Mathf.Min(signboard.Coverage[i], minVisibility);
                maxVisibility = Mathf.Max(signboard.Coverage[i], maxVisibility);
            }

            foreach (SignboardData signboard in signboardData) {
                signboard.Coverage[i] = normalize(signboard.Coverage[i], minCoverage, maxCoverage, MIN_NORMALIZED,
                    MAX_NORMALIZED);
                signboard.Visibility[i] = normalize(signboard.Visibility[i], minVisibility, maxVisibility, MIN_NORMALIZED,
                    MAX_NORMALIZED);
            }
        }
        return signboardData;
    }

    public void ExportCSV() {
        Directory.CreateDirectory(outputFilePath);
        string fileName = $"{testName}-VisibilityData.csv";
        string filePath = Path.Combine(outputFilePath, Path.GetFileName(fileName));
        using StreamWriter writer = new StreamWriter(filePath, false);
        CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture) {
            HasHeaderRecord = false
        };
        using CsvWriter csv = new CsvWriter(writer, config);
        csv.Context.RegisterClassMap(new SignboardDataMap());

        csv.WriteField("sep=" + csv.Configuration.Delimiter, false);
        csv.NextRecord();
        string header = "Name";
        for(int i = 0; i < visibilityHandler.agentTypes.Length; i++) {
            header += csv.Configuration.Delimiter + "Coverage[" + visibilityHandler.agentTypes[i].Key + "]";
        }
        for(int i = 0; i < visibilityHandler.agentTypes.Length; i++) {
            header += csv.Configuration.Delimiter + "Visibility[" + visibilityHandler.agentTypes[i].Key + "]";
        }
        csv.WriteField(header, false);
        csv.NextRecord();
        List<SignboardData> signboardData = createSignboardsData();
        List<SignboardData> normalizedData = normalizeSignboardData(signboardData);
        csv.WriteRecords(normalizedData);
    }

    private class SignboardData {
        public readonly string SignboardName;
        public readonly float[] Coverage;
        public readonly float[] Visibility;
        
        public int AgentTypeSize => Coverage.Length;

        public SignboardData(string signboardName, float[] coverage, float[] visibility) {
            this.SignboardName = signboardName;
            this.Coverage = coverage;
            this.Visibility = visibility;
        }

    }

    private sealed class SignboardDataMap : ClassMap<SignboardData> {
        public SignboardDataMap() {
            Map(m => m.SignboardName).Name("Name");
            Map(m => m.Coverage).Name("Coverage");
            Map(m => m.Visibility).Name("Visibility");
        }
    }
    
}
