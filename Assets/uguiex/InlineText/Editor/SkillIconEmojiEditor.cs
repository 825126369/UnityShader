using CSharpJExcel.Jxl;
using CSharpJExcel.Jxl.Write;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SkillIconEmojiEditor{

    private static string _exlFoldPath = "../../tools/ResConvert/Excel/";

    [MenuItem("EmojiText/SkillIconForEmoji")]
    static void GetSkillInfo()
    {

        List<string> resIds = new List<string>();

        Dictionary<string, string> resPathDict = new Dictionary<string, string>();

        string excelDir = _exlFoldPath + "Skill" + ".xls";
        FileInfo info = new FileInfo(excelDir);

        WorkbookSettings ws = new WorkbookSettings();
        ws.setEncoding("UTF8");

        Workbook book = Workbook.getWorkbook(info);

        Sheet sheet = book.getSheet("#skill_cfg");
        Cell[] cells = sheet.getColumn(7);
       
        for(int i = 5; i < cells.Length;i++ )
        {
            string content = cells[i].getContents();
            content = content.Trim();
            if (content.Length > 0)
            {
                resIds.Add(content);
            }
        }
        cells = null;
        sheet = null;
        ws = null;

        info = null;

        excelDir = _exlFoldPath + "ResPathConfig" + ".xls";
        info = new FileInfo(excelDir);

        ws = new WorkbookSettings();
        ws.setEncoding("UTF8");

        book = Workbook.getWorkbook(info);

        sheet = book.getSheet("#res_path_cfg");
        cells = sheet.getColumn(0);

        for (int i = 5; i < cells.Length; i++)
        {
            string content = cells[i].getContents();
            content = content.Trim();
            if (content.Length > 0)
            {
                for( int j = 0; j < resIds.Count;j++)
                {
                    string id = resIds[j];
                    if (id.Equals(content))
                    {
                        Cell cell = sheet.getCell(1, i);
                        string path = cell.getContents();
                        
                        string inputPath = "Assets/" + path;
                        string outputPath = "Assets/arts/gui/tex_all/emoji_tex/skill_input/" + id + ".png";
                        Object obj = AssetDatabase.LoadAssetAtPath<Texture>(inputPath);
                        Object targetObj = AssetDatabase.LoadAssetAtPath<Texture>(outputPath);
                        if (obj != null && targetObj == null)
                        {
                            //Debug.Log("path :" + id + " " + path);
                            AssetDatabase.CopyAsset(inputPath, outputPath);
                        }
                        
                    }
                }
            }
        }

        cells = null;
        sheet = null;
        ws = null;
        info = null;

    }
}
