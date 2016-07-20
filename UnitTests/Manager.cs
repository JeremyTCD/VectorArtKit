﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jering.ASP.NET.Identity.Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using Dapper;
using System.Data;

namespace Jering.ASP.NET.Identity.Dapper.Tests
{
    [TestClass()]
    public class Manager
    {
        [AssemblyInitialize()]
        public static void Initialize(TestContext testContext)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString))
            {
                sqlConnection.Execute(@"[Website].[InsertMember]",
                  new
                  {
                      PasswordHash = "passwordHash1",
                      SecurityStamp = "securityStamp1",
                      Email = "email1",
                      NormalizedEmail = "email1"
                  },
                commandType: CommandType.StoredProcedure);
            }
        }

        [AssemblyCleanup()]
        public static void Cleanup()
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString))
            {
                sqlConnection.Execute(@"Delete FROM [dbo].[Members]; DBCC CHECKIDENT('[dbo].[Members]', RESEED, 0);");
            }
        }
    }
}