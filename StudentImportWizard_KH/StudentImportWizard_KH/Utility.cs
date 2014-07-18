﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using FISCA.Data;
using System.Xml.Linq;
using FISCA.DSAClient;
using JHSchool.StudentExtendControls.Ribbon.StudentImportWizardControls;

namespace StudentImportWizard_KH
{
    public class Utility
    {
        public static string SendDataList(string action, List<logStud> logStudList, ImportMode importMode)
        {
            string DSNS = FISCA.Authentication.DSAServices.AccessPoint;

            string AccessPoint = @"j.kh.edu.tw";

            if (FISCA.RTContext.IsDiagMode)
            {
                string accPoint = FISCA.RTContext.GetConstant("KH_AccessPoint");
                AccessPoint = accPoint;
            }

            string Contract = "log";
            string ServiceName = "_.InsertLog";

            string errMsg = "";
            try
            {

                XElement xmlRoot = new XElement("Request");
                XElement s1 = new XElement("SchoolLog");
                XElement s2 = new XElement("Field");

                s2.SetElementValue("DSNS", DSNS);
                s2.SetElementValue("Action", action);
                XElement Content = new XElement("Content");
                string summaryTxt = action + " 共 " + logStudList.Count + " 筆";
                Content.SetElementValue("Summary", summaryTxt);
                XElement Detail = new XElement("Detail");

                if (importMode == ImportMode.Insert)
                {
                    foreach (logStud ls in logStudList)
                    {
                        XElement StudentXML = new XElement("Student");
                        StudentXML.SetElementValue("IDNumber", ls.IDNumber);
                        StudentXML.SetElementValue("StudentNumber", ls.StudentNumber);
                        StudentXML.SetElementValue("StudentName", ls.StudentName);
                        StudentXML.SetElementValue("ClassName", ls.ClassName);                     
                        StudentXML.SetElementValue("SeatNo", ls.SeatNo);
                        StudentXML.SetElementValue("GradeYear", ls.GradeYear);
                        Detail.Add(StudentXML);
                    }
                }
                else
                {
                    //  更新
                    foreach (logStud ls in logStudList)
                    {
                        XElement StudentXML = new XElement("Student");
                        StudentXML.SetElementValue("IDNumber", ls.IDNumber);
                        StudentXML.SetElementValue("StudentNumber", ls.StudentNumber);
                        StudentXML.SetElementValue("StudentName", ls.StudentName);
                        StudentXML.SetElementValue("ClassName", ls.oClassName);
                        StudentXML.SetElementValue("NewClassName", ls.ClassName);
                        StudentXML.SetElementValue("SeatNo", ls.SeatNo);
                        StudentXML.SetElementValue("GradeYear", ls.GradeYear);
                        Detail.Add(StudentXML);
                    }                
                }               

                s2.Add(Content);
                s2.Add(Detail);
                s1.Add(s2);
                xmlRoot.Add(s1);
                XmlHelper reqXML = new XmlHelper(xmlRoot.ToString());
                FISCA.DSAClient.Connection cn = new FISCA.DSAClient.Connection();
                cn.Connect(AccessPoint, Contract, DSNS, DSNS);
                Envelope rsp = cn.SendRequest(ServiceName, new Envelope(reqXML));
                XElement rspXML = XElement.Parse(rsp.XmlString);
            }
            catch (Exception ex) { errMsg = ex.Message; }

            return errMsg;
        }

        public static List<logStud> ConveroClassName(List<logStud> logStudList)
        {
            List<string> sidList = new List<string>();
            List<string> snoList = new List<string>();
            foreach (logStud ls in logStudList)
            {
                if (!string.IsNullOrEmpty(ls.StudentID))
                {
                    sidList.Add(ls.StudentID);
                }
                else {
                    snoList.Add(ls.StudentNumber);
                }
            }

            Dictionary<string, string> ddDict = new Dictionary<string, string>();
            if (sidList.Count > 0)
            {
                ddDict.Clear();
                string querStr = "select student.id,student_number,class.class_name from student inner join class on student.ref_class_id=class.id where student.status=1 and student.id in("+string.Join(",",sidList.ToArray())+")";
                QueryHelper qh1 = new QueryHelper();
                DataTable dt1 = qh1.Select(querStr);

                foreach (DataRow dr in dt1.Rows)
                    ddDict.Add(dr["id"].ToString(), dr["class_name"].ToString());

                foreach (logStud ls in logStudList)
                {
                    if (ddDict.ContainsKey(ls.StudentID))
                        ls.oClassName = ddDict[ls.StudentID];
                }
            }

            if (snoList.Count > 0)
            {
                ddDict.Clear();
                string querStr = "select student.id,student_number,class.class_name from student inner join class on student.ref_class_id=class.id where student.status=1 and student.student_number in('" + string.Join("','", snoList.ToArray()) + "')";
                QueryHelper qh1 = new QueryHelper();
                DataTable dt1 = qh1.Select(querStr);

                foreach (DataRow dr in dt1.Rows)
                    ddDict.Add(dr["student_number"].ToString(), dr["class_name"].ToString());

                foreach (logStud ls in logStudList)
                {
                    if (ddDict.ContainsKey(ls.StudentNumber))
                        ls.oClassName = ddDict[ls.StudentNumber];
                }
            }
            return logStudList;
        }
    }
  

}