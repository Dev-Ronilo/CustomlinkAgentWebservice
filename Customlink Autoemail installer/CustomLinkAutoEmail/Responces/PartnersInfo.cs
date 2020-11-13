using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEmailPartner.Responces
{
    class PartnersInfo
    {
        
        public int rescode { get; set; }
        
        public String message { get; set; }
        
        public String errorDetail { get; set; }
        
        public List<partnerThreshold> partnersDetails { get; set; }
    }
    public class partnerThreshold
    {
        public String accountID { get; set; }

        public String accountName { get; set; }

        public String prefund { get; set; }

        public String email { get; set; }

        public String emailcontent { get; set; }

        public String threshold { get; set; }
    }
    public class partInfo 
    {
        public String accountID { get; set; }

        public String accountName { get; set; }

        public String prefund { get; set; }

        public String email { get; set; }

        public String threshold { get; set; }
    }
    public class partnerRunningBal
    {
        
        public String accountID { get; set; }
        
        public String accountName { get; set; }
        
        public String runningBalance { get; set; }
       
        public String amountLoaded { get; set; }

    }
    public class respartBal 
    {
        public String rescode { get; set; }
        public String message { get; set; }
        public List<partnerRunningBal> partnerBalance { get; set; }
    }
    public class Emailed 
    {
        public String rescode { get; set; }
        public String message { get; set; }
    }
    public class responce 
    {
        public String rescode { get; set; }
        public String resmessage { get; set; }
    }
}
