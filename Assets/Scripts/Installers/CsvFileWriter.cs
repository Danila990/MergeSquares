using System.IO;
using GoogleSheetsToUnity;
using UnityEngine;

namespace Installers
{
    [CreateAssetMenu(fileName = "CsvFileWriter", menuName = "Installers/CsvFileWriter")]
    public class CsvFileWriter : ScriptableObject
    {
        [SerializeField] private ScriptableInstaller installer;

        public void WriteFiles()
        {
            Debug.Log($"[CsvFileWriter][WriteFiles] Write started!");
            foreach (var repositoryState in installer.CollectRepositoryStates())
            {
                SpreadsheetManager.ReadPublicSpreadsheet(
                    new GSTU_Search(
                        repositoryState.repository.AssociatedSheet,
                        repositoryState.repository.AssociatedWorksheet),
                    (ss) => WriteFile(repositoryState.repository.AssociatedWorksheet, ss));
            }
        }

        private void WriteFile(string fileName, GstuSpreadSheet ss, string firstColumn = "Id")
        {
            Debug.Log($"[CsvFileWriter][WriteFile] Started to write file: {fileName}");
            string filePath = GetPath(fileName);
 
            StreamWriter writer = new StreamWriter(filePath);

            // Use Key for localization repository
            firstColumn = ss.columns.ContainsKey(firstColumn) ? firstColumn : "Key"; 

            foreach (var cell in ss.columns[firstColumn])
            {
                if (cell.value.Trim().Length == 0)
                    break;

                var str = "";
                foreach (var rowCell in ss.rows[cell.value])
                {
                    var value = rowCell.value.Replace(",", ".");
                    str += value + ",";
                }
                writer.WriteLine(str);
            }

            writer.Flush();
            writer.Close();
            Debug.Log($"[CsvFileWriter][WriteFile] Write to file: {fileName} success!");
        }
        
        public static string GetPath(string fileName)
        {
            return Application.dataPath + "/Resources/CsvData/"  + fileName + ".csv";
        }
    }
}
