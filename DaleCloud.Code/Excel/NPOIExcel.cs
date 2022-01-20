/*******************************************************************************
 * Copyright © 2016 NFine.Framework 版权所有
 * Author: DaleCloud
 * Description: DaleCloud快速开发平台
 * Website：
*********************************************************************************/
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.IO;
using System.Web;

namespace DaleCloud.Code.Excel
{
    public class NPOIExcel
    {
        private string _title;
        private string _sheetName;
        private string _filePath;
        private int errorLineNo { get; set; }
        /// <summary>
        /// 导出到Excel
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool ToExcel(DataTable table)
        {
            FileStream fs = new FileStream(this._filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            IWorkbook workBook = new HSSFWorkbook();
            this._sheetName = this._sheetName.IsEmpty() ? "sheet1" : this._sheetName;
            ISheet sheet = workBook.CreateSheet(this._sheetName);

            //处理表格标题
            IRow row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue(this._title);
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, table.Columns.Count - 1));
            row.Height = 500;

            ICellStyle cellStyle = workBook.CreateCellStyle();
            IFont font = workBook.CreateFont();
            font.FontName = "微软雅黑";
            font.FontHeightInPoints = 17;
            cellStyle.SetFont(font);
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.Alignment = HorizontalAlignment.Center;
            row.Cells[0].CellStyle = cellStyle;

            //处理表格列头
            row = sheet.CreateRow(1);
            for (int i = 0; i < table.Columns.Count; i++)
            {
                row.CreateCell(i).SetCellValue(table.Columns[i].ColumnName);
                row.Height = 350;
                sheet.AutoSizeColumn(i);
            }

            //处理数据内容
            for (int i = 0; i < table.Rows.Count; i++)
            {
                row = sheet.CreateRow(2 + i);
                row.Height = 250;
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    row.CreateCell(j).SetCellValue(table.Rows[i][j].ToString());
                    sheet.SetColumnWidth(j, 256 * 15);
                }
            }

            //写入数据流
            workBook.Write(fs);
            fs.Flush();
            fs.Close();

            return true;
        }

        /// <summary>
        /// 导出到Excel
        /// </summary>
        /// <param name="table"></param>
        /// <param name="title"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public bool ToExcel(DataTable table, string title, string sheetName, string filePath)
        {
            this._title = title;
            this._sheetName = sheetName;
            this._filePath = filePath;
            return ToExcel(table);
        }



        /// <summary>
        /// 将excel中的数据导入到DataTable中
        /// </summary>
        /// <param name="sheetName">excel工作薄sheet的名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns>返回的DataTable</returns>
        public DataTable ExcelToDataTable(HttpPostedFile file, string sheetName, bool isFirstRowColumn)
        {
            ISheet sheet = null;
            IWorkbook workbook = null;
            DataTable data = new DataTable();
            int startRow = 0;
            try
            {
                file.InputStream.Position = 0;
                if (file.FileName.IndexOf(".xlsx") > 0) // 2007版本

                    workbook = new XSSFWorkbook(file.InputStream);
                else if (file.FileName.IndexOf(".xls") > 0) // 2003版本
                    workbook = new HSSFWorkbook(file.InputStream);

                if (sheetName != null)
                {
                    sheet = workbook.GetSheet(sheetName);
                    if (sheet == null) //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                    {
                        sheet = workbook.GetSheetAt(0);
                    }
                }
                else
                {
                    sheet = workbook.GetSheetAt(0);
                }
                if (sheet != null)
                {
                    IRow firstRow = sheet.GetRow(0);
                    int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数

                    if (isFirstRowColumn)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            NPOI.SS.UserModel.ICell cell = firstRow.GetCell(i);
                            if (cell != null)
                            {
                                string cellValue = cell.StringCellValue;
                                if (cellValue != null)
                                {
                                    DataColumn column = new DataColumn(cellValue);
                                    data.Columns.Add(column);
                                }
                            }
                            else
                            {
                                DataColumn column = new DataColumn(string.Empty);
                                data.Columns.Add(column);
                            }
                        }
                        startRow = sheet.FirstRowNum + 1;
                    }
                    else
                    {
                        startRow = sheet.FirstRowNum;
                    }

                    //最后一列的标号
                    int rowCount = sheet.LastRowNum;

                    for (int i = startRow; i <= rowCount; ++i)
                    {
                        errorLineNo = i;

                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; //没有数据的行默认是null　　　　　　　
                        bool isEmpty = true;
                        DataRow dataRow = data.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            if (row.GetCell(j) != null) //同理，没有数据的单元格都默认是null
                            {
                                if (row.GetCell(j).CellType == CellType.Numeric)
                                {
                                    if (DateUtil.IsCellDateFormatted(row.GetCell(j)))
                                    {
                                        dataRow[j] = row.GetCell(j).DateCellValue.ToString("yyyy-MM-dd");
                                    }
                                    else
                                    {
                                        dataRow[j] = row.GetCell(j).NumericCellValue.ToString();
                                    }

                                }
                                else
                                {
                                    dataRow[j] = row.GetCell(j).StringCellValue.ToString();
                                }
                                // dataRow[j] = row.GetCell(j).StringCellValue.ToString();

                            }
                            if (!string.IsNullOrWhiteSpace(dataRow[j].ToString().Trim()))
                            {
                                isEmpty = false;
                            }

                        }

                        if (!isEmpty)
                        {
                            data.Rows.Add(dataRow);
                        }
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + errorLineNo + ex.Message);
                return null;
            }
        }

    }
}
