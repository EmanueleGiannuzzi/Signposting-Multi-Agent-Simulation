using System.IO;

public static class Dependencies {
    public static class IfcConverter {
        private static readonly string IfcConverterExePath = Path.GetFullPath("Plugins/IFC_Converter/IfcConvert.exe");

        private static System.Diagnostics.Process run(string path, bool showConsole = false) {
            return Utility.RunCommand(IfcConverterExePath, path + " --use-element-guids -y", showConsole);
        }

        public static void StartConvertAndWait(string ifcPath, string objPath, string xmlPath) {
            System.Diagnostics.Process processOBJ = run($"\"{ifcPath}\" \"{objPath}\"");
            System.Diagnostics.Process processXML = run($"\"{ifcPath}\" \"{xmlPath}\"");
            
            processOBJ.WaitForExit();
            processXML.WaitForExit();
        }
    }
}
