using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Responce
/// </summary>
public class Responce
{
    public int rescode { get; set;}
    public string resmessage { get; set; }
}
public class CustAgentDetails 
{
    public string branchcode { get; set; }
    public string areacode { get; set; }
    public string areaname { get; set; }
    public string regioncode { get; set; }
    public string regionname { get; set; }
    public string zonename { get; set; }
    public string branchName { get; set; }
    public string dateInstalled { get; set; }
    public string lastNotRunningDate { get; set; }
}