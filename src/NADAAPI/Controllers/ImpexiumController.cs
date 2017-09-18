using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NADAAPI.Repository;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Xml;
using Newtonsoft.Json;
using System.IO;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace NADAAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    public class ImpexiumController : Controller
    {
        private IImpexiumRepository _impexiumRepository;
        private ILogger<ImpexiumController> _logger;
        private ImpexiumProperties _prop;

        public ImpexiumController(IImpexiumRepository impexiumRepository, ILogger<ImpexiumController> logger, IOptions<ImpexiumProperties> prop)
        {
            _impexiumRepository = impexiumRepository;
            _logger = logger;
            _prop = prop.Value;
        }

        [HttpGet]
        [Authorize(Policy = "VendorCheck")]
        public async Task<string> GetConsultantContact(string groupCode)
        {
            string consultId = "";
            string name = "";
            string title = "";
            string cphone = "";
            string wphone = "";
            string fax = "";
            string email = "";

            var json = await _impexiumRepository.GetImpexiumCommitteemembers(groupCode, false);
            if (json.ToString() != "ProtocolError")
            {
                var memResult = _impexiumRepository.ParseJsonToDictionary(json);
                var dataList = _impexiumRepository.GetList(memResult["dataList"]);

                if (dataList != null)
                {
                    foreach (var x in dataList)
                    {
                        var items = _impexiumRepository.GetDictionary(x);
                        if (items["positionCode"].ToString() == "CONSULT")
                        {
                            consultId = items["id"].ToString();
                        }
                    }

                }
            }

            if (!string.IsNullOrEmpty(consultId) && (consultId != "0"))
            {
                json = await _impexiumRepository.GetImpexiumUser(consultId);
                if (json.ToString() != "ProtocolError")
                {
                    var memResult = _impexiumRepository.ParseJsonToDictionary(json);
                    var dataList = _impexiumRepository.GetList(memResult["dataList"]);
                    var individual = _impexiumRepository.GetDictionary(dataList[0]);

                    name = individual["preferredFirstName"] + " " + individual["lastName"];
                    title = individual["title"].ToString();
                    var indphones = _impexiumRepository.GetList(individual["phones"]);
                    if (indphones != null)
                    {
                        foreach (var phone in indphones)
                        {
                            var phonedet = _impexiumRepository.GetDictionary(phone);
                            string typeOfPhone = phonedet["typeName"].ToString();
                            switch (typeOfPhone.ToString())
                            {
                                case "Work":
                                    wphone = phonedet["number"].ToString();
                                    wphone = string.Format(
                                         "{0}.{1}.{2}", wphone.Substring(1, 3), wphone.Substring(5, 4), wphone.Substring(10)).Replace(" ", "");
                                    break;
                                case "Mobile":
                                    cphone = phonedet["number"].ToString();
                                    cphone = string.Format("{0}.{1}.{2}", cphone.Substring(1, 3), cphone.Substring(5, 4), cphone.Substring(10)).Replace(" ", "");
                                    break;
                                case "Fax":
                                    fax = phonedet["number"].ToString();
                                    fax = string.Format(
                                        "{0}.{1}.{2}", fax.Substring(1, 3), fax.Substring(5, 4), fax.Substring(10)).Replace(" ", "");
                                    break;
                            }
                        }
                    }
                    email = individual["email"].ToString();
                }
            }
            else
            {
                return "[{result: 0}]";
            }
            string s = string.Format(
                    "\"result\": \"0\", \"name\": \"{0}\", \"title\": \"{1}\", \"cphone\": \"{2}\", \"wphone\": \"{3}\", \"fax\": \"{4}\", \"email\": \"{5}\"",
                    name,
                    title,
                    cphone,
                    wphone,
                    fax,
                    email);
            return "[{" + s + "}]";

        }

        [HttpGet]
        [Authorize(Policy = "VendorCheck")]
        public IActionResult DealershipByCompanyIdDelayed(string companyId)
        {

            var details = new Dictionary<string, object>();

            SqlConnection conn = new SqlConnection(_prop.NADACopyDB);
            SqlTransaction transaction = null;
            try
            {
                
                    string queryString = "DECLARE @RecordNumber nvarchar(50) " +
                                    "     SET @RecordNumber = '" + companyId + "'  " +
                                    "     SELECT " +
                                     "      FirstSet.*, " +
                                     "      Secondset.Recipient, " +
                                      "     Secondset.Recipient_Email " +
                                     "    FROM (SELECT DISTINCT " +
                                     "      f.code AS MEMBER_TYPE, " +
                                     "      a.recordnumber AS CO_ID, " +
                                     "      a.name AS COMPANY, " +
                                      "     a.recordnumber AS Grp_ID, " +
                                      "     g.line1 AS ADDRESS_1, " +
                                      "     g.line2 AS ADDRESS_2, " +
                                      "     g.City AS City, " +
                                      "     g.[State] AS ST, " +
                                     "      g.Zip AS ZIP, " +
                                    "    c.Prefix AS Prefix, " +
                                     "      c.FirstName + ' ' + c.LastName   AS FULL_NAME, " +
                                     "      b.RelatedFirstName AS FIRST_NAME, " +
                                     "      b.RelatedLastName AS LAST_NAME, " +
                                    "       c.SUFFIX AS SUFFIX, " +
                                    "       (select title from  [Crm].[Individual] where   ContactId= b.RelatedIndividualId )as Title,  " +
                                     "      d.[Address] AS EMAIL " +

                                     "    FROM [Crm].[Organization] a " +
                                     "    INNER JOIN [Crm].[RelatedContact] b " +
                                      "     ON a.contactId = b.contactId " +
                                      "   INNER JOIN [Crm].[Individual] c " +
                                      "     ON b.relatedindividualid = c.contactid " +
                                     "    LEFT JOIN [Crm].[CustomerEmail] d " +
                                     "      ON c.contactid = d.contactid " +
                                     "    INNER JOIN [Crm].[MembershipBenefit] e " +
                                     "      ON a.contactid = e.ReceivesBenefitsfromcustomerid " +
                                     "    INNER JOIN [Shopping].[Membership] f " +
                                      "     ON f.id = e.MembershipProductId " +
                                    "     INNER JOIN [Crm].[CustomerAddress] g " +
                                    "       ON g.Contactid = a.ContactId " +


                                      "   WHERE a.recordnumber = @RecordNumber " +
                                     "    AND b.relatedrelationshipname IN ('Authorized Representative') " +
                                     "    AND g.IsPrimary = 1) AS FirstSet " +
                                     "    INNER JOIN (SELECT DISTINCT " +
                                     "      f.code AS MEMBER_TYPE, " +
                                     "      b.RelationshipName AS TITLE, " +
                                     "      d.[Address] AS EMAIL, " +
                                     "       b.RelatedFirstName + ' ' + b.RelatedLastName   AS Recipient, " +
                                     "      d.[Address] AS Recipient_Email " +

                                    "     FROM [Crm].[Organization] a " +
                                     "    INNER JOIN [Crm].[RelatedContact] b " +
                                     "      ON a.contactId = b.contactId " +
                                    "     INNER JOIN [Crm].[Individual] c " +
                                     "      ON b.relatedindividualid = c.contactid " +
                                     "    LEFT JOIN [Crm].[CustomerEmail] d " +
                                     "      ON c.contactid = d.contactid " +
                                     "    INNER JOIN [Crm].[MembershipBenefit] e " +
                                     "      ON a.contactid = e.ReceivesBenefitsfromcustomerid " +
                                     "    INNER JOIN [Shopping].[Membership] f " +
                                     "      ON f.id = e.MembershipProductId " +
                                     "    INNER JOIN [Crm].[CustomerAddress] g " +
                                      "     ON g.Contactid = a.ContactId " +

                                      "   WHERE a.recordnumber = @RecordNumber " +
                                      "   AND b.relatedrelationshipname IN ('Dealer Principal') " +
                                      "   AND g.IsPrimary = 1) AS SecondSet " +

                                        "   ON FirstSet.MEMBER_TYPE = SecondSet.MEMBER_TYPE " +
                                        " ORDER BY FirstSet.MEMBER_TYPE";
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    SqlCommand cmd = new SqlCommand(queryString, conn);
                    cmd.Transaction = transaction;

                    cmd.CommandType = CommandType.Text;


                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows && rdr.Read())
                        {
                            for (int i = 0; i < rdr.FieldCount; i++)
                            {
                                details.Add(rdr.GetName(i), rdr.IsDBNull(i) ? null : rdr.GetValue(i));
                            }
                        }
                    }
              //  }
            }
            catch (SqlException ex)
            {
                _logger.LogError(" Method : DealershipByCompanyIdDelayed , Company :{0} , Exception : {1}", ex.ToString(), companyId);
                transaction.Rollback();
            }
            finally
            {
                //conn.Close();
            }

            //return ds;
            return Json(details);


        }

        [HttpGet]
        [Authorize(Policy = "VendorCheck")]
        public async Task<IActionResult> DealershipByCompanyId(string companyId)
        {
            try
            {
               
                    var s = await _impexiumRepository.ValidateToken();
                    dynamic accessData = _impexiumRepository.ReadfileAndReturnString();
                    StoresByGroupId obj = new StoresByGroupId();
                    obj = await GetOrgDetailsAndInfo(companyId, accessData, companyId);

                    return Json(obj);
               
            }
            catch (Exception ex)
            {
                _impexiumRepository.WriteError(ex.ToString(), "DealershipByCompanyId", "Web Service Error", "DealershipByCompanyIdLive : " + companyId);

            }
            return BadRequest();

        }

        [HttpGet]
        [Authorize(Policy = "VendorCheck")]
        public async Task<ActionResult> StoresByGroupId(string companyId)
        {
            try
            {
                   List<StoresByGroupId> stores = new List<StoresByGroupId>();  // Main List 
                    string Grp_Id = "";
                    var s = await _impexiumRepository.ValidateToken();
                    dynamic accessData = _impexiumRepository.ReadfileAndReturnString();
        
                    List<object> dataListRelation = new List<object>();
                    Dictionary<string, object> OrgRelDetail = new Dictionary<string, object>();
                List<Dictionary<string, object>> lstParentOrgs = new List<Dictionary<string, object>>();
                List<Dictionary<string, object>> lstBranchOrgs = new List<Dictionary<string, object>>();
                    var OrgJson = await _impexiumRepository.CallOrgProfile(accessData, companyId);
                    if (OrgJson != null)
                    {
                        /// 
                        var orgResult = _impexiumRepository.ParseJsonToDictionary(OrgJson.ToString());
                        var dataList = _impexiumRepository.GetList(orgResult["dataList"]);
                        var OrgDetail = _impexiumRepository.GetDictionary(dataList[0]);

                        var storeImpexiumID = OrgDetail["id"];
                        /// Get store Relationships
                        var OrgJsonRelation = await _impexiumRepository.CallOrgRelation(accessData, storeImpexiumID, "",true);
                        if (OrgJsonRelation != null)
                        {
                            /// 
                            var orgResultRelation = _impexiumRepository.ParseJsonToDictionary(OrgJsonRelation.ToString());
                            dataListRelation = _impexiumRepository.GetList(orgResultRelation["dataList"]);
                            OrgRelDetail = _impexiumRepository.GetDictionary(dataListRelation[0]);
                            var reciprocalRelationshipName = "";
                           
                            /// Loop through to get the functional title 
                            for (var p = 0; p <= dataListRelation.Count - 1; p++)
                            {
                                var Rel = _impexiumRepository.GetDictionary(dataListRelation[p]);
                                if (Rel != null)
                                {

                                    if (Rel["reciprocalRelationshipName"].ToString() == "Parent Organization")
                                    {
                                    var relOrg = _impexiumRepository.GetDictionary(Rel["relatedToCustomer"]);
                                    lstParentOrgs.Add(relOrg);
                                    Grp_Id = relOrg["recordNumber"].ToString();
                                    }

                                }

                            }
                            
                            ////end 
                            if (lstParentOrgs.Count > 0)
                            {
                               
                                ///// Get all the parent  companies Relationship 
                                foreach (var parent in lstParentOrgs)
                                {
                                    var obj =await GetOrgDetailsAndInfo(Grp_Id, accessData, parent["id"].ToString());
                                stores.Add(obj);
                                    /// Get the parent Company Relationships of Child 
                                    OrgJsonRelation = null;
                                    OrgJsonRelation =await _impexiumRepository.CallOrgRelation(accessData, parent["id"].ToString(), "",true);
                                if (OrgJsonRelation != null)
                                    {
                                        orgResult = _impexiumRepository.ParseJsonToDictionary(OrgJsonRelation.ToString());
                                        dataList = _impexiumRepository.GetList(orgResult["dataList"]);
                                        OrgRelDetail = _impexiumRepository.GetDictionary(dataList[0]);
                                        reciprocalRelationshipName = "";

                                        /// Loop through to get the functional title 
                                        for (var p = 0; p <= dataList.Count - 1; p++)
                                        {
                                            var Rel = _impexiumRepository.GetDictionary(dataList[p]);
                                            if (Rel != null)
                                            {

                                                if (Rel["reciprocalRelationshipName"].ToString() == "Dealer Organization")
                                                {
                                                    lstBranchOrgs.Add(Rel);
                                                }

                                            }

                                        }

                                    }
                                }
                            }

                        }

                    }



                    if (lstBranchOrgs.Count > 0)
                    {
                        foreach (var Rel in lstBranchOrgs)
                        {
                            var relCustomer = _impexiumRepository.GetDictionary(Rel["relatedToCustomer"]);
                            var orgID = relCustomer["id"].ToString();
                            if (orgID != null)
                            {
                                var obj = await GetOrgDetailsAndInfo(Grp_Id, accessData, orgID, Rel);
                                stores.Add(obj);
                            }

                        }
                    }
                    
                    return Json(stores);
                
            }
            catch (Exception ex)
            {
                _impexiumRepository.WriteError(ex.ToString(), "StoresByGroupId", "Web Service Error", "CompanyId : " + companyId);
                return null;
            }

            return BadRequest();
        }


        [HttpGet]
        public IActionResult StoresByGroupIdDelayed(string companyId)
        {
            SqlConnection conn = new SqlConnection(_prop.NADACopyDB);
            SqlTransaction transaction = null;
            var details = new Dictionary<string, object>();
           
          
                try
                {
                    string queryString = "DECLARE @RecordNumber NVARCHAR(50)  " +
    " DECLARE @RelatedorgName NVARCHAR(50)   " +
    " DECLARE @parentorgId NVARCHAR(max)   " +
    " DECLARE @RelatedorgId2 NVARCHAR(max)   " +

    " SET @RecordNumber = '" + companyId + "'" +
    " SET @RelatedorgName = (SELECT id   FROM   [Crm].[organization] WHERE  recordnumber = @RecordNumber)   " +

    "  SET @parentorgId = (select  contactid  FROM [Crm].[relatedcontact] t WHERE t.relationshipname LIKE '%Parent Organization%' AND t.relatedOrgId = @RelatedorgName)   " +

    "  (SELECT  x.* " +
         "    INTO #tempcontacts  FROM " +
       "   ( SELECT contactid   " +
      "    FROM [Crm].[relatedcontact] t WHERE t.relationshipname LIKE " +
      "    '%Dealer Organization%' and t.relatedcontactid = @parentorgId     " +
     "     union all" +
     "      select  contactid  FROM [Crm].[relatedcontact] t WHERE t.relationshipname LIKE '%Parent Organization%' AND t.relatedOrgId = @RelatedorgName " +
      "    )  x )" +

    " (SELECT FirstSet.*,   " +
       "     Secondset.recipient,   " +
        "    Secondset.recipient_email   " +
    " FROM   (SELECT DISTINCT f.code     AS MEMBER_TYPE   ,   " +
                    "         a.recordnumber   " +
                    "         AS CO_ID,   " +
                    "         a.NAME                                    AS COMPANY,   " +
                     "        a.recordnumber                            AS Grp_ID,   " +
                     "        g.line1                                   AS ADDRESS_1,   " +
                     "        g.line2                                   AS ADDRESS_2,   " +
                     "        g.city                                    AS City,   " +
                      "       g.[state]                                 AS ST,   " +
                     "        g.zip                                     AS ZIP,   " +
                     "        c.prefix                                  AS Prefix,   " +
                     "        c.firstname + ' ' + c.lastname            AS FULL_NAME,   " +
                    "         b.relatedfirstname                        AS FIRST_NAME,   " +
                   "          b.relatedlastname                         AS LAST_NAME,   " +
                   "          c.suffix                                  AS SUFFIX,   " +
                   "          (SELECT title   " +
                   "           FROM   [Crm].[individual]   " +
                   "           WHERE  contactid = b.relatedindividualid)AS Title,   " +
                  "           d.[address]                               AS EMAIL   " +
         "    FROM   [Crm].[organization] a   " +
                "    INNER JOIN [Crm].[relatedcontact] b   " +
              "              ON a.contactid = b.contactid   " +
              "      INNER JOIN [Crm].[individual] c   " +
             "               ON b.relatedindividualid = c.contactid   " +
            "        LEFT JOIN [Crm].[customeremail] d   " +
            "               ON c.contactid = d.contactid   " +
            "        INNER JOIN [Crm].[membershipbenefit] e   " +
             "               ON a.contactid = e.receivesbenefitsfromcustomerid   " +
             "       INNER JOIN [Shopping].[membership] f   " +
    "   ON f.id = e.membershipproductid   " +
             "       INNER JOIN [Crm].[customeraddress] g   " +
           "                 ON g.contactid = a.contactid   " +
          "   WHERE  a.recordnumber = @RecordNumber   " +
          "          AND b.relatedrelationshipname IN ( 'Authorized Representative' )   " +
          "          AND g.isprimary = 1) AS FirstSet   " +
          "  INNER JOIN (SELECT DISTINCT f.code   " +
                                "        AS   " +
                                "        MEMBER_TYPE,   " +
                               "         b.relationshipname  " +
                             "           AS   " +
                             "                                   TITLE,   " +
                             "           d.[address]   " +
                             "           AS   " +
                             "                                   EMAIL,   " +
                             "           b.relatedfirstname + ' ' + b.relatedlastname   " +
                             "           AS   " +
                             "                                   Recipient,   " +
                             "           d.[address]   " +
                            "            AS   " +
                            "                                    Recipient_Email   " +
                    "    FROM   [Crm].[organization] a   " +
                         "      INNER JOIN [Crm].[relatedcontact] b   " +
                          "             ON a.contactid = b.contactid   " +
                          "     INNER JOIN [Crm].[individual] c   " +
                          "             ON b.relatedindividualid = c.contactid   " +
                           "    LEFT JOIN [Crm].[customeremail] d   " +
                           "           ON c.contactid = d.contactid   " +
                           "    INNER JOIN [Crm].[membershipbenefit] e   " +
                           "            ON a.contactid =   " +
                            "              e.receivesbenefitsfromcustomerid   " +
                        "       INNER JOIN [Shopping].[membership] f   " +
                           "            ON f.id = e.membershipproductid   " +
                         "      INNER JOIN [Crm].[customeraddress] g   " +
                         "              ON g.contactid = a.contactid   " +
                      "  WHERE  a.recordnumber = @RecordNumber   " +
                       "        AND b.relatedrelationshipname IN   " +
                        "           ( 'Dealer Principal' )   " +
                         "      AND g.isprimary = 1) AS SecondSet  " +
                  "  ON FirstSet.member_type = SecondSet.member_type   " +
     "  )  " +
    " UNION   " +
    " (  " +
    " SELECT FirstSet.*,   " +
       "     Secondset.recipient,   " +
         "   Secondset.recipient_email   " +
    " FROM   (SELECT DISTINCT f.code                                    AS MEMBER_TYPE   " +
                        " ,   " +
                     "        a.recordnumber   " +
                    "         AS CO_ID,   " +
                    "         a.NAME                                    AS COMPANY,   " +
                     "        a.recordnumber                            AS Grp_ID,   " +
                    "         g.line1                                   AS ADDRESS_1,   " +
                     "        g.line2                                   AS ADDRESS_2,   " +
                     "        g.city                                    AS City,   " +
                    "         g.[state]                                 AS ST,   " +
                     "        g.zip                                     AS ZIP,   " +
                      "       c.prefix                                  AS Prefix,   " +
                      "       c.firstname + ' ' + c.lastname            AS FULL_NAME,   " +
                       "      b.relatedfirstname                        AS FIRST_NAME,   " +
                    "         b.relatedlastname                         AS LAST_NAME,   " +
                     "        c.suffix                                  AS SUFFIX,   " +
                     "        (SELECT title   " +
                      "        FROM   [Crm].[individual]   " +
                        "      WHERE  contactid = b.relatedindividualid)AS Title,   " +
                   "          d.[address]                               AS EMAIL   " +
          "   FROM   [Crm].[organization] a   " +
                "    INNER JOIN [Crm].[relatedcontact] b   " +
                "            ON a.contactid = b.contactid   " +
              "      INNER JOIN [Crm].[individual] c   " +
                 "           ON b.relatedindividualid = c.contactid   " +
                "    LEFT JOIN [Crm].[customeremail] d   " +
                   "        ON c.contactid = d.contactid   " +
               "     INNER JOIN [Crm].[membershipbenefit] e   " +
               "             ON a.contactid = e.receivesbenefitsfromcustomerid   " +
              "      INNER JOIN [Shopping].[membership] f   " +
               "             ON f.id = e.membershipproductid   " +
               "     INNER JOIN [Crm].[customeraddress] g   " +
                "            ON g.contactid = a.contactid   " +
          "   WHERE  a.id IN ( select * from #tempContacts )   " +
                "    AND b.relatedrelationshipname IN ( 'Authorized Representative' )   " +
                 "   AND g.isprimary = 1) AS FirstSet   " +
          "  INNER JOIN (SELECT DISTINCT f.code   " +
                                "        AS   " +
                                 "       MEMBER_TYPE,   " +
                                  "      b.relationshipname   " +
                                  "      AS   " +
                                    "                            TITLE,   " +
                                 "       d.[address]   " +
                                 "       AS   " +
                                 "                               EMAIL,   " +
                                 "       b.relatedfirstname + ' ' + b.relatedlastname   " +
                                "        AS   " +
                                "                                Recipient,   " +
                                "        d.[address]   " +
                                "        AS   " +
                               "                                 Recipient_Email   " +
                     "   FROM   [Crm].[organization] a   " +
                         "      INNER JOIN [Crm].[relatedcontact] b   " +
                           "            ON a.contactid = b.contactid   " +
                            "   INNER JOIN [Crm].[individual] c   " +
                               "        ON b.relatedindividualid = c.contactid   " +
                           "    LEFT JOIN [Crm].[customeremail] d   " +
                           "           ON c.contactid = d.contactid   " +
                           "    INNER JOIN [Crm].[membershipbenefit] e   " +
                          "             ON a.contactid =   " +
                          "                e.receivesbenefitsfromcustomerid   " +
                        "       INNER JOIN [Shopping].[membership] f   " +
                        "               ON f.id = e.membershipproductid   " +
                       "        INNER JOIN [Crm].[customeraddress] g   " +
                      "                 ON g.contactid = a.contactid   " +
                   "     WHERE   a.id IN (select * from #tempContacts )   " +
                  "             AND b.relatedrelationshipname IN   " +
                    "               ( 'Dealer Principal' )   " +
                    "           AND g.isprimary = 1) AS SecondSet   " +
                 "   ON FirstSet.member_type = SecondSet.member_type join   " +
              " )";
                    // Being Transaction
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    SqlCommand cmd = new SqlCommand(queryString, conn);
                    cmd.Transaction = transaction;

                    cmd.CommandType = CommandType.Text;

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows && rdr.Read())
                        {
                            for (int i = 0; i < rdr.FieldCount; i++)
                            {
                                details.Add(rdr.GetName(i), rdr.IsDBNull(i) ? null : rdr.GetValue(i));
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    _impexiumRepository.WriteError(ex.ToString(), "StoresByGroupIdDelayed", "Web Service Error", "CompanyId : " + companyId);
                    transaction.Rollback();
                }
                finally
                {
                    conn.Close();
                }
          
            return Json(details);
        }

        private async Task<object> GetOrgDetailsAndInfo(string groupid, dynamic accessData, string companyId, Dictionary<string, Object> relCustomerOrgList = null)
        {
            try
            {
                var arID = "";
                var dpID = "";
                StoresByGroupId obj = new StoresByGroupId();
                var OrgJson = await _impexiumRepository.CallOrgProfile(accessData, companyId);
                if (OrgJson != null)
                {
                    /// 
                    var orgResult = _impexiumRepository.ParseJsonToDictionary(OrgJson.ToString());
                    var dataList = _impexiumRepository.GetList(orgResult["dataList"]);
                    var OrgDetail = _impexiumRepository.GetDictionary(dataList[0]);
                    var storeImpexiumID = OrgDetail["id"];

                    obj.COMPANY = OrgDetail["name"];
                    obj.CO_ID = OrgDetail["recordNumber"];
                    obj.Grp_ID = groupid;
                    obj.Impexium_CO_ID = OrgDetail["id"];
                    /// Memberships GetDictionary
                    var memberships = _impexiumRepository.GetList(OrgDetail["memberships"]);
                    if (memberships != null)
                    {
                        foreach (var mem in memberships)
                        {
                            var membership = _impexiumRepository.GetDictionary(mem);
                            obj.MEMBER_TYPE = membership["code"];

                        }
                    }

                    var addressesDictionary = _impexiumRepository.GetList(OrgDetail["addresses"]);
                    if (addressesDictionary != null)
                    {
                        foreach (var address in addressesDictionary)
                        {
                            if (address["primary"].ToString().ToLower() == "true")
                            {
                                var add = _impexiumRepository.GetDictionary(address);
                                obj.ADDRESS_1 = add["line1"] + " ";
                                obj.ADDRESS_2 = add["line2"] + " ";
                                obj.CITY = add["city"] + " ";
                                obj.ST = add["state"] + " ";
                                obj.ZIP = add["zipcode"] + " ";
                                obj.COUNTRY = add["country"] + " ";
                            }
                        }
                    }

                }
                /// Loop through to get the functional title 
                if (obj.Impexium_CO_ID != null)
                {

                    var OrgJsonRelation = await _impexiumRepository.CallOrgRelation(accessData, obj.Impexium_CO_ID, "Employer",true);
                    if (OrgJsonRelation != null)
                    {
                        /// 
                        var orgResult = _impexiumRepository.ParseJsonToDictionary(OrgJsonRelation.ToString());
                        var dataList = _impexiumRepository.GetList(orgResult["dataList"]);
                        var OrgRelDetail = _impexiumRepository.GetDictionary(dataList[0]);
                        var reciprocalRelationshipName = "";

                        ///
                        // var relCustomer = GetDictionary(relCustomerOrgList["relatedToCustomer"]);
                        // var orgID = relCustomer["id"].ToString();

                        /// Loop through to get the functional title 
                        for (var p = 0; p <= dataList.Count - 1; p++)
                        {
                            var Rel = _impexiumRepository.GetDictionary(dataList[p]);
                            if (Rel != null)
                            {
                                if (Rel["reciprocalRelationshipName"].ToString() == "Authorized Representative")
                                {
                                    var relCustomer = _impexiumRepository.GetDictionary(Rel["relatedToCustomer"]);
                                    obj.FULL_NAME = relCustomer["name"];
                                    arID = relCustomer["id"];
                                    /// Get Profile of user 
                                    var userJson = await _impexiumRepository.CallFullUserProfile(accessData, arID,false);
                                    bool issueWithEmail = false;
                                    if (userJson != null)
                                    {
                                        var indResult = _impexiumRepository.ParseJsonToDictionary(userJson.ToString());
                                        /// Get User Information 

                                        var userdataList = _impexiumRepository.GetList(indResult["dataList"]);
                                        var individual = _impexiumRepository.GetDictionary(userdataList[0]);

                                        obj.PREFIX = string.IsNullOrEmpty(individual["prefix"])
                                                     ? ""
                                                     : individual["prefix"];

                                        obj.LAST_NAME = string.IsNullOrEmpty(individual["lastName"])
                                                       ? ""
                                                       : individual["lastName"].ToString();
                                        obj.FIRST_NAME = string.IsNullOrEmpty(individual["firstName"])
                                                        ? ""
                                                        : individual["firstName"].ToString();
                                        obj.SUFFIX = string.IsNullOrEmpty(individual["suffix"])
                                                      ? ""
                                                      : individual["suffix"].ToString();

                                        try
                                        {///Some Times the JSON doesnt Return Email field for Individual
                                            obj.EMAIL = string.IsNullOrEmpty(individual["email"])
                                                         ? ""
                                                         : individual["email"].ToString();
                                        }
                                        catch (Exception )
                                        {
                                            issueWithEmail = true;
                                        }


                                    }
                                    obj.TITLE = string.IsNullOrEmpty(relCustomer["title"])
                                                     ? ""
                                                     : relCustomer["title"].ToString();
                                    if (issueWithEmail)
                                    {
                                        obj.EMAIL = string.IsNullOrEmpty(relCustomer["emails"])
                                                         ? ""
                                                         : relCustomer["emails"].ToString();
                                    }


                                }
                                if (Rel["reciprocalRelationshipName"].ToString() == "Dealer Principal")
                                {
                                    var relCustomer = _impexiumRepository.GetDictionary(Rel["relatedToCustomer"]);

                                    obj.Recipient = string.IsNullOrEmpty(relCustomer["name"])
                                                    ? ""
                                                    : relCustomer["name"].ToString();
                                    obj.Recipient_Email = string.IsNullOrEmpty(relCustomer["emails"])
                                                    ? ""
                                                    : relCustomer["emails"].ToString();
                                    dpID = string.IsNullOrEmpty(relCustomer["id"])
                                                  ? ""
                                                  : relCustomer["id"].ToString();

                                }


                            }

                        }

                    }
                }
                if (obj.Recipient_Email == null || obj.Recipient_Email == "")
                {
                    if (arID == dpID)
                    {
                        obj.Recipient_Email = obj.EMAIL;
                    }
                    else
                    {
                        var userJson = await _impexiumRepository.CallFullUserProfile(accessData, dpID,false);

                        if (userJson != null)
                        {
                            var indResult = _impexiumRepository.ParseJsonToDictionary(userJson.ToString());
                            /// Get User Information 

                            var userdataList = _impexiumRepository.GetList(indResult["dataList"]);
                            var individual = _impexiumRepository.GetDictionary(userdataList[0]);



                            obj.Recipient_Email = string.IsNullOrEmpty(individual["email"])
                                          ? ""
                                          : individual["email"].ToString();
                        }
                    }
                }
                return obj;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}