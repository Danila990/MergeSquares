using System;
using System.Collections.Generic;
using GoogleSheetsToUnity;

namespace Core.Repositories
{
    public class TableParseContext<T>
    {
        public Action<TableParseContext<T>> parseRow;
        public Action<TableParseContext<T>> init;
        public string name;
        public T value;
        public GstuSpreadSheet ss;
        public List<GSTU_Cell> row;
        public GSTU_Cell cell;
    }
}