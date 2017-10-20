using System;
using System.Collections.Generic;
using System.Data;

namespace Server.Interfaces
{
    public interface IDataBase
    {
        void bind(string field, string value);
        void bind(string[] fields);
        int executeNonQuery(string query);
        //Below its wrong i think
        string singleSelect(string query);
        string[] tableToRow();
        List<string> tableToColumn();
        DataTable manySelect(string query);
        Int64 getLastInsertedID();
    }
}
