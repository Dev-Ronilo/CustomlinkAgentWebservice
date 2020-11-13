using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Runtime.Serialization;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Web;
using MySql.Data.MySqlClient;
using System.Text;
using System.Net;
using System.Net.Security;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using log4net;
using log4net.Config;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service" in code, svc and config file together.
public class Service : IService
{
    private String CustConnection = ConfigurationManager.ConnectionStrings["CustAgent"].ToString();
    private String DomesticConnection = ConfigurationManager.ConnectionStrings["DomesticDB"].ToString();
    private String Regionname, Zonename, Regioncode, Areacode, Branchname, Areaname, DateInstalled, DateTimes, datesReff;
    private Int32 NOofCounts;
    private DateTime datetime;
    IDictionary config;
    private String EmailAddresses = String.Empty;
    private String smtpServer = String.Empty;
    private String smtpUser = String.Empty;
    private String smtpPass = String.Empty;
    private String smtpSender = String.Empty;
    private Boolean smtpSsl = false;
    private Boolean status = true;

    List<CustAgentDetails> custDetails = new List<CustAgentDetails>();
    private static readonly ILog log = LogManager.GetLogger(typeof(Service));
	public Service()
	{
        log4net.Config.XmlConfigurator.Configure();
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
        config = (IDictionary)(ConfigurationManager.GetSection("CustAgentSettingsSection"));
        smtpServer = config["smtpServer"].ToString();
        smtpUser = config["smtpUser"].ToString();
        smtpPass = config["smtpPass"].ToString();
        smtpSender = config["smtpSender"].ToString();
        smtpSsl = Convert.ToBoolean(config["smtpSsl"]);
        DateTimes = selectDBdatetim().Replace("'","");
        EmailAddresses = ConfigurationManager.AppSettings["EmailAddresses"];
	}
    private string selectDBdatetim() 
    {
        try 
        {
            using (MySqlConnection con = new MySqlConnection(CustConnection))
            {
                con.Open();
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = cmd.CommandText = "SELECT NOW()";
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    rdr.Read();
                    datetime = Convert.ToDateTime(rdr[0]);
                    rdr.Close();
                    return datetime.ToString("yyyy-MM-dd  HH:mm:ss");
                }
                con.Close();
            }
            log.Info("SUCCESS: [Data: " + datetime.ToString("yyyy-MM-dd  HH:mm:ss") + "]");
        }
        catch(Exception ex)
        {
            log.Error("FAILED: [Data: " + ex.ToString() + "]");
            return null;
        }
    }
    public Responce insertCustInfo(string branchcode, string zone, string customizeLinkAppRunning, string customizeLinkAppNotRunning, string fileCreatedDate, string customizeLinkAppVersion, string noofConsecutivedays, string transType) 
    {
        String strquery;
        checkIfExist(branchcode, zone, fileCreatedDate);
        if (customizeLinkAppNotRunning != "") 
        {
            previousDate(customizeLinkAppNotRunning, branchcode, zone, fileCreatedDate);
        }
        if (customizeLinkAppRunning != "" && customizeLinkAppNotRunning == "") 
        {
            strquery = "INSERT INTO `CustDetails`.`CustAgentMonitoring`(ZoneCode,BranchCode,LastRunningDate,NoofCountsNotRunning,CustAppVersion,DTCreated,transType)" +
                         " VALUES(@ZoneCode,@BranchCode,@LastRunningDate,@NumberofCounts,@CustAppVersion,@dtCreated,@transType)";
        }
        else 
        {
            strquery = "INSERT INTO `CustDetails`.`CustAgentMonitoring`(ZoneCode,BranchCode,LastNotRunningDate,NoofCountsNotRunning,CustAppVersion,DTCreated,transType)" +
                         " VALUES(@ZoneCode,@BranchCode,@LastNotRunningDate,@NumberofCounts,@CustAppVersion,@dtCreated,@transType)";
        }
        List<CustAgentDetails> lstStr = selectToDomestic(branchcode, zone);
        foreach(var list in lstStr)
        {
            Regionname = list.regionname;
            Zonename = list.zonename;
            Regioncode = list.regioncode;
            Areacode = list.areacode;
            Areaname = list.areaname;
            Branchname = list.branchName;
        }
        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        try 
        {
            using(MySqlConnection con = new MySqlConnection(CustConnection))
            {
                con.Open();
                using (MySqlCommand cmd = con.CreateCommand()) 
                {
                    if (status == false) 
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "INSERT INTO `CustDetails`.`InstalledBranches`(ZoneCode,ZoneName,RegionCode,RegionName,AreaCode,AreaName,BranchCode,BranchName,DateInstalled,DTCreated,ConsecutiveCountsNotRunning)" +
                        " VALUES(@ZoneCode,@ZoneName,@RegionCode,@RegionName,@AreaCode,@AreaName,@BranchCode,@BranchName,@DateInstalled,@dtCreated,@ConsecutiveCountsNotRunning)";
                        cmd.Parameters.Add("ZoneCode", zone);
                        cmd.Parameters.Add("ZoneName", Zonename);
                        cmd.Parameters.Add("RegionCode", Regioncode);
                        cmd.Parameters.Add("RegionName", Regionname);
                        cmd.Parameters.Add("AreaCode", Areacode);
                        cmd.Parameters.Add("AreaName", Areaname);
                        cmd.Parameters.Add("BranchCode", branchcode);
                        cmd.Parameters.Add("BranchName", Branchname);
                        cmd.Parameters.Add("DateInstalled", fileCreatedDate);
                        cmd.Parameters.Add("dtCreated", DateTimes);
                        cmd.Parameters.Add("ConsecutiveCountsNotRunning", 0);
                        cmd.ExecuteNonQuery();


                        log.Info("Done inserting data to `CustDetails`.`InstalledBranches` [Data: " + Zonename + "-" + branchcode + "-" + DateTimes + "]");

                        cmd.Parameters.Clear();
                        cmd.CommandText = strquery;
                        cmd.Parameters.Add("ZoneCode", zone);
                        cmd.Parameters.Add("BranchCode", branchcode);
                        if (customizeLinkAppRunning != "")
                        {
                            cmd.Parameters.Add("LastRunningDate", customizeLinkAppRunning);
                        }
                        if (customizeLinkAppNotRunning != "")
                        {
                            cmd.Parameters.Add("LastNotRunningDate", customizeLinkAppNotRunning);
                        }
                        cmd.Parameters.Add("NumberofCounts", NOofCounts);
                        cmd.Parameters.Add("CustAppVersion", customizeLinkAppVersion);
                        cmd.Parameters.Add("dtCreated", DateTimes);
                        cmd.Parameters.Add("transType", (NOofCounts != 0)? 0:1);
                        cmd.ExecuteNonQuery();
                        log.Info("Done inserting data to `CustDetails`.`CustAgentMonitoring` [Data: " + Zonename + "-" + branchcode + "-" + DateTimes + "]");
                    }
                    else 
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = strquery;
                        cmd.Parameters.Add("ZoneCode", zone);
                        cmd.Parameters.Add("BranchCode", branchcode);
                        if (customizeLinkAppRunning != "")
                        {
                            cmd.Parameters.Add("LastRunningDate", customizeLinkAppRunning);
                        }
                        if (customizeLinkAppNotRunning != "")
                        {
                            cmd.Parameters.Add("LastNotRunningDate", customizeLinkAppNotRunning);
                        }
                        cmd.Parameters.Add("NumberofCounts", NOofCounts);
                        cmd.Parameters.Add("CustAppVersion", customizeLinkAppVersion);
                        cmd.Parameters.Add("dtCreated", DateTimes);
                        cmd.Parameters.Add("transType", (NOofCounts != 0)? 0:1);
                        cmd.ExecuteNonQuery();
                        log.Info("Done inserting data to `CustDetails`.`CustAgentMonitoring` [Data: " + Zonename + "-" + branchcode + "-" + DateTimes + "]");
                    }
                }

            }
            return new Responce { rescode = 1, resmessage = "SUCCESS" };
            log.Info("SUCCESS: [Data: " + Zonename + "-" + branchcode + "-" + DateTimes + "]");
        }
        catch(Exception ex)
        {
            return new Responce { rescode = 0, resmessage = "ERROR: " + ex.ToString() };
            log.Error("ERROR: " + ex.ToString());
        }
    }
    public Boolean sendEmail(string RegionCode, string RegionName) 
    {
        string emailAddress = EmailAddresses;
        string[] emailArray = emailAddress.Split(',');
        List<CustAgentDetails> listCount = getConsecutive(RegionCode);
        try
        {
            SmtpClient client = new SmtpClient(smtpServer);
            MailMessage msg = new MailMessage();
            msg.To.Add(emailArray[0]);
            int X = emailArray.Count();
            for (int i = 1; i < X ; i++)
            {
                msg.CC.Add(emailArray[i].ToString());
            }

            msg.From = new MailAddress(smtpUser);
            msg.Subject = "[" + RegionCode + " " + RegionName + " Customize Link is not running]";
            msg.Body = "<h2>List of branches per area from " + RegionCode + " " + RegionName + " that has stopped its Customize Link Application: </h2>";
            msg.Body += "<table style=\" font-family: arial, sans-serif; border-collapse: collapse; width: auto\">";
            msg.Body += "<tr style=\"background-color: white\"> <th style=\"border: 1px solid black;text-align: left;padding: 8px;\">Area Name</th> <th style=\"border: 1px solid black;text-align: left;padding: 8px;\">Branch Name</th> <th style=\"border: 1px solid black;text-align: left;padding: 8px;\">Branch Code</th> <th style=\"border: 1px solid black;text-align: left;padding: 8px;\">Last Custom Link Application Not Running</th></tr>";
            foreach (var item in listCount) 
            {
                if (item.lastNotRunningDate != String.Empty) 
                {
                    msg.Body += "<tr style=\"background-color: white\"><td style=\"border: 1px solid black;text-align: left;padding: 8px;\">" + item.areaname + "</td><td style=\"border: 1px solid black;text-align: left;padding: 8px;\">" + item.branchName + "</td><td style=\"border: 1px solid black;text-align: left;padding: 8px;\">" + item.branchcode + "</td><td style=\"border: 1px solid black;text-align: left;padding: 8px;\">" + item.lastNotRunningDate + "</td></tr>"; 
                }
            }
            msg.Body += "</table>";
            msg.IsBodyHtml = true;
            client.Port = 587;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(smtpSender, smtpPass);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = false;
            client.Send(msg);

            return true;
            log.Info("Success sending Email for [" + RegionCode + "-" + RegionName + "-" + DateTimes + "]");
        }
        catch(Exception ex)
        {
            return false;
            log.Error("Error: " + ex.ToString());
        }
    }
    public List<CustAgentDetails> selectToDomestic(string branchCode, string zoneCode)
    {
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        try
        {
            using (MySqlConnection conn = new MySqlConnection(DomesticConnection))
            {
                conn.Open();
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT a.areacode,a.areaname,a.regioncode,a.regionname,b.zonename ,branchname FROM `kpusers`.`branches` a " +
                                                        "INNER JOIN kpusers.zonecodes b ON a.zonecode=b.zonecode  WHERE a.zonecode = "+ zoneCode +" AND a.branchcode = " + branchCode;
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.HasRows) 
                    {
                        while (rdr.Read()) 
                        {
                            custDetails.Add(new CustAgentDetails {

                                areacode = rdr["areacode"].ToString(),
                                areaname = rdr["areaname"].ToString(),
                                regioncode = rdr["regioncode"].ToString(),
                                regionname = rdr["regionname"].ToString(),
                                zonename = rdr["zonename"].ToString(),
                                branchName = rdr["branchname"].ToString()
                            });
                        }
                    }
                    rdr.Close();
                }
                conn.Close();
            }
            return new List<CustAgentDetails>(custDetails);
            log.Info("Succes on getting details for: "+ zoneCode +"-"+ branchCode);
        }
        catch (Exception ex)
        {
            return new List<CustAgentDetails>(null);
            log.Info("Failed on getting details for: " + zoneCode + "-" + branchCode +"Error: " + ex.ToString());
        }
    }
    private bool checkIfExist(string bcode, string zcode, string fileCreatedDate) 
    {
        List<CustAgentDetails> list = new List<CustAgentDetails>();
        using (MySqlConnection con = new MySqlConnection(CustConnection))
        {
            con.Open();
            using (MySqlCommand cmd = con.CreateCommand())
            {
                    cmd.CommandText = cmd.CommandText = "Select ZoneName,BranchName,AreaCode,AreaName,RegionCode,RegionName,DateInstalled FROM `CustDetails`.`InstalledBranches` WHERE ZoneCode = " + zcode + " AND BranchCode = " + bcode;
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        list.Add(new CustAgentDetails(){
                            zonename = rdr["ZoneName"].ToString(),
                            branchName = rdr["BranchName"].ToString(),
                            areacode = rdr["AreaCode"].ToString(),
                            areaname = rdr["AreaName"].ToString(),
                            regioncode = rdr["RegionCode"].ToString(),
                            regionname = rdr["RegionName"].ToString(),
                            dateInstalled = rdr["DateInstalled"].ToString()
                    });
                        foreach(var item in list)
                        {
                            Zonename = item.zonename;
                            Branchname = item.branchName;
                            Areacode = item.areacode;
                            Areaname = item.areaname;
                            Regioncode = item.regioncode;
                            Regionname = item.regionname;
                            DateInstalled = item.dateInstalled;
                        }
                    }
                    if (!rdr.HasRows)
                    {
                      status = false;
                      log.Info("Does not exist in `CustDetails`.`InstalledBranches` for : " + bcode + "-" + zcode);
                    }
                    else 
                    {
                        datetime = Convert.ToDateTime(DateInstalled);
                        String selectDate = datetime.ToString("yyyy-MM-dd HH:mm:ss");
                        rdr.Close();
                        if (fileCreatedDate != selectDate)
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandText = "INSERT INTO `CustDetails`.`InstalledBranches`(ZoneCode,BranchCode,ZoneName,BranchName,AreaName,AreaCode,RegionCode,RegionName,DateInstalled,DTCreated,ConsecutiveCountsNotRunning)" +
                            " VALUES(@ZoneCode,@BranchCode,@ZoneName,@BranchName,@AreaName,@AreaCode,@RegionCode,@RegionName,@DateInstalled,@dtCreated,@consecutiveCounts)";
                            cmd.Parameters.Add("ZoneCode", zcode);
                            cmd.Parameters.Add("BranchCode", bcode);
                            cmd.Parameters.Add("ZoneName",Zonename);
                            cmd.Parameters.Add("BranchName", Branchname);
                            cmd.Parameters.Add("AreaCode", Areacode);
                            cmd.Parameters.Add("AreaName", Areaname);
                            cmd.Parameters.Add("RegionCode", Regioncode);
                            cmd.Parameters.Add("RegionName", Regionname);
                            cmd.Parameters.Add("DateInstalled", fileCreatedDate);
                            cmd.Parameters.Add("dtCreated", DateTimes);
                            cmd.Parameters.Add("consecutiveCounts", 0);
                            cmd.ExecuteNonQuery();
                            log.Info("Done Inserting new data in `CustDetails`.`InstalledBranches` for : " + bcode + "-" + zcode);
                        }
                        log.Info("Existing in `CustDetails`.`InstalledBranches` for : " + bcode + "-" + zcode);
                    }
            }
            con.Close();
        }
        return status;
    }
    public List<CustAgentDetails> getConsecutive(string regionCode) 
    {
         List<CustAgentDetails> list = new List<CustAgentDetails>();
         using (MySqlConnection con = new MySqlConnection(CustConnection))
         {
             con.Open();
             using (MySqlCommand cmd = con.CreateCommand())
             {
                 cmd.CommandText = cmd.CommandText = "SELECT a.areaname,a.branchname,a.BranchCode, b.LastNotRunningDate FROM `CustDetails`.`InstalledBranches` a " +
                                                     "INNER JOIN `CustDetails`.`CustAgentMonitoring` b ON a.zonecode=b.zonecode WHERE a.RegionCode = " + regionCode + " ORDER BY LastNotRunningDate DESC";
                 MySqlDataReader rdr = cmd.ExecuteReader();
                 if (rdr.HasRows) 
                 {
                     while (rdr.Read())
                     {
                         list.Add(new CustAgentDetails {
                             areaname = rdr["areaname"].ToString(),
                             branchName = rdr["branchname"].ToString(),
                             branchcode = rdr["BranchCode"].ToString(),
                             lastNotRunningDate = rdr["LastNotRunningDate"].ToString()
                         });
                     }
                 }
             }
         }
        return new List<CustAgentDetails>(list);
    }
    private String previousDate(string date, string bcode, string zcode,string fileCreatedDate) 
    {
        DateTime currDate = Convert.ToDateTime(date);
        DateTime previousDate = currDate.AddDays(-1);

        using (MySqlConnection con = new MySqlConnection(CustConnection))
        {
            con.Open();
            using (MySqlCommand cmd = con.CreateCommand())
            {
                cmd.CommandText = "SELECT NoofCountsNotRunning FROM `CustDetails`.`CustAgentMonitoring` WHERE ZoneCode = " + zcode + " AND BranchCode = " + bcode + " AND LastNotRunningDate LIKE '%" + previousDate.ToString("yyyy-MM-dd") + "%'";
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read()) 
                    {
                        NOofCounts = Convert.ToInt32(rdr["NoofCountsNotRunning"]);
                        NOofCounts = NOofCounts + 1;
                    }
                    rdr.Close();
                    con.Close();
                    if (NOofCounts >= 3) 
                    {
                        int consecutiveCounts = (NOofCounts / 3);
                        string counted = ((consecutiveCounts > 1)? consecutiveCounts : 1).ToString();
                        updateConsecutive(counted, bcode, zcode, fileCreatedDate);
                    }
                }
                else 
                {
                    NOofCounts = 1;
                }
                
            }
            con.Close();
            return NOofCounts.ToString();
        }
    }
    public void updateConsecutive(string numberofCountsNotRunning,string bcode, string zcode,string fileCreatedDate) 
    {
        try 
        {
            using (MySqlConnection con = new MySqlConnection(CustConnection))
            {
                con.Open();
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT BranchCode,ZoneCode FROM `CustDetails`.`InstalledBranches` WHERE ZoneCode = " + zcode + " AND BranchCode = " + bcode + " AND DateInstalled = '" + fileCreatedDate + "'";
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        rdr.Close();
                        cmd.CommandText = "UPDATE `CustDetails`.`InstalledBranches` SET ConsecutiveCountsNotRunning = " + numberofCountsNotRunning + ", dtCreated = '" + DateTimes + "' WHERE ZoneCode = " + zcode + " AND BranchCode = " + bcode + " AND DateInstalled = '" + fileCreatedDate + "'";
                        cmd.ExecuteNonQuery();
                        
                    }
                    rdr.Close();
                }
                con.Close();
            }
            resetMonth(zcode, bcode, fileCreatedDate);
            log.Info("Success Updating in `CustDetails`.`InstalledBranches` for : " + bcode + "-" + zcode);
        }
        catch (Exception ex) 
        {
            log.Error("Failed to Update in `CustDetails`.`InstalledBranches` for : " + bcode + "-" + zcode + "Error: "+ ex.ToString());
        }
    }
    public void createLogging(string zonecode, string branchcode) 
    {
        try 
        {
            using (MySqlConnection con = new MySqlConnection(CustConnection))
            {
                con.Open();
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "INSERT INTO `CustDetails`.`CustLinkHistory`(ZoneCode,BranchCode,dateCreated) VALUES(@ZoneCode,@BranchCode,@dateCreated)";
                    cmd.Parameters.Add("ZoneCode", zonecode);
                    cmd.Parameters.Add("BranchCode", branchcode);
                    cmd.Parameters.Add("dateCreated", DateTimes);
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }
            log.Info("Success Updating in `CustDetails`.`CustLinkHistory` for : " + zonecode + "-" + branchcode);
        }
        catch(Exception ex)
        {
            log.Error("Failed to Update in `CustDetails`.`CustLinkHistory` for : " + zonecode + "-" + branchcode+ " Error: "+ex.ToString());
        }
    }
    public Responce sendingCustDetails() 
    {
        List<CustAgentinfo> list = new List<CustAgentinfo>();
        try 
        {
            using (MySqlConnection con = new MySqlConnection(CustConnection))
            {
                con.Open();
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT ZoneCode,BranchCode,areaname,branchname,RegionCode,RegionName,dtCreated FROM `CustDetails`.`InstalledBranches` WHERE IF(ConsecutiveCountsNotRunning >= 1,ConsecutiveCountsNotRunning,0)";
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        while (rdr.Read())
                        {
                            list.Add(new CustAgentinfo
                            {
                                zoneCode = rdr["ZoneCode"].ToString(),
                                branchCode = rdr["BranchCode"].ToString(),
                                areaName = rdr["areaname"].ToString(),
                                branchName = rdr["branchname"].ToString(),
                                regionCode = rdr["RegionCode"].ToString(),
                                regionName = rdr["RegionName"].ToString(),
                                dtCreated = rdr["dtCreated"].ToString()
                            });
                        }
                        rdr.Close();
                        con.Close();
                    }
                    if (list.Count != 0)
                    {
                        foreach (var item in list)
                        {
                           if (checkIfEmailed(item.dtCreated,item.zoneCode,item.branchCode) == false) 
                           {
                               bool state = sendEmail(item.regionCode, item.regionName);
                               if (state == true)
                               {
                                   createLogging(item.zoneCode, item.branchCode);
                               }
                           }
                        }
                    }
                }
            }
            return new Responce { rescode = 1, resmessage = "Done Sending Email" };
        }
        catch(Exception ex) 
        {
            return new Responce { rescode = 0, resmessage = "Failed Sending Email" +ex.ToString() };
        }
    }
    public void resetMonth(string zoneCode,string branchCode,string fileCreated) 
    {

        DateTime currentDate = Convert.ToDateTime(DateTimes);
        DateTime DTCREATED;
        using (MySqlConnection con = new MySqlConnection(CustConnection))
        {
            con.Open();
            using (MySqlCommand cmd = con.CreateCommand())
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "SELECT dtCreated FROM `CustDetails`.`InstalledBranches` WHERE  ZoneCode = " + zoneCode + " AND BranchCode = " + branchCode + " AND DateInstalled = '" + fileCreated + "'";
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        datesReff = rdr["dtCreated"].ToString();
                    }
                    rdr.Close();
                }
                DTCREATED = Convert.ToDateTime(datesReff);
                if (currentDate.Month != DTCREATED.Month) 
                {
                    cmd.CommandText = "UPDATE `CustDetails`.`InstalledBranches` SET ConsecutiveCountsNotRunning =  0, dtCreated = '" + DateTimes + "' WHERE ZoneCode = " + zoneCode + " AND BranchCode = " + branchCode + " AND DateInstalled = '" + fileCreated + "'";
                    cmd.ExecuteNonQuery();
                    log.Info("Updated consecutive counts for ZoneCode = " + zoneCode + " AND BranchCode = " + branchCode);
                }
            }
        }
    }
    public Boolean checkIfEmailed(string dtcreated, string zcode, string bcode) 
    {
        DateTime checkDate = Convert.ToDateTime(dtcreated);
        try
        {
            using (MySqlConnection con = new MySqlConnection(CustConnection))
            {
                con.Open();
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT BranchCode,ZoneCode FROM `CustDetails`.`CustLinkHistory` WHERE ZoneCode = " + zcode + " AND BranchCode = " + bcode + " AND dateCreated LIKE '%" + checkDate.ToString("yyyy-MM-dd") + "%'";
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.HasRows) 
                    {
                        status = true;
                    }
                    else 
                    {
                        status = false;
                    }
                }
                con.Close();
            }
            return status;
            log.Info("Exist in `CustDetails`.`CustLinkHistory`: " + status);
        }
        catch (Exception ex)
        {
            return status = false;
            log.Error(ex.ToString());
        }
    }
}
