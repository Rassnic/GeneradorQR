using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace QRgenerate
{
    public class conexion
    {

        public SqlConnection connection = new SqlConnection();


        private string connectionString = "Data Source=SERVERSAP;Initial Catalog=Sbo_Masdel;Persist Security Info=True;User ID=sa;Password=masdel214*";

        private void connect()
        {
            connection = new SqlConnection(connectionString);

        }

        public conexion()
        {
            connect();
        }


        public SqlConnection getCon()
        {
            return connection;
        }

    }
}