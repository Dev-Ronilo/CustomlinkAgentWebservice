using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService" in both code and config file together.
[ServiceContract]
public interface IService
{

    [OperationContract]
    [WebInvoke(Method = "POST",
         ResponseFormat = WebMessageFormat.Json,
         RequestFormat = WebMessageFormat.Json,
         BodyStyle = WebMessageBodyStyle.WrappedRequest,
         UriTemplate = "/insertCustInfo")]

    Responce insertCustInfo(String branchcode, String zone, String customizeLinkAppRunning, String customizeLinkAppNotRunning, String fileCreatedDate, String customizeLinkAppVersion, String noofConsecutivedays, String transType);

    [OperationContract]
    [WebInvoke(Method = "GET",
        ResponseFormat = WebMessageFormat.Json,
        RequestFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/sendEmail/?regioncode={RegionCode}&regionname={RegionName}")]
    Boolean sendEmail(string RegionCode, string RegionName);

    [OperationContract]
    [WebInvoke(Method = "GET",
        ResponseFormat = WebMessageFormat.Json,
        RequestFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/sendingCustDetails")]
    Responce sendingCustDetails();
}


// Use a data contract as illustrated in the sample below to add composite types to service operations.
[DataContract]
public class CustAgentinfo
{
    [DataMember]
    public string zoneName { get; set; }
    public string zoneCode { get; set; }
    public string branchName { get; set; }
    public string branchCode { get; set; }
    public string areaName { get; set; }
    public string regionName { get; set; }
    public string regionCode { get; set; }
    public string dtCreated { get; set; }
}
